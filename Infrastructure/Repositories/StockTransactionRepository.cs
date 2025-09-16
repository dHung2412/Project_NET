using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Infrastructure.Data;

namespace InventoryManagement.Infrastructure.Repositories
{
    public class StockTransactionRepository : IStockTransactionRepository
    {
        private readonly InventoryDbContext _context;

        public StockTransactionRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StockTransaction>> GetByProductIdAsync(Guid productId)
        {
            return await _context.StockTransactions
                .Where(st => st.ProductId == productId)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockTransactions
                .Where(st => st.TransactionDate >= startDate && st.TransactionDate <= endDate)
                .OrderByDescending(st => st.TransactionDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<StockTransaction>> GetRecentTransactionsAsync(int count = 50)
        {
            return await _context.StockTransactions
                .OrderByDescending(st => st.TransactionDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task AddAsync(StockTransaction transaction)
        {
            await _context.StockTransactions.AddAsync(transaction);
        }
    }
}