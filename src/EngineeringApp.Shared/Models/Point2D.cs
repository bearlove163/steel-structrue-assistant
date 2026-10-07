namespace EngineeringApp.Shared.Models;

/// <summary>
/// 二维平面坐标点 (Y为截面横向水平轴，Z为截面竖向垂直轴)
/// </summary>
public record Point2D(double Y, double Z)
{
    public static Point2D Zero => new(0, 0);

    public double DistanceTo(Point2D other)
    {
        double dy = Y - other.Y;
        double dz = Z - other.Z;
        return Math.Sqrt(dy * dy + dz * dz);
    }
}
