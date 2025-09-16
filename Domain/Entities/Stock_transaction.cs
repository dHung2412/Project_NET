using System;

namespace InventoryManagement.Domain.Entities
{
    public class StockTransaction
    {
        public Guid Id { get; private set; }
        public Guid ProductId { get; private set; }
        public TransactionType Type { get; private set; }
        public int Quantity { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public DateTime TransactionDate { get; private set; }

        public StockTransaction(Guid productId, TransactionType type, int quantity, string reason)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            Id = Guid.NewGuid();
            ProductId = productId;
            Type = type;
            Quantity = quantity;
            Reason = reason ?? string.Empty;
            TransactionDate = DateTime.UtcNow;
        }

        // Private constructor for EF Core
        private StockTransaction() { }
    }

    public enum TransactionType
    {
        StockIn = 1,
        StockOut = 2
    }
}