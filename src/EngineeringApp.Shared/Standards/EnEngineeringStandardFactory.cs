using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Standards;

/// <summary>
/// 欧洲标准体系工程工厂 (EN / Eurocode)
/// 涵盖: EN 1993-1-1 (Eurocode 3)《钢结构设计》、EN 10029《热轧钢板厚度偏差》
/// </summary>
public class EnEngineeringStandardFactory : IEngineeringStandardFactory
{
    public StandardSystem SystemType => StandardSystem.EN;
    public string SystemCode => "EN 1993-1-1:2005 / EN 10029:2010";
    public string SystemName => "欧洲标准体系 (Eurocode / EN)";

    public IStandardPricingStrategy CreatePricingStrategy() => new EnPricingStrategy();
    public IToleranceRuleEngine CreateToleranceEngine() => new En10029ToleranceEngine();
    public IStructuralLimitsEngine CreateLimitsEngine() => new En1993LimitsEngine();
    public IStandardFormulaFormatter CreateFormulaFormatter() => new EnFormulaFormatter();
}

public class En10029ToleranceEngine : IToleranceRuleEngine
{
    public string StandardCode => "EN 10029:2010 Table 1 (Class B)";

    public double GetThicknessTolerance(double nominalThicknessMm, string toleranceClass = "B")
    {
        return nominalThicknessMm switch
        {
            <= 8 => 0.50,
            <= 15 => 0.60,
            <= 25 => 0.70,
            <= 40 => 0.80,
            <= 80 => 1.00,
            _ => 1.25
        };
    }
}

public class En1993LimitsEngine : IStructuralLimitsEngine
{
    public string CodeReference => "EN 1993-1-1:2005 Clause 6.3";

    public double GetTensionSlendernessLimit() => 300.0;
    public double GetCompressionSlendernessLimit(bool isMainMember = true) => isMainMember ? 180.0 : 220.0;
    public double GetResistanceFactor() => 1.0; // γ_M0 = 1.0
}

public class EnFormulaFormatter : IStandardFormulaFormatter
{
    public string SpecificationName => "EN 1993-1-1:2005 (Eurocode 3)";

    public string FormatYieldStress(double fy) =>
        $"Design yield strength fy,d = {fy:F1} MPa (Partial safety factor γM0 = 1.00)";

    public string FormatSafetyCheck(string component, double demand, double capacity, double ratio) =>
        $"{component}: Stress utilization ratio η = {ratio:P1} (Ed = {demand:F1} MPa ≤ Rd = {capacity:F1} MPa)";
}
