using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Pricing;

/// <summary>
/// 截面涂装与防腐展开表面积计算扩展方法 (通过扩展方法将涂装商业规则解耦，保持截面微内核纯净)
/// </summary>
public static class SectionPaintingExtensions
{
    /// <summary>
    /// 根据涂装计算选项（如扣除楼板、扣除底面、包含内孔等）计算延米净涂装面积 (m²/m)
    /// </summary>
    public static double CalculatePaintingArea(this SectionPropertiesResult prop, PaintingCalculationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(prop);

        if (options == null)
        {
            return (prop.OuterPerimeter > 0 ? prop.OuterPerimeter : prop.Perimeter) / 1000.0;
        }

        double basePerimeter = prop.OuterPerimeter > 0 ? prop.OuterPerimeter : prop.Perimeter;
        if (options.IncludeInnerSurface)
        {
            basePerimeter += prop.InnerPerimeter;
        }

        if (options.ExcludeTopSurface)
        {
            double deduct = options.TopSurfaceDeductionWidth ?? prop.TopSurfaceWidth;
            basePerimeter -= deduct;
        }

        if (options.ExcludeBottomSurface)
        {
            double deduct = options.BottomSurfaceDeductionWidth ?? prop.BottomSurfaceWidth;
            basePerimeter -= deduct;
        }

        double netArea = Math.Max(0, basePerimeter) / 1000.0;
        return netArea * options.LossRatio;
    }

    /// <summary>
    /// 获取指定构件长度下的总有效涂装展开面积 (m²)
    /// </summary>
    public static double CalculateTotalPaintingArea(this SectionPropertiesResult prop, double lengthMeter, PaintingCalculationOptions? options = null)
    {
        double areaPerM = prop.CalculatePaintingArea(options);
        return Math.Max(0, lengthMeter) * areaPerM;
    }
}
