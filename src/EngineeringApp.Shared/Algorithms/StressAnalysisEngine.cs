using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面受力应力状态与中和轴求解引擎
/// </summary>
public static class StressAnalysisEngine
{
    /// <summary>
    /// 对给定截面计算内力工况下的正应力极值、分布及中和轴
    /// </summary>
    public static StressAnalysisResult Analyze(
        SectionPropertiesResult prop,
        LoadCase load,
        MaterialProfile material)
    {
        var result = new StressAnalysisResult();

        if (prop.Area <= 1e-9)
            return result;

        // 单位转换:
        // N: kN -> N (×1000)
        // My, Mz: kN·m -> N·mm (×1,000,000)
        double n = load.AxialForceN * 1000.0;
        double my = load.BendingMomentMy * 1000000.0;
        double mz = load.BendingMomentMz * 1000000.0;

        double a = prop.Area;
        double iy = prop.Iy;
        double iz = prop.Iz;
        double iyz = prop.Iyz;

        // 广义双向弯曲应力公式分母: Iy * Iz - Iyz²
        double denom = iy * iz - iyz * iyz;
        if (denom <= 1e-6)
            denom = Math.Max(1e-6, iy * iz);

        // σ(y', z') = C0 + Cz * z' + Cy * y'
        // C0: 轴力引起均匀应力 (N/A)
        // 正 My: 上边缘 (z' > 0) 受压 (-), 下边缘受拉 (+) => Cz = -(My * Iz - Mz * Iyz) / denom
        // 正 Mz: 右边缘 (y' > 0) 受压 (-), 左边缘受拉 (+) => Cy = -(Mz * Iy - My * Iyz) / denom
        double c0 = n / a;
        double cz = -(my * iz - mz * iyz) / denom;
        double cy = -(mz * iy - my * iyz) / denom;

        result.NeutralAxisA = cy;
        result.NeutralAxisB = cz;
        result.NeutralAxisC = c0;

        // 收集待计算应力的截面边界关键特征点 (外轮廓顶点 + 极值点 + 内孔洞顶点)
        List<Point2D> criticalPoints = [];
        criticalPoints.AddRange(prop.OuterContour);
        foreach (var hole in prop.InnerContours)
        {
            criticalPoints.AddRange(hole);
        }

        // 添加包络矩形四角点测试
        criticalPoints.Add(new Point2D(prop.Ymin, prop.Zmin));
        criticalPoints.Add(new Point2D(prop.Ymax, prop.Zmin));
        criticalPoints.Add(new Point2D(prop.Ymax, prop.Zmax));
        criticalPoints.Add(new Point2D(prop.Ymin, prop.Zmax));

        double maxSigma = double.MinValue;
        double minSigma = double.MaxValue;
        Point2D maxPt = Point2D.Zero;
        Point2D minPt = Point2D.Zero;

        foreach (var pt in criticalPoints)
        {
            // 相对形心坐标
            double yPrime = pt.Y - prop.Yc;
            double zPrime = pt.Z - prop.Zc;

            double sigma = c0 + cz * zPrime + cy * yPrime;

            if (sigma > maxSigma)
            {
                maxSigma = sigma;
                maxPt = pt;
            }
            if (sigma < minSigma)
            {
                minSigma = sigma;
                minPt = pt;
            }
        }

        result.MaxTensileStress = maxSigma;
        result.MaxTensilePoint = maxPt;
        result.MinCompressiveStress = minSigma;
        result.MinCompressivePoint = minPt;

        double extremeAbs = Math.Max(Math.Abs(maxSigma), Math.Abs(minSigma));
        result.StressRatio = material.YieldStrength > 0 ? extremeAbs / material.YieldStrength : 0;

        // 求解中和轴在截面外包络范围内的线段端点:
        // C0 + Cz * (z - Zc) + Cy * (y - Yc) = 0
        ComputeNeutralAxisSegment(result, prop, c0, cy, cz);

        // 生成采样应力云图点集
        GenerateSampleGrid(result, prop, c0, cy, cz);

        return result;
    }

    private static void ComputeNeutralAxisSegment(
        StressAnalysisResult result,
        SectionPropertiesResult prop,
        double c0,
        double cy,
        double cz)
    {
        // 若只有纯轴力 (Cz ≈ 0 && Cy ≈ 0)，中和轴在无穷远处
        if (Math.Abs(cy) < 1e-12 && Math.Abs(cz) < 1e-12)
        {
            result.NeutralAxisPoint1 = null;
            result.NeutralAxisPoint2 = null;
            return;
        }

        double ymin = prop.Ymin;
        double ymax = prop.Ymax;
        double zmin = prop.Zmin;
        double zmax = prop.Zmax;

        // 方程: cy * (y - Yc) + cz * (z - Zc) + c0 = 0
        // 即: cy * y + cz * z + (c0 - cy * Yc - cz * Zc) = 0
        double constTerm = c0 - cy * prop.Yc - cz * prop.Zc;

        List<Point2D> intersects = [];

        // 与 y = ymin 的交点
        if (Math.Abs(cz) > 1e-12)
        {
            double z = -(constTerm + cy * ymin) / cz;
            if (z >= zmin - 1e-4 && z <= zmax + 1e-4) intersects.Add(new Point2D(ymin, z));
        }

        // 与 y = ymax 的交点
        if (Math.Abs(cz) > 1e-12)
        {
            double z = -(constTerm + cy * ymax) / cz;
            if (z >= zmin - 1e-4 && z <= zmax + 1e-4) intersects.Add(new Point2D(ymax, z));
        }

        // 与 z = zmin 的交点
        if (Math.Abs(cy) > 1e-12)
        {
            double y = -(constTerm + cz * zmin) / cy;
            if (y >= ymin - 1e-4 && y <= ymax + 1e-4) intersects.Add(new Point2D(y, zmin));
        }

        // 与 z = zmax 的交点
        if (Math.Abs(cy) > 1e-12)
        {
            double y = -(constTerm + cz * zmax) / cy;
            if (y >= ymin - 1e-4 && y <= ymax + 1e-4) intersects.Add(new Point2D(y, zmax));
        }

        // 去重
        List<Point2D> unique = [];
        foreach (var p in intersects)
        {
            if (!unique.Any(u => u.DistanceTo(p) < 1.0))
            {
                unique.Add(p);
            }
        }

        if (unique.Count >= 2)
        {
            result.NeutralAxisPoint1 = unique[0];
            result.NeutralAxisPoint2 = unique[1];
        }
    }

    private static void GenerateSampleGrid(
        StressAnalysisResult result,
        SectionPropertiesResult prop,
        double c0,
        double cy,
        double cz)
    {
        int gridCountY = 24;
        int gridCountZ = 28;
        double dy = prop.TotalWidth / gridCountY;
        double dz = prop.TotalHeight / gridCountZ;

        if (dy <= 0 || dz <= 0) return;

        for (int i = 0; i <= gridCountY; i++)
        {
            double y = (i == gridCountY) ? prop.Ymax : (prop.Ymin + i * dy);
            for (int j = 0; j <= gridCountZ; j++)
            {
                double z = (j == gridCountZ) ? prop.Zmax : (prop.Zmin + j * dz);
                double yPrime = y - prop.Yc;
                double zPrime = z - prop.Zc;
                double sigma = c0 + cz * zPrime + cy * yPrime;
                result.StressSamplePoints.Add(new StressPoint(y, z, sigma));
            }
        }
    }
}
