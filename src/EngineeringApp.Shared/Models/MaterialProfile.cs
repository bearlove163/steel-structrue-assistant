namespace EngineeringApp.Shared.Models;

/// <summary>
/// 结构工程材料模型
/// </summary>
public class MaterialProfile
{
    public string Name { get; set; } = "Q235 碳素结构钢";

    /// <summary>弹性模量 E (MPa = N/mm²)</summary>
    public double ElasticModulus { get; set; } = 206000.0;

    /// <summary>屈服强度 fy (MPa)</summary>
    public double YieldStrength { get; set; } = 235.0;

    /// <summary>泊松比 ν</summary>
    public double PoissonRatio { get; set; } = 0.3;

    /// <summary>剪变模量 G (MPa)</summary>
    public double ShearModulus => ElasticModulus / (2.0 * (1.0 + PoissonRatio));

    /// <summary>材料密度 ρ (kg/m³)</summary>
    public double Density { get; set; } = 7850.0;

    /// <summary>预置典型工程材料列表</summary>
    public static List<MaterialProfile> Presets =>
    [
        new MaterialProfile { Name = "Q235 碳素结构钢", ElasticModulus = 206000, YieldStrength = 235, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "Q355 低合金高强钢", ElasticModulus = 206000, YieldStrength = 355, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "Q420 高强结构钢", ElasticModulus = 206000, YieldStrength = 420, PoissonRatio = 0.3, Density = 7850 },
        new MaterialProfile { Name = "S30408 不锈钢", ElasticModulus = 200000, YieldStrength = 205, PoissonRatio = 0.3, Density = 7930 },
        new MaterialProfile { Name = "6061-T6 结构铝合金", ElasticModulus = 68900, YieldStrength = 240, PoissonRatio = 0.33, Density = 2700 },
        new MaterialProfile { Name = "C30 结构混凝土", ElasticModulus = 30000, YieldStrength = 20.1, PoissonRatio = 0.2, Density = 2500 },
        new MaterialProfile { Name = "TC11 结构木材", ElasticModulus = 10000, YieldStrength = 13.0, PoissonRatio = 0.35, Density = 500 },
        new MaterialProfile { Name = "自定义材料", ElasticModulus = 206000, YieldStrength = 345, PoissonRatio = 0.3, Density = 7850 }
    ];
}
