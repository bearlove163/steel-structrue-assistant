using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面几何与力学特性计算引擎统一接口
/// </summary>
public interface ISectionCalculator
{
    /// <summary>
    /// 根据输入的几何与材料参数计算截面全套特性
    /// </summary>
    SectionPropertiesResult Calculate(SectionParameters parameters);
}
