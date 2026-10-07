namespace EngineeringApp.Shared.Models;

/// <summary>
/// 截面受力应力计算与中和轴分析结果
/// </summary>
public class StressAnalysisResult
{
    /// <summary>截面最大正应力 σ_max (MPa, 正值为拉应力)</summary>
    public double MaxTensileStress { get; set; }

    /// <summary>最大拉应力发生点坐标 (Y, Z, 相对形心)</summary>
    public Point2D MaxTensilePoint { get; set; } = Point2D.Zero;

    /// <summary>截面最小正应力 σ_min (MPa, 负值为压应力)</summary>
    public double MinCompressiveStress { get; set; }

    /// <summary>最大压应力发生点坐标 (Y, Z, 相对形心)</summary>
    public Point2D MinCompressivePoint { get; set; } = Point2D.Zero;

    /// <summary>截面最大绝对正应力 |σ|_extreme (MPa)</summary>
    public double MaxAbsoluteStress => Math.Max(Math.Abs(MaxTensileStress), Math.Abs(MinCompressiveStress));

    /// <summary>截面强度应力比 / 利用率 η = |σ|_extreme / fy</summary>
    public double StressRatio { get; set; }

    /// <summary>中和轴直线方程系数: A * y' + B * z' + C = 0 (y', z' 为相对形心坐标)</summary>
    public double NeutralAxisA { get; set; }
    public double NeutralAxisB { get; set; }
    public double NeutralAxisC { get; set; }

    /// <summary>截面边界内中和轴线段两个交点 (相对外轮廓左下角)</summary>
    public Point2D? NeutralAxisPoint1 { get; set; }
    public Point2D? NeutralAxisPoint2 { get; set; }

    /// <summary>是否全截面受拉</summary>
    public bool IsAllTension => MinCompressiveStress >= 0;

    /// <summary>是否全截面受压</summary>
    public bool IsAllCompression => MaxTensileStress <= 0;

    /// <summary>网格采样点及各点正应力值 (用于云图着色渲染)</summary>
    public List<StressPoint> StressSamplePoints { get; set; } = [];
}

/// <summary>
/// 采样应力点
/// </summary>
public record StressPoint(double Y, double Z, double Stress);
