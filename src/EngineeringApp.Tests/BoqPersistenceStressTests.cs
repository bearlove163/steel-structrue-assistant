using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using EngineeringApp.Server.Data;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests;

/// <summary>
/// 对 Server 端 EF Core SQLite 持久化层 (AppDbContext) 进行敌手化压力测试 (Empirical Challenger Suite)
/// 重点验证：SQLite 实机建库、4 级级联删除、外键锁定限制 (DeleteBehavior.Restrict)、唯一约束与非唯一索引、元数据 NotMapped 检查与大批量深层树汇聚
/// </summary>
public class BoqPersistenceStressTests : IDisposable
{
    private readonly string _dbPath;
    private readonly SqliteConnection _sqliteConnection;
    private readonly DbContextOptions<AppDbContext> _dbContextOptions;

    public BoqPersistenceStressTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"boq_stress_{Guid.NewGuid():N}.db");
        // 使用实际 SQLite 物理连接并启用外键约束 PRAGMA foreign_keys = ON;
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = _dbPath,
            ForeignKeys = true,
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        _sqliteConnection = new SqliteConnection(connectionString);
        _sqliteConnection.Open();

        _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_sqliteConnection)
            .EnableSensitiveDataLogging()
            .Options;
    }

    public void Dispose()
    {
        _sqliteConnection.Close();
        _sqliteConnection.Dispose();
        if (File.Exists(_dbPath))
        {
            try { File.Delete(_dbPath); } catch { }
        }
    }

    private AppDbContext CreateContext() => new(_dbContextOptions);

    [Fact]
    public void Stress01_DbContext_SQLite_EnsureCreated_And_BOQ_Tables_Succeeds()
    {
        using var db = CreateContext();

        // 验证 EnsureCreated 正常执行且建表无冲突
        bool created = db.Database.EnsureCreated();
        Assert.True(created);

        // 验证 6 大 BOQ 数据集均可执行探测
        Assert.Empty(db.BoqProjects.ToList());
        Assert.Empty(db.BoqTenderItems.ToList());
        Assert.Empty(db.BoqSubProjects.ToList());
        Assert.Empty(db.BoqParts.ToList());
        Assert.Empty(db.FabricationQuotaRules.ToList());
        Assert.Empty(db.InstallationQuotaRules.ToList());
    }

    [Fact]
    public void Stress01_B_SeedData_Executes_Cleanly_Without_Exceptions()
    {
        using var db = CreateContext();
        db.Database.EnsureCreated();

        // 验证 SeedData 在干净数据库上顺利执行，且供应商、定额数据与宁波奥体项目均已正确初始化
        AppDbContext.SeedData(db);

        Assert.True(db.Suppliers.Any(), "供应商表应当已成功填充");
        Assert.True(db.FabricationQuotaRules.Any(), "车间制作定额应当已成功填充");
        Assert.True(db.InstallationQuotaRules.Any(), "现场安装定额应当已成功填充");
        Assert.True(db.BoqProjects.Any(p => p.ProjectCode == "NB-ATZX"), "宁波奥体中心项目应当已成功填充");

        // 验证二次调用幂等性
        AppDbContext.SeedData(db);
        Assert.Equal(1, db.BoqProjects.Count(p => p.ProjectCode == "NB-ATZX"));
    }

    [Fact]
    public void Stress02_CascadeDelete_FullHierarchy_RootProjectDelete_WipesAllDescendants()
    {
        using (var db = CreateContext())
        {
            db.Database.EnsureCreated();

            var project = new BoqProject
            {
                ProjectCode = "NB-TEST-ROOT",
                ProjectName = "宁波测试项目-全级联验证",
                ClientName = "宁波奥体",
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var tender1 = new BoqTenderItem
            {
                ItemTag = "NH",
                BillItemCode = "010101001",
                BillItemName = "南主桁架",
                Unit = "t",
                TenderQuantity = 500
            };
            var tender2 = new BoqTenderItem
            {
                ItemTag = "NF",
                BillItemCode = "010101002",
                BillItemName = "南副桁架",
                Unit = "t",
                TenderQuantity = 300
            };

            var sub1 = new BoqSubProject { SubProjectCode = "SUB-01", SubProjectName = "主桁架分段1", Quantity = 2 };
            var sub2 = new BoqSubProject { SubProjectCode = "SUB-02", SubProjectName = "主桁架分段2", Quantity = 2 };
            var sub3 = new BoqSubProject { SubProjectCode = "SUB-03", SubProjectName = "副桁架分段1", Quantity = 1 };

            var part1 = new BoqPart { PartMark = "P-01", SectionSpecification = "PL50*2000", SteelGrade = "Q460GJC", Quantity = 4 };
            var part2 = new BoqPart { PartMark = "P-02", SectionSpecification = "PL60*2000", SteelGrade = "Q460GJC", Quantity = 4 };
            var part3 = new BoqPart { PartMark = "P-03", SectionSpecification = "PIP1200*50", SteelGrade = "Q460GJC", Quantity = 2 };
            var part4 = new BoqPart { PartMark = "P-04", SectionSpecification = "PIP500*20", SteelGrade = "Q355B", Quantity = 6 };

            sub1.Parts.Add(part1);
            sub1.Parts.Add(part2);
            sub2.Parts.Add(part3);
            sub3.Parts.Add(part4);

            tender1.SubProjects.Add(sub1);
            tender1.SubProjects.Add(sub2);
            tender2.SubProjects.Add(sub3);

            project.TenderItems.Add(tender1);
            project.TenderItems.Add(tender2);

            db.BoqProjects.Add(project);
            db.SaveChanges();

            Assert.True(project.Id > 0);
            Assert.Equal(1, db.BoqProjects.Count());
            Assert.Equal(2, db.BoqTenderItems.Count());
            Assert.Equal(3, db.BoqSubProjects.Count());
            Assert.Equal(4, db.BoqParts.Count());
        }

        // 在独立全新上下文实例中删除根项目 BoqProject，验证 SQLite 外键与 EF Core 映射的真实级联
        using (var dbDelete = CreateContext())
        {
            var project = dbDelete.BoqProjects.Single(p => p.ProjectCode == "NB-TEST-ROOT");
            dbDelete.BoqProjects.Remove(project);
            dbDelete.SaveChanges();
        }

        // 验证全部 4 级后代已被 SQLite 数据库彻底级联清空
        using (var dbVerify = CreateContext())
        {
            Assert.Equal(0, dbVerify.BoqProjects.Count());
            Assert.Equal(0, dbVerify.BoqTenderItems.Count());
            Assert.Equal(0, dbVerify.BoqSubProjects.Count());
            Assert.Equal(0, dbVerify.BoqParts.Count());
        }
    }

    [Fact]
    public void Stress03_CascadeDelete_IntermediateTenderItem_DeletesOnlyItsSubProjectsAndParts()
    {
        int projId;
        int tender1Id;
        int tender2Id;

        using (var db = CreateContext())
        {
            db.Database.EnsureCreated();

            var project = new BoqProject { ProjectCode = "NB-INTERMEDIATE", ProjectName = "中间层级级联测试" };
            var t1 = new BoqTenderItem { ItemTag = "T1", BillItemCode = "B1", BillItemName = "Item 1" };
            var t2 = new BoqTenderItem { ItemTag = "T2", BillItemCode = "B2", BillItemName = "Item 2" };

            var sub1 = new BoqSubProject { SubProjectCode = "SUB-T1", SubProjectName = "Sub of T1" };
            var sub2 = new BoqSubProject { SubProjectCode = "SUB-T2", SubProjectName = "Sub of T2" };

            var part1 = new BoqPart { PartMark = "PART-T1", SectionSpecification = "PL20", SteelGrade = "Q355B" };
            var part2 = new BoqPart { PartMark = "PART-T2", SectionSpecification = "PL30", SteelGrade = "Q355B" };

            sub1.Parts.Add(part1);
            sub2.Parts.Add(part2);

            t1.SubProjects.Add(sub1);
            t2.SubProjects.Add(sub2);

            project.TenderItems.Add(t1);
            project.TenderItems.Add(t2);

            db.BoqProjects.Add(project);
            db.SaveChanges();

            projId = project.Id;
            tender1Id = t1.Id;
            tender2Id = t2.Id;
        }

        // 删除 T1 清单项
        using (var db = CreateContext())
        {
            var t1 = db.BoqTenderItems.Find(tender1Id);
            Assert.NotNull(t1);
            db.BoqTenderItems.Remove(t1);
            db.SaveChanges();
        }

        // 验证 T1 及其子项已删除，但 Project 与 T2 及其子项完好无损
        using (var db = CreateContext())
        {
            Assert.Equal(1, db.BoqProjects.Count(p => p.Id == projId));
            Assert.Equal(0, db.BoqTenderItems.Count(t => t.Id == tender1Id));
            Assert.Equal(1, db.BoqTenderItems.Count(t => t.Id == tender2Id));

            Assert.Equal(0, db.BoqSubProjects.Count(s => s.SubProjectCode == "SUB-T1"));
            Assert.Equal(1, db.BoqSubProjects.Count(s => s.SubProjectCode == "SUB-T2"));

            Assert.Equal(0, db.BoqParts.Count(p => p.PartMark == "PART-T1"));
            Assert.Equal(1, db.BoqParts.Count(p => p.PartMark == "PART-T2"));
        }
    }

    [Fact]
    public void Stress04_ForeignKey_Restrict_FabricationQuotaRule_BlocksDeletionWhenReferenced()
    {
        using var db = CreateContext();
        db.Database.EnsureCreated();

        var fabRule = new FabricationQuotaRule
        {
            QuotaCode = "FAB_RESTRICT_01",
            QuotaName = "特厚板焊接定额",
            StructuralSystem = "空间管桁架",
            BaseLaborRatePerTon = 1800.0
        };
        db.FabricationQuotaRules.Add(fabRule);
        db.SaveChanges();

        var project = new BoqProject { ProjectCode = "PRJ_FK_TEST", ProjectName = "外键测试" };
        var tender = new BoqTenderItem { ItemTag = "FK_T", BillItemCode = "B_FK", BillItemName = "外键清单项" };
        var sub = new BoqSubProject
        {
            SubProjectCode = "SUB_FK",
            SubProjectName = "引用定额构件",
            FabricationQuotaRuleId = fabRule.Id
        };
        tender.SubProjects.Add(sub);
        project.TenderItems.Add(tender);
        db.BoqProjects.Add(project);
        db.SaveChanges();

        string ddl = db.Database.GenerateCreateScript();
        // Console or output
        Assert.Contains("FabricationQuotaRules", ddl);

        // 此时尝试在独立上下文中删除被引用的 FabricationQuotaRule (数据库级外键检查)
        using (var dbDelete = CreateContext())
        {
            var rule = dbDelete.FabricationQuotaRules.Single(r => r.QuotaCode == "FAB_RESTRICT_01");
            dbDelete.FabricationQuotaRules.Remove(rule);
            var ex = Assert.ThrowsAny<DbUpdateException>(() => dbDelete.SaveChanges());
            Assert.NotNull(ex);
        }

        // 解除引用后再次尝试删除，应顺利成功
        using var dbFix = CreateContext();
        var subProject = dbFix.BoqSubProjects.Single(s => s.SubProjectCode == "SUB_FK");
        subProject.FabricationQuotaRuleId = null;
        dbFix.SaveChanges();

        var ruleToDelete = dbFix.FabricationQuotaRules.Single(r => r.QuotaCode == "FAB_RESTRICT_01");
        dbFix.FabricationQuotaRules.Remove(ruleToDelete);
        dbFix.SaveChanges();

        Assert.Equal(0, dbFix.FabricationQuotaRules.Count(r => r.QuotaCode == "FAB_RESTRICT_01"));
    }

    [Fact]
    public void Stress05_ForeignKey_Restrict_InstallationQuotaRule_BlocksDeletionWhenReferenced()
    {
        using var db = CreateContext();
        db.Database.EnsureCreated();

        var insRule = new InstallationQuotaRule
        {
            QuotaCode = "INS_RESTRICT_01",
            QuotaName = "高空原位散拼吊装定额",
            ApplicableRole = "主桁架",
            BaseLaborRatePerTon = 2200.0,
            MainCraneRatePerTon = 750.0
        };
        db.InstallationQuotaRules.Add(insRule);
        db.SaveChanges();

        var project = new BoqProject { ProjectCode = "PRJ_INS_FK", ProjectName = "安装外键测试" };
        var tender = new BoqTenderItem { ItemTag = "INS_T", BillItemCode = "B_INS", BillItemName = "安装清单项" };
        var sub = new BoqSubProject
        {
            SubProjectCode = "SUB_INS_FK",
            SubProjectName = "引用安装定额构件",
            InstallationQuotaRuleId = insRule.Id
        };
        tender.SubProjects.Add(sub);
        project.TenderItems.Add(tender);
        db.BoqProjects.Add(project);
        db.SaveChanges();

        Assert.True(insRule.Id > 0);
        var checkSub = db.BoqSubProjects.Single(s => s.SubProjectCode == "SUB_INS_FK");
        Assert.Equal(insRule.Id, checkSub.InstallationQuotaRuleId);

        // 尝试在独立上下文中删除被引用的 InstallationQuotaRule (数据库级外键检查)
        using (var dbDelete = CreateContext())
        {
            var rule = dbDelete.InstallationQuotaRules.Single(r => r.QuotaCode == "INS_RESTRICT_01");
            dbDelete.InstallationQuotaRules.Remove(rule);
            var ex = Assert.ThrowsAny<DbUpdateException>(() => dbDelete.SaveChanges());
            Assert.NotNull(ex);
        }

        // 解除引用并删除
        using var dbFix = CreateContext();
        var subProject = dbFix.BoqSubProjects.Single(s => s.SubProjectCode == "SUB_INS_FK");
        subProject.InstallationQuotaRuleId = null;
        dbFix.SaveChanges();

        var ruleToDelete = dbFix.InstallationQuotaRules.Single(r => r.QuotaCode == "INS_RESTRICT_01");
        dbFix.InstallationQuotaRules.Remove(ruleToDelete);
        dbFix.SaveChanges();

        Assert.Equal(0, dbFix.InstallationQuotaRules.Count(r => r.QuotaCode == "INS_RESTRICT_01"));
    }

    [Fact]
    public void Stress06_Indices_UniqueConstraints_Enforced_In_Database()
    {
        using var db = CreateContext();
        db.Database.EnsureCreated();

        // 1. FabricationQuotaRule.QuotaCode 唯一性冲突
        var rule1 = new FabricationQuotaRule { QuotaCode = "FAB_UNIQUE_COLLISION", QuotaName = "规则1" };
        var rule2 = new FabricationQuotaRule { QuotaCode = "FAB_UNIQUE_COLLISION", QuotaName = "规则2" };

        db.FabricationQuotaRules.Add(rule1);
        db.SaveChanges();

        db.FabricationQuotaRules.Add(rule2);
        Assert.ThrowsAny<DbUpdateException>(() => db.SaveChanges());

        // 2. InstallationQuotaRule.QuotaCode 唯一性冲突
        using var db2 = CreateContext();
        var ins1 = new InstallationQuotaRule { QuotaCode = "INS_UNIQUE_COLLISION", QuotaName = "安装1" };
        var ins2 = new InstallationQuotaRule { QuotaCode = "INS_UNIQUE_COLLISION", QuotaName = "安装2" };

        db2.InstallationQuotaRules.Add(ins1);
        db2.SaveChanges();

        db2.InstallationQuotaRules.Add(ins2);
        Assert.ThrowsAny<DbUpdateException>(() => db2.SaveChanges());
    }

    [Fact]
    public void Stress07_Indices_NonUnique_AllowsDuplicatesAcrossProjects()
    {
        using var db = CreateContext();
        db.Database.EnsureCreated();

        // 验证非唯一索引字段（如 ItemTag "NH"、SubProjectCode、PartMark）允许在合法业务中重复出现
        var p1 = new BoqProject { ProjectCode = "P1", ProjectName = "Project 1" };
        var p2 = new BoqProject { ProjectCode = "P2", ProjectName = "Project 2" };

        var t1 = new BoqTenderItem { ItemTag = "NH", BillItemCode = "0101", BillItemName = "南主桁架 1" };
        var t2 = new BoqTenderItem { ItemTag = "NH", BillItemCode = "0101", BillItemName = "南主桁架 2" };

        var sub1 = new BoqSubProject { SubProjectCode = "COMMON-SUB", SubProjectName = "标准段 1" };
        var sub2 = new BoqSubProject { SubProjectCode = "COMMON-SUB", SubProjectName = "标准段 2" };

        var part1 = new BoqPart { PartMark = "HLP-15", SectionSpecification = "PL50*2150", SteelGrade = "Q460GJC" };
        var part2 = new BoqPart { PartMark = "HLP-15", SectionSpecification = "PL50*2150", SteelGrade = "Q460GJC" };

        sub1.Parts.Add(part1);
        sub2.Parts.Add(part2);

        t1.SubProjects.Add(sub1);
        t2.SubProjects.Add(sub2);

        p1.TenderItems.Add(t1);
        p2.TenderItems.Add(t2);

        db.BoqProjects.AddRange(p1, p2);
        db.SaveChanges();

        Assert.Equal(2, db.BoqProjects.Count());
        Assert.Equal(2, db.BoqTenderItems.Count(t => t.ItemTag == "NH"));
        Assert.Equal(2, db.BoqSubProjects.Count(s => s.SubProjectCode == "COMMON-SUB"));
        Assert.Equal(2, db.BoqParts.Count(p => p.PartMark == "HLP-15"));
    }

    [Fact]
    public void Stress08_EFCore_ModelMetadata_VerifiesRequiredFields_And_NotMappedProperties()
    {
        using var db = CreateContext();
        var model = db.Model;

        // 验证 6 大 BOQ 实体在 EF Core 元数据中均已正确发现
        var projEntity = model.FindEntityType(typeof(BoqProject));
        var tenderEntity = model.FindEntityType(typeof(BoqTenderItem));
        var subEntity = model.FindEntityType(typeof(BoqSubProject));
        var partEntity = model.FindEntityType(typeof(BoqPart));
        var fabEntity = model.FindEntityType(typeof(FabricationQuotaRule));
        var insEntity = model.FindEntityType(typeof(InstallationQuotaRule));

        Assert.NotNull(projEntity);
        Assert.NotNull(tenderEntity);
        Assert.NotNull(subEntity);
        Assert.NotNull(partEntity);
        Assert.NotNull(fabEntity);
        Assert.NotNull(insEntity);

        // 验证 [NotMapped] 字段绝不会在数据库表中生成多余字段
        Assert.Null(tenderEntity.FindProperty(nameof(BoqTenderItem.DirectCostPricePerUnit)));
        Assert.Null(subEntity.FindProperty(nameof(BoqSubProject.MaxPieceThicknessMm)));
        Assert.Null(subEntity.FindProperty(nameof(BoqSubProject.InstallationElevationMeter)));
        Assert.Null(subEntity.FindProperty(nameof(BoqSubProject.PieceWeightTon)));
        Assert.Null(subEntity.FindProperty(nameof(BoqSubProject.IsSiteWelded)));
        Assert.Null(subEntity.FindProperty(nameof(BoqSubProject.DesignFireResistanceHours)));

        Assert.Null(partEntity.FindProperty(nameof(BoqPart.Specification)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.MaterialGrade)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.PlateThicknessMm)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.Width)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.CutLengthMm)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.LinearWeightKgM)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.QuantityPerSubProject)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.SingleNetWeightKg)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.LossRate)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.SingleGrossWeightKg)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.ZPerformance)));
        Assert.Null(partEntity.FindProperty(nameof(BoqPart.ComprehensiveMaterialUnitPricePerTon)));

        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.ApplicableCategory)));
        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.BaseLaborDaysPerTon)));
        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.BaseFabricationFeePerTon)));
        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.IntersectingCutSurcharge)));
        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.UltrasonicTestingFeePerTon)));
        Assert.Null(fabEntity.FindProperty(nameof(FabricationQuotaRule.TrialAssemblyFeePerTon)));

        Assert.Null(insEntity.FindProperty(nameof(InstallationQuotaRule.BaseInstallationLaborDaysPerTon)));
        Assert.Null(insEntity.FindProperty(nameof(InstallationQuotaRule.BaseInstallationFeePerTon)));
        Assert.Null(insEntity.FindProperty(nameof(InstallationQuotaRule.BaseCraneCostPerTon)));
        Assert.Null(insEntity.FindProperty(nameof(InstallationQuotaRule.BaseScaffoldingFeePerTon)));

        // 验证字符串最大长度限制配置
        Assert.Equal(50, projEntity.FindProperty(nameof(BoqProject.ProjectCode))?.GetMaxLength());
        Assert.Equal(200, projEntity.FindProperty(nameof(BoqProject.ProjectName))?.GetMaxLength());
        Assert.Equal(50, tenderEntity.FindProperty(nameof(BoqTenderItem.ItemTag))?.GetMaxLength());
        Assert.Equal(100, subEntity.FindProperty(nameof(BoqSubProject.SubProjectCode))?.GetMaxLength());
        Assert.Equal(100, partEntity.FindProperty(nameof(BoqPart.PartMark))?.GetMaxLength());
    }

    [Fact]
    public void Stress09_BulkHierarchy_EagerLoading_And_Performance_StressTest()
    {
        using (var db = CreateContext())
        {
            db.Database.EnsureCreated();

            var project = new BoqProject
            {
                ProjectCode = "NB-OLYMPIC-BULK",
                ProjectName = "宁波奥体高并发大批量汇算测试",
                ClientName = "指挥部"
            };

            // 构造 5 个清单项，每个清单项 10 个构件，每个构件 20 个零件 = 1000 个零件
            for (int t = 1; t <= 5; t++)
            {
                var tender = new BoqTenderItem
                {
                    ItemTag = $"ITEM_{t}",
                    BillItemCode = $"010100{t}",
                    BillItemName = $"清单项 {t}",
                    TenderQuantity = 200.0 * t
                };

                for (int s = 1; s <= 10; s++)
                {
                    var sub = new BoqSubProject
                    {
                        SubProjectCode = $"SUB_{t}_{s}",
                        SubProjectName = $"分段构件 {t}-{s}",
                        Quantity = 1,
                        MaxPlateThickness = 50.0
                    };

                    for (int p = 1; p <= 20; p++)
                    {
                        sub.Parts.Add(new BoqPart
                        {
                            PartMark = $"P_{t}_{s}_{p}",
                            SectionSpecification = "PL50*2000",
                            SectionType = "PL",
                            SteelGrade = "Q460GJC",
                            Thickness = 50.0,
                            Quantity = 1,
                            SingleNetWeight = 100.0,
                            TotalNetWeight = 100.0,
                            PurchasingWasteFactor = 1.15,
                            TotalGrossWeight = 115.0
                        });
                    }
                    tender.SubProjects.Add(sub);
                }
                project.TenderItems.Add(tender);
            }

            db.BoqProjects.Add(project);
            db.SaveChanges();
        }

        // 测试深度贪婪加载 (Eager Loading Include)
        using (var dbQuery = CreateContext())
        {
            var loaded = dbQuery.BoqProjects
                .Include(p => p.TenderItems)
                    .ThenInclude(t => t.SubProjects)
                        .ThenInclude(s => s.Parts)
                .Single(p => p.ProjectCode == "NB-OLYMPIC-BULK");

            Assert.NotNull(loaded);
            Assert.Equal(5, loaded.TenderItems.Count);
            Assert.Equal(50, loaded.TenderItems.SelectMany(t => t.SubProjects).Count());
            Assert.Equal(1000, loaded.TenderItems.SelectMany(t => t.SubProjects).SelectMany(s => s.Parts).Count());

            // 验证累计毛重
            double totalGross = loaded.TenderItems
                .SelectMany(t => t.SubProjects)
                .SelectMany(s => s.Parts)
                .Sum(p => p.TotalGrossWeight);
            Assert.Equal(1000 * 115.0, totalGross, 2);
        }
    }
}
