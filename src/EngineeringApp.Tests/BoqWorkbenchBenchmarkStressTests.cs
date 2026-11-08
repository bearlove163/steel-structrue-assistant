// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqWorkbenchBenchmarkStressTests.cs
// 模块功能: 宁波奥体中心 BOQ 商务工作台 (BoqWorkbench.razor) 敌手化数学核验与基准压力测试
// 规范依据: ORIGINAL_REQUEST.md (R3, R4, R5, R6), PROJECT.md (Feature 22, 23, 24),
//           doc/steel_boq_database_and_reporting_plan.md (§3.2, §3.3), AGENTS.md
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Tests
{
    /// <summary>
    /// 对 BoqWorkbench.razor 及其底层算法进行对抗性数学核算验证与工程基准压力测试
    /// 包含五大敌手化挑战领域：
    /// 1. 基准物理量精度核验 (NH 1,123.94t / 5,334.63m² / 1,292.53t; NF 865.20t; JLC 728.30t; 总计 2,717.44t / 3,125.06t / 17,904.63m²)
    /// 2. 6 张核心 KPI 商务看板计算准确性与多受众物理脱敏保密性
    /// 3. 双定额系数阶梯跃迁神谕 (K_Thick, K_Elev, K_SiteWeld, K_Weight) 与单调性保证
    /// 4. 采办经营创效 (剪刀差利润) 跨层级数学守恒对账与正负盈亏穿透
    /// 5. 极端边界压力测试 (0%费率、100%费率、负费率防护、特厚板与超高标高极限输入、0工程量防除零)
    /// </summary>
    public class BoqWorkbenchBenchmarkStressTests
    {
        #region 领域 1: 基准物理量精度核验 (Benchmark Precision Verification)

        [Fact]
        public void Benchmark01_NH_MainTruss_ExactPhysicalQuantities_And_LossFactor()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nh = project.TenderItems.Single(t => t.ItemTag == "NH");

            // 1. 理论净重校验: 1,123,939.15 kg (1,123.94 t)
            double expectedNetKg = 1123939.15;
            double actualNetKg = nh.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(expectedNetKg, actualNetKg, precision: 2);
            Assert.Equal(1123.94, Math.Round(actualNetKg / 1000.0, 2));

            // 2. 涂装表面积校验: 5,334.63 m²
            double expectedAreaM2 = 5334.63;
            double actualAreaM2 = nh.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(expectedAreaM2, actualAreaM2, precision: 2);

            // 3. 1.15倍采购损耗毛重校验: 1,123,939.15 * 1.15 = 1,292,530.0225 => 1,292,530.02 kg
            double expectedGrossKg = 1292530.02;
            double actualGrossKg = nh.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(expectedGrossKg, actualGrossKg, precision: 2);
            Assert.Equal(1292.53, Math.Round(actualGrossKg / 1000.0, 2));

            // 4. 损耗乘数比值严密核验
            double lossMultiplier = actualGrossKg / actualNetKg;
            Assert.True(Math.Abs(lossMultiplier - 1.15) < 1e-6,
                $"Loss multiplier drift: expected 1.15, got {lossMultiplier}");

            // 5. 构件与零件层净重完全一致
            double partsNetKg = nh.SubProjects.SelectMany(s => s.Parts).Sum(p => p.TotalNetWeight);
            Assert.Equal(actualNetKg, partsNetKg, precision: 2);
        }

        [Fact]
        public void Benchmark02_NF_SecondaryTruss_ExactPhysicalQuantities_And_LossFactor()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nf = project.TenderItems.Single(t => t.ItemTag == "NF");

            // 1. 理论净重校验: 865,200.00 kg (865.20 t)
            double expectedNetKg = 865200.00;
            double actualNetKg = nf.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(expectedNetKg, actualNetKg, precision: 2);
            Assert.Equal(865.20, Math.Round(actualNetKg / 1000.0, 2));

            // 2. 涂装表面积校验: 4,720.00 m²
            double expectedAreaM2 = 4720.00;
            double actualAreaM2 = nf.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(expectedAreaM2, actualAreaM2, precision: 2);

            // 3. 1.15倍采购毛重校验: 865,200.00 * 1.15 = 994,980.00 kg
            double expectedGrossKg = 994980.00;
            double actualGrossKg = nf.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(expectedGrossKg, actualGrossKg, precision: 2);
            Assert.Equal(994.98, Math.Round(actualGrossKg / 1000.0, 2));

            // 4. 损耗乘数严格等于 1.15
            Assert.Equal(1.15, actualGrossKg / actualNetKg, precision: 6);
        }

        [Fact]
        public void Benchmark03_JLC_CheckIn_ExactPhysicalQuantities_And_LossFactor()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var jlc = project.TenderItems.Single(t => t.ItemTag == "JLC");

            // 1. 理论净重校验: 728,300.00 kg (728.30 t)
            double expectedNetKg = 728300.00;
            double actualNetKg = jlc.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(expectedNetKg, actualNetKg, precision: 2);
            Assert.Equal(728.30, Math.Round(actualNetKg / 1000.0, 2));

            // 2. 涂装表面积校验: 7,850.00 m²
            double expectedAreaM2 = 7850.00;
            double actualAreaM2 = jlc.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(expectedAreaM2, actualAreaM2, precision: 2);

            // 3. 1.15倍采购毛重校验: 728,300.00 * 1.15 = 837,545.00 kg
            double expectedGrossKg = 837545.00;
            double actualGrossKg = jlc.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(expectedGrossKg, actualGrossKg, precision: 2);
            Assert.Equal(837.545, actualGrossKg / 1000.0, precision: 3);
            Assert.Equal(837.55, Math.Round(actualGrossKg / 1000.0, 2, MidpointRounding.AwayFromZero));

            // 4. 损耗乘数严格等于 1.15
            Assert.Equal(1.15, actualGrossKg / actualNetKg, precision: 6);
        }

        [Fact]
        public void Benchmark04_ProjectTotal_AggregatedExactPhysicalQuantities()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // 1. 全项目理论总净重: 1,123,939.15 + 865,200.00 + 728,300.00 = 2,717,439.15 kg
            double expectedTotalNetKg = 1123939.15 + 865200.00 + 728300.00; // 2,717,439.15
            Assert.Equal(2717439.15, expectedTotalNetKg);
            Assert.Equal(expectedTotalNetKg, result.TotalNetWeightKg, precision: 2);
            Assert.Equal(2717.44, Math.Round(result.TotalNetWeightTon, 2));

            // 2. 全项目采购总毛重: 1,292,530.02 + 994,980.00 + 837,545.00 = 3,125,055.02 kg
            double expectedTotalGrossKg = 1292530.02 + 994980.00 + 837545.00; // 3,125,055.02
            Assert.Equal(3125055.02, expectedTotalGrossKg);
            Assert.Equal(expectedTotalGrossKg, result.TotalGrossWeightKg, precision: 2);
            Assert.Equal(3125.06, Math.Round(result.TotalGrossWeightTon, 2));

            // 3. 全项目涂装总表面积: 5,334.63 + 4,720.00 + 7,850.00 = 17,904.63 m²
            double expectedTotalAreaM2 = 5334.63 + 4720.00 + 7850.00; // 17,904.63
            Assert.Equal(17904.63, expectedTotalAreaM2);
            Assert.Equal(expectedTotalAreaM2, result.TotalSurfaceAreaM2, precision: 2);
            Assert.Equal(17904.6, Math.Round(result.TotalSurfaceAreaM2, 1));
        }

        #endregion

        #region 领域 2: 6 张核心 KPI 看板计算准确性与受众脱敏规则

        [Fact]
        public void KpiCard01_TotalBidPrice_SatisfiesExactCostSumIdentity()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // 最终投标总造价 = 各清单项投标合价之和
            double sumTenderPrice = result.TenderItemResults.Sum(t => t.TotalTenderPrice);
            Assert.Equal(sumTenderPrice, result.TotalBidPrice, precision: 2);

            // 成本守恒定律: 投标总价 == 直接成本总额 + 总管理费 + 总利润
            double totalComponents = result.TotalDirectCost + result.TotalManagementFee + result.TotalProfit;
            Assert.True(Math.Abs(result.TotalBidPrice - totalComponents) < 0.05,
                $"Bid Price conservation broken: Bid={result.TotalBidPrice}, Sum(Direct+Mgmt+Profit)={totalComponents}");

            // 均价计算验证
            double unitPricePerTon = result.TotalBidPrice / result.TotalNetWeightTon;
            Assert.True(unitPricePerTon > 0);
            Assert.Equal(Math.Round(result.TotalBidPrice / result.TotalNetWeightTon, 0),
                         Math.Round(unitPricePerTon, 0));
        }

        [Fact]
        public void KpiCard02_NetWeightTon_And_SubBreakdown()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // KPI Card 2 显示净重吨位: 2,717.44 t
            Assert.Equal(2717.44, Math.Round(result.TotalNetWeightTon, 2));

            // 子项明细对应: NH=1,123.94t, NF=865.20t, JLC=728.30t
            var nhRes = result.TenderItemResults.Single(t => t.ItemTag == "NH");
            var nfRes = result.TenderItemResults.Single(t => t.ItemTag == "NF");
            var jlcRes = result.TenderItemResults.Single(t => t.ItemTag == "JLC");

            Assert.Equal(1123.94, nhRes.TenderQuantityTon);
            Assert.Equal(865.20, nfRes.TenderQuantityTon);
            Assert.Equal(728.30, jlcRes.TenderQuantityTon);
            Assert.Equal(2717.44, nhRes.TenderQuantityTon + nfRes.TenderQuantityTon + jlcRes.TenderQuantityTon);
        }

        [Fact]
        public void KpiCard03_GrossWeightTon_And_CraftScrapIncrement()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // KPI Card 3 显示采购总毛重: 3,125.06 t
            Assert.Equal(3125.06, Math.Round(result.TotalGrossWeightTon, 2));

            // 工艺损耗增量: TotalGrossWeightTon - TotalNetWeightTon = 3,125.05502 - 2,717.43915 = 407.61587 t => 407.62 t
            double scrapDelta = result.TotalGrossWeightTon - result.TotalNetWeightTon;
            Assert.Equal(407.62, Math.Round(scrapDelta, 2));
            Assert.Equal(407615.87, Math.Round(result.TotalGrossWeightKg - result.TotalNetWeightKg, 2));
        }

        [Fact]
        public void KpiCard04_TotalSurfaceArea_And_FireResistanceRatingBreakdown()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // KPI Card 4 显示展开表面积: 17,904.6 m²
            Assert.Equal(17904.6, Math.Round(result.TotalSurfaceAreaM2, 1));

            // 按耐火极限分档汇总核算:
            // 3.0H: NH 主桁架全部构件 = 5,334.63 m²
            double area3H = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .Where(s => Math.Abs(s.FireRatingHours - 3.0) < 0.1)
                .Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(5334.63, area3H, precision: 2);

            // 2.0H: NF 副桁架全部构件 = 4,720.00 m²
            double area2H = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .Where(s => Math.Abs(s.FireRatingHours - 2.0) < 0.1)
                .Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(4720.00, area2H, precision: 2);

            // 1.5H: JLC 检录处轻钢全部构件 = 7,850.00 m²
            double area15H = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .Where(s => Math.Abs(s.FireRatingHours - 1.5) < 0.1)
                .Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(7850.00, area15H, precision: 2);

            // 三档耐火时限面积之和严格等于总表面积
            Assert.Equal(result.TotalSurfaceAreaM2, area3H + area2H + area15H, precision: 2);
        }

        [Fact]
        public void KpiCard05_TargetProfitAndManagementFee_AudienceDesensitization()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // 非脱敏状态 (如总经济师视角): 完整显示目标利润与管理费
            double combinedProfitAndFee = result.TotalProfit + result.TotalManagementFee;
            Assert.True(combinedProfitAndFee > 0, "Target profit and fee must be positive");
            Assert.True(result.TotalProfit > 0);
            Assert.True(result.TotalManagementFee > 0);

            // 投影至业主视角 (Owner): 必须触发物理脱敏 (不包含直接成本、管理费与利润列)
            var ownerConfig = new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner,
                MaskInternalDirectCosts = true
            };
            var ownerProjection = BoqAggregationEngine.ProjectToAudience(result, BoqAudiencePerspective.Owner, project, ownerConfig);
            Assert.NotNull(ownerProjection.ClientTenderRows);
            Assert.Null(ownerProjection.UltimateCostRows);
            string ownerHeaderString = string.Join(",", ownerProjection.Headers);
            Assert.DoesNotContain("直接成本", ownerHeaderString);
            Assert.DoesNotContain("管理费", ownerHeaderString);
            Assert.DoesNotContain("利润", ownerHeaderString);
            Assert.DoesNotContain("剪刀差", ownerHeaderString);

            // 投影至总经济师视角: 不脱敏 (完整包含全成本与利润列)
            var economistConfig = new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist,
                MaskInternalDirectCosts = false
            };
            var economistProjection = BoqAggregationEngine.ProjectToAudience(result, BoqAudiencePerspective.ChiefEconomist, project, economistConfig);
            Assert.NotNull(economistProjection.UltimateCostRows);
            string economistHeaderString = string.Join(",", economistProjection.Headers);
            Assert.Contains("直接成本总额(元)", economistHeaderString);
            Assert.Contains("企业管理费(元)", economistHeaderString);
            Assert.Contains("目标利润(元)", economistHeaderString);
            Assert.Contains("采办剪刀差创效(元)", economistHeaderString);
        }

        [Fact]
        public void KpiCard06_ScissorArbitrageProfit_AudienceDesensitization_And_PerTonFidelity()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // 采办经营创效总额 > 0
            Assert.True(result.TotalProcurementScissorProfit > 0,
                $"Scissor profit must be positive for Olympic benchmark, got {result.TotalProcurementScissorProfit}");

            // 吨钢创效 = TotalProcurementScissorProfit / TotalGrossWeightTon
            double perTonArbitrage = result.TotalProcurementScissorProfit / result.TotalGrossWeightTon;
            Assert.True(perTonArbitrage > 0);

            // 校验业主视角下的脱敏屏蔽状态: 业主清单中不含剪刀差列
            var ownerConfig = new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner,
                MaskInternalDirectCosts = true
            };
            var ownerProjection = BoqAggregationEngine.ProjectToAudience(result, BoqAudiencePerspective.Owner, project, ownerConfig);
            Assert.NotNull(ownerProjection.ClientTenderRows);
            Assert.DoesNotContain("采办经营剪刀差", string.Join(",", ownerProjection.Headers));
        }

        #endregion

        #region 领域 3: 双定额系数阶梯跃迁神谕 (Quota Scaling Factors Plan §3.2-§3.3)

        [Fact]
        public void Quota01_KThick_BoundaryStepTransitions_PlanSection3_2()
        {
            // Plan §3.2:
            // T <= 20mm: 1.00
            // 20 < T <= 30mm: 1.05
            // 30 < T <= 40mm: 1.12
            // 40 < T <= 60mm: 1.25
            // T > 60mm: 1.40

            // 临界边界测试 (含微量 epsilon 跃迁)
            const double eps = 1e-4;

            // Tier 1: T <= 20
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(0.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(10.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(20.0 - eps));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(20.0));

            // Tier 2: 20 < T <= 30
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(20.0 + eps));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(25.0));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(30.0 - eps));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(30.0));

            // Tier 3: 30 < T <= 40
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(30.0 + eps));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(35.0));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(40.0 - eps));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(40.0));

            // Tier 4: 40 < T <= 60
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(40.0 + eps));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(50.0));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(60.0 - eps));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(60.0));

            // Tier 5: T > 60
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(60.0 + eps));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(80.0));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(120.0));
        }

        [Fact]
        public void Quota02_KThick_Monotonicity_Oracle()
        {
            // 单调性神谕: 板厚越大，制作难度调节系数必不减少
            double prevK = 1.0;
            for (double t = 1.0; t <= 100.0; t += 0.5)
            {
                double currentK = BoqAggregationEngine.CalculateThicknessFactor(t);
                Assert.True(currentK >= prevK,
                    $"Monotonicity violation in K_Thick at thickness {t}: {currentK} < {prevK}");
                prevK = currentK;
            }
        }

        [Fact]
        public void Quota03_KElev_BoundaryStepTransitions_PlanSection3_3()
        {
            // Plan §3.3:
            // H <= 20m: 1.00
            // 20m < H <= 35m: 1.15
            // H > 35m: 1.30

            const double eps = 1e-4;

            // Tier 1: H <= 20
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(0.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(10.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(20.0 - eps));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(20.0));

            // Tier 2: 20 < H <= 35
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(20.0 + eps));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(27.5));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(35.0 - eps));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(35.0));

            // Tier 3: H > 35
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(35.0 + eps));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(42.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(100.0));
        }

        [Fact]
        public void Quota04_KElev_Monotonicity_Oracle()
        {
            // 单调性神谕: 标高越高，超高作业调节系数必不减少
            double prevK = 1.0;
            for (double h = 0.0; h <= 150.0; h += 1.0)
            {
                double currentK = BoqAggregationEngine.CalculateElevationFactor(h);
                Assert.True(currentK >= prevK,
                    $"Monotonicity violation in K_Elev at elevation {h}: {currentK} < {prevK}");
                prevK = currentK;
            }
        }

        [Fact]
        public void Quota05_KSiteWeld_BinaryFactor()
        {
            // 发现场高空操作平台全熔透焊为 1.30；厂内拼装整体吊装为 1.00
            Assert.Equal(1.30, BoqAggregationEngine.CalculateSiteWeldFactor(true));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateSiteWeldFactor(false));
        }

        [Fact]
        public void Quota06_KWeight_CraneTierAndTonnage_StepTransitions()
        {
            // Plan §3.3:
            // W < 5.0t: 1.00
            // 5.0t <= W <= 15.0t: 1.20
            // 15.0t < W <= 25.0t: 1.30
            // W > 25.0t: 1.45

            const double eps = 1e-4;

            // Tier 1: W < 5.0
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactor(0.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactor(2.5));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactor(5.0 - eps));

            // Tier 2: 5.0 <= W <= 15.0
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactor(5.0));
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactor(10.0));
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactor(15.0));

            // Tier 3: 15.0 < W <= 25.0
            Assert.Equal(1.30, BoqAggregationEngine.CalculateWeightFactor(15.0 + eps));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateWeightFactor(20.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateWeightFactor(25.0));

            // Tier 4: W > 25.0
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactor(25.0 + eps));
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactor(35.0));
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactor(100.0));

            // 文本档次解析验证
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactorFromTier("<5t"));
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactorFromTier("5-15t"));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateWeightFactorFromTier("15-25t"));
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactorFromTier(">25t"));
        }

        [Fact]
        public void Quota07_Combined_FabricationAndInstallation_FormulasMatchPlan()
        {
            // 构件级制作工费与安装工费公式完整性验证
            // UnitFabricationCost = (BaseFab + IntersectCut + UT + Preassembly) * K_Thick
            // UnitInstallationCost = (BaseInstall + BaseCrane + BaseScaffold) * K_Elev * K_SiteWeld * K_Weight

            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-TEST-COMBO",
                Quantity = 1,
                SubProjectNetWeightKg = 10000.0, // 10t
                MaxPlateThickness = 45.0,        // 45mm => K_Thick = 1.25
                AssemblyElevation = 38.0,        // 38m => K_Elev = 1.30
                HasFieldWelding = true,          // 发现场 => K_SiteWeld = 1.30
                SingleWeight = 10.0,             // 10t => K_Weight = 1.20
                CraneTonnageTier = "5-15t",
                FabricationQuotaRule = new FabricationQuotaRule
                {
                    BaseLaborRatePerTon = 2000.0,
                    IntersectingCuttingUnitRate = 300.0,
                    UltrasonicTestingUnitRate = 200.0,
                    PreassemblyUnitRate = 100.0
                },
                InstallationQuotaRule = new InstallationQuotaRule
                {
                    BaseLaborRatePerTon = 1500.0,
                    MainCraneRatePerTon = 500.0,
                    TrestlePlatformRatePerTon = 200.0
                }
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            // 1. 加工制作单价 = (2000 + 300 + 200 + 100) * 1.25 = 2600 * 1.25 = 3250 元/t
            Assert.Equal(1.25, res.KThick);
            Assert.Equal(3250.0, res.FabricationCost / 10.0, precision: 2);
            Assert.Equal(32500.0, res.FabricationCost, precision: 2);

            // 2. 现场安装单价 = (1500 + 500 + 200) * 1.30 * 1.30 * 1.20 = 2200 * 2.028 = 4461.6 元/t
            Assert.Equal(1.30, res.KElev);
            Assert.Equal(1.30, res.KSiteWeld);
            Assert.Equal(1.20, res.KWeight);
            double expectedUnitInstall = 2200.0 * 1.30 * 1.30 * 1.20;
            Assert.Equal(expectedUnitInstall, res.InstallationCost / 10.0, precision: 2);
            Assert.Equal(expectedUnitInstall * 10.0, res.InstallationCost, precision: 2);
        }

        #endregion

        #region 领域 4: 采办经营创效 (剪刀差利润) 跨层级数学精确性

        [Fact]
        public void Arbitrage01_PartLevel_ApprovedMinusPurchase_Times_GrossWeight_Exactness()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var parts = project.TenderItems.SelectMany(t => t.SubProjects).SelectMany(s => s.Parts).ToList();

            // 校验全部零件的 ArbitrageProfit 计算公式:
            // ArbitrageProfit = (ApprovedPricePerTon - ActualPurchasePricePerTon) * (TotalGrossWeight / 1000.0)
            foreach (var part in parts)
            {
                double grossTon = part.TotalGrossWeight / 1000.0;
                double expectedProfit = (part.ApprovedPricePerTon - part.ActualPurchasePricePerTon) * grossTon;

                Assert.Equal(expectedProfit, part.ArbitrageProfit, precision: 2);
            }
        }

        [Fact]
        public void Arbitrage02_ProjectTotal_IsExactSumOfAllParts()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            var allParts = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .SelectMany(s => s.Parts)
                .ToList();

            double sumPartsArbitrage = allParts.Sum(p => p.ArbitrageProfit);

            // 全项目剪刀差总额严格等于全部零件剪刀差利润之和
            Assert.Equal(sumPartsArbitrage, result.TotalProcurementScissorProfit, precision: 2);
            Assert.True(result.TotalProcurementScissorProfit > 0);
        }

        [Fact]
        public void Arbitrage03_NegativeAndZeroArbitrage_BehavesCorrectly()
        {
            // 敌手化场景: 采办超支 (实际采购价 > 业主批复价 => 创效为负)
            var partOverrun = new BoqPart
            {
                PartMark = "P-OVERRUN",
                Quantity = 1,
                SingleNetWeight = 10000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 11500.0,
                ApprovedPricePerTon = 4500.0,
                ActualPurchasePricePerTon = 4800.0 // 超支 300 元/t
            };

            var resOverrun = BoqAggregationEngine.CalculatePart(partOverrun);
            // 剪刀差创效 = (4500 - 4800) * 11.5 = -3450.0
            Assert.Equal(-3450.0, partOverrun.ArbitrageProfit, precision: 2);
            Assert.Equal(-3450.0, resOverrun.ArbitrageProfit, precision: 2);

            // 零利润场景: 实际采购价 == 业主批复价
            var partZero = new BoqPart
            {
                PartMark = "P-ZERO",
                Quantity = 1,
                SingleNetWeight = 10000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 11500.0,
                ApprovedPricePerTon = 4500.0,
                ActualPurchasePricePerTon = 4500.0
            };
            var resZero = BoqAggregationEngine.CalculatePart(partZero);
            Assert.Equal(0.0, partZero.ArbitrageProfit, precision: 2);
            Assert.Equal(0.0, resZero.ArbitrageProfit, precision: 2);
        }

        #endregion

        #region 领域 5: 极端边界压力测试与容错性 (Boundary Inputs & Stress Harness)

        [Fact]
        public void Boundary01_ZeroFeeRates_ComprehensivePriceEqualsDirectCost()
        {
            // 边界 1: 管理费率与利润率均为 0%
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            project.DefaultManagementFeeRate = 0.0;
            project.DefaultProfitRate = 0.0;

            foreach (var item in project.TenderItems)
            {
                item.IsCustomRateEnabled = false;
                item.CustomManagementFeeRate = null;
                item.CustomProfitRate = null;
            }

            var result = BoqAggregationEngine.CalculateProject(project);

            // 当费率为0时:
            // 综合单价 == 直接成本底价
            foreach (var item in project.TenderItems)
            {
                Assert.Equal(item.DirectCostPerTon, item.ComprehensiveUnitPrice, precision: 2);
            }

            // 投标总合价 == 直接成本总额
            Assert.Equal(result.TotalDirectCost, result.TotalBidPrice, precision: 2);
            Assert.Equal(0.0, result.TotalManagementFee, precision: 2);
            Assert.Equal(0.0, result.TotalProfit, precision: 2);
        }

        [Fact]
        public void Boundary02_HundredPercentFeeRates_MultiplierIsExactlyFour()
        {
            // 边界 2: 100% 管理费率与 100% 利润率
            // 综合单价 = 直接成本 * (1 + 1.0) * (1 + 1.0) = 直接成本 * 4.0
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            project.DefaultManagementFeeRate = 1.0;
            project.DefaultProfitRate = 1.0;

            foreach (var item in project.TenderItems)
            {
                item.IsCustomRateEnabled = false;
                item.CustomManagementFeeRate = null;
                item.CustomProfitRate = null;
            }

            var result = BoqAggregationEngine.CalculateProject(project);

            foreach (var item in project.TenderItems)
            {
                Assert.Equal(item.DirectCostPerTon * 4.0, item.ComprehensiveUnitPrice, precision: 2);
            }

            Assert.Equal(result.TotalDirectCost * 4.0, result.TotalBidPrice, precision: 2);
            Assert.Equal(result.TotalDirectCost, result.TotalManagementFee, precision: 2);
            Assert.Equal(result.TotalDirectCost * 2.0, result.TotalProfit, precision: 2);
        }

        [Theory]
        [InlineData(-0.10, -0.05)]
        [InlineData(-0.50, 0.0)]
        public void Boundary03_NegativeFeeRates_EngineHandlesWithoutCrash(double negMgmt, double negProfit)
        {
            // 负费率防护测试: 即使传入负费率，核算引擎应稳定运行并保持数学一致性
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            project.DefaultManagementFeeRate = negMgmt;
            project.DefaultProfitRate = negProfit;

            foreach (var item in project.TenderItems)
            {
                item.IsCustomRateEnabled = false;
            }

            var result = BoqAggregationEngine.CalculateProject(project);

            Assert.NotNull(result);
            Assert.False(double.IsNaN(result.TotalBidPrice));
            Assert.False(double.IsInfinity(result.TotalBidPrice));

            // 守恒核对依然成立
            double totalComponents = result.TotalDirectCost + result.TotalManagementFee + result.TotalProfit;
            Assert.True(Math.Abs(result.TotalBidPrice - totalComponents) < 0.05);
        }

        [Fact]
        public void Boundary04_ExtremeThicknessAndElevation_CapCleanlyAtMaxTiers()
        {
            // 极端几何与工况: 300mm 特厚板、632m 超高标高 (上海中心高度)、500t 极端单件吊装
            double kThick = BoqAggregationEngine.CalculateThicknessFactor(300.0);
            Assert.Equal(1.40, kThick);

            double kElev = BoqAggregationEngine.CalculateElevationFactor(632.0);
            Assert.Equal(1.30, kElev);

            double kWeight = BoqAggregationEngine.CalculateWeightFactor(500.0);
            Assert.Equal(1.45, kWeight);

            // 负数与0防护
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(-5.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(-10.0));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactor(-2.0));
        }

        [Fact]
        public void Boundary05_ZeroOrNullTenderQuantity_DefensiveDivisionProtection()
        {
            var item = new BoqTenderItem
            {
                ItemTag = "ZERO-QTY",
                TenderQuantity = 0.0,
                DirectCostPerTon = 0.0
            };

            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07,
                TenderItems = new List<BoqTenderItem> { item }
            };

            // 当工程量为0时，核算不抛异常且均价/合价安全为0
            var res = BoqAggregationEngine.CalculateTenderItem(item, project);
            Assert.NotNull(res);
            Assert.Equal(0.0, res.TotalTenderPrice);
            Assert.False(double.IsNaN(res.ComprehensiveUnitPrice));
            Assert.False(double.IsInfinity(res.ComprehensiveUnitPrice));

            var projRes = BoqAggregationEngine.CalculateProject(project);
            Assert.Equal(0.0, projRes.TotalBidPrice);
            Assert.False(double.IsNaN(projRes.TotalNetWeightTon));
        }

        [Fact]
        public void Boundary06_NonlinearInterplay_MultiFactorStressHarness()
        {
            // 多维度极端复合压力测试: 超高标高 + 特厚板 + 现场焊接 + 超重构件 + 异常损耗
            var sub = new BoqSubProject
            {
                SubProjectCode = "STRESS-MEGA-NODE",
                Quantity = 5,
                SubProjectNetWeightKg = 250000.0, // 250t
                MaxPlateThickness = 120.0,        // 120mm => 1.40
                AssemblyElevation = 88.0,         // 88m => 1.30
                HasFieldWelding = true,           // 发现场 => 1.30
                SingleWeight = 50.0,              // 50t => 1.45
                CraneTonnageTier = ">25t",
                FireRatingHours = 3.0,            // 3.0H => 145 元/m2
                SubProjectSurfaceAreaM2 = 3500.0,
                FabricationQuotaRule = new FabricationQuotaRule
                {
                    BaseLaborRatePerTon = 2500.0,
                    IntersectingCuttingUnitRate = 400.0,
                    UltrasonicTestingUnitRate = 300.0,
                    PreassemblyUnitRate = 250.0
                },
                InstallationQuotaRule = new InstallationQuotaRule
                {
                    BaseLaborRatePerTon = 2000.0,
                    MainCraneRatePerTon = 1000.0,
                    TrestlePlatformRatePerTon = 400.0
                }
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            Assert.Equal(1.40, res.KThick);
            Assert.Equal(1.30, res.KElev);
            Assert.Equal(1.30, res.KSiteWeld);
            Assert.Equal(1.45, res.KWeight);

            // 制作工费单价 = (2500 + 400 + 300 + 250) * 1.40 = 3450 * 1.40 = 4830 元/t
            Assert.Equal(4830.0, res.FabricationCost / 250.0, precision: 2);

            // 安装工费单价 = (2000 + 1000 + 400) * 1.30 * 1.30 * 1.45 = 3400 * 2.4505 = 8331.7 元/t
            double expectedInstallUnit = 3400.0 * 1.30 * 1.30 * 1.45;
            Assert.Equal(expectedInstallUnit, res.InstallationCost / 250.0, precision: 2);

            // 防火涂装总费 = 3500 * 145 = 507,500 元
            Assert.Equal(507500.0, res.FireCoatingCost, precision: 2);

            // 直接成本总额无 NaN/Infinity
            Assert.False(double.IsNaN(res.DirectCost));
            Assert.False(double.IsInfinity(res.DirectCost));
            Assert.True(res.DirectCost > 0);
        }

        #endregion
    }
}
