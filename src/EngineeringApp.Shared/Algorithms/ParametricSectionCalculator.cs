using EngineeringApp.Shared.Algorithms.Strategies;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 参数化截面与任意多边形力学特性综合计算器 (已全面重构为策略注册表模式 Registry + Strategy Pattern)
/// 彻底消灭 700 行 switch-case 巨石代码，截面计算独立解耦为单一职责策略
/// </summary>
public class ParametricSectionCalculator : ISectionCalculator
{
    public SectionPropertiesResult Calculate(SectionParameters p)
    {
        var res = new SectionPropertiesResult();
        var mat = p.Material ?? new MaterialProfile();

        // 统一通过策略注册表分发计算，无需巨大的 switch-case
        SectionStrategyRegistry.ExecuteCalculation(p, res, mat);

        // 计算线密度: 质量 m (kg/m) = A (m²) * ρ (kg/m³) = (A / 1,000,000) * ρ
        res.LinearMass = (res.Area / 1e6) * mat.Density;

        return res;
    }
}
