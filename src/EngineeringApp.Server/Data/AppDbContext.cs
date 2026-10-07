using Microsoft.EntityFrameworkCore;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Data;

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
/// SQLite 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<StandardSteelEntity> StandardSteels => Set<StandardSteelEntity>();
    public DbSet<PaintProductEntity> PaintProducts => Set<PaintProductEntity>();

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
    }

    /// <summary>
    /// 初始化并自动填充基准数据 (欧标、美标、国标型钢及油漆价格数据)
    /// </summary>
    public static void SeedData(AppDbContext db)
    {
        db.Database.EnsureCreated();

        // 填充油漆基准价格数据库
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

        // 填充全球标准型钢库 (国标/欧标/美标)
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
    }
}
