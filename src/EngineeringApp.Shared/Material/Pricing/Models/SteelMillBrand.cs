namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢厂品牌与产地信息模型
/// </summary>
public class SteelMillBrand
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string ProductionBase { get; set; } = "";
    public int Tier { get; set; } = 1; // 1: 一线央企/龙头, 2: 区域龙头/大型民营, 3: 普碳小钢厂
    
    /// <summary>相对大盘基准价格的品牌溢价 (元/吨)</summary>
    public double BrandPremiumPerTon { get; set; }
    
    /// <summary>钢厂核心优势品种描述</summary>
    public string SpecialtyDescription { get; set; } = "";
    
    /// <summary>是否支持特定严苛认证 (如 EN 10204 3.2 第三方验资)</summary>
    public bool SupportsThirdPartyCertification { get; set; } = true;

    /// <summary>是否为当前常用推荐品牌</summary>
    public bool IsRecommended { get; set; }
}
