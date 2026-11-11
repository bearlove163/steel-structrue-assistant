using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.QuotationExport;

namespace EngineeringApp.Shared.Reports;

/// <summary>
/// 报表可访问元素接口 (Visitor Pattern)
/// </summary>
public interface IReportElement
{
    void Accept(IReportVisitor visitor);
}

/// <summary>
/// 项目概况元素
/// </summary>
public class ReportProjectOverview : IReportElement
{
    public string ProjectName { get; set; } = "工业钢结构综合工程";
    public string ClientOrBidder { get; set; } = "钢结构工程技术有限公司";
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public string StandardSystemName { get; set; } = "GB 50017-2017《钢结构设计标准》";
    public string Description { get; set; } = "";

    public void Accept(IReportVisitor visitor) => visitor.VisitProjectOverview(this);
}

/// <summary>
/// 截面几何与惯性力学验算元素
/// </summary>
public class ReportSectionMechanics : IReportElement
{
    public SectionParameters? Parameters { get; set; }
    public SectionPropertiesResult? Result { get; set; }
    public MemberBucklingParameters? Buckling { get; set; }
    public LoadCase? LoadCase { get; set; }

    public void Accept(IReportVisitor visitor) => visitor.VisitSectionMechanics(this);
}

/// <summary>
/// 商业投标报价清单元素
/// </summary>
public class ReportQuotationBid : IReportElement
{
    public CommercialBidSummary? BidSummary { get; set; }

    public void Accept(IReportVisitor visitor) => visitor.VisitQuotationBid(this);
}

/// <summary>
/// 双时空版本审计调差与套利创效元素
/// </summary>
public class ReportTemporalAudit : IReportElement
{
    public string BaselineSnapshotId { get; set; } = "";
    public string CurrentSnapshotId { get; set; } = "";
    public DateTime BaselineTimestamp { get; set; }
    public DateTime CurrentTimestamp { get; set; }
    public double TotalDeltaAmount { get; set; }
    public double ScissorProfitPerTon { get; set; }
    public double TotalScissorProfit { get; set; }
    public string AuditConclusion { get; set; } = "";

    public void Accept(IReportVisitor visitor) => visitor.VisitTemporalAudit(this);
}
