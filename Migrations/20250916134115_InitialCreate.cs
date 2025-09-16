using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventoryManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentStock = table.Column<int>(type: "int", nullable: false),
                    MinimumStock = table.Column<int>(type: "int", nullable: false),
                    MaximumStock = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedAt", "CurrentStock", "Description", "MaximumStock", "MinimumStock", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("550e8400-e29b-41d4-a716-446655440001"), "Electronics", new DateTime(2025, 8, 17, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9243), 25, "Laptop văn phòng hiệu năng cao với CPU Intel i5, RAM 8GB, SSD 256GB", 50, 5, "Laptop Dell Inspiron 15", 15000000m, new DateTime(2025, 8, 17, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9249) },
                    { new Guid("550e8400-e29b-41d4-a716-446655440002"), "Electronics", new DateTime(2025, 8, 22, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9253), 8, "Chuột không dây ergonomic với độ chính xác cao", 100, 10, "Chuột không dây Logitech", 500000m, new DateTime(2025, 8, 22, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9253) },
                    { new Guid("550e8400-e29b-41d4-a716-446655440003"), "Electronics", new DateTime(2025, 8, 27, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9255), 2, "Bàn phím cơ RGB với switch Cherry MX Blue", 30, 5, "Bàn phím cơ Gaming", 1200000m, new DateTime(2025, 8, 27, 13, 41, 10, 344, DateTimeKind.Utc).AddTicks(9255) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CurrentStock",
                table: "Products",
                column: "CurrentStock");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId",
                table: "StockTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_ProductId_TransactionDate",
                table: "StockTransactions",
                columns: new[] { "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_TransactionDate",
                table: "StockTransactions",
                column: "TransactionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockTransactions");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
