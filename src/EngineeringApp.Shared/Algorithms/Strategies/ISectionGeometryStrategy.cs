using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms.Strategies;

/// <summary>
/// 单个截面几何力学计算策略接口 (Strategy Pattern)
/// 将各个截面类型的力学公式完全解耦为独立策略，消灭巨石 switch-case
/// </summary>
public interface ISectionGeometryStrategy
{
    /// <summary>支持的截面类型</summary>
    SectionType SupportedType { get; }

    /// <summary>执行截面几何特性计算</summary>
    void Calculate(SectionParameters parameters, SectionPropertiesResult result, MaterialProfile material);
}
