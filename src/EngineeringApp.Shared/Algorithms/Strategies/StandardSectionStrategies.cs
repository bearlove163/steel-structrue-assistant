using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms.Strategies;

/// <summary>
/// 辅助几何工具方法
/// </summary>
public static class GeometryUtils
{
    public static List<Point2D> GenerateCirclePolygon(double cx, double cy, double r, int segments)
    {
        var pts = new List<Point2D>(segments);
        for (int i = 0; i < segments; i++)
        {
            double theta = 2.0 * Math.PI * i / segments;
            pts.Add(new Point2D(cx + r * Math.Cos(theta), cy + r * Math.Sin(theta)));
        }
        return pts;
    }
}

/// <summary>
/// 实心矩形截面几何力学计算策略
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
/// 钢板截面力学计算策略
/// </summary>
public class PlateSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Plate;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double t = Math.Max(0.5, p.LegThickness > 0 ? p.LegThickness : p.Height);
        double b = Math.Max(10.0, p.Width > 0 ? p.Width : 1000.0);

        res.Area = b * t;
        res.OuterPerimeter = 2.0 * (b + t);
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = b;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = t;
        res.Yc = b / 2.0;
        res.Zc = t / 2.0;

        res.Iy = (1.0 / 12.0) * b * Math.Pow(t, 3);
        res.Iz = (1.0 / 12.0) * t * Math.Pow(b, 3);
        res.Iyz = 0.0;
        res.I1 = Math.Max(res.Iy, res.Iz);
        res.I2 = Math.Min(res.Iy, res.Iz);
        res.PrincipalAngleDeg = 0.0;

        res.WyTop = (1.0 / 6.0) * b * Math.Pow(t, 2);
        res.WyBot = res.WyTop;
        res.WzLeft = (1.0 / 6.0) * t * Math.Pow(b, 2);
        res.WzRight = res.WzLeft;

        var (wply, wplz) = PlasticModulusEngine.CalculateRectangle(b, t);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateRectangle(b, t);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = (5.0 / 6.0) * res.Area;
        res.ShearAreaZ = (5.0 / 6.0) * res.Area;

        res.OuterContour =
        [
            new(0, 0),
            new(b, 0),
            new(b, t),
            new(0, t)
        ];
    }
}

/// <summary>
/// 实心圆形截面力学计算策略
/// </summary>
public class CircleSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Circle;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double d = Math.Max(1.0, p.OuterDiameter);
        double r = d / 2.0;

        res.Area = Math.PI * r * r;
        res.OuterPerimeter = Math.PI * d;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = 0;
        res.BottomSurfaceWidth = 0;
        res.Ymin = 0; res.Ymax = d;
        res.Zmin = 0; res.Zmax = d;
        res.Yc = r;
        res.Zc = r;

        res.Iy = Math.PI * Math.Pow(d, 4) / 64.0;
        res.Iz = res.Iy;
        res.Iyz = 0.0;
        res.I1 = res.Iy;
        res.I2 = res.Iz;
        res.PrincipalAngleDeg = 0.0;

        res.WyTop = Math.PI * Math.Pow(d, 3) / 32.0;
        res.WyBot = res.WyTop;
        res.WzLeft = res.WyTop;
        res.WzRight = res.WyTop;

        var (wply, wplz) = PlasticModulusEngine.CalculateCircle(d);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateCircle(d);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = 0.9 * res.Area;
        res.ShearAreaZ = 0.9 * res.Area;

        res.IsCircular = true;
        res.CircleOuterRadius = r;
        res.CircleInnerRadius = 0;

        res.OuterContour = GeometryUtils.GenerateCirclePolygon(r, r, r, 64);
    }
}

