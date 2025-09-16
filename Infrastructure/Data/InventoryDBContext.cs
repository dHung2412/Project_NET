using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Entities;

namespace InventoryManagement.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<StockTransaction> StockTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.CurrentStock)
                    .IsRequired();

                entity.Property(e => e.MinimumStock)
                    .IsRequired();

                entity.Property(e => e.MaximumStock)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired();

                // Index for better query performance
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CurrentStock);
            });

            // StockTransaction Configuration
            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.ProductId)
                    .IsRequired();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.Reason)
                    .HasMaxLength(500);

                entity.Property(e => e.TransactionDate)
                    .IsRequired();

                // Index for better query performance
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => new { e.ProductId, e.TransactionDate });
            });

            // Configure relationship (optional - EF Core can infer this)
            modelBuilder.Entity<StockTransaction>()
                .HasOne<Product>()
                .WithMany(p => p.StockTransactions)
                .HasForeignKey(st => st.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed some sample products
            var sampleProducts = new[]
            {
                new 
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                    Name = "Laptop Dell Inspiron 15",
                    Description = "Laptop văn phòng hiệu năng cao với CPU Intel i5, RAM 8GB, SSD 256GB",
                    Category = "Electronics",
                    Price = 15000000m,
                    CurrentStock = 25,
                    MinimumStock = 5,
                    MaximumStock = 50,
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new 
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                    Name = "Chuột không dây Logitech",
                    Description = "Chuột không dây ergonomic với độ chính xác cao",
                    Category = "Electronics",
                    Price = 500000m,
                    CurrentStock = 8,
                    MinimumStock = 10,
                    MaximumStock = 100,
                    CreatedAt = DateTime.UtcNow.AddDays(-25),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new 
                {
                    Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440003"),
                    Name = "Bàn phím cơ Gaming",
                    Description = "Bàn phím cơ RGB với switch Cherry MX Blue",
                    Category = "Electronics",
                    Price = 1200000m,
                    CurrentStock = 2,
                    MinimumStock = 5,
                    MaximumStock = 30,
                    CreatedAt = DateTime.UtcNow.AddDays(-20),
                    UpdatedAt = DateTime.UtcNow.AddDays(-20)
                }
            };

            foreach (var product in sampleProducts)
            {
                modelBuilder.Entity<Product>().HasData(product);
            }
        }
    }
}