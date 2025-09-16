using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.AI
{
    public class CategoryPredictionService : ICategoryPredictionService
    {
        private readonly ILogger<CategoryPredictionService> _logger;

        // Predefined categories with keywords for simple classification
        private readonly Dictionary<string, List<string>> _categoryKeywords = new()
        {
            ["Electronics"] = new() { "laptop", "computer", "mouse", "keyboard", "monitor", "phone", "tablet", "headphone", "speaker", "camera", "tv", "điện thoại", "máy tính", "chuột", "bàn phím" },
            ["Clothing"] = new() { "shirt", "pants", "dress", "shoes", "jacket", "hat", "socks", "áo", "quần", "giày", "mũ", "váy", "tất" },
            ["Books"] = new() { "book", "novel", "textbook", "magazine", "journal", "sách", "tiểu thuyết", "tạp chí", "giáo trình" },
            ["Food & Beverage"] = new() { "food", "drink", "coffee", "tea", "snack", "candy", "chocolate", "thức ăn", "đồ uống", "cà phê", "trà", "kẹo" },
            ["Home & Garden"] = new() { "furniture", "chair", "table", "lamp", "plant", "tool", "nội thất", "ghế", "bàn", "đèn", "cây", "dụng cụ" },
            ["Sports"] = new() { "ball", "sports", "fitness", "gym", "exercise", "bike", "bóng", "thể thao", "tập luyện", "xe đạp" },
            ["Beauty & Health"] = new() { "cosmetic", "skincare", "makeup", "medicine", "vitamin", "mỹ phẩm", "chăm sóc da", "trang điểm", "thuốc", "vitamin" },
            ["Toys"] = new() { "toy", "game", "puzzle", "doll", "car", "đồ chơi", "trò chơi", "búp bê", "xe" },
            ["Automotive"] = new() { "car", "auto", "tire", "oil", "battery", "ô tô", "xe hơi", "lốp", "dầu", "ắc quy" },
            ["Office Supplies"] = new() { "pen", "paper", "notebook", "folder", "stapler", "bút", "giấy", "sổ", "thư mục", "máy bấm kim" }
        };

        public CategoryPredictionService(ILogger<CategoryPredictionService> logger)
        {
            _logger = logger;
        }

        public Task<string> PredictCategoryAsync(string productName, string description)
        {
            try
            {
                // Combine name and description for analysis
                var text = $"{productName} {description}".ToLowerInvariant();

                // Simple keyword-based classification
                var categoryScores = new Dictionary<string, int>();

                foreach (var category in _categoryKeywords)
                {
                    var score = category.Value.Count(keyword => text.Contains(keyword.ToLowerInvariant()));
                    if (score > 0)
                    {
                        categoryScores[category.Key] = score;
                    }
                }

                if (categoryScores.Count > 0)
                {
                    var bestCategory = categoryScores.OrderByDescending(kvp => kvp.Value).First().Key;
                    _logger.LogInformation($"Predicted category '{bestCategory}' for product '{productName}'");
                    return Task.FromResult(bestCategory);
                }

                // Default category if no matches found
                return Task.FromResult("Uncategorized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error predicting category for product '{productName}'");
                return Task.FromResult("Uncategorized");
            }
        }

        public Task<IEnumerable<string>> GetSuggestedCategoriesAsync(string productName, string description, int maxSuggestions = 3)
        {
            try
            {
                var text = $"{productName} {description}".ToLowerInvariant();
                var categoryScores = new Dictionary<string, int>();

                foreach (var category in _categoryKeywords)
                {
                    var score = category.Value.Count(keyword => text.Contains(keyword.ToLowerInvariant()));
                    if (score > 0)
                    {
                        categoryScores[category.Key] = score;
                    }
                }

                var suggestions = categoryScores
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(maxSuggestions)
                    .Select(kvp => kvp.Key)
                    .ToList();

                // Always include some default suggestions if we don't have enough matches
                var defaultSuggestions = new[] { "Electronics", "Clothing", "Books", "Food & Beverage", "Home & Garden" };
                
                foreach (var defaultSuggestion in defaultSuggestions)
                {
                    if (suggestions.Count >= maxSuggestions) break;
                    if (!suggestions.Contains(defaultSuggestion))
                    {
                        suggestions.Add(defaultSuggestion);
                    }
                }

                _logger.LogInformation($"Generated {suggestions.Count} category suggestions for product '{productName}'");
                return Task.FromResult<IEnumerable<string>>(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating category suggestions for product '{productName}'");
                return Task.FromResult<IEnumerable<string>>(new[] { "Electronics", "Clothing", "Books" });
            }
        }
    }
}