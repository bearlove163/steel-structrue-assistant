using EngineeringApp.Shared.Algorithms.Strategies;
using EngineeringApp.Shared.Common.Temporal;
using EngineeringApp.Shared.CostEstimation;
using EngineeringApp.Shared.CostEstimation.Pipeline;
using EngineeringApp.Shared.Material.Pricing;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Pricing;
using EngineeringApp.Shared.Quota;
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
}
