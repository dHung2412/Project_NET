using Microsoft.EntityFrameworkCore;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.AI;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5123); // HTTP
    // options.ListenAnyIP(5125, listenOptions => listenOptions.UseHttps()); // HTTPS nếu cần
});

// Database Configuration - Sử dụng In-Memory Database cho development
builder.Services.AddDbContext<InventoryDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Sử dụng In-Memory Database cho development
        options.UseInMemoryDatabase("InventoryDb");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions => sqlOptions.EnableRetryOnFailure());
    }
});

// Repository Pattern & Unit of Work
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AI Services
builder.Services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
builder.Services.AddScoped<IStockPredictionService, StockPredictionService>();
builder.Services.AddScoped<IProductDescriptionService, ProductDescriptionService>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAIService, AIService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Inventory Management System API", 
        Version = "v1",
        Description = "Hệ thống quản lý kho sản phẩm kèm AI hỗ trợ"
    });
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline - ĐÚng thứ tự middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API V1");
        c.RoutePrefix = string.Empty; // Swagger UI tại root
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Database initialization với error handling
await InitializeDatabaseAsync(app.Services);
var httpUrl = builder.Configuration["Kestrel:Endpoints:Http:Url"];
var httpsUrl = builder.Configuration["Kestrel:Endpoints:Https:Url"];

Console.WriteLine("🚀 Inventory Management System API is running...");
Console.WriteLine($"📍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("📊 Features:");
Console.WriteLine("   • CRUD sản phẩm với validation");
Console.WriteLine("   • Quản lý tồn kho thông minh");
Console.WriteLine("   • AI gợi ý danh mục sản phẩm");
Console.WriteLine("   • AI dự đoán tồn kho cần nhập");
Console.WriteLine("   • AI tạo/cải thiện mô tả sản phẩm");
Console.WriteLine("   • Báo cáo và phân tích xu hướng");
Console.WriteLine("   • Clean Architecture với DDD");
Console.WriteLine();
Console.WriteLine($"🌐 HTTP: {httpUrl}");
Console.WriteLine($"🔒 HTTPS: {httpsUrl}");
Console.WriteLine($"📖 Swagger UI: {httpUrl}/swagger");

app.Run();

// Helper method cho database initialization
static async Task InitializeDatabaseAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        if (context.Database.IsInMemory())
        {
            await context.Database.EnsureCreatedAsync();
            await SeedDataAsync(context);
        }
        else
        {
            await context.Database.MigrateAsync();
        }
        
        logger.LogInformation("Database initialized successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        // Không throw exception để app vẫn có thể start
    }
}

// Seed data method
static async Task SeedDataAsync(InventoryDbContext context)
{
    if (await context.Products.AnyAsync()) return; // Đã có data

    var products = new[]
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
        }
        // Add more sample products...
    };

    // Seed logic here if needed
}