/// <summary>
/// 空心圆管 (CHS) 截面力学计算策略
/// </summary>
public class ChsSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.CHS;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double d = Math.Max(1.0, p.OuterDiameter);
        double t = Math.Clamp(p.WallThickness, 0.1, d / 2.0 - 0.1);
        double dInner = Math.Max(0.1, d - 2.0 * t);
        double ro = d / 2.0;
        double ri = dInner / 2.0;

        res.Area = Math.PI * (ro * ro - ri * ri);
        res.OuterPerimeter = Math.PI * d;
        res.InnerPerimeter = Math.PI * dInner;
        res.Perimeter = res.OuterPerimeter + res.InnerPerimeter;
        res.TopSurfaceWidth = 0;
        res.BottomSurfaceWidth = 0;
        res.Ymin = 0; res.Ymax = d;
        res.Zmin = 0; res.Zmax = d;
        res.Yc = ro;
        res.Zc = ro;

        res.Iy = Math.PI * (Math.Pow(d, 4) - Math.Pow(dInner, 4)) / 64.0;
        res.Iz = res.Iy;
        res.Iyz = 0.0;
        res.I1 = res.Iy;
        res.I2 = res.Iz;
        res.PrincipalAngleDeg = 0.0;

        res.WyTop = res.Iy / ro;
        res.WyBot = res.WyTop;
        res.WzLeft = res.WyTop;
        res.WzRight = res.WyTop;

        var (wply, wplz) = PlasticModulusEngine.CalculateCHS(d, t);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateCHS(d, t);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = 0.5 * res.Area;
        res.ShearAreaZ = 0.5 * res.Area;

        res.IsCircular = true;
        res.CircleOuterRadius = ro;
        res.CircleInnerRadius = ri;

        res.OuterContour = GeometryUtils.GenerateCirclePolygon(ro, ro, ro, 64);
        res.InnerContours = [GeometryUtils.GenerateCirclePolygon(ro, ro, ri, 64)];
    }
}

/// <summary>
/// 方矩管 / 箱型截面 (RHS) 截面力学计算策略
/// </summary>
public class RhsSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.RHS;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double t = Math.Clamp(p.WallThickness, 0.1, Math.Min(b, h) / 2.0 - 0.1);

        double bi = Math.Max(0.1, b - 2.0 * t);
        double hi = Math.Max(0.1, h - 2.0 * t);

        res.Area = b * h - bi * hi;
        res.OuterPerimeter = 2.0 * (b + h);
        res.InnerPerimeter = 2.0 * (bi + hi);
        res.Perimeter = res.OuterPerimeter + res.InnerPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = b;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = b / 2.0;
        res.Zc = h / 2.0;

        res.Iy = (1.0 / 12.0) * (b * Math.Pow(h, 3) - bi * Math.Pow(hi, 3));
        res.Iz = (1.0 / 12.0) * (h * Math.Pow(b, 3) - hi * Math.Pow(bi, 3));
        res.Iyz = 0.0;
        res.I1 = Math.Max(res.Iy, res.Iz);
        res.I2 = Math.Min(res.Iy, res.Iz);
        res.PrincipalAngleDeg = res.Iy >= res.Iz ? 0.0 : 90.0;

        res.WyTop = res.Iy / (h / 2.0);
        res.WyBot = res.WyTop;
        res.WzLeft = res.Iz / (b / 2.0);
        res.WzRight = res.WzLeft;

        var (wply, wplz) = PlasticModulusEngine.CalculateRHS(b, h, t);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateRHS(b, h, t);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = 2.0 * bi * t;
        res.ShearAreaZ = 2.0 * hi * t;

        res.OuterContour =
        [
            new(0, 0),
            new(b, 0),
            new(b, h),
            new(0, h)
        ];

        res.InnerContours =
        [
            [
                new(t, t),
                new(t, t + hi),
                new(t + bi, t + hi),
                new(t + bi, t)
            ]
        ];
    }
}

