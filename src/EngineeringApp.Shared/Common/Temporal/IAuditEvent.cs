namespace EngineeringApp.Shared.Common.Temporal;

/// <summary>
/// 全系统不可篡改审计事件通用接口 (Event Sourcing 模式)
/// 记录由于“调价原则修订”、“时间差异重核”、“图纸深化变更”或“二次议价”引发的修改过程痕迹
/// </summary>
public interface IAuditEvent
{
    /// <summary>审计版本流水编号 (如 Rev 1, Rev 2)</summary>
    int RevisionId { get; set; }

    /// <summary>关联实体唯一物料指纹或构件标签 (如 PL-Q355B-T22-...) </summary>
    string EntityFingerprint { get; set; }

    /// <summary>发生时间</summary>
    DateTime RevisedAt { get; set; }

    /// <summary>经办人/工程师</summary>
    string RevisedBy { get; set; }

    /// <summary>变更原因说明</summary>
    string Reason { get; set; }

    /// <summary>适用的加价原则版本或规范条款依据</summary>
    string AppliedPrinciple { get; set; }

    /// <summary>修改前基准成本/单价指标</summary>
    double OldCostMetric { get; set; }

    /// <summary>修改后新成本/单价指标</summary>
    double NewCostMetric { get; set; }

    /// <summary>造价或指标差额: ΔCost = New - Old</summary>
    double CostDelta => NewCostMetric - OldCostMetric;

    /// <summary>结构化修改明细对照 (JSON 或描述文本)</summary>
    string DetailComparisonJson { get; set; }
}

/// <summary>
/// 通用审计事件标准实体
/// </summary>
public class GenericAuditEvent : IAuditEvent
{
    public int RevisionId { get; set; }
    public string EntityFingerprint { get; set; } = "";
    public DateTime RevisedAt { get; set; } = DateTime.Now;
    public string RevisedBy { get; set; } = "造价工程师";
    public string Reason { get; set; } = "";
    public string AppliedPrinciple { get; set; } = "";
    public double OldCostMetric { get; set; }
    public double NewCostMetric { get; set; }
    public string DetailComparisonJson { get; set; } = "";
}
