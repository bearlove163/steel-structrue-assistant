using System.Text;

namespace EngineeringApp.Shared.Reports;

/// <summary>
/// Markdown 格式报表访问者 (Visitor)
/// 导出符合 GitHub 规范的专业工程计算书与报价成果文档
/// </summary>
public class MarkdownReportVisitor : IReportVisitor
{
    private readonly StringBuilder _sb = new();

    public void VisitProjectOverview(ReportProjectOverview element)
    {
        _sb.AppendLine($"# {element.ProjectName} - 综合成果报告");
        _sb.AppendLine();
        _sb.AppendLine($"- **编制单位 / 投标人**：{element.ClientOrBidder}");
        _sb.AppendLine($"- **编制时间**：{element.ReportDate:yyyy-MM-dd HH:mm:ss}");
        _sb.AppendLine($"- **依据工程规范**：{element.StandardSystemName}");
        if (!string.IsNullOrWhiteSpace(element.Description))
        {
            _sb.AppendLine($"- **工程简述**：{element.Description}");
        }
        _sb.AppendLine();
    }

    public void VisitSectionMechanics(ReportSectionMechanics element)
    {
        if (element.Result == null || element.Parameters == null) return;

        var p = element.Parameters;
        var r = element.Result;

        _sb.AppendLine("## 截面几何与惯性力学验算计算书");
        _sb.AppendLine();
        _sb.AppendLine($"- **截面形式**：{p.Type} ({p.StandardProfileName ?? "自定义尺寸"})");
        _sb.AppendLine($"- **材料材质**：{p.Material.Name} (fy={p.Material.YieldStrength:F0} MPa, E={p.Material.ElasticModulus:N0} MPa)");
        _sb.AppendLine($"- **主要尺寸**：H={p.Height:F1} mm, B={p.Width:F1} mm, tw={p.WebThickness:F1} mm, tf={p.FlangeThickness:F1} mm");
        _sb.AppendLine();

        _sb.AppendLine("### 1. 截面物理与几何特性表");
        _sb.AppendLine("| 指标项目 | 符号 | 计算数值 | 工程单位 |");
        _sb.AppendLine("| :--- | :--- | :--- | :--- |");
        _sb.AppendLine($"| 截面面积 | A | {r.AreaCm2:F2} | cm² |");
        _sb.AppendLine($"| 理论线密度 (米重) | m | {r.LinearMass:F2} | kg/m |");
        _sb.AppendLine($"| 延米外周展开面积 | A_paint | {r.GrossPaintingAreaPerMeter:F3} | m²/m |");
        if (r.TopSurfaceWidth > 0)
        {
            _sb.AppendLine($"| 扣除楼板贴合净表面积 | A_net_slab | {r.PaintingAreaExcludingTop:F3} | m²/m |");
        }
        _sb.AppendLine($"| 吨钢比表面积 | A_ton | {r.AreaPerTon:F1} | m²/t |");
        _sb.AppendLine($"| 强轴惯性矩 | Iy | {r.IyCm4:F2} | cm⁴ |");
        _sb.AppendLine($"| 弱轴惯性矩 | Iz | {r.IzCm4:F2} | cm⁴ |");
        _sb.AppendLine($"| 强轴回转半径 | iy | {r.IyRadius:F2} | mm |");
        _sb.AppendLine($"| 弱轴回转半径 | iz | {r.IzRadius:F2} | mm |");
        _sb.AppendLine($"| 弹性抗弯截面模量 | Wy,min | {r.WyMinCm3:F2} | cm³ |");
        _sb.AppendLine($"| 塑性截面模量 | Wpl_y | {r.WplyCm3:F2} | cm³ |");
        _sb.AppendLine($"| 自由扭转常数 | J | {r.TorsionConstantJCm4:F2} | cm⁴ |");
        _sb.AppendLine($"| 翘曲常数 | Iw | {r.WarpingConstantIwCm6:F2} | cm⁶ |");
        _sb.AppendLine();

        if (element.Buckling?.EnableSlendernessCheck == true && r.SlendernessResult != null)
        {
            _sb.AppendLine("### 2. 构件长细比校核");
            _sb.AppendLine($"- 构件计算长度：L0x = {r.SlendernessResult.L0x:F0} mm, L0y = {r.SlendernessResult.L0y:F0} mm");
            _sb.AppendLine($"- 最大长细比 λmax = {r.SlendernessResult.LambdaMax:F1} (规范限值 [λ] = {r.SlendernessResult.AllowableLambda:F0})");
            _sb.AppendLine($"- 验算判定：{(r.SlendernessResult.IsSatisfied ? "✅ 合格 (PASSED)" : "❌ 超限 (WARNING)")}");
            _sb.AppendLine();
        }

        if (element.LoadCase?.EnableStressCheck == true && r.StressResult != null)
        {
            var s = r.StressResult;
            _sb.AppendLine("### 3. 截面正应力验算");
            _sb.AppendLine($"- 作用荷载：N = {element.LoadCase.AxialForceN:F1} kN, My = {element.LoadCase.BendingMomentMy:F1} kN·m, Mz = {element.LoadCase.BendingMomentMz:F1} kN·m");
            _sb.AppendLine($"- 最大拉应力 σ_max = +{s.MaxTensileStress:F1} MPa, 最大压应力 σ_min = {s.MinCompressiveStress:F1} MPa");
            _sb.AppendLine($"- 应力利用率比值 η = {s.StressRatio:P1}");
            bool isSafe = s.MaxAbsoluteStress <= p.Material.YieldStrength;
            _sb.AppendLine($"- 验算结论：{(isSafe ? "✅ 强度满足规范要求 (PASSED)" : "❌ 截面应力超限，建议加大截面 (FAILED)")}");
            _sb.AppendLine();
        }
    }

