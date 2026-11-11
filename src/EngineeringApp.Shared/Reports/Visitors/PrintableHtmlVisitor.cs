using System.Text;

namespace EngineeringApp.Shared.Reports;

/// <summary>
/// 可打印 HTML 报表访问者 (Visitor)
/// 导出带工程公文样式、防伪水印与表格排版的标准 HTML 成果
/// </summary>
public class PrintableHtmlVisitor : IReportVisitor
{
    private readonly StringBuilder _sb = new();

    public PrintableHtmlVisitor()
    {
        _sb.AppendLine("<!DOCTYPE html>");
        _sb.AppendLine("<html lang=\"zh-CN\">");
        _sb.AppendLine("<head>");
        _sb.AppendLine("<meta charset=\"utf-8\" />");
        _sb.AppendLine("<title>工程计算书与报价成果报告</title>");
        _sb.AppendLine("<style>");
        _sb.AppendLine("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; line-height: 1.5; padding: 24px; max-width: 1000px; margin: 0 auto; }");
        _sb.AppendLine("h1, h2, h3 { color: #0f172a; margin-top: 1.5em; margin-bottom: 0.5em; }");
        _sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 1.5em; font-size: 13px; }");
        _sb.AppendLine("th, td { border: 1px solid #cbd5e1; padding: 8px 12px; text-align: left; }");
        _sb.AppendLine("th { background-color: #f1f5f9; font-weight: 600; text-align: center; }");
        _sb.AppendLine(".badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: bold; }");
        _sb.AppendLine(".badge-pass { background-color: #dcfce7; color: #15803d; }");
        _sb.AppendLine(".badge-fail { background-color: #fee2e2; color: #b91c1c; }");
        _sb.AppendLine(".summary-box { background: #f8fafc; border-left: 4px solid #3b82f6; padding: 12px 16px; margin: 16px 0; border-radius: 0 4px 4px 0; }");
        _sb.AppendLine("@media print { body { padding: 0; } .no-print { display: none; } }");
        _sb.AppendLine("</style>");
        _sb.AppendLine("</head>");
        _sb.AppendLine("<body>");
    }

    public void VisitProjectOverview(ReportProjectOverview element)
    {
        _sb.AppendLine($"<div style=\"border-bottom: 2px solid #0284c7; padding-bottom: 12px; margin-bottom: 20px;\">");
        _sb.AppendLine($"<h1 style=\"margin: 0; color: #0284c7;\">{element.ProjectName}</h1>");
        _sb.AppendLine($"<div style=\"font-size: 13px; color: #64748b; margin-top: 6px;\">");
        _sb.AppendLine($"编制单位：{element.ClientOrBidder} | 编制日期：{element.ReportDate:yyyy-MM-dd HH:mm:ss} | 依据规范：{element.StandardSystemName}");
        _sb.AppendLine($"</div>");
        _sb.AppendLine($"</div>");
    }

    public void VisitSectionMechanics(ReportSectionMechanics element)
    {
        if (element.Result == null || element.Parameters == null) return;
        var p = element.Parameters;
        var r = element.Result;

        _sb.AppendLine("<h2>一、 截面几何与惯性力学验算</h2>");
        _sb.AppendLine($"<p><strong>截面形式：</strong> {p.Type} ({p.StandardProfileName ?? "自定义"}) | <strong>材料：</strong> {p.Material.Name} (fy={p.Material.YieldStrength:F0} MPa)</p>");

        _sb.AppendLine("<table>");
        _sb.AppendLine("<thead><tr><th>指标名称</th><th>符号</th><th>计算数值</th><th>工程单位</th></tr></thead>");
        _sb.AppendLine("<tbody>");
        _sb.AppendLine($"<tr><td>截面面积</td><td style=\"text-align:center;\">A</td><td style=\"font-weight:bold;\">{r.AreaCm2:F2}</td><td>cm²</td></tr>");
        _sb.AppendLine($"<tr><td>理论线密度 (米重)</td><td style=\"text-align:center;\">m</td><td style=\"font-weight:bold;\">{r.LinearMass:F2}</td><td>kg/m</td></tr>");
        _sb.AppendLine($"<tr><td>延米外周展开表面积</td><td style=\"text-align:center;\">A_paint</td><td>{r.GrossPaintingAreaPerMeter:F3}</td><td>m²/m</td></tr>");
        _sb.AppendLine($"<tr><td>吨钢比表面积</td><td style=\"text-align:center;\">A_ton</td><td>{r.AreaPerTon:F1}</td><td>m²/t</td></tr>");
        _sb.AppendLine($"<tr><td>强轴惯性矩</td><td style=\"text-align:center;\">Iy</td><td>{r.IyCm4:F2}</td><td>cm⁴</td></tr>");
        _sb.AppendLine($"<tr><td>弱轴惯性矩</td><td style=\"text-align:center;\">Iz</td><td>{r.IzCm4:F2}</td><td>cm⁴</td></tr>");
        _sb.AppendLine($"<tr><td>强轴回转半径</td><td style=\"text-align:center;\">iy</td><td>{r.IyRadius:F2}</td><td>mm</td></tr>");
        _sb.AppendLine($"<tr><td>截面弹性模量</td><td style=\"text-align:center;\">Wy,min</td><td>{r.WyMinCm3:F2}</td><td>cm³</td></tr>");
        _sb.AppendLine("</tbody>");
        _sb.AppendLine("</table>");

        if (element.LoadCase?.EnableStressCheck == true && r.StressResult != null)
        {
            var s = r.StressResult;
            bool isSafe = s.MaxAbsoluteStress <= p.Material.YieldStrength;
            _sb.AppendLine("<div class=\"summary-box\">");
            _sb.AppendLine($"<strong>截面正应力验算结论：</strong> 最大应力 σ = {s.MaxAbsoluteStress:F1} MPa，强度利用率 η = {s.StressRatio:P1} ");
            _sb.AppendLine($"<span class=\"badge {(isSafe ? "badge-pass" : "badge-fail")}\">{(isSafe ? "合格 PASSED" : "不合格 FAILED")}</span>");
            _sb.AppendLine("</div>");
        }
    }

    public void VisitQuotationBid(ReportQuotationBid element)
    {
        if (element.BidSummary == null) return;
        var b = element.BidSummary;

        _sb.AppendLine("<h2>二、 商业工程量清单与投标报价汇总</h2>");
        _sb.AppendLine("<table>");
        _sb.AppendLine("<thead><tr><th>序号</th><th>编码</th><th>项目名称</th><th>特征描述</th><th>单位</th><th>工程量</th><th>综合单价 (元/t)</th><th>合价金额 (元)</th></tr></thead>");
        _sb.AppendLine("<tbody>");
        foreach (var item in b.Items)
        {
            _sb.AppendLine($"<tr><td>{item.ItemNumber}</td><td>{item.BillCode}</td><td>{item.BillName}</td><td>{item.ItemDescription}</td><td>{item.Unit}</td><td>{item.Quantity:F2}</td><td>{item.UnitPrice:F2}</td><td style=\"font-weight:bold;\">{item.TotalPrice:N2}</td></tr>");
        }
        _sb.AppendLine("</tbody>");
        _sb.AppendLine("</table>");
        _sb.AppendLine($"<div class=\"summary-box\"><strong>报价汇总：</strong> 构件总净重 <strong>{b.TotalNetWeightTon:F2} t</strong>，投标总报价 <strong>¥{b.TotalQuotationAmount:N2} 元</strong>，平均吨钢单价 <strong>¥{b.AverageUnitPricePerTon:F2} 元/t</strong>。</div>");
    }

    public void VisitTemporalAudit(ReportTemporalAudit element)
    {
        _sb.AppendLine("<h2>三、 双时空版本审计调差与套利创效定案</h2>");
        _sb.AppendLine("<table>");
        _sb.AppendLine("<thead><tr><th>基准快照版本</th><th>对比定案版本</th><th>总造价调差额 (元)</th><th>采办剪刀差利润 (元/t)</th><th>总创效净利润 (元)</th></tr></thead>");
        _sb.AppendLine($"<tbody><tr><td>{element.BaselineSnapshotId}</td><td>{element.CurrentSnapshotId}</td><td>¥{element.TotalDeltaAmount:+0.00;-0.00;0.00}</td><td style=\"color:#16a34a; font-weight:bold;\">+{element.ScissorProfitPerTon:F2}</td><td style=\"color:#16a34a; font-weight:bold;\">¥{element.TotalScissorProfit:N2}</td></tr></tbody>");
        _sb.AppendLine("</table>");
        if (!string.IsNullOrWhiteSpace(element.AuditConclusion))
        {
            _sb.AppendLine($"<div class=\"summary-box\"><strong>审计定案意见：</strong> {element.AuditConclusion}</div>");
        }
    }

    public string GetExportContent()
    {
        _sb.AppendLine("<div style=\"margin-top: 40px; font-size: 12px; color: #94a3b8; text-align: right; border-top: 1px solid #e2e8f0; padding-top: 8px;\">");
        _sb.AppendLine("系统引擎: Antigravity Structural Engineering Workstation (Blazor WASM)</div>");
        _sb.AppendLine("</body></html>");
        return _sb.ToString();
    }
}
