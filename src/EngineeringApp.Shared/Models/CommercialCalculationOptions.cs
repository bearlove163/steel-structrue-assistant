namespace EngineeringApp.Shared.Models;

/// <summary>
/// 商务软件算量与成本估算配置选项
/// </summary>
public class CommercialCalculationOptions
{
    /// <summary>单根构件长度 (m)，默认为 1.0 米</summary>
    public double LengthMeter { get; set; } = 1.0;

    /// <summary>构件根数 / 数量，默认为 1 根</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>钢材采购单价 (元/kg)，例如 4.2 元/kg (即 4200 元/吨)</summary>
    public double SteelUnitPricePerKg { get; set; } = 4.20;

    /// <summary>表面防腐/防火涂装单价 (元/m²)</summary>
    public double PaintingUnitPricePerM2 { get; set; } = 25.0;

    /// <summary>钢材下料损耗率 (例如 0.03 表示 3% 损耗，默认 0.0)</summary>
    public double SteelLossRate { get; set; } = 0.0;

    /// <summary>
    /// 涂装计算选项（支持扣除楼板密贴上表面、箱型内腔处理等）
    /// </summary>
    public PaintingCalculationOptions Painting { get; set; } = new();
}
