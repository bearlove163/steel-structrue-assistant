using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 型材大类划分
/// </summary>
public enum ProfileCategory
{
    /// <summary>热轧型钢 (H型钢 / 工字钢 / 槽钢 / 角钢)</summary>
    HotRolled_H,

    /// <summary>无缝钢管 (CHS / Seamless Pipe)</summary>
    Seamless_CHS,

    /// <summary>直缝高频焊管 (CHS / ERW Pipe)</summary>
    Welded_CHS,

    /// <summary>常规冷弯方矩管 (RHS / Standard Cold-formed Box)</summary>
    StandardColdFormed_RHS,

    /// <summary>特种圆转方冷成型厚壁方矩管 (RHS / Heavy Round-to-Square)</summary>
    RoundToSquare_RHS
}

public static class ProfileCategoryExtensions
{
    public static string GetDisplayName(this ProfileCategory category) => category switch
    {
        ProfileCategory.HotRolled_H => "热轧H型钢 / 工字钢",
        ProfileCategory.Seamless_CHS => "无缝钢管 (热轧穿孔)",
        ProfileCategory.Welded_CHS => "直缝焊管 (ERW 高频焊)",
        ProfileCategory.StandardColdFormed_RHS => "常规冷弯方矩管",
        ProfileCategory.RoundToSquare_RHS => "圆转方特种冷挤压方矩管",
        _ => category.ToString()
    };
}

/// <summary>
/// 型材成型工艺
/// </summary>
public enum ProfileFormingProcess
{
    DirectHotRolled,          // 钢厂大型轧机直接热轧 (成品出厂)
    HotPiercedSeamless,       // 管坯加热斜轧穿孔连轧 (无缝管)
    HighFrequencyWelded,      // 带钢连续辊弯高频电阻焊 (ERW)
    RoundToSquareColdFormed,  // 焊管后经多道次冷辊压模具强行挤压成方 (圆转方)
    PlateWeldedBuiltUp        // 钢板数控下料组焊 (焊接箱型/焊接H型)
}

/// <summary>
/// 型材商业价格与工艺核算参数
/// </summary>
public class ProfilePricingParameters
{
    public StandardSystem Standard { get; set; } = StandardSystem.GB;
    public SteelGrade Grade { get; set; } = SteelGrade.Q355B;
    public ProfileCategory Category { get; set; } = ProfileCategory.HotRolled_H;
    public ProfileFormingProcess Process { get; set; } = ProfileFormingProcess.DirectHotRolled;

    /// <summary>截面型号/标注 (如 HM 300x200x9x14, □400x400x16, Φ325x16)</summary>
    public string SectionDesignation { get; set; } = "HM 300x200x9x14";

    public double HeightMm { get; set; } = 300.0;
    public double WidthMm { get; set; } = 200.0;
    public double WallThicknessMm { get; set; } = 14.0;
    public double LengthMeter { get; set; } = 12.0;

    // 供应链属性
    public string SteelMillId { get; set; } = "Jinxi";
    public DeliveryCondition Delivery { get; set; } = DeliveryCondition.Delivered_FabricationPlant;
    public string DestinationRegion { get; set; } = "华东-浙江制造车间";

    // 工艺与定尺附加
    /// <summary>圆转方角部去应力退火热处理 (特厚方管防裂纹)</summary>
    public bool NeedsCornerStressReliefAnneal { get; set; } = false;

    /// <summary>是否定尺订货 (>12m 特长定尺加价)</summary>
    public bool IsFixedLength { get; set; } = true;
}
