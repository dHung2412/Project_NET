using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public interface IAIService
    {   
        // Gọi AI đoán danh mục
        Task<string> SuggestCategoryAsync(string productName, string description);
        // Nhiều gợi ý danh mục 
        Task<IEnumerable<string>> GetCategorySuggestionsAsync(string productName, string description);
        // Tạo mô tả
        Task<string> GenerateProductDescriptionAsync(string productName, string category);
        // Phân tích xu hướng tồn kho
        Task<StockAnalysis> AnalyzeProductStockAsync(Guid productId);
        // Đưa ra khuyến nghị nhập hàng
        Task<IEnumerable<StockRecommendation>> GetStockRecommendationsAsync();
        // Cải thiện mô tả
        Task<string> ImproveProductDescriptionAsync(string currentDescription, string productName, string category);
    }

    public class AIService : IAIService
    {
        private readonly ICategoryPredictionService _categoryPredictionService;
        private readonly IStockPredictionService _stockPredictionService;
        private readonly IProductDescriptionService _descriptionService;
        private readonly IUnitOfWork _unitOfWork;

        public AIService(
            ICategoryPredictionService categoryPredictionService,
            IStockPredictionService stockPredictionService,
            IProductDescriptionService descriptionService,
            IUnitOfWork unitOfWork)
        {
            _categoryPredictionService = categoryPredictionService;
            _stockPredictionService = stockPredictionService;
            _descriptionService = descriptionService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> SuggestCategoryAsync(string productName, string description)
        {
            return await _categoryPredictionService.PredictCategoryAsync(productName, description);
        }

        public async Task<IEnumerable<string>> GetCategorySuggestionsAsync(string productName, string description)
        {
            return await _categoryPredictionService.GetSuggestedCategoriesAsync(productName, description, 5);
        }

        public async Task<string> GenerateProductDescriptionAsync(string productName, string category)
        {
            return await _descriptionService.GenerateDescriptionAsync(productName, category);
        }

        public async Task<string> ImproveProductDescriptionAsync(string currentDescription, string productName, string category)
        {
            return await _descriptionService.ImproveDescriptionAsync(currentDescription, productName, category);
        }

        public async Task<StockAnalysis> AnalyzeProductStockAsync(Guid productId)
        {
            return await _stockPredictionService.AnalyzeStockTrendsAsync(productId);
        }

        public async Task<IEnumerable<StockRecommendation>> GetStockRecommendationsAsync()
        {
            var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync();
            var recommendations = new List<StockRecommendation>();

            foreach (var product in lowStockProducts)
            {
                try
                {
                    var analysis = await _stockPredictionService.AnalyzeStockTrendsAsync(product.Id);
                    var reorderQuantity = await _stockPredictionService.PredictReorderQuantityAsync(product.Id);
                    var nextRestockDate = await _stockPredictionService.PredictNextRestockDateAsync(product.Id);

                    var recommendation = new StockRecommendation
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Category = product.Category,
                        CurrentStock = product.CurrentStock,
                        MinimumStock = product.MinimumStock,
                        RecommendedOrderQuantity = reorderQuantity,
                        PredictedRestockDate = nextRestockDate,
                        Priority = GetPriority(product.CurrentStock, product.MinimumStock, analysis.DaysUntilStockout),
                        Reason = GenerateRecommendationReason(product, analysis, reorderQuantity),
                        EstimatedCost = product.Price * reorderQuantity * 0.7m, // Assuming wholesale cost is 70% of retail
                        DaysUntilStockout = analysis.DaysUntilStockout
                    };

                    recommendations.Add(recommendation);
                }
                catch (Exception)
                {
                    // Fallback to simple calculation if AI service fails
                    var fallbackRecommendation = new StockRecommendation
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Category = product.Category,
                        CurrentStock = product.CurrentStock,
                        MinimumStock = product.MinimumStock,
                        RecommendedOrderQuantity = product.GetRecommendedOrderQuantity(),
                        PredictedRestockDate = DateTime.UtcNow.AddDays(7),
                        Priority = GetPriority(product.CurrentStock, product.MinimumStock, 0),
                        Reason = "Tồn kho thấp - cần nhập hàng ngay",
                        EstimatedCost = product.Price * product.GetRecommendedOrderQuantity() * 0.7m,
                        DaysUntilStockout = product.CurrentStock <= 0 ? 0 : 30
                    };

                    recommendations.Add(fallbackRecommendation);
                }
            }

            return recommendations.OrderBy(r => r.Priority);
        }

        private static RecommendationPriority GetPriority(int currentStock, int minimumStock, int daysUntilStockout)
        {
            if (currentStock <= 0 || daysUntilStockout <= 1)
                return RecommendationPriority.Critical;

            if (currentStock <= minimumStock * 0.5 || daysUntilStockout <= 7)
                return RecommendationPriority.High;

            if (currentStock <= minimumStock || daysUntilStockout <= 14)
                return RecommendationPriority.Medium;

            return RecommendationPriority.Low;
        }

        private static string GenerateRecommendationReason(Domain.Entities.Product product, StockAnalysis analysis, int reorderQuantity)
        {
            if (product.CurrentStock <= 0)
                return $"Sản phẩm đã hết hàng. Cần nhập {reorderQuantity} sản phẩm ngay lập tức.";

            if (analysis.DaysUntilStockout <= 3)
                return $"Sản phẩm sẽ hết hàng trong {analysis.DaysUntilStockout} ngày. Mức tiêu thụ trung bình: {analysis.AverageDailyUsage:F1} sản phẩm/ngày.";

            if (product.CurrentStock <= product.MinimumStock)
                return $"Tồn kho đã xuống dưới mức tối thiểu ({product.MinimumStock}). Nên nhập {reorderQuantity} sản phẩm.";

            return $"Dự báo cần nhập {reorderQuantity} sản phẩm dựa trên xu hướng tiêu thụ hiện tại.";
        }
    }

    public class StockRecommendation
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public DateTime PredictedRestockDate { get; set; }
        public RecommendationPriority Priority { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int DaysUntilStockout { get; set; }
    }

    public enum RecommendationPriority
    {
        Critical = 1,
        High = 2,
        Medium = 3,
        Low = 4
    }
}