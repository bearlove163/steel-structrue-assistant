using Microsoft.EntityFrameworkCore;
using EngineeringApp.Server.Data;
using EngineeringApp.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

// 配置 SQLite EF Core 数据库
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "engineering_workstation.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// 允许 Blazor WASM 客户端跨域访问
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// 自动建库并填充基准数据
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    AppDbContext.SeedData(db);
}

// ======================== API 路由端点 ========================

// 1. 系统健康检查与数据概览
app.MapGet("/api/health", async (AppDbContext db) =>
{
    var steelCount = await db.StandardSteels.CountAsync();
    var paintCount = await db.PaintProducts.CountAsync();
    return Results.Ok(new
    {
        Status = "Online",
        Database = "SQLite (engineering_workstation.db)",
        StandardSteelsCount = steelCount,
        PaintProductsCount = paintCount,
        Timestamp = DateTime.UtcNow
    });
});

// 2. 油漆产品与价格列表
app.MapGet("/api/database/paints", async (AppDbContext db) =>
{
    var paints = await db.PaintProducts.OrderBy(p => p.LayerType).ThenBy(p => p.Id).ToListAsync();
    return Results.Ok(paints);
});

// 3. 更新油漆产品价格与指标
app.MapPut("/api/database/paints/{id:int}", async (int id, PaintProductEntity input, AppDbContext db) =>
{
    var item = await db.PaintProducts.FindAsync(id);
    if (item == null) return Results.NotFound();

    item.Name = input.Name;
    item.Brand = input.Brand;
    item.MaterialUnitPricePerKg = input.MaterialUnitPricePerKg;
    item.LaborUnitPricePerM2 = input.LaborUnitPricePerM2;
    item.VolumeSolidsPercent = input.VolumeSolidsPercent;
    item.DensityKgL = input.DensityKgL;
    item.DefaultDft = input.DefaultDft;
    item.LossRatePercent = input.LossRatePercent;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

// 4. 获取欧标型钢数据库
app.MapGet("/api/database/sections/euro", async (AppDbContext db) =>
{
    var items = await db.StandardSteels
        .Where(s => s.StandardSystem.Contains("欧标") || s.StandardSystem.Contains("EN"))
        .ToListAsync();
    return Results.Ok(items);
});

// 5. 获取美标型钢数据库
app.MapGet("/api/database/sections/aisc", async (AppDbContext db) =>
{
    var items = await db.StandardSteels
        .Where(s => s.StandardSystem.Contains("美标") || s.StandardSystem.Contains("AISC"))
        .ToListAsync();
    return Results.Ok(items);
});

// 6. 获取国标型钢数据库
app.MapGet("/api/database/sections/gb", async (AppDbContext db) =>
{
    var items = await db.StandardSteels
        .Where(s => s.StandardSystem.Contains("国标") || s.StandardSystem.Contains("GB"))
        .ToListAsync();
    return Results.Ok(items);
});

// 7. 全局型钢综合检索接口
app.MapGet("/api/database/sections", async (string? system, string? category, string? keyword, AppDbContext db) =>
{
    var query = db.StandardSteels.AsQueryable();

    if (!string.IsNullOrWhiteSpace(system) && system != "全部标准体系")
        query = query.Where(s => s.StandardSystem.Contains(system));

    if (!string.IsNullOrWhiteSpace(category) && category != "全部系列")
        query = query.Where(s => s.Category.Contains(category));

    if (!string.IsNullOrWhiteSpace(keyword))
        query = query.Where(s => s.Designation.Contains(keyword) || s.Category.Contains(keyword));

    var results = await query.Take(200).ToListAsync();
    return Results.Ok(results);
});

app.Run();
