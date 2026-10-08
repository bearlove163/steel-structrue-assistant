namespace EngineeringApp.Shared.Material;

/// <summary>
/// 结构工程材料力学与物性模型 (零商务依赖的材料物理基石)
/// </summary>
public class MaterialProfile
{
    public string Name { get; set; } = "Q235 碳素结构钢";

    /// <summary>标准钢材材质枚举 (可选)</summary>
    public SteelGrade Grade { get; set; } = SteelGrade.Q235B;

    /// <summary>弹性模量 E (MPa = N/mm²)</summary>
    public double ElasticModulus { get; set; } = 206000.0;

    /// <summary>屈服强度 fy (MPa)</summary>
    public double YieldStrength { get; set; } = 235.0;

    /// <summary>抗拉强度极限 fu (MPa)</summary>
    public double TensileStrength { get; set; } = 370.0;

    /// <summary>泊松比 ν</summary>
    public double PoissonRatio { get; set; } = 0.3;

    /// <summary>剪变模量 G (MPa)</summary>
    public double ShearModulus => ElasticModulus / (2.0 * (1.0 + PoissonRatio));

    /// <summary>材料密度 ρ (kg/m³)，钢结构标准为 7850</summary>
    public double Density { get; set; } = 7850.0;

    /// <summary>热膨胀系数 α (1/℃)</summary>
    public double ThermalExpansionCoefficient { get; set; } = 1.2e-5;

    /// <summary>预置典型工程结构材料列表</summary>
    public static List<MaterialProfile> Presets =>
    [
        new MaterialProfile { Name = "Q235 碳素结构钢", Grade = SteelGrade.Q235B, ElasticModulus = 206000, YieldStrength = 235, TensileStrength = 370, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "Q355 低合金高强钢", Grade = SteelGrade.Q355B, ElasticModulus = 206000, YieldStrength = 355, TensileStrength = 470, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "Q420 高强结构钢", Grade = SteelGrade.Q420B, ElasticModulus = 206000, YieldStrength = 420, TensileStrength = 520, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "S355 欧标高强钢", Grade = SteelGrade.S355JR, ElasticModulus = 210000, YieldStrength = 355, TensileStrength = 490, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "A992 美标结构型钢", Grade = SteelGrade.A992, ElasticModulus = 200000, YieldStrength = 345, TensileStrength = 450, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "S30408 不锈钢", Grade = SteelGrade.StainlessSteel_S30408, ElasticModulus = 200000, YieldStrength = 205, TensileStrength = 520, PoissonRatio = 0.3, Density = 7930 },
        new MaterialProfile { Name = "6061-T6 结构铝合金", Grade = SteelGrade.Aluminum_6061T6, ElasticModulus = 68900, YieldStrength = 240, TensileStrength = 290, PoissonRatio = 0.33, Density = 2700 },
        new MaterialProfile { Name = "C30 结构混凝土", Grade = SteelGrade.Custom, ElasticModulus = 30000, YieldStrength = 20.1, TensileStrength = 2.01, PoissonRatio = 0.2, Density = 2500 },
        new MaterialProfile { Name = "TC11 结构木材", Grade = SteelGrade.Custom, ElasticModulus = 10000, YieldStrength = 13.0, TensileStrength = 13.0, PoissonRatio = 0.35, Density = 500 },
        new MaterialProfile { Name = "自定义材料", Grade = SteelGrade.Custom, ElasticModulus = 206000, YieldStrength = 345, TensileStrength = 470, PoissonRatio = 0.3, Density = 7850 }
    ];
}
