using EngineeringApp.Shared.CostEstimation;
using EngineeringApp.Shared.Pricing;

namespace EngineeringApp.Shared.CostEstimation.Pipeline;

/// <summary>
/// 成本测算执行上下文 (Pipeline Context)
/// </summary>
public class CostEstimationContext
{
    public List<MemberPricingResult> Members { get; }
    public CostEstimationOptions Options { get; set; }
    public ProjectCostSheet ResultSheet { get; }

    public CostEstimationContext(IEnumerable<MemberPricingResult> members, CostEstimationOptions? options = null)
    {
        Members = members?.ToList() ?? [];
        Options = options ?? new CostEstimationOptions();
        ResultSheet = new ProjectCostSheet
        {
            ProjectName = Options.ProjectName,
            ClientOrBidUnit = Options.ClientName,
            TotalMemberQuantity = Members.Sum(m => m.Quantity),
            TotalSteelNetWeightTon = Members.Sum(m => m.TotalNetWeightTon),
            TotalSteelGrossWeightTon = Members.Sum(m => m.TotalGrossWeightTon),
            TotalPaintingAreaM2 = Members.Sum(m => m.TotalPaintingAreaM2),
            OverheadRatePercent = Options.OverheadRatePercent,
            FinancingCostRatePercent = Options.FinancingCostRatePercent,
            TargetGrossMarginPercent = Options.TargetGrossMarginPercent,
            TaxRatePercent = Options.TaxRatePercent
        };
    }
}

/// <summary>
/// 成本流水线工序步骤接口 (Pipeline Step / Chain of Responsibility)
/// </summary>
public interface ICostEstimationStep
{
    string StepName { get; }
    void Execute(CostEstimationContext context);
}

/// <summary>
/// 步骤 1: 直接材料采购与下料损耗费计算工序
/// </summary>
public class MaterialCostStep : ICostEstimationStep
{
    public string StepName => "主材采购与下料损耗费核算";

    public void Execute(CostEstimationContext context)
    {
        context.ResultSheet.MaterialProcurementCost = context.Members.Sum(m => m.MaterialProcurementCost);
    }
}

/// <summary>
/// 步骤 2: 车间加工制作工费核算工序
/// </summary>
public class FabricationCostStep : ICostEstimationStep
{
    public string StepName => "车间加工制作工费核算";

    public void Execute(CostEstimationContext context)
    {
        context.ResultSheet.WorkshopFabricationCost = context.Members.Sum(m => m.FabricationCost);
    }
}

/// <summary>
/// 步骤 3: 构件表面涂装防腐与防火直接费核算工序
/// </summary>
public class CoatingCostStep : ICostEstimationStep
{
    public string StepName => "涂装防腐与防火直接费核算";

    public void Execute(CostEstimationContext context)
    {
        context.ResultSheet.SurfaceCoatingCost = context.Members.Sum(m => m.CoatingCost);
    }
}

/// <summary>
/// 步骤 4: 干线物流运输与现场吊装措施费核算工序
/// </summary>
public class LogisticsAndErectionStep : ICostEstimationStep
{
    public string StepName => "物流调运与现场安装措施费核算";

    public void Execute(CostEstimationContext context)
    {
        var sheet = context.ResultSheet;
        var opt = context.Options;

        sheet.TransportationCost = sheet.TotalSteelNetWeightTon * Math.Max(0, opt.TransportationUnitPricePerTon);
        sheet.SiteErectionCost = sheet.TotalSteelNetWeightTon * Math.Max(0, opt.SiteErectionUnitPricePerTon);
        sheet.DetailingBimFee = sheet.TotalSteelNetWeightTon * Math.Max(0, opt.DetailingBimFeePerTon);
        sheet.InspectionTestingFee = sheet.TotalSteelNetWeightTon * Math.Max(0, opt.InspectionTestingFeePerTon);
        sheet.TemporarySupportFee = Math.Max(0, opt.LumpSumTemporarySupportFee);
    }
}

/// <summary>
/// 步骤 5: 综合管理费、利税与目标利润费率核算工序
/// </summary>
public class OverheadProfitAndTaxStep : ICostEstimationStep
{
    public string StepName => "综合管理费、财务利税与目标利润费率配置";

    public void Execute(CostEstimationContext context)
    {
        var sheet = context.ResultSheet;
        var opt = context.Options;

        sheet.OverheadRatePercent = opt.OverheadRatePercent;
        sheet.FinancingCostRatePercent = opt.FinancingCostRatePercent;
        sheet.TargetGrossMarginPercent = opt.TargetGrossMarginPercent;
        sheet.TaxRatePercent = opt.TaxRatePercent;
    }
}

/// <summary>
/// 全要素工程造价责任链流水线管道 (Pipeline Pattern)
/// 允许针对总包 EPC、加工厂出厂、贸易提货等不同工程模式自由插拔或重组计价步骤
/// </summary>
public class CostEstimationPipeline
{
    private readonly List<ICostEstimationStep> _steps = [];

    public IReadOnlyList<ICostEstimationStep> Steps => _steps.AsReadOnly();

    public CostEstimationPipeline AddStep(ICostEstimationStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _steps.Add(step);
        return this;
    }

    /// <summary>执行成本测算流水线</summary>
    public ProjectCostSheet Execute(IEnumerable<MemberPricingResult> members, CostEstimationOptions? options = null)
    {
        var context = new CostEstimationContext(members, options);
        foreach (var step in _steps)
        {
            step.Execute(context);
        }
        return context.ResultSheet;
    }

    /// <summary>构造标准施工总承包 (EPC) 完整组价流水线</summary>
    public static CostEstimationPipeline CreateStandardEpcPipeline()
    {
        return new CostEstimationPipeline()
            .AddStep(new MaterialCostStep())
            .AddStep(new FabricationCostStep())
            .AddStep(new CoatingCostStep())
            .AddStep(new LogisticsAndErectionStep())
            .AddStep(new OverheadProfitAndTaxStep());
    }

    /// <summary>构造构件制造加工厂出厂 (Ex-Works) 组价流水线 (免现场吊装)</summary>
    public static CostEstimationPipeline CreateExWorksFabricationPipeline()
    {
        return new CostEstimationPipeline()
            .AddStep(new MaterialCostStep())
            .AddStep(new FabricationCostStep())
            .AddStep(new CoatingCostStep())
            .AddStep(new OverheadProfitAndTaxStep());
    }
}
