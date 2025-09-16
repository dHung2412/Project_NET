using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Services;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả sản phẩm
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, "Lỗi hệ thống khi lấy danh sách sản phẩm");
            }
        }

        /// <summary>
        /// Lấy thông tin sản phẩm theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi lấy thông tin sản phẩm");
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm theo danh mục
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(string category)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(category);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products by category {Category}", category);
                return StatusCode(500, "Lỗi hệ thống khi lấy sản phẩm theo danh mục");
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm có tồn kho thấp
        /// </summary>
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts()
        {
            try
            {
                var products = await _productService.GetLowStockProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                return StatusCode(500, "Lỗi hệ thống khi lấy sản phẩm tồn kho thấp");
            }
        }

        /// <summary>
        /// Tạo sản phẩm mới
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.CreateProductAsync(createProductDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Lỗi hệ thống khi tạo sản phẩm mới");
            }
        }

        /// <summary>
        /// Cập nhật thông tin sản phẩm
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductDto updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.UpdateProductAsync(id, updateProductDto);
                return Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi cập nhật sản phẩm");
            }
        }

        /// <summary>
        /// Cập nhật giới hạn tồn kho
        /// </summary>
        [HttpPut("{id}/stock-limits")]
        public async Task<IActionResult> UpdateStockLimits(Guid id, UpdateStockLimitsDto updateStockLimitsDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _productService.UpdateStockLimitsAsync(id, updateStockLimitsDto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock limits for product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi cập nhật giới hạn tồn kho");
            }
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi xóa sản phẩm");
            }
        }

        /// <summary>
        /// Nhập hàng vào kho
        /// </summary>
        [HttpPost("{id}/stock/add")]
        public async Task<IActionResult> AddStock(Guid id, AddStockDto addStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _productService.AddStockAsync(id, addStockDto);
                if (!success)
                {
                    return BadRequest("Không thể nhập hàng: vượt quá tồn kho tối đa");
                }

                return Ok(new { message = $"Đã nhập {addStockDto.Quantity} sản phẩm vào kho" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding stock for product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi nhập hàng");
            }
        }

        /// <summary>
        /// Xuất hàng khỏi kho
        /// </summary>
        [HttpPost("{id}/stock/remove")]
        public async Task<IActionResult> RemoveStock(Guid id, RemoveStockDto removeStockDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _productService.RemoveStockAsync(id, removeStockDto);
                if (!success)
                {
                    return BadRequest("Không thể xuất hàng: không đủ tồn kho");
                }

                return Ok(new { message = $"Đã xuất {removeStockDto.Quantity} sản phẩm khỏi kho" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing stock for product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi xuất hàng");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả danh mục
        /// </summary>
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetAllCategories()
        {
            try
            {
                var categories = await _productService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, "Lỗi hệ thống khi lấy danh sách danh mục");
            }
        }

        /// <summary>
        /// Lấy lịch sử giao dịch của sản phẩm
        /// </summary>
        [HttpGet("{id}/transactions")]
        public async Task<ActionResult<IEnumerable<StockTransactionDto>>> GetProductTransactions(Guid id)
        {
            try
            {
                var transactions = await _productService.GetProductTransactionsAsync(id);
                return Ok(transactions);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Không tìm thấy sản phẩm với ID: {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for product {ProductId}", id);
                return StatusCode(500, "Lỗi hệ thống khi lấy lịch sử giao dịch");
            }
        }
    }
}