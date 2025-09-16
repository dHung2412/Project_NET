using System;
using System.Collections.Generic;

namespace InventoryManagement.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int CurrentStock { get; private set; }
        public int MinimumStock { get; private set; }
        public int MaximumStock { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<StockTransaction> _stockTransactions = new();
        public IReadOnlyList<StockTransaction> StockTransactions => _stockTransactions.AsReadOnly();

        // Constructor for new product
        public Product(string name, string description, string category, decimal price, 
                      int minimumStock, int maximumStock, int initialStock = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));
            
            if (minimumStock < 0)
                throw new ArgumentException("Minimum stock cannot be negative", nameof(minimumStock));
            
            if (maximumStock <= minimumStock)
                throw new ArgumentException("Maximum stock must be greater than minimum stock", nameof(maximumStock));

            Id = Guid.NewGuid();
            Name = name;
            Description = description ?? string.Empty;
            Category = category ?? "Uncategorized";
            Price = price;
            MinimumStock = minimumStock;
            MaximumStock = maximumStock;
            CurrentStock = initialStock;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

            if (initialStock > 0)
            {
                AddStockTransaction(TransactionType.StockIn, initialStock, "Initial stock");
            }
        }

        // Private constructor for EF Core
        private Product() { }

        public void UpdateInfo(string name, string description, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));

            Name = name;
            Description = description ?? string.Empty;
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateCategory(string category)
        {
            Category = category ?? "Uncategorized";
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStockLimits(int minimumStock, int maximumStock)
        {
            if (minimumStock < 0)
                throw new ArgumentException("Minimum stock cannot be negative", nameof(minimumStock));
            
            if (maximumStock <= minimumStock)
                throw new ArgumentException("Maximum stock must be greater than minimum stock", nameof(maximumStock));

            MinimumStock = minimumStock;
            MaximumStock = maximumStock;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool AddStock(int quantity, string reason = "Stock added")
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (CurrentStock + quantity > MaximumStock)
                return false; // Cannot exceed maximum stock

            CurrentStock += quantity;
            AddStockTransaction(TransactionType.StockIn, quantity, reason);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public bool RemoveStock(int quantity, string reason = "Stock removed")
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            if (CurrentStock < quantity)
                return false; // Insufficient stock

            CurrentStock -= quantity;
            AddStockTransaction(TransactionType.StockOut, quantity, reason);
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        public bool IsLowStock()
        {
            return CurrentStock <= MinimumStock;
        }

        public bool IsOverStock()
        {
            return CurrentStock >= MaximumStock;
        }

        public int GetRecommendedOrderQuantity()
        {
            if (!IsLowStock()) return 0;
            
            // Simple logic: order enough to reach optimal stock level (75% of max)
            int optimalStock = (int)(MaximumStock * 0.75);
            return Math.Max(0, optimalStock - CurrentStock);
        }

        private void AddStockTransaction(TransactionType type, int quantity, string reason)
        {
            var transaction = new StockTransaction(Id, type, quantity, reason);
            _stockTransactions.Add(transaction);
        }
    }
}