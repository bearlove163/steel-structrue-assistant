using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Standards;

/// <summary>
/// 中国国家标准体系工程工厂 (GB / GB/T)
/// 涵盖: GB 50017-2017《钢结构设计标准》、GB/T 709-2019《热轧钢板和钢带的尺寸、外形、重量及允许偏差》
/// </summary>
public class GbEngineeringStandardFactory : IEngineeringStandardFactory
{
    public StandardSystem SystemType => StandardSystem.GB;
    public string SystemCode => "GB 50017-2017 / GB/T 709-2019";
    public string SystemName => "中国国家标准体系 (GB)";

    public IStandardPricingStrategy CreatePricingStrategy() => new GbPricingStrategy();
    public IToleranceRuleEngine CreateToleranceEngine() => new GbT709ToleranceEngine();
    public IStructuralLimitsEngine CreateLimitsEngine() => new Gb50017LimitsEngine();
    public IStandardFormulaFormatter CreateFormulaFormatter() => new GbFormulaFormatter();
}

public class GbT709ToleranceEngine : IToleranceRuleEngine
{
    public string StandardCode => "GB/T 709-2019 表 1 (B 类偏差)";

    public double GetThicknessTolerance(double nominalThicknessMm, string toleranceClass = "B")
    {
        // 国标 GB/T 709 B类公差 (允许偏差下限收紧，适用于工程结构钢板)
        return nominalThicknessMm switch
        {
            <= 10 => 0.45,
            <= 16 => 0.55,
            <= 25 => 0.65,
            <= 40 => 0.75,
            <= 60 => 0.90,
            <= 100 => 1.10,
            _ => 1.30
        };
    }
}

public class Gb50017LimitsEngine : IStructuralLimitsEngine
{
    public string CodeReference => "GB 50017-2017 第 7.4 条";

    public double GetTensionSlendernessLimit() => 350.0; // 受拉构件限值 [λ]=350
    public double GetCompressionSlendernessLimit(bool isMainMember = true) => isMainMember ? 150.0 : 200.0; // 受压主构件 150，支撑 200
    public double GetResistanceFactor() => 1.087; // 抗力分项系数 γ_R ≈ 1.087 (f = fy / 1.11)
}

public class GbFormulaFormatter : IStandardFormulaFormatter
{
    public string SpecificationName => "GB 50017-2017《钢结构设计标准》";

    public string FormatYieldStress(double fy) =>
        $"设计抗拉/抗压强度 f = {(fy / 1.11):F1} MPa (由屈服强度 fy = {fy:F1} MPa 经抗力分项系数折算)";

    public string FormatSafetyCheck(string component, double demand, double capacity, double ratio) =>
        $"{component}: 正应力利用率 η = {ratio:P1} (σ = {demand:F1} MPa ≤ [f] = {capacity:F1} MPa)";
}
