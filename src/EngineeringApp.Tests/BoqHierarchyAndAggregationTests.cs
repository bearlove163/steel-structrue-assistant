// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqHierarchyAndAggregationTests.cs
// 模块功能: 钢结构 BOQ 层级领域模型、自底向上数学卷积与多受众投影聚合端到端单元测试
// 规范依据: doc/steel_boq_database_and_reporting_plan.md (R1, R2, R6), AGENTS.md
// ============================================================================

using System;
using System.IO;
using System.Linq;
using Xunit;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Export;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Tests
{
    public class BoqHierarchyAndAggregationTests
    {
        [Fact]
        public void Test01_NingboOlympicBenchmark_WeightAndArea_Match_ExactAuthoritativeValues()
        {
            // 验证南侧主桁架基准工程量
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            Assert.NotNull(project);
            Assert.Equal("NB-ATZX", project.ProjectCode);

            var nh = project.TenderItems.FirstOrDefault(t => t.ItemTag == "NH");
            Assert.NotNull(nh);

            // 卷积汇算
            var projRes = BoqAggregationEngine.CalculateProject(project);
            Assert.NotNull(projRes);

            // 验证理论净重 1,123,939.15 kg 与 展开表面积 5,334.63 m2
            var totalNetWeight = nh.SubProjects.Sum(s => s.SubProjectNetWeightKg);
            var totalArea = nh.SubProjects.Sum(s => s.SubProjectSurfaceAreaM2);

            Assert.Equal(1123939.15, totalNetWeight, 2);
            Assert.Equal(5334.63, totalArea, 2);
        }

        [Fact]
        public void Test02_CheckinHall_PurchasingLossFactor_IsExactly_1_15()
        {
            // 验证第一检录处 1.15 倍采购损耗系数
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var jlc = project.TenderItems.FirstOrDefault(t => t.ItemTag == "JLC");
            Assert.NotNull(jlc);

            BoqAggregationEngine.CalculateProject(project);

            foreach (var sub in jlc.SubProjects)
            {
                foreach (var part in sub.Parts)
                {
                    Assert.Equal(1.15, part.PurchasingWasteFactor, 4);
                    double expectedGross = part.TotalNetWeight * 1.15;
                    Assert.Equal(expectedGross, part.TotalGrossWeight, 2);
                }
            }
        }

        [Fact]
        public void Test03_PartMaterialCost_Includes_Thickness_And_ZPerformance_Surcharges()
        {
            // 验证零件层对应材料费模块的基价、厚度加价与 Z向性能加价
            var part = new BoqPart
            {
                PartMark = "HLP-TEST-01",
                SteelGrade = "Q460GJC",
                SectionType = "PL",
                Thickness = 50.0,
                ZDirectionGrade = "Z15",
                Quantity = 1,
                SingleNetWeight = 1000.0,
                PurchasingWasteFactor = 1.15
            };

            var partResult = BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(1000.0, part.TotalNetWeight);
            Assert.Equal(1150.0, part.TotalGrossWeight);
            Assert.True(part.MaterialUnitPricePerTon > 4000.0);
            Assert.True(part.TotalPartMaterialCost > 0.0);
            Assert.Equal(part.TotalPartMaterialCost, partResult.TotalPartCost, 2);
        }

        [Fact]
        public void Test04_SubProject_FabricationAndInstallation_QuotaFactorScaling()
        {
            // 验证构件层加工制作定额 (K_Thick) 与安装定额 (K_Elev, K_SiteWeld, K_Weight)
            Assert.Equal(1.00, BoqAggregationEngine.CalculateThicknessFactor(16.0));
            Assert.Equal(1.05, BoqAggregationEngine.CalculateThicknessFactor(25.0));
            Assert.Equal(1.12, BoqAggregationEngine.CalculateThicknessFactor(35.0));
            Assert.Equal(1.25, BoqAggregationEngine.CalculateThicknessFactor(50.0));
            Assert.Equal(1.40, BoqAggregationEngine.CalculateThicknessFactor(70.0));

            // 安装标高系数
            Assert.Equal(1.00, BoqAggregationEngine.CalculateElevationFactor(15.0));
            Assert.Equal(1.15, BoqAggregationEngine.CalculateElevationFactor(30.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateElevationFactor(45.0));

            // 发现场现场全熔透坡口焊系数
            Assert.Equal(1.30, BoqAggregationEngine.CalculateSiteWeldFactor(isSiteWelded: true));
            Assert.Equal(1.00, BoqAggregationEngine.CalculateSiteWeldFactor(isSiteWelded: false));

            // 单件吊重分档
            Assert.Equal(1.00, BoqAggregationEngine.CalculateWeightFactor(3.0));
            Assert.Equal(1.20, BoqAggregationEngine.CalculateWeightFactor(10.0));
            Assert.Equal(1.30, BoqAggregationEngine.CalculateWeightFactor(20.0));
            Assert.Equal(1.45, BoqAggregationEngine.CalculateWeightFactor(30.0));
        }

        [Fact]
        public void Test05_TenderItem_FeeRateOverride_And_DefaultInheritance()
        {
            // 验证费率分级判定：全局默认 vs 单项独立覆盖
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08, // 默认 8%
                DefaultProfitRate = 0.07          // 默认 7%
            };

            var itemDefault = new BoqTenderItem
            {
                ItemTag = "NF",
                DirectCostPerTon = 10000.0,
                IsCustomRateEnabled = false
            };

            var (mgmt1, profit1) = itemDefault.GetEffectiveRates(project);
            Assert.Equal(0.08, mgmt1);
            Assert.Equal(0.07, profit1);

            // 组价综合单价 = 10000 * 1.08 * 1.07 = 11556
            itemDefault.ComprehensiveUnitPrice = itemDefault.DirectCostPerTon * (1.0 + mgmt1) * (1.0 + profit1);
            Assert.Equal(11556.0, itemDefault.ComprehensiveUnitPrice, 2);

            // 单项启用覆盖 (不平衡调高利润至 12%, 管理费 10%)
            var itemCustom = new BoqTenderItem
            {
                ItemTag = "NH",
                DirectCostPerTon = 10000.0,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.10,
                CustomProfitRate = 0.12
            };

            var (mgmt2, profit2) = itemCustom.GetEffectiveRates(project);
            Assert.Equal(0.10, mgmt2);
            Assert.Equal(0.12, profit2);

            // 组价综合单价 = 10000 * 1.10 * 1.12 = 12320
            itemCustom.ComprehensiveUnitPrice = itemCustom.DirectCostPerTon * (1.0 + mgmt2) * (1.0 + profit2);
            Assert.Equal(12320.0, itemCustom.ComprehensiveUnitPrice, 2);
        }

        [Fact]
        public void Test06_MultiAudienceProjection_And_ExcelExport()
        {
            // 验证五大受众投影与 Excel/CSV 导出
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            BoqAggregationEngine.CalculateProject(project);

            // 1. 业主对外视窗 (脱敏内部直接底价)
            var config = new BoqReportConfig
            {
                AudienceMode = BoqReportAudienceMode.ClientTenderMaster,
                MaskInternalDirectCosts = true
            };
            var ownerProj = BoqAggregationEngine.GenerateAudienceReport(project, config);
            Assert.NotNull(ownerProj);
            Assert.Contains("业主", ownerProj.PerspectiveName);

            // 2. 导出 CSV
            var csv = BoqExcelExporter.ExportToCsv(ownerProj);
            Assert.NotEmpty(csv);
            Assert.StartsWith("\uFEFF", csv); // 包含 UTF-8 BOM 杜绝乱码

            // 3. 导出多工作表 XML Spreadsheet
            var xml = BoqExcelExporter.ExportToXmlSpreadsheet(ownerProj, project.ProjectName);
            Assert.NotEmpty(xml);
            Assert.Contains("Workbook", xml);
            Assert.Contains("Worksheet", xml);
        }
    }
}
