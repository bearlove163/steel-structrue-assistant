namespace EngineeringApp.Shared.Models;

/// <summary>
/// 截面类型枚举
/// </summary>
public enum SectionType
{
    /// <summary>实心矩形</summary>
    Rectangle,

    /// <summary>实心圆形</summary>
    Circle,

    /// <summary>空心圆管 (CHS - Circular Hollow Section)</summary>
    CHS,

    /// <summary>空心方矩管 / 箱型截面 (RHS / Box Section)</summary>
    RHS,

    /// <summary>H型钢 / 工字钢 (I / H Beam)</summary>
    HBeam,

    /// <summary>槽钢 (Channel / C Section)</summary>
    Channel,

    /// <summary>等边 / 不等边角钢 (Angle / L Section)</summary>
    Angle,

    /// <summary>T型钢 (T Section)</summary>
    TSection,

    /// <summary>十字形截面 (Cruciform Section)</summary>
    Cruciform,

    /// <summary>任意多边形 (Arbitrary Polygon / Composite)</summary>
    Polygon
}
