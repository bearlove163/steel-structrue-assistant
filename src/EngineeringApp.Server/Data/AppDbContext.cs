using Microsoft.EntityFrameworkCore;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Server.Data;

/// <summary>
/// 基础数据实体：型钢数据库持久化模型
/// </summary>
public class StandardSteelEntity
{
    public int Id { get; set; }
    public string StandardSystem { get; set; } = "";
    public string Category { get; set; } = "";
    public string Designation { get; set; } = "";
    public SectionType SectionType { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public double WebThickness { get; set; }
    public double FlangeThickness { get; set; }
    public double RootRadius { get; set; }
    public double StandardAreaCm2 { get; set; }
    public double StandardMassKgM { get; set; }
    public double StandardIxCm4 { get; set; }
    public double StandardIyCm4 { get; set; }
    public double StandardWxCm3 { get; set; }
    public double StandardWyCm3 { get; set; }
}

/// <summary>
/// 基础数据实体：油漆涂料产品与单价持久化模型
/// </summary>
public class PaintProductEntity
{
    public int Id { get; set; }
    public CoatingLayerType LayerType { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Description { get; set; } = "";
    public double VolumeSolidsPercent { get; set; }
    public double DensityKgL { get; set; }
    public double DefaultDft { get; set; }
    public double LossRatePercent { get; set; }
    public double MaterialUnitPricePerKg { get; set; }
    public double LaborUnitPricePerM2 { get; set; }
}

/// <summary>
/// 基础数据实体：材料价格基准快照持久化模型
/// </summary>
public class MaterialPriceSnapshotEntity
{
    public int Id { get; set; }
    public string SnapshotId { get; set; } = "";
    public string SnapshotName { get; set; } = "";
    public DateTime EffectiveDate { get; set; }
    public PriceSourceType SourceType { get; set; }
    public string SourceSupplier { get; set; } = "";
    public bool IsTaxInclusive { get; set; } = true;
    public DeliveryCondition DefaultDelivery { get; set; }
    public double PlateBaseQ235B { get; set; }
    public double PlateBaseQ355B { get; set; }
    public double HBeamBaseQ235B { get; set; }
    public double HBeamBaseQ355B { get; set; }
    public double HotRolledCoilBase { get; set; }
    public double WeldedTubeBase { get; set; }
    public double SeamlessTubeBase { get; set; }
    public bool IsLocked { get; set; }
    public string Remarks { get; set; } = "";
}

/// <summary>
/// 基础数据实体：板厚加价阶梯持久化模型
/// </summary>
public class PlateThicknessLadderEntity
{
    public int Id { get; set; }
    public double MinThicknessMm { get; set; }
    public double MaxThicknessMm { get; set; }
    public double BenchmarkSurcharge { get; set; }
    public double? CustomSurcharge { get; set; }
    public string Description { get; set; } = "";
}

/// <summary>
/// 基础数据实体：板宽与定尺尺寸加价规则持久化模型
/// </summary>
public class PlateDimensionRuleEntity
{
    public int Id { get; set; }
    public string RuleCode { get; set; } = "";
    public string Name { get; set; } = "";
    public double BenchmarkSurcharge { get; set; }
    public double? CustomSurcharge { get; set; }
    public string ConditionDescription { get; set; } = "";
}

/// <summary>
/// 基础数据实体：钢厂品牌与基准溢价持久化模型
/// </summary>
public class SteelMillEntity
{
    public int Id { get; set; }
    public string MillId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string ProductionBase { get; set; } = "";
    public int Tier { get; set; } = 1;
    public double BenchmarkPremiumPerTon { get; set; }
    public double? CustomPremiumPerTon { get; set; }
    public string SpecialtyDescription { get; set; } = "";
    public bool SupportsThirdPartyCertification { get; set; } = true;
    public bool IsRecommended { get; set; }
}

/// <summary>
/// 基础数据实体：特定价格快照下钢厂时序溢价持久化模型
/// </summary>
public class SteelMillSnapshotPremiumEntity
{
    public int Id { get; set; }
    public string SnapshotId { get; set; } = "";
    public string MillId { get; set; } = "";
    public double PremiumPerTon { get; set; }
    public string Remarks { get; set; } = "";
}

/// <summary>
/// 基础数据实体：物流调运路线费率持久化模型
/// </summary>
public class FreightRouteEntity
{
    public int Id { get; set; }
    public string RouteId { get; set; } = "";
    public string OriginMillBase { get; set; } = "";
    public string DestinationRegion { get; set; } = "";
    public double BenchmarkFreightPerTon { get; set; }
    public double? CustomFreightPerTon { get; set; }
    public string TransportMode { get; set; } = "";
    public string Description { get; set; } = "";
}

/// <summary>
/// 基础数据实体：物料出处记录持久化模型
/// </summary>
public class MaterialProvenanceRecordEntity
{
    public int Id { get; set; }
    public string MaterialTag { get; set; } = "";
    public string DisplayTitle { get; set; } = "";
    public string Category { get; set; } = "Plate";
    public string StandardSpecification { get; set; } = "";
    public string DimensionText { get; set; } = "";
    public string SnapshotId { get; set; } = "";
    public string SnapshotName { get; set; } = "";
    public DateTime EffectiveDate { get; set; }
    public DateTime RecordedAt { get; set; }
    public string ProjectReference { get; set; } = "";
    public string MillName { get; set; } = "";
    public string DeliveryAndFreightText { get; set; } = "";
    public double BasePricePerTon { get; set; }
    public double MillPremiumPerTon { get; set; }
    public double ThicknessSurchargePerTon { get; set; }
    public double DimensionSurchargePerTon { get; set; }
    public double ToleranceSurchargePerTon { get; set; }
    public double PerformanceSurchargePerTon { get; set; }
    public double InspectionSurchargePerTon { get; set; }
    public double FreightPerTon { get; set; }
    public double FinalPricePerTon { get; set; }
    public string FullDescription { get; set; } = "";
    public string PlateParametersJson { get; set; } = "";

