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
    var supplierCount = await db.Suppliers.CountAsync();
    return Results.Ok(new
    {
        Status = "Online",
        Database = "SQLite (engineering_workstation.db)",
        StandardSteelsCount = steelCount,
        PaintProductsCount = paintCount,
        SuppliersCount = supplierCount,
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

// 13. 获取工程物料出处记录库列表
app.MapGet("/api/pricing/provenance-records", async (AppDbContext db) =>
{
    var entities = await db.MaterialProvenanceRecords
        .OrderByDescending(r => r.RecordedAt)
        .ToListAsync();
    var records = entities.Select(e => e.ToDomain()).ToList();
    return Results.Ok(records);
});

// 14. 归档新增工程物料出处记录 (支持时间标签与特征指纹)
app.MapPost("/api/pricing/provenance-records", async (EngineeringApp.Shared.Material.Pricing.MaterialProvenanceRecord input, AppDbContext db) =>
{
    if (string.IsNullOrEmpty(input.MaterialTag) && input.PlateParameters != null)
    {
        input.MaterialTag = EngineeringApp.Shared.Material.Pricing.MaterialProvenanceTagBuilder.GeneratePlateTag(input.PlateParameters);
    }
    if (input.RecordedAt == default)
    {
        input.RecordedAt = DateTime.Now;
    }

    var entity = MaterialProvenanceRecordEntity.FromDomain(input);
    db.MaterialProvenanceRecords.Add(entity);
    await db.SaveChangesAsync();

    var saved = entity.ToDomain();
    EngineeringApp.Shared.Material.Pricing.SeedProvenanceRecords.AddRecord(saved);
    return Results.Created($"/api/pricing/provenance-records/{saved.Id}", saved);
});

// 14.1 更新/修订物料出处记录 (写入变更履历与新版本)
app.MapPut("/api/pricing/provenance-records/{id:int}", async (int id, EngineeringApp.Shared.Material.Pricing.MaterialProvenanceRecord input, AppDbContext db) =>
{
    var existing = await db.MaterialProvenanceRecords.FindAsync(id);
    if (existing == null) return Results.NotFound();

    existing.DisplayTitle = input.DisplayTitle;
    existing.ProjectReference = input.ProjectReference;
    existing.InquiryBatchId = input.InquiryBatchId;
    existing.InquiryDate = input.InquiryDate;
    existing.InquiryVendor = input.InquiryVendor;
    existing.RevisionNumber = input.RevisionNumber;
    existing.PricingPrincipleNote = input.PricingPrincipleNote;
    existing.BasePricePerTon = input.BasePricePerTon;
    existing.MillPremiumPerTon = input.MillPremiumPerTon;
    existing.ThicknessSurchargePerTon = input.ThicknessSurchargePerTon;
    existing.ContractThicknessSurchargePerTon = input.ContractThicknessSurchargePerTon;
    existing.DimensionSurchargePerTon = input.DimensionSurchargePerTon;
    existing.ToleranceSurchargePerTon = input.ToleranceSurchargePerTon;
    existing.PerformanceSurchargePerTon = input.PerformanceSurchargePerTon;
    existing.InspectionSurchargePerTon = input.InspectionSurchargePerTon;
    existing.FreightPerTon = input.FreightPerTon;
    if (existing.InitialRecordedPricePerTon <= 0)
    {
        existing.InitialRecordedPricePerTon = input.ContractBaselinePrice;
    }
    else if (input.InitialRecordedPricePerTon > 0)
    {
        existing.InitialRecordedPricePerTon = input.InitialRecordedPricePerTon;
    }
    existing.FinalPricePerTon = input.FinalPricePerTon;
    existing.FullDescription = input.FullDescription;
    existing.RevisionLogsJson = System.Text.Json.JsonSerializer.Serialize(input.RevisionLogs ?? []);
    existing.PlateParametersJson = System.Text.Json.JsonSerializer.Serialize(input.PlateParameters);

    await db.SaveChangesAsync();
    return Results.Ok(existing.ToDomain());
});

// 15. 删除指定物料出处记录
app.MapDelete("/api/pricing/provenance-records/{id:int}", async (int id, AppDbContext db) =>
{
    var item = await db.MaterialProvenanceRecords.FindAsync(id);
    if (item == null) return Results.NotFound();

    db.MaterialProvenanceRecords.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = $"Material provenance record {id} removed." });
});

// ======================== 供应商名录与资质库 API ========================

