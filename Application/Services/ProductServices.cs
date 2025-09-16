using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;

namespace InventoryManagement.Application.Services
{
    public interface IProductService
    {   
        // CRUD + quản lý tồn kho
        Task<ProductDto> GetProductByIdAsync(Guid id);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto);
        Task UpdateStockLimitsAsync(Guid id, UpdateStockLimitsDto updateStockLimitsDto);
        Task DeleteProductAsync(Guid id);
        Task<bool> AddStockAsync(Guid id, AddStockDto addStockDto);
        Task<bool> RemoveStockAsync(Guid id, RemoveStockDto removeStockDto);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<IEnumerable<StockTransactionDto>> GetProductTransactionsAsync(Guid id);
    }

    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryPredictionService _categoryPredictionService;

        public ProductService(IUnitOfWork unitOfWork, ICategoryPredictionService categoryPredictionService)
        {
            _unitOfWork = unitOfWork;
            _categoryPredictionService = categoryPredictionService;
        }

        public async Task<ProductDto> GetProductByIdAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            var products = await _unitOfWork.Products.GetByCategoryAsync(category);
            return products.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await _unitOfWork.Products.GetLowStockProductsAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto)
        {
            // Sử dụng AI để gợi ý category nếu không được cung cấp
            string category = createProductDto.Category;
            if (string.IsNullOrWhiteSpace(category))
            {
                category = await _categoryPredictionService.PredictCategoryAsync(
                    createProductDto.Name, createProductDto.Description);
            }

            var product = new Product(
                createProductDto.Name,
                createProductDto.Description,
                category,
                createProductDto.Price,
                createProductDto.MinimumStock,
                createProductDto.MaximumStock,
                createProductDto.InitialStock
            );

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto updateProductDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            product.UpdateInfo(updateProductDto.Name, updateProductDto.Description, updateProductDto.Price);

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task UpdateStockLimitsAsync(Guid id, UpdateStockLimitsDto updateStockLimitsDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            product.UpdateStockLimits(updateStockLimitsDto.MinimumStock, updateStockLimitsDto.MaximumStock);

            await _unitOfWork.Products.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var exists = await _unitOfWork.Products.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            await _unitOfWork.Products.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> AddStockAsync(Guid id, AddStockDto addStockDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            var success = product.AddStock(addStockDto.Quantity, addStockDto.Reason);
            if (success)
            {
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }

            return success;
        }

        public async Task<bool> RemoveStockAsync(Guid id, RemoveStockDto removeStockDto)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

            var success = product.RemoveStock(removeStockDto.Quantity, removeStockDto.Reason);
            if (success)
            {
                await _unitOfWork.Products.UpdateAsync(product);
                await _unitOfWork.SaveChangesAsync();
            }

            return success;
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.Products.GetAllCategoriesAsync();
        }

        public async Task<IEnumerable<StockTransactionDto>> GetProductTransactionsAsync(Guid id)
        {
            var transactions = await _unitOfWork.StockTransactions.GetByProductIdAsync(id);
            var product = await _unitOfWork.Products.GetByIdAsync(id);

            return transactions.Select(t => new StockTransactionDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = product?.Name ?? "Unknown",
                TransactionType = t.Type == TransactionType.StockIn ? "Nhập kho" : "Xuất kho",
                Quantity = t.Quantity,
                Reason = t.Reason,
                TransactionDate = t.TransactionDate
            }).OrderByDescending(t => t.TransactionDate);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = product.Category,
                Price = product.Price,
                CurrentStock = product.CurrentStock,
                MinimumStock = product.MinimumStock,
                MaximumStock = product.MaximumStock,
                IsLowStock = product.IsLowStock(),
                IsOverStock = product.IsOverStock(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}