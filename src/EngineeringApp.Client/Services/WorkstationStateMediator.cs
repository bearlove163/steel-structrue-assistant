using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Client.Services;

/// <summary>
/// 三栏工作台全局中介者与领域状态总线 (Mediator & State Bus Pattern)
/// 消除左树、中台、右向导之间层层嵌套的 EventCallback 胶水代码，实现星型解耦
/// </summary>
public class WorkstationStateMediator
{
    /// <summary>当前选定的多国标准体系</summary>
    public StandardSystem CurrentStandard { get; private set; } = StandardSystem.GB;

    /// <summary>当前选定的价格基准日快照编号</summary>
    public string SelectedSnapshotId { get; private set; } = "";

    /// <summary>当前选定的物料指纹或构件标签</summary>
    public string ActiveEntityFingerprint { get; private set; } = "";

    /// <summary>右侧向导与审计抽屉开启状态</summary>
    public bool IsInspectorOpen { get; private set; } = true;

    /// <summary>标准体系切换通知事件</summary>
    public event Action<StandardSystem>? OnStandardChanged;

    /// <summary>价格快照切换通知事件</summary>
    public event Action<string>? OnSnapshotChanged;

    /// <summary>选定构件/物料切换通知事件</summary>
    public event Action<string>? OnEntityChanged;

    /// <summary>右侧向导显示/折叠切换通知事件</summary>
    public event Action<bool>? OnInspectorToggled;

    /// <summary>切换标准体系</summary>
    public void SetStandard(StandardSystem standard)
    {
        if (CurrentStandard != standard)
        {
            CurrentStandard = standard;
            OnStandardChanged?.Invoke(CurrentStandard);
        }
    }

    /// <summary>切换快照基准日</summary>
    public void SetSnapshotId(string snapshotId)
    {
        if (SelectedSnapshotId != snapshotId)
        {
            SelectedSnapshotId = snapshotId;
            OnSnapshotChanged?.Invoke(SelectedSnapshotId);
        }
    }

    /// <summary>在中栏或左栏选中特定物料/构件</summary>
    public void SelectEntity(string fingerprint)
    {
        ActiveEntityFingerprint = fingerprint;
        OnEntityChanged?.Invoke(ActiveEntityFingerprint);
    }

    /// <summary>切换右侧向导或抽屉的展开/收起</summary>
    public void ToggleInspector()
    {
        IsInspectorOpen = !IsInspectorOpen;
        OnInspectorToggled?.Invoke(IsInspectorOpen);
    }

    public void SetInspectorState(bool isOpen)
    {
        if (IsInspectorOpen != isOpen)
        {
            IsInspectorOpen = isOpen;
            OnInspectorToggled?.Invoke(IsInspectorOpen);
        }
    }
}
