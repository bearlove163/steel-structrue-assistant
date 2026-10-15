using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms.Strategies;

/// <summary>
/// 截面几何力学计算策略注册表 (Registry Pattern)
/// 遵循开闭原则 (OCP)，支持无侵入式注册新型截面计算策略或组合构件
/// </summary>
public static class SectionStrategyRegistry
{
    private static readonly Dictionary<SectionType, ISectionGeometryStrategy> _strategies = new();
    private static readonly ParametricSectionCalculator _defaultCalculator = new();

    static SectionStrategyRegistry()
    {
        // 注册已实现的一级策略
        RegisterStrategy(new RectangleSectionStrategy());
    }

    /// <summary>注册或覆盖特定截面类型的计算策略</summary>
    public static void RegisterStrategy(ISectionGeometryStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies[strategy.SupportedType] = strategy;
    }

    /// <summary>根据截面类型获取计算策略；若未注册独立类则回退至通用参数化引擎</summary>
    public static void ExecuteCalculation(SectionParameters parameters, SectionPropertiesResult result, MaterialProfile material)
    {
        if (_strategies.TryGetValue(parameters.Type, out var strategy))
        {
            strategy.Calculate(parameters, result, material);
        }
        else
        {
            // 回退到内置参数化求解器
            var fallbackRes = _defaultCalculator.Calculate(parameters);
            CopyProperties(fallbackRes, result);
        }
    }

    private static void CopyProperties(SectionPropertiesResult src, SectionPropertiesResult dest)
    {
        dest.Area = src.Area;
        dest.LinearMass = src.LinearMass;
        dest.Perimeter = src.Perimeter;
        dest.OuterPerimeter = src.OuterPerimeter;
        dest.InnerPerimeter = src.InnerPerimeter;
        dest.TopSurfaceWidth = src.TopSurfaceWidth;
        dest.BottomSurfaceWidth = src.BottomSurfaceWidth;
        dest.Ymin = src.Ymin; dest.Ymax = src.Ymax;
        dest.Zmin = src.Zmin; dest.Zmax = src.Zmax;
        dest.Yc = src.Yc; dest.Zc = src.Zc;
        dest.Iy = src.Iy; dest.Iz = src.Iz; dest.Iyz = src.Iyz;
        dest.I1 = src.I1; dest.I2 = src.I2; dest.PrincipalAngleDeg = src.PrincipalAngleDeg;
        dest.WyTop = src.WyTop; dest.WyBot = src.WyBot;
        dest.WzLeft = src.WzLeft; dest.WzRight = src.WzRight;
        dest.Wply = src.Wply; dest.Wplz = src.Wplz;
        dest.TorsionConstantJ = src.TorsionConstantJ;
        dest.WarpingConstantIw = src.WarpingConstantIw;
        dest.ShearAreaY = src.ShearAreaY; dest.ShearAreaZ = src.ShearAreaZ;
        dest.OuterContour = src.OuterContour;
        dest.InnerContours = src.InnerContours;
    }
}
