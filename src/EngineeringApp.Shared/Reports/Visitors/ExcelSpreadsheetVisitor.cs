using System.Text;

namespace EngineeringApp.Shared.Reports;

/// <summary>
/// Excel/表格网格格式访问者 (Visitor)
/// 导出制表符分隔 (TSV) 的二维矩阵表格，可无缝粘贴至 Excel 或直接载入 RadzenSpreadsheet / BlazorDatasheet 网格控件
/// </summary>
public class ExcelSpreadsheetVisitor : IReportVisitor
{
    private readonly StringBuilder _sb = new();

    public void VisitProjectOverview(ReportProjectOverview element)
    {
        _sb.AppendLine($"项目名称\t{element.ProjectName}");
        _sb.AppendLine($"编制单位\t{element.ClientOrBidder}");
        _sb.AppendLine($"编制日期\t{element.ReportDate:yyyy-MM-dd HH:mm:ss}");
        _sb.AppendLine($"设计标准\t{element.StandardSystemName}");
        _sb.AppendLine();
    }

    public void VisitSectionMechanics(ReportSectionMechanics element)
    {
        if (element.Result == null || element.Parameters == null) return;
        var p = element.Parameters;
        var r = element.Result;

        _sb.AppendLine("【截面特性计算表】");
        _sb.AppendLine("截面类型\t规格型号\t材料材质\t外高H(mm)\t外宽B(mm)\t腹板tw(mm)\t翼缘tf(mm)");
        _sb.AppendLine($"{p.Type}\t{p.StandardProfileName ?? "自定义"}\t{p.Material.Name}\t{p.Height:F1}\t{p.Width:F1}\t{p.WebThickness:F1}\t{p.FlangeThickness:F1}");
        _sb.AppendLine();

        _sb.AppendLine("指标名称\t符号\t计算数值\t工程单位");
        _sb.AppendLine($"截面面积\tA\t{r.AreaCm2:F2}\tcm²");
        _sb.AppendLine($"理论线密度\tm\t{r.LinearMass:F2}\tkg/m");
        _sb.AppendLine($"展开涂装表面积\tA_paint\t{r.GrossPaintingAreaPerMeter:F3}\tm²/m");
        _sb.AppendLine($"吨钢表面积\tA_ton\t{r.AreaPerTon:F1}\tm²/t");
        _sb.AppendLine($"强轴惯性矩\tIy\t{r.IyCm4:F2}\tcm⁴");
        _sb.AppendLine($"弱轴惯性矩\tIz\t{r.IzCm4:F2}\tcm⁴");
        _sb.AppendLine($"强轴回转半径\tiy\t{r.IyRadius:F2}\tmm");
        _sb.AppendLine($"弱轴回转半径\tiz\t{r.IzRadius:F2}\tmm");
        _sb.AppendLine($"截面抗弯模量\tWy,min\t{r.WyMinCm3:F2}\tcm³");
        _sb.AppendLine($"塑性截面模量\tWpl_y\t{r.WplyCm3:F2}\tcm³");
        _sb.AppendLine();
    }

    public void VisitQuotationBid(ReportQuotationBid element)
    {
        if (element.BidSummary == null) return;
        var b = element.BidSummary;

        _sb.AppendLine("【商业投标工程量清单表】");
        _sb.AppendLine("序号\t清单项目编码\t项目名称\t特征描述\t单位\t工程量\t综合单价(元/t)\t合价金额(元)");

        foreach (var item in b.Items)
        {
            _sb.AppendLine($"{item.ItemNumber}\t{item.BillCode}\t{item.BillName}\t{item.ItemDescription}\t{item.Unit}\t{item.Quantity:F2}\t{item.UnitPrice:F2}\t{item.TotalPrice:F2}");
        }

        _sb.AppendLine($"合计\t\t\t\tt\t{b.TotalNetWeightTon:F2}\t{b.AverageUnitPricePerTon:F2}\t{b.TotalQuotationAmount:F2}");
        _sb.AppendLine();
    }

    public void VisitTemporalAudit(ReportTemporalAudit element)
    {
        _sb.AppendLine("【双时空版本审计对比表】");
        _sb.AppendLine("基准快照版本\t对比定案版本\t总造价差额(元)\t采办套利剪刀差(元/吨)\t累计创效总净利润(元)");
        _sb.AppendLine($"{element.BaselineSnapshotId}\t{element.CurrentSnapshotId}\t{element.TotalDeltaAmount:F2}\t{element.ScissorProfitPerTon:F2}\t{element.TotalScissorProfit:F2}");
        _sb.AppendLine($"审计结论\t{element.AuditConclusion}");
        _sb.AppendLine();
    }

    public string GetExportContent() => _sb.ToString();
}
