namespace EngineeringApp.Shared.Pricing;

/// <summary>
/// 零件在装配构件中的细分功能分类
/// </summary>
public enum PartFunctionalRole
{
    /// <summary>主肢型钢 / 主筒体 (主要承重杆体)</summary>
    MainProfile,

    /// <summary>牛腿梁段 (如焊接在柱侧的外伸 H型钢梁段)</summary>
    BracketBeam,

    /// <summary>内隔板 / 封头板 (如箱型柱内部电渣压力焊隔板)</summary>
    Diaphragm,

    /// <summary>柱底承压底板 / 支座垫板 (基础受压钢板)</summary>
    BasePlate,

    /// <summary>加劲肋板 / 节点加强肋</summary>
    StiffenerRib,

    /// <summary>节点连接板 / 剪切角钢</summary>
    ConnectionPlate,

    /// <summary>柱梁工地安装拼接连接耳板 / 临时固定板</summary>
    SplicePlate,

    /// <summary>吊装工艺吊耳</summary>
    LiftingLug
}
