namespace EngineeringApp.Shared.CostEstimation;

/// <summary>
/// 公司预置标准成本测算模板
/// </summary>
public static class CompanyCostTemplate
{
    /// <summary>
    /// 模板一：钢结构制造加工厂出厂供货模式 (FOB 出厂价，不含现场安装)
    /// </summary>
    public static CostEstimationOptions FactorySupplyOnly(string projectName = "钢结构制造加工供货项目") => new()
    {
        ProjectName = projectName,
        TransportationUnitPricePerTon = 200.0,
        SiteErectionUnitPricePerTon = 0.0, // 无现场安装
        DetailingBimFeePerTon = 80.0,
        InspectionTestingFeePerTon = 45.0,
        LumpSumTemporarySupportFee = 0.0,
        OverheadRatePercent = 3.0,
        FinancingCostRatePercent = 1.5,
        TargetGrossMarginPercent = 8.0,
        TaxRatePercent = 13.0 // 制造货物增值税率 13%
    };

    /// <summary>
    /// 模板二：专业分包综合安装模式 (含车间加工 + 物流 + 现场吊装 + 深化设计)
    /// </summary>
    public static CostEstimationOptions SubcontractWithErection(string projectName = "钢结构专业分包工程") => new()
    {
        ProjectName = projectName,
        TransportationUnitPricePerTon = 260.0,
        SiteErectionUnitPricePerTon = 850.0, // 包含现场安装吊装机械与人工
        DetailingBimFeePerTon = 120.0,
        InspectionTestingFeePerTon = 60.0,
        LumpSumTemporarySupportFee = 50000.0, // 包含现场拼装胎架措施
        OverheadRatePercent = 3.5,
        FinancingCostRatePercent = 2.0,
        TargetGrossMarginPercent = 10.0,
        TaxRatePercent = 9.0 // 建筑安装工程增值税率 9%
    };

    /// <summary>
    /// 模板三：EPC总承包或复杂空间结构综合模式 (大跨度空间桁架、高风险垫资)
    /// </summary>
    public static CostEstimationOptions ComplexStructureEpc(string projectName = "复杂空间钢结构工程") => new()
    {
        ProjectName = projectName,
        TransportationUnitPricePerTon = 350.0,
        SiteErectionUnitPricePerTon = 1200.0, // 大跨度空间吊装
        DetailingBimFeePerTon = 160.0,
        InspectionTestingFeePerTon = 90.0,
        LumpSumTemporarySupportFee = 120000.0,
        OverheadRatePercent = 4.5,
        FinancingCostRatePercent = 3.0, // 垫资比例高
        TargetGrossMarginPercent = 12.0,
        TaxRatePercent = 9.0
    };
}
