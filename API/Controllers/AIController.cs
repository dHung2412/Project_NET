using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Application.Services;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ILogger<AIController> _logger;

        public AIController(IAIService aiService, ILogger<AIController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        /// <summary>
        /// Gợi ý danh mục sản phẩm dựa trên tên và mô tả
        /// </summary>
        [HttpPost("suggest-category")]
        public async Task<ActionResult<string>> SuggestCategory([FromBody] CategorySuggestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductName))
                {
                    return BadRequest("Tên sản phẩm không được để trống");
                }

                var suggestedCategory = await _aiService.SuggestCategoryAsync(request.ProductName, request.Description ?? "");
                return Ok(new { category = suggestedCategory });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suggesting category for product {ProductName}", request.ProductName);
                return StatusCode(500, "Lỗi hệ thống khi gợi ý danh mục");
            }
        }

        /// <summary>
        /// Cải thiện mô tả sản phẩm hiện có
        /// </summary>
        [HttpPost("improve-description")]
        public async Task<ActionResult<string>> ImproveDescription([FromBody] DescriptionImprovementRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductName) || string.IsNullOrWhiteSpace(request.CurrentDescription))
                {
                    return BadRequest("Tên sản phẩm và mô tả hiện tại không được để trống");
                }

                var improvedDescription = await _aiService.ImproveProductDescriptionAsync(
                    request.CurrentDescription, request.ProductName, request.Category ?? "");
                return Ok(new { description = improvedDescription });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error improving description for product {ProductName}", request.ProductName);
                return StatusCode(500, "Lỗi hệ thống khi cải thiện mô tả sản phẩm");
            }
        }

        /// <summary>
        /// Phân tích tồn kho sản phẩm bằng AI
        /// </summary>
        [HttpGet("analyze-stock/{productId}")]
        public async Task<ActionResult<StockAnalysis>> AnalyzeStock(Guid productId)
        {
            try
            {
                var analysis = await _aiService.AnalyzeProductStockAsync(productId);
                return Ok(analysis);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing stock for product {ProductId}", productId);
                return StatusCode(500, "Lỗi hệ thống khi phân tích tồn kho");
            }
        }

        /// <summary>
        /// Lấy khuyến nghị nhập hàng thông minh
        /// </summary>
        [HttpGet("stock-recommendations")]
        public async Task<ActionResult<IEnumerable<StockRecommendation>>> GetStockRecommendations()
        {
            try
            {
                var recommendations = await _aiService.GetStockRecommendationsAsync();
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock recommendations");
                return StatusCode(500, "Lỗi hệ thống khi lấy khuyến nghị nhập hàng");
            }
        }

        /// <summary>
        /// Lấy danh sách các gợi ý danh mục
        /// </summary>
        [HttpPost("category-suggestions")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategorySuggestions([FromBody] CategorySuggestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductName))
                {
                    return BadRequest("Tên sản phẩm không được để trống");
                }

                var suggestions = await _aiService.GetCategorySuggestionsAsync(request.ProductName, request.Description ?? "");
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category suggestions for product {ProductName}", request.ProductName);
                return StatusCode(500, "Lỗi hệ thống khi lấy gợi ý danh mục");
            }
        }

        /// <summary>
        /// Tạo mô tả sản phẩm tự động
        /// </summary>
        [HttpPost("generate-description")]
        public async Task<ActionResult<string>> GenerateDescription([FromBody] DescriptionGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.ProductName))
                {
                    return BadRequest("Tên sản phẩm không được để trống");
                }

                var description = await _aiService.GenerateProductDescriptionAsync(request.ProductName, request.Category ?? "");
                return Ok(new { description });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating description for product {ProductName}", request.ProductName);
                return StatusCode(500, "Lỗi hệ thống khi tạo mô tả sản phẩm");
            }
        }
    }

    // Request DTOs
    public class CategorySuggestionRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DescriptionGenerationRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    public class DescriptionImprovementRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string CurrentDescription { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}