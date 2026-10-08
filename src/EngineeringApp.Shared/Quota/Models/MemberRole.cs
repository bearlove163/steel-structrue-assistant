namespace EngineeringApp.Shared.Quota;

/// <summary>
/// 构件在空间建筑结构体系中的工程功能角色 (清单项归类、安装工法与附属节点特征正交维度)
/// </summary>
public enum MemberRole
{
    /// <summary>框架钢柱 / 门架刚架柱 / 抗风柱 (GZ / KFZ)</summary>
    Column,

    /// <summary>框架主梁 / 楼层次梁 / 悬挑梁 / 吊车梁 (GL / CL / DL)</summary>
    Beam,

    /// <summary>柱间垂直支撑 / 屋面水平支撑 / 刚性系杆 (ZC / SC / XG)</summary>
    Bracing,

    /// <summary>空间管桁架 / 平面桁架弦杆 (上弦杆 / 下弦杆)</summary>
    TrussChord,

    /// <summary>空间桁架腹杆 (斜腹杆 / 竖腹杆，涉及相贯线切口组焊)</summary>
    TrussWeb,

    /// <summary>独立悬挑牛腿 / 托架 / 结构挑檐</summary>
    CantileverBracket,

    /// <summary>冷弯薄壁檩条 / 墙梁次结构 (C/Z型钢檩条)</summary>
    PurlinGirth,

    /// <summary>零星钢构构件 (钢楼梯 / 检修平台 / 护栏 / 预埋件件)</summary>
    Miscellaneous
}

public static class MemberRoleExtensions
{
    public static string GetDisplayName(this MemberRole role) => role switch
    {
        MemberRole.Column => "钢柱 (Column / GZ)",
        MemberRole.Beam => "钢梁 (Beam / GL)",
        MemberRole.Bracing => "支撑/系杆 (Bracing / ZC)",
        MemberRole.TrussChord => "桁架弦杆 (Truss Chord)",
        MemberRole.TrussWeb => "桁架腹杆 (Truss Web)",
        MemberRole.CantileverBracket => "悬臂牛腿/托架 (Bracket)",
        MemberRole.PurlinGirth => "檩条/墙梁 (Purlin / Girth)",
        MemberRole.Miscellaneous => "零星构件 (Miscellaneous)",
        _ => role.ToString()
    };

    public static string GetStandardBillCode(this MemberRole role) => role switch
    {
        MemberRole.Column => "010605001 (钢柱)",
        MemberRole.Beam => "010606001 (钢梁)",
        MemberRole.Bracing => "010607001 (钢支撑)",
        MemberRole.TrussChord or MemberRole.TrussWeb => "010607002 (钢屋架/桁架)",
        MemberRole.PurlinGirth => "010608001 (钢檩条)",
        _ => "010609001 (零星钢构件)"
    };
}
