using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.AI
{
    public class StockPredictionService : IStockPredictionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StockPredictionService> _logger;

        public StockPredictionService(IUnitOfWork unitOfWork, ILogger<StockPredictionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> PredictOptimalStockLevelAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null) return 0;

                var analysis = await AnalyzeStockTrendsAsync(productId);
                
                // Calculate optimal stock level based on average daily usage
                // Optimal = (Average daily usage * Lead time) + Safety stock
                var leadTimeDays = 7; // Assume 7 days lead time
                var safetyStockDays = 14; // 2 weeks safety stock
                
                var optimalStock = (int)(analysis.AverageDailyUsage * (leadTimeDays + safetyStockDays));
                
                // Ensure it's within min/max bounds
                optimalStock = Math.Max(optimalStock, product.MinimumStock);
                optimalStock = Math.Min(optimalStock, product.MaximumStock);

                _logger.LogInformation($"Predicted optimal stock level {optimalStock} for product {productId}");
                return optimalStock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error predicting optimal stock level for product {productId}");
                return 0;
            }
        }

        public async Task<int> PredictReorderQuantityAsync(Guid productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null) return 0;

                var optimalStock = await PredictOptimalStockLevelAsync(productId);
                var reorderQuantity = Math.Max(0, optimalStock - product.CurrentStock);

                _logger.LogInformation($"Predicted reorder quantity {reorderQuantity} for product {productId}");
                return reorderQuantity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error predicting reorder quantity for product {productId}");
                return 0;
            }
        }

        public async Task<DateTime> PredictNextRestockDateAsync(Guid productId)
        {
            try
            {
                var analysis = await AnalyzeStockTrendsAsync(productId);
                
                // Based on current usage rate, predict when to restock
                var restockBuffer = 7; // Restock 7 days before stockout
                var daysUntilRestock = Math.Max(1, analysis.DaysUntilStockout - restockBuffer);
                
                var nextRestockDate = DateTime.UtcNow.AddDays(daysUntilRestock);
                
                _logger.LogInformation($"Predicted next restock date {nextRestockDate:yyyy-MM-dd} for product {productId}");
                return nextRestockDate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error predicting next restock date for product {productId}");
                return DateTime.UtcNow.AddDays(7); // Default to 1 week
            }
        }

        public async Task<StockAnalysis> AnalyzeStockTrendsAsync(Guid productId, int daysBack = 30)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {productId} not found");
                }

                var startDate = DateTime.UtcNow.AddDays(-daysBack);
                var transactions = await _unitOfWork.StockTransactions.GetByDateRangeAsync(startDate, DateTime.UtcNow);
                var productTransactions = transactions.Where(t => t.ProductId == productId).ToList();

                var analysis = new StockAnalysis
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    AnalysisDate = DateTime.UtcNow
                };

                if (productTransactions.Count == 0)
                {
                    // No transaction data available, use simple estimates
                    analysis.AverageDailyUsage = 1.0; // Default assumption
                    analysis.StockTurnoverRate = 0.1; // 10% per month
                    analysis.DaysUntilStockout = product.CurrentStock > 0 ? product.CurrentStock : 0;
                    analysis.SuggestedOrderQuantity = product.GetRecommendedOrderQuantity();
                    analysis.Recommendation = "Không có đủ dữ liệu lịch sử. Sử dụng ước tính mặc định.";
                    return analysis;
                }

                // Calculate average daily usage from stock-out transactions
                var stockOutTransactions = productTransactions.Where(t => t.Type == TransactionType.StockOut).ToList();
                var totalStockOut = stockOutTransactions.Sum(t => t.Quantity);
                analysis.AverageDailyUsage = daysBack > 0 ? (double)totalStockOut / daysBack : 0;

                // Calculate stock turnover rate
                var totalStockMovement = productTransactions.Sum(t => t.Quantity);
                var averageStock = (double)(product.MinimumStock + product.MaximumStock) / 2;
                analysis.StockTurnoverRate = averageStock > 0 ? totalStockMovement / averageStock / daysBack : 0;

                // Predict days until stockout
                if (analysis.AverageDailyUsage > 0)
                {
                    analysis.DaysUntilStockout = (int)(product.CurrentStock / analysis.AverageDailyUsage);
                }
                else
                {
                    analysis.DaysUntilStockout = product.CurrentStock > 0 ? 365 : 0; // No usage, assume very long time
                }

                // Generate trends
                analysis.Trends = GenerateStockTrends(productTransactions, daysBack);

                // Calculate suggested order quantity
                var optimalStock = await PredictOptimalStockLevelAsync(productId);
                analysis.SuggestedOrderQuantity = Math.Max(0, optimalStock - product.CurrentStock);

                // Generate recommendation
                analysis.Recommendation = GenerateRecommendation(analysis, product);

                _logger.LogInformation($"Completed stock analysis for product {productId}: {analysis.Recommendation}");
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error analyzing stock trends for product {productId}");
                
                // Return default analysis on error
                return new StockAnalysis
                {
                    ProductId = productId,
                    ProductName = "Unknown",
                    AverageDailyUsage = 1.0,
                    StockTurnoverRate = 0.1,
                    DaysUntilStockout = 30,
                    SuggestedOrderQuantity = 10,
                    Recommendation = "Lỗi phân tích dữ liệu. Sử dụng giá trị mặc định.",
                    AnalysisDate = DateTime.UtcNow,
                    Trends = new List<StockTrend>()
                };
            }
        }

        private static List<StockTrend> GenerateStockTrends(List<StockTransaction> transactions, int daysBack)
        {
            var trends = new List<StockTrend>();
            var startDate = DateTime.UtcNow.AddDays(-daysBack);
            
            // Group transactions by date
            var dailyTransactions = transactions
                .Where(t => t.TransactionDate >= startDate)
                .GroupBy(t => t.TransactionDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            var currentStock = 0;
            
            foreach (var dayGroup in dailyTransactions)
            {
                var dailyChange = dayGroup.Sum(t => t.Type == TransactionType.StockIn ? t.Quantity : -t.Quantity);
                currentStock += dailyChange;
                
                var reasons = dayGroup.Select(t => t.Reason).Where(r => !string.IsNullOrEmpty(r)).ToList();
                var combinedReason = reasons.Count > 0 ? string.Join(", ", reasons) : "Giao dịch thường";

                trends.Add(new StockTrend
                {
                    Date = dayGroup.Key,
                    StockLevel = Math.Max(0, currentStock),
                    DailyChange = dailyChange,
                    Reason = combinedReason
                });
            }

            return trends;
        }

        private static string GenerateRecommendation(StockAnalysis analysis, Product product)
        {
            if (analysis.DaysUntilStockout <= 0)
                return "⚠️ SẢN PHẨM ĐÃ HẾT HÀNG! Cần nhập hàng ngay lập tức.";

            if (analysis.DaysUntilStockout <= 3)
                return $"🔴 KHẨN CẤP: Sản phẩm sẽ hết hàng trong {analysis.DaysUntilStockout} ngày. Nên nhập {analysis.SuggestedOrderQuantity} sản phẩm ngay.";

            if (analysis.DaysUntilStockout <= 7)
                return $"🟠 CẢNH BÁO: Sản phẩm sẽ hết hàng trong {analysis.DaysUntilStockout} ngày. Cần lên kế hoạch nhập {analysis.SuggestedOrderQuantity} sản phẩm.";

            if (product.IsLowStock())
                return $"🟡 TỒNG KHO THẤP: Hiện tại dưới mức tối thiểu. Nên nhập {analysis.SuggestedOrderQuantity} sản phẩm để đảm bảo không thiếu hàng.";

            if (analysis.AverageDailyUsage > 0)
                return $"✅ BÌNH THƯỜNG: Tồn kho ổn định với mức tiêu thụ trung bình {analysis.AverageDailyUsage:F1} sản phẩm/ngày. Dự kiến hết hàng trong {analysis.DaysUntilStockout} ngày.";

            return "📊 Không có đủ dữ liệu để đưa ra khuyến nghị cụ thể.";
        }
    }
}