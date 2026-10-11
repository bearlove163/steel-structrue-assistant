namespace EngineeringApp.Shared.Pricing.Coating;

/// <summary>
/// 涂料防腐防火价格数据源类型
/// </summary>
public enum PaintPriceSourceType
{
    /// <summary>行业大盘涂装材料信息价 (如造价信息网/涂料行业月刊采样)</summary>
    MarketWebIndex,

    /// <summary>涂料厂家/官方授权经销商直发指导报价 (如佐敦、阿克苏官方报价函)</summary>
    ManufacturerDirect,

    /// <summary>投标合同锁定基期调差价 (用于竣工调差与标底比对)</summary>
    ContractLockedBaseline,

    /// <summary>企业集团集采内控框架协议价 (年度战略集采锁定价)</summary>
    CorporateInternalRate
}

public static class PaintPriceSourceTypeExtensions
{
    public static string GetDisplayName(this PaintPriceSourceType source) => source switch
    {
        PaintPriceSourceType.MarketWebIndex => "行业大盘涂装信息价",
        PaintPriceSourceType.ManufacturerDirect => "涂料厂家直发指导价",
        PaintPriceSourceType.ContractLockedBaseline => "投标合同锁定基期价",
        PaintPriceSourceType.CorporateInternalRate => "企业战略集采框架价",
        _ => source.ToString()
    };
}