    public MaterialProvenanceRecord ToDomain()
    {
        PlatePricingParameters p = new();
        if (!string.IsNullOrEmpty(PlateParametersJson))
        {
            try
            {
                p = System.Text.Json.JsonSerializer.Deserialize<PlatePricingParameters>(PlateParametersJson) ?? new();
            }
            catch { }
        }

        return new MaterialProvenanceRecord
        {
            Id = Id,
            MaterialTag = MaterialTag,
            DisplayTitle = DisplayTitle,
            Category = Category,
            StandardSpecification = StandardSpecification,
            DimensionText = DimensionText,
            SnapshotId = SnapshotId,
            SnapshotName = SnapshotName,
            EffectiveDate = EffectiveDate,
            RecordedAt = RecordedAt,
            ProjectReference = ProjectReference,
            MillName = MillName,
            DeliveryAndFreightText = DeliveryAndFreightText,
            BasePricePerTon = BasePricePerTon,
            MillPremiumPerTon = MillPremiumPerTon,
            ThicknessSurchargePerTon = ThicknessSurchargePerTon,
            DimensionSurchargePerTon = DimensionSurchargePerTon,
            ToleranceSurchargePerTon = ToleranceSurchargePerTon,
            PerformanceSurchargePerTon = PerformanceSurchargePerTon,
            InspectionSurchargePerTon = InspectionSurchargePerTon,
            FreightPerTon = FreightPerTon,
            FinalPricePerTon = FinalPricePerTon,
            FullDescription = FullDescription,
            PlateParameters = p
        };
    }

