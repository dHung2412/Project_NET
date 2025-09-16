using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Domain.Interfaces
{
    public interface IProductRepository
    {   
        //CRUD + query cho Product
        // Lấy sản phẩm theo Id, danh mục, tất cả
        Task<Product?> GetByIdAsync(Guid id);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(string category);
        // Lấy các sản phẩm tồn kho thấp.
        Task<IEnumerable<Product>> GetLowStockProductsAsync();
        // Thêm, cập nhật, xóa.
        Task<Product> AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        // Lấy danh sách tất cả categories.
        Task<IEnumerable<string>> GetAllCategoriesAsync();
    }

    public interface IStockTransactionRepository
    {   
        //CRUD cho StockTransaction
        Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId);
        Task<IEnumerable<StockTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 50);
        Task AddAsync(StockTransaction transaction);
    }

    public interface IUnitOfWork : IDisposable
    {   
        // Quản lý transaction
        IProductRepository Products { get; }
        IStockTransactionRepository StockTransactions { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}