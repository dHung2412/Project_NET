using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Repositories;
using InventoryManagement.Infrastructure.AI;
using InventoryManagement.Domain.Interfaces;
using InventoryManagement.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Memory Cache for user sessions and caching
builder.Services.AddMemoryCache();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5123); // HTTP
    options.ListenAnyIP(5125, listenOptions => listenOptions.UseHttps()); // HTTPS nếu cần
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

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
    };
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("AllRoles", policy => policy.RequireRole("Admin", "Manager", "Employee"));
});


// Repository Pattern & Unit of Work
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// AI Services
builder.Services.AddScoped<ICategoryPredictionService, CategoryPredictionService>();
builder.Services.AddScoped<IStockPredictionService, StockPredictionService>();
builder.Services.AddScoped<IProductDescriptionService, ProductDescriptionService>();

// Application Services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Inventory Management System API", 
        Version = "v1",
        Description = "Hệ thống quản lý kho sản phẩm kèm AI hỗ trợ với xác thực JWT"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000") // React app URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Important for cookies
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

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization với error handling
await InitializeDatabaseAsync(app.Services);

Console.WriteLine("🚀 Inventory Management System API is running...");
Console.WriteLine($"📍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("📊 Features:");
Console.WriteLine("   • JWT Authentication & Authorization");
Console.WriteLine("   • Role-based access control (Admin, Manager, Employee)");
Console.WriteLine("   • CRUD sản phẩm với validation");
Console.WriteLine("   • Quản lý tồn kho thông minh");
Console.WriteLine("   • AI gợi ý danh mục sản phẩm");
Console.WriteLine("   • AI dự đoán tồn kho cần nhập");
Console.WriteLine("   • AI tạo/cải thiện mô tả sản phẩm");
Console.WriteLine("   • Báo cáo và phân tích xu hướng");
Console.WriteLine("   • Clean Architecture với DDD");
Console.WriteLine();
Console.WriteLine($"🌐 HTTP: http://localhost:5123");
Console.WriteLine($"🔒 HTTPS: https://localhost:5125");
Console.WriteLine($"📖 Swagger UI: https://localhost:5125/swagger");
Console.WriteLine();
Console.WriteLine("🔐 Default Admin Account:");
Console.WriteLine("   Username: admin");
Console.WriteLine("   Password: admin123");

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
            await SeedDataAsync(context, logger);
        }
        else
        {
            await context.Database.MigrateAsync();
            await SeedDataAsync(context, logger);

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
static async Task SeedDataAsync(InventoryDbContext context, ILogger logger)
{
    try
    {
        // Seed default admin user if not exists
        if (!await context.Users.AnyAsync())
        {
            logger.LogInformation("Creating default admin user...");
            
            // Create admin user
            var adminPasswordHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.Create()
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes("admin123" + "InventorySalt2024"))
            );

            var adminUser = new InventoryManagement.Domain.Entities.User(
                "admin", 
                "admin@inventory.com", 
                adminPasswordHash,
                "System",
                "Administrator",
                InventoryManagement.Domain.Entities.UserRole.Admin
            );

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            
            logger.LogInformation("Default admin user created successfully!");
        }

        // Seed sample products if not exists
        if (!await context.Products.AnyAsync())
        {
            logger.LogInformation("Creating sample products...");
            
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

            // Note: In real implementation, you would use Product constructor
            // This is just for seeding demonstration
            logger.LogInformation("Sample products would be created here");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error seeding initial data");
    }
}