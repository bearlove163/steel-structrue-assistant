using EngineeringApp.Shared.Material;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Pricing;

/// <summary>
/// 构件装配件内包含的零件/板件明细实体 (BOM Leaf Node)
/// </summary>
public class SteelPartItem
{
    public string PartId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string PartMark { get; set; } = "p1"; // 零件编号，如 m1(主肢), br1(牛腿), dp1(隔板)
    public PartFunctionalRole Role { get; set; } = PartFunctionalRole.MainProfile;
    public string PartName { get; set; } = "主肢型钢";

    /// <summary>零件钢材材质牌号</summary>
    public SteelGrade MaterialGrade { get; set; } = SteelGrade.Q355B;

    /// <summary>该零件的物理截面型式 (箱型、H型、圆管、平板等)</summary>
    public SectionType SectionType { get; set; } = SectionType.RHS;

    /// <summary>主要板厚 (mm)</summary>
    public double ThicknessMm { get; set; } = 20.0;

    /// <summary>零件下料长度 (m)</summary>
    public double LengthMeter { get; set; } = 6.0;

    /// <summary>单件零件理论净重量 (kg)</summary>
    public double NetWeightKg { get; set; }

    /// <summary>单件涂装外表面积 (m²)</summary>
    public double NetSurfaceAreaM2 { get; set; }

    /// <summary>一根装配构件内包含此零件的数量</summary>
    public int QuantityPerAssembly { get; set; } = 1;

    /// <summary>零件下料切割损耗率 (默认板材 5%，型钢 3%)</summary>
    public double PartLossRate { get; set; } = 0.05;

    /// <summary>零件采购毛重 (kg)</summary>
    public double GrossWeightKg => NetWeightKg * (1.0 + Math.Max(0, PartLossRate));
}
