using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Standards;

/// <summary>
/// 美国标准体系工程工厂 (ASTM / AISC)
/// 涵盖: AISC 360-22《结构钢建筑规范》、ASTM A6/A6M《结构轧制钢板与型钢通用要求》
/// </summary>
public class AstmEngineeringStandardFactory : IEngineeringStandardFactory
{
    public StandardSystem SystemType => StandardSystem.ASTM;
    public string SystemCode => "AISC 360-22 / ASTM A6/A6M";
    public string SystemName => "美国标准体系 (AISC / ASTM)";

    public IStandardPricingStrategy CreatePricingStrategy() => new AstmPricingStrategy();
    public IToleranceRuleEngine CreateToleranceEngine() => new AstmA6ToleranceEngine();
    public IStructuralLimitsEngine CreateLimitsEngine() => new Aisc360LimitsEngine();
    public IStandardFormulaFormatter CreateFormulaFormatter() => new AiscFormulaFormatter();
}

public class AstmA6ToleranceEngine : IToleranceRuleEngine
{
    public string StandardCode => "ASTM A6/A6M Table 1";

    public double GetThicknessTolerance(double nominalThicknessMm, string toleranceClass = "B")
    {
        return nominalThicknessMm switch
        {
            <= 10 => 0.40,
            <= 20 => 0.55,
            <= 30 => 0.70,
            <= 50 => 0.85,
            <= 100 => 1.15,
            _ => 1.40
        };
    }
}

public class Aisc360LimitsEngine : IStructuralLimitsEngine
{
    public string CodeReference => "AISC 360-22 Section E2 & D1";

    public double GetTensionSlendernessLimit() => 300.0; // AISC User Note: L/r preferably not exceed 300
    public double GetCompressionSlendernessLimit(bool isMainMember = true) => 200.0; // AISC E2: KL/r shall not exceed 200
    public double GetResistanceFactor() => 0.90; // LRFD resistance factor φ = 0.90
}

public class AiscFormulaFormatter : IStandardFormulaFormatter
{
    public string SpecificationName => "AISC 360-22 Specification for Structural Steel Buildings";

    public string FormatYieldStress(double fy) =>
        $"Design yield stress φFy = {(fy * 0.90):F1} MPa (LRFD resistance factor φ = 0.90, nominal Fy = {fy:F1} MPa)";

    public string FormatSafetyCheck(string component, double demand, double capacity, double ratio) =>
        $"{component}: Demand/Capacity Ratio DCR = {ratio:P1} (Required strength Ru = {demand:F1} MPa ≤ Available strength φRn = {capacity:F1} MPa)";
}
