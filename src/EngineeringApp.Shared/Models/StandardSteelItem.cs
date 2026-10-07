namespace EngineeringApp.Shared.Models;

/// <summary>
/// 国标热轧型钢型号条目数据
/// </summary>
public class StandardSteelItem
{
    /// <summary>所属大类 (如 "GB/T 11263 热轧H型钢", "GB/T 706 工字钢", "GB/T 706 槽钢", "GB/T 706 等边角钢" 等)</summary>
    public string Category { get; set; } = "";

    /// <summary>型号代号 (如 "HW 300×300×10×15", "HN 400×200×8×13", "20a", "L 100×10")</summary>
    public string Designation { get; set; } = "";

    /// <summary>映射的参数化截面类型</summary>
    public SectionType SectionType { get; set; } = SectionType.HBeam;

    /// <summary>高度 h / 外径 D / 长肢宽 b1 (mm)</summary>
    public double Height { get; set; }

    /// <summary>宽度 b / 短肢宽 b2 (mm)</summary>
    public double Width { get; set; }

    /// <summary>腹板厚度 tw / 管壁厚度 t / 角钢厚度 t (mm)</summary>
    public double WebThickness { get; set; }

    /// <summary>翼缘厚度 tf (mm)</summary>
    public double FlangeThickness { get; set; }

    /// <summary>角钢长肢宽 b1 (mm)</summary>
    public double LegWidth1 { get => Height; set => Height = value; }

    /// <summary>角钢短肢宽 b2 (mm)</summary>
    public double LegWidth2 { get => Width; set => Width = value; }

    /// <summary>角钢厚度 t (mm)</summary>
    public double LegThickness { get => WebThickness; set => WebThickness = value; }

    /// <summary>圆管外径 D (mm)</summary>
    public double OuterDiameter { get => Height; set => Height = value; }

    /// <summary>管壁厚度 t (mm)</summary>
    public double WallThickness { get => WebThickness; set => WebThickness = value; }

    /// <summary>内圆弧半径 r (mm)</summary>
    public double RootRadius { get; set; }

    /// <summary>边缘圆弧半径 r1 (mm)</summary>
    public double ToeRadius { get; set; }

    /// <summary>国标出厂截面面积 (cm²)</summary>
    public double StandardAreaCm2 { get; set; }

    /// <summary>国标出厂理论重量 (kg/m)</summary>
    public double StandardMassKgM { get; set; }

    /// <summary>国标出厂强轴惯性矩 Ix (cm⁴)</summary>
    public double StandardIxCm4 { get; set; }

    /// <summary>国标出厂弱轴惯性矩 Iy (cm⁴)</summary>
    public double StandardIyCm4 { get; set; }

    /// <summary>国标出厂强轴截面模量 Wx (cm³)</summary>
    public double StandardWxCm3 { get; set; }

    /// <summary>国标出厂弱轴截面模量 Wy (cm³)</summary>
    public double StandardWyCm3 { get; set; }
}
