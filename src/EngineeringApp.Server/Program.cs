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

// ======================== 材料定价域 API 端点 ========================

// 8. 获取材料价格时间快照列表
app.MapGet("/api/pricing/snapshots", async (AppDbContext db) =>
{
    var snapshots = await db.MaterialPriceSnapshots
        .OrderByDescending(s => s.EffectiveDate)
        .ToListAsync();
    return Results.Ok(snapshots);
});

// 9. 获取全国及国际钢厂品牌字典 (从数据库持久化层获取)
app.MapGet("/api/pricing/mills", async (AppDbContext db) =>
{
    var mills = await db.SteelMills.OrderBy(m => m.Tier).ThenBy(m => m.Id).ToListAsync();
    return Results.Ok(mills);
});

// 9.1 更新钢厂自定义溢价
app.MapPut("/api/pricing/mills/{millId}", async (string millId, double? customPremium, AppDbContext db) =>
{
    var mill = await db.SteelMills.FirstOrDefaultAsync(m => m.MillId == millId);
    if (mill == null) return Results.NotFound();
    mill.CustomPremiumPerTon = customPremium;
    await db.SaveChangesAsync();
    return Results.Ok(mill);
});

// 10. 获取物流调运路线费率库 (从数据库持久化层获取)
app.MapGet("/api/pricing/routes", async (AppDbContext db) =>
{
    var routes = await db.FreightRoutes.OrderBy(r => r.Id).ToListAsync();
    return Results.Ok(routes);
});

// 10.1 更新物流路线调运费
app.MapPut("/api/pricing/routes/{routeId}", async (string routeId, double? customFreight, AppDbContext db) =>
{
    var route = await db.FreightRoutes.FirstOrDefaultAsync(r => r.RouteId == routeId);
    if (route == null) return Results.NotFound();
    route.CustomFreightPerTon = customFreight;
    await db.SaveChangesAsync();
    return Results.Ok(route);
});

// 10.2 获取板厚加价阶梯表
app.MapGet("/api/pricing/rules/thickness-ladders", async (AppDbContext db) =>
{
    var ladders = await db.PlateThicknessLadders.OrderBy(l => l.MinThicknessMm).ToListAsync();
    return Results.Ok(ladders);
});

// 10.3 批量保存板厚加价阶梯
app.MapPut("/api/pricing/rules/thickness-ladders", async (List<PlateThicknessLadderEntity> input, AppDbContext db) =>
{
    foreach (var item in input)
    {
        var existing = await db.PlateThicknessLadders.FindAsync(item.Id);
        if (existing != null)
        {
            existing.CustomSurcharge = item.CustomSurcharge;
            existing.Description = item.Description;
        }
    }
    await db.SaveChangesAsync();
    return Results.Ok(await db.PlateThicknessLadders.OrderBy(l => l.MinThicknessMm).ToListAsync());
});

// 10.4 获取定尺与尺寸加价规则表
app.MapGet("/api/pricing/rules/dimension-rules", async (AppDbContext db) =>
{
    var rules = await db.PlateDimensionRules.OrderBy(r => r.Id).ToListAsync();
    return Results.Ok(rules);
});

// 10.5 批量保存定尺与尺寸加价规则
app.MapPut("/api/pricing/rules/dimension-rules", async (List<PlateDimensionRuleEntity> input, AppDbContext db) =>
{
    foreach (var item in input)
    {
        var existing = await db.PlateDimensionRules.FindAsync(item.Id);
        if (existing != null)
        {
            existing.CustomSurcharge = item.CustomSurcharge;
            existing.Name = item.Name;
        }
    }
    await db.SaveChangesAsync();
    return Results.Ok(await db.PlateDimensionRules.OrderBy(r => r.Id).ToListAsync());
});

// 10.6 恢复所有加价规则与运费为行业出厂基准
app.MapPost("/api/pricing/rules/reset-defaults", async (AppDbContext db) =>
{
    // 重置厚度阶梯
    var ladders = await db.PlateThicknessLadders.ToListAsync();
    foreach (var l in ladders) l.CustomSurcharge = null;

    // 重置尺寸规则
    var dimRules = await db.PlateDimensionRules.ToListAsync();
    foreach (var d in dimRules) d.CustomSurcharge = null;

    // 重置钢厂溢价
    var mills = await db.SteelMills.ToListAsync();
    foreach (var m in mills) m.CustomPremiumPerTon = null;

    // 重置路线运费
    var routes = await db.FreightRoutes.ToListAsync();
    foreach (var r in routes) r.CustomFreightPerTon = null;

    await db.SaveChangesAsync();
    return Results.Ok(new { Message = "All pricing rules and logistics routes have been reset to benchmark defaults." });
});

// 11. 钢板多维加价核算接口
app.MapPost("/api/pricing/calculate-plate", (EngineeringApp.Shared.Material.Pricing.PlatePricingParameters input, string? snapshotId) =>
{
    var snapshot = string.IsNullOrEmpty(snapshotId)
        ? EngineeringApp.Shared.Material.Pricing.SeedPriceSnapshots.Latest
        : EngineeringApp.Shared.Material.Pricing.SeedPriceSnapshots.GetById(snapshotId);

    var result = EngineeringApp.Shared.Material.Pricing.PlatePricingRuleEngine.Calculate(input, snapshot);
    return Results.Ok(result);
});

// 12. 型材商业价格与工艺核算接口
app.MapPost("/api/pricing/calculate-profile", (EngineeringApp.Shared.Material.Pricing.ProfilePricingParameters input, string? snapshotId) =>
{
    var snapshot = string.IsNullOrEmpty(snapshotId)
        ? EngineeringApp.Shared.Material.Pricing.SeedPriceSnapshots.Latest
        : EngineeringApp.Shared.Material.Pricing.SeedPriceSnapshots.GetById(snapshotId);

    var result = EngineeringApp.Shared.Material.Pricing.ProfilePricingRuleEngine.Calculate(input, snapshot);
    return Results.Ok(result);
});

app.Run();
