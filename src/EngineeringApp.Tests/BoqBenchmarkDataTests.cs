// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqBenchmarkDataTests.cs
// 模块功能: 宁波奥林匹克体育中心 BOQ 基准算量数据与数据库种子加载器完备性验证测试
// 规范依据: ORIGINAL_REQUEST.md (R3, R6), PROJECT.md (Feature 22, 23), AGENTS.md
// ============================================================================

using System;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using EngineeringApp.Server.Data;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Tests
{
    public class BoqBenchmarkDataTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqliteConnection _sqliteConnection;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public BoqBenchmarkDataTests()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"boq_benchmark_{Guid.NewGuid():N}.db");
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
        public void Test01_BenchmarkConstants_Match_Authoritative_Spec()
        {
            Assert.Equal("NB-ATZX", NingboOlympicSeedData.BenchmarkProjectCode);
            Assert.Equal(0.08, NingboOlympicSeedData.DefaultManagementFeeRate);
            Assert.Equal(0.07, NingboOlympicSeedData.DefaultProfitRate);
            Assert.Equal(1123939.15, NingboOlympicSeedData.BenchmarkNhNetWeightKg);
            Assert.Equal(5334.63, NingboOlympicSeedData.BenchmarkNhSurfaceAreaM2);
            Assert.Equal(1292530.02, NingboOlympicSeedData.BenchmarkNhGrossWeightKg);
            Assert.Equal(1.15, NingboOlympicSeedData.BenchmarkWasteFactor);
            Assert.Equal(1.15, NingboOlympicSeedData.BenchmarkPurchasingWasteFactor);
            Assert.Equal(1123.94, NingboOlympicSeedData.BenchmarkNhTenderQuantityTon);
            Assert.Equal(0.10, NingboOlympicSeedData.BenchmarkNhCustomMgmtRate);
            Assert.Equal(0.12, NingboOlympicSeedData.BenchmarkNhCustomProfitRate);
            Assert.Equal(865200.00, NingboOlympicSeedData.BenchmarkNfNetWeightKg);
            Assert.Equal(865.20, NingboOlympicSeedData.BenchmarkNfTenderQuantityTon);
            Assert.Equal(728300.00, NingboOlympicSeedData.BenchmarkJlcNetWeightKg);
            Assert.Equal(728.30, NingboOlympicSeedData.BenchmarkJlcTenderQuantityTon);
            Assert.Equal(837545.00, NingboOlympicSeedData.BenchmarkJlcGrossWeightKg);
            Assert.Equal(0.05, NingboOlympicSeedData.BenchmarkJlcCustomMgmtRate);
            Assert.Equal(0.04, NingboOlympicSeedData.BenchmarkJlcCustomProfitRate);
        }

        [Fact]
        public void Test02_NH_NetWeight_SurfaceArea_And_GrossWeight_Are_Exact()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var itemNH = project.TenderItems.Single(t => t.ItemTag == "NH");

            // 7 个代表性与主体工程构件
            Assert.Equal(7, itemNH.SubProjects.Count);

            // 理论净重精准核验：1,123,939.15 kg
            double sumNetWeight = itemNH.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(1123939.15, sumNetWeight, precision: 2);

            // 零件层理论净重精准核验
            double sumPartsNetWeight = itemNH.SubProjects.SelectMany(s => s.Parts).Sum(p => p.TotalNetWeight);
            Assert.Equal(1123939.15, sumPartsNetWeight, precision: 2);

            // 展开表面积精准核验：5,334.63 m2
            double sumArea = itemNH.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(5334.63, sumArea, precision: 2);

            // 零件层表面积精准核验
            double sumPartsArea = itemNH.SubProjects.SelectMany(s => s.Parts).Sum(p => p.TotalSurfaceAreaM2);
            Assert.Equal(5334.63, sumPartsArea, precision: 2);

            // 1.15 倍采购损耗毛重精准核验：1,292,530.02 kg
            double sumGrossWeight = itemNH.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(1292530.02, Math.Round(sumGrossWeight, 2));

            // 全零件 1.15 损耗系数无变动
            Assert.All(itemNH.SubProjects.SelectMany(s => s.Parts), p =>
            {
                Assert.Equal(1.15, p.PurchasingWasteFactor);
                Assert.Equal(Math.Round(p.TotalNetWeight * 1.15, 4), Math.Round(p.TotalGrossWeight, 4));
            });
        }

        [Fact]
        public void Test03_NH_Representative_Parts_Match_Survey_Spec()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var itemNH = project.TenderItems.Single(t => t.ItemTag == "NH");

            // 1. N-NHXX-48 (空间相贯节点段, 净重 322.70 kg, 表面积 4.24 m2)
            var sub48 = itemNH.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-48");
            Assert.Equal(322.70, sub48.SubProjectNetWeightKg, precision: 2);
            Assert.Equal(4.24, sub48.SubProjectSurfaceAreaM2, precision: 2);
            Assert.True(sub48.HasFieldWelding);
            Assert.Equal(16.0, sub48.MaxPlateThickness);
            Assert.Equal(36.0, sub48.AssemblyElevation);
            Assert.Equal("<5t", sub48.CraneTonnageTier);

            // 2. N-NHXX-49 (特厚下弦主肢大管段, 净重 12,850.00 kg, 表面积 40.33 m2)
            var sub49 = itemNH.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-49");
            Assert.Equal(12850.00, sub49.SubProjectNetWeightKg, precision: 2);
            Assert.Equal(40.33, sub49.SubProjectSurfaceAreaM2, precision: 2);
            Assert.Equal(50.0, sub49.MaxPlateThickness);
            Assert.Equal("5-15t", sub49.CraneTonnageTier);
            var part15 = sub49.Parts.Single(p => p.PartMark == "HLP-15");
            Assert.Equal("Q460GJC", part15.SteelGrade);
            Assert.Equal("Z15", part15.ZDirectionGrade);
            Assert.Equal(50.0, part15.Thickness);
            Assert.Equal(11350.00, part15.TotalNetWeight);

            // 3. N-NHXX-50 (空间主弦杆对接段, 净重 4,170.00 kg, 表面积 14.60 m2)
            var sub50 = itemNH.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-50");
            Assert.Equal(4170.00, sub50.SubProjectNetWeightKg, precision: 2);
            Assert.Equal(14.60, sub50.SubProjectSurfaceAreaM2, precision: 2);
            Assert.True(sub50.HasFieldWelding);
            Assert.Equal(42.0, sub50.AssemblyElevation);
        }

        [Fact]
        public void Test04_FeeRates_TwoTier_Overrides_And_Projections()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // 1. NH 启用自定义单项费率覆盖: 管理费 10%, 利润 12%
            var itemNH = project.TenderItems.Single(t => t.ItemTag == "NH");
            Assert.True(itemNH.IsCustomRateEnabled);
            var (nhMgmt, nhProfit) = itemNH.GetEffectiveRates(project);
            Assert.Equal(0.10, nhMgmt);
            Assert.Equal(0.12, nhProfit);
            // 组价倍数: (1 + 0.10) * (1 + 0.12) = 1.232
            double expectedNhMarkup = 1.10 * 1.12;
            Assert.Equal(expectedNhMarkup, (1.0 + nhMgmt) * (1.0 + nhProfit), precision: 4);
            Assert.Equal(itemNH.DirectCostPerTon * expectedNhMarkup, itemNH.ComprehensiveUnitPrice, precision: 2);

            // 2. NF 继承项目默认费率: 管理费 8%, 利润 7%
            var itemNF = project.TenderItems.Single(t => t.ItemTag == "NF");
            Assert.False(itemNF.IsCustomRateEnabled);
            var (nfMgmt, nfProfit) = itemNF.GetEffectiveRates(project);
            Assert.Equal(0.08, nfMgmt);
            Assert.Equal(0.07, nfProfit);
            double expectedNfMarkup = 1.08 * 1.07; // 1.1556
            Assert.Equal(expectedNfMarkup, (1.0 + nfMgmt) * (1.0 + nfProfit), precision: 4);
            Assert.Equal(itemNF.DirectCostPerTon * expectedNfMarkup, itemNF.ComprehensiveUnitPrice, precision: 2);

            // 3. JLC 启用自定义单项竞争性降价费率: 管理费 5%, 利润 4%
            var itemJLC = project.TenderItems.Single(t => t.ItemTag == "JLC");
            Assert.True(itemJLC.IsCustomRateEnabled);
            var (jlcMgmt, jlcProfit) = itemJLC.GetEffectiveRates(project);
            Assert.Equal(0.05, jlcMgmt);
            Assert.Equal(0.04, jlcProfit);
            double expectedJlcMarkup = 1.05 * 1.04; // 1.092
            Assert.Equal(expectedJlcMarkup, (1.0 + jlcMgmt) * (1.0 + jlcProfit), precision: 4);
            Assert.Equal(itemJLC.DirectCostPerTon * expectedJlcMarkup, itemJLC.ComprehensiveUnitPrice, precision: 2);
        }

        [Fact]
        public void Test05_NF_And_JLC_Metrics_Match_Authoritative_Spec()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // NF 副桁架: 865.20 t (865,200.00 kg), 2.0H 防火涂装
            var itemNF = project.TenderItems.Single(t => t.ItemTag == "NF");
            Assert.Equal(3, itemNF.SubProjects.Count);
            double nfNetKg = itemNF.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(865200.00, nfNetKg, precision: 2);
            double nfGrossKg = itemNF.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(994980.00, nfGrossKg, precision: 2);
            Assert.All(itemNF.SubProjects, s => Assert.Equal(2.0, s.FireRatingHours));

            // JLC 检录处: 728.30 t (728,300.00 kg), 837.545 t 毛重, 1.5H 防火涂装
            var itemJLC = project.TenderItems.Single(t => t.ItemTag == "JLC");
            Assert.Equal(3, itemJLC.SubProjects.Count);
            double jlcNetKg = itemJLC.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(728300.00, jlcNetKg, precision: 2);
            double jlcGrossKg = itemJLC.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(837545.00, jlcGrossKg, precision: 2);
            Assert.All(itemJLC.SubProjects, s => Assert.Equal(1.5, s.FireRatingHours));
        }

        [Fact]
        public void Test06_QuotaRules_And_ScalingFactors_Are_Sound()
        {
            // 加工定额
            var fabRules = NingboOlympicSeedData.GetDefaultFabricationQuotaRules();
            Assert.True(fabRules.Count >= 3);
            Assert.Contains(fabRules, r => r.QuotaCode == "FAB_SPACE_TRUSS_MAIN" && r.BaseLaborRatePerTon == 2200.0);
            Assert.Contains(fabRules, r => r.QuotaCode == "FAB_SPACE_TRUSS_SUB" && r.BaseLaborRatePerTon == 1600.0);
            Assert.Contains(fabRules, r => r.QuotaCode == "FAB_LIGHT_STEEL" && r.BaseLaborRatePerTon == 1100.0);

            // 安装定额
            var insRules = NingboOlympicSeedData.GetDefaultInstallationQuotaRules();
            Assert.True(insRules.Count >= 3);
            Assert.Contains(insRules, r => r.QuotaCode == "INS_SPACE_TRUSS_H30M" && r.BaseLaborRatePerTon == 2100.0);
            Assert.Contains(insRules, r => r.QuotaCode == "INS_SPACE_TRUSS_SUB" && r.BaseLaborRatePerTon == 1400.0);
            Assert.Contains(insRules, r => r.QuotaCode == "INS_LIGHT_STEEL" && r.BaseLaborRatePerTon == 900.0);

            // 验算板厚难度系数阶梯 (K_Thick)
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(16.0));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(25.0));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(35.0));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(50.0));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(65.0));

            // 验算安装就位标高阶梯 (K_Elev)
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(15.0));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(30.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(38.5));

            // 验算现场焊系数 (K_SiteWeld)
            Assert.Equal(1.30, BoqAggregationEngine.CalculateSiteWeldFactor(true));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateSiteWeldFactor(false));

            // 验算起重机吨位系数 (K_Weight)
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactorFromTier("<5t", 3.0));
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactorFromTier("5-15t", 12.85));
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactorFromTier(">25t", 28.5));

            // 验算耐火极限涂装费率映射
            Assert.Equal(145.0, BoqAggregationEngine.CalculateFireRatingRate(3.0));
            Assert.Equal(110.0, BoqAggregationEngine.CalculateFireRatingRate(2.0));
            Assert.Equal(75.0, BoqAggregationEngine.CalculateFireRatingRate(1.5));
        }

        [Fact]
        public void Test07_DatabaseSeeder_SQLite_RoundTrip_And_Idempotence()
        {
            using (var db = CreateContext())
            {
                db.Database.EnsureCreated();

                // 首次执行装载
                NingboOlympicDataSeeder.Seed(db);

                // 校验项目主表
                var project = db.BoqProjects
                    .Include(p => p.TenderItems)
                        .ThenInclude(t => t.SubProjects)
                            .ThenInclude(s => s.Parts)
                    .Single(p => p.ProjectCode == "NB-ATZX");

                Assert.NotNull(project);
                Assert.Equal("宁波奥林匹克体育中心主体育场与配套工程", project.ProjectName);
                Assert.Equal(3, project.TenderItems.Count);

                // 校验定额规则已持久化且具备自增外键
                Assert.True(db.FabricationQuotaRules.Count() >= 3);
                Assert.True(db.InstallationQuotaRules.Count() >= 3);

                // 校验 NH 构件数量与累计指标
                var nh = project.TenderItems.Single(t => t.ItemTag == "NH");
                Assert.Equal(7, nh.SubProjects.Count);
                Assert.Equal(1123939.15, nh.SubProjects.Sum(s => s.SubProjectNetWeightKg), precision: 2);
                Assert.Equal(5334.63, nh.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2), precision: 2);

                // 校验外键均已正确关联且不为 null
                Assert.All(nh.SubProjects, s =>
                {
                    Assert.NotNull(s.FabricationQuotaRuleId);
                    Assert.NotNull(s.InstallationQuotaRuleId);
                    Assert.True(s.FabricationQuotaRuleId > 0);
                    Assert.True(s.InstallationQuotaRuleId > 0);
                });

                // 校验零件总数 (NH 14 件 + NF 3 件 + JLC 3 件 = 20 件)
                int totalParts = project.TenderItems.SelectMany(t => t.SubProjects).SelectMany(s => s.Parts).Count();
                Assert.Equal(20, totalParts);

                // 二次执行测试幂等性
                NingboOlympicDataSeeder.Seed(db);
                Assert.Equal(1, db.BoqProjects.Count(p => p.ProjectCode == "NB-ATZX"));
            }
        }
    }
}
