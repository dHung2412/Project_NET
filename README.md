# 🏪 Hệ thống Quản lý Kho Sản phẩm kèm AI

Hệ thống quản lý kho hiện đại được xây dựng với .NET 8, Entity Framework Core và tích hợp AI để hỗ trợ quyết định kinh doanh thông minh.

## 🎯 Tính năng chính

### 📦 Quản lý Sản phẩm
- ✅ CRUD sản phẩm hoàn chỉnh (Create, Read, Update, Delete)
- ✅ Quản lý thông tin: tên, mô tả, danh mục, giá cả
- ✅ Validation dữ liệu đầu vào chi tiết
- ✅ Tìm kiếm và lọc sản phẩm theo danh mục

### 📊 Quản lý Tồn kho
- ✅ Theo dõi tồn kho theo thời gian thực
- ✅ Thiết lập tồn kho tối thiểu/tối đa
- ✅ Nhập/xuất kho với ghi nhận lý do
- ✅ Cảnh báo tự động khi tồn kho thấp
- ✅ Lịch sử giao dịch chi tiết

### 🤖 Tính năng AI thông minh
- 🔮 **Gợi ý danh mục**: AI phân tích tên và mô tả để đề xuất danh mục phù hợp
- 📈 **Dự đoán tồn kho**: Phân tích xu hướng và đưa ra khuyến nghị nhập hàng
- ✍️ **Tạo mô tả sản phẩm**: AI sinh mô tả sản phẩm tự động
- 📊 **Phân tích xu hướng**: Báo cáo chi tiết về tình hình tồn kho

### 📋 Báo cáo & Phân tích
- 📊 Dashboard tổng quan
- 📈 Biểu đồ xu hướng tồn kho
- 🔍 Phân tích mức tiêu thụ hàng ngày
- 💡 Khuyến nghị thời điểm nhập hàng tối ưu

## 🏗️ Kiến trúc Clean Architecture

Hệ thống được thiết kế theo nguyên tắc Domain-Driven Design (DDD) với 3 layer chính:

```
📁 Domain Layer (Entities + Interfaces)
├── 🏷️ Product.cs - Entity sản phẩm với business logic
├── 📝 StockTransaction.cs - Entity giao dịch kho
└── 🔌 Interfaces/ - Contracts cho Repository và AI Services

📁 Application Layer (Use Cases + Services)
├── 📋 DTOs/ - Data Transfer Objects
├── 🔧 Services/ - Business logic orchestration
└── 🎯 Use Cases - Các trường hợp sử dụng

📁 Infrastructure Layer (Implementation)
├── 💾 Data/ - Database context và configurations
├── 📚 Repositories/ - Data access implementations
└── 🤖 AI/ - AI services implementations

📁 API Layer (Controllers)
├── 🎮 ProductsController - REST API cho sản phẩm
└── 🤖 AIController - REST API cho tính năng AI
```

## 🚀 Cài đặt và Chạy

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server hoặc LocalDB
- Visual Studio 2022 hoặc VS Code

### Bước 1: Clone repository
```bash
git clone <repository-url>
cd InventoryManagement
```

### Bước 2: Khôi phục packages
```bash
dotnet restore
```

### Bước 3: Cấu hình database
```bash
# Cập nhật connection string trong appsettings.json
dotnet ef database update
```

### Bước 4: Chạy ứng dụng
```bash
dotnet run
```

Mở trình duyệt và truy cập: `https://localhost:5001` để xem Swagger API Documentation.

## 📚 API Endpoints

### 🛍️ Products API
```http
GET    /api/products              # Lấy tất cả sản phẩm
GET    /api/products/{id}         # Lấy sản phẩm theo ID
GET    /api/products/category/{category}  # Lấy sản phẩm theo danh mục
GET    /api/products/low-stock    # Lấy sản phẩm tồn kho thấp
POST   /api/products              # Tạo sản phẩm mới
PUT    /api/products/{id}         # Cập nhật sản phẩm
DELETE /api/products/{id}         # Xóa sản phẩm

POST   /api/products/{id}/stock/add     # Nhập hàng
POST   /api/products/{id}/stock/remove  # Xuất hàng
GET    /api/products/{id}/transactions  # Lịch sử giao dịch
```

### 🤖 AI API
```http
POST   /api/ai/suggest-category         # Gợi ý danh mục
POST   /api/ai/category-suggestions     # Nhiều gợi ý danh mục
POST   /api/ai/generate-description     # Tạo mô tả tự động
POST   /api/ai/improve-description      # Cải thiện mô tả
GET    /api/ai/analyze-stock/{id}       # Phân tích tồn kho
GET    /api/ai/stock-recommendations    # Khuyến nghị nhập hàng
```

## 🔧 Cấu hình AI Services

### Keyword-based Classification (Mặc định)
Hệ thống sử dụng thuật toán phân loại dựa trên từ khóa, hoạt động offline và không cần API key.

### Advanced AI Integration (Tùy chọn)
Để sử dụng OpenAI GPT, cập nhật `appsettings.json`:

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "your-openai-api-key",
      "Model": "gpt-3.5-turbo"
    }
  }
}
```

## 🧪 Testing

### Unit Tests
```bash
dotnet test
```

### Integration Tests
```bash
dotnet test --filter Category=Integration
```

## 📊 Sample Data

Hệ thống đi kèm với dữ liệu mẫu:
- 3 sản phẩm electronics
- Các giao dịch nhập/xuất kho
- Cấu hình tồn kho tối thiểu/tối đa

## 🔮 Roadmap

### Phase 1 (Hiện tại)
- ✅ Basic CRUD operations
- ✅ Stock management
- ✅ Simple AI suggestions
- ✅ Clean Architecture implementation

### Phase 2 (Tương lai)
- 🔄 ML.NET integration
- 🔄 Advanced forecasting algorithms  
- 🔄 Real-time notifications
- 🔄 Mobile app support
- 🔄 Multi-warehouse management

### Phase 3 (Dài hạn)
- 🔄 Machine Learning model training
- 🔄 Supplier integration
- 🔄 Advanced analytics dashboard
- 🔄 IoT sensors integration

## 🤝 Đóng góp

Chúng tôi hoan nghênh mọi đóng góp! Vui lòng:

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Tạo Pull Request

## 📄 License

MIT License - xem file [LICENSE](LICENSE) để biết chi tiết.

## 📞 Liên hệ

- 📧 Email: developer@company.com
- 💼 LinkedIn: [Profile](https://linkedin.com/in/developer)
- 🐛 Issues: [GitHub Issues](https://github.com/repo/issues)

---

⭐ **Nếu project này hữu ích, hãy cho chúng tôi một star!** ⭐