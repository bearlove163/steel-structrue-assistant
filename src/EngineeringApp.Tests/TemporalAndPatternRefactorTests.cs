using EngineeringApp.Shared.Algorithms.Strategies;
using EngineeringApp.Shared.Common.Temporal;
using EngineeringApp.Shared.CostEstimation;
using EngineeringApp.Shared.CostEstimation.Pipeline;
using EngineeringApp.Shared.Material.Pricing;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Pricing;
using EngineeringApp.Shared.Quota;
using EngineeringApp.Shared.QuotationExport;
using EngineeringApp.Shared.Reports;
using EngineeringApp.Shared.Standards;
using Xunit;

namespace EngineeringApp.Tests;

/// <summary>
/// 通用版本回溯与时间重塑引擎、截面策略注册表、造价流水线管道单元测试
/// </summary>
public class TemporalAndPatternRefactorTests
{
    [Fact]
    public void UniversalTemporalEngine_ShouldTimeTravelAndCompareSnapshotsAccurately()
    {
        // 1. 定义测试用快照差异比对算法
        var engine = new UniversalTemporalEngine<double, GenericDiffItem>((baseSnap, targetSnap) =>
        {
            var diff = new VersionDiffReport<GenericDiffItem>
            {
                BaselineSnapshotId = baseSnap.SnapshotId,
                BaselineSnapshotName = baseSnap.SnapshotName,
                BaselineDate = baseSnap.EffectiveDate,
                TargetSnapshotId = targetSnap.SnapshotId,
                TargetSnapshotName = targetSnap.SnapshotName,
                TargetDate = targetSnap.EffectiveDate,
                TotalBaselineAmount = baseSnap.StateData,
                TotalTargetAmount = targetSnap.StateData,
                DifferenceItems =
                [
                    new GenericDiffItem
                    {
                        ItemKey = "PRICE_TEST",
                        ItemName = "测试单价指标",
                        BaselineValue = baseSnap.StateData,
                        TargetValue = targetSnap.StateData
                    }
                ]
            };
            return diff;
        });

        // 2. 注册 2026年3月(基期 4000) 与 2026年10月(目标期 4300) 两个快照
        var snap1 = new GenericVersionSnapshot<double>(
            "SNAP-2026-03", "2026年3月投标锁价基准期",
            new DateTime(2026, 3, 1), 4000.0, isLocked: true);

        var snap2 = new GenericVersionSnapshot<double>(
            "SNAP-2026-10", "2026年10月现行施工结算期",
            new DateTime(2026, 10, 8), 4300.0, isLocked: false);

        engine.RegisterSnapshot(snap1);
        engine.RegisterSnapshot(snap2);

        // 3. 验证时光倒流检索能力
        var historicalSnap = engine.TimeTravelTo(new DateTime(2026, 5, 1));
        Assert.NotNull(historicalSnap);
        Assert.Equal("SNAP-2026-03", historicalSnap.SnapshotId);
        Assert.Equal(4000.0, historicalSnap.StateData);

        // 4. 验证双时期差异对比分析
        var diffReport = engine.Compare("SNAP-2026-03", "SNAP-2026-10");
        Assert.Equal(300.0, diffReport.TotalDifferenceAmount);
        Assert.Equal(7.5, diffReport.OverallPercentChange, 1);
        Assert.Single(diffReport.DifferenceItems);
        Assert.Equal("涨价/增加", diffReport.DifferenceItems[0].ChangeStatus);

        // 5. 验证不可篡改审计履历链条追加
        engine.AppendAuditEvent(new GenericAuditEvent
        {
            EntityFingerprint = "PL-Q355B-TEST",
            Reason = "二次议价定案折让",
            OldCostMetric = 4300.0,
            NewCostMetric = 4250.0,
            RevisedBy = "项目总造价师"
        });

        var timeline = engine.GetAuditTimeline("PL-Q355B-TEST");
        Assert.Single(timeline);
        Assert.Equal(-50.0, timeline[0].CostDelta);
        Assert.Equal("项目总造价师", timeline[0].RevisedBy);
    }

