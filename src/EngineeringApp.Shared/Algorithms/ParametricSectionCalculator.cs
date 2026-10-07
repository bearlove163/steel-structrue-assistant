using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 参数化截面与任意多边形力学特性综合计算器
/// </summary>
public class ParametricSectionCalculator : ISectionCalculator
{
    public SectionPropertiesResult Calculate(SectionParameters p)
    {
        var res = new SectionPropertiesResult();
        var mat = p.Material ?? new MaterialProfile();

        switch (p.Type)
        {
            case SectionType.Rectangle:
                CalculateRectangle(p, res, mat);
                break;
            case SectionType.Circle:
                CalculateCircle(p, res, mat);
                break;
            case SectionType.CHS:
                CalculateCHS(p, res, mat);
                break;
            case SectionType.RHS:
                CalculateRHS(p, res, mat);
                break;
            case SectionType.HBeam:
                CalculateHBeam(p, res, mat);
                break;
            case SectionType.Channel:
                CalculateChannel(p, res, mat);
                break;
            case SectionType.Angle:
                CalculateAngle(p, res, mat);
                break;
            case SectionType.TSection:
                CalculateTSection(p, res, mat);
                break;
            case SectionType.Cruciform:
                CalculateCruciform(p, res, mat);
                break;
            case SectionType.Polygon:
                CalculatePolygon(p, res, mat);
                break;
            default:
                CalculateRectangle(p, res, mat);
                break;
        }

        // 计算线密度: 质量 m (kg/m) = A (m²) * ρ (kg/m³) = (A / 1,000,000) * ρ
        res.LinearMass = (res.Area / 1e6) * mat.Density;

        return res;
    }

    private void CalculateRectangle(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
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

    private void CalculateCircle(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
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

        // 离散化用于通用分析
        res.OuterContour = GenerateCirclePolygon(r, r, r, 64);
    }

    private void CalculateCHS(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
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

        res.OuterContour = GenerateCirclePolygon(ro, ro, ro, 64);
        res.InnerContours = [GenerateCirclePolygon(ro, ro, ri, 64)];
    }

    private void CalculateRHS(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
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

    private void CalculateHBeam(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
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

        // Iy = 1/12 * [b * h³ - (b - tw) * hw³]
        res.Iy = (1.0 / 12.0) * (b * Math.Pow(h, 3) - (b - tw) * Math.Pow(hw, 3));
        // Iz = 2 * (1/12 * tf * b³) + 1/12 * hw * tw³
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

        // 12 个轮廓点绘制标准工字轮廓 (逆时针排列)
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

    private void CalculateChannel(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h / 2.0 - 0.1);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);

        // 槽钢几何轮廓 (腹板在左侧 x=0 ~ tw)
        // 8 个顶点逆时针排列:
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

    private void CalculateAngle(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b1 = Math.Max(1.0, p.LegWidth1); // 竖向长肢
        double b2 = Math.Max(1.0, p.LegWidth2); // 水平短肢
        double t = Math.Clamp(p.LegThickness, 0.1, Math.Min(b1, b2) - 0.1);

        // 6 个顶点 (L型截面):
        // (0,0) -> (b2, 0) -> (b2, t) -> (t, t) -> (t, b1) -> (0, b1)
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

    private void CalculateTSection(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double b = Math.Max(1.0, p.Width);
        double h = Math.Max(1.0, p.Height);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h - 0.1);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);

        double y1 = (b - tw) / 2.0;
        double y2 = y1 + tw;

        // 8 个顶点 T型截面 (翼缘在顶部，腹板向下延伸):
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

    private void CalculateCruciform(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        double h = Math.Max(1.0, p.Height);
        double b = Math.Max(1.0, p.Width);
        double tw = Math.Clamp(p.WebThickness, 0.1, b - 0.1);
        double tf = Math.Clamp(p.FlangeThickness, 0.1, h - 0.1);

        double y1 = (b - tw) / 2.0;
        double y2 = y1 + tw;
        double z1 = (h - tf) / 2.0;
        double z2 = z1 + tf;

        // 十字形 12 个顶点
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

    private void CalculatePolygon(SectionParameters p, SectionPropertiesResult res, MaterialProfile mat)
    {
        var outer = p.PolygonOuterLoop ?? [];
        if (outer.Count < 3)
        {
            CalculateRectangle(p, res, mat);
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

        // 任意多边形简化的扭转常数 (薄壁中面或截面积等效)
        res.TorsionConstantJ = (res.Area * res.Area * res.Area) / (40.0 * (res.Iy + res.Iz));
        res.WarpingConstantIw = 0;

        res.ShearAreaY = 0.8 * res.Area;
        res.ShearAreaZ = 0.8 * res.Area;

        res.OuterContour = outer;
        res.InnerContours = holes ?? [];
    }

    private static List<Point2D> GenerateCirclePolygon(double cx, double cy, double r, int segments)
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
