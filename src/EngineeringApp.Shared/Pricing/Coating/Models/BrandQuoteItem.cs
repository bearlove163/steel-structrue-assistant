namespace EngineeringApp.Shared.Pricing.Coating;

/// <summary>
/// 品牌实体真实询价记录条目
/// 用于承载佐敦、阿克苏诺贝尔、中远关西、海虹老人等具体品牌的真实绝对单价，
/// 以及支持商务人员随时录入新品牌供应商的实际询报价。
/// </summary>
public class BrandQuoteItem
{
    public string QuoteId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>品牌名称 (如 "佐敦 Jotun", "阿克苏诺贝尔", "中远关西", "新威实业")</summary>
    public string BrandName { get; set; } = "";

    /// <summary>品类划分 (底漆, 中间漆, 面漆, 封闭漆, 防火涂料, 镀锌加工)</summary>
    public string MaterialCategory { get; set; } = "底漆";

    /// <summary>产品型号/规格标识 (如 "Penguard Primer", "Intergard 251")</summary>
    public string ProductModel { get; set; } = "";

    /// <summary>实际询价/签约单价 (元/kg 或 元/t)</summary>
    public double UnitPrice { get; set; }

    /// <summary>计价单位 (默认 元/kg，镀锌加工为 元/t)</summary>
    public string Unit { get; set; } = "元/kg";

    /// <summary>报价供应商/代理商全称</summary>
    public string SupplierName { get; set; } = "";

    /// <summary>询价生效日期</summary>
    public DateTime QuoteDate { get; set; } = DateTime.Today;

    /// <summary>备注说明 (如含税条件、最小起订量、账期)</summary>
    public string Remarks { get; set; } = "";

    /// <summary>是否为用户在界面现场临时录入的新品牌</summary>
    public bool IsCustomAdded { get; set; } = false;
}
