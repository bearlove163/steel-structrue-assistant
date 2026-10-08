using EngineeringApp.Shared.Pricing;

namespace EngineeringApp.Shared.CostEstimation;

/// <summary>
/// 成本测算配置选项 (允许配置运费、安装费、措施费率、公司固定管理费率与利润率)
/// </summary>
public class CostEstimationOptions
{
    public string ProjectName { get; set; } = "标准钢结构工程测算";
    public string ClientName { get; set; } = "工程业主/投资方";

    /// <summary>物流吨运费 (元/吨净重，默认 250 元/t)</summary>
    public double TransportationUnitPricePerTon { get; set; } = 250.0;

    /// <summary>工地现场安装吊装综合单价 (元/吨净重，默认 850 元/t；若为制造厂出厂模式则设为 0)</summary>
    public double SiteErectionUnitPricePerTon { get; set; } = 850.0;

    /// <summary>深化设计与 BIM 建模放样费 (元/吨净重，默认 120 元/t)</summary>
    public double DetailingBimFeePerTon { get; set; } = 120.0;

    /// <summary>无损探伤与第三方力学试验费 (元/吨净重，默认 60 元/t)</summary>
    public double InspectionTestingFeePerTon { get; set; } = 60.0;

    /// <summary>临时支撑胎架措施固定包干费 (元，默认 0)</summary>
    public double LumpSumTemporarySupportFee { get; set; } = 0.0;

    /// <summary>企业车间综合管理费率 (%)</summary>
    public double OverheadRatePercent { get; set; } = 3.5;

    /// <summary>财务资金占用与垫资成本率 (%)</summary>
    public double FinancingCostRatePercent { get; set; } = 2.0;

    /// <summary>目标毛利率 (%)</summary>
    public double TargetGrossMarginPercent { get; set; } = 8.0;

    /// <summary>建筑工程增值税税率 (%)</summary>
    public double TaxRatePercent { get; set; } = 9.0;
}

/// <summary>
/// 公司固定格式成本测算套算核心引擎
/// </summary>
public static class CostEstimationEngine
{
    /// <summary>
    /// 根据构件工料机组价结果集合，自动套入公司固定的成本测算表
    /// </summary>
    public static ProjectCostSheet Calculate(
        IEnumerable<MemberPricingResult> members,
        CostEstimationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(members);
        options ??= new CostEstimationOptions();

        var memberList = members.ToList();
        double totalNetWeightTon = memberList.Sum(m => m.TotalNetWeightTon);
        double totalGrossWeightTon = memberList.Sum(m => m.TotalGrossWeightTon);
        double totalPaintingAreaM2 = memberList.Sum(m => m.TotalPaintingAreaM2);
        int totalQuantity = memberList.Sum(m => m.Quantity);

        double totalMaterialProcurement = memberList.Sum(m => m.MaterialProcurementCost);
        double totalWorkshopFabrication = memberList.Sum(m => m.FabricationCost);
        double totalSurfaceCoating = memberList.Sum(m => m.CoatingCost);

        // 物流与现场安装直接费
        double transportationCost = totalNetWeightTon * Math.Max(0, options.TransportationUnitPricePerTon);
        double siteErectionCost = totalNetWeightTon * Math.Max(0, options.SiteErectionUnitPricePerTon);

        // 技术措施与检测费
        double detailingBimFee = totalNetWeightTon * Math.Max(0, options.DetailingBimFeePerTon);
        double inspectionFee = totalNetWeightTon * Math.Max(0, options.InspectionTestingFeePerTon);
        double tempSupportFee = Math.Max(0, options.LumpSumTemporarySupportFee);

        var sheet = new ProjectCostSheet
        {
            ProjectName = options.ProjectName,
            ClientOrBidUnit = options.ClientName,
            TotalMemberQuantity = totalQuantity,
            TotalSteelNetWeightTon = totalNetWeightTon,
            TotalSteelGrossWeightTon = totalGrossWeightTon,
            TotalPaintingAreaM2 = totalPaintingAreaM2,

            MaterialProcurementCost = totalMaterialProcurement,
            WorkshopFabricationCost = totalWorkshopFabrication,
            SurfaceCoatingCost = totalSurfaceCoating,
            TransportationCost = transportationCost,
            SiteErectionCost = siteErectionCost,

            DetailingBimFee = detailingBimFee,
            InspectionTestingFee = inspectionFee,
            TemporarySupportFee = tempSupportFee,

            OverheadRatePercent = options.OverheadRatePercent,
            FinancingCostRatePercent = options.FinancingCostRatePercent,
            TargetGrossMarginPercent = options.TargetGrossMarginPercent,
            TaxRatePercent = options.TaxRatePercent
        };

        return sheet;
    }
}