/// <summary>
/// H型钢 / 工字钢截面力学计算策略
/// </summary>
public class HBeamSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.HBeam;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h / 2.0 - 0.1);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);

        double hw = h - 2.0 * tf;
        double aFlanges = 2.0 * b * tf;
        double aWeb = hw * tw;

        res.Area = aFlanges + aWeb;
        res.OuterPerimeter = 2.0 * (2.0 * b + h - tw);
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = b;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = b / 2.0;
        res.Zc = h / 2.0;

        res.Iy = (1.0 / 12.0) * (b * Math.Pow(h, 3) - (b - tw) * Math.Pow(hw, 3));
        res.Iz = (2.0 / 12.0) * tf * Math.Pow(b, 3) + (1.0 / 12.0) * hw * Math.Pow(tw, 3);
        res.Iyz = 0.0;
        res.I1 = Math.Max(res.Iy, res.Iz);
        res.I2 = Math.Min(res.Iy, res.Iz);
        res.PrincipalAngleDeg = 0.0;

        res.WyTop = res.Iy / (h / 2.0);
        res.WyBot = res.WyTop;
        res.WzLeft = res.Iz / (b / 2.0);
        res.WzRight = res.WzLeft;

        var (wply, wplz) = PlasticModulusEngine.CalculateHBeam(b, h, tw, tf);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateHBeam(b, h, tw, tf);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaZ = hw * tw;
        res.ShearAreaY = 2.0 * b * tf;

        double y1 = (b - tw) / 2.0;
        double y2 = y1 + tw;

        res.OuterContour =
        [
            new(0, 0),
            new(b, 0),
            new(b, tf),
            new(y2, tf),
            new(y2, h - tf),
            new(b, h - tf),
            new(b, h),
            new(0, h),
            new(0, h - tf),
            new(y1, h - tf),
            new(y1, tf),
            new(0, tf)
        ];
    }
}

/// <summary>
/// 槽钢 (Channel) 截面力学计算策略
/// </summary>
public class ChannelSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Channel;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h / 2.0 - 0.1);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);

        List<Point2D> pts =
        [
            new(0, 0),
            new(b, 0),
            new(b, tf),
            new(tw, tf),
            new(tw, h - tf),
            new(b, h - tf),
            new(b, h),
            new(0, h)
        ];

        var polyRes = GreenTheoremPolygonEngine.CalculateProperties(pts);
        res.Area = polyRes.Area;
        res.OuterPerimeter = polyRes.Perimeter;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = b;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = polyRes.Yc;
        res.Zc = polyRes.Zc;

        res.Iy = polyRes.Iy;
        res.Iz = polyRes.Iz;
        res.Iyz = polyRes.Iyz;
        res.I1 = polyRes.I1;
        res.I2 = polyRes.I2;
        res.PrincipalAngleDeg = polyRes.AlphaDeg;

        res.WyTop = res.Iy / Math.Max(1e-4, h - res.Zc);
        res.WyBot = res.Iy / Math.Max(1e-4, res.Zc);
        res.WzLeft = res.Iz / Math.Max(1e-4, res.Yc);
        res.WzRight = res.Iz / Math.Max(1e-4, b - res.Yc);

        var (wply, wplz) = PlasticModulusEngine.CalculateBySlicing(pts);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateChannel(b, h, tw, tf);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaZ = (h - 2.0 * tf) * tw;
        res.ShearAreaY = 2.0 * b * tf;

        res.OuterContour = pts;
    }
}

/// <summary>
/// 角钢 (Angle) 截面力学计算策略
/// </summary>
public class AngleSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Angle;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b1 = Math.Max(1.0, p.LegWidth1);
        double b2 = Math.Max(1.0, p.LegWidth2);
        double t = Math.Clamp(p.LegThickness, 0.1, Math.Min(b1, b2) - 0.1);

        List<Point2D> pts =
        [
            new(0, 0),
            new(b2, 0),
            new(b2, t),
            new(t, t),
            new(t, b1),
            new(0, b1)
        ];

        var polyRes = GreenTheoremPolygonEngine.CalculateProperties(pts);
        res.Area = polyRes.Area;
        res.OuterPerimeter = polyRes.Perimeter;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = t;
        res.BottomSurfaceWidth = b2;
        res.Ymin = 0; res.Ymax = b2;
        res.Zmin = 0; res.Zmax = b1;
        res.Yc = polyRes.Yc;
        res.Zc = polyRes.Zc;

        res.Iy = polyRes.Iy;
        res.Iz = polyRes.Iz;
        res.Iyz = polyRes.Iyz;
        res.I1 = polyRes.I1;
        res.I2 = polyRes.I2;
        res.PrincipalAngleDeg = polyRes.AlphaDeg;

        res.WyTop = res.Iy / Math.Max(1e-4, b1 - res.Zc);
        res.WyBot = res.Iy / Math.Max(1e-4, res.Zc);
        res.WzLeft = res.Iz / Math.Max(1e-4, res.Yc);
        res.WzRight = res.Iz / Math.Max(1e-4, b2 - res.Yc);

        var (wply, wplz) = PlasticModulusEngine.CalculateBySlicing(pts);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateAngle(b1, b2, t);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = b2 * t;
        res.ShearAreaZ = b1 * t;

        res.OuterContour = pts;
    }
}

