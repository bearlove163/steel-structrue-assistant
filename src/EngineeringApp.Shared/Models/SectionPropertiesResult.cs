namespace EngineeringApp.Shared.Models;

/// <summary>
/// 截面几何与力学特性完整计算结果
/// </summary>
public class SectionPropertiesResult
{
    /// <summary>截面面积 A (mm²)</summary>
    public double Area { get; set; }

    /// <summary>截面面积 A (cm²)</summary>
    public double AreaCm2 => Area / 100.0;

    /// <summary>截面外表面轮廓周长 P_outer (mm)</summary>
    public double OuterPerimeter { get; set; }

    /// <summary>截面内孔洞/内部空腔周长 P_inner (mm)</summary>
    public double InnerPerimeter { get; set; }

    /// <summary>截面全周长 P (mm，默认等于外表面周长与内表面周长之和)</summary>
    public double Perimeter
    {
        get => OuterPerimeter > 0 ? (OuterPerimeter + InnerPerimeter) : _perimeter;
        set => _perimeter = value;
    }
    private double _perimeter;

    /// <summary>截面上表面暴露宽度 (mm，例如 H型钢上翼缘宽 b，箱型梁顶宽 b，用于扣除楼板贴合免涂装面积)</summary>
    public double TopSurfaceWidth { get; set; }

    /// <summary>截面下表面暴露宽度 (mm，例如底翼缘或底板宽度)</summary>
    public double BottomSurfaceWidth { get; set; }

    /// <summary>外表面延米全涂装表面积 (m²/m)</summary>
    public double GrossPaintingAreaPerMeter => (OuterPerimeter > 0 ? OuterPerimeter : Perimeter) / 1000.0;

    /// <summary>扣除上表面（例如与混凝土楼板/压型钢板贴合）后的延米净涂装表面积 (m²/m)</summary>
    public double PaintingAreaExcludingTop => Math.Max(0, ((OuterPerimeter > 0 ? OuterPerimeter : Perimeter) - TopSurfaceWidth) / 1000.0);

    /// <summary>延米理论涂装表面积 (m²/m，默认按外表面计算)</summary>
    public double PaintingAreaPerMeter => GrossPaintingAreaPerMeter;

    /// <summary>延米理论线密度 / 米重 (kg/m)</summary>
    public double LinearMass { get; set; }

    /// <summary>吨钢比表面积 / 展开度 (m²/t，延米外表面积 / 延米吨重，商务防腐/防火涂料报价核心指标)</summary>
    public double AreaPerTon => LinearMass > 1e-4 ? (GrossPaintingAreaPerMeter / (LinearMass / 1000.0)) : 0;

    /// <summary>
    /// 根据涂装计算选项（如扣除楼板、扣除底面、包含内孔等）计算延米净涂装面积 (m²/m)
    /// </summary>
    public double CalculatePaintingArea(PaintingCalculationOptions? options = null)
    {
        if (options == null) return GrossPaintingAreaPerMeter;

        double basePerimeter = OuterPerimeter > 0 ? OuterPerimeter : Perimeter;
        if (options.IncludeInnerSurface)
        {
            basePerimeter += InnerPerimeter;
        }

        if (options.ExcludeTopSurface)
        {
            double deduct = options.TopSurfaceDeductionWidth ?? TopSurfaceWidth;
            basePerimeter -= deduct;
        }

        if (options.ExcludeBottomSurface)
        {
            double deduct = options.BottomSurfaceDeductionWidth ?? BottomSurfaceWidth;
            basePerimeter -= deduct;
        }

        double netArea = Math.Max(0, basePerimeter) / 1000.0;
        return netArea * options.LossRatio;
    }

    /// <summary>形心横坐标 Yc (mm, 相对外轮廓包络矩形左下角)</summary>
    public double Yc { get; set; }

    /// <summary>形心竖坐标 Zc (mm, 相对外轮廓包络矩形左下角)</summary>
    public double Zc { get; set; }

    /// <summary>形心水平主轴惯性矩 Iy (mm⁴)</summary>
    public double Iy { get; set; }

    /// <summary>形心水平主轴惯性矩 Iy (cm⁴)</summary>
    public double IyCm4 => Iy / 10000.0;

    /// <summary>形心竖向主轴惯性矩 Iz (mm⁴)</summary>
    public double Iz { get; set; }

    /// <summary>形心竖向主轴惯性矩 Iz (cm⁴)</summary>
    public double IzCm4 => Iz / 10000.0;

    /// <summary>形心惯性积 Iyz (mm⁴)</summary>
    public double Iyz { get; set; }

    /// <summary>第一主惯性矩 I1 (mm⁴)</summary>
    public double I1 { get; set; }

    /// <summary>第一主惯性矩 I1 (cm⁴)</summary>
    public double I1Cm4 => I1 / 10000.0;

    /// <summary>第二主惯性矩 I2 (mm⁴)</summary>
    public double I2 { get; set; }

