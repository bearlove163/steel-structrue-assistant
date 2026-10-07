namespace EngineeringApp.Shared.Models;

/// <summary>
/// 商务软件专用的截面工程量与成本指标输出结果
/// 聚焦“线重量”、“表面积”、“吨钢比表面积”与“扣除楼板后的净涂装量”
/// </summary>
public class CommercialSectionResult
{
    /// <summary>截面型号或代号 (如 "HW 300×300×10×15" 或 "Box 400×400×16")</summary>
    public string Designation { get; set; } = "";

    /// <summary>截面类型</summary>
    public SectionType SectionType { get; set; }

    /// <summary>理论线密度 / 米重 (kg/m)</summary>
    public double LinearMass { get; set; }

    /// <summary>国标出厂标准理论米重 (kg/m，仅国标型钢具有，可用于商务核对与检尺结算)</summary>
    public double? StandardMassKgM { get; set; }

    /// <summary>结算推荐米重 (kg/m，若存在国标标准重则优先取国标重，否则取几何理论重)</summary>
    public double SettlementMassKgM => StandardMassKgM ?? LinearMass;

    /// <summary>截面全外表面展开延米表面积 (m²/m)</summary>
    public double OuterSurfaceAreaPerMeter { get; set; }

    /// <summary>截面上表面宽度 (mm，例如楼板密贴面宽)</summary>
    public double TopSurfaceWidth { get; set; }

    /// <summary>扣除上表面（楼板贴合）后的延米涂装表面积 (m²/m)</summary>
    public double PaintingAreaExcludingTop { get; set; }

    /// <summary>按照用户指定涂装选项计算得到的净有效延米涂装表面积 (m²/m)</summary>
    public double NetPaintingAreaPerMeter { get; set; }

    /// <summary>内部表面积 (m²/m，闭口箱型/钢管内腔表面)</summary>
    public double InnerSurfaceAreaPerMeter { get; set; }

    /// <summary>吨钢比表面积 (m²/t = 延米外表面积 / 延米吨重)</summary>
    public double AreaPerTon { get; set; }

    /// <summary>板件或冷弯型钢展开料宽 (mm，用于薄壁型钢下料开平卷板采购)</summary>
    public double? FlattedWidth { get; set; }

    /// <summary>单根构件计算长度 (m)</summary>
    public double LengthMeter { get; set; }

    /// <summary>构件数量 (根)</summary>
    public int Quantity { get; set; }

    /// <summary>总延长米 (m = LengthMeter * Quantity)</summary>
    public double TotalLengthMeter => LengthMeter * Quantity;

    /// <summary>理论总重量 (kg，含设定的材料损耗率)</summary>
    public double TotalWeightKg { get; set; }

    /// <summary>理论总重量 (t，吨)</summary>
    public double TotalWeightTons => TotalWeightKg / 1000.0;

    /// <summary>总实际涂装表面积 (m² = TotalLengthMeter * NetPaintingAreaPerMeter)</summary>
    public double TotalPaintingAreaM2 { get; set; }

    /// <summary>估算钢材总采购金额 (元)</summary>
    public double EstimatedSteelCost { get; set; }

    /// <summary>估算防腐/防火涂装总金额 (元)</summary>
    public double EstimatedPaintingCost { get; set; }

    /// <summary>预估综合合计成本 (元)</summary>
    public double TotalEstimatedCost => EstimatedSteelCost + EstimatedPaintingCost;

    /// <summary>原始力学与几何完整特性计算结果引用</summary>
    public SectionPropertiesResult? Properties { get; set; }
}