    [Fact]
    public void SectionStrategyRegistry_CompositeSection_ShouldCalculateInertiaViaParallelAxisTheorem()
    {
        // 测试两块 100x10mm 钢板组成的组合截面 (通过平行轴移轴定理验证)
        var composite = new CompositeSectionStrategy();
        var p1 = new SectionParameters { Type = SectionType.Rectangle, Width = 100, Height = 10 };
        var p2 = new SectionParameters { Type = SectionType.Rectangle, Width = 100, Height = 10 };

        // 第一块位于 Z=0~10 (形心 Z=5)，第二块向上平移 90mm，位于 Z=90~100 (形心 Z=95)
        composite.AddPart(new RectangleSectionStrategy(), p1, offsetY: 0, offsetZ: 0);
        composite.AddPart(new RectangleSectionStrategy(), p2, offsetY: 0, offsetZ: 90);

        var res = new SectionPropertiesResult();
        composite.Calculate(new SectionParameters { Type = SectionType.Polygon }, res, new MaterialProfile());

        // 总面积应为 100*10 + 100*10 = 2000 mm²
        Assert.Equal(2000.0, res.Area);

        // 组合形心竖坐标应为 (1000*5 + 1000*95) / 2000 = 50 mm
        Assert.Equal(50.0, res.Zc, 1);

        // 验证主惯性矩存在且大于 0
        Assert.True(res.Iy > 0);
        Assert.True(res.Iz > 0);
    }

    [Fact]
    public void CostEstimationPipeline_ShouldCalculateEpcAndExWorksCorrectly()
    {
        // 构造测试构件
        var members = new List<MemberPricingResult>
        {
            new()
            {
                AssemblyMark = "KZ-1",
                Role = MemberRole.Column,
                Quantity = 5,
                TotalNetWeightTon = 10.0,
                TotalGrossWeightTon = 10.6,
                TotalPaintingAreaM2 = 120.0,
                MaterialProcurementCost = 45000.0,
                FabricationCost = 18000.0,
                CoatingCost = 5400.0
            }
        };

        var options = new CostEstimationOptions
        {
            ProjectName = "华东高层测试项目",
            ClientName = "总承包商测试部",
            TransportationUnitPricePerTon = 200.0,
            SiteErectionUnitPricePerTon = 800.0
        };

        // 1. 标准总包 (EPC) 管道测试
        var epcPipeline = CostEstimationPipeline.CreateStandardEpcPipeline();
        var epcSheet = epcPipeline.Execute(members, options);

        Assert.Equal(45000.0, epcSheet.MaterialProcurementCost);
        Assert.Equal(18000.0, epcSheet.WorkshopFabricationCost);
        Assert.Equal(5400.0, epcSheet.SurfaceCoatingCost);
        Assert.Equal(2000.0, epcSheet.TransportationCost); // 10t * 200
        Assert.Equal(8000.0, epcSheet.SiteErectionCost);   // 10t * 800
        Assert.True(epcSheet.FinalBidQuotation > 0);

        // 2. 制造出厂 (Ex-Works) 管道测试 (免现场安装和运费)
        var exWorksPipeline = CostEstimationPipeline.CreateExWorksFabricationPipeline();
        var exWorksSheet = exWorksPipeline.Execute(members, options);

        Assert.Equal(0.0, exWorksSheet.SiteErectionCost);
        Assert.Equal(0.0, exWorksSheet.TransportationCost);
        Assert.True(exWorksSheet.FinalBidQuotation < epcSheet.FinalBidQuotation);
    }

