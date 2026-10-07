using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 基于格林公式数值线积分的任意多边形截面力学特性求解引擎
/// 支持任意复杂多边形外边界与多个内环孔洞
/// </summary>
public class GreenTheoremPolygonEngine
{
    public static (double Area, double Yc, double Zc, double Iy, double Iz, double Iyz, double I1, double I2, double AlphaDeg, double Perimeter)
        CalculateProperties(List<Point2D> outerLoop, List<List<Point2D>>? innerHoles = null)
    {
        if (outerLoop.Count < 3)
            return (0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        // 规范化外环：确保逆时针排列 (面积为正)
        var orientedOuter = EnsureOrientation(outerLoop, counterClockwise: true);

        double totalArea = 0;
        double totalSz = 0; // ∫ y dA
        double totalSy = 0; // ∫ z dA
        double totalIy0 = 0; // ∫ z² dA
        double totalIz0 = 0; // ∫ y² dA
        double totalIyz0 = 0; // ∫ y z dA
        double perimeter = CalculateLoopPerimeter(orientedOuter);

        // 积分外环
        IntegrateLoop(orientedOuter, ref totalArea, ref totalSz, ref totalSy, ref totalIy0, ref totalIz0, ref totalIyz0);

        // 积分内孔 (内孔必须顺时针，贡献为负面积)
        if (innerHoles != null)
        {
            foreach (var hole in innerHoles)
            {
                if (hole.Count < 3) continue;
                var orientedHole = EnsureOrientation(hole, counterClockwise: false);
                perimeter += CalculateLoopPerimeter(orientedHole);
                IntegrateLoop(orientedHole, ref totalArea, ref totalSz, ref totalSy, ref totalIy0, ref totalIz0, ref totalIyz0);
            }
        }

        if (totalArea <= 1e-9)
            return (0, 0, 0, 0, 0, 0, 0, 0, 0, perimeter);

        // 形心坐标 (相对原点)
        double yc = totalSz / totalArea;
        double zc = totalSy / totalArea;

        // 平行移轴定理求解形心惯性矩
        double iy = totalIy0 - totalArea * zc * zc;
        double iz = totalIz0 - totalArea * yc * yc;
        double iyz = totalIyz0 - totalArea * yc * zc;

        // 规避数值精度微小负值
        iy = Math.Max(0, iy);
        iz = Math.Max(0, iz);

        // 主惯性矩与主轴方位角
        double deltaI = (iy - iz) / 2.0;
        double r = Math.Sqrt(deltaI * deltaI + iyz * iyz);
        double i1 = (iy + iz) / 2.0 + r;
        double i2 = (iy + iz) / 2.0 - r;
        i2 = Math.Max(0, i2);

        // 主轴偏角 (从水平Y轴逆时针起算，范围 -90° ~ +90°)
        double alphaRad = 0.5 * Math.Atan2(-2.0 * iyz, iy - iz);
        double alphaDeg = alphaRad * 180.0 / Math.PI;

        return (totalArea, yc, zc, iy, iz, iyz, i1, i2, alphaDeg, perimeter);
    }

    private static void IntegrateLoop(
        List<Point2D> pts,
        ref double area,
        ref double sz,
        ref double sy,
        ref double iy0,
        ref double iz0,
        ref double iyz0)
    {
        int n = pts.Count;
        for (int i = 0; i < n; i++)
        {
            Point2D p1 = pts[i];
            Point2D p2 = pts[(i + 1) % n];

            double c = p1.Y * p2.Z - p2.Y * p1.Z; // 交叉积

            area += 0.5 * c;
            sz += (1.0 / 6.0) * c * (p1.Y + p2.Y);
            sy += (1.0 / 6.0) * c * (p1.Z + p2.Z);
            iy0 += (1.0 / 12.0) * c * (p1.Z * p1.Z + p1.Z * p2.Z + p2.Z * p2.Z);
            iz0 += (1.0 / 12.0) * c * (p1.Y * p1.Y + p1.Y * p2.Y + p2.Y * p2.Y);
            iyz0 += (1.0 / 24.0) * c * (p1.Y * p2.Z + 2.0 * p1.Y * p1.Z + 2.0 * p2.Y * p2.Z + p2.Y * p1.Z);
        }
    }

    public static List<Point2D> EnsureOrientation(List<Point2D> pts, bool counterClockwise)
    {
        double signedArea = 0;
        int n = pts.Count;
        for (int i = 0; i < n; i++)
        {
            Point2D p1 = pts[i];
            Point2D p2 = pts[(i + 1) % n];
            signedArea += 0.5 * (p1.Y * p2.Z - p2.Y * p1.Z);
        }

        bool isCurrentlyCCW = signedArea > 0;
        if (isCurrentlyCCW == counterClockwise)
            return new List<Point2D>(pts);

        var reversed = new List<Point2D>(pts);
        reversed.Reverse();
        return reversed;
    }

    private static double CalculateLoopPerimeter(List<Point2D> pts)
    {
        double perim = 0;
        int n = pts.Count;
        for (int i = 0; i < n; i++)
        {
            perim += pts[i].DistanceTo(pts[(i + 1) % n]);
        }
        return perim;
    }
}
