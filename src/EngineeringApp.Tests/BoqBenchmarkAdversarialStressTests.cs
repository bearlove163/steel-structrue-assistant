// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqBenchmarkAdversarialStressTests.cs
// 模块功能: 宁波奥体中心基准算量与卷积核算引擎敌手化压力测试套件 (Adversarial Stress Harness)
// 规范依据: ORIGINAL_REQUEST.md (R2, R3, R6), PROJECT.md (M3, M5), AGENTS.md
// ============================================================================

using System;
using System.Collections.Generic;
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
    /// <summary>
    /// 对宁波奥体中心基准数据 (NingboOlympicSeedData) 与卷积核算引擎 (BoqAggregationEngine) 的敌手化对抗测试
    /// 包含：零件级重量/面积精度与浮点漂移挑战、损耗系数缩放不变性、定额阶梯临界跃迁与单调性神谕、
    /// 耐火极限涂装排程、双层费率判定及五大受众投影跨视角守恒对账
    /// </summary>
    public class BoqBenchmarkAdversarialStressTests : IDisposable
    {
        private readonly string _dbPath;
        private readonly SqliteConnection _sqliteConnection;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;

        public BoqBenchmarkAdversarialStressTests()
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

        #region 1. 零件级理论净重、表面积精度与浮点漂移挑战 (Part-Level Precision & Permutation Drift)

        [Fact]
        public void Adv01_NH_PartLevelNetWeight_MatchesExactBenchmark_AcrossPermutations()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nhItem = project.TenderItems.Single(t => t.ItemTag == "NH");
            var parts = nhItem.SubProjects.SelectMany(s => s.Parts).ToList();

            // 1. 校验零件总数为 14 件
            Assert.Equal(14, parts.Count);

            // 2. 基准常数精准等于 1,123,939.15 kg
            double exactTarget = NingboOlympicSeedData.BenchmarkNhNetWeightKg;
            Assert.Equal(1123939.15, exactTarget);

            // 3. 顺序求和
            double forwardSum = parts.Sum(p => p.TotalNetWeight);
            Assert.Equal(exactTarget, forwardSum, precision: 2);

            // 4. 逆序求和
            double reverseSum = parts.AsEnumerable().Reverse().Sum(p => p.TotalNetWeight);
            Assert.Equal(exactTarget, reverseSum, precision: 2);

            // 5. 5 次伪随机置换求和，验证浮点数相加顺序变化时漂移 < 1e-6 kg
            var rng = new Random(42);
            for (int i = 0; i < 5; i++)
            {
                var shuffled = parts.OrderBy(_ => rng.Next()).ToList();
                double shuffledSum = shuffled.Sum(p => p.TotalNetWeight);
                Assert.True(Math.Abs(shuffledSum - exactTarget) < 1e-6,
                    $"Permutation {i} resulted in floating-point drift: {shuffledSum} vs {exactTarget}");
            }

            // 6. 验证每个零件自身 SingleNetWeight * Quantity 与 TotalNetWeight 的一致性
            foreach (var part in parts)
            {
                if (part.Quantity > 1 && part.SingleNetWeight > 0)
                {
                    double calculatedTotal = part.Quantity * part.SingleNetWeight;
                    Assert.True(Math.Abs(calculatedTotal - part.TotalNetWeight) < 0.05,
                        $"Part {part.PartMark} drift: Qty({part.Quantity}) * Single({part.SingleNetWeight}) = {calculatedTotal} != Total({part.TotalNetWeight})");
                }
            }
        }

        [Fact]
        public void Adv02_NH_PartLevelSurfaceArea_MatchesExactBenchmark_AcrossPermutations()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nhItem = project.TenderItems.Single(t => t.ItemTag == "NH");
            var parts = nhItem.SubProjects.SelectMany(s => s.Parts).ToList();

            double exactAreaTarget = NingboOlympicSeedData.BenchmarkNhSurfaceAreaM2;
            Assert.Equal(5334.63, exactAreaTarget);

            // 1. 顺序求和
            double sumPartsArea = parts.Sum(p => p.TotalSurfaceAreaM2);
            Assert.Equal(exactAreaTarget, sumPartsArea, precision: 2);

            // 2. 构件层展开面积求和
            double sumSubProjectsArea = nhItem.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(exactAreaTarget, sumSubProjectsArea, precision: 2);

            // 3. 验证 7 大构件每个构件的面积都等于其零件面积之和
            foreach (var sub in nhItem.SubProjects)
            {
                double expectedSubArea = sub.Parts.Sum(p => p.TotalSurfaceAreaM2);
                Assert.Equal(expectedSubArea, sub.SubProjectSurfaceAreaM2, precision: 2);
            }

            // 4. 置换不变性验证
            var rng = new Random(101);
            for (int i = 0; i < 5; i++)
            {
                var shuffled = parts.OrderBy(_ => rng.Next()).ToList();
                double shuffledSum = shuffled.Sum(p => p.TotalSurfaceAreaM2);
                Assert.True(Math.Abs(shuffledSum - exactAreaTarget) < 1e-6,
                    $"Surface area permutation drift: {shuffledSum} vs {exactAreaTarget}");
            }
        }

        [Fact]
        public void Adv03_NH_GrossWeight_Exactness_And_RoundingInvariant()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nhItem = project.TenderItems.Single(t => t.ItemTag == "NH");
            var parts = nhItem.SubProjects.SelectMany(s => s.Parts).ToList();

            // 1,123,939.15 * 1.15 = 1,292,530.0225 kg
            double exactAnalyticalGrossKg = 1123939.15 * 1.15;
            Assert.Equal(1292530.0225, exactAnalyticalGrossKg, precision: 4);

            // 零件毛重求和
            double sumPartGrossKg = parts.Sum(p => p.TotalGrossWeight);
            Assert.Equal(exactAnalyticalGrossKg, sumPartGrossKg, precision: 4);

            // 构件毛重求和
            double sumSubGrossKg = nhItem.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(exactAnalyticalGrossKg, sumSubGrossKg, precision: 4);

            // 四舍五入到两位小数后严格等于 BenchmarkNhGrossWeightKg (1,292,530.02)
            Assert.Equal(NingboOlympicSeedData.BenchmarkNhGrossWeightKg, Math.Round(sumSubGrossKg, 2));
        }

        #endregion

        #region 2. 采购毛重定额系数缩放不变性 (Gross Weight & PurchasingWasteFactor Invariance)

        [Theory]
        [InlineData(1.00)]
        [InlineData(1.05)]
        [InlineData(1.10)]
        [InlineData(1.15)]
        [InlineData(1.20)]
        [InlineData(1.25)]
        [InlineData(1.50)]
        [InlineData(2.00)]
        public void Adv04_PurchasingWasteFactor_ScalesGrossWeightDeterministically(double wasteFactor)
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // 修改所有零件的采购损耗系数
            foreach (var part in project.TenderItems.SelectMany(t => t.SubProjects).SelectMany(s => s.Parts))
            {
                part.PurchasingWasteFactor = wasteFactor;
            }

            // 重新卷积计算大盘
            var result = BoqAggregationEngine.CalculateProject(project);

            // 校验全项目毛重严格等于净重乘以损耗系数
            double expectedGrossKg = result.TotalNetWeightKg * wasteFactor;
            Assert.Equal(expectedGrossKg, result.TotalGrossWeightKg, precision: 2);

            // 校验每个构件毛重严格等于净重乘以损耗系数
            foreach (var itemRes in result.TenderItemResults)
            {
                foreach (var subRes in itemRes.SubProjectResults)
                {
                    double expectedSubGross = subRes.TotalNetWeightKg * wasteFactor;
                    Assert.Equal(expectedSubGross, subRes.TotalGrossWeightKg, precision: 2);
                }
            }
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-0.15)]
        [InlineData(-1.0)]
        [InlineData(-99.9)]
        public void Adv05_PurchasingWasteFactor_InvalidValues_FallbackTo1_15Default(double invalidFactor)
        {
            var part = new BoqPart
            {
                PartMark = "P-TEST-INVALID",
                Quantity = 1,
                SingleNetWeight = 2000.0,
                TotalNetWeight = 2000.0,
                PurchasingWasteFactor = invalidFactor
            };

            BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(1.15, part.PurchasingWasteFactor);
            Assert.Equal(2300.0, part.TotalGrossWeight, precision: 2);
        }

        [Fact]
        public void Adv06_SubProjectWithoutParts_CalculatesGrossWeightDeterministically()
        {
            var sub = new BoqSubProject
            {
                SubProjectCode = "EMPTY-PARTS-SUB",
                SubProjectName = "虚拟无零件构件",
                SubProjectNetWeightKg = 10000.0,
                SubProjectGrossWeightKg = 0.0 // 未提供，引擎应依据 1.15 倍自动推导
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            Assert.Equal(10000.0, sub.SubProjectNetWeightKg);
            Assert.Equal(11500.0, sub.SubProjectGrossWeightKg, precision: 2);
            Assert.Equal(11500.0, res.TotalGrossWeightKg, precision: 2);
        }

        #endregion

        #region 3. 定额阶梯跃迁边界与严格单调性神谕 (Quota Ladder Transitions & Monotonicity Oracles)

        [Fact]
        public void Adv07_ThicknessFactor_BoundaryTransitions_MatchExactTiers()
        {
            // T <= 20mm: 1.00
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(-5.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(0.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(19.999));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(20.0));

            // 20 < T <= 30mm: 1.05
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(20.0001));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(25.0));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(30.0));

            // 30 < T <= 40mm: 1.12
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(30.0001));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(35.0));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(40.0));

            // 40 < T <= 60mm: 1.25
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(40.0001));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(50.0));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(60.0));

            // T > 60mm: 1.40
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(60.0001));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(80.0));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(200.0));
        }

        [Fact]
        public void Adv08_ThicknessFactor_StrictMonotonicity_Oracle()
        {
            // 敌手化神谕检验：厚度单调递增，难度调节系数必须单调不减
            double prevFactor = 0.0;
            for (double t = 0.0; t <= 120.0; t += 0.5)
            {
                double currentFactor = BoqAggregationEngine.CalculateThicknessFactor(t);
                Assert.True(currentFactor >= prevFactor,
                    $"Monotonicity violated at thickness {t}mm: {currentFactor} < {prevFactor}");
                prevFactor = currentFactor;
            }
        }

        [Fact]
        public void Adv09_BenchmarkComponents_MaxThickness_MatchesExpectedTiers()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nhItem = project.TenderItems.Single(t => t.ItemTag == "NH");

            // N-NHXX-48 (16mm -> 1.00)
            var sub48 = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-48");
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(sub48.MaxPlateThickness));

            // N-NHXX-49 (50mm -> 1.25)
            var sub49 = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-49");
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(sub49.MaxPlateThickness));

            // N-NHXX-50 (40mm boundary -> 1.12)
            var sub50 = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NHXX-50");
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(sub50.MaxPlateThickness));

            // N-NH-CHORD-MAIN (50mm -> 1.25)
            var subChord = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NH-CHORD-MAIN");
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(subChord.MaxPlateThickness));

            // N-NH-WEB-DIAG (25mm -> 1.05)
            var subDiag = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NH-WEB-DIAG");
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(subDiag.MaxPlateThickness));

            // N-NH-WEB-VERT (30mm boundary -> 1.05)
            var subVert = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NH-WEB-VERT");
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(subVert.MaxPlateThickness));

            // N-NH-NODE-GUSSET (60mm boundary -> 1.25)
            var subNode = nhItem.SubProjects.Single(s => s.SubProjectCode == "N-NH-NODE-GUSSET");
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(subNode.MaxPlateThickness));
        }

        [Fact]
        public void Adv10_ElevationFactor_BoundaryTransitions_And_Monotonicity()
        {
            // H <= 20: 1.00
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(0.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(19.999));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(20.0));

            // 20 < H <= 35: 1.15
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(20.0001));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(30.0));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(35.0));

            // H > 35: 1.30
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(35.0001));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(50.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(100.0));

            // 严格单调性
            double prevElev = 0.0;
            for (double h = 0.0; h <= 80.0; h += 0.5)
            {
                double currentElev = BoqAggregationEngine.CalculateElevationFactor(h);
                Assert.True(currentElev >= prevElev, $"Elevation monotonicity violated at {h}m");
                prevElev = currentElev;
            }
        }

        [Theory]
        [InlineData("<5t", 1.00)]
        [InlineData(" <5T ", 1.00)]
        [InlineData("5-15t", 1.20)]
        [InlineData("5~15T", 1.20)]
        [InlineData("15-25t", 1.30)]
        [InlineData("15~25t", 1.30)]
        [InlineData(">25t", 1.45)]
        [InlineData(" >25 T ", 1.45)]
        public void Adv11_WeightFactor_TierParsing_HandlesFormatVariations(string tier, double expectedFactor)
        {
            Assert.Equal(expectedFactor, BoqAggregationEngine.CalculateWeightFactorFromTier(tier));
        }

        [Theory]
        [InlineData(0.0, 1.00)]
        [InlineData(4.999, 1.00)]
        [InlineData(5.0, 1.20)]
        [InlineData(14.999, 1.20)]
        [InlineData(15.0, 1.20)]
        [InlineData(15.0001, 1.30)]
        [InlineData(24.999, 1.30)]
        [InlineData(25.0, 1.30)]
        [InlineData(25.0001, 1.45)]
        [InlineData(100.0, 1.45)]
        public void Adv12_WeightFactor_FallbackPieceWeight_BoundaryTransitions(double pieceWeight, double expectedFactor)
        {
            // 空 tier 或未知 tier 触发单件重量判定
            Assert.Equal(expectedFactor, BoqAggregationEngine.CalculateWeightFactorFromTier(null, pieceWeight));
            Assert.Equal(expectedFactor, BoqAggregationEngine.CalculateWeightFactorFromTier("", pieceWeight));
            Assert.Equal(expectedFactor, BoqAggregationEngine.CalculateWeightFactorFromTier("CUSTOM_TIER", pieceWeight));
            Assert.Equal(expectedFactor, BoqAggregationEngine.CalculateWeightFactor(pieceWeight));
        }

        [Fact]
        public void Adv13_PartThicknessSurcharge_BoundaryTransitions_And_Monotonicity()
        {
            // T <= 20: 0
            Assert.Equal(0.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(10.0));
            Assert.Equal(0.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(20.0));

            // 20 < T <= 30: 80
            Assert.Equal(80.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(20.0001));
            Assert.Equal(80.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(30.0));

            // 30 < T <= 40: 180
            Assert.Equal(180.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(30.0001));
            Assert.Equal(180.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(40.0));

            // 40 < T <= 50: 350
            Assert.Equal(350.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(40.0001));
            Assert.Equal(350.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(50.0));

            // 50 < T <= 60: 500
            Assert.Equal(500.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(50.0001));
            Assert.Equal(500.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(60.0));

            // T > 60: 750
            Assert.Equal(750.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(60.0001));
            Assert.Equal(750.0, BoqAggregationEngine.CalculatePartThicknessSurcharge(100.0));

            // 单调性
            double prevSur = 0.0;
            for (double t = 0.0; t <= 100.0; t += 1.0)
            {
                double cur = BoqAggregationEngine.CalculatePartThicknessSurcharge(t);
                Assert.True(cur >= prevSur, $"Part thickness surcharge monotonicity violated at {t}mm");
                prevSur = cur;
            }
        }

        #endregion

        #region 4. 耐火极限涂装排程与费用核算 (Fire Rating Cost Calculations)

        [Theory]
        [InlineData(4.0, 145.0)]
        [InlineData(3.0, 145.0)]
        [InlineData(2.99, 110.0)]
        [InlineData(2.5, 110.0)]
        [InlineData(2.0, 110.0)]
        [InlineData(1.99, 75.0)]
        [InlineData(1.5, 75.0)]
        [InlineData(1.0, 75.0)]
        [InlineData(0.5, 75.0)]
        [InlineData(0.0, 75.0)] // 保底 75.0 元/m2
        public void Adv14_FireRatingRate_BoundaryTransitions_MatchSpecification(double hours, double expectedRate)
        {
            Assert.Equal(expectedRate, BoqAggregationEngine.CalculateFireRatingRate(hours));
        }

        [Fact]
        public void Adv15_BenchmarkFireCoating_Totals_ReconcileDeterministically()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // NH: 5,334.63 m2 @ 145.0 = 773,521.35 元
            var nhRes = result.TenderItemResults.Single(t => t.ItemTag == "NH");
            double nhFireCost = nhRes.SubProjectResults.Sum(s => s.FireCoatingCost);
            Assert.Equal(5334.63 * 145.0, nhFireCost, precision: 2);
            Assert.Equal(773521.35, nhFireCost, precision: 2);

            // NF: 4,720.00 m2 @ 110.0 = 519,200.00 元
            var nfRes = result.TenderItemResults.Single(t => t.ItemTag == "NF");
            double nfFireCost = nfRes.SubProjectResults.Sum(s => s.FireCoatingCost);
            Assert.Equal(4720.00 * 110.0, nfFireCost, precision: 2);
            Assert.Equal(519200.00, nfFireCost, precision: 2);

            // JLC: 7,850.00 m2 @ 75.0 = 588,750.00 元
            var jlcRes = result.TenderItemResults.Single(t => t.ItemTag == "JLC");
            double jlcFireCost = jlcRes.SubProjectResults.Sum(s => s.FireCoatingCost);
            Assert.Equal(7850.00 * 75.0, jlcFireCost, precision: 2);
            Assert.Equal(588750.00, jlcFireCost, precision: 2);

            // 全项目防火涂装总费用 = 773521.35 + 519200.00 + 588750.00 = 1,881,471.35 元
            Assert.Equal(1881471.35, result.TotalFireCoatingCost, precision: 2);
        }

        #endregion

        #region 5. 双层费率覆盖与对外商务组价 (Fee Rate Overrides & Commercial Markup)

        [Fact]
        public void Adv16_FeeOverrides_CompoundingMultiplier_And_Independence()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // 1. NH 单项覆盖: 10% 管理费, 12% 利润 => (1 + 0.10) * (1 + 0.12) = 1.2320
            var itemNH = project.TenderItems.Single(t => t.ItemTag == "NH");
            Assert.True(itemNH.IsCustomRateEnabled);
            var (nhM, nhP) = itemNH.GetEffectiveRates(project);
            Assert.Equal(0.10, nhM);
            Assert.Equal(0.12, nhP);
            Assert.Equal(1.2320, (1.0 + nhM) * (1.0 + nhP), precision: 4);

            // 2. NF 继承默认: 8% 管理费, 7% 利润 => (1 + 0.08) * (1 + 0.07) = 1.1556
            var itemNF = project.TenderItems.Single(t => t.ItemTag == "NF");
            Assert.False(itemNF.IsCustomRateEnabled);
            var (nfM, nfP) = itemNF.GetEffectiveRates(project);
            Assert.Equal(0.08, nfM);
            Assert.Equal(0.07, nfP);
            Assert.Equal(1.1556, (1.0 + nfM) * (1.0 + nfP), precision: 4);

            // 3. JLC 降价覆盖: 5% 管理费, 4% 利润 => (1 + 0.05) * (1 + 0.04) = 1.0920
            var itemJLC = project.TenderItems.Single(t => t.ItemTag == "JLC");
            Assert.True(itemJLC.IsCustomRateEnabled);
            var (jlcM, jlcP) = itemJLC.GetEffectiveRates(project);
            Assert.Equal(0.05, jlcM);
            Assert.Equal(0.04, jlcP);
            Assert.Equal(1.0920, (1.0 + jlcM) * (1.0 + jlcP), precision: 4);

            // 4. 动态改变全局默认费率，验证 NH 与 JLC 保持锁定不变，NF 自动跟随变动
            project.DefaultManagementFeeRate = 0.09;
            project.DefaultProfitRate = 0.08;

            var (nhM2, nhP2) = itemNH.GetEffectiveRates(project);
            Assert.Equal(0.10, nhM2); // 覆盖项保持不变
            Assert.Equal(0.12, nhP2);

            var (nfM2, nfP2) = itemNF.GetEffectiveRates(project);
            Assert.Equal(0.09, nfM2); // 继承项跟随改变
            Assert.Equal(0.08, nfP2);
        }

        #endregion

        #region 6. 五大受众投影跨视角守恒对账 (Audience Projections Reconciliation)

        [Fact]
        public void Adv17_AudienceProjections_ClientMaster_And_UltimateCost_Totals_Reconcile()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            var clientReport = BoqAggregationEngine.GenerateAudienceReport(project,
                new BoqReportConfig { AudiencePerspective = BoqAudiencePerspective.Owner });

            var ultimateReport = BoqAggregationEngine.GenerateAudienceReport(project,
                new BoqReportConfig { AudiencePerspective = BoqAudiencePerspective.ChiefEconomist });

            // 1. 顶层大盘总投标报价对账
            double clientTotalBid = Convert.ToDouble(clientReport.SummaryMetrics["总投标报价(元)"]);
            double ultimateTotalBid = Convert.ToDouble(ultimateReport.SummaryMetrics["全项目总投标报价(元)"]);
            Assert.Equal(clientTotalBid, ultimateTotalBid, precision: 2);

            // 2. 逐行合价对账
            Assert.NotNull(clientReport.ClientTenderRows);
            Assert.NotNull(ultimateReport.UltimateCostRows);
            Assert.Equal(clientReport.ClientTenderRows!.Count, ultimateReport.UltimateCostRows!.Count);
            for (int i = 0; i < clientReport.ClientTenderRows!.Count; i++)
            {
                var cRow = clientReport.ClientTenderRows[i];
                var uRow = ultimateReport.UltimateCostRows[i];

                Assert.Equal(cRow.ItemTag, uRow.ItemTag);
                Assert.Equal(cRow.TenderQuantity, uRow.TenderQuantity);
                Assert.Equal(cRow.ComprehensiveUnitPrice, uRow.ComprehensiveUnitPrice, precision: 2);
                Assert.Equal(cRow.TotalTenderPrice, uRow.TotalTenderPrice, precision: 2);
            }

            // 3. 业主视图严格脱敏检查：绝对不包含底价与定额加价字段
            var clientHeaders = clientReport.Headers;
            Assert.DoesNotContain(clientHeaders, h => h.Contains("直接成本"));
            Assert.DoesNotContain(clientHeaders, h => h.Contains("底价"));
            Assert.DoesNotContain(clientHeaders, h => h.Contains("剪刀差"));
            Assert.DoesNotContain(clientHeaders, h => h.Contains("利润率"));
            Assert.DoesNotContain(clientHeaders, h => h.Contains("制作工费"));
        }

        [Fact]
        public void Adv18_AudienceProjections_PhysicalWeights_ReconcileAcrossAll5Views()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var pResult = BoqAggregationEngine.CalculateProject(project);

            var rOwner = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.Owner, project);
            var rEcon = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.ChiefEconomist, project);
            var rFab = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.FabricationShop, project);
            var rSite = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.SiteProjectDept, project);
            var rMat = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.Procurement, project);

            // 1. 顶层总净重：2,717,439.15 kg (2717.439 t)
            double expectedTotalNetTon = (1123939.15 + 865200.00 + 728300.00) / 1000.0;
            Assert.Equal(expectedTotalNetTon, pResult.TotalNetWeightTon, precision: 3);

            // 2. 总经济师视图理论净重总和
            Assert.NotNull(rEcon.UltimateCostRows);
            double econTotalNetTon = rEcon.UltimateCostRows!.Sum(r => r.NetWeightTon);
            Assert.Equal(expectedTotalNetTon, econTotalNetTon, precision: 3);

            // 3. 车间制作视图净重总和 (13 个构件)
            Assert.NotNull(rFab.FabricationRows);
            double fabTotalNetTon = rFab.FabricationRows!.Sum(r => r.TotalNetWeightTon);
            Assert.Equal(expectedTotalNetTon, fabTotalNetTon, precision: 3);

            // 4. 现场安装视图净重总和 (13 个构件)
            Assert.NotNull(rSite.SiteErectionRows);
            double siteTotalNetTon = rSite.SiteErectionRows!.Sum(r => r.TotalNetWeightTon);
            Assert.Equal(expectedTotalNetTon, siteTotalNetTon, precision: 3);

            // 5. 物资采办视图净重总和 (按材质规格归并后的零件)
            Assert.NotNull(rMat.MaterialProcurementRows);
            double matTotalNetTon = rMat.MaterialProcurementRows!.Sum(r => r.TotalNetWeightTon);
            Assert.Equal(expectedTotalNetTon, matTotalNetTon, precision: 3);

            // 6. 物资采办视图 1.15 毛重总和 (行级独立截断至3位小数产生的累加舍入容差在 0.001t 即 1kg 以内)
            double matTotalGrossTon = rMat.MaterialProcurementRows!.Sum(r => r.TotalGrossWeightTon);
            double expectedGrossTon = pResult.TotalGrossWeightKg / 1000.0;
            Assert.True(Math.Abs(expectedGrossTon - matTotalGrossTon) <= 0.002,
                $"Gross ton mismatch: expected {expectedGrossTon}, got {matTotalGrossTon}");

            double matTotalGrossKg = rMat.MaterialProcurementRows!.Sum(r => r.TotalGrossWeightKg);
            Assert.Equal(pResult.TotalGrossWeightKg, matTotalGrossKg, precision: 2);
        }

        [Fact]
        public void Adv19_CostMetrics_ReconcileAcrossPerspectives()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var pResult = BoqAggregationEngine.CalculateProject(project);

            var rEcon = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.ChiefEconomist, project);
            var rFab = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.FabricationShop, project);
            var rSite = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.SiteProjectDept, project);

            // 车间工费跨受众对账：车间制作视图总额 (13行独立四舍五入求和) 与 决策层穿透总额 (全局四舍五入) 偏差在 0.02元 (2分钱) 以内
            double econFabCost = Convert.ToDouble(rEcon.SummaryMetrics["车间制作工费总额(元)"]);
            double shopFabCost = Convert.ToDouble(rFab.SummaryMetrics["车间制作工费总额(元)"]);
            Assert.True(Math.Abs(econFabCost - shopFabCost) <= 0.02,
                $"Fabrication cost mismatch: econ {econFabCost} vs shop {shopFabCost}");
            Assert.True(Math.Abs(pResult.TotalFabricationCost - shopFabCost) <= 0.02,
                $"Fabrication cost vs project result mismatch: {pResult.TotalFabricationCost} vs {shopFabCost}");

            // 现场安装跨受众对账：现场安装视图总额 与 决策层穿透总额 偏差在 0.02元 以内
            double econSiteCost = Convert.ToDouble(rEcon.SummaryMetrics["现场安装工费总额(元)"]);
            double siteInstallCost = Convert.ToDouble(rSite.SummaryMetrics["现场安装总工费(元)"]);
            Assert.True(Math.Abs(econSiteCost - siteInstallCost) <= 0.02,
                $"Installation cost mismatch: econ {econSiteCost} vs site {siteInstallCost}");
            Assert.True(Math.Abs(pResult.TotalInstallationCost - siteInstallCost) <= 0.02,
                $"Installation cost vs project result mismatch: {pResult.TotalInstallationCost} vs {siteInstallCost}");
        }

        [Fact]
        public void Adv20_ScissorDifference_ReconcilesBetweenEconomistAndProcurement()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var pResult = BoqAggregationEngine.CalculateProject(project);

            var rEcon = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.ChiefEconomist, project);
            var rMat = BoqAggregationEngine.ProjectToAudience(pResult, BoqAudiencePerspective.Procurement, project);

            double totalScissorProject = pResult.TotalProcurementScissorProfit;
            Assert.NotNull(rEcon.UltimateCostRows);
            Assert.NotNull(rMat.MaterialProcurementRows);
            double econSumScissor = rEcon.UltimateCostRows!.Sum(r => r.ScissorDifferenceProfit);
            double matSumScissor = rMat.MaterialProcurementRows!.Sum(r => r.ScissorDifferenceProfit);

            Assert.Equal(totalScissorProject, econSumScissor, precision: 2);
            Assert.Equal(totalScissorProject, matSumScissor, precision: 2);
        }

        #endregion

        #region 7. 幂等性与极限重算压力 (Idempotence & Calculation Stability)

        [Fact]
        public void Adv20_CalculateProject_100ConsecutiveIterations_ZeroDrift()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // 首次计算并记录基准数字
            var baseline = BoqAggregationEngine.CalculateProject(project);
            double baseBid = baseline.TotalBidPrice;
            double baseDirect = baseline.TotalDirectCost;
            double baseNetKg = baseline.TotalNetWeightKg;
            double baseGrossKg = baseline.TotalGrossWeightKg;
            double baseAreaM2 = baseline.TotalSurfaceAreaM2;

            // 连续重算 100 次，验证完全幂等且无累加或浮点漂移
            for (int i = 0; i < 100; i++)
            {
                var run = BoqAggregationEngine.CalculateProject(project);
                Assert.Equal(baseBid, run.TotalBidPrice, precision: 2);
                Assert.Equal(baseDirect, run.TotalDirectCost, precision: 2);
                Assert.Equal(baseNetKg, run.TotalNetWeightKg, precision: 2);
                Assert.Equal(baseGrossKg, run.TotalGrossWeightKg, precision: 2);
                Assert.Equal(baseAreaM2, run.TotalSurfaceAreaM2, precision: 2);
            }
        }

        [Fact]
        public void Adv21_DatabaseSeeder_MultipleConsecutiveSeeds_IsIdempotent()
        {
            using var db = CreateContext();
            db.Database.EnsureCreated();

            // 连续执行 5 次种子加载
            for (int i = 0; i < 5; i++)
            {
                NingboOlympicDataSeeder.Seed(db);
            }

            // 数据库内必须且仅有 1 条项目记录
            Assert.Equal(1, db.BoqProjects.Count(p => p.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode));

            // 清单项恰好为 3 项
            Assert.Equal(3, db.BoqTenderItems.Count(t => t.Project!.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode));

            // 构件恰好为 13 项 (7 NH + 3 NF + 3 JLC)
            Assert.Equal(13, db.BoqSubProjects.Count(s => s.TenderItem!.Project!.ProjectCode == NingboOlympicSeedData.BenchmarkProjectCode));

            // 零件恰好为 20 项 (14 NH + 3 NF + 3 JLC)
            Assert.Equal(20, db.BoqParts.Count());
        }

        #endregion

        #region 8. 极端与边界防崩溃安全测试 (Defensive Robustness & Null Handling)

        [Fact]
        public void Adv22_ProjectToAudience_NullProject_DoesNotThrow()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var pResult = BoqAggregationEngine.CalculateProject(project);

            // 当 project 为 null 时，所有 5 个受众投影应安全回退并输出，绝不抛出 NullReferenceException
            foreach (BoqAudiencePerspective perspective in Enum.GetValues(typeof(BoqAudiencePerspective)))
            {
                var report = BoqAggregationEngine.ProjectToAudience(pResult, perspective, project: null);
                Assert.NotNull(report);
                Assert.NotNull(report.Headers);
                Assert.NotNull(report.Rows);
                Assert.NotNull(report.SummaryMetrics);
            }
        }

        [Fact]
        public void Adv23_ZeroQuantityTenderItem_DoesNotDivideByZero()
        {
            var item = new BoqTenderItem
            {
                ItemTag = "ZERO_QTY",
                BillItemCode = "000000",
                BillItemName = "零工程量清单项",
                TenderQuantity = 0.0
            };

            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-ZERO",
                SubProjectName = "零工程量子项",
                SubProjectNetWeightKg = 0.0
            };
            item.SubProjects.Add(sub);

            var res = BoqAggregationEngine.CalculateTenderItem(item);

            Assert.False(double.IsNaN(res.DirectCostPerTon));
            Assert.False(double.IsInfinity(res.DirectCostPerTon));
            Assert.False(double.IsNaN(res.ComprehensiveUnitPrice));
            Assert.False(double.IsInfinity(res.ComprehensiveUnitPrice));
            Assert.False(double.IsNaN(res.TotalTenderPrice));
            Assert.Equal(0.0, res.TotalTenderPrice);
        }

        #endregion
    }
}
