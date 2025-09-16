using Microsoft.EntityFrameworkCore;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.AI;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Database Configuration
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Repository Pattern & Unit of Work
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AI Services (tách module riêng)
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
    
    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Management API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Database Migration and Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Database initialized successfully!");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
    }
}

Console.WriteLine("🚀 Inventory Management System API is starting...");
Console.WriteLine("📊 Features:");
Console.WriteLine("   • CRUD sản phẩm với validation");
Console.WriteLine("   • Quản lý tồn kho thông minh");
Console.WriteLine("   • AI gợi ý danh mục sản phẩm");
Console.WriteLine("   • AI dự đoán tồn kho cần nhập");
Console.WriteLine("   • AI tạo/cải thiện mô tả sản phẩm");
Console.WriteLine("   • Báo cáo và phân tích xu hướng");
Console.WriteLine("   • Clean Architecture với DDD");
Console.WriteLine();

app.Run();