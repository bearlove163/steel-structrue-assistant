// ============================================================================
// 文件归属: src/EngineeringApp.Tests/BoqWorkbenchComponentTests.cs
// 模块功能: Milestone 4 Blazor WebAssembly BOQ 工作台与反应式状态机经验化验证测试套件
// 验证范围:
//   1. 5 大受众视角投影完整性与业主脱敏审计隔离 (Owner, ChiefEconomist, FabricationShop, SiteProjectDept, Procurement)
//   2. 交互式独立费率单项覆盖与双向无漂移往返重算 (IsCustomRateEnabled / CustomRates Round-Trip Zero-Drift)
//   3. 构件树表展开状态跟踪器 (HashSet<int> 幂等性、对称性与实体ID零碰撞实证挑战)
//   4. 跨构件原材料集中采办归并与 1.15 采购损耗毛重及剪刀差核算
//   5. 纯 C# 多工作表 SpreadsheetML 2003 XML 与 UTF-8 BOM CSV 导出完备性与 XML 语法合法性
//   6. 结构系统、耐火时限及全成本瀑布辅助聚合算法经验化校验
// ============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Export;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    public class BoqWorkbenchComponentTests
    {
        #region Test Fixture Helpers

        private static BoqProject CreateBenchmarkProject()
        {
            return NingboOlympicSeedData.CreateBenchmarkProject();
        }

        #endregion

        #region 1. Perspective Switching & Owner Desensitization Tests

        [Fact]
        public void Perspective_OwnerAuditView_StrictlyDesensitizesInternalDirectCostsAndProfits()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);
            var config = new BoqReportConfig
            {
                AudiencePerspective = BoqAudiencePerspective.Owner,
                ShowGrossWeight = true,
                MaskInternalDirectCosts = true
            };

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.Owner, project, config);

            // Assert
            Assert.NotNull(projection);
            Assert.Equal(BoqAudiencePerspective.Owner, projection.Perspective);
            Assert.True(projection.Rows.Count > 0, "业主清单投影行数必须大于0");
            Assert.Equal(project.TenderItems.Count, projection.Rows.Count);

            // 验证表头中绝对严禁出现敏感内部直接成本与套利剪刀差列名
            string[] forbiddenHeaderSubstrings = new[]
            {
                "直接成本", "底价", "采办剪刀差", "套利", "利润率", "管理费率",
                "工料机", "材料到场价", "制作工费", "安装工费"
            };

            foreach (var header in projection.Headers)
            {
                foreach (var forbidden in forbiddenHeaderSubstrings)
                {
                    Assert.DoesNotContain(forbidden, header);
                }
            }

            // 验证数据行中只包含公开商务数据 (GB 50500 清单计价列)
            foreach (var row in projection.Rows)
            {
                Assert.True(row.ContainsKey("清单编码"));
                Assert.True(row.ContainsKey("综合单价(元)"));
                Assert.True(row.ContainsKey("合价(元)"));

                // 敏感内部键必须不存在
                Assert.False(row.ContainsKey("直接成本"));
                Assert.False(row.ContainsKey("采办经营创效"));
                Assert.False(row.ContainsKey("实际采办价"));
            }

            // 验证合价总和严格等于项目投标总报价
            if (projection.ClientTenderRows != null && projection.ClientTenderRows.Count > 0)
            {
                double sumTenderPrice = projection.ClientTenderRows.Sum(r => r.TotalTenderPrice);
                Assert.True(Math.Abs(projectResult.TotalBidPrice - sumTenderPrice) < 0.05,
                    $"合价总和({sumTenderPrice:F2})与项目总报价({projectResult.TotalBidPrice:F2})差异必须小于5分钱(四舍五入尾差)");
            }
        }

        [Fact]
        public void Perspective_ChiefEconomist_ProvidesFullCostPenetrationMatrix()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.ChiefEconomist, project);

            // Assert
            Assert.NotNull(projection);
            Assert.Equal(BoqAudiencePerspective.ChiefEconomist, projection.Perspective);
            Assert.Equal(project.TenderItems.Count, projection.Rows.Count);

            // 验证全成本穿透表头包含全部直接与间接成本维度
            Assert.Contains(projection.Headers, h => h.Contains("材料"));
            Assert.Contains(projection.Headers, h => h.Contains("制作"));
            Assert.Contains(projection.Headers, h => h.Contains("安装"));
            Assert.Contains(projection.Headers, h => h.Contains("涂装"));
            Assert.Contains(projection.Headers, h => h.Contains("管理费"));
            Assert.Contains(projection.Headers, h => h.Contains("利润"));
            Assert.Contains(projection.Headers, h => h.Contains("创效") || h.Contains("剪刀差"));

            // 核心数学恒等式校验: 直接成本 + 管理费 + 利润 == 投标总价
            double directSum = projectResult.TotalDirectCost;
            double mgmtSum = projectResult.TotalManagementFee;
            double profitSum = projectResult.TotalProfit;
            double bidSum = projectResult.TotalBidPrice;

            Assert.Equal(bidSum, directSum + mgmtSum + profitSum, precision: 2);
            Assert.True(projectResult.TotalProcurementScissorProfit > 0, "采办经营创效剪刀差必须为正创效");
        }

        [Fact]
        public void Perspective_FabricationShop_ExposesKThickAndLaborDays()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.FabricationShop, project);

            // Assert
            Assert.NotNull(projection);
            Assert.Equal(BoqAudiencePerspective.FabricationShop, projection.Perspective);

            // 验证构件分段排产行数等于全工程构件总数 (13个分段)
            int totalSubProjects = project.TenderItems.Sum(t => t.SubProjects.Count);
            Assert.Equal(totalSubProjects, projection.Rows.Count);

            // 验证车间工艺表头包含板厚与工日字段
            Assert.Contains(projection.Headers, h => h.Contains("K_Thick") || h.Contains("厚板难度"));
            Assert.Contains(projection.Headers, h => h.Contains("相贯"));
            Assert.Contains(projection.Headers, h => h.Contains("探伤") || h.Contains("UT"));
            Assert.Contains(projection.Headers, h => h.Contains("工日"));

            // 验证摘要指标
            Assert.True(projection.SummaryMetrics.ContainsKey("车间总需求工时(工日)"));
            Assert.True(projection.SummaryMetrics.ContainsKey("车间总加工净重(t)"));
        }

        [Fact]
        public void Perspective_SiteProjectDept_ExposesElevationWeldWeightFactors()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.SiteProjectDept, project);

            // Assert
            Assert.NotNull(projection);
            Assert.Equal(BoqAudiencePerspective.SiteProjectDept, projection.Perspective);
            Assert.Equal(project.TenderItems.Sum(t => t.SubProjects.Count), projection.Rows.Count);

            // 验证现场吊装表头包含高空、现场焊与吊重系数
            Assert.Contains(projection.Headers, h => h.Contains("K_Elev") || h.Contains("标高"));
            Assert.Contains(projection.Headers, h => h.Contains("K_SiteWeld") || h.Contains("现场焊"));
            Assert.Contains(projection.Headers, h => h.Contains("K_Weight") || h.Contains("吊装重量"));
            Assert.Contains(projection.Headers, h => h.Contains("机械配置建议"));
        }

        [Fact]
        public void Perspective_Procurement_ConsolidatesRawMaterialsWith115Waste()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.Procurement, project);

            // Assert
            Assert.NotNull(projection);
            Assert.Equal(BoqAudiencePerspective.Procurement, projection.Perspective);
            Assert.True(projection.Rows.Count > 0);

            // 验证采办订货表头包含 1.15 毛重、五维加价与剪刀差
            Assert.Contains(projection.Headers, h => h.Contains("1.15"));
            Assert.Contains(projection.Headers, h => h.Contains("板厚阶梯加价"));
            Assert.Contains(projection.Headers, h => h.Contains("材质牌号加价"));
            Assert.Contains(projection.Headers, h => h.Contains("Z向性能加价"));
            Assert.Contains(projection.Headers, h => h.Contains("剪刀差"));

            if (projection.MaterialProcurementRows != null)
            {
                foreach (var row in projection.MaterialProcurementRows)
                {
                    Assert.Equal(1.15, row.PurchasingWasteFactor);
                    Assert.Equal(row.TotalNetWeightTon * 1.15, row.TotalGrossWeightTon, precision: 2);
                }
            }
        }

        #endregion

        #region 2. Interactive Fee Rate Override & Zero-Drift Recalculation Tests

        [Fact]
        public void FeeRateOverride_TogglingCustomRatesOnNF_RecalculatesAccuratelyAndDriftFree()
        {
            // Arrange: NF 清单项在基准数据中初始为未启用覆盖 (IsCustomRateEnabled = false, 继承全局 8% 和 7%)
            var project = CreateBenchmarkProject();
            var initialResult = BoqAggregationEngine.CalculateProject(project);
            double initialTotalBid = initialResult.TotalBidPrice;
            var nfItem = project.TenderItems.First(t => t.ItemTag == "NF");
            double initialNfUnitPrice = nfItem.ComprehensiveUnitPrice;
            double initialNfTotalPrice = nfItem.TotalTenderPrice;

            Assert.False(nfItem.IsCustomRateEnabled, "NF 初始必须继承项目默认费率");

            // Act 1: 启用 NF 自定义独立费率 (管理费 12%, 利润率 10%)
            nfItem.IsCustomRateEnabled = true;
            nfItem.CustomManagementFeeRate = 0.12;
            nfItem.CustomProfitRate = 0.10;

            BoqAggregationEngine.RollupCostToTenderItem(nfItem, project);
            var updatedResult1 = BoqAggregationEngine.CalculateProject(project);

            // Assert 1: NF 单价与总价必须增加，全项目总投标额随之增加
            Assert.True(nfItem.ComprehensiveUnitPrice > initialNfUnitPrice);
            Assert.True(nfItem.TotalTenderPrice > initialNfTotalPrice);
            Assert.True(updatedResult1.TotalBidPrice > initialTotalBid);

            // 精准公式验证: TenderQuantity * DirectCostPerTon * (1 + 0.12) * (1 + 0.10)
            double expectedNfTotal = nfItem.TenderQuantity * nfItem.DirectCostPerTon * (1.0 + 0.12) * (1.0 + 0.10);
            Assert.Equal(expectedNfTotal, nfItem.TotalTenderPrice, precision: 2);

            // Act 2: 恢复为继承默认 (IsCustomRateEnabled = false)
            nfItem.IsCustomRateEnabled = false;
            BoqAggregationEngine.RollupCostToTenderItem(nfItem, project);
            var restoredResult = BoqAggregationEngine.CalculateProject(project);

            // Assert 2: 零漂移恢复至初始基准
            Assert.Equal(initialNfUnitPrice, nfItem.ComprehensiveUnitPrice, precision: 4);
            Assert.Equal(initialNfTotalPrice, nfItem.TotalTenderPrice, precision: 4);
            Assert.Equal(initialTotalBid, restoredResult.TotalBidPrice, precision: 4);
        }

        [Fact]
        public void FeeRateOverride_TogglingCustomRatesOnNH_RecalculatesAccuratelyAndDriftFree()
        {
            // Arrange: NH 清单项在基准数据中初始为已启用覆盖 (10% 管理费, 12% 利润)
            var project = CreateBenchmarkProject();
            var initialResult = BoqAggregationEngine.CalculateProject(project);
            double initialTotalBid = initialResult.TotalBidPrice;
            var nhItem = project.TenderItems.First(t => t.ItemTag == "NH");
            double initialNhUnitPrice = nhItem.ComprehensiveUnitPrice;
            double initialNhTotalPrice = nhItem.TotalTenderPrice;

            Assert.True(nhItem.IsCustomRateEnabled);
            Assert.Equal(0.10, nhItem.CustomManagementFeeRate);
            Assert.Equal(0.12, nhItem.CustomProfitRate);

            // Act 1: 禁用 NH 自定义独立费率 (切换为继承默认 8% 和 7%)
            nhItem.IsCustomRateEnabled = false;
            BoqAggregationEngine.RollupCostToTenderItem(nhItem, project);
            var updatedResult1 = BoqAggregationEngine.CalculateProject(project);

            // Assert 1: 降为 8% 和 7%，单价与合价必须降低
            Assert.True(nhItem.ComprehensiveUnitPrice < initialNhUnitPrice);
            Assert.True(nhItem.TotalTenderPrice < initialNhTotalPrice);
            Assert.True(updatedResult1.TotalBidPrice < initialTotalBid);

            double expectedNhDefaultTotal = nhItem.TenderQuantity * nhItem.DirectCostPerTon * (1.0 + 0.08) * (1.0 + 0.07);
            Assert.Equal(expectedNhDefaultTotal, nhItem.TotalTenderPrice, precision: 2);

            // Act 2: 重新开启覆盖，并恢复为 10% 和 12%
            nhItem.IsCustomRateEnabled = true;
            nhItem.CustomManagementFeeRate = 0.10;
            nhItem.CustomProfitRate = 0.12;
            BoqAggregationEngine.RollupCostToTenderItem(nhItem, project);
            var restoredResult = BoqAggregationEngine.CalculateProject(project);

            // Assert 2: 严格零数值漂移恢复
            Assert.Equal(initialNhUnitPrice, nhItem.ComprehensiveUnitPrice, precision: 4);
            Assert.Equal(initialNhTotalPrice, nhItem.TotalTenderPrice, precision: 4);
            Assert.Equal(initialTotalBid, restoredResult.TotalBidPrice, precision: 4);
        }

        [Fact]
        public void FeeRateOverride_100CyclesRapidToggleStressTest_AccumulatesZeroNumericalDrift()
        {
            // Arrange: 使用 NF 清单项进行 100 次往返快速开关
            var project = CreateBenchmarkProject();
            var initialResult = BoqAggregationEngine.CalculateProject(project);
            double baseBidPrice = initialResult.TotalBidPrice;
            var nfItem = project.TenderItems.First(t => t.ItemTag == "NF");
            double baseItemPrice = nfItem.TotalTenderPrice;

            // Act: 快速往返切换 100 次
            for (int cycle = 0; cycle < 100; cycle++)
            {
                nfItem.IsCustomRateEnabled = true;
                nfItem.CustomManagementFeeRate = 0.05 + (cycle % 10) * 0.01;
                nfItem.CustomProfitRate = 0.04 + (cycle % 10) * 0.01;
                BoqAggregationEngine.RollupCostToTenderItem(nfItem, project);
                BoqAggregationEngine.CalculateProject(project);

                nfItem.IsCustomRateEnabled = false;
                BoqAggregationEngine.RollupCostToTenderItem(nfItem, project);
                BoqAggregationEngine.CalculateProject(project);
            }

            var finalResult = BoqAggregationEngine.CalculateProject(project);

            // Assert: 100 次切换后无数值漂移
            Assert.Equal(baseItemPrice, nfItem.TotalTenderPrice, precision: 6);
            Assert.Equal(baseBidPrice, finalResult.TotalBidPrice, precision: 6);
        }

        #endregion

        #region 3. Tree-Grid Expansion Tracking Tests

        [Fact]
        public void TreeGrid_ExpansionTracking_IdempotencyAndToggleSymmetry()
        {
            // Arrange: 模拟 BoqWorkbench.razor 中的 HashSet<int> 状态管理器
            var expandedTenderItemIds = new HashSet<int>();
            var expandedSubProjectIds = new HashSet<int>();

            void ToggleTender(int id)
            {
                if (!expandedTenderItemIds.Add(id))
                    expandedTenderItemIds.Remove(id);
            }

            void ToggleSub(int id)
            {
                if (!expandedSubProjectIds.Add(id))
                    expandedSubProjectIds.Remove(id);
            }

            // Act & Assert 1: 单次添加 -> 展开 (包含)
            ToggleTender(1);
            Assert.Contains(1, expandedTenderItemIds);
            Assert.Single(expandedTenderItemIds);

            ToggleSub(101);
            Assert.Contains(101, expandedSubProjectIds);
            Assert.Single(expandedSubProjectIds);

            // Act & Assert 2: 再次点击 -> 折叠 (移除)
            ToggleTender(1);
            Assert.DoesNotContain(1, expandedTenderItemIds);
            Assert.Empty(expandedTenderItemIds);

            ToggleSub(101);
            Assert.DoesNotContain(101, expandedSubProjectIds);
            Assert.Empty(expandedSubProjectIds);

            // Act & Assert 3: 奇数次切换展开，偶数次切换折叠
            for (int i = 0; i < 99; i++)
            {
                ToggleTender(2);
            }
            Assert.Contains(2, expandedTenderItemIds);

            ToggleTender(2);
            Assert.DoesNotContain(2, expandedTenderItemIds);
        }

        #endregion

        #region 4. Cross-Component Material Procurement Aggregation Tests

        [Fact]
        public void MaterialProcurement_AggregateMaterialProcurement_ConsolidatesCorrectly()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var procurementList = BoqAggregationEngine.AggregateMaterialProcurement(project);

            // Assert
            Assert.NotNull(procurementList);
            Assert.True(procurementList.Count > 0, "采办材料归并列表不能为空");

            // 1. 质量守恒定理校验:
            // 归并后的理论总净重必须等于项目工程总净重
            double totalNetKg = procurementList.Sum(p => p.TotalNetWeightKg);
            Assert.Equal(projectResult.TotalNetWeightKg, totalNetKg, precision: 1);

            // 归并后的采购毛重必须严格为理论净重的 1.15 倍
            double totalGrossKg = procurementList.Sum(p => p.TotalGrossWeightKg);
            Assert.Equal(projectResult.TotalGrossWeightKg, totalGrossKg, precision: 1);

            // 2. 跨构件归并验证: 相同材质、厚度、截面的零件聚合在同一条采购记录中
            var duplicates = procurementList
                .GroupBy(p => new { p.MaterialGrade, p.SectionType, p.Specification, p.Thickness, p.ZPerformance })
                .Where(g => g.Count() > 1)
                .ToList();
            Assert.Empty(duplicates);

            // 3. 校验宁波奥体特种钢材 Q460GJC 主桁架钢管与钢板
            var q460Items = procurementList.Where(p => p.MaterialGrade.Contains("460")).ToList();
            Assert.NotEmpty(q460Items);
            foreach (var q in q460Items)
            {
                Assert.True(q.Thickness >= 30.0, "Q460GJC 特种主桁架钢板/管壁厚度必须 >= 30mm");
                Assert.True(q.MaterialUnitPricePerTon > 4800.0, "特厚高强钢板到场单价必须包含厚度与材质及Z向加价");
            }
        }

        [Fact]
        public void MaterialProcurement_ProjectToMaterialProcurementOrder_MatchesTab3Expectations()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var orderProjection = BoqAggregationEngine.ProjectToMaterialProcurementOrder(projectResult, project);

            // Assert
            Assert.NotNull(orderProjection.MaterialProcurementRows);
            Assert.True(orderProjection.MaterialProcurementRows.Count > 0);

            double sumScissor = orderProjection.MaterialProcurementRows.Sum(r => r.ScissorDifferenceProfit);
            Assert.Equal(projectResult.TotalProcurementScissorProfit, sumScissor, precision: 2);

            // 验证每条记录的订货建议与五维加价
            foreach (var row in orderProjection.MaterialProcurementRows)
            {
                Assert.False(string.IsNullOrWhiteSpace(row.MillOrderingNotes));
                Assert.True(row.MaterialUnitPricePerTon >= row.BaseNetPricePerTon);
                Assert.True(row.EstimatedProcurementCost > 0);
            }
        }

        #endregion

        #region 5. Excel & CSV Export Data Generation Tests

        [Fact]
        public void Export_MultiPerspectiveSpreadsheetXml_GeneratesValid5SheetWorkbook()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            byte[] xmlBytes = BoqExcelExporter.ExportMultiPerspectiveXmlSpreadsheetBytes(project, projectResult);

            // Assert
            Assert.NotNull(xmlBytes);
            Assert.True(xmlBytes.Length > 1024, "导出的 XML 电子表格字节数必须超过 1 KB");

            string xmlString = Encoding.UTF8.GetString(xmlBytes);
            Assert.StartsWith("<?xml", xmlString.TrimStart());

            // 校验 XML 语法有效性 (XDocument.Parse 不抛异常)
            var xDoc = XDocument.Parse(xmlString);
            Assert.NotNull(xDoc.Root);

            XNamespace ssNs = "urn:schemas-microsoft-com:office:spreadsheet";
            var worksheets = xDoc.Root.Elements(ssNs + "Worksheet").ToList();

            // 必须严格包含全部 5 大受众独立 Tab 标签页
            Assert.Equal(5, worksheets.Count);

            var sheetNames = worksheets.Select(w => (string?)w.Attribute(ssNs + "Name")).ToList();
            Assert.Contains(sheetNames, s => s != null && s.Contains("Owner"));
            Assert.Contains(sheetNames, s => s != null && s.Contains("Chief"));
            Assert.Contains(sheetNames, s => s != null && s.Contains("Shop"));
            Assert.Contains(sheetNames, s => s != null && s.Contains("Site"));
            Assert.Contains(sheetNames, s => s != null && s.Contains("Procure"));

            // 验证每个工作表内都包含非空的 Table 与 Row 元素
            foreach (var sheet in worksheets)
            {
                var table = sheet.Element(ssNs + "Table");
                Assert.NotNull(table);
                var rows = table.Elements(ssNs + "Row").ToList();
                Assert.True(rows.Count >= 2, $"工作表 {sheet.Attribute(ssNs + "Name")?.Value} 至少应包含表头和数据行");
            }
        }

        [Fact]
        public void Export_CsvBytes_ContainsUtf8BomAndValidEscapedContent()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.ChiefEconomist, project);

            // Act
            byte[] csvBytes = BoqExcelExporter.ExportToCsvBytes(projection);

            // Assert
            Assert.NotNull(csvBytes);
            Assert.True(csvBytes.Length > 100);

            // 验证首部 3 字节为 UTF-8 BOM: 0xEF, 0xBB, 0xBF (Windows Excel 防乱码标准)
            Assert.Equal(0xEF, csvBytes[0]);
            Assert.Equal(0xBB, csvBytes[1]);
            Assert.Equal(0xBF, csvBytes[2]);

            string csvContent = Encoding.UTF8.GetString(csvBytes);
            Assert.Contains("终极全成本穿透大盘", csvContent);
            Assert.Contains("材料", csvContent);
        }

        #endregion

        #region 6. Workbench Analytics Helpers Stress Tests

        [Fact]
        public void Analytics_StructuralSystemAndFireRatingCalculations_MatchProjectTotals()
        {
            // Arrange
            var project = CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act 1: 结构系统聚合 (模拟 BoqWorkbench.razor 中的 GetStructuralSystemAnalytics)
            var allSubs = project.TenderItems.SelectMany(t => t.SubProjects).ToList();
            var systemGroups = allSubs
                .GroupBy(s => string.IsNullOrWhiteSpace(s.StructuralRole) ? "其他结构" : s.StructuralRole)
                .Select(g => new
                {
                    SystemName = g.Key,
                    NetWeightTon = g.Sum(s => s.SubProjectNetWeightKg) / 1000.0,
                    GrossWeightTon = g.Sum(s => s.SubProjectGrossWeightKg) / 1000.0
                })
                .ToList();

            // Assert 1: 结构系统重量之和守恒
            double sysNetSum = systemGroups.Sum(g => g.NetWeightTon);
            Assert.Equal(projectResult.TotalNetWeightTon, sysNetSum, precision: 2);

            // Act 2: 防火时限聚合 (模拟 BoqWorkbench.razor 中的 GetFireRatingAnalytics)
            var fireGroups = allSubs
                .GroupBy(s => s.FireRatingHours)
                .Select(g => new
                {
                    Hours = g.Key,
                    Area = g.Sum(s => s.SubProjectSurfaceAreaM2),
                    Rate = BoqAggregationEngine.CalculateFireRatingRate(g.Key)
                })
                .ToList();

            // Assert 2: 面积守恒与定额单价阶梯
            double fireAreaSum = fireGroups.Sum(f => f.Area);
            Assert.Equal(projectResult.TotalSurfaceAreaM2, fireAreaSum, precision: 1);

            foreach (var f in fireGroups)
            {
                if (f.Hours >= 3.0) Assert.Equal(145.0, f.Rate);
                else if (f.Hours >= 2.0) Assert.Equal(110.0, f.Rate);
                else Assert.Equal(75.0, f.Rate);
            }
        }

        #endregion

        #region 7. Empirical Remediation Verification: Non-Zero Entity IDs & Predicate Guard Tests

        [Fact]
        public void VerifyingFix_InMemoryBenchmarkSeedData_EntityIdsAreDistinctAndNonZero_PreventingUICollisions()
        {
            // Arrange
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // Verification 1: All IDs are non-zero positive integers
            var tenderItemIds = project.TenderItems.Select(t => t.Id).ToList();
            var subProjectIds = project.TenderItems.SelectMany(t => t.SubProjects).Select(s => s.Id).ToList();
            var partIds = project.TenderItems.SelectMany(t => t.SubProjects).SelectMany(s => s.Parts).Select(p => p.Id).ToList();

            Assert.Equal(3, tenderItemIds.Count);
            Assert.All(tenderItemIds, id => Assert.True(id > 0, "TenderItem ID must be positive"));
            Assert.Equal(tenderItemIds.Count, tenderItemIds.Distinct().Count()); // Unique!

            Assert.Equal(13, subProjectIds.Count);
            Assert.All(subProjectIds, id => Assert.True(id > 0, "SubProject ID must be positive"));
            Assert.Equal(subProjectIds.Count, subProjectIds.Distinct().Count()); // Unique!

            Assert.Equal(20, partIds.Count);
            Assert.All(partIds, id => Assert.True(id > 0, "Part ID must be positive"));
            Assert.Equal(partIds.Count, partIds.Distinct().Count()); // Unique!

            // Verification 2: Independent UI expansion state
            var expandedTenderSet = new HashSet<int>();
            expandedTenderSet.Add(project.TenderItems[0].Id); // user clicks item 0 (NH)

            Assert.Contains(project.TenderItems[0].Id, expandedTenderSet);
            Assert.DoesNotContain(project.TenderItems[1].Id, expandedTenderSet);
            Assert.DoesNotContain(project.TenderItems[2].Id, expandedTenderSet);

            var expandedTagSet = new HashSet<string>();
            expandedTagSet.Add(project.TenderItems[0].ItemTag);
            Assert.Contains(project.TenderItems[0].ItemTag, expandedTagSet);
            Assert.DoesNotContain(project.TenderItems[1].ItemTag, expandedTagSet);
            Assert.DoesNotContain(project.TenderItems[2].ItemTag, expandedTagSet);
        }

        [Fact]
        public void VerifyingFix_BoqAggregationEngine_IdCollisionPrevented_CorrectCharacteristicsRetained()
        {
            // Arrange
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act: Project to Owner / ClientTenderMaster
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.Owner, project);

            var clientRows = projection.ClientTenderRows;
            Assert.NotNull(clientRows);
            Assert.Equal(3, clientRows.Count);

            var nhRow = clientRows.First(r => r.ItemTag == "NH");
            var nfRow = clientRows.First(r => r.ItemTag == "NF");
            var jlcRow = clientRows.First(r => r.ItemTag == "JLC");

            var nhItem = project.TenderItems.First(t => t.ItemTag == "NH");
            var nfItem = project.TenderItems.First(t => t.ItemTag == "NF");
            var jlcItem = project.TenderItems.First(t => t.ItemTag == "JLC");

            // Verification: Each row preserves its own genuine characteristics
            Assert.Equal(nhItem.ProjectCharacteristics, nhRow.ProjectCharacteristics);
            Assert.Equal(nfItem.ProjectCharacteristics, nfRow.ProjectCharacteristics);
            Assert.Equal(jlcItem.ProjectCharacteristics, jlcRow.ProjectCharacteristics);
            Assert.NotEqual(nhRow.ProjectCharacteristics, nfRow.ProjectCharacteristics);

            // Verification: Fabrication workshop rows preserve authentic subproject attributes
            var fabProjection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.FabricationShop, project);
            Assert.NotNull(fabProjection.FabricationRows);
            Assert.Equal(13, fabProjection.FabricationRows.Count);

            var subNH48Row = fabProjection.FabricationRows.First(r => r.SubProjectCode == "N-NHXX-48");
            var subNH49Row = fabProjection.FabricationRows.First(r => r.SubProjectCode == "N-NHXX-49");
            Assert.Equal(16.0, subNH48Row.MaxPlateThickness);
            Assert.Equal(50.0, subNH49Row.MaxPlateThickness); // No longer suppressed by subNH48's 16.0!

            // Verification: Site erection schedule rows preserve authentic elevations
            var erectionProjection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.SiteProjectDept, project);
            Assert.NotNull(erectionProjection.SiteErectionRows);
            var subJLCColRow = erectionProjection.SiteErectionRows.First(r => r.SubProjectCode == "JLC-COL-01");
            Assert.Equal(9.5, subJLCColRow.AssemblyElevation); // No longer corrupted to subNH48's 36.0m!
            Assert.False(subJLCColRow.HasFieldWelding); // No longer corrupted to subNH48's true!
        }

        [Fact]
        public void VerifyingFix_BoqAggregationEngine_GracefulFallbackWhenEntityIdsAreZero()
        {
            // Arrange: Project with unassigned (0) IDs
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            foreach (var item in project.TenderItems)
            {
                item.Id = 0;
                foreach (var sub in item.SubProjects)
                {
                    sub.Id = 0;
                }
            }

            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act
            var projection = BoqAggregationEngine.ProjectToAudience(projectResult, BoqAudiencePerspective.Owner, project);
            var clientRows = projection.ClientTenderRows!;

            var nfRow = clientRows.First(r => r.ItemTag == "NF");
            var nfItem = project.TenderItems.First(t => t.ItemTag == "NF");

            // Verification: Falls back to ItemTag == "NF", NOT matching 0 == 0 on item 0!
            Assert.Equal(nfItem.ProjectCharacteristics, nfRow.ProjectCharacteristics);
        }

        #endregion

        #region 8. Steel Grade Analytics & Dynamic KPI Subtitles Tests

        [Fact]
        public void Analytics_SteelGradeCalculations_MatchProjectTotals()
        {
            // Arrange
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act: 钢材材质牌号聚合 (模拟 BoqWorkbench.razor 中的 GetSteelGradeAnalytics)
            var allParts = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .SelectMany(s => s.Parts)
                .ToList();

            var gradeRows = allParts
                .GroupBy(p => string.IsNullOrWhiteSpace(p.SteelGrade) ? "其他材质" : p.SteelGrade.Trim())
                .Select(g => new SteelGradeAnalyticsRow
                {
                    GradeName = g.Key,
                    PartCount = g.Sum(p => Math.Max(1, p.Quantity)),
                    TotalNetWeightTon = g.Sum(p => p.TotalNetWeight) / 1000.0,
                    TotalGrossWeightTon = g.Sum(p => p.TotalGrossWeight > 0.0 ? p.TotalGrossWeight : p.TotalNetWeight * 1.15) / 1000.0,
                    TotalMaterialCost = g.Sum(p => p.TotalPartMaterialCost),
                    TotalScissorProfit = g.Sum(p => p.ArbitrageProfit)
                })
                .OrderByDescending(r => r.TotalNetWeightTon)
                .ToList();

            // Assert 1: 必须包含宁波奥体全部三大代表性材质
            Assert.True(gradeRows.Count >= 3, "必须包含至少 Q460GJC, Q355B, Q235B 三大材质牌号");
            Assert.Contains(gradeRows, r => r.GradeName == "Q460GJC");
            Assert.Contains(gradeRows, r => r.GradeName == "Q355B");
            Assert.Contains(gradeRows, r => r.GradeName == "Q235B");

            // Assert 2: 理论净重之和守恒
            double sumNetTon = gradeRows.Sum(r => r.TotalNetWeightTon);
            Assert.Equal(projectResult.TotalNetWeightTon, sumNetTon, precision: 2);

            // Assert 3: 采购毛重之和守恒 (1.15倍损耗)
            double sumGrossTon = gradeRows.Sum(r => r.TotalGrossWeightTon);
            Assert.Equal(projectResult.TotalGrossWeightTon, sumGrossTon, precision: 2);

            // Assert 4: 材料到场采购合价守恒
            double sumMatCost = gradeRows.Sum(r => r.TotalMaterialCost);
            Assert.Equal(projectResult.TotalMaterialCost, sumMatCost, precision: 1);

            // Assert 5: 采办剪刀差利润总额守恒
            double sumScissor = gradeRows.Sum(r => r.TotalScissorProfit);
            Assert.Equal(projectResult.TotalProcurementScissorProfit, sumScissor, precision: 1);
        }

        [Fact]
        public void Subtitles_Card2AndCard4DynamicLinq_ProduceCorrectFormattedStrings()
        {
            // Arrange
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var projectResult = BoqAggregationEngine.CalculateProject(project);

            // Act 1: Card 2 Dynamic LINQ
            string card2Subtitle = string.Join(" | ", project.TenderItems.Select(t => $"{t.ItemTag}: {t.TenderQuantity:N2}t"));

            // Assert 1: Card 2 dynamic string
            Assert.Contains("NH: 1,123.94t", card2Subtitle);
            Assert.Contains("NF: 865.20t", card2Subtitle);
            Assert.Contains("JLC: 728.30t", card2Subtitle);

            // Act 2: Card 4 Dynamic LINQ (simulating GetFireRatingAnalytics)
            var allSubs = project.TenderItems.SelectMany(t => t.SubProjects).ToList();
            var fireAnalytics = allSubs
                .GroupBy(s => s.FireRatingHours)
                .OrderByDescending(g => g.Key)
                .Select(g => new
                {
                    RatingHours = g.Key,
                    SurfaceAreaM2 = g.Sum(s => s.SubProjectSurfaceAreaM2)
                })
                .ToList();

            string card4Subtitle = string.Join(" | ", fireAnalytics.Select(f => $"{f.RatingHours:F1}H: {f.SurfaceAreaM2:N1}m²"));

            // Assert 2: Card 4 dynamic string
            Assert.Contains("3.0H: 5,334.6m²", card4Subtitle);
            Assert.Contains("2.0H: 4,720.0m²", card4Subtitle);
            Assert.Contains("1.5H: 7,850.0m²", card4Subtitle);
        }

        #endregion
    }
}
