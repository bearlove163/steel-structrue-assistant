namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 价格数据来源体系
/// </summary>
public enum PriceSourceType
{
    /// <summary>我的钢铁网 (MySteel) 行业大盘网价</summary>
    MySteelWebIndex,

    /// <summary>兰格钢铁网 (Lange) 现货指数价</summary>
    LangeWebIndex,

    /// <summary>钢厂直发 / 出厂结算指导价 (如宝钢、津西订货单)</summary>
    SteelMillDirect,

    /// <summary>现货贸易商 / 一级协议经销商自提报价</summary>
    TraderSpotOffer,

    /// <summary>企业内部定额信息价 / 历史集采标底控制价</summary>
    InternalCorporateRate
}

public static class PriceSourceTypeExtensions
{
    public static string GetDisplayName(this PriceSourceType source) => source switch
    {
        PriceSourceType.MySteelWebIndex => "我的钢铁网 (MySteel) 大盘价",
        PriceSourceType.LangeWebIndex => "兰格钢铁网现货指数",
        PriceSourceType.SteelMillDirect => "钢厂出厂结算指导价",
        PriceSourceType.TraderSpotOffer => "现货贸易商协议提货价",
        PriceSourceType.InternalCorporateRate => "企业内控定额信息价",
        _ => source.ToString()
    };
}

/// <summary>
/// 材料价格基准快照 (含时间标签与大盘行情)
/// </summary>
public class MaterialPriceSnapshot
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string SnapshotName { get; set; } = "2026年10月上旬-华东区市场基准价";
    
    /// <summary>价格基准生效日期 (时间标签)</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    /// <summary>数据来源分类</summary>
    public PriceSourceType SourceType { get; set; } = PriceSourceType.MySteelWebIndex;

    /// <summary>来源机构/供应商说明</summary>
    public string SourceSupplier { get; set; } = "上海我的钢铁网大盘采样";

    /// <summary>是否含 13% 增值税</summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>默认基准交货状态</summary>
    public DeliveryCondition DefaultDelivery { get; set; } = DeliveryCondition.ExWorks_Mill;

    // ================= 大盘基准指数 (元/吨) =================
    /// <summary>普碳钢板 Q235B 基准出厂价 (14~20mm)</summary>
    public double PlateBaseQ235B { get; set; } = 3850.0;

    /// <summary>低合金高强板 Q355B 基准出厂价 (14~20mm)</summary>
    public double PlateBaseQ355B { get; set; } = 4050.0;

    /// <summary>热轧型钢 H型钢 Q235B 基价</summary>
    public double HBeamBaseQ235B { get; set; } = 3950.0;

    /// <summary>热轧型钢 H型钢 Q355B 基价</summary>
    public double HBeamBaseQ355B { get; set; } = 4150.0;

    /// <summary>热轧卷板基价 (方矩管冷弯成型与圆转方基料)</summary>
    public double HotRolledCoilBase { get; set; } = 3900.0;

    /// <summary>直缝焊管基价 (ERW 管材)</summary>
    public double WeldedTubeBase { get; set; } = 4250.0;

    /// <summary>无缝钢管管坯与常规无缝管基价</summary>
    public double SeamlessTubeBase { get; set; } = 5150.0;

    /// <summary>是否已锁定为合同定标基期 (锁定后不参与自动联动)</summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>备注说明</summary>
    public string Remarks { get; set; } = "";
}
