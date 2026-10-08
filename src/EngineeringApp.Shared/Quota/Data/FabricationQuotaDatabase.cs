using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Quota;

/// <summary>
/// 钢结构构件加工制造定额数据库 (实现 SectionType 截面物理型式 x MemberRole 构件角色的二维正交工费矩阵)
/// </summary>
public static class FabricationQuotaDatabase
{
    private static readonly List<FabricationQuotaItem> _items = [];

    static FabricationQuotaDatabase()
    {
        InitializeDatabase();
    }

    public static IReadOnlyList<FabricationQuotaItem> AllItems => _items;

    private static void InitializeDatabase()
    {
        // 1. 焊接箱型截面 (RHS / Box)
        Add(SectionType.RHS, MemberRole.Column, 2100.0, 2.3, "四板组对埋弧焊、内部多道电渣焊熔透隔板、四向牛腿装配与柱脚组焊", includesDiaphragms: true);
        Add(SectionType.RHS, MemberRole.Beam, 1700.0, 1.9, "四板组装埋弧焊、跨中设计预起拱、两端抗剪抗弯端板组装", includesDiaphragms: false);
        Add(SectionType.RHS, MemberRole.Bracing, 1500.0, 1.6, "箱型斜支撑、端部十字插板组对焊接");
        Add(SectionType.RHS, MemberRole.TrussChord, 1850.0, 2.0, "箱型桁架主弦杆、节点承压封头板与节点板焊接");

        // 2. 焊接H型钢 / 工字钢截面 (HBeam)
        Add(SectionType.HBeam, MemberRole.Column, 1400.0, 1.6, "三板H型钢组立门焊、柱中加强肋板组焊、牛腿装配");
        Add(SectionType.HBeam, MemberRole.Beam, 1100.0, 1.3, "三板H型钢数控下料组立、自动埋弧焊、高强螺栓孔数控钻孔");
        Add(SectionType.HBeam, MemberRole.Bracing, 1050.0, 1.2, "H型钢斜撑构件、两端节点连接板组装");
        Add(SectionType.HBeam, MemberRole.CantileverBracket, 1350.0, 1.5, "变截面或等截面悬臂牛腿段坡口熔透组对");

        // 3. 结构圆管截面 (CHS)
        Add(SectionType.CHS, MemberRole.Column, 1750.0, 2.0, "厚壁大口径钢管柱接长组对、柱脚环形加劲锚栓板、管内自密实混凝土透气孔");
        Add(SectionType.CHS, MemberRole.TrussChord, 1900.0, 2.1, "主弦管定尺下料接长、环缝内衬管自动焊、相贯支管汇交承压区强化");
        Add(SectionType.CHS, MemberRole.TrussWeb, 2300.0, 2.6, "五轴数控立体相贯线马鞍形切割(Saddle Cut)、相贯变坡口组焊与无损探伤", includesIntersecting: true);
        Add(SectionType.CHS, MemberRole.Bracing, 1400.0, 1.5, "圆管支撑、端部封板及开槽节点板组焊");

        // 4. 十字形组合截面 (Cruciform)
        Add(SectionType.Cruciform, MemberRole.Column, 2400.0, 2.7, "工字钢剖分或双T组合拼焊、四象限对称多道焊防止焊接变形、内隔板电渣焊", includesDiaphragms: true);

        // 5. 槽钢与角钢 (Channel / Angle)
        Add(SectionType.Angle, MemberRole.Bracing, 750.0, 0.9, "角钢数控冲孔、定长剪切、端部斜角下料");
        Add(SectionType.Channel, MemberRole.Beam, 850.0, 1.0, "热轧槽钢檩条或次梁数控钻孔与加劲肋拼装");
        Add(SectionType.Angle, MemberRole.PurlinGirth, 700.0, 0.8, "角钢拉条、剪刀撑批量下料冲孔");

        // 6. 默认与零星构件
        Add(SectionType.Rectangle, MemberRole.Miscellaneous, 1200.0, 1.4, "实心扁钢或预埋件加工");
        Add(SectionType.Polygon, MemberRole.Miscellaneous, 1800.0, 2.0, "异型多边形构件或复杂组合节点件拼装");
    }

    private static void Add(
        SectionType sectionType,
        MemberRole role,
        double unitPrice,
        double laborDays,
        string desc,
        bool includesDiaphragms = false,
        bool includesIntersecting = false)
    {
        _items.Add(new FabricationQuotaItem
        {
            SectionType = sectionType,
            Role = role,
            BaseFabricationUnitPricePerTon = unitPrice,
            BaseLaborDaysPerTon = laborDays,
            ProcessDescription = desc,
            IncludesInternalDiaphragms = includesDiaphragms,
            IncludesIntersectingCuts = includesIntersecting
        });
    }

    /// <summary>
    /// 根据截面物理型式与构件工程角色精确查询基础定额工费
    /// </summary>
    public static FabricationQuotaItem Query(SectionType sectionType, MemberRole role)
    {
        var matched = _items.FirstOrDefault(x => x.SectionType == sectionType && x.Role == role);
        if (matched != null) return matched;

        // 次级降级：优先匹配同截面型式的默认项
        var fallbackSection = _items.FirstOrDefault(x => x.SectionType == sectionType);
        if (fallbackSection != null) return fallbackSection;

        // 通用兜底
        return new FabricationQuotaItem
        {
            SectionType = sectionType,
            Role = role,
            BaseFabricationUnitPricePerTon = 1350.0,
            BaseLaborDaysPerTon = 1.5,
            ProcessDescription = "通用构件下料与拼焊组装工艺"
        };
    }
}