// 16. 获取供应商列表 (支持分类筛选与模糊关键字搜索)
app.MapGet("/api/database/suppliers", async (SupplierCategory? category, string? keyword, AppDbContext db) =>
{
    var query = db.Suppliers.AsQueryable();

    if (category.HasValue && (int)category.Value > 0)
    {
        query = query.Where(s => s.Category == category.Value);
    }

    if (!string.IsNullOrWhiteSpace(keyword))
    {
        var kw = keyword.Trim().ToLower();
        query = query.Where(s =>
            s.Name.ToLower().Contains(kw) ||
            s.ShortName.ToLower().Contains(kw) ||
            s.Code.ToLower().Contains(kw) ||
            s.SubCategory.ToLower().Contains(kw) ||
            s.ContactPerson.ToLower().Contains(kw) ||
            s.CityRegion.ToLower().Contains(kw));
    }

    var list = await query
        .OrderBy(s => s.Category)
        .ThenBy(s => s.Id)
        .Select(s => s.ToDomain())
        .ToListAsync();

    return Results.Ok(list);
});

// 17. 获取单个供应商详情
app.MapGet("/api/database/suppliers/{id:int}", async (int id, AppDbContext db) =>
{
    var supplier = await db.Suppliers.FindAsync(id);
    if (supplier == null) return Results.NotFound();

    return Results.Ok(supplier.ToDomain());
});

// 18. 新增供应商档案
app.MapPost("/api/database/suppliers", async (SupplierItem input, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(input.Name))
    {
        return Results.BadRequest(new { Message = "供应商法定名称不能为空" });
    }

    if (string.IsNullOrWhiteSpace(input.Code))
    {
        var count = await db.Suppliers.CountAsync(s => s.Category == input.Category) + 1;
        var prefix = input.Category switch
        {
            SupplierCategory.SteelMill => "SUP-MILL",
            SupplierCategory.SteelTrader => "SUP-TRD",
            SupplierCategory.CraneRental => "SUP-CRN",
            SupplierCategory.FastenerBolt => "SUP-BLT",
            SupplierCategory.SpecialtyMaterial => "SUP-MAT",
            SupplierCategory.PaintCoating => "SUP-PNT",
            _ => "SUP-CUS"
        };
        input.Code = $"{prefix}-{count:D3}";
    }

    var entity = SupplierEntity.FromDomain(input);
    entity.Id = 0; // 自增
    entity.CreatedAt = DateTime.Now;
    entity.UpdatedAt = DateTime.Now;

    db.Suppliers.Add(entity);
    await db.SaveChangesAsync();

    return Results.Created($"/api/database/suppliers/{entity.Id}", entity.ToDomain());
});

// 19. 更新供应商档案
app.MapPut("/api/database/suppliers/{id:int}", async (int id, SupplierItem input, AppDbContext db) =>
{
    var entity = await db.Suppliers.FindAsync(id);
    if (entity == null) return Results.NotFound();

    entity.Name = input.Name;
    entity.ShortName = input.ShortName;
    entity.Category = input.Category;
    entity.SubCategory = input.SubCategory;
    entity.ContactPerson = input.ContactPerson;
    entity.ContactPhone = input.ContactPhone;
    entity.Email = input.Email;
    entity.CityRegion = input.CityRegion;
    entity.CooperationStatus = input.CooperationStatus;
    entity.Remarks = input.Remarks;
    entity.UpdatedAt = DateTime.Now;

    await db.SaveChangesAsync();
    return Results.Ok(entity.ToDomain());
});

// 20. 删除供应商档案 (带外键引用保护检查)
app.MapDelete("/api/database/suppliers/{id:int}", async (int id, AppDbContext db) =>
{
    var entity = await db.Suppliers.FindAsync(id);
    if (entity == null) return Results.NotFound();

    // 检查是否有物料出处记录外键强关联引用
    var isReferencedInProvenance = await db.MaterialProvenanceRecords.AnyAsync(r => r.SupplierId == id);
    var isReferencedInPaint = await db.PaintProducts.AnyAsync(p => p.SupplierId == id);

    if (isReferencedInProvenance || isReferencedInPaint)
    {
        return Results.BadRequest(new
        {
            Message = $"无法删除供应商【{entity.Name}】(ID: {id})：该供应商已被物料询价出处或涂装产品名录引用，请先解除业务关联或将其合作状态设为【暂停合作】。"
        });
    }

    db.Suppliers.Remove(entity);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = $"供应商【{entity.Name}】已成功移除。" });
});

// 21. 重置为行业基准供应商库预设
app.MapPost("/api/database/suppliers/reset", async (AppDbContext db) =>
{
    var existingCodes = await db.Suppliers.Select(s => s.Code).ToListAsync();
    var seedSuppliers = EngineeringApp.Shared.Data.SupplierDatabase.AllSuppliers;

    foreach (var seed in seedSuppliers)
    {
        if (!existingCodes.Contains(seed.Code))
        {
            db.Suppliers.Add(SupplierEntity.FromDomain(seed));
        }
    }

    await db.SaveChangesAsync();
    var total = await db.Suppliers.CountAsync();
    return Results.Ok(new { Message = "供应商标杆数据已同步对齐重置完成。", TotalCount = total });
});

app.Run();