    public static MaterialProvenanceRecordEntity FromDomain(MaterialProvenanceRecord r) => new()
    {
        Id = r.Id,
        MaterialTag = r.MaterialTag,
        DisplayTitle = r.DisplayTitle,
        Category = r.Category,
        StandardSpecification = r.StandardSpecification,
        DimensionText = r.DimensionText,
        SnapshotId = r.SnapshotId,
        SnapshotName = r.SnapshotName,
        EffectiveDate = r.EffectiveDate,
        RecordedAt = r.RecordedAt == default ? DateTime.Now : r.RecordedAt,
        ProjectReference = r.ProjectReference,
        MillName = r.MillName,
        DeliveryAndFreightText = r.DeliveryAndFreightText,
        BasePricePerTon = r.BasePricePerTon,
        MillPremiumPerTon = r.MillPremiumPerTon,
        ThicknessSurchargePerTon = r.ThicknessSurchargePerTon,
        DimensionSurchargePerTon = r.DimensionSurchargePerTon,
        ToleranceSurchargePerTon = r.ToleranceSurchargePerTon,
        PerformanceSurchargePerTon = r.PerformanceSurchargePerTon,
        InspectionSurchargePerTon = r.InspectionSurchargePerTon,
        FreightPerTon = r.FreightPerTon,
        FinalPricePerTon = r.FinalPricePerTon,
        FullDescription = r.FullDescription,
        PlateParametersJson = System.Text.Json.JsonSerializer.Serialize(r.PlateParameters)
    };
}

/// <summary>
/// SQLite 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<StandardSteelEntity> StandardSteels => Set<StandardSteelEntity>();
    public DbSet<PaintProductEntity> PaintProducts => Set<PaintProductEntity>();
    public DbSet<MaterialPriceSnapshotEntity> MaterialPriceSnapshots => Set<MaterialPriceSnapshotEntity>();
    public DbSet<PlateThicknessLadderEntity> PlateThicknessLadders => Set<PlateThicknessLadderEntity>();
    public DbSet<PlateDimensionRuleEntity> PlateDimensionRules => Set<PlateDimensionRuleEntity>();
    public DbSet<SteelMillEntity> SteelMills => Set<SteelMillEntity>();
    public DbSet<SteelMillSnapshotPremiumEntity> SteelMillSnapshotPremiums => Set<SteelMillSnapshotPremiumEntity>();
    public DbSet<FreightRouteEntity> FreightRoutes => Set<FreightRouteEntity>();
    public DbSet<MaterialProvenanceRecordEntity> MaterialProvenanceRecords => Set<MaterialProvenanceRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StandardSteelEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Designation);
            e.HasIndex(x => x.StandardSystem);
            e.HasIndex(x => x.Category);
        });

        modelBuilder.Entity<PaintProductEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.LayerType);
        });

        modelBuilder.Entity<MaterialPriceSnapshotEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SnapshotId).IsUnique();
            e.HasIndex(x => x.EffectiveDate);
        });

        modelBuilder.Entity<PlateThicknessLadderEntity>(e =>
        {
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<PlateDimensionRuleEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RuleCode).IsUnique();
        });

        modelBuilder.Entity<SteelMillEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MillId).IsUnique();
        });

        modelBuilder.Entity<SteelMillSnapshotPremiumEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SnapshotId, x.MillId });
        });

        modelBuilder.Entity<FreightRouteEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RouteId);
        });

        modelBuilder.Entity<MaterialProvenanceRecordEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MaterialTag);
            e.HasIndex(x => x.SnapshotId);
        });
    }

    /// <summary>
    /// 初始化并自动填充基准数据 (型钢、油漆、材料快照、厚度阶梯、尺寸规则、钢厂溢价、物流路线)
    /// </summary>
    public static void SeedData(AppDbContext db)
    {
        db.Database.EnsureCreated();

        // 1. 填充油漆基准价格数据库
        if (!db.PaintProducts.Any())
        {
            var seedPaints = PaintProductDatabase.AllProducts.Select(p => new PaintProductEntity
            {
                Id = p.Id,
                LayerType = p.LayerType,
                Name = p.Name,
                Brand = p.Brand,
                Description = p.Description,
                VolumeSolidsPercent = p.VolumeSolidsPercent,
                DensityKgL = p.DensityKgL,
                DefaultDft = p.DefaultDft,
                LossRatePercent = p.LossRatePercent,
                MaterialUnitPricePerKg = p.MaterialUnitPricePerKg,
                LaborUnitPricePerM2 = p.LaborUnitPricePerM2
            }).ToList();

            db.PaintProducts.AddRange(seedPaints);
            db.SaveChanges();
        }

        // 2. 填充全球标准型钢库 (国标/欧标/美标)
        if (!db.StandardSteels.Any())
        {
            int id = 1;
            var seedSteels = StructuralSteelLibrary.AllSections.Select(s => new StandardSteelEntity
            {
                Id = id++,
                StandardSystem = s.StandardSystem,
                Category = s.Category,
                Designation = s.Designation,
                SectionType = s.SectionType,
                Height = s.Height,
                Width = s.Width,
                WebThickness = s.WebThickness,
                FlangeThickness = s.FlangeThickness,
                RootRadius = s.RootRadius,
                StandardAreaCm2 = s.StandardAreaCm2,
                StandardMassKgM = s.StandardMassKgM,
                StandardIxCm4 = s.StandardIxCm4,
                StandardIyCm4 = s.StandardIyCm4,
                StandardWxCm3 = s.StandardWxCm3,
                StandardWyCm3 = s.StandardWyCm3
            }).ToList();

            db.StandardSteels.AddRange(seedSteels);
            db.SaveChanges();
        }

        // 3. 填充材料价格时间快照数据
        if (!db.MaterialPriceSnapshots.Any())
        {
            int sId = 1;
            var seedSnapshots = SeedPriceSnapshots.AllSnapshots.Select(s => new MaterialPriceSnapshotEntity
            {
                Id = sId++,
                SnapshotId = s.SnapshotId,
                SnapshotName = s.SnapshotName,
                EffectiveDate = s.EffectiveDate,
                SourceType = s.SourceType,
                SourceSupplier = s.SourceSupplier,
                IsTaxInclusive = s.IsTaxInclusive,
                DefaultDelivery = s.DefaultDelivery,
                PlateBaseQ235B = s.PlateBaseQ235B,
                PlateBaseQ355B = s.PlateBaseQ355B,
                HBeamBaseQ235B = s.HBeamBaseQ235B,
                HBeamBaseQ355B = s.HBeamBaseQ355B,
                HotRolledCoilBase = s.HotRolledCoilBase,
                WeldedTubeBase = s.WeldedTubeBase,
                SeamlessTubeBase = s.SeamlessTubeBase,
                IsLocked = s.IsLocked,
                Remarks = s.Remarks
            }).ToList();

            db.MaterialPriceSnapshots.AddRange(seedSnapshots);
            db.SaveChanges();
        }

        // 4. 填充板厚加价阶梯基准数据
        if (!db.PlateThicknessLadders.Any())
        {
            var seedLadders = PlateThicknessLadder.GetDefaultLadders().Select(l => new PlateThicknessLadderEntity
            {
                Id = l.Id,
                MinThicknessMm = l.MinThicknessMm,
                MaxThicknessMm = l.MaxThicknessMm,
                BenchmarkSurcharge = l.BenchmarkSurcharge,
                CustomSurcharge = l.CustomSurcharge,
                Description = l.Description
            }).ToList();

            db.PlateThicknessLadders.AddRange(seedLadders);
            db.SaveChanges();
        }

        // 5. 填充板宽与定尺尺寸加价规则
        if (!db.PlateDimensionRules.Any())
        {
            var seedDimRules = PlateDimensionRule.GetDefaultRules().Select(r => new PlateDimensionRuleEntity
            {
                Id = r.Id,
                RuleCode = r.RuleCode,
                Name = r.Name,
                BenchmarkSurcharge = r.BenchmarkSurcharge,
                CustomSurcharge = r.CustomSurcharge,
                ConditionDescription = r.ConditionDescription
            }).ToList();

            db.PlateDimensionRules.AddRange(seedDimRules);
            db.SaveChanges();
        }

        // 6. 填充钢厂品牌与溢价
        if (!db.SteelMills.Any())
        {
            int mId = 1;
            var seedMills = SeedSteelMills.AllMills.Select(m => new SteelMillEntity
            {
                Id = mId++,
                MillId = m.Id,
                Name = m.Name,
                ShortName = m.ShortName,
                ProductionBase = m.ProductionBase,
                Tier = m.Tier,
                BenchmarkPremiumPerTon = m.BrandPremiumPerTon,
                CustomPremiumPerTon = null,
                SpecialtyDescription = m.SpecialtyDescription,
                SupportsThirdPartyCertification = m.SupportsThirdPartyCertification,
                IsRecommended = m.IsRecommended
            }).ToList();

            db.SteelMills.AddRange(seedMills);
            db.SaveChanges();
        }

        // 7. 填充物流调运路线与费率
        if (!db.FreightRoutes.Any())
        {
            int rId = 1;
            var seedRoutes = SeedFreightRoutes.AllRoutes.Select(r => new FreightRouteEntity
            {
                Id = rId++,
                RouteId = r.RouteId,
                OriginMillBase = r.OriginMillBase,
                DestinationRegion = r.DestinationRegion,
                BenchmarkFreightPerTon = r.EstimatedFreightPerTon,
                CustomFreightPerTon = null,
                TransportMode = r.TransportMode,
                Description = r.Description
            }).ToList();

            db.FreightRoutes.AddRange(seedRoutes);
            db.SaveChanges();
        }

        // 8. 填充物料出处记录库 (Seed Provenance Records)
        if (!db.MaterialProvenanceRecords.Any())
        {
            int recId = 1;
            var seedRecords = SeedProvenanceRecords.AllRecords.Select(r => new MaterialProvenanceRecordEntity
            {
                Id = recId++,
                MaterialTag = r.MaterialTag,
                DisplayTitle = r.DisplayTitle,
                Category = r.Category,
                StandardSpecification = r.StandardSpecification,
                DimensionText = r.DimensionText,
                SnapshotId = r.SnapshotId,
                SnapshotName = r.SnapshotName,
                EffectiveDate = r.EffectiveDate,
                RecordedAt = r.RecordedAt,
                ProjectReference = r.ProjectReference,
                MillName = r.MillName,
                DeliveryAndFreightText = r.DeliveryAndFreightText,
                BasePricePerTon = r.BasePricePerTon,
                MillPremiumPerTon = r.MillPremiumPerTon,
                ThicknessSurchargePerTon = r.ThicknessSurchargePerTon,
                DimensionSurchargePerTon = r.DimensionSurchargePerTon,
                ToleranceSurchargePerTon = r.ToleranceSurchargePerTon,
                PerformanceSurchargePerTon = r.PerformanceSurchargePerTon,
                InspectionSurchargePerTon = r.InspectionSurchargePerTon,
                FreightPerTon = r.FreightPerTon,
                FinalPricePerTon = r.FinalPricePerTon,
                FullDescription = r.FullDescription,
                PlateParametersJson = System.Text.Json.JsonSerializer.Serialize(r.PlateParameters)
            }).ToList();

            db.MaterialProvenanceRecords.AddRange(seedRecords);
            db.SaveChanges();
        }
    }
}
