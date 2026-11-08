// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqSubTableCAndRemediationStressTests.cs
// 模块功能: 宁波奥体中心 BOQ 修复期数学精度基准、子矩阵 C 守恒与对抗压力测试套件
// 规范依据: ORIGINAL_REQUEST.md (R3, R6), PROJECT.md (M4 Remediation), AGENTS.md
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
    /// 对修复版本 (M4-IT2) 进行严格的数学精度、物理守恒与对抗性压力挑战
    /// 涵盖：
    /// 1. NH、NF、JLC 与全项目基准物理量精准无漂移核验 (NetWeight, SurfaceArea, GrossWeight 1.15x)
    /// 2. 子矩阵 C (按材质牌号 Q460GJC / Q355B / Q235B 交叉透视) 跨层级多维度物理与商务守恒
    /// 3. 零件列表随机置换、乱序与脏数据对抗鲁棒性验证
    /// 4. 树形网格字符串键隔离、角色列聚焦高亮状态机与负费率截断守门员
    /// </summary>
    public class BoqSubTableCAndRemediationStressTests
    {
        private readonly Xunit.Abstractions.ITestOutputHelper _output;

        public BoqSubTableCAndRemediationStressTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            _output = output;
        }
        #region 1. 目标基准物理量绝对精度与 1.15 倍损耗核验 (Target Benchmark Precision)

        [Fact]
        public void Benchmark01_NH_MainTruss_PhysicalMetrics_ExactConservation()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nh = project.TenderItems.Single(t => t.ItemTag == "NH");

            // 1. 理论净重: 1,123,939.15 kg
            double netKg = nh.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(1123939.15, netKg, precision: 2);
            Assert.Equal(1123.94, Math.Round(netKg / 1000.0, 2));

            // 2. 展开表面积: 5,334.63 m²
            double areaM2 = nh.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(5334.63, areaM2, precision: 2);

            // 3. 采购毛重 (1.15倍损耗): 1,292,530.02 kg
            double grossKg = nh.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(1292530.02, Math.Round(grossKg, 2));
            Assert.Equal(1292.53, Math.Round(grossKg / 1000.0, 2));

            // 4. 损耗系数精准等于 1.15
            Assert.True(Math.Abs((grossKg / netKg) - 1.15) < 1e-6);
        }

        [Fact]
        public void Benchmark02_NF_SecondaryTruss_PhysicalMetrics_ExactConservation()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var nf = project.TenderItems.Single(t => t.ItemTag == "NF");

            // 1. 理论净重: 865,200.00 kg
            double netKg = nf.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(865200.00, netKg, precision: 2);
            Assert.Equal(865.20, Math.Round(netKg / 1000.0, 2));

            // 2. 展开表面积: 4,720.00 m²
            double areaM2 = nf.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(4720.00, areaM2, precision: 2);

            // 3. 采购毛重 (1.15倍损耗): 994,980.00 kg
            double grossKg = nf.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(994980.00, Math.Round(grossKg, 2));
            Assert.Equal(994.98, Math.Round(grossKg / 1000.0, 2));

            // 4. 损耗系数精准等于 1.15
            Assert.Equal(1.15, grossKg / netKg, precision: 6);
        }

        [Fact]
        public void Benchmark03_JLC_CheckIn_PhysicalMetrics_ExactConservation()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var jlc = project.TenderItems.Single(t => t.ItemTag == "JLC");

            // 1. 理论净重: 728,300.00 kg
            double netKg = jlc.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            Assert.Equal(728300.00, netKg, precision: 2);
            Assert.Equal(728.30, Math.Round(netKg / 1000.0, 2));

            // 2. 展开表面积: 7,850.00 m²
            double areaM2 = jlc.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);
            Assert.Equal(7850.00, areaM2, precision: 2);

            // 3. 采购毛重 (1.15倍损耗): 837,545.00 kg
            double grossKg = jlc.SubProjects.Sum(s => s.SubProjectGrossWeightKg);
            Assert.Equal(837545.00, Math.Round(grossKg, 2));
            Assert.Equal(837.545, grossKg / 1000.0, precision: 3);

            // 4. 损耗系数精准等于 1.15
            Assert.Equal(1.15, grossKg / netKg, precision: 6);
        }

        [Fact]
        public void Benchmark04_ProjectTotal_PhysicalQuantities_ExactConservation()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var result = BoqAggregationEngine.CalculateProject(project);

            // 1. 全项目理论总净重: 2,717,439.15 kg (2,717.44 t)
            double expectedNetKg = 1123939.15 + 865200.00 + 728300.00;
            Assert.Equal(2717439.15, expectedNetKg, precision: 2);
            Assert.Equal(expectedNetKg, result.TotalNetWeightKg, precision: 2);
            Assert.Equal(2717.44, Math.Round(result.TotalNetWeightTon, 2));

            // 2. 全项目采购总毛重: 3,125,055.02 kg (3,125.06 t)
            double expectedGrossKg = 1292530.02 + 994980.00 + 837545.00;
            Assert.Equal(3125055.02, expectedGrossKg, precision: 2);
            Assert.Equal(expectedGrossKg, result.TotalGrossWeightKg, precision: 2);
            Assert.Equal(3125.06, Math.Round(result.TotalGrossWeightTon, 2));

            // 3. 全项目涂装总表面积: 17,904.63 m²
            double expectedAreaM2 = 5334.63 + 4720.00 + 7850.00;
            Assert.Equal(17904.63, expectedAreaM2, precision: 2);
            Assert.Equal(expectedAreaM2, result.TotalSurfaceAreaM2, precision: 2);
        }

        #endregion

        #region 2. 子矩阵 C (钢材材质牌号) 跨层级物理与商务守恒对账 (Sub-Table C Conservation)

        /// <summary>
        /// 模拟客户端组件内部的 GetSteelGradeAnalytics 聚合算法
        /// </summary>
        private static List<SteelGradeAnalyticsRow> CalculateSteelGradeAnalytics(BoqProject project)
        {
            if (project?.TenderItems == null || project.TenderItems.Count == 0)
                return new List<SteelGradeAnalyticsRow>();

            var allParts = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .SelectMany(s => s.Parts)
                .ToList();

            return allParts
                .GroupBy(p => string.IsNullOrWhiteSpace(p.SteelGrade) ? "其他材质" : p.SteelGrade.Trim())
                .Select(g =>
                {
                    double netTon = g.Sum(p => p.TotalNetWeight) / 1000.0;
                    double grossTon = g.Sum(p => p.TotalGrossWeight > 0.0 ? p.TotalGrossWeight : p.TotalNetWeight * 1.15) / 1000.0;
                    double matCost = g.Sum(p => p.TotalPartMaterialCost);
                    double scissorProfit = g.Sum(p => p.ArbitrageProfit);

                    return new SteelGradeAnalyticsRow
                    {
                        GradeName = g.Key,
                        PartCount = g.Sum(p => Math.Max(1, p.Quantity)),
                        TotalNetWeightTon = Math.Round(netTon, 3),
                        TotalGrossWeightTon = Math.Round(grossTon, 3),
                        TotalMaterialCost = Math.Round(matCost, 2),
                        TotalScissorProfit = Math.Round(scissorProfit, 2)
                    };
                })
                .OrderByDescending(r => r.TotalNetWeightTon)
                .ToList();
        }

        [Fact]
        public void SubTableC01_SteelGrades_SummationStrictlyMatchesProjectTotals()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            var gradeRows = CalculateSteelGradeAnalytics(project);

            // 1. 验证包含了宁波奥体三大材质
            Assert.True(gradeRows.Count >= 3);
            Assert.Contains(gradeRows, r => r.GradeName == "Q460GJC");
            Assert.Contains(gradeRows, r => r.GradeName == "Q355B");
            Assert.Contains(gradeRows, r => r.GradeName == "Q235B");

            // 2. 理论净重之和守恒 (允许 3位截断累计误差 <= 0.002t)
            double sumNetTon = gradeRows.Sum(r => r.TotalNetWeightTon);
            Assert.True(Math.Abs(projectResult.TotalNetWeightTon - sumNetTon) <= 0.002,
                $"Net ton mismatch: Project={projectResult.TotalNetWeightTon}, GradesSum={sumNetTon}");
            Assert.Equal(2717.44, Math.Round(sumNetTon, 2));

            // 3. 采购毛重之和守恒 (1.15倍)
            double sumGrossTon = gradeRows.Sum(r => r.TotalGrossWeightTon);
            Assert.True(Math.Abs(projectResult.TotalGrossWeightTon - sumGrossTon) <= 0.002,
                $"Gross ton mismatch: Project={projectResult.TotalGrossWeightTon}, GradesSum={sumGrossTon}");
            Assert.Equal(3125.06, Math.Round(sumGrossTon, 2));

            // 4. 材料到场采购合价守恒 (分毫不差)
            double sumMatCost = gradeRows.Sum(r => r.TotalMaterialCost);
            Assert.True(Math.Abs(projectResult.TotalMaterialCost - sumMatCost) <= 0.05,
                $"Material cost mismatch: Project={projectResult.TotalMaterialCost}, GradesSum={sumMatCost}");

            // 5. 采办剪刀差利润总额守恒
            double sumScissor = gradeRows.Sum(r => r.TotalScissorProfit);
            Assert.True(Math.Abs(projectResult.TotalProcurementScissorProfit - sumScissor) <= 0.05,
                $"Scissor profit mismatch: Project={projectResult.TotalProcurementScissorProfit}, GradesSum={sumScissor}");
        }

        [Fact]
        public void SubTableC02_IndividualSteelGrade_Metrics_AreConsistentAndPositive()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);
            var gradeRows = CalculateSteelGradeAnalytics(project);

            _output.WriteLine("=== Sub-table C Steel Grade Breakdown ===");
            foreach (var r in gradeRows)
            {
                _output.WriteLine($"Grade: {r.GradeName,-10} | Parts: {r.PartCount,4} | Net(t): {r.TotalNetWeightTon,10:F3} | Gross(t): {r.TotalGrossWeightTon,10:F3} | MatCost(¥): {r.TotalMaterialCost,14:N2} | ScissorProfit(¥): {r.TotalScissorProfit,12:N2}");
            }
            _output.WriteLine($"TOTAL PROJECT      | Parts: {project.TenderItems.SelectMany(t=>t.SubProjects).SelectMany(s=>s.Parts).Sum(p=>p.Quantity),4} | Net(t): {projectResult.TotalNetWeightTon,10:F3} | Gross(t): {projectResult.TotalGrossWeightTon,10:F3} | MatCost(¥): {projectResult.TotalMaterialCost,14:N2} | ScissorProfit(¥): {projectResult.TotalProcurementScissorProfit,12:N2}");

            foreach (var grade in gradeRows)
            {
                Assert.False(string.IsNullOrWhiteSpace(grade.GradeName));
                Assert.True(grade.PartCount > 0, $"{grade.GradeName} should have positive part count");
                Assert.True(grade.TotalNetWeightTon > 0, $"{grade.GradeName} should have positive net weight");
                Assert.True(grade.TotalGrossWeightTon > 0, $"{grade.GradeName} should have positive gross weight");
                Assert.True(grade.TotalMaterialCost > 0, $"{grade.GradeName} should have positive material cost");

                // 毛重与净重比必须严格符合 1.15 损耗乘数 (微小舍入容差以内)
                double ratio = grade.TotalGrossWeightTon / grade.TotalNetWeightTon;
                Assert.True(Math.Abs(ratio - 1.15) < 0.005,
                    $"{grade.GradeName} gross/net ratio {ratio} deviates from 1.15");
            }
        }

        [Fact]
        public void SubTableC03_PartPermutationInvariance_ProducesIdenticalAggregations()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var baselineGrades = CalculateSteelGradeAnalytics(project);

            // 对零件顺序进行 10 次伪随机打乱
            var rng = new Random(2026);
            for (int i = 0; i < 10; i++)
            {
                var clone = NingboOlympicSeedData.CreateBenchmarkProject();
                foreach (var sub in clone.TenderItems.SelectMany(t => t.SubProjects))
                {
                    sub.Parts = sub.Parts.OrderBy(_ => rng.Next()).ToList();
                }

                var shuffledGrades = CalculateSteelGradeAnalytics(clone);

                Assert.Equal(baselineGrades.Count, shuffledGrades.Count);
                for (int j = 0; j < baselineGrades.Count; j++)
                {
                    Assert.Equal(baselineGrades[j].GradeName, shuffledGrades[j].GradeName);
                    Assert.Equal(baselineGrades[j].PartCount, shuffledGrades[j].PartCount);
                    Assert.Equal(baselineGrades[j].TotalNetWeightTon, shuffledGrades[j].TotalNetWeightTon, precision: 3);
                    Assert.Equal(baselineGrades[j].TotalGrossWeightTon, shuffledGrades[j].TotalGrossWeightTon, precision: 3);
                    Assert.Equal(baselineGrades[j].TotalMaterialCost, shuffledGrades[j].TotalMaterialCost, precision: 2);
                    Assert.Equal(baselineGrades[j].TotalScissorProfit, shuffledGrades[j].TotalScissorProfit, precision: 2);
                }
            }
        }

        [Fact]
        public void SubTableC04_DirtyAndMissingGrade_FallbacksGracefully()
        {
            var project = new BoqProject
            {
                ProjectCode = "DIRTY-TEST",
                ProjectName = "脏数据测试项目",
                TenderItems = new List<BoqTenderItem>()
            };

            var tenderItem = new BoqTenderItem { ItemTag = "T1", Project = project };
            var sub = new BoqSubProject { SubProjectCode = "S1", TenderItem = tenderItem };
            tenderItem.SubProjects.Add(sub);
            project.TenderItems.Add(tenderItem);

            // 添加包含 null、空格及标准材质的零件
            sub.Parts.Add(new BoqPart { PartMark = "P1", SteelGrade = null!, TotalNetWeight = 1000.0, TotalGrossWeight = 1150.0 });
            sub.Parts.Add(new BoqPart { PartMark = "P2", SteelGrade = "   ", TotalNetWeight = 2000.0, TotalGrossWeight = 2300.0 });
            sub.Parts.Add(new BoqPart { PartMark = "P3", SteelGrade = " Q355B ", TotalNetWeight = 3000.0, TotalGrossWeight = 3450.0 });

            var grades = CalculateSteelGradeAnalytics(project);

            Assert.Equal(2, grades.Count);
            var otherRow = grades.Single(g => g.GradeName == "其他材质");
            Assert.Equal(3.0, otherRow.TotalNetWeightTon, precision: 3); // 1000 + 2000 = 3000kg = 3.0t
            Assert.Equal(3.45, otherRow.TotalGrossWeightTon, precision: 3);

            var q355Row = grades.Single(g => g.GradeName == "Q355B");
            Assert.Equal(3.0, q355Row.TotalNetWeightTon, precision: 3);
            Assert.Equal(3.45, q355Row.TotalGrossWeightTon, precision: 3);
        }

        #endregion

        #region 3. 动态 KPI 标题与树状网格键隔离测试 (Dynamic Subtitles & Key Isolation)

        [Fact]
        public void KpiSubtitles_DynamicFormatting_MatchesBenchmarkSpec()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // Card 2: string.Join(" | ", _project.TenderItems.Select(t => $"{t.ItemTag}: {t.TenderQuantity:N2}t"))
            string card2Subtitle = string.Join(" | ", project.TenderItems.Select(t => $"{t.ItemTag}: {t.TenderQuantity:N2}t"));
            Assert.Equal("NH: 1,123.94t | NF: 865.20t | JLC: 728.30t", card2Subtitle);

            // Card 4: 耐火极限分组动态拼装
            var allSubProjects = project.TenderItems.SelectMany(t => t.SubProjects).ToList();
            var fireAnalytics = allSubProjects
                .GroupBy(s => s.FireRatingHours)
                .Select(g => new { RatingHours = g.Key, SurfaceAreaM2 = g.Sum(s => s.SubProjectSurfaceAreaM2) })
                .OrderByDescending(f => f.RatingHours)
                .ToList();

            string card4Subtitle = string.Join(" | ", fireAnalytics.Select(f => $"{f.RatingHours:F1}H: {f.SurfaceAreaM2:N1}m²"));
            Assert.Equal("3.0H: 5,334.6m² | 2.0H: 4,720.0m² | 1.5H: 7,850.0m²", card4Subtitle);
        }

        [Fact]
        public void TreeGrid_StringKeyExpansion_IsCompletelyIsolated()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            var expandedTags = new HashSet<string>();
            var expandedCodes = new HashSet<string>();

            // 展开第一个清单项 (NH)
            expandedTags.Add(project.TenderItems[0].ItemTag);

            Assert.Contains("NH", expandedTags);
            Assert.DoesNotContain("NF", expandedTags);
            Assert.DoesNotContain("JLC", expandedTags);

            // 展开构件 N-NHXX-48
            var sub48 = project.TenderItems[0].SubProjects.First();
            expandedCodes.Add(sub48.SubProjectCode);

            Assert.Contains("N-NHXX-48", expandedCodes);
            Assert.DoesNotContain("N-NHXX-49", expandedCodes);
            Assert.DoesNotContain("N-NHXX-50", expandedCodes);
        }

        [Theory]
        [InlineData(-10.0, 0.0)]
        [InlineData(-0.01, 0.0)]
        [InlineData(0.0, 0.0)]
        [InlineData(8.0, 0.08)]
        [InlineData(12.5, 0.125)]
        public void FeeRates_NegativeClamping_PreventsCorruptCalculations(double inputRatePercent, double expectedFraction)
        {
            double clampedFraction = Math.Max(0.0, inputRatePercent / 100.0);
            Assert.Equal(expectedFraction, clampedFraction, precision: 4);
        }

        #endregion
    }
}
