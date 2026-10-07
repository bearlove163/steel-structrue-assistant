namespace EngineeringApp.Shared.Models;

/// <summary>
/// 涂装与防腐表面积计算配置选项
/// 支持按土木工程/钢结构施工规则扣除免涂装区域（例如楼板贴合面、垫层基础面）
/// </summary>
public class PaintingCalculationOptions
{
    /// <summary>
    /// 是否扣除截面上表面涂装面积（例如 H型钢/箱型钢梁 上表面与现浇混凝土楼板、组合楼板或压型钢板密贴，无需喷涂防腐漆）
    /// </summary>
    public bool ExcludeTopSurface { get; set; } = false;

    /// <summary>
    /// 自定义扣除的上表面宽度 (mm)。
    /// 若为 null，则自动使用截面的顶部暴露宽度 TopSurfaceWidth（如 H型钢上翼缘全宽 b，箱型梁顶宽 b）
    /// </summary>
    public double? TopSurfaceDeductionWidth { get; set; }

    /// <summary>
    /// 是否扣除截面下表面涂装面积（例如下表面埋置于混凝土或基础中）
    /// </summary>
    public bool ExcludeBottomSurface { get; set; } = false;

    /// <summary>
    /// 自定义扣除的下表面宽度 (mm)。
    /// 若为 null，则自动使用截面的底部暴露宽度 BottomSurfaceWidth
    /// </summary>
    public double? BottomSurfaceDeductionWidth { get; set; }

    /// <summary>
    /// 闭口截面（方矩管 RHS、圆管 CHS、箱型截面）是否包含内表面积。
    /// - false（默认）：建筑钢结构常规油漆防腐仅喷涂外表面；
    /// - true：用于热浸镀锌、酸洗钝化或双面重防腐等需要内外表面全浸润的工况。
    /// </summary>
    public bool IncludeInnerSurface { get; set; } = false;

    /// <summary>
    /// 施工喷涂损耗系数 (例如 1.05 表示考虑 5% 的现场喷涂或粗糙度损耗，默认为 1.0)
    /// </summary>
    public double LossRatio { get; set; } = 1.0;

    /// <summary>
    /// 便捷工厂方法：创建常规“楼板密贴/免喷涂上表面”的配置
    /// </summary>
    public static PaintingCalculationOptions BeamWithFloorSlab() => new()
    {
        ExcludeTopSurface = true
    };

    /// <summary>
    /// 便捷工厂方法：创建外表面全涂装配置（常规裸露梁柱构件）
    /// </summary>
    public static PaintingCalculationOptions StandardOuter() => new()
    {
        ExcludeTopSurface = false,
        IncludeInnerSurface = false
    };

    /// <summary>
    /// 便捷工厂方法：创建热浸镀锌全表面配置（含内部浸锌）
    /// </summary>
    public static PaintingCalculationOptions HotDipGalvanizing() => new()
    {
        ExcludeTopSurface = false,
        IncludeInnerSurface = true
    };
}