    [Fact]
    public void EngineeringStandardFactory_ShouldProduceAccuratePricingToleranceAndLimitsForGB_EN_ASTM()
    {
        // 1. 验证国标 GB 工厂
        var gbFactory = EngineeringStandardFactoryProvider.GetFactory(StandardSystem.GB);
        Assert.Equal(StandardSystem.GB, gbFactory.SystemType);
        Assert.Equal(350.0, gbFactory.CreateLimitsEngine().GetTensionSlendernessLimit());
        Assert.Equal(150.0, gbFactory.CreateLimitsEngine().GetCompressionSlendernessLimit(true));
        Assert.True(gbFactory.CreateToleranceEngine().GetThicknessTolerance(20) > 0);
        Assert.NotNull(gbFactory.CreatePricingStrategy());

        // 2. 验证欧标 EN 工厂
        var enFactory = EngineeringStandardFactoryProvider.GetFactory(StandardSystem.EN);
        Assert.Equal(StandardSystem.EN, enFactory.SystemType);
        Assert.Equal(300.0, enFactory.CreateLimitsEngine().GetTensionSlendernessLimit());
        Assert.Equal(1.0, enFactory.CreateLimitsEngine().GetResistanceFactor());
        Assert.NotNull(enFactory.CreatePricingStrategy());

        // 3. 验证美标 ASTM 工厂
        var astmFactory = EngineeringStandardFactoryProvider.GetFactory(StandardSystem.ASTM);
        Assert.Equal(StandardSystem.ASTM, astmFactory.SystemType);
        Assert.Equal(0.90, astmFactory.CreateLimitsEngine().GetResistanceFactor());
        Assert.Equal(200.0, astmFactory.CreateLimitsEngine().GetCompressionSlendernessLimit());
        Assert.NotNull(astmFactory.CreatePricingStrategy());

        // 4. 验证 PricingStrategyFactory 兼容性委托
        var pricingGb = PricingStrategyFactory.GetStrategy(StandardSystem.GB);
        var pricingEn = PricingStrategyFactory.GetStrategy(StandardSystem.EN);
        var pricingAstm = PricingStrategyFactory.GetStrategy(StandardSystem.ASTM);
        Assert.NotNull(pricingGb);
        Assert.NotNull(pricingEn);
        Assert.NotNull(pricingAstm);
        Assert.NotEqual(pricingGb.GetType(), pricingEn.GetType());
    }

    [Fact]
    public void EngineeringReportBuilder_And_Visitors_ShouldGenerateAccurateMarkdownExcelAndHtmlOutputs()
    {
        var parameters = new SectionParameters
        {
            Type = SectionType.HBeam,
            StandardProfileName = "HM 350x250x9x14",
            Height = 350,
            Width = 250,
            WebThickness = 9,
            FlangeThickness = 14,
            Material = new MaterialProfile { Name = "Q355B", YieldStrength = 355, ElasticModulus = 206000 }
        };

        var result = new SectionPropertiesResult
        {
            Area = 10140,
            LinearMass = 79.7,
            OuterPerimeter = 1680,
            Iy = 228000000,
            Iz = 36500000,
            WyTop = 1300000,
            WyBot = 1300000
        };

        var bidSummary = new CommercialBidSummary
        {
            ProjectName = "中建某工业厂房总包工程",
            Items =
            [
                new QuotationBillItem
                {
                    ItemNumber = 1,
                    BillCode = "010605001001",
                    BillName = "钢柱",
                    ItemDescription = "Q355B HM350x250",
                    Unit = "t",
                    Quantity = 50.0,
                    UnitPrice = 6500.0
                }
            ]
        };

        var builder = new EngineeringReportBuilder()
            .AddProjectOverview("钢结构工程综合成果报告", "中建测试工程局", "GB 50017-2017", "主跨工业厂房")
            .AddSectionMechanics(parameters, result)
            .AddQuotationBid(bidSummary)
            .AddTemporalAudit("SNAP-01", "SNAP-02", DateTime.Now.AddDays(-30), DateTime.Now, 15000, 80, 4000, "双方核对无误，同意定案");

        // 1. 验证 Markdown 访问者成果
        var mdOutput = builder.Build(new MarkdownReportVisitor());
        Assert.Contains("# 钢结构工程综合成果报告", mdOutput);
        Assert.Contains("HM 350x250x9x14", mdOutput);
        Assert.Contains("010605001001", mdOutput);
        Assert.Contains("剪刀差利润", mdOutput);

        // 2. 验证 Excel (TSV) 访问者成果
        var excelOutput = builder.Build(new ExcelSpreadsheetVisitor());
        Assert.Contains("【截面特性计算表】", excelOutput);
        Assert.Contains("【商业投标工程量清单表】", excelOutput);
        Assert.Contains("【双时空版本审计对比表】", excelOutput);
        Assert.Contains("\t", excelOutput); // 包含制表符

        // 3. 验证 Printable HTML 访问者成果
        var htmlOutput = builder.Build(new PrintableHtmlVisitor());
        Assert.Contains("<!DOCTYPE html>", htmlOutput);
        Assert.Contains("钢结构工程综合成果报告", htmlOutput);
        Assert.Contains("<table", htmlOutput);
        Assert.Contains("</body></html>", htmlOutput);
    }
}
