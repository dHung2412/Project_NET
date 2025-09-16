using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Application.DTOs
{
    public class ProductDto // Dùng khi trả dữ liệu ra ngoài
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int MaximumStock { get; set; }
        public bool IsLowStock { get; set; }
        public bool IsOverStock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateProductDto // Dùng khi tạo sản phẩm mới 
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Danh mục không được vượt quá 100 ký tự")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Tồn kho tối thiểu là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho tối thiểu phải lớn hơn hoặc bằng 0")]
        public int MinimumStock { get; set; }

        [Required(ErrorMessage = "Tồn kho tối đa là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tồn kho tối đa phải lớn hơn 0")]
        public int MaximumStock { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho ban đầu phải lớn hơn hoặc bằng 0")]
        public int InitialStock { get; set; } = 0;
    }

    public class UpdateProductDto // Update thông tin cơ bản
    {
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được vượt quá 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }
    }

    public class UpdateStockLimitsDto // Dùng riêng để thay đổi giới hạn tồn kho (min/max)
    {
        [Required(ErrorMessage = "Tồn kho tối thiểu là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho tối thiểu phải lớn hơn hoặc bằng 0")]
        public int MinimumStock { get; set; }

        [Required(ErrorMessage = "Tồn kho tối đa là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Tồn kho tối đa phải lớn hơn 0")]
        public int MaximumStock { get; set; }
    }

    public class StockTransactionDto // Dùng khi trả lịch sử giao dịch
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
    }

    // Dùng cho action nhập hàng hoặc xuất hàng
    public class AddStockDto
    {
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }

    public class RemoveStockDto
    {
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }
}