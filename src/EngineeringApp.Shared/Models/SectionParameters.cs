namespace EngineeringApp.Shared.Models;

/// <summary>
/// 截面几何尺寸输入参数集合
/// </summary>
public class SectionParameters
{
    public SectionType Type { get; set; } = SectionType.HBeam;

    /// <summary>截面高度 / 外高 h (mm)</summary>
    public double Height { get; set; } = 300.0;

    /// <summary>截面宽度 / 外宽 b (mm)</summary>
    public double Width { get; set; } = 300.0;

    /// <summary>腹板厚度 tw (mm)</summary>
    public double WebThickness { get; set; } = 10.0;

    /// <summary>翼缘厚度 tf (mm)</summary>
    public double FlangeThickness { get; set; } = 15.0;

    /// <summary>圆形截面外径 D (mm)</summary>
    public double OuterDiameter { get; set; } = 219.0;

    /// <summary>管壁厚度 t (mm)</summary>
    public double WallThickness { get; set; } = 8.0;

    /// <summary>不等边角钢长肢宽 b1 (mm)</summary>
    public double LegWidth1 { get; set; } = 125.0;

    /// <summary>不等边角钢短肢宽 b2 (mm)</summary>
    public double LegWidth2 { get; set; } = 80.0;

    /// <summary>角钢/板厚度 t (mm)</summary>
    public double LegThickness { get; set; } = 10.0;

    /// <summary>根部圆弧半径 r (mm)</summary>
    public double RootRadius { get; set; } = 12.0;

    /// <summary>端部圆弧半径 r1 (mm)</summary>
    public double ToeRadius { get; set; } = 6.0;

    /// <summary>十字形截面腹板翼缘宽 b_cross (mm)</summary>
    public double CrossFlangeWidth { get; set; } = 200.0;

    /// <summary>十字形截面板厚 t_cross (mm)</summary>
    public double CrossThickness { get; set; } = 12.0;

    /// <summary>任意多边形外边界顶点序列 (逆时针排列)</summary>
    public List<Point2D> PolygonOuterLoop { get; set; } =
    [
        new(0, 0),
        new(300, 0),
        new(300, 50),
        new(50, 50),
        new(50, 200),
        new(0, 200)
    ];

    /// <summary>任意多边形内部孔洞顶点序列列表 (顺时针排列)</summary>
    public List<List<Point2D>> PolygonInnerHoles { get; set; } = [];

    /// <summary>选中的国标型钢代号 (如 "HW 300x300x10x15")</summary>
    public string? StandardProfileName { get; set; }

    /// <summary>材料配置</summary>
    public MaterialProfile Material { get; set; } = new();
}
