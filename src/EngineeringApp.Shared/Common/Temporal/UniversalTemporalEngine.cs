namespace EngineeringApp.Shared.Common.Temporal;

/// <summary>
/// 通用版本差异对比单项结果模型
/// </summary>
public class GenericDiffItem
{
    public string ItemKey { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string Specification { get; set; } = "";
    public double BaselineValue { get; set; }
    public double TargetValue { get; set; }
    public double Difference => TargetValue - BaselineValue;
    public double PercentChange => BaselineValue != 0 ? (Difference / BaselineValue) * 100.0 : 0;
    public string ChangeStatus => Difference > 0 ? "涨价/增加" : Difference < 0 ? "降价/缩减" : "持平";
    public string Remarks { get; set; } = "";
}

/// <summary>
/// 通用版本差异分析报告模型
/// </summary>
/// <typeparam name="TDiffItem">明细差异项类型</typeparam>
public class VersionDiffReport<TDiffItem>
{
    public string BaselineSnapshotId { get; set; } = "";
    public string BaselineSnapshotName { get; set; } = "";
    public DateTime BaselineDate { get; set; }

    public string TargetSnapshotId { get; set; } = "";
    public string TargetSnapshotName { get; set; } = "";
    public DateTime TargetDate { get; set; }

    public List<TDiffItem> DifferenceItems { get; set; } = [];

    /// <summary>基准期总造价/总量</summary>
    public double TotalBaselineAmount { get; set; }

    /// <summary>目标期总造价/总量</summary>
    public double TotalTargetAmount { get; set; }

    /// <summary>全工程总差值: ΔTotal = Target - Baseline</summary>
    public double TotalDifferenceAmount => TotalTargetAmount - TotalBaselineAmount;

    /// <summary>总体波动涨跌百分比 (%)</summary>
    public double OverallPercentChange => TotalBaselineAmount != 0 ? (TotalDifferenceAmount / TotalBaselineAmount) * 100.0 : 0;

    /// <summary>定案审计结论综述</summary>
    public string AuditConclusionSummary { get; set; } = "";
}

/// <summary>
/// 全系统通用时空双时期对比与时光穿梭引擎 (Universal Temporal Engine)
/// </summary>
/// <typeparam name="TData">业务数据快照模型</typeparam>
/// <typeparam name="TDiffItem">差异项模型</typeparam>
public class UniversalTemporalEngine<TData, TDiffItem>
{
    private readonly List<IVersionSnapshot<TData>> _snapshots = [];
    private readonly List<IAuditEvent> _auditChain = [];
    private readonly Func<IVersionSnapshot<TData>, IVersionSnapshot<TData>, VersionDiffReport<TDiffItem>> _diffAlgorithm;

    public IReadOnlyList<IVersionSnapshot<TData>> Snapshots => _snapshots.AsReadOnly();
    public IReadOnlyList<IAuditEvent> AuditChain => _auditChain.AsReadOnly();

    public UniversalTemporalEngine(Func<IVersionSnapshot<TData>, IVersionSnapshot<TData>, VersionDiffReport<TDiffItem>> diffAlgorithm)
    {
        _diffAlgorithm = diffAlgorithm ?? throw new ArgumentNullException(nameof(diffAlgorithm));
    }

    /// <summary>注册一个新快照</summary>
    public void RegisterSnapshot(IVersionSnapshot<TData> snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var existing = _snapshots.FirstOrDefault(s => s.SnapshotId == snapshot.SnapshotId);
        if (existing != null)
        {
            _snapshots.Remove(existing);
        }
        _snapshots.Add(snapshot);
    }

    /// <summary>批量注册快照</summary>
    public void RegisterSnapshots(IEnumerable<IVersionSnapshot<TData>> snapshots)
    {
        foreach (var snap in snapshots)
        {
            RegisterSnapshot(snap);
        }
    }

    /// <summary>时光穿梭：根据指定的现实日期，查询该时间点生效的有效快照</summary>
    public IVersionSnapshot<TData>? TimeTravelTo(DateTime pointInTime)
    {
        return _snapshots
            .Where(s => s.EffectiveDate <= pointInTime)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefault()
            ?? _snapshots.OrderBy(s => s.EffectiveDate).FirstOrDefault();
    }

    /// <summary>双时期对比分析：对比基准期与目标期快照的差异</summary>
    public VersionDiffReport<TDiffItem> Compare(string baselineSnapshotId, string targetSnapshotId)
    {
        var baseSnap = _snapshots.FirstOrDefault(s => s.SnapshotId == baselineSnapshotId)
            ?? throw new KeyNotFoundException($"未找到基准期快照: {baselineSnapshotId}");
        var targetSnap = _snapshots.FirstOrDefault(s => s.SnapshotId == targetSnapshotId)
            ?? throw new KeyNotFoundException($"未找到目标期快照: {targetSnapshotId}");

        return _diffAlgorithm(baseSnap, targetSnap);
    }

    /// <summary>追加不可篡改审计履历事件 (Event Sourcing)</summary>
    public void AppendAuditEvent(IAuditEvent auditEvent)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);
        if (auditEvent.RevisionId <= 0)
        {
            auditEvent.RevisionId = _auditChain.Count + 1;
        }
        _auditChain.Add(auditEvent);
    }

    /// <summary>根据物料指纹或构件标签提取审计时间轴</summary>
    public List<IAuditEvent> GetAuditTimeline(string entityFingerprint)
    {
        if (string.IsNullOrWhiteSpace(entityFingerprint))
        {
            return _auditChain.OrderByDescending(e => e.RevisedAt).ToList();
        }
        return _auditChain
            .Where(e => e.EntityFingerprint == entityFingerprint)
            .OrderByDescending(e => e.RevisedAt)
            .ToList();
    }
}
