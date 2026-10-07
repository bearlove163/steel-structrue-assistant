using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 塑性截面模量 Wpl_y 与 Wpl_z 计算引擎
/// 支持解析解加速与高精度数值切片积分算法
/// </summary>
public static class PlasticModulusEngine
{
    /// <summary>
    /// 实心矩形塑性模量
    /// </summary>
    public static (double Wply, double Wplz) CalculateRectangle(double b, double h)
    {
        return (0.25 * b * h * h, 0.25 * h * b * b);
    }

    /// <summary>
    /// 实心圆塑性模量
    /// </summary>
    public static (double Wply, double Wplz) CalculateCircle(double d)
    {
        double r = d / 2.0;
        double wpl = (4.0 / 3.0) * r * r * r;
        return (wpl, wpl);
    }

    /// <summary>
    /// 空心圆管 (CHS) 塑性模量
    /// </summary>
    public static (double Wply, double Wplz) CalculateCHS(double d, double t)
    {
        double ro = d / 2.0;
        double ri = Math.Max(0, ro - t);
        double wpl = (4.0 / 3.0) * (ro * ro * ro - ri * ri * ri);
        return (wpl, wpl);
    }

    /// <summary>
    /// 空心方矩管 (RHS) 塑性模量
    /// </summary>
    public static (double Wply, double Wplz) CalculateRHS(double b, double h, double t)
    {
        double bi = Math.Max(0, b - 2.0 * t);
        double hi = Math.Max(0, h - 2.0 * t);
        double wply = 0.25 * (b * h * h - bi * hi * hi);
        double wplz = 0.25 * (h * b * b - hi * bi * bi);
        return (wply, wplz);
    }

    /// <summary>
    /// H型钢 / 双对称工字钢塑性模量
    /// </summary>
    public static (double Wply, double Wplz) CalculateHBeam(double b, double h, double tw, double tf)
    {
        double hw = Math.Max(0, h - 2.0 * tf);
        // Wpl_y = b * tf * (h - tf) + 0.25 * tw * hw²
        double wply = b * tf * (h - tf) + 0.25 * tw * hw * hw;
        // Wpl_z = 2 * (0.25 * tf * b²) + 0.25 * hw * tw²
        double wplz = 0.5 * tf * b * b + 0.25 * hw * tw * tw;
        return (wply, wplz);
    }

    /// <summary>
    /// 基于高精度数值切片的通用截面塑性截面模量计算 (适用于角钢、槽钢、任意多边形等)
    /// </summary>
    public static (double Wply, double Wplz) CalculateBySlicing(List<Point2D> outerLoop, List<List<Point2D>>? innerHoles = null, int numSlices = 500)
    {
        if (outerLoop.Count < 3) return (0, 0);

        // 获取包络
        double ymin = outerLoop.Min(p => p.Y);
        double ymax = outerLoop.Max(p => p.Y);
        double zmin = outerLoop.Min(p => p.Z);
        double zmax = outerLoop.Max(p => p.Z);

        double totalArea = 0;
        var orientedOuter = GreenTheoremPolygonEngine.EnsureOrientation(outerLoop, true);
        var res = GreenTheoremPolygonEngine.CalculateProperties(orientedOuter, innerHoles);
        totalArea = res.Area;
        if (totalArea <= 1e-9) return (0, 0);

        double halfArea = totalArea / 2.0;

        // 1. 求解水平塑性中和轴 Z_pna
        double dz = (zmax - zmin) / numSlices;
        double currentArea = 0;
        double zpna = zmin;
        for (int i = 0; i < numSlices; i++)
        {
            double zMid = zmin + (i + 0.5) * dz;
            double widthAtZ = GetCrossSectionWidthAtZ(zMid, outerLoop, innerHoles);
            double sliceArea = widthAtZ * dz;
            if (currentArea + sliceArea >= halfArea)
            {
                zpna = zmin + i * dz + (halfArea - currentArea) / Math.Max(1e-6, widthAtZ);
                break;
            }
            currentArea += sliceArea;
        }

        // 计算 Wpl_y = ∫ |z - zpna| dA
        double wply = 0;
        for (int i = 0; i < numSlices; i++)
        {
            double zMid = zmin + (i + 0.5) * dz;
            double widthAtZ = GetCrossSectionWidthAtZ(zMid, outerLoop, innerHoles);
            wply += Math.Abs(zMid - zpna) * widthAtZ * dz;
        }

        // 2. 求解竖向塑性中和轴 Y_pna
        double dy = (ymax - ymin) / numSlices;
        currentArea = 0;
        double ypna = ymin;
        for (int i = 0; i < numSlices; i++)
        {
            double yMid = ymin + (i + 0.5) * dy;
            double heightAtY = GetCrossSectionHeightAtY(yMid, outerLoop, innerHoles);
            double sliceArea = heightAtY * dy;
            if (currentArea + sliceArea >= halfArea)
            {
                ypna = ymin + i * dy + (halfArea - currentArea) / Math.Max(1e-6, heightAtY);
                break;
            }
            currentArea += sliceArea;
        }

        // 计算 Wpl_z = ∫ |y - ypna| dA
        double wplz = 0;
        for (int i = 0; i < numSlices; i++)
        {
            double yMid = ymin + (i + 0.5) * dy;
            double heightAtY = GetCrossSectionHeightAtY(yMid, outerLoop, innerHoles);
            wplz += Math.Abs(yMid - ypna) * heightAtY * dy;
        }

        return (wply, wplz);
    }

