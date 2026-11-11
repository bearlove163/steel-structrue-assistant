namespace EngineeringApp.Shared.Reports;

/// <summary>
/// 工程报表访问者接口 (Visitor Pattern)
/// 将业务模型 (截面力学、报价清单、版本审计) 与输出格式 (Markdown, Excel, HTML) 完全解耦
/// </summary>
public interface IReportVisitor
{
    void VisitProjectOverview(ReportProjectOverview element);
    void VisitSectionMechanics(ReportSectionMechanics element);
    void VisitQuotationBid(ReportQuotationBid element);
    void VisitTemporalAudit(ReportTemporalAudit element);

    /// <summary>获取最终导出或渲染的文本成果</summary>
    string GetExportContent();
}