    public void VisitQuotationBid(ReportQuotationBid element)
    {
        if (element.BidSummary == null) return;
        var b = element.BidSummary;

        _sb.AppendLine("## 商业投标工程量清单与组价成果");
        _sb.AppendLine();
        _sb.AppendLine("| 序号 | 项目编码 | 项目名称 | 工程特征 | 单位 | 工程量 | 综合单价 (元/t) | 合价金额 (元) |");
        _sb.AppendLine("| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |");

        foreach (var item in b.Items)
        {
            _sb.AppendLine($"| {item.ItemNumber} | {item.BillCode} | {item.BillName} | {item.ItemDescription} | {item.Unit} | {item.Quantity:F2} | {item.UnitPrice:F2} | {item.TotalPrice:F2} |");
        }

        _sb.AppendLine();
        _sb.AppendLine($"> **合计**：总净重 **{b.TotalNetWeightTon:F2} t**，总报价合计 **¥{b.TotalQuotationAmount:N2} 元**，平均吨钢单价 **¥{b.AverageUnitPricePerTon:F2} 元/t**。");
        _sb.AppendLine();
    }

    public void VisitTemporalAudit(ReportTemporalAudit element)
    {
        _sb.AppendLine("## 双时空版本审计调差与采办创效定案");
        _sb.AppendLine();
        _sb.AppendLine($"- **基准快照版本**：`{element.BaselineSnapshotId}` ({element.BaselineTimestamp:yyyy-MM-dd HH:mm})");
        _sb.AppendLine($"- **对比定案版本**：`{element.CurrentSnapshotId}` ({element.CurrentTimestamp:yyyy-MM-dd HH:mm})");
        _sb.AppendLine($"- **总造价调差变动**：¥{element.TotalDeltaAmount:+0.00;-0.00;0.00} 元");
        _sb.AppendLine($"- **采办套利剪刀差利润**：+¥{element.ScissorProfitPerTon:F2} 元/吨 (累计总创效净利润 ¥{element.TotalScissorProfit:N2} 元)");
        _sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(element.AuditConclusion))
        {
            _sb.AppendLine($"> **审计结论**：{element.AuditConclusion}");
            _sb.AppendLine();
        }
    }

    public string GetExportContent() => _sb.ToString();
}
