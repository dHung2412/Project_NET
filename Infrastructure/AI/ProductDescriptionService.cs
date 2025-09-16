using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.AI
{
    public class ProductDescriptionService : IProductDescriptionService
    {
        private readonly ILogger<ProductDescriptionService> _logger;

        // Templates for generating descriptions based on category
        private readonly Dictionary<string, List<string>> _descriptionTemplates = new()
        {
            ["Electronics"] = new()
            {
                "Sản phẩm công nghệ hiện đại với thiết kế tinh tế và tính năng vượt trội.",
                "Thiết bị điện tử chất lượng cao, đáp ứng nhu cầu sử dụng hàng ngày.",
                "Công nghệ tiên tiến kết hợp với thiết kế ergonomic mang lại trải nghiệm tuyệt vời."
            },
            ["Clothing"] = new()
            {
                "Trang phục thời trang với chất liệu cao cấp, kiểu dáng hiện đại.",
                "Sản phẩm thời trang chất lượng, phù hợp với xu hướng đương đại.",
                "Thiết kế trendy với chất liệu thoải mái, phù hợp cho nhiều dịp khác nhau."
            },
            ["Books"] = new()
            {
                "Cuốn sách hay với nội dung phong phú và bổ ích.",
                "Tác phẩm được đánh giá cao với văn phong hấp dẫn.",
                "Kiến thức chuyên sâu được trình bày một cách dễ hiểu và thú vị."
            },
            ["Food & Beverage"] = new()
            {
                "Sản phẩm thực phẩm chất lượng cao với hương vị đặc trưng.",
                "Nguyên liệu tự nhiên, đảm bảo an toàn thực phẩm.",
                "Hương vị thơm ngon, được chế biến theo công thức truyền thống."
            },
            ["Default"] = new()
            {
                "Sản phẩm chất lượng cao với nhiều tính năng hữu ích.",
                "Thiết kế hiện đại, phù hợp với nhu cầu sử dụng đa dạng.",
                "Chất lượng đáng tin cậy với giá cả hợp lý."
            }
        };

        private readonly Dictionary<string, List<string>> _categoryFeatures = new()
        {
            ["Electronics"] = new()
            {
                "Tiết kiệm năng lượng", "Kết nối không dây", "Màn hình HD", "Pin bền",
                "Thiết kế compact", "Dễ sử dụng", "Bảo hành chính hãng", "Tương thích cao"
            },
            ["Clothing"] = new()
            {
                "Chất liệu cotton", "Thoáng mát", "Form chuẩn", "Màu sắc bền",
                "Dễ giặt ủi", "Kiểu dáng trẻ trung", "Size đa dạng", "Phong cách hiện đại"
            },
            ["Books"] = new()
            {
                "Nội dung hay", "Tác giả nổi tiếng", "Thiết kế bìa đẹp", "Giấy chất lượng",
                "Dễ đọc", "Kiến thức hữu ích", "Phù hợp mọi lứa tuổi", "Cập nhật thông tin mới"
            },
            ["Food & Beverage"] = new()
            {
                "Nguyên liệu tự nhiên", "Không chất bảo quản", "Hương vị đậm đà", "Giàu dinh dưỡng",
                "Dễ sử dụng", "Đóng gói tiện lợi", "Hạn sử dụng dài", "An toàn sức khỏe"
            }
        };

        public ProductDescriptionService(ILogger<ProductDescriptionService> logger)
        {
            _logger = logger;
        }

        public Task<string> GenerateDescriptionAsync(string productName, string category)
        {
            try
            {
                var templates = _descriptionTemplates.GetValueOrDefault(category, _descriptionTemplates["Default"]);
                var features = _categoryFeatures.GetValueOrDefault(category, _categoryFeatures["Electronics"]);

                var random = new Random();
                var selectedTemplate = templates[random.Next(templates.Count)];
                
                // Add 2-3 random features
                var selectedFeatures = features.OrderBy(x => random.Next()).Take(3).ToList();
                var featuresText = string.Join(", ", selectedFeatures.Select(f => f.ToLowerInvariant()));

                var description = $"{selectedTemplate} Các tính năng nổi bật: {featuresText}. " +
                                $"Sản phẩm {productName.ToLowerInvariant()} này sẽ đáp ứng tốt nhu cầu của bạn với chất lượng đáng tin cậy.";

                _logger.LogInformation($"Generated description for product '{productName}' in category '{category}'");
                return Task.FromResult(description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating description for product '{productName}'");
                return Task.FromResult($"Sản phẩm {productName} chất lượng cao với nhiều tính năng hữu ích và thiết kế hiện đại.");
            }
        }

        public async Task<string> ImproveDescriptionAsync(string currentDescription, string productName, string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currentDescription))
                {
                    return await GenerateDescriptionAsync(productName, category);
                }

                // Simple improvement: add more details and features
                var features = _categoryFeatures.GetValueOrDefault(category, _categoryFeatures["Electronics"]);
                var random = new Random();
                var additionalFeatures = features.OrderBy(x => random.Next()).Take(2).ToList();

                var improvements = new[]
                {
                    $"Đặc biệt phù hợp cho người dùng có nhu cầu {GetUsageContext(category)}.",
                    $"Tính năng bổ sung: {string.Join(", ", additionalFeatures.Select(f => f.ToLowerInvariant()))}.",
                    "Được nhiều khách hàng tin tưởng và đánh giá cao.",
                    "Cam kết chất lượng và dịch vụ hậu mãi tốt nhất."
                };

                var selectedImprovement = improvements[random.Next(improvements.Length)];
                var improvedDescription = $"{currentDescription.TrimEnd('.')}. {selectedImprovement}";

                _logger.LogInformation($"Improved description for product '{productName}'");
                return improvedDescription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error improving description for product '{productName}'");
                return currentDescription;
            }
        }

        public Task<IEnumerable<string>> GenerateProductTagsAsync(string productName, string description)
        {
            try
            {
                var tags = new List<string>();
                var text = $"{productName} {description}".ToLowerInvariant();

                // Category-based tags
                foreach (var category in _categoryFeatures.Keys)
                {
                    if (ContainsCategoryKeywords(text, category))
                    {
                        tags.Add(category);
                        
                        // Add specific features as tags
                        var relevantFeatures = _categoryFeatures[category]
                            .Where(feature => text.Contains(feature.ToLowerInvariant()) || 
                                            productName.ToLowerInvariant().Contains(feature.ToLowerInvariant()))
                            .Take(3);
                        
                        tags.AddRange(relevantFeatures);
                    }
                }

                // Add generic quality tags
                var qualityTags = new[] { "Chất lượng cao", "Đáng tin cậy", "Hiện đại", "Tiện dụng" };
                tags.AddRange(qualityTags.Take(2));

                // Remove duplicates and limit to 8 tags
                var uniqueTags = tags.Distinct().Take(8).ToList();

                _logger.LogInformation($"Generated {uniqueTags.Count} tags for product '{productName}'");
                return Task.FromResult<IEnumerable<string>>(uniqueTags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating tags for product '{productName}'");
                return Task.FromResult<IEnumerable<string>>(new[] { "Sản phẩm", "Chất lượng", "Đáng tin cậy" });
            }
        }

        private static string GetUsageContext(string category)
        {
            return category switch
            {
                "Electronics" => "công nghệ và giải trí",
                "Clothing" => "thời trang và phong cách",
                "Books" => "học tập và nghiên cứu",
                "Food & Beverage" => "ẩm thực và dinh dưỡng",
                "Home & Garden" => "trang trí và tiện nghi gia đình",
                "Sports" => "thể thao và sức khỏe",
                "Beauty & Health" => "làm đẹp và chăm sóc sức khỏe",
                "Toys" => "giải trí và phát triển trí tuệ",
                "Automotive" => "vận chuyển và di chuyển",
                "Office Supplies" => "văn phòng và học tập",
                _ => "sử dụng hàng ngày"
            };
        }

        private static bool ContainsCategoryKeywords(string text, string category)
        {
            var keywords = category switch
            {
                "Electronics" => new[] { "điện", "máy", "thiết bị", "công nghệ", "digital", "smart" },
                "Clothing" => new[] { "áo", "quần", "giày", "thời trang", "trang phục", "mặc" },
                "Books" => new[] { "sách", "đọc", "học", "giáo trình", "tiểu thuyết", "tác phẩm" },
                "Food & Beverage" => new[] { "ăn", "uống", "thức ăn", "đồ uống", "thực phẩm", "nước" },
                _ => new[] { category.ToLowerInvariant() }
            };

            return keywords.Any(keyword => text.Contains(keyword));
        }
    }
}