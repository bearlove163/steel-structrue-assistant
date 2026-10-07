namespace EngineeringApp.Shared.Models;

/// <summary>
/// 涂装防腐与防火涂层层次分类
/// </summary>
public enum CoatingLayerType
{
    /// <summary>底漆 (Primer: 环氧富锌/无机富锌/醇酸防锈底漆)</summary>
    Primer,

    /// <summary>中间漆 (Intermediate: 环氧云铁厚浆中间漆)</summary>
    Intermediate,

    /// <summary>封闭漆 (Sealer: 环氧封闭漆/聚氨酯封闭底漆)</summary>
    Sealer,

    /// <summary>面漆 (Topcoat: 丙烯酸聚氨酯/氟碳/醇酸面漆)</summary>
    Topcoat,

    /// <summary>防火涂料 (Fireproof: 室内膨胀型超薄型/厚型钢结构防火涂料)</summary>
    Fireproof
}

/// <summary>
/// 油漆与涂料产品库条目
/// </summary>
public class PaintProductItem
{
    public int Id { get; set; }
    public CoatingLayerType LayerType { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "工业标准品";
    public string Description { get; set; } = "";

    /// <summary>固体体积含量 (Volume Solids %，如 65 表示 65%)</summary>
    public double VolumeSolidsPercent { get; set; } = 60.0;

    /// <summary>涂料比重 (kg/L)</summary>
    public double DensityKgL { get; set; } = 1.35;

    /// <summary>推荐设计干膜厚度 (微米 μm，防火涂料为 mm)</summary>
    public double DefaultDft { get; set; } = 75.0;

    /// <summary>施工损耗率估计 (%，如 30 表示 30%)</summary>
    public double LossRatePercent { get; set; } = 30.0;

    /// <summary>涂料材料单价 (元/kg)</summary>
    public double MaterialUnitPricePerKg { get; set; } = 35.0;

    /// <summary>现场喷涂施工工费单价 (元/m²)</summary>
    public double LaborUnitPricePerM2 { get; set; } = 15.0;
}

/// <summary>
/// 单层涂料设计计算输入参数
/// </summary>
public class CoatingLayerConfig
{
    public CoatingLayerType LayerType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string ProductName { get; set; } = "";

    /// <summary>设计干膜厚度 (μm，防火涂料单位为 mm)</summary>
    public double Dft { get; set; } = 70.0;

    /// <summary>固体体积含量 (%)</summary>
    public double VolumeSolids { get; set; } = 60.0;

    /// <summary>涂料密度 (kg/L)</summary>
    public double Density { get; set; } = 1.35;

    /// <summary>施工喷涂损耗率 (%)</summary>
    public double LossRate { get; set; } = 30.0;

    /// <summary>材料单价 (元/kg)</summary>
    public double MaterialPrice { get; set; } = 35.0;

    /// <summary>施工人工与辅材单价 (元/m²)</summary>
    public double LaborPrice { get; set; } = 15.0;

    public string GetDisplayName() => LayerType switch
    {
        CoatingLayerType.Primer => "底漆 (Primer)",
        CoatingLayerType.Intermediate => "中间漆 (Intermediate)",
        CoatingLayerType.Sealer => "封闭漆 (Sealer)",
        CoatingLayerType.Topcoat => "面漆 (Topcoat)",
        CoatingLayerType.Fireproof => "防火涂料 (Fireproof)",
        _ => LayerType.ToString()
    };
}

/// <summary>
/// 单层涂装计算结果详情
/// </summary>
public class CoatingLayerResult
{
    public CoatingLayerType LayerType { get; set; }
    public string LayerName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public double Dft { get; set; }

    /// <summary>理论涂布率 (m²/kg)</summary>
    public double TheoreticalCoverageM2Kg { get; set; }

    /// <summary>实际消耗量指标 (kg/m²)</summary>
    public double ActualUsageKgPerM2 { get; set; }

    /// <summary>每平方米材料成本 (元/m²)</summary>
    public double MaterialCostPerM2 { get; set; }

    /// <summary>每平方米施工工费 (元/m²)</summary>
    public double LaborCostPerM2 { get; set; }

    /// <summary>每平方米综合单价 (元/m²)</summary>
    public double TotalCostPerM2 => MaterialCostPerM2 + LaborCostPerM2;

    /// <summary>延米涂料用量 (kg/m)</summary>
    public double LinearUsageKgPerMeter { get; set; }

    /// <summary>延米涂装综合造价 (元/m)</summary>
    public double LinearCostPerMeter { get; set; }
}

/// <summary>
/// 涂装防腐防火系统综合计算成果
/// </summary>
public class PaintSystemCalculationResult
{
    /// <summary>参与计算的有效图层列表</summary>
    public List<CoatingLayerResult> Layers { get; set; } = [];

    /// <summary>采用的构件涂装面积基准 (m²/m)</summary>
    public double AppliedPaintingAreaPerMeter { get; set; }

    /// <summary>是否扣除了楼板贴合上表面</summary>
    public bool TopSurfaceExcluded { get; set; }

    /// <summary>截面吨钢展开比表面积 (m²/t)</summary>
    public double AreaPerTon { get; set; }

    /// <summary>总干膜厚度 (微米 μm，不含防火涂料)</summary>
    public double TotalPaintDftMicrons { get; set; }

    /// <summary>防火涂料厚度 (mm)</summary>
    public double FireproofThicknessMm { get; set; }

    /// <summary>每平方米综合造价合计 (元/m²)</summary>
    public double TotalCostPerM2 { get; set; }

    /// <summary>每平方米材料费合计 (元/m²)</summary>
    public double TotalMaterialCostPerM2 { get; set; }

    /// <summary>每平方米工费合计 (元/m²)</summary>
    public double TotalLaborCostPerM2 { get; set; }

    /// <summary>延米涂料总用量 (kg/m)</summary>
    public double TotalUsageKgPerMeter { get; set; }

    /// <summary>延米涂装综合总造价 (元/m)</summary>
    public double TotalCostPerMeter { get; set; }

    /// <summary>吨钢涂料总消耗量 (kg/t)</summary>
    public double TotalUsageKgPerTon { get; set; }

    /// <summary>吨钢涂装综合总造价 (元/t)</summary>
    public double TotalCostPerTon { get; set; }

    // 工程批量估算数据
    /// <summary>输入构件总长度 (m)</summary>
    public double ProjectTotalLengthM { get; set; } = 100.0;

    /// <summary>工程总涂料采购重量 (kg)</summary>
    public double ProjectTotalPaintKg => TotalUsageKgPerMeter * ProjectTotalLengthM;

    /// <summary>工程涂装防腐总造价 (元)</summary>
    public double ProjectTotalCostYuan => TotalCostPerMeter * ProjectTotalLengthM;
}
