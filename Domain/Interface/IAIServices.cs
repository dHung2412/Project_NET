using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces
{
    public interface ICategoryPredictionService
    {   //Đoán danh mục từ tên + sản phẩm
        Task<string> PredictCategoryAsync(string productName, string description);
        //Đưa gợi ý nhiều danh mục khả dĩ
        Task<IEnumerable<string>> GetSuggestedCategoriesAsync(string productName, string description, int maxSuggestions = 3);
    }

    public interface IStockPredictionService
    {   
        // Dự đoán mức tồn kho tối ưu
        Task<int> PredictOptimalStockLevelAsync(Guid productId);
        // Dự đoán số lượng cần nhập lại
        Task<int> PredictReorderQuantityAsync(Guid productId);
        // Dự đoán ngày nhập hàng tiếp theo
        Task<DateTime> PredictNextRestockDateAsync(Guid productId);
        // Phân tích xu hướng tồn kho
        Task<StockAnalysis> AnalyzeStockTrendsAsync(Guid productId, int daysBack = 30);
    }

    public interface IProductDescriptionService
    {   
        // Tạo mô tả sản phẩm
        Task<string> GenerateDescriptionAsync(string productName, string category);
        // Cải thiện mô tả hiện tại
        Task<string> ImproveDescriptionAsync(string currentDescription, string productName, string category);
        // Sinh ra các tags cho sản phẩm
        Task<IEnumerable<string>> GenerateProductTagsAsync(string productName, string description);
    }

    public class StockAnalysis
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double AverageDailyUsage { get; set; }
        public double StockTurnoverRate { get; set; }
        public int DaysUntilStockout { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public int SuggestedOrderQuantity { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<StockTrend> Trends { get; set; } = new();
    }

    public class StockTrend
    {
        public DateTime Date { get; set; }
        public int StockLevel { get; set; }
        public int DailyChange { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}