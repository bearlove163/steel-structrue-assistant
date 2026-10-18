using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.QuotationExport;

namespace EngineeringApp.Shared.Reports;

/// <summary>
/// 工程报表建造者 (Builder Pattern)
/// 通过流式链式调用灵活组合工程概况、截面计算、商业清单与版本审计等不同报告板块
/// </summary>
public class EngineeringReportBuilder
{
    private readonly List<IReportElement> _elements = [];

    public EngineeringReportBuilder AddProjectOverview(
        string projectName,
        string clientOrBidder,
        string standardSystemName = "GB 50017-2017《钢结构设计标准》",
        string description = "")
    {
        _elements.Add(new ReportProjectOverview
        {
            ProjectName = projectName,
            ClientOrBidder = clientOrBidder,
            ReportDate = DateTime.Now,
            StandardSystemName = standardSystemName,
            Description = description
        });
        return this;
    }

    public EngineeringReportBuilder AddSectionMechanics(
        SectionParameters parameters,
        SectionPropertiesResult result,
        MemberBucklingParameters? buckling = null,
        LoadCase? loadCase = null)
    {
        _elements.Add(new ReportSectionMechanics
        {
            Parameters = parameters,
            Result = result,
            Buckling = buckling,
            LoadCase = loadCase
        });
        return this;
    }

    public EngineeringReportBuilder AddQuotationBid(CommercialBidSummary bidSummary)
    {
        _elements.Add(new ReportQuotationBid
        {
            BidSummary = bidSummary
        });
        return this;
    }

    public EngineeringReportBuilder AddTemporalAudit(
        string baselineId,
        string currentId,
        DateTime baselineTime,
        DateTime currentTime,
        double totalDeltaAmount,
        double scissorProfitPerTon,
        double totalScissorProfit,
        string auditConclusion)
    {
        _elements.Add(new ReportTemporalAudit
        {
            BaselineSnapshotId = baselineId,
            CurrentSnapshotId = currentId,
            BaselineTimestamp = baselineTime,
            CurrentTimestamp = currentTime,
            TotalDeltaAmount = totalDeltaAmount,
            ScissorProfitPerTon = scissorProfitPerTon,
            TotalScissorProfit = totalScissorProfit,
            AuditConclusion = auditConclusion
        });
        return this;
    }

    /// <summary>
    /// 将构建好的报告元素输入给指定的 Visitor 访问者，生成最终成果
    /// </summary>
    public string Build(IReportVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        foreach (var element in _elements)
        {
            element.Accept(visitor);
        }

        return visitor.GetExportContent();
    }
}
