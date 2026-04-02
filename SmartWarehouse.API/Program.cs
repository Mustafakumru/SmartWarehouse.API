// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Managers;
using SmartWarehouse.API.Managers.Interfaces;
using SmartWarehouse.API.Repositories;
using SmartWarehouse.API.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── DbContext ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SmartWarehouseDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repository Kayıtları ─────────────────────────────────────────────────────
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IWarehouseZoneRepository, WarehouseZoneRepository>();
builder.Services.AddScoped<IWarehouseRackRepository, WarehouseRackRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IWarehouseStockRepository, WarehouseStockRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();

// ── Manager Kayıtları ────────────────────────────────────────────────────────
builder.Services.AddScoped<IProductCategoryManager, ProductCategoryManager>();
builder.Services.AddScoped<IWarehouseZoneManager, WarehouseZoneManager>();
builder.Services.AddScoped<IWarehouseRackManager, WarehouseRackManager>();
builder.Services.AddScoped<IProductManager, ProductManager>();
builder.Services.AddScoped<IInventoryTransactionManager, InventoryTransactionManager>();

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // KIRMIZI ÇİZGİ: JSON PascalCase olarak serialize/deserialize edilir.
        // Frontend camelCase'e kendi tarafında dönüştürür.
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ── Swagger ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Smart Warehouse API",
        Version = "v1",
        Description = "Akıllı Depo Yönetimi REST API — .NET 9.0"
    });
});

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Warehouse API v1");
        options.RoutePrefix = string.Empty;
    });
}
app.UseHttpsRedirection(); // ✅ EKLENDİ
app.UseRouting();          // ✅ EKLENDİ
app.UseCors("AllowReact"); // ✅ UseRouting'den SONRA olmalı

app.UseAuthorization();
app.MapControllers();

app.Run();