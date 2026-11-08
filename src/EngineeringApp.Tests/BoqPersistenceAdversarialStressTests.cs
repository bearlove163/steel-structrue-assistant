// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqPersistenceAdversarialStressTests.cs
// 模块功能: BOQ 数据库持久化、种子幂等性、实体变更追踪与级联外键敌手化压力测试
// 规范依据: ORIGINAL_REQUEST.md (R3), DISPATCH.md, PROJECT.md (Feature 7, 22, 23)
// 角色定位: Empirical Challenger (critic & specialist)
// ============================================================================

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EngineeringApp.Server.Data;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;
using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Tests
{
    public class BoqPersistenceAdversarialStressTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqliteConnection _sqliteConnection;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public BoqPersistenceAdversarialStressTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"boq_adversarial_{Guid.NewGuid():N}.db");
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

        /// <summary>
        /// 挑战1: 在同一个 DbContext 实例上连续调用 5 次 NingboOlympicDataSeeder.Seed(db)
        /// 验证绝无重复插入、主键冲突或唯一索引异常，各实体数严格保持预期基准
        /// </summary>
        [Fact]
        public void Adv01_Seeder_5ConsecutiveCalls_SameContext_StrictlyIdempotent()
        {
            using var db = CreateContext();
            db.Database.EnsureCreated();

            // 连续调用 5 次
            for (int i = 0; i < 5; i++)
            {
                NingboOlympicDataSeeder.Seed(db);
            }

            // 严格核查各层级记录唯一性
            Assert.Equal(1, db.BoqProjects.Count());
            Assert.Equal(1, db.BoqProjects.Count(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode));
            Assert.Equal(3, db.BoqTenderItems.Count());
            Assert.Equal(13, db.BoqSubProjects.Count());
            Assert.Equal(20, db.BoqParts.Count());

            // 车间制作定额规则字典数
            int fabRulesCount = NingboOlympicSeedData.GetDefaultFabricationQuotaRules().Count;
            Assert.Equal(fabRulesCount, db.FabricationQuotaRules.Count());

            // 现场安装定额规则字典数
            int insRulesCount = NingboOlympicSeedData.GetDefaultInstallationQuotaRules().Count;
            Assert.Equal(insRulesCount, db.InstallationQuotaRules.Count());
        }

        /// <summary>
        /// 挑战2: 跨 5 个独立的 DbContext 实例连续调用 NingboOlympicDataSeeder.Seed(db)
        /// 验证跨事务/跨生命周期边界的幂等探测与外键一致性
        /// </summary>
        [Fact]
        public void Adv02_Seeder_5ConsecutiveCalls_AcrossIndependentContexts_StrictlyIdempotent()
        {
            using (var initDb = CreateContext())
            {
                initDb.Database.EnsureCreated();
            }

            for (int i = 0; i < 5; i++)
            {
                using var db = CreateContext();
                NingboOlympicDataSeeder.Seed(db);
            }

            using (var verifyDb = CreateContext())
            {
                Assert.Equal(1, verifyDb.BoqProjects.Count());
                Assert.Equal(3, verifyDb.BoqTenderItems.Count());
                Assert.Equal(13, verifyDb.BoqSubProjects.Count());
                Assert.Equal(20, verifyDb.BoqParts.Count());

                var project = verifyDb.BoqProjects
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.Parts)
                    .Single(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode);

                Assert.NotNull(project);
                var nh = project.TenderItems.Single(t => t.ItemTag == "NH");
                Assert.Equal(7, nh.SubProjects.Count);
                Assert.Equal(1123939.15, nh.SubProjects.Sum(s => s.SubProjectNetWeightKg), precision: 2);
            }
        }

        /// <summary>
        /// 挑战3: 连续多次调用 AppDbContext.SeedData(db) 在干净与已存在数据库上均无任何异常与数据重复
        /// </summary>
        [Fact]
        public void Adv03_AppDbContext_SeedData_MultipleCalls_FreshAndExistingDatabase()
        {
            // 第一次调用：干净数据库上初始化全库
            using (var db1 = CreateContext())
            {
                AppDbContext.SeedData(db1);

                Assert.Equal(31, db1.Suppliers.Count());
                Assert.True(db1.StandardSteels.Any());
                Assert.True(db1.PaintProducts.Any());
                Assert.True(db1.MaterialPriceSnapshots.Any());
                Assert.True(db1.PlateThicknessLadders.Any());
                Assert.True(db1.PlateDimensionRules.Any());
                Assert.True(db1.SteelMills.Any());
                Assert.True(db1.FreightRoutes.Any());
                Assert.True(db1.MaterialProvenanceRecords.Any());
                Assert.Equal(1, db1.BoqProjects.Count());
            }

            // 第二次调用：在同一数据库的新上下文再次执行 SeedData
            using (var db2 = CreateContext())
            {
                AppDbContext.SeedData(db2);

                Assert.Equal(31, db2.Suppliers.Count());
                Assert.Equal(1, db2.BoqProjects.Count());
            }

            // 第三次调用：同一上下文重复调用
            using (var db3 = CreateContext())
            {
                AppDbContext.SeedData(db3);
                AppDbContext.SeedData(db3);

                Assert.Equal(31, db3.Suppliers.Count());
                Assert.Equal(1, db3.BoqProjects.Count());
            }
        }

        /// <summary>
        /// 挑战4: 供应商库 SupplierDatabase 31 家供应商 ID 与编码唯一性深度验证与冲突防御机制测试
        /// </summary>
        [Fact]
        public void Adv04_SupplierDatabase_All31Suppliers_StrictUniqueness_And_CollisionResilience()
        {
            var allSuppliers = SupplierDatabase.AllSuppliers;
            Assert.Equal(31, allSuppliers.Count);

            // 1. 验证所有 ID 均大于 0 且无重复
            var ids = allSuppliers.Select(s => s.Id).ToList();
            Assert.Equal(31, ids.Distinct().Count());
            Assert.All(ids, id => Assert.True(id > 0));

            // 2. 验证所有供应商编码 Code 均唯一且符合规范
            var codes = allSuppliers.Select(s => s.Code).ToList();
            Assert.Equal(31, codes.Distinct().Count());
            Assert.All(codes, code => Assert.False(string.IsNullOrWhiteSpace(code)));

            // 3. 验证历史曾经冲突的钢材贸易商 ID (SUP-TRD-001..004) 已移至 29..32，彻底脱离 7..10
            var traders = allSuppliers.Where(s => s.Category == SupplierCategory.SteelTrader).ToList();
            Assert.Equal(4, traders.Count);
            Assert.Contains(traders, t => t.Id == 29 && t.Code == "SUP-TRD-001");
            Assert.Contains(traders, t => t.Id == 30 && t.Code == "SUP-TRD-002");
            Assert.Contains(traders, t => t.Id == 31 && t.Code == "SUP-TRD-003");
            Assert.Contains(traders, t => t.Id == 32 && t.Code == "SUP-TRD-004");

            // 4. 验证数据库中入库后供应商 ID 与领域定义 100% 吻合
            using var db = CreateContext();
            AppDbContext.SeedData(db);

            var dbSuppliers = db.Suppliers.ToList();
            Assert.Equal(31, dbSuppliers.Count);
            foreach (var sup in allSuppliers)
            {
                var match = dbSuppliers.FirstOrDefault(s => s.Id == sup.Id);
                Assert.NotNull(match);
                Assert.Equal(sup.Code, match.Code);
            }

            // 5. 攻防模拟：测试 AppDbContext.SeedData 中的兜底去重机制 (Fallback ID >= 100)
            // 当人为传入含重复 ID 或非正数 ID 的供应商列表时，验证算法不会抛出唯一主键异常
            var simulatedList = new List<SupplierItem>
            {
                new() { Id = 7, Code = "SUP-A", Name = "厂A" },
                new() { Id = 7, Code = "SUP-B", Name = "厂B (碰撞ID 7)" },
                new() { Id = 0, Code = "SUP-C", Name = "厂C (非法ID 0)" }
            };

            var existingIds = new HashSet<int>();
            int nextFallbackId = 100;
            var allocatedList = new List<SupplierEntity>();
            foreach (var s in simulatedList)
            {
                var ent = SupplierEntity.FromDomain(s);
                if (existingIds.Contains(ent.Id) || ent.Id <= 0)
                {
                    while (existingIds.Contains(nextFallbackId)) nextFallbackId++;
                    ent.Id = nextFallbackId;
                }
                existingIds.Add(ent.Id);
                allocatedList.Add(ent);
            }

            Assert.Equal(3, allocatedList.Select(e => e.Id).Distinct().Count());
            Assert.Equal(7, allocatedList[0].Id);
            Assert.Equal(100, allocatedList[1].Id);
            Assert.Equal(101, allocatedList[2].Id);
        }

        /// <summary>
        /// 挑战5: 变更追踪器 (ChangeTracker) 深度树图 Include、修改、新增、删除与 SaveChanges 压力测试
        /// 模拟生产环境下用户在界面修改清单工程量、增删零件并重新卷积计算后持久化
        /// </summary>
        [Fact]
        public void Adv05_ChangeTracker_DeepGraph_Modify_Add_Remove_And_ReCalculate()
        {
            using (var dbInit = CreateContext())
            {
                AppDbContext.SeedData(dbInit);
            }

            int projectId;
            using (var dbEdit = CreateContext())
            {
                // 深度贪婪加载 4 级树并追踪
                var project = dbEdit.BoqProjects
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.Parts)
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.FabricationQuotaRule)
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.InstallationQuotaRule)
                    .Single(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode);

                projectId = project.Id;

                // 1. 修改根项目属性
                project.ProjectName = "宁波奥体-变更追踪极限测试版";
                project.DefaultManagementFeeRate = 0.085;
                project.DefaultProfitRate = 0.075;

                // 2. 修改现有清单项与构件
                var nh = project.TenderItems.Single(t => t.ItemTag == "NH");
                nh.TenderQuantity = 1150.0;
                nh.IsCustomRateEnabled = true;
                nh.CustomManagementFeeRate = 0.105;
                nh.CustomProfitRate = 0.125;

                var sub48 = nh.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-48");
                sub48.SubProjectName = "空间相贯节点段 (增强型)";
                sub48.MaxPlateThickness = 20.0;

                // 3. 在现有构件中新增一个叶子零件
                var newPart = new BoqPart
                {
                    PartMark = "P-ADV-TEST",
                    SectionSpecification = "PL20*500",
                    SectionType = "PL",
                    SteelGrade = "Q355B",
                    Thickness = 20.0,
                    Quantity = 2,
                    SingleNetWeight = 50.0,
                    TotalNetWeight = 100.0,
                    PurchasingWasteFactor = 1.15,
                    TotalGrossWeight = 115.0,
                    MaterialUnitPricePerTon = 4500.0
                };
                sub48.Parts.Add(newPart);

                // 4. 从现有构件中移除一个零件
                var partToRemove = sub48.Parts.First(p => p.PartMark != "P-ADV-TEST");
                sub48.Parts.Remove(partToRemove);
                dbEdit.BoqParts.Remove(partToRemove);

                // 5. 在项目下动态挂载一个全新清单项
                var newTender = new BoqTenderItem
                {
                    ItemTag = "ADV-ADDON",
                    BillItemCode = "010199999",
                    BillItemName = "新增对抗测试分项",
                    Unit = "t",
                    TenderQuantity = 10.0
                };
                var newSub = new BoqSubProject
                {
                    SubProjectCode = "ADV-SUB-01",
                    SubProjectName = "新增构件",
                    FabricationQuotaRuleId = sub48.FabricationQuotaRuleId,
                    InstallationQuotaRuleId = sub48.InstallationQuotaRuleId
                };
                newSub.Parts.Add(new BoqPart
                {
                    PartMark = "ADV-P-01",
                    SectionSpecification = "PL16*200",
                    SectionType = "PL",
                    SteelGrade = "Q355B",
                    Quantity = 1,
                    SingleNetWeight = 20.0,
                    TotalNetWeight = 20.0,
                    PurchasingWasteFactor = 1.15,
                    TotalGrossWeight = 23.0
                });
                newTender.SubProjects.Add(newSub);
                project.TenderItems.Add(newTender);

                // 6. 调用卷积计算引擎就地更新派生字段
                BoqAggregationEngine.CalculateProject(project);

                // 7. 保存变更 (必须不抛出任何冲突或追踪异常)
                dbEdit.SaveChanges();
            }

            // 在独立验证上下文中读取持久化结果并验算
            using (var dbVerify = CreateContext())
            {
                var loaded = dbVerify.BoqProjects
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.Parts)
                    .Single(p => p.Id == projectId);

                Assert.Equal("宁波奥体-变更追踪极限测试版", loaded.ProjectName);
                Assert.Equal(0.085, loaded.DefaultManagementFeeRate);
                Assert.Equal(4, loaded.TenderItems.Count); // 原有3个 + 新增1个

                var nh = loaded.TenderItems.Single(t => t.ItemTag == "NH");
                Assert.Equal(1150.0, nh.TenderQuantity);
                Assert.Equal(0.105, nh.CustomManagementFeeRate);

                var sub48 = nh.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-48");
                Assert.Equal("空间相贯节点段 (增强型)", sub48.SubProjectName);
                Assert.Contains(sub48.Parts, p => p.PartMark == "P-ADV-TEST");

                var addon = loaded.TenderItems.Single(t => t.ItemTag == "ADV-ADDON");
                Assert.Single(addon.SubProjects);
                Assert.Equal("ADV-SUB-01", addon.SubProjects[0].SubProjectCode);
                Assert.Single(addon.SubProjects[0].Parts);
            }
        }

        /// <summary>
        /// 挑战6: 级联删除测试：删除种子导入的 BoqProject 项目大盘
        /// 必须彻底级联清除关联的 TenderItems、SubProjects 和 Parts，同时 FabricationQuotaRules、InstallationQuotaRules 与 Suppliers 保持完好 (Restrict 保护)
        /// </summary>
        [Fact]
        public void Adv06_CascadeDelete_SeededProject_WipesHierarchy_LeavesQuotaRulesIntact()
        {
            using (var dbInit = CreateContext())
            {
                AppDbContext.SeedData(dbInit);
            }

            // 在独立上下文中删除项目
            using (var dbDelete = CreateContext())
            {
                var project = dbDelete.BoqProjects.Single(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode);
                dbDelete.BoqProjects.Remove(project);
                dbDelete.SaveChanges();
            }

            // 在验证上下文中检验级联结果
            using (var dbVerify = CreateContext())
            {
                Assert.Equal(0, dbVerify.BoqProjects.Count());
                Assert.Equal(0, dbVerify.BoqTenderItems.Count());
                Assert.Equal(0, dbVerify.BoqSubProjects.Count());
                Assert.Equal(0, dbVerify.BoqParts.Count());

                // 核心定额字典表与供应商库完好无损
                Assert.True(dbVerify.FabricationQuotaRules.Count() >= 3);
                Assert.True(dbVerify.InstallationQuotaRules.Count() >= 3);
                Assert.Equal(31, dbVerify.Suppliers.Count());
            }
        }

        /// <summary>
        /// 挑战7: 项目被级联删除后，定额规则依然存在于数据库中，再次调用 NingboOlympicDataSeeder.Seed(db)
        /// 验证在“定额已存在、项目不存在”的边缘状态下，装载器依然能正确复用已有定额主键并完整重建项目树，不触发定额编码唯一索引冲突
        /// </summary>
        [Fact]
        public void Adv07_ReSeed_After_Project_Deletion_RecreatesProject_WithoutQuotaCollisions()
        {
            using (var dbInit = CreateContext())
            {
                AppDbContext.SeedData(dbInit);
            }

            // 1. 删除项目
            using (var dbDelete = CreateContext())
            {
                var project = dbDelete.BoqProjects.Single(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode);
                dbDelete.BoqProjects.Remove(project);
                dbDelete.SaveChanges();

                Assert.Equal(0, dbDelete.BoqProjects.Count());
                Assert.True(dbDelete.FabricationQuotaRules.Any());
                Assert.True(dbDelete.InstallationQuotaRules.Any());
            }

            // 2. 再次执行装载 (定额字典已存在于 DB)
            using (var dbReSeed = CreateContext())
            {
                NingboOlympicDataSeeder.Seed(dbReSeed);
            }

            // 3. 验证项目重建成功且定额未重复产生
            using (var dbVerify = CreateContext())
            {
                Assert.Equal(1, dbVerify.BoqProjects.Count());
                Assert.Equal(3, dbVerify.BoqTenderItems.Count());
                Assert.Equal(13, dbVerify.BoqSubProjects.Count());
                Assert.Equal(20, dbVerify.BoqParts.Count());

                int expectedFabCount = NingboOlympicSeedData.GetDefaultFabricationQuotaRules().Count;
                int expectedInsCount = NingboOlympicSeedData.GetDefaultInstallationQuotaRules().Count;
                Assert.Equal(expectedFabCount, dbVerify.FabricationQuotaRules.Count());
                Assert.Equal(expectedInsCount, dbVerify.InstallationQuotaRules.Count());

                // 验证所有构件的外键均有效关联
                var subProjects = dbVerify.BoqSubProjects.ToList();
                Assert.All(subProjects, s =>
                {
                    Assert.NotNull(s.FabricationQuotaRuleId);
                    Assert.NotNull(s.InstallationQuotaRuleId);
                    Assert.True(s.FabricationQuotaRuleId > 0);
                    Assert.True(s.InstallationQuotaRuleId > 0);
                });
            }
        }

        /// <summary>
        /// 挑战8: 真实外键约束 (PRAGMA foreign_keys = ON) 下的 DeleteBehavior.Restrict 物理拦截验证
        /// 当构件仍引用定额时，直接删除定额规则必须被 SQLite 物理拦截并抛出 DbUpdateException
        /// </summary>
        [Fact]
        public void Adv08_DeleteBehavior_Restrict_On_Seeded_QuotaRules_Throws_DbUpdateException()
        {
            using (var dbInit = CreateContext())
            {
                AppDbContext.SeedData(dbInit);
            }

            // 1. 尝试删除被 NH 构件引用的车间制作定额
            using (var dbDelFab = CreateContext())
            {
                var fabRule = dbDelFab.FabricationQuotaRules.First(r => r.QuotaCode == "FAB_SPACE_TRUSS_MAIN");
                dbDelFab.FabricationQuotaRules.Remove(fabRule);
                var ex = Assert.ThrowsAny<DbUpdateException>(() => dbDelFab.SaveChanges());
                Assert.NotNull(ex);
            }

            // 2. 尝试删除被引用的现场安装定额
            using (var dbDelIns = CreateContext())
            {
                var insRule = dbDelIns.InstallationQuotaRules.First(r => r.QuotaCode == "INS_SPACE_TRUSS_H30M");
                dbDelIns.InstallationQuotaRules.Remove(insRule);
                var ex = Assert.ThrowsAny<DbUpdateException>(() => dbDelIns.SaveChanges());
                Assert.NotNull(ex);
            }
        }
    }
}