/// <summary>
/// T型钢 (TSection) 截面力学计算策略
/// </summary>
public class TSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.TSection;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h - 0.1);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);

        double y1 = (b - tw) / 2.0;
        double y2 = y1 + tw;

        List<Point2D> pts =
        [
            new(y1, 0),
            new(y2, 0),
            new(y2, h - tf),
            new(b, h - tf),
            new(b, h),
            new(0, h),
            new(0, h - tf),
            new(y1, h - tf)
        ];

        var polyRes = GreenTheoremPolygonEngine.CalculateProperties(pts);
        res.Area = polyRes.Area;
        res.OuterPerimeter = polyRes.Perimeter;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = b;
        res.BottomSurfaceWidth = tw;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = polyRes.Yc;
        res.Zc = polyRes.Zc;

        res.Iy = polyRes.Iy;
        res.Iz = polyRes.Iz;
        res.Iyz = polyRes.Iyz;
        res.I1 = polyRes.I1;
        res.I2 = polyRes.I2;
        res.PrincipalAngleDeg = polyRes.AlphaDeg;

        res.WyTop = res.Iy / Math.Max(1e-4, h - res.Zc);
        res.WyBot = res.Iy / Math.Max(1e-4, res.Zc);
        res.WzLeft = res.Iz / Math.Max(1e-4, res.Yc);
        res.WzRight = res.Iz / Math.Max(1e-4, b - res.Yc);

        var (wply, wplz) = PlasticModulusEngine.CalculateBySlicing(pts);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateTSection(b, h, tw, tf);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = b * tf;
        res.ShearAreaZ = (h - tf) * tw;

        res.OuterContour = pts;
    }
}

/// <summary>
/// 十字形 (Cruciform) 截面力学计算策略
/// </summary>
public class CruciformSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Cruciform;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double h = Math.Max(1.0, p.Height);
        double b = Math.Max(1.0, p.Width);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h - 0.1);

        double y1 = (b - tw) / 2.0;
        double y2 = y1 + tw;
        double z1 = (h - tf) / 2.0;
        double z2 = z1 + tf;

        List<Point2D> pts =
        [
            new(y1, 0),
            new(y2, 0),
            new(y2, z1),
            new(b, z1),
            new(b, z2),
            new(y2, z2),
            new(y2, h),
            new(y1, h),
            new(y1, z2),
            new(0, z2),
            new(0, z1),
            new(y1, z1)
        ];

        var polyRes = GreenTheoremPolygonEngine.CalculateProperties(pts);
        res.Area = polyRes.Area;
        res.OuterPerimeter = polyRes.Perimeter;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.TopSurfaceWidth = tw;
        res.BottomSurfaceWidth = tw;
        res.Ymin = 0; res.Ymax = b;
        res.Zmin = 0; res.Zmax = h;
        res.Yc = polyRes.Yc;
        res.Zc = polyRes.Zc;

        res.Iy = polyRes.Iy;
        res.Iz = polyRes.Iz;
        res.Iyz = polyRes.Iyz;
        res.I1 = polyRes.I1;
        res.I2 = polyRes.I2;
        res.PrincipalAngleDeg = polyRes.AlphaDeg;

        res.WyTop = res.Iy / (h / 2.0);
        res.WyBot = res.WyTop;
        res.WzLeft = res.Iz / (b / 2.0);
        res.WzRight = res.WzLeft;

        var (wply, wplz) = PlasticModulusEngine.CalculateBySlicing(pts);
        res.Wply = wply;
        res.Wplz = wplz;

        var (j, iw) = TorsionWarpingEngine.CalculateCruciform(h, b, tw, tf);
        res.TorsionConstantJ = j;
        res.WarpingConstantIw = iw;

        res.ShearAreaY = (b - tw) * tf;
        res.ShearAreaZ = h * tw;

        res.OuterContour = pts;
    }
}

