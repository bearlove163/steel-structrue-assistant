using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms.Strategies;

/// <summary>
/// 矩形与钢板截面力学计算策略
/// </summary>
public class RectangleSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Rectangle;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);

        res.Area = b * h;
        res.OuterPerimeter = 2.0 * (b + h);
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = b;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = b / 2.0;
        res.Zc = h / 2.0;

        res.Iy = (1.0 / 12.0) * b * Math.Pow(h, 3);
        res.Iz = (1.0 / 12.0) * h * Math.Pow(b, 3);
        res.Iyz = 0.0;
        res.I1 = Math.Max(res.Iy, res.Iz);
        res.I2 = Math.Min(res.Iy, res.Iz);
        res.PrincipalAngleDeg = res.Iy >= res.Iz ? 0.0 : 90.0;

        res.WyTop = (1.0 / 6.0) * b * Math.Pow(h, 2);
        res.WyBot = res.WyTop;
        res.WzLeft = (1.0 / 6.0) * h * Math.Pow(b, 2);
        res.WzRight = res.WzLeft;

        var (wply, wplz) = PlasticModulusEngine.CalculateRectangle(b, h);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateRectangle(b, h);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = (5.0 / 6.0) * res.Area;
        res.ShearAreaZ = (5.0 / 6.0) * res.Area;

        res.OuterContour =
        [
            new(0, 0),
            new(b, 0),
            new(b, h),
            new(0, h)
        ];
    }
}

/// <summary>
/// 组合截面复合策略 (Composite Pattern)
/// 将两个或多个独立子构件的几何力学参数，依据平行轴移轴定理 (Parallel Axis Theorem) 进行代数积分合成
/// </summary>
public class CompositeSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Polygon;

    private readonly List<(ISectionGeometryStrategy Strategy, SectionParameters Params, double OffsetY, double OffsetZ)> _components = [];

    public CompositeSectionStrategy AddPart(ISectionGeometryStrategy strategy, SectionParameters parameters, double offsetY = 0, double offsetZ = 0)
    {
        _components.Add((strategy, parameters, offsetY, offsetZ));
        return this;
    }

    public void Calculate(SectionParameters parameters, SectionPropertiesResult res, MaterialProfile mat)
    {
        if (_components.Count == 0) return;

        double totalArea = 0;
        double sumSy = 0; // 静力矩关于原点
        double sumSz = 0;

        var subResults = new List<(SectionPropertiesResult SubRes, double OffY, double OffZ)>();

        foreach (var (strat, p, offY, offZ) in _components)
        {
            var sub = new SectionPropertiesResult();
            strat.Calculate(p, sub, mat);
            subResults.Add((sub, offY, offZ));

            totalArea += sub.Area;
            sumSz += sub.Area * (sub.Yc + offY); // 静力矩 Sz = \int y dA
            sumSy += sub.Area * (sub.Zc + offZ); // 静力矩 Sy = \int z dA
        }

        res.Area = totalArea;
        if (totalArea <= 0) return;

        res.Yc = sumSz / totalArea; // 形心坐标
        res.Zc = sumSy / totalArea;

        // 平行轴公式累加惯性矩: I = \sum (I_sub + A * d^2)
        double totalIy = 0;
        double totalIz = 0;
        double totalIyz = 0;
        double totalPerimeter = 0;

        foreach (var (sub, offY, offZ) in subResults)
        {
            double dy = (sub.Yc + offY) - res.Yc;
            double dz = (sub.Zc + offZ) - res.Zc;

            totalIy += sub.Iy + sub.Area * Math.Pow(dz, 2);
            totalIz += sub.Iz + sub.Area * Math.Pow(dy, 2);
            totalIyz += sub.Iyz + sub.Area * dy * dz;
            totalPerimeter += sub.Perimeter;
        }

        res.Iy = totalIy;
        res.Iz = totalIz;
        res.Iyz = totalIyz;
        res.Perimeter = totalPerimeter;
        res.OuterPerimeter = totalPerimeter;
        res.ShearAreaY = 0.8 * totalArea;
        res.ShearAreaZ = 0.8 * totalArea;

        // 主惯性矩计算
        double avgI = (totalIy + totalIz) / 2.0;
        double diffI = (totalIy - totalIz) / 2.0;
        double r = Math.Sqrt(diffI * diffI + totalIyz * totalIyz);

        res.I1 = avgI + r;
        res.I2 = avgI - r;
        res.PrincipalAngleDeg = (Math.Abs(totalIyz) > 1e-9)
            ? 0.5 * Math.Atan2(-2.0 * totalIyz, totalIy - totalIz) * (180.0 / Math.PI)
            : 0.0;
    }
}
