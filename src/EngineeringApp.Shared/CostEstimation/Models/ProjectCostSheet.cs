namespace EngineeringApp.Shared.CostEstimation;

/// <summary>
/// 公司固定格式钢结构工程项目成本测算表模型
/// 汇聚构件工料机、直接工程费、技术措施费、企业管理费、垫资财务成本、税金与目标利润
/// </summary>
public class ProjectCostSheet
{
    public string ProjectName { get; set; } = "标准钢结构项目成本测算";
    public string ClientOrBidUnit { get; set; } = "业主方/总包方";
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    // ================= 1. 物理工程量汇总 (Physical Quantities) =================
    /// <summary>构件总套数/根数</summary>
    public int TotalMemberQuantity { get; set; }

    /// <summary>钢结构总净重 (吨 t)</summary>
    public double TotalSteelNetWeightTon { get; set; }

    /// <summary>钢材采购总毛重 (含下料损耗 吨 t)</summary>
    public double TotalSteelGrossWeightTon { get; set; }

    /// <summary>下料综合损耗比率 (%)</summary>
    public double MaterialLossRatePercent => TotalSteelNetWeightTon > 0
        ? ((TotalSteelGrossWeightTon - TotalSteelNetWeightTon) / TotalSteelNetWeightTon) * 100.0
        : 0;

    /// <summary>展开涂装防腐与防火总表面积 (m²)</summary>
    public double TotalPaintingAreaM2 { get; set; }

    /// <summary>综合吨钢比表面积 / 展开度 (m²/t)</summary>
    public double AverageAreaPerTon => TotalSteelNetWeightTon > 0
        ? TotalPaintingAreaM2 / TotalSteelNetWeightTon
        : 0;

    // ================= 2. 直接工程成本 (Direct Engineering Costs) =================
    /// <summary>① 原材料采购直接费 (钢板、型钢毛重采购费 元)</summary>
    public double MaterialProcurementCost { get; set; }

    /// <summary>② 车间加工制作直接工费 (数控下料、拼装、主焊缝、内隔板、牛腿、矫正 元)</summary>
    public double WorkshopFabricationCost { get; set; }

    /// <summary>③ 表面抛丸除锈与防腐防火涂装费 (油漆材料与喷涂施工 元)</summary>
    public double SurfaceCoatingCost { get; set; }

    /// <summary>④ 物流运杂费 (出厂至工地运输费，含超宽超长附加 元)</summary>
    public double TransportationCost { get; set; }

    /// <summary>⑤ 工地现场安装吊装直接费 (现场吊车台班、垂直运输、高空组对焊接收口 元)</summary>
    public double SiteErectionCost { get; set; }

    /// <summary>直接工程成本小计 (元)</summary>
    public double TotalDirectCost => MaterialProcurementCost + WorkshopFabricationCost +
                                     SurfaceCoatingCost + TransportationCost + SiteErectionCost;

    // ================= 3. 技术措施与第三方检测费 (Measures & Technical Fees) =================
    /// <summary>深化设计与 BIM 建模放样费 (元)</summary>
    public double DetailingBimFee { get; set; }

    /// <summary>无损探伤检验与第三方力学试验费 (UT超声波探伤/磁粉探伤 元)</summary>
    public double InspectionTestingFee { get; set; }

    /// <summary>施工临时支撑胎架、安全文明措施费 (元)</summary>
    public double TemporarySupportFee { get; set; }

    /// <summary>技术措施与服务费小计 (元)</summary>
    public double TotalMeasureCost => DetailingBimFee + InspectionTestingFee + TemporarySupportFee;

    // ================= 4. 公司固定费率分摊与财务成本 (Company Fixed Overhead & Financial) =================
    /// <summary>企业车间与综合管理费分摊比率 (%)</summary>
    public double OverheadRatePercent { get; set; } = 3.5;

    /// <summary>企业综合管理费金额 (元)</summary>
    public double OverheadCost => (TotalDirectCost + TotalMeasureCost) * (OverheadRatePercent / 100.0);

    /// <summary>资金垫资与财务资金占用成本率 (%)</summary>
    public double FinancingCostRatePercent { get; set; } = 2.0;

    /// <summary>财务资金占用成本金额 (元)</summary>
    public double FinancingCost => (TotalDirectCost + TotalMeasureCost) * (FinancingCostRatePercent / 100.0);

    /// <summary>
    /// 公司保本成本底价 (不含税、不含利润的纯成本底线 元)
    /// </summary>
    public double FactoryCostFloor => TotalDirectCost + TotalMeasureCost + OverheadCost + FinancingCost;

    /// <summary>公司保本吨单价 (元/吨净重)</summary>
    public double CostFloorPerTon => TotalSteelNetWeightTon > 0 ? FactoryCostFloor / TotalSteelNetWeightTon : 0;

    // ================= 5. 目标利润与最终对外报价 (Profit Margin & Quotation) =================
    /// <summary>目标毛利率 (%)</summary>
    public double TargetGrossMarginPercent { get; set; } = 8.0;

    /// <summary>预期利润总额 (元)</summary>
    public double ProfitAmount => FactoryCostFloor * (TargetGrossMarginPercent / 100.0);

    /// <summary>除税前报价小计 (保本成本 + 预期利润 元)</summary>
    public double SubtotalBeforeTax => FactoryCostFloor + ProfitAmount;

    /// <summary>建筑工程增值税税率 (%)</summary>
    public double TaxRatePercent { get; set; } = 9.0;

    /// <summary>增值税销项税金金额 (元)</summary>
    public double TaxAmount => SubtotalBeforeTax * (TaxRatePercent / 100.0);

    /// <summary>最终对外含税投标总报价 (元)</summary>
    public double FinalBidQuotation => SubtotalBeforeTax + TaxAmount;

    /// <summary>对外含税综合单价 (元/吨净重，商务投标核心对外指标)</summary>
    public double FinalUnitPricePerTon => TotalSteelNetWeightTon > 0 ? FinalBidQuotation / TotalSteelNetWeightTon : 0;
}
