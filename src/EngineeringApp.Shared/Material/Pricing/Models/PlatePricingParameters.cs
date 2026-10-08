using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板厚度公差与精度等级 (GB/T 709 与 EN 10029 等效)
/// </summary>
public enum ToleranceClass
{
    /// <summary>A 类公差：普通允许较大负偏差 (国内建筑常规，基准 0)</summary>
    ClassA,

    /// <summary>B 类公差：固定限制负偏差不超过 -0.3mm (加价约 60元/吨)</summary>
    ClassB,

    /// <summary>C 类公差：下偏差为 0，绝对不允许负偏差，保证全厚度 (加价约 150元/吨)</summary>
    ClassC
}

public static class ToleranceClassExtensions
{
    public static string GetDisplayName(this ToleranceClass tol, StandardSystem sys = StandardSystem.GB) => (sys, tol) switch
    {
        (StandardSystem.EN, ToleranceClass.ClassA) => "EN 10029 Class A (Standard)",
        (StandardSystem.EN, ToleranceClass.ClassB) => "EN 10029 Class B (Lower dev -0.3mm)",
        (StandardSystem.EN, ToleranceClass.ClassC) => "EN 10029 Class C (Zero minus, Full thickness)",
        (StandardSystem.ASTM, ToleranceClass.ClassC) => "ASTM A6 Restricted Thickness (Full Thickness)",
        (_, ToleranceClass.ClassA) => "GB/T 709 A类公差 (常规负偏差，基准)",
        (_, ToleranceClass.ClassB) => "GB/T 709 B类公差 (严控负偏差≤-0.3mm)",
        (_, ToleranceClass.ClassC) => "GB/T 709 C类公差 (保证全厚度，无负偏差)",
        _ => tol.ToString()
    };
}

/// <summary>
/// 钢板不平度与平整精度等级
/// </summary>
public enum FlatnessClass
{
    /// <summary>普通不平度精度 (N级)</summary>
    Normal_N,

    /// <summary>高级不平度精度 (H级 / 超平整校平，加价约 80~100元/吨)</summary>
    High_H
}

/// <summary>
/// 厚度方向抗层状撕裂性能 (Z向性能 GB/T 5313 / EN 10164 / ASTM A770)
/// </summary>
public enum ZDirectionQuality
{
    None,
    Z15, // 断面收缩率 ≥15% (加价 150元/吨)
    Z25, // 断面收缩率 ≥25% (加价 250元/吨)
    Z35  // 断面收缩率 ≥35% (加价 400元/吨)
}

/// <summary>
/// 冲击韧性与温度等级
/// </summary>
public enum ImpactGrade
{
    GradeB, // 20℃ 常温冲击 (基准 0)
    GradeC, // 0℃ 冲击 (加价约 60元/吨)
    GradeD, // -20℃ 低温冲击 (加价约 150元/吨)
    GradeE  // -40℃ 极寒冲击 (加价约 350元/吨)
}

/// <summary>
/// 冶金工艺与热处理交货状态
/// </summary>
public enum MetallurgyProcess
{
    AsRolled,              // 热轧状态 (基准 0)
    Normalized,            // 正火处理 / +N (加价约 180元/吨)
    TMCP,                  // 热机械控制工艺控轧 (加价约 100元/吨)
    QuenchedAndTempered    // 调质高强处理 (加价约 300元/吨)
}

/// <summary>
/// 超声波无损探伤级别 (NB/T 47013 / GB/T 2970)
/// </summary>
public enum UltrasonicInspection
{
    None,
    ClassII, // 二级探伤 (加价 120元/吨)
    ClassI   // 一级探伤 (加价 220元/吨)
}

/// <summary>
/// 质量检验认证文件类别 (国内常规质保书 vs 欧标 EN 10204 3.1/3.2)
/// </summary>
public enum InspectionCertificateType
{
    StandardMTC,  // 钢厂常规质保证书 (0 加价)
    EN10204_31,   // EN 10204 3.1 钢厂独立检验代表签名证明 (0~30元/吨)
    EN10204_32    // EN 10204 3.2 国际第三方机构 (BV/DNV/TÜV/SGS) 见证签名 (加价 450元/吨)
}

/// <summary>
/// 定尺与切割状态
/// </summary>
public enum PlateDimensionCutType
{
    MillStandard,    // 钢厂散尺/非定尺 (基准 0)
    FixedDimension,  // 常用定宽定尺 (加价约 60元/吨)
    SuperWide,       // 超宽板 (宽度 > 2800mm，加价约 160元/吨)
    SuperLong,       // 超长板 (长度 > 15m，加价约 180元/吨)
    SmallCut         // 小定尺切割 (长度 < 4m，加价约 100元/吨)
}

/// <summary>
/// 钢板多维加价核算参数模型
/// </summary>
public class PlatePricingParameters
{
    public StandardSystem Standard { get; set; } = StandardSystem.GB;
    public SteelGrade Grade { get; set; } = SteelGrade.Q355B;
    public double ThicknessMm { get; set; } = 20.0;
    public double WidthMm { get; set; } = 2200.0;
    public double LengthMm { get; set; } = 10000.0;
    
    // 钢厂与物流
    public string SteelMillId { get; set; } = "Baosteel";
    public DeliveryCondition Delivery { get; set; } = DeliveryCondition.Delivered_FabricationPlant;
    public string DestinationRegion { get; set; } = "华东-浙江制造车间";

    // 加价维度
    public PlateDimensionCutType CutType { get; set; } = PlateDimensionCutType.FixedDimension;
    public ToleranceClass Tolerance { get; set; } = ToleranceClass.ClassA;
    public FlatnessClass Flatness { get; set; } = FlatnessClass.Normal_N;
    public ImpactGrade Impact { get; set; } = ImpactGrade.GradeB;
    public ZDirectionQuality ZDirection { get; set; } = ZDirectionQuality.None;
    public MetallurgyProcess Metallurgy { get; set; } = MetallurgyProcess.AsRolled;
    public UltrasonicInspection UT { get; set; } = UltrasonicInspection.None;
    public InspectionCertificateType Certificate { get; set; } = InspectionCertificateType.StandardMTC;
}
