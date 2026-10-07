using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面涂装与防腐展开表面积计算引擎接口
/// 允许用户/商务软件灵活配置扣除楼板贴合、扣除底面、计算管内浸锌表面积及施工损耗等规则
/// </summary>
public interface IPaintingAreaCalculator
{
    /// <summary>
    /// 计算截面延米净有效涂装表面积 (m²/m)
    /// </summary>
    /// <param name="section">截面几何与力学特性计算结果</param>
    /// <param name="options">涂装计算选项（扣除楼板、扣除底面、管内表面积等）</param>
    /// <returns>净有效涂装面积 (m²/m)</returns>
    double CalculateNetPaintingArea(SectionPropertiesResult section, PaintingCalculationOptions options);

    /// <summary>
    /// 计算指定构件长度下的总涂装表面积 (m²)
    /// </summary>
    /// <param name="section">截面几何与力学特性计算结果</param>
    /// <param name="lengthMeter">构件长度 (m)</param>
    /// <param name="options">涂装计算选项</param>
    /// <returns>总涂装表面积 (m²)</returns>
    double CalculateTotalPaintingArea(SectionPropertiesResult section, double lengthMeter, PaintingCalculationOptions options);
}

/// <summary>
/// 默认涂装表面积计算引擎实现
/// </summary>
public class DefaultPaintingAreaCalculator : IPaintingAreaCalculator
{
    public double CalculateNetPaintingArea(SectionPropertiesResult section, PaintingCalculationOptions options)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(options);

        return section.CalculatePaintingArea(options);
    }

    public double CalculateTotalPaintingArea(SectionPropertiesResult section, double lengthMeter, PaintingCalculationOptions options)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(options);

        double areaPerMeter = CalculateNetPaintingArea(section, options);
        return Math.Max(0, lengthMeter) * areaPerMeter;
    }
}
