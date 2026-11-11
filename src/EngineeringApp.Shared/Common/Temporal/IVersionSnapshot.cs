using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Common.Temporal;

/// <summary>
/// 全系统通用的版本快照泛型元数据接口 (适用于材料价格、涂料定额、BOM清册、图纸模型、力学计算书)
/// </summary>
/// <typeparam name="TData">快照保存的具体状态切片模型</typeparam>
public interface IVersionSnapshot<TData>
{
    /// <summary>快照唯一标识符 (如 "SNAP-20261008-GB")</summary>
    string SnapshotId { get; set; }

    /// <summary>快照人类可读名称 (如 "2026年10月现行施工期市场网价")</summary>
    string SnapshotName { get; set; }

    /// <summary>实体业务品类 ("MaterialPrice", "PaintCoating", "MemberBOM", "DrawingCAD", "StructuralCalc")</summary>
    string EntityCategory { get; set; }

    /// <summary>现实世界生效时间 (Valid / Effective Time，如定标锁价基准日)</summary>
    DateTime EffectiveDate { get; set; }

    /// <summary>系统事务录入时间 (Transaction / Recorded Time)</summary>
    DateTime RecordedAt { get; set; }

    /// <summary>是否作为法定/合同基准锁定不可篡改</summary>
    bool IsLocked { get; set; }

    /// <summary>操作人或造价工程师</summary>
    string Operator { get; set; }

    /// <summary>关联规范标准体系 (如 GB 国标、EN 欧标、ASTM 美标)</summary>
    StandardSystem Standard { get; set; }

    /// <summary>完整状态数据备忘录切片</summary>
    TData StateData { get; set; }
}

/// <summary>
/// 通用版本快照标准实体
/// </summary>
public class GenericVersionSnapshot<TData> : IVersionSnapshot<TData>
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string SnapshotName { get; set; } = "未命名快照";
    public string EntityCategory { get; set; } = "General";
    public DateTime EffectiveDate { get; set; } = DateTime.Today;
    public DateTime RecordedAt { get; set; } = DateTime.Now;
    public bool IsLocked { get; set; }
    public string Operator { get; set; } = "造价工程师";
    public StandardSystem Standard { get; set; } = StandardSystem.GB;
    public TData StateData { get; set; } = default!;

    public GenericVersionSnapshot() { }

    public GenericVersionSnapshot(string id, string name, DateTime effectiveDate, TData data, bool isLocked = false, StandardSystem standard = StandardSystem.GB)
    {
        SnapshotId = id;
        SnapshotName = name;
        EffectiveDate = effectiveDate;
        StateData = data;
        IsLocked = isLocked;
        Standard = standard;
    }
}
