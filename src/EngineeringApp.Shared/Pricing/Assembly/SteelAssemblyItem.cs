using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Quota;

namespace EngineeringApp.Shared.Pricing;

/// <summary>
/// 构件装配层级模型 (Assembly / Mark Node，如 GZ1 箱型柱 或 GL1 钢梁)
/// 支撑构件级 BOM 汇总、材料毛重采购及正交工艺组价
/// </summary>
public class SteelAssemblyItem
{
    public string AssemblyId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>工程构件编号 (如 GZ1, GL1, ZC1)</summary>
    public string AssemblyMark { get; set; } = "GZ1";

    // ================= 正交双字段核心 =================
    /// <summary>字段一：构件工程角色 (决定清单归类、安装工法与附属节点工价)</summary>
    public MemberRole Role { get; set; } = MemberRole.Column;

    /// <summary>字段二：主肢物理截面类型 (决定主干母材下料与主焊缝工艺)</summary>
    public SectionType MainSectionType { get; set; } = SectionType.RHS;

    /// <summary>工程总套数/根数</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>构件总长度/标高跨度 (m)</summary>
    public double OverallLengthMeter { get; set; } = 6.0;

    /// <summary>所属图纸子项 / 建筑楼层标高分区</summary>
    public string FloorOrZone { get; set; } = "1F-4F 标高区间";

    /// <summary>装配件包含的所有内部板件/零件清单 (BOM Leaf Nodes)</summary>
    public List<SteelPartItem> Parts { get; set; } = [];

    // ================= 物理指标汇总 =================
    /// <summary>单根构件净重 (kg)</summary>
    public double SingleAssemblyNetWeightKg => Parts.Sum(p => p.NetWeightKg * p.QuantityPerAssembly);

    /// <summary>单根构件采购毛重 (kg)</summary>
    public double SingleAssemblyGrossWeightKg => Parts.Sum(p => p.GrossWeightKg * p.QuantityPerAssembly);

    /// <summary>单根构件展开涂装外表面积 (m²)</summary>
    public double SingleAssemblyPaintingAreaM2 => Parts.Sum(p => p.NetSurfaceAreaM2 * p.QuantityPerAssembly);

    /// <summary>本批次构件总净重 (吨 t)</summary>
    public double TotalBatchNetWeightTon => (SingleAssemblyNetWeightKg * Quantity) / 1000.0;

    /// <summary>本批次构件总采购毛重 (吨 t)</summary>
    public double TotalBatchGrossWeightTon => (SingleAssemblyGrossWeightKg * Quantity) / 1000.0;

    /// <summary>本批次构件总涂装面积 (m²)</summary>
    public double TotalBatchPaintingAreaM2 => SingleAssemblyPaintingAreaM2 * Quantity;

    /// <summary>构件中最大主要板厚 (mm)</summary>
    public double MaxThicknessMm => Parts.Any() ? Parts.Max(p => p.ThicknessMm) : 20.0;

    /// <summary>外伸牛腿总数量</summary>
    public int CantileverBracketCount => Parts.Where(p => p.Role == PartFunctionalRole.BracketBeam).Sum(p => p.QuantityPerAssembly);

    /// <summary>节点板/牛腿等配件重量占整构件比例 (%)</summary>
    public double FittingWeightRatio => SingleAssemblyNetWeightKg <= 0 ? 0 :
        (Parts.Where(p => p.Role != PartFunctionalRole.MainProfile).Sum(p => p.NetWeightKg * p.QuantityPerAssembly) / SingleAssemblyNetWeightKg) * 100.0;
}