/// <summary>
/// 任意多边形与带孔洞多边形截面力学计算策略
/// </summary>
public class PolygonSectionStrategy : ISectionGeometryStrategy
{
    public SectionType SupportedType => SectionType.Polygon;

    public void Calculate(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        var outer = p.PolygonOuterLoop ?? [];
        if (outer.Count < 3)
        {
            new RectangleSectionStrategy().Calculate(p, res, mat);
            return;
        }

        var holes = p.PolygonInnerHoles;
        var polyRes = GreenTheoremPolygonEngine.CalculateProperties(outer, holes);

        res.Area = polyRes.Area;
        res.OuterPerimeter = polyRes.Perimeter;
        res.InnerPerimeter = 0;
        res.Perimeter = res.OuterPerimeter;
        res.Ymin = outer.Min(pt => pt.Y);
        res.Ymax = outer.Max(pt => pt.Y);
        res.Zmin = outer.Min(pt => pt.Z);
        res.Zmax = outer.Max(pt => pt.Z);
        res.TopSurfaceWidth = res.TotalWidth;
        res.BottomSurfaceWidth = res.TotalWidth;
        res.Yc = polyRes.Yc;
        res.Zc = polyRes.Zc;

        res.Iy = polyRes.Iy;
        res.Iz = polyRes.Iz;
        res.Iyz = polyRes.Iyz;
        res.I1 = polyRes.I1;
        res.I2 = polyRes.I2;
        res.PrincipalAngleDeg = polyRes.AlphaDeg;

        res.WyTop = res.Iy / Math.Max(1e-4, res.Zmax - res.Zc);
        res.WyBot = res.Iy / Math.Max(1e-4, res.Zc - res.Zmin);
        res.WzLeft = res.Iz / Math.Max(1e-4, res.Yc - res.Ymin);
        res.WzRight = res.Iz / Math.Max(1e-4, res.Ymax - res.Yc);

        var (wply, wplz) = PlasticModulusEngine.CalculateBySlicing(outer, holes);
        res.Wply = wply;
        res.Wplz = wplz;

        res.TorsionConstantJ = (res.Area * res.Area * res.Area) / (40.0 * (res.Iy + res.Iz));
        res.WarpingConstantIw = 0;

        res.ShearAreaY = 0.8 * res.Area;
        res.ShearAreaZ = 0.8 * res.Area;

        res.OuterContour = outer;
        res.InnerContours = holes ?? [];
    }
}

/// <summary>
/// 组合截面复合策略 (Composite Pattern)
/// 将两个或多个独立子构件的几何力学参数，依据平行轴移轴定理进行代数积分合成
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
        double sumSy = 0;
        double sumSz = 0;

        var subResults = new List<(SectionPropertiesResult SubRes, double OffY, double OffZ)>();

        foreach (var (strat, p, offY, offZ) in _components)
        {
            var sub = new SectionPropertiesResult();
            strat.Calculate(p, sub, mat);
            subResults.Add((sub, offY, offZ));

            totalArea += sub.Area;
            sumSz += sub.Area * (sub.Yc + offY);
            sumSy += sub.Area * (sub.Zc + offZ);
        }

        res.Area = totalArea;
        if (totalArea <= 0) return;

        res.Yc = sumSz / totalArea;
        res.Zc = sumSy / totalArea;

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