    private static double GetCrossSectionWidthAtZ(double z, List<Point2D> outerLoop, List<List<Point2D>>? innerHoles)
    {
        double totalWidth = GetPolygonHorizontalIntersectionLength(z, outerLoop);
        if (innerHoles != null)
        {
            foreach (var hole in innerHoles)
            {
                totalWidth -= GetPolygonHorizontalIntersectionLength(z, hole);
            }
        }
        return Math.Max(0, totalWidth);
    }

    private static double GetCrossSectionHeightAtY(double y, List<Point2D> outerLoop, List<List<Point2D>>? innerHoles)
    {
        double totalHeight = GetPolygonVerticalIntersectionLength(y, outerLoop);
        if (innerHoles != null)
        {
            foreach (var hole in innerHoles)
            {
                totalHeight -= GetPolygonVerticalIntersectionLength(y, hole);
            }
        }
        return Math.Max(0, totalHeight);
    }

    private static double GetPolygonHorizontalIntersectionLength(double z, List<Point2D> pts)
    {
        List<double> intersections = [];
        int n = pts.Count;
        for (int i = 0; i < n; i++)
        {
            var p1 = pts[i];
            var p2 = pts[(i + 1) % n];

            if ((p1.Z <= z && p2.Z > z) || (p2.Z <= z && p1.Z > z))
            {
                double t = (z - p1.Z) / (p2.Z - p1.Z);
                double y = p1.Y + t * (p2.Y - p1.Y);
                intersections.Add(y);
            }
        }

        intersections.Sort();
        double length = 0;
        for (int i = 0; i + 1 < intersections.Count; i += 2)
        {
            length += (intersections[i + 1] - intersections[i]);
        }
        return length;
    }

    private static double GetPolygonVerticalIntersectionLength(double y, List<Point2D> pts)
    {
        List<double> intersections = [];
        int n = pts.Count;
        for (int i = 0; i < n; i++)
        {
            var p1 = pts[i];
            var p2 = pts[(i + 1) % n];

            if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
            {
                double t = (y - p1.Y) / (p2.Y - p1.Y);
                double z = p1.Z + t * (p2.Z - p1.Z);
                intersections.Add(z);
            }
        }

        intersections.Sort();
        double length = 0;
        for (int i = 0; i + 1 < intersections.Count; i += 2)
        {
            length += (intersections[i + 1] - intersections[i]);
        }
        return length;
    }
}