    /// <summary>第二主惯性矩 I2 (cm⁴)</summary>
    public double I2Cm4 => I2 / 10000.0;

    /// <summary>主轴方位角 αp (度, 逆时针从水平Y轴起算)</summary>
    public double PrincipalAngleDeg { get; set; }

    /// <summary>水平轴回转半径 / 惯性半径 iy (mm)</summary>
    public double IyRadius => Area > 1e-9 ? Math.Sqrt(Math.Max(0, Iy / Area)) : 0;

    /// <summary>竖向轴回转半径 / 惯性半径 iz (mm)</summary>
    public double IzRadius => Area > 1e-9 ? Math.Sqrt(Math.Max(0, Iz / Area)) : 0;

    /// <summary>第一主轴回转半径 i1 (mm)</summary>
    public double I1Radius => Area > 1e-9 ? Math.Sqrt(Math.Max(0, I1 / Area)) : 0;

    /// <summary>第二主轴回转半径 i2 (mm)</summary>
    public double I2Radius => Area > 1e-9 ? Math.Sqrt(Math.Max(0, I2 / Area)) : 0;

    /// <summary>对上边缘的弹性截面模量 Wy_top (mm³)</summary>
    public double WyTop { get; set; }

    /// <summary>对下边缘的弹性截面模量 Wy_bot (mm³)</summary>
    public double WyBot { get; set; }

    /// <summary>Wy 较小值 Wy_min (cm³)</summary>
    public double WyMinCm3 => Math.Min(WyTop, WyBot) / 1000.0;

    /// <summary>对左边缘的弹性截面模量 Wz_left (mm³)</summary>
    public double WzLeft { get; set; }

    /// <summary>对右边缘的弹性截面模量 Wz_right (mm³)</summary>
    public double WzRight { get; set; }

    /// <summary>Wz 较小值 Wz_min (cm³)</summary>
    public double WzMinCm3 => Math.Min(WzLeft, WzRight) / 1000.0;

    /// <summary>水平轴塑性截面模量 Wpl_y (mm³)</summary>
    public double Wply { get; set; }

    /// <summary>水平轴塑性截面模量 Wpl_y (cm³)</summary>
    public double WplyCm3 => Wply / 1000.0;

    /// <summary>竖向轴塑性截面模量 Wpl_z (mm³)</summary>
    public double Wplz { get; set; }

    /// <summary>竖向轴塑性截面模量 Wpl_z (cm³)</summary>
    public double WplzCm3 => Wplz / 1000.0;

    /// <summary>水平塑性截面形状系数 γy = Wpl_y / Wy_min</summary>
    public double GammaY => WyTop > 0 && WyBot > 0 ? Wply / Math.Min(WyTop, WyBot) : 1.0;

    /// <summary>竖向塑性截面形状系数 γz = Wpl_z / Wz_min</summary>
    public double GammaZ => WzLeft > 0 && WzRight > 0 ? Wplz / Math.Min(WzLeft, WzRight) : 1.0;

    /// <summary>圣维南自由扭转常数 J / It (mm⁴)</summary>
    public double TorsionConstantJ { get; set; }

    /// <summary>圣维南扭转常数 J (cm⁴)</summary>
    public double TorsionConstantJCm4 => TorsionConstantJ / 10000.0;

    /// <summary>扇性惯性矩 / 翘曲常数 Iw / Cw (mm⁶)</summary>
    public double WarpingConstantIw { get; set; }

    /// <summary>翘曲常数 Iw (cm⁶)</summary>
    public double WarpingConstantIwCm6 => WarpingConstantIw / 1e6;

    /// <summary>水平抗剪有效截面面积 Asy (mm²)</summary>
    public double ShearAreaY { get; set; }

    /// <summary>竖向抗剪有效截面面积 Asz (mm²)</summary>
    public double ShearAreaZ { get; set; }

    /// <summary>截面几何包络尺寸: [Ymin, Zmin, Ymax, Zmax]</summary>
    public double Ymin { get; set; }
    public double Zmin { get; set; }
    public double Ymax { get; set; }
    public double Zmax { get; set; }

    public double TotalWidth => Math.Max(0, Ymax - Ymin);
    public double TotalHeight => Math.Max(0, Zmax - Zmin);

    /// <summary>SVG 矢量多边形外边界路径</summary>
    public List<Point2D> OuterContour { get; set; } = [];

    /// <summary>SVG 矢量多边形内孔洞路径列表</summary>
    public List<List<Point2D>> InnerContours { get; set; } = [];

    /// <summary>如果是圆或圆管，提供圆形参数用于SVG高平滑渲染</summary>
    public bool IsCircular { get; set; }
    public double CircleOuterRadius { get; set; }
    public double CircleInnerRadius { get; set; }

    /// <summary>应力极值分析结果 (若已输入荷载)</summary>
    public StressAnalysisResult? StressResult { get; set; }

    /// <summary>构件面外计算长度与长细比计算结果</summary>
    public MemberSlendernessResult? SlendernessResult { get; set; }
}

