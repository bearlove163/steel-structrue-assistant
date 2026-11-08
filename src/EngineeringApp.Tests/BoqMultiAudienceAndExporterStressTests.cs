// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqMultiAudienceAndExporterStressTests.cs
// 模块描述: 钢结构 BOQ 多受众投影视图与 BoqExcelExporter 敌手化压力测试套件
// 验证范围:
//   1. 业主审计视图脱敏机制 (底价、成本明细、费率利润、采办剪刀差等严禁泄露)
//   2. 总经济师全成本穿透大盘 (完整 26 项核心成本穿透列及数学逻辑严密性)
//   3. 物资采办视图 (跨构件母材归并、1.15倍采购损耗毛重、阶梯加价与剪刀差)
//   4. BoqExcelExporter 导出引擎 (CSV UTF-8 BOM 编码、RFC 4180 转义、XML 电子表格语法有效性、多工作表合一、HTML 渲染)
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Export;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    public class BoqMultiAudienceAndExporterStressTests
    {
        #region Test Fixtures & Factories

        /// <summary>
        /// 创建模拟宁波奥体中心基准工程的测试实体 (包含 NH、NF、JLC 清单项与多级构件、零件)
        /// </summary>
        private static BoqProject CreateBenchmarkOlympicProject()
        {
            var project = new BoqProject
            {
                Id = 1,
                ProjectCode = "NB-ATZX",
                ProjectName = "宁波奥林匹克体育中心主体育场与配套工程",
                ClientName = "宁波市奥体投资建设有限公司 / 城投指挥部",
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07,
                ActiveMaterialSnapshotId = "SNAP-2026-Q1",
                ActivePaintSnapshotId = "PAINT-2026-Q1"
            };

            // 定额规则
            var fabRuleTruss = new FabricationQuotaRule
            {
                Id = 1,
                QuotaCode = "FAB_SPACE_TRUSS_MAIN",
                QuotaName = "大跨空间管桁架主肢车间制作",
                StructuralSystem = "空间管桁架",
                BaseLaborDays = 1.8,
                BaseLaborRatePerTon = 2200.0,
                IntersectingCuttingUnitRate = 280.0,
                UltrasonicTestingUnitRate = 150.0,
                PreassemblyUnitRate = 200.0
            };

            var fabRuleSecondary = new FabricationQuotaRule
            {
                Id = 2,
                QuotaCode = "FAB_SPACE_TRUSS_SUB",
                QuotaName = "空间管桁架副桁架车间制作",
                StructuralSystem = "副桁架",
                BaseLaborDays = 1.5,
                BaseLaborRatePerTon = 1500.0,
                IntersectingCuttingUnitRate = 200.0,
                UltrasonicTestingUnitRate = 120.0,
                PreassemblyUnitRate = 150.0
            };

            var installRuleHigh = new InstallationQuotaRule
            {
                Id = 1,
                QuotaCode = "INS_SPACE_TRUSS_H30M",
                QuotaName = "空间管桁架高空原位散装全熔透焊",
                ApplicableRole = "主桁架",
                BaseLaborDays = 2.5,
                BaseLaborRatePerTon = 1800.0,
                MainCraneRatePerTon = 650.0,
                TrestlePlatformRatePerTon = 300.0,
                ElevationSurchargeFactor = 1.15
            };

            var installRuleStandard = new InstallationQuotaRule
            {
                Id = 2,
                QuotaCode = "INS_SECONDARY_TRUSS",
                QuotaName = "空间副桁架吊装拼装",
                ApplicableRole = "副桁架",
                BaseLaborDays = 1.8,
                BaseLaborRatePerTon = 1400.0,
                MainCraneRatePerTon = 450.0,
                TrestlePlatformRatePerTon = 200.0,
                ElevationSurchargeFactor = 1.0
            };

            // 1. 清单项 NH: 主体育场南侧大跨主桁架制作安装 (开启独立费率覆盖: 管理费 10%, 利润 12%)
            var itemNH = new BoqTenderItem
            {
                Id = 101,
                ProjectId = project.Id,
                Project = project,
                ItemTag = "NH",
                BillItemCode = "010101001001",
                BillItemName = "南侧大跨主桁架制作安装",
                ProjectCharacteristics = "高强结构钢Q460GJC-Z15、大管径1200x50、3.0H厚型防火涂装、现场高空原位散装",
                Unit = "t",
                TenderQuantity = 1123.94,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.10,
                CustomProfitRate = 0.12
            };

            // 构件 1: N-NHXX-48 下弦大管节
            var subNH48 = new BoqSubProject
            {
                Id = 1001,
                TenderItemId = itemNH.Id,
                TenderItem = itemNH,
                SubProjectCode = "N-NHXX-48",
                SubProjectName = "南侧主桁架下弦重型大管段-48",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 50.0,
                AssemblyElevation = 32.0,
                SingleWeight = 12.85,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "5-15t",
                FireRatingHours = 3.0,
                FabricationQuotaRule = fabRuleTruss,
                InstallationQuotaRule = installRuleHigh
            };

            // 零件 1: PIP1200*50 主管
            subNH48.Parts.Add(new BoqPart
            {
                Id = 10001,
                SubProjectId = subNH48.Id,
                SubProject = subNH48,
                PartMark = "HLP-15",
                SectionSpecification = "PIP1200*50",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 1200.0,
                Length = 6500.0,
                Quantity = 2,
                TotalNetWeight = 18434.0, // 18.434 t
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 18434.0 * 1.15,
                TotalSurfaceAreaM2 = 49.0,
                ZDirectionGrade = "Z15",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4050.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 5800.0,
                ActualPurchasePricePerTon = 5300.0 // 剪刀差创效 500元/t
            });

            // 零件 2: PL50*2150 节点连接板
            subNH48.Parts.Add(new BoqPart
            {
                Id = 10002,
                SubProjectId = subNH48.Id,
                SubProject = subNH48,
                PartMark = "HLP-144",
                SectionSpecification = "PL50*2150",
                SectionType = "PL",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 2150.0,
                Length = 2500.0,
                Quantity = 4,
                TotalNetWeight = 4220.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 4220.0 * 1.15,
                TotalSurfaceAreaM2 = 21.5,
                ZDirectionGrade = "Z15",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4050.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 5800.0,
                ActualPurchasePricePerTon = 5300.0
            });

            itemNH.SubProjects.Add(subNH48);
            project.TenderItems.Add(itemNH);

            // 2. 清单项 NF: 主体育场南侧副桁架系统 (继承默认费率: 管理费 8%, 利润 7%)
            var itemNF = new BoqTenderItem
            {
                Id = 102,
                ProjectId = project.Id,
                Project = project,
                ItemTag = "NF",
                BillItemCode = "010101001002",
                BillItemName = "南侧副桁架制作安装",
                ProjectCharacteristics = "Q355B、中等管径500x20、2.0H防火涂料、厂内拼装发运",
                Unit = "t",
                TenderQuantity = 865.20,
                IsCustomRateEnabled = false
            };

            var subNFSX1 = new BoqSubProject
            {
                Id = 1002,
                TenderItemId = itemNF.Id,
                TenderItem = itemNF,
                SubProjectCode = "NFSX1",
                SubProjectName = "副桁架上弦标准节",
                StructuralRole = "副桁架",
                InstallationMethod = "厂内拼装整体吊装",
                MaxPlateThickness = 20.0,
                AssemblyElevation = 18.0,
                SingleWeight = 4.5,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 2.0,
                FabricationQuotaRule = fabRuleSecondary,
                InstallationQuotaRule = installRuleStandard
            };

            subNFSX1.Parts.Add(new BoqPart
            {
                Id = 10003,
                SubProjectId = subNFSX1.Id,
                SubProject = subNFSX1,
                PartMark = "HLP-NF-01",
                SectionSpecification = "PIP500*20",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 20.0,
                DiameterOrWidthMm = 500.0,
                Length = 9000.0,
                Quantity = 2,
                TotalNetWeight = 4261.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 4261.0 * 1.15,
                TotalSurfaceAreaM2 = 28.2,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 4050.0,
                ThicknessSurchargePerTon = 0.0,
                GradeSurchargePerTon = 0.0,
                ZPerformanceSurchargePerTon = 0.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4600.0,
                ActualPurchasePricePerTon = 4450.0 // 剪刀差创效 150元/t
            });

            itemNF.SubProjects.Add(subNFSX1);
            project.TenderItems.Add(itemNF);

            // 3. 清单项 JLC: 第一检录处轻型钢结构 (独立费率覆盖: 竞争性报价 4% 利润)
            var itemJLC = new BoqTenderItem
            {
                Id = 103,
                ProjectId = project.Id,
                Project = project,
                ItemTag = "JLC",
                BillItemCode = "010101001003",
                BillItemName = "第一检录处轻型门式钢架制作安装",
                ProjectCharacteristics = "Q235B、焊接H型钢、1.5H薄涂防火",
                Unit = "t",
                TenderQuantity = 728.30,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.06,
                CustomProfitRate = 0.04
            };

            var subJLC1 = new BoqSubProject
            {
                Id = 1003,
                TenderItemId = itemJLC.Id,
                TenderItem = itemJLC,
                SubProjectCode = "JLC-G1",
                SubProjectName = "门式刚架钢梁",
                StructuralRole = "梁",
                InstallationMethod = "厂内拼装发运",
                MaxPlateThickness = 14.0,
                AssemblyElevation = 12.0,
                SingleWeight = 3.2,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 1.5,
                FabricationQuotaRule = fabRuleSecondary,
                InstallationQuotaRule = installRuleStandard
            };

            subJLC1.Parts.Add(new BoqPart
            {
                Id = 10004,
                SubProjectId = subJLC1.Id,
                SubProject = subJLC1,
                PartMark = "JLC-P1",
                SectionSpecification = "PL14*350",
                SectionType = "PL",
                SteelGrade = "Q235B",
                Thickness = 14.0,
                DiameterOrWidthMm = 350.0,
                Length = 12000.0,
                Quantity = 2,
                TotalNetWeight = 923.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 923.0 * 1.15,
                TotalSurfaceAreaM2 = 16.8,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 3920.0,
                ThicknessSurchargePerTon = 0.0,
                GradeSurchargePerTon = 0.0,
                ZPerformanceSurchargePerTon = 0.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4300.0,
                ActualPurchasePricePerTon = 4180.0
            });

            itemJLC.SubProjects.Add(subJLC1);
            project.TenderItems.Add(itemJLC);

            return project;
        }

        #endregion

        #region 1. Owner View Masking Tests (Adversarial Verification)

        [Fact]
        public void OwnerView_Headers_ContainStrictlyOnlyAuthorizedClientColumns()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            // 1. 验证列数量：必须为 10 列标准清单列
            Assert.Equal(10, report.Headers.Count);

            var expectedHeaders = new[]
            {
                "序号", "清单代号", "清单编码", "清单项目名称", "项目特征描述",
                "计量单位", "投标工程量", "综合单价(元)", "合价(元)", "合规审计说明"
            };
            Assert.Equal(expectedHeaders, report.Headers);

            // 2. 严厉检查：绝对不得出现任何内部定额、直接成本底价、费率、利润或剪刀差词汇
            var forbiddenTokens = new[]
            {
                "成本", "底价", "管理费", "利润", "毛利", "利润率", "管理费率",
                "剪刀差", "采办创效", "信息指导价", "实际采办", "制作工费", "安装工费",
                "材料费", "DirectCost", "Margin", "Scissor", "Profit"
            };

            foreach (var header in report.Headers)
            {
                foreach (var forbidden in forbiddenTokens)
                {
                    Assert.DoesNotContain(forbidden, header, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        [Fact]
        public void OwnerView_Rows_ContainOnlyAuthorizedKeys_NoSensitiveLeakage()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            Assert.NotEmpty(report.Rows);

            var forbiddenTokens = new[]
            {
                "成本", "底价", "管理费", "利润", "毛利", "利润率", "管理费率",
                "剪刀差", "采办创效", "信息价", "实际采办", "制作工费", "安装工费", "材料费"
            };

            foreach (var row in report.Rows)
            {
                // 验证每行包含的键严格等于 Headers 集合
                Assert.Equal(report.Headers.Count, row.Keys.Count);
                foreach (var key in row.Keys)
                {
                    Assert.Contains(key, report.Headers);
                    foreach (var forbidden in forbiddenTokens)
                    {
                        Assert.DoesNotContain(forbidden, key, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }

        [Fact]
        public void OwnerView_TypedRows_ClientTenderMasterRow_HasNoInternalCostProperties()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            Assert.NotNull(report.ClientTenderRows);
            Assert.Equal(project.TenderItems.Count, report.ClientTenderRows.Count);

            // 反射审查 ClientTenderMasterRow 实体模型的所有属性
            var properties = typeof(ClientTenderMasterRow).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var forbiddenPropertyNames = new[]
            {
                "DirectCost", "DirectCostPerTon", "DirectCostSum", "MaterialCost",
                "FabricationCost", "InstallationCost", "FireCoatingCost", "ManagementFee",
                "Profit", "TargetProfit", "Scissor", "Arbitrage", "EffectiveRate"
            };

            foreach (var prop in properties)
            {
                foreach (var forbidden in forbiddenPropertyNames)
                {
                    Assert.False(prop.Name.Contains(forbidden, StringComparison.OrdinalIgnoreCase),
                        $"ClientTenderMasterRow contains confidential internal field: {prop.Name}");
                }
            }
        }

        [Fact]
        public void OwnerView_SummaryMetrics_ContainsNoSensitiveDirectCostOrProfit()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            Assert.NotNull(report.SummaryMetrics);
            Assert.NotEmpty(report.SummaryMetrics);

            var forbiddenMetricKeywords = new[]
            {
                "直接成本", "底价", "管理费", "利润", "毛利", "剪刀差", "采办创效", "制作工费", "安装工费"
            };

            foreach (var metricKey in report.SummaryMetrics.Keys)
            {
                foreach (var forbidden in forbiddenMetricKeywords)
                {
                    Assert.DoesNotContain(forbidden, metricKey, StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        [Fact]
        public void OwnerView_ExportToCsvAndXmlAndHtml_VerifiablyMasksInternalDirectCosts()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            // 1. CSV 脱敏检查
            string csv = BoqExcelExporter.ExportToCsv(report);
            Assert.Contains("业主审计对外清单", csv);
            Assert.Contains("010101001001", csv);
            Assert.DoesNotContain("直接成本", csv);
            Assert.DoesNotContain("底价", csv);
            Assert.DoesNotContain("管理费", csv);
            Assert.DoesNotContain("利润", csv);
            Assert.DoesNotContain("剪刀差", csv);
            Assert.DoesNotContain("材料费小计", csv);

            // 2. XML 电子表格脱敏检查
            string xml = BoqExcelExporter.ExportToXmlSpreadsheet(report, project.ProjectName);
            Assert.DoesNotContain("直接成本", xml);
            Assert.DoesNotContain("底价", xml);
            Assert.DoesNotContain("管理费", xml);
            Assert.DoesNotContain("利润", xml);
            Assert.DoesNotContain("剪刀差", xml);

            // 3. HTML 报表脱敏检查
            string html = BoqExcelExporter.ExportToHtml(report);
            Assert.DoesNotContain("直接成本", html);
            Assert.DoesNotContain("底价", html);
            Assert.DoesNotContain("管理费", html);
            Assert.DoesNotContain("利润", html);
            Assert.DoesNotContain("剪刀差", html);
        }

        [Fact]
        public void OwnerView_EvenWithExtremeProfitAndHugeScissors_RemainsCompletelyMasked()
        {
            var project = CreateBenchmarkOlympicProject();
            // 人为构造极端高利润与千万级剪刀差套利
            project.TenderItems[0].CustomManagementFeeRate = 0.50; // 50% 管理费
            project.TenderItems[0].CustomProfitRate = 0.60;        // 60% 利润
            foreach (var part in project.TenderItems[0].SubProjects[0].Parts)
            {
                part.ApprovedPricePerTon = 15000.0;
                part.ActualPurchasePricePerTon = 4000.0; // 11,000 元/t 剪刀差
            }

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });

            // 对外呈现只显示最终对外综合单价与合价，绝无内部底价与剪刀差
            Assert.Equal(10, report.Headers.Count);
            foreach (var row in report.Rows)
            {
                Assert.DoesNotContain("直接成本", row.Keys);
                Assert.DoesNotContain("采办剪刀差", row.Keys);
            }

            string csv = BoqExcelExporter.ExportToCsv(report);
            Assert.DoesNotContain("11000", csv); // 内部剪刀差未泄露
            Assert.DoesNotContain("直接成本", csv);
        }

        #endregion

        #region 2. Chief Economist View Tests (26-Column Full Breakdown & Formulas)

        [Fact]
        public void ChiefEconomistView_Headers_ContainFull26CostBreakdownColumnsPlusIndex()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            // 序号 + 26 项核心工料机利税费全要素列 = 总计 27 列
            Assert.Equal(27, report.Headers.Count);

            var expectedHeaders = new[]
            {
                "序号", "清单代号", "清单名称", "投标工程量(t)", "理论净重(t)", "1.15采购毛重(t)",
                "材料费小计(元)", "吨钢材料费(元/t)", "制作工费小计(元)", "吨钢制作费(元/t)",
                "安装工费小计(元)", "吨钢安装费(元/t)", "防火涂装小计(元)", "吨钢涂装费(元/t)",
                "直接成本总额(元)", "直接成本底价(元/t)", "费率策略", "有效管理费率(%)",
                "企业管理费(元)", "有效利润率(%)", "目标利润(元)", "对外综合单价(元/t)",
                "投标总合价(元)", "采办剪刀差创效(元)", "综合毛利总额(元)", "综合毛利率(%)", "吨钢综合净利(元/t)"
            };

            Assert.Equal(expectedHeaders, report.Headers);
        }

        [Fact]
        public void ChiefEconomistView_MathematicalConsistency_AllFormulasStrictlyTied()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            Assert.NotNull(report.UltimateCostRows);
            Assert.Equal(3, report.UltimateCostRows.Count);

            foreach (var row in report.UltimateCostRows)
            {
                // 1. 直接成本总额 = 材料费 + 制作工费 + 安装工费 + 防火涂装费 (容差 1.0 元防舍入误差)
                double sumDirectCost = row.MaterialCostSum + row.FabricationCostSum + row.InstallationCostSum + row.FireCoatingCostSum;
                Assert.True(Math.Abs(row.DirectCostSum - sumDirectCost) <= 1.0,
                    $"DirectCostSum mismatch: expected {sumDirectCost}, actual {row.DirectCostSum}");

                // 2. 吨钢直接成本底价 = 直接成本总额 / 投标工程量
                double expectedDirectPerTon = row.DirectCostSum / row.TenderQuantity;
                Assert.True(Math.Abs(row.DirectCostPerTon - expectedDirectPerTon) <= 0.1,
                    $"DirectCostPerTon mismatch: expected {expectedDirectPerTon}, actual {row.DirectCostPerTon}");

                // 3. 企业管理费 = 直接成本总额 × 有效管理费率
                double expectedMgmtFee = row.DirectCostSum * row.EffectiveManagementFeeRate;
                Assert.True(Math.Abs(row.ManagementFeeAmount - expectedMgmtFee) <= 1.0,
                    $"ManagementFeeAmount mismatch: expected {expectedMgmtFee}, actual {row.ManagementFeeAmount}");

                // 4. 目标利润 = (直接成本总额 + 企业管理费) × 有效利润率
                double expectedProfit = (row.DirectCostSum + expectedMgmtFee) * row.EffectiveProfitRate;
                Assert.True(Math.Abs(row.TargetProfitAmount - expectedProfit) <= 1.0,
                    $"TargetProfitAmount mismatch: expected {expectedProfit}, actual {row.TargetProfitAmount}");

                // 5. 对外综合单价 = 直接成本底价 × (1 + 管理费率) × (1 + 利润率)
                double expectedUnitPrice = row.DirectCostPerTon * (1.0 + row.EffectiveManagementFeeRate) * (1.0 + row.EffectiveProfitRate);
                Assert.True(Math.Abs(row.ComprehensiveUnitPrice - expectedUnitPrice) <= 0.1,
                    $"ComprehensiveUnitPrice mismatch: expected {expectedUnitPrice}, actual {row.ComprehensiveUnitPrice}");

                // 6. 投标总合价 = 投标工程量 × 对外综合单价
                double expectedTotalTender = row.TenderQuantity * row.ComprehensiveUnitPrice;
                Assert.True(Math.Abs(row.TotalTenderPrice - expectedTotalTender) <= 2.0,
                    $"TotalTenderPrice mismatch: expected {expectedTotalTender}, actual {row.TotalTenderPrice}");

                // 7. 综合毛利总额 = 目标利润 + 采办剪刀差创效
                double expectedNetProfit = row.TargetProfitAmount + row.ScissorDifferenceProfit;
                Assert.True(Math.Abs(row.ComprehensiveNetProfit - expectedNetProfit) <= 1.0,
                    $"ComprehensiveNetProfit mismatch: expected {expectedNetProfit}, actual {row.ComprehensiveNetProfit}");

                // 8. 综合毛利率 = 综合毛利总额 / 投标总合价
                double expectedMarginRate = row.ComprehensiveNetProfit / row.TotalTenderPrice;
                Assert.True(Math.Abs(row.NetMarginRate - expectedMarginRate) <= 0.001,
                    $"NetMarginRate mismatch: expected {expectedMarginRate}, actual {row.NetMarginRate}");

                // 9. 吨钢综合净利 = 综合毛利总额 / 投标工程量
                double expectedProfitPerTon = row.ComprehensiveNetProfit / row.TenderQuantity;
                Assert.True(Math.Abs(row.ComprehensiveNetProfitPerTon - expectedProfitPerTon) <= 0.1,
                    $"ComprehensiveNetProfitPerTon mismatch: expected {expectedProfitPerTon}, actual {row.ComprehensiveNetProfitPerTon}");
            }
        }

        [Fact]
        public void ChiefEconomistView_RateOverrideStatus_ReflectsCustomVsDefaultStrategy()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            var rows = report.UltimateCostRows!;
            var nhRow = rows.First(r => r.ItemTag == "NH");
            var nfRow = rows.First(r => r.ItemTag == "NF");
            var jlcRow = rows.First(r => r.ItemTag == "JLC");

            // NH: 开启单项覆盖 (管理费 10%, 利润 12%)
            Assert.Equal("★单项覆盖", nhRow.RateOverrideStatus);
            Assert.True(nhRow.IsCustomRateApplied);
            Assert.Equal(0.10, nhRow.EffectiveManagementFeeRate);
            Assert.Equal(0.12, nhRow.EffectiveProfitRate);

            // NF: 继承默认费率 (管理费 8%, 利润 7%)
            Assert.Equal("继承默认", nfRow.RateOverrideStatus);
            Assert.False(nfRow.IsCustomRateApplied);
            Assert.Equal(0.08, nfRow.EffectiveManagementFeeRate);
            Assert.Equal(0.07, nfRow.EffectiveProfitRate);

            // JLC: 开启单项覆盖 (管理费 6%, 利润 4%)
            Assert.Equal("★单项覆盖", jlcRow.RateOverrideStatus);
            Assert.True(jlcRow.IsCustomRateApplied);
            Assert.Equal(0.06, jlcRow.EffectiveManagementFeeRate);
            Assert.Equal(0.04, jlcRow.EffectiveProfitRate);
        }

        [Fact]
        public void ChiefEconomistView_ZeroQuantity_HandlesGracefullyWithoutDivideByZero()
        {
            var project = new BoqProject { ProjectName = "零工程量测试项目" };
            var item = new BoqTenderItem
            {
                ItemTag = "TEST-ZERO",
                BillItemName = "零工程量清单项",
                TenderQuantity = 0.0 // 故意设为 0
            };
            var sub = new BoqSubProject { SubProjectCode = "SUB-0" };
            sub.Parts.Add(new BoqPart { TotalNetWeight = 1000.0 });
            item.SubProjects.Add(sub);
            project.TenderItems.Add(item);

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            var row = report.UltimateCostRows!.First();
            Assert.Equal(0.0, row.TenderQuantity);
            Assert.Equal(0.0, row.MaterialTonCost);
            Assert.Equal(0.0, row.FabricationTonCost);
            Assert.Equal(0.0, row.InstallationTonCost);
            Assert.Equal(0.0, row.FireCoatingTonCost);
            // 当 TenderQuantity 为 0 时，引擎退回以构件净重 (1.0t) 核算底价，防范除以零异常
            Assert.Equal(9629.50, row.DirectCostPerTon);
            Assert.Equal(0.0, row.ComprehensiveNetProfitPerTon);
            Assert.Equal(0.0, row.NetMarginRate);

            // 彻底全 0 极值测试：工程量与构件重量均为 0
            var emptyProject = new BoqProject();
            var emptyItem = new BoqTenderItem { TenderQuantity = 0.0 };
            emptyProject.TenderItems.Add(emptyItem);
            var emptyReport = BoqAggregationEngine.GenerateAudienceReport(emptyProject, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });
            var emptyRow = emptyReport.UltimateCostRows!.First();
            Assert.Equal(0.0, emptyRow.DirectCostPerTon);
            Assert.Equal(0.0, emptyRow.DirectCostSum);
        }

        [Fact]
        public void ChiefEconomistView_NegativeScissorProfit_CalculatesNetProfitAccurately()
        {
            var project = CreateBenchmarkOlympicProject();
            // 将 NF 清单项下属零件设为严重采办超支 (实际到场单价高于业主指导价)
            var nfPart = project.TenderItems[1].SubProjects[0].Parts[0];
            nfPart.ApprovedPricePerTon = 4500.0;
            nfPart.ActualPurchasePricePerTon = 5000.0; // 采办倒贴超支 -500元/t

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            var nfRow = report.UltimateCostRows!.First(r => r.ItemTag == "NF");
            Assert.True(nfRow.ScissorDifferenceProfit < 0, "采办剪刀差利润应为负数 (采办超支)");
            // 综合毛利 = 目标利润 + 负的剪刀差
            Assert.Equal(nfRow.TargetProfitAmount + nfRow.ScissorDifferenceProfit, nfRow.ComprehensiveNetProfit, precision: 1);
        }

        #endregion

        #region 3. Procurement View Tests (Grouping & 1.15 Factor)

        [Fact]
        public void ProcurementView_GroupsPartsByGradeSpecThicknessZDir_AcrossSubProjects()
        {
            var project = new BoqProject { ProjectCode = "MERGE-TEST" };
            var item1 = new BoqTenderItem { ItemTag = "ITEM1" };
            var item2 = new BoqTenderItem { ItemTag = "ITEM2" };

            // 构件 1 内的零件 A: Q460GJC, PL, PL50*2150, 50mm, Z15
            var sub1 = new BoqSubProject { SubProjectCode = "SUB1" };
            sub1.Parts.Add(new BoqPart
            {
                SteelGrade = "Q460GJC",
                SectionType = "PL",
                SectionSpecification = "PL50*2150",
                Thickness = 50.0,
                ZDirectionGrade = "Z15",
                Quantity = 5,
                TotalNetWeight = 5000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 5750.0
            });
            item1.SubProjects.Add(sub1);

            // 构件 2 内的零件 B: 完全相同的材质规格，跨构件应自动归并
            var sub2 = new BoqSubProject { SubProjectCode = "SUB2" };
            sub2.Parts.Add(new BoqPart
            {
                SteelGrade = "Q460GJC",
                SectionType = "PL",
                SectionSpecification = "PL50*2150",
                Thickness = 50.0,
                ZDirectionGrade = "Z15",
                Quantity = 3,
                TotalNetWeight = 3000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 3450.0
            });
            item2.SubProjects.Add(sub2);

            project.TenderItems.Add(item1);
            project.TenderItems.Add(item2);

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Procurement
            });

            var rows = report.MaterialProcurementRows!;
            // 两条零件因 5 维属性完全一致，应成功归并为 1 行订货条目
            Assert.Single(rows);

            var group = rows[0];
            Assert.Equal("Q460GJC", group.MaterialGrade);
            Assert.Equal("PL", group.SectionType);
            Assert.Equal("PL50*2150", group.Specification);
            Assert.Equal(50.0, group.Thickness);
            Assert.Equal("Z15", group.ZPerformance);
            Assert.Equal(8, group.PartCount); // 5 + 3 = 8
            Assert.Equal(8000.0, group.TotalNetWeightKg); // 5000 + 3000 = 8000 kg
            Assert.Equal(8.0, group.TotalNetWeightTon);
            Assert.Equal(1.15, group.PurchasingWasteFactor);
            Assert.Equal(9200.0, group.TotalGrossWeightKg); // 8000 * 1.15 = 9200 kg
            Assert.Equal(9.2, group.TotalGrossWeightTon);
        }

        [Fact]
        public void ProcurementView_SeparatesPartsWhenAnyGroupingDimensionDiffers()
        {
            var project = new BoqProject { ProjectCode = "DIFF-TEST" };
            var item = new BoqTenderItem { ItemTag = "ITEM" };
            var sub = new BoqSubProject { SubProjectCode = "SUB" };

            // Part 1: Q460GJC, 50mm, Z15
            sub.Parts.Add(new BoqPart { SteelGrade = "Q460GJC", SectionType = "PL", SectionSpecification = "PL50*2150", Thickness = 50.0, ZDirectionGrade = "Z15", TotalNetWeight = 1000.0 });
            // Part 2: Q460GJC, 50mm, Z25 (Z向性能不同 -> 分开)
            sub.Parts.Add(new BoqPart { SteelGrade = "Q460GJC", SectionType = "PL", SectionSpecification = "PL50*2150", Thickness = 50.0, ZDirectionGrade = "Z25", TotalNetWeight = 1000.0 });
            // Part 3: Q355B, 50mm, Z15 (材质不同 -> 分开)
            sub.Parts.Add(new BoqPart { SteelGrade = "Q355B", SectionType = "PL", SectionSpecification = "PL50*2150", Thickness = 50.0, ZDirectionGrade = "Z15", TotalNetWeight = 1000.0 });
            // Part 4: Q460GJC, 60mm, Z15 (厚度不同 -> 分开)
            sub.Parts.Add(new BoqPart { SteelGrade = "Q460GJC", SectionType = "PL", SectionSpecification = "PL60*2150", Thickness = 60.0, ZDirectionGrade = "Z15", TotalNetWeight = 1000.0 });

            item.SubProjects.Add(sub);
            project.TenderItems.Add(item);

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Procurement
            });

            var rows = report.MaterialProcurementRows!;
            // 4 个零件维度各不相同，必须分成 4 个采购订货批次
            Assert.Equal(4, rows.Count);
        }

        [Fact]
        public void ProcurementView_GrossWeight_StrictlyOnePointOneFiveTimesNetWeight()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Procurement
            });

            var rows = report.MaterialProcurementRows!;
            Assert.NotEmpty(rows);

            foreach (var row in rows)
            {
                double expectedGrossKg = row.TotalNetWeightKg * 1.15;
                Assert.True(Math.Abs(row.TotalGrossWeightKg - expectedGrossKg) <= 0.05,
                    $"Gross weight mismatch: Net={row.TotalNetWeightKg}, Gross={row.TotalGrossWeightKg}, Expected={expectedGrossKg}");

                double expectedGrossTon = row.TotalNetWeightTon * 1.15;
                Assert.True(Math.Abs(row.TotalGrossWeightTon - expectedGrossTon) <= 0.005,
                    $"Gross ton mismatch: NetTon={row.TotalNetWeightTon}, GrossTon={row.TotalGrossWeightTon}, Expected={expectedGrossTon}");

                // 预算合价 = 采购毛重吨位 × 综合采购单价 (以全精度公斤数 / 1000 核算，防范吨位截断舍入误差)
                double expectedCost = (row.TotalGrossWeightKg / 1000.0) * row.MaterialUnitPricePerTon;
                Assert.True(Math.Abs(row.EstimatedProcurementCost - expectedCost) <= 0.05,
                    $"Estimated procurement cost mismatch: actual {row.EstimatedProcurementCost}, expected {expectedCost}");
            }
        }

        [Fact]
        public void ProcurementView_AggregateMaterialProcurement_ProducesIdenticalGroups()
        {
            var project = CreateBenchmarkOlympicProject();

            // 1. 调用引擎直接聚合方法
            var directList = BoqAggregationEngine.AggregateMaterialProcurement(project);

            // 2. 调用受众投影方法
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Procurement
            });
            var reportList = report.MaterialProcurementRows!;

            Assert.Equal(directList.Count, reportList.Count);

            for (int i = 0; i < directList.Count; i++)
            {
                var d = directList[i];
                var r = reportList[i];

                Assert.Equal(d.MaterialGrade, r.MaterialGrade);
                Assert.Equal(d.SectionType, r.SectionType);
                Assert.Equal(d.Specification, r.Specification);
                Assert.Equal(d.Thickness, r.Thickness);
                Assert.Equal(d.ZPerformance, r.ZPerformance);
                Assert.Equal(d.TotalNetWeightKg, r.TotalNetWeightKg);
                Assert.Equal(d.TotalGrossWeightKg, r.TotalGrossWeightKg);
                Assert.Equal(d.MaterialUnitPricePerTon, r.MaterialUnitPricePerTon);
                Assert.Equal(d.EstimatedMaterialCost, r.EstimatedProcurementCost);
            }
        }

        [Fact]
        public void ProcurementView_EdgeCases_NullOrWhitespaceGrade_DefaultsGracefully()
        {
            var project = new BoqProject();
            var item = new BoqTenderItem();
            var sub = new BoqSubProject();

            sub.Parts.Add(new BoqPart
            {
                SteelGrade = "", // 空字符串
                SectionType = null!, // null
                SectionSpecification = "   ",
                Thickness = 0.0,
                ZDirectionGrade = null!,
                TotalNetWeight = 500.0
            });

            item.SubProjects.Add(sub);
            project.TenderItems.Add(item);

            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Procurement
            });

            var rows = report.MaterialProcurementRows!;
            Assert.Single(rows);
            // 验证默认填充逻辑
            Assert.Equal("Q355B", rows[0].MaterialGrade);
            Assert.Equal("PL", rows[0].SectionType);
            Assert.Equal("无", rows[0].ZPerformance);
            Assert.Equal(575.0, rows[0].TotalGrossWeightKg);
        }

        #endregion

        #region 4. BoqExcelExporter Multi-Format Tests

        [Fact]
        public void BoqExcelExporter_Csv_Utf8Bom_EncodingAndPreambleVerified()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            // 1. 验证字符串导出包含 '\uFEFF' BOM 字符
            string csvString = BoqExcelExporter.ExportToCsv(report);
            Assert.StartsWith("\uFEFF", csvString);

            // 2. 验证二进制字节数组导出以标准 UTF-8 BOM [0xEF, 0xBB, 0xBF] 开头
            byte[] csvBytes = BoqExcelExporter.ExportToCsvBytes(report);
            Assert.True(csvBytes.Length >= 3);
            Assert.Equal(0xEF, csvBytes[0]);
            Assert.Equal(0xBB, csvBytes[1]);
            Assert.Equal(0xBF, csvBytes[2]);

            // 3. 验证紧接着 BOM 的内容不是第二个 BOM (防止重复 BOM 造成格式损坏)
            Assert.NotEqual(0xEF, csvBytes[3]);

            // 4. 解码验证
            string decoded = Encoding.UTF8.GetString(csvBytes);
            Assert.Contains("终极全成本穿透大盘", decoded);
            Assert.Contains("NH", decoded);

            // 5. 验证业主视角导出包含清单编码
            var ownerReport = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner
            });
            byte[] ownerCsvBytes = BoqExcelExporter.ExportToCsvBytes(ownerReport);
            string ownerDecoded = Encoding.UTF8.GetString(ownerCsvBytes);
            Assert.Contains("010101001001", ownerDecoded);
            Assert.Equal(0xEF, ownerCsvBytes[0]);
            Assert.Equal(0xBB, ownerCsvBytes[1]);
            Assert.Equal(0xBF, ownerCsvBytes[2]);
        }

        [Fact]
        public void BoqExcelExporter_Csv_Rfc4180Escaping_CommasQuotesNewlines()
        {
            var projection = new AudienceProjectionResult
            {
                PerspectiveName = "RFC4180测试视图",
                Headers = new List<string> { "代号", "说明", "备注" },
                Rows = new List<Dictionary<string, object?>>
                {
                    new()
                    {
                        ["代号"] = "PART-01",
                        ["说明"] = "大跨度,含逗号规格", // 包含逗号
                        ["备注"] = "普通文本"
                    },
                    new()
                    {
                        ["代号"] = "PART-02",
                        ["说明"] = "具有\"双引号\"的构件", // 包含双引号
                        ["备注"] = "第一行\n第二行换行" // 包含换行
                    }
                }
            };

            string csv = BoqExcelExporter.ExportToCsv(projection);

            // 包含逗号的字段必须用双引号包裹
            Assert.Contains("\"大跨度,含逗号规格\"", csv);
            // 包含双引号的字段双引号必须成对转义为 "" 并整体外包双引号
            Assert.Contains("\"具有\"\"双引号\"\"的构件\"", csv);
            // 包含换行的字段必须整体外包双引号
            Assert.Contains("\"第一行\n第二行换行\"", csv);
        }

        [Fact]
        public void BoqExcelExporter_XmlSpreadsheet_SyntaxValidity_ParsedByXDocument()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            string xmlString = BoqExcelExporter.ExportToXmlSpreadsheet(report, project.ProjectName);

            // 使用 System.Xml.Linq.XDocument 解析 XML，验证符合标准 XML 规范
            XDocument xDoc = XDocument.Parse(xmlString);
            Assert.NotNull(xDoc.Root);

            // 验证 Workbook 根元素与命名空间
            XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";
            Assert.Equal("Workbook", xDoc.Root.Name.LocalName);
            Assert.Equal(ss, xDoc.Root.Name.Namespace);

            // 验证 Styles 节点存在且包含 HeaderStyle 等
            var styles = xDoc.Root.Element(ss + "Styles");
            Assert.NotNull(styles);
            Assert.Contains(styles.Elements(ss + "Style"), s => s.Attribute(ss + "ID")?.Value == "HeaderStyle");
            Assert.Contains(styles.Elements(ss + "Style"), s => s.Attribute(ss + "ID")?.Value == "DataNumber");
            Assert.Contains(styles.Elements(ss + "Style"), s => s.Attribute(ss + "ID")?.Value == "HighlightRow");

            // 验证 Worksheet 存在且包含 Table
            var worksheet = xDoc.Root.Element(ss + "Worksheet");
            Assert.NotNull(worksheet);
            var table = worksheet.Element(ss + "Table");
            Assert.NotNull(table);

            // 验证表头行单元格数量与 Headers 一致
            var rows = table.Elements(ss + "Row").ToList();
            Assert.True(rows.Count >= 2); // 至少有大标题行/表头行和数据行
        }

        [Fact]
        public void BoqExcelExporter_XmlSpreadsheet_EscapesSpecialCharacters()
        {
            // 包含 XML 特殊字符 & < > " '
            var projection = new AudienceProjectionResult
            {
                PerspectiveName = "XML特殊字符测试",
                Headers = new List<string> { "特征", "数学符号", "HTML代码" },
                Rows = new List<Dictionary<string, object?>>
                {
                    new()
                    {
                        ["特征"] = "Q460GJC & Q355B 混合焊",
                        ["数学符号"] = "厚度 t < 50mm 且 宽度 w > 1200mm",
                        ["HTML代码"] = "<tag attr='value'>\"Hello\"</tag>"
                    }
                }
            };

            string xml = BoqExcelExporter.ExportToXmlSpreadsheet(projection, "项目 & 工程 <测试>");

            // 必须能够被 XDocument 顺利解析，绝对不抛出 XmlException
            var xDoc = XDocument.Parse(xml);
            Assert.NotNull(xDoc.Root);

            // 检查在生成的文本中实体已被转义
            Assert.Contains("&amp;", xml);
            Assert.Contains("&lt;", xml);
            Assert.Contains("&gt;", xml);
            Assert.DoesNotContain("<tag", xml); // 未转义的裸标签严禁出现
        }

        [Fact]
        public void BoqExcelExporter_XmlSpreadsheet_NumericFormatting_InvariantCulture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                // 将当前线程设为法语 (以逗号作为小数点的区域文化)
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

                var projection = new AudienceProjectionResult
                {
                    PerspectiveName = "区域文化数字测试",
                    Headers = new List<string> { "单价值", "净重吨位", "利润率" },
                    Rows = new List<Dictionary<string, object?>>
                    {
                        new()
                        {
                            ["单价值"] = 12345.67,
                            ["净重吨位"] = 890.123,
                            ["利润率"] = 0.08
                        }
                    }
                };

                string xml = BoqExcelExporter.ExportToXmlSpreadsheet(projection);

                // 在 XML Spreadsheet 规范中，数字必须使用点 "." 分隔，严禁使用逗号 ","
                Assert.Contains("12345.67", xml);
                Assert.Contains("890.123", xml);
                Assert.Contains("0.08", xml);
                Assert.DoesNotContain("12345,67", xml);

                // 能够被 XDocument 顺利解析
                var doc = XDocument.Parse(xml);
                Assert.NotNull(doc);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void BoqExcelExporter_MultiPerspectiveXmlSpreadsheet_ContainsAllFiveWorksheets()
        {
            var project = CreateBenchmarkOlympicProject();

            // 导出全要素 5 大视角合一工作簿
            string multiXml = BoqExcelExporter.ExportMultiPerspectiveXmlSpreadsheet(project);

            // 1. XML 格式有效性检验
            var xDoc = XDocument.Parse(multiXml);
            Assert.NotNull(xDoc.Root);

            XNamespace ss = "urn:schemas-microsoft-com:office:spreadsheet";

            // 2. 验证正好包含 5 个独立的 Worksheet 标签页
            var worksheets = xDoc.Root.Elements(ss + "Worksheet").ToList();
            Assert.Equal(5, worksheets.Count);

            // 3. 验证 5 大受众专属 Tab 工作表名称
            var sheetNames = worksheets.Select(w => w.Attribute(ss + "Name")?.Value).ToList();
            var expectedSheetNames = new[]
            {
                "1.业主审计报价 (Owner)",
                "2.终极全成本大盘 (Chief)",
                "3.车间制作排产 (Shop)",
                "4.现场吊装施工 (Site)",
                "5.材料采办订货 (Procure)"
            };

            foreach (var expectedName in expectedSheetNames)
            {
                Assert.Contains(expectedName, sheetNames);
            }

            // 4. 验证每个工作表名称长度未超过 Excel 限制 (31 字符)
            foreach (var name in sheetNames)
            {
                Assert.NotNull(name);
                Assert.True(name.Length <= 31, $"Sheet name '{name}' exceeds Excel 31-char limit");
            }

            // 5. 验证各工作表均有有效的数据行
            foreach (var ws in worksheets)
            {
                var table = ws.Element(ss + "Table");
                Assert.NotNull(table);
                Assert.NotEmpty(table.Elements(ss + "Row"));
            }

            // 6. 验证字节流导出
            byte[] xmlBytes = BoqExcelExporter.ExportMultiPerspectiveXmlSpreadsheetBytes(project);
            Assert.NotEmpty(xmlBytes);
            string decoded = Encoding.UTF8.GetString(xmlBytes);
            Assert.Contains("5.材料采办订货 (Procure)", decoded);
        }

        [Fact]
        public void BoqExcelExporter_Html_RenderingStructure_ContainsTableStylesAndClasses()
        {
            var project = CreateBenchmarkOlympicProject();
            var report = BoqAggregationEngine.GenerateAudienceReport(project, new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.ChiefEconomist
            });

            string html = BoqExcelExporter.ExportToHtml(report);

            // 1. 验证 HTML 标准基础标签
            Assert.Contains("<!DOCTYPE html>", html);
            Assert.Contains("<html", html);
            Assert.Contains("<table", html);
            Assert.Contains("<thead", html);
            Assert.Contains("<tbody", html);
            Assert.Contains("</table>", html);
            Assert.Contains("</html>", html);

            // 2. 验证受众标题
            Assert.Contains("<h2>终极全成本穿透大盘</h2>", html);

            // 3. 验证表头列渲染
            foreach (var header in report.Headers)
            {
                Assert.Contains($"<th>{header}</th>", html);
            }

            // 4. 验证样式类及独立覆盖费率高亮样式 (NH 清单项)
            Assert.Contains("class=\"highlight\"", html);

            // 5. 验证数字格式类
            Assert.Contains("class=\"num\"", html);
            Assert.Contains("class=\"weight\"", html);

            // 6. 验证底部核心指标大盘卡片
            Assert.Contains("class=\"summary-card\"", html);
            Assert.Contains("核心指标大盘汇总", html);
            Assert.Contains("全项目总投标报价(元)", html);

            // 7. 验证 HTML 字节流导出与 UTF-8 编码解码一致性
            byte[] htmlBytes = BoqExcelExporter.ExportToHtmlBytes(report);
            Assert.NotEmpty(htmlBytes);
            string decodedHtml = Encoding.UTF8.GetString(htmlBytes);
            Assert.StartsWith("<!DOCTYPE html>", decodedHtml);
            Assert.Contains("终极全成本穿透大盘", decodedHtml);
        }

        [Fact]
        public void BoqExcelExporter_MetadataUtilities_FileNamesAndMimeTypes()
        {
            var project = new BoqProject { ProjectCode = "NB-ATZX" };

            // 1. 文件名生成测试
            string ownerCsvName = BoqExcelExporter.GetRecommendedFileName(project, BoqAudiencePerspective.Owner, "csv");
            Assert.StartsWith("NB-ATZX_业主审计对外清单_", ownerCsvName);
            Assert.EndsWith(".csv", ownerCsvName);

            string chiefXmlName = BoqExcelExporter.GetRecommendedFileName(project, BoqAudiencePerspective.ChiefEconomist, "xml");
            Assert.StartsWith("NB-ATZX_终极全成本穿透大盘_", chiefXmlName);
            Assert.EndsWith(".xml", chiefXmlName);

            string multiXmlName = BoqExcelExporter.GetMultiSheetRecommendedFileName(project, "xml");
            Assert.StartsWith("NB-ATZX_多视角全要素商务工程量清单_", multiXmlName);
            Assert.EndsWith(".xml", multiXmlName);

            // 2. MIME Content-Type 测试
            Assert.Equal("text/csv; charset=utf-8", BoqExcelExporter.GetContentType("csv"));
            Assert.Equal("text/csv; charset=utf-8", BoqExcelExporter.GetContentType(".csv"));
            Assert.Equal("application/vnd.ms-excel", BoqExcelExporter.GetContentType("xml"));
            Assert.Equal("application/vnd.ms-excel", BoqExcelExporter.GetContentType(".xls"));
            Assert.Equal("text/html; charset=utf-8", BoqExcelExporter.GetContentType("html"));
            Assert.Equal("application/octet-stream", BoqExcelExporter.GetContentType("bin"));
        }

        #endregion

        #region 5. High-Throughput & Extreme Character Stress Tests

        [Fact]
        public void StressTest_LargeScaleProject_1000Parts_GeneratesAllProjectionsAndMultiSheetXmlUnderTwoSeconds()
        {
            var project = new BoqProject
            {
                ProjectCode = "LARGE-1000",
                ProjectName = "超大规模钢结构体育场全要素测算",
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var grades = new[] { "Q355B", "Q460GJC", "Q235B" };
            var specs = new[] { "PL20*1500", "PL50*2150", "PIP1200*50", "PIP500*20", "BOX400*400*25" };
            var zDirs = new[] { "无", "Z15", "Z25" };

            // 生成 5 个清单项，每个清单项 10 个构件，每个构件 20 个零件 = 总计 1,000 个零件
            for (int t = 1; t <= 5; t++)
            {
                var tenderItem = new BoqTenderItem
                {
                    Id = t,
                    ItemTag = $"ITEM-{t:D2}",
                    BillItemCode = $"01010100{t:D4}",
                    BillItemName = $"分项工程-{t}",
                    TenderQuantity = 500.0,
                    IsCustomRateEnabled = t % 2 == 1,
                    CustomManagementFeeRate = 0.10,
                    CustomProfitRate = 0.12
                };

                for (int s = 1; s <= 10; s++)
                {
                    var sub = new BoqSubProject
                    {
                        Id = t * 100 + s,
                        SubProjectCode = $"SUB-{t}-{s:D2}",
                        SubProjectName = $"子构件-{t}-{s}",
                        StructuralRole = "主桁架",
                        MaxPlateThickness = 45.0,
                        AssemblyElevation = 25.0,
                        Quantity = 1
                    };

                    for (int p = 1; p <= 20; p++)
                    {
                        string g = grades[(t + s + p) % grades.Length];
                        string sp = specs[(t + s + p) % specs.Length];
                        string z = zDirs[(t + s + p) % zDirs.Length];

                        sub.Parts.Add(new BoqPart
                        {
                            Id = t * 10000 + s * 100 + p,
                            PartMark = $"P-{t}-{s}-{p}",
                            SteelGrade = g,
                            SectionType = sp.StartsWith("PIP") ? "PIP" : (sp.StartsWith("BOX") ? "BOX" : "PL"),
                            SectionSpecification = sp,
                            Thickness = sp.Contains("50") ? 50.0 : 20.0,
                            ZDirectionGrade = z,
                            Quantity = 2,
                            TotalNetWeight = 250.0,
                            PurchasingWasteFactor = 1.15,
                            TotalGrossWeight = 250.0 * 1.15,
                            ApprovedPricePerTon = 5200.0,
                            ActualPurchasePricePerTon = 4800.0
                        });
                    }

                    tenderItem.SubProjects.Add(sub);
                }

                project.TenderItems.Add(tenderItem);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 1. 全项目卷积核算
            var costResult = BoqAggregationEngine.CalculateProject(project);
            Assert.True(costResult.TotalNetWeightTon > 0);

            // 2. 全部 5 大受众投影报表生成
            var ownerReport = BoqAggregationEngine.ProjectToAudience(costResult, BoqAudiencePerspective.Owner, project);
            var chiefReport = BoqAggregationEngine.ProjectToAudience(costResult, BoqAudiencePerspective.ChiefEconomist, project);
            var shopReport = BoqAggregationEngine.ProjectToAudience(costResult, BoqAudiencePerspective.FabricationShop, project);
            var siteReport = BoqAggregationEngine.ProjectToAudience(costResult, BoqAudiencePerspective.SiteProjectDept, project);
            var procureReport = BoqAggregationEngine.ProjectToAudience(costResult, BoqAudiencePerspective.Procurement, project);

            Assert.Equal(5, ownerReport.Rows.Count);
            Assert.Equal(5, chiefReport.Rows.Count);
            Assert.Equal(50, shopReport.Rows.Count);
            Assert.Equal(50, siteReport.Rows.Count);
            Assert.True(procureReport.Rows.Count > 0);

            // 3. 5 表合一多工作表 XML 电子表格生成
            string multiXml = BoqExcelExporter.ExportMultiPerspectiveXmlSpreadsheet(project, costResult);
            Assert.NotEmpty(multiXml);

            stopwatch.Stop();

            // 验证 1,000 零件规模核算与导出耗时在 2,000 毫秒以内 (高吞吐性能)
            Assert.True(stopwatch.ElapsedMilliseconds < 2000,
                $"Large scale benchmark took too long: {stopwatch.ElapsedMilliseconds} ms");

            // 验证生成的 XML 具备完全合法的 XML 结构
            var xDoc = XDocument.Parse(multiXml);
            Assert.NotNull(xDoc.Root);
        }

        [Fact]
        public void StressTest_ExtremeTextCharacters_UnicodeEmojiAndChinesePunctuation_ExportedCleanly()
        {
            var projection = new AudienceProjectionResult
            {
                PerspectiveName = "极限特殊字符测试【特殊符号】",
                Headers = new List<string> { "代号", "富文本描述", "国际工程标注" },
                Rows = new List<Dictionary<string, object?>>
                {
                    new()
                    {
                        ["代号"] = "PART-★-01",
                        ["富文本描述"] = "采用《钢结构设计标准》(GB 50017) & 「优质特厚钢板」: PL50*2150 (Z15)，单价 €5,000 / ¥38,000",
                        ["国际工程标注"] = "🏗️ Super-Truss Section #1 @ High Altitude [Level +35.0m] \\ Path / Cut"
                    }
                }
            };

            // 1. CSV 导出与转义测试
            string csv = BoqExcelExporter.ExportToCsv(projection);
            Assert.Contains("PART-★-01", csv);
            Assert.Contains("Super-Truss Section #1", csv);
            Assert.Contains("🏗️", csv);

            // 2. XML 导出与 XML 语法合法性测试
            string xml = BoqExcelExporter.ExportToXmlSpreadsheet(projection);
            var xDoc = XDocument.Parse(xml);
            Assert.NotNull(xDoc.Root);

            // 3. HTML 导出与实体转义测试
            string html = BoqExcelExporter.ExportToHtml(projection);
            Assert.Contains("&amp;", html);
            Assert.Contains("PART-★-01", html);
        }

        #endregion
    }
}

