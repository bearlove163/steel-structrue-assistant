// ============================================================================
// 文件归属: src/EngineeringApp.Shared/BOQ/Export/BoqExcelExporter.cs
// 模块描述: 钢结构大型工程 BOQ 纯 C# 多受众 Excel 与 CSV 导出引擎
// 核心特征: 零外部组件依赖 (No EPPlus/NPOI/ClosedXML)、零 JS/TS、
//           原生支持 UTF-8 BOM RFC 4180 CSV、SpreadsheetML 2003 XML 多工作表及 Excel HTML
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Shared.BOQ.Export
{
    /// <summary>
    /// 钢结构 BOQ 纯 C# 多受众 Excel 与 CSV 导出引擎
    /// </summary>
    public static class BoqExcelExporter
    {
        private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

        // =========================================================================
        // 1. CSV EXPORT API (RFC 4180, UTF-8 BOM 杜绝中文乱码)
        // =========================================================================

        /// <summary>
        /// 从已生成的受众投影结果导出 CSV 文本字符串 (含 UTF-8 BOM 与指标卡片)
        /// </summary>
        public static string ExportToCsv(AudienceProjectionResult projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            var sb = new StringBuilder();
            // 1. 插入 UTF-8 BOM 字符，防止 Windows 简体中文版 Excel 打开时中文乱码
            sb.Append('\uFEFF');

            // 2. 报表表头元信息横幅
            sb.AppendLine($"# 报表视角: {projection.PerspectiveName}");
            sb.AppendLine($"# 导出时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("#");

            // 3. 列标题行
            sb.AppendLine(string.Join(",", projection.Headers.Select(EscapeCsvField)));

            // 4. 数据数据行
            foreach (var row in projection.Rows)
            {
                var lineValues = projection.Headers.Select(h =>
                {
                    row.TryGetValue(h, out var val);
                    return EscapeCsvField(val);
                });
                sb.AppendLine(string.Join(",", lineValues));
            }

            // 5. 核心指标大盘汇总 (如果有)
            if (projection.SummaryMetrics.Count > 0)
            {
                sb.AppendLine("#");
                sb.AppendLine("# === 核心指标大盘汇总 ===");
                foreach (var kvp in projection.SummaryMetrics)
                {
                    sb.AppendLine($"# {EscapeCsvField(kvp.Key)}: {EscapeCsvField(kvp.Value)}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 从已生成的受众投影结果导出 UTF-8 BOM 字节数组 (供 Blazor WASM 下载)
        /// </summary>
        public static byte[] ExportToCsvBytes(AudienceProjectionResult projection)
        {
            string csvContent = ExportToCsv(projection);
            return Utf8WithBom.GetBytes(csvContent);
        }

        /// <summary>
        /// 从 BoqProject 实体与配置导出指定受众视角的 CSV 文本
        /// </summary>
        public static string ExportToCsv(
            BoqProject project,
            BoqReportConfig? config = null,
            ProjectCostResult? precalculatedResult = null)
        {
            ArgumentNullException.ThrowIfNull(project);
            config ??= new BoqReportConfig();

            var costResult = precalculatedResult ?? BoqAggregationEngine.CalculateProject(project);
            var projection = BoqAggregationEngine.ProjectToAudience(costResult, config.AudiencePerspective, project, config);

            return ExportToCsv(projection);
        }

        /// <summary>
        /// 从 BoqProject 实体与配置导出指定受众视角的 CSV 字节流
        /// </summary>
        public static byte[] ExportToCsvBytes(
            BoqProject project,
            BoqReportConfig? config = null,
            ProjectCostResult? precalculatedResult = null)
        {
            string csv = ExportToCsv(project, config, precalculatedResult);
            return Utf8WithBom.GetBytes(csv);
        }

        private static string EscapeCsvField(object? value)
        {
            if (value == null) return string.Empty;

            string str = value switch
            {
                double d => d.ToString("0.##", CultureInfo.InvariantCulture),
                float f => f.ToString("0.##", CultureInfo.InvariantCulture),
                decimal m => m.ToString("0.##", CultureInfo.InvariantCulture),
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                bool b => b ? "是" : "否",
                _ => value.ToString() ?? string.Empty
            };

            // RFC 4180 标准：若字段含逗号、双引号、换行，用双引号包裹且内部双引号成对转义
            if (str.Contains(',') || str.Contains('"') || str.Contains('\n') || str.Contains('\r'))
            {
                return $"\"{str.Replace("\"", "\"\"")}\"";
            }
            return str;
        }

        // =========================================================================
        // 2. SPREADSHEETML 2003 XML EXPORT API (EXCEL 原生多工作表工作簿)
        // =========================================================================

        /// <summary>
        /// 从单受众投影结果导出兼容 Excel 的单工作表 XML 电子表格文本
        /// </summary>
        public static string ExportToXmlSpreadsheet(
            AudienceProjectionResult projection,
            string? projectTitle = null)
        {
            ArgumentNullException.ThrowIfNull(projection);

            var sb = new StringBuilder();
            AppendXmlHeader(sb);

            string sheetName = CleanXmlSheetName(projection.PerspectiveName);
            AppendWorksheet(sb, sheetName, projection, projectTitle);

            AppendXmlFooter(sb);
            return sb.ToString();
        }

        /// <summary>
        /// 从单受众投影结果导出 XML 电子表格 UTF-8 字节流
        /// </summary>
        public static byte[] ExportToXmlSpreadsheetBytes(
            AudienceProjectionResult projection,
            string? projectTitle = null)
        {
            string xml = ExportToXmlSpreadsheet(projection, projectTitle);
            return Encoding.UTF8.GetBytes(xml);
        }

        // 兼容别名 API
        public static string ExportToSpreadsheetXml(AudienceProjectionResult projection, string? projectTitle = null) =>
            ExportToXmlSpreadsheet(projection, projectTitle);

        public static byte[] ExportToSpreadsheetXmlBytes(AudienceProjectionResult projection, string? projectTitle = null) =>
            ExportToXmlSpreadsheetBytes(projection, projectTitle);

        public static string ExportToExcelXml(AudienceProjectionResult projection, string? projectTitle = null) =>
            ExportToXmlSpreadsheet(projection, projectTitle);

        public static byte[] ExportToExcelXmlBytes(AudienceProjectionResult projection, string? projectTitle = null) =>
            ExportToXmlSpreadsheetBytes(projection, projectTitle);

        /// <summary>
        /// 从 BoqProject 导出单受众视角的 XML 电子表格字节流
        /// </summary>
        public static byte[] ExportToXmlSpreadsheetBytes(
            BoqProject project,
            BoqReportConfig? config = null,
            ProjectCostResult? precalculatedResult = null)
        {
            ArgumentNullException.ThrowIfNull(project);
            config ??= new BoqReportConfig();

            var costResult = precalculatedResult ?? BoqAggregationEngine.CalculateProject(project);
            var projection = BoqAggregationEngine.ProjectToAudience(costResult, config.AudiencePerspective, project, config);

            return ExportToXmlSpreadsheetBytes(projection, project.ProjectName);
        }

        /// <summary>
        /// ★ 原生多工作表工作簿合一导出：生成包含全部 5 大受众独立 Tab 标签页的 XML 电子表格文本
        /// Sheet 1: 1.业主审计报价 (Owner)
        /// Sheet 2: 2.终极全成本大盘 (Chief)
        /// Sheet 3: 3.车间制作排产 (Shop)
        /// Sheet 4: 4.现场吊装施工 (Site)
        /// Sheet 5: 5.材料采办订货 (Procure)
        /// </summary>
        public static string ExportMultiPerspectiveXmlSpreadsheet(
            BoqProject project,
            ProjectCostResult? precalculatedResult = null)
        {
            ArgumentNullException.ThrowIfNull(project);

            var costResult = precalculatedResult ?? BoqAggregationEngine.CalculateProject(project);

            var sb = new StringBuilder();
            AppendXmlHeader(sb);

            var perspectives = new[]
            {
                (BoqAudiencePerspective.Owner, "1.业主审计报价 (Owner)"),
                (BoqAudiencePerspective.ChiefEconomist, "2.终极全成本大盘 (Chief)"),
                (BoqAudiencePerspective.FabricationShop, "3.车间制作排产 (Shop)"),
                (BoqAudiencePerspective.SiteProjectDept, "4.现场吊装施工 (Site)"),
                (BoqAudiencePerspective.Procurement, "5.材料采办订货 (Procure)")
            };

            foreach (var (persp, tabName) in perspectives)
            {
                var config = new BoqReportConfig { AudiencePerspective = persp };
                var projection = BoqAggregationEngine.ProjectToAudience(costResult, persp, project, config);
                AppendWorksheet(sb, tabName, projection, $"{project.ProjectName} - {projection.PerspectiveName}");
            }

            AppendXmlFooter(sb);
            return sb.ToString();
        }

        /// <summary>
        /// ★ 原生多工作表工作簿合一导出：生成包含 5 大受众独立 Tab 标签页的 XML 电子表格 UTF-8 字节流
        /// </summary>
        public static byte[] ExportMultiPerspectiveXmlSpreadsheetBytes(
            BoqProject project,
            ProjectCostResult? precalculatedResult = null)
        {
            string xml = ExportMultiPerspectiveXmlSpreadsheet(project, precalculatedResult);
            return Encoding.UTF8.GetBytes(xml);
        }

        // -------------------------------------------------------------------------
        // SPREADSHEETML 2003 XML 结构构建辅助
        // -------------------------------------------------------------------------

        private static void AppendXmlHeader(StringBuilder sb)
        {
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
            sb.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
            sb.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
            sb.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");

            // 样式定义
            sb.AppendLine(" <Styles>");
            sb.AppendLine("  <Style ss:ID=\"Default\" ss:Name=\"Normal\">");
            sb.AppendLine("   <Alignment ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <Font ss:FontName=\"微软雅黑\" ss:Size=\"10\" ss:Color=\"#333333\"/>");
            sb.AppendLine("  </Style>");

            // 表头样式 (钢结构商务深蓝底色，白色加粗字体)
            sb.AppendLine("  <Style ss:ID=\"HeaderStyle\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\" ss:WrapText=\"1\"/>");
            sb.AppendLine("   <Borders>");
            sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/>");
            sb.AppendLine("    <Border ss:Position=\"Left\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/>");
            sb.AppendLine("    <Border ss:Position=\"Right\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/>");
            sb.AppendLine("    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#B0C4DE\"/>");
            sb.AppendLine("   </Borders>");
            sb.AppendLine("   <Font ss:FontName=\"微软雅黑\" ss:Size=\"10\" ss:Color=\"#FFFFFF\" ss:Bold=\"1\"/>");
            sb.AppendLine("   <Interior ss:Color=\"#1F4E79\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("  </Style>");

            // 数据行基础文本样式
            sb.AppendLine("  <Style ss:ID=\"DataText\">");
            sb.AppendLine("   <Alignment ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#E5E5E5\"/></Borders>");
            sb.AppendLine("   <Font ss:FontName=\"微软雅黑\" ss:Size=\"10\" ss:Color=\"#333333\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine("  <Style ss:ID=\"DataCenter\" ss:Parent=\"DataText\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Center\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine("  <Style ss:ID=\"DataNumber\" ss:Parent=\"DataText\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.00\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine("  <Style ss:ID=\"DataWeight\" ss:Parent=\"DataText\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.000\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine("  <Style ss:ID=\"DataPercent\" ss:Parent=\"DataText\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <NumberFormat ss:Format=\"0.0%\"/>");
            sb.AppendLine("  </Style>");

            // 独立费率覆盖高亮行 (浅金黄背景)
            sb.AppendLine("  <Style ss:ID=\"HighlightRow\">");
            sb.AppendLine("   <Alignment ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <Borders><Border ss:Position=\"Bottom\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#D4AF37\"/></Borders>");
            sb.AppendLine("   <Font ss:FontName=\"微软雅黑\" ss:Size=\"10\" ss:Color=\"#8A6D3B\" ss:Bold=\"1\"/>");
            sb.AppendLine("   <Interior ss:Color=\"#FFF2CC\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("  </Style>");

            // 指标卡片与合计汇总样式 (顶部单细线，底部双底线，浅米黄底色)
            sb.AppendLine("  <Style ss:ID=\"SummaryStyle\">");
            sb.AppendLine("   <Alignment ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <Borders>");
            sb.AppendLine("    <Border ss:Position=\"Top\" ss:LineStyle=\"Continuous\" ss:Weight=\"1\" ss:Color=\"#000000\"/>");
            sb.AppendLine("    <Border ss:Position=\"Bottom\" ss:LineStyle=\"Double\" ss:Weight=\"3\" ss:Color=\"#000000\"/>");
            sb.AppendLine("   </Borders>");
            sb.AppendLine("   <Font ss:FontName=\"微软雅黑\" ss:Size=\"10\" ss:Color=\"#000000\" ss:Bold=\"1\"/>");
            sb.AppendLine("   <Interior ss:Color=\"#FFFDE8\" ss:Pattern=\"Solid\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine("  <Style ss:ID=\"SummaryNumber\" ss:Parent=\"SummaryStyle\">");
            sb.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
            sb.AppendLine("   <NumberFormat ss:Format=\"#,##0.00\"/>");
            sb.AppendLine("  </Style>");

            sb.AppendLine(" </Styles>");
        }

        private static void AppendWorksheet(
            StringBuilder sb,
            string sheetName,
            AudienceProjectionResult projection,
            string? projectTitle)
        {
            sb.AppendLine($" <Worksheet ss:Name=\"{EscapeXml(sheetName)}\">");
            sb.AppendLine("  <Table>");

            // 列宽适配定义
            foreach (var header in projection.Headers)
            {
                double width = GetEstimatedColumnWidth(header);
                sb.AppendLine($"   <Column ss:Width=\"{width}\"/>");
            }

            // 大标题行
            if (!string.IsNullOrEmpty(projectTitle))
            {
                sb.AppendLine("   <Row ss:Height=\"30\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"{Math.Max(0, projection.Headers.Count - 1)}\"><Data ss:Type=\"String\">{EscapeXml(projectTitle)}</Data></Cell>");
                sb.AppendLine("   </Row>");
            }

            // 表头行
            sb.AppendLine("   <Row ss:Height=\"26\">");
            foreach (var header in projection.Headers)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"HeaderStyle\"><Data ss:Type=\"String\">{EscapeXml(header)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");

            // 数据行
            foreach (var row in projection.Rows)
            {
                bool isCustomOverride = row.Values.Any(v => v?.ToString()?.Contains("[覆盖]") == true || v?.ToString()?.Contains("★") == true);
                string rowStyle = isCustomOverride ? "HighlightRow" : "DataText";

                sb.AppendLine("   <Row ss:Height=\"20\">");
                foreach (var header in projection.Headers)
                {
                    row.TryGetValue(header, out var cellValue);
                    AppendXmlCell(sb, cellValue, header, rowStyle);
                }
                sb.AppendLine("   </Row>");
            }

            // 核心指标汇总卡片 (如果在单 sheet 底部展示)
            if (projection.SummaryMetrics.Count > 0)
            {
                sb.AppendLine("   <Row ss:Height=\"15\"></Row>"); // 空行分隔
                sb.AppendLine("   <Row ss:Height=\"24\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"{Math.Max(0, Math.Min(3, projection.Headers.Count - 1))}\" ss:StyleID=\"SummaryStyle\"><Data ss:Type=\"String\">【核心指标大盘汇总】</Data></Cell>");
                sb.AppendLine("   </Row>");

                foreach (var kvp in projection.SummaryMetrics)
                {
                    sb.AppendLine("   <Row ss:Height=\"20\">");
                    sb.AppendLine($"    <Cell ss:StyleID=\"DataText\"><Data ss:Type=\"String\">{EscapeXml(kvp.Key)}</Data></Cell>");
                    AppendXmlCell(sb, kvp.Value, kvp.Key, "DataText");
                    sb.AppendLine("   </Row>");
                }
            }

            sb.AppendLine("  </Table>");
            sb.AppendLine(" </Worksheet>");
        }

        private static void AppendXmlCell(StringBuilder sb, object? val, string header, string defaultStyle)
        {
            if (val == null)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"{defaultStyle}\"><Data ss:Type=\"String\"></Data></Cell>");
                return;
            }

            if (val is double d)
            {
                string style = header.Contains("率") ? "DataPercent"
                    : (header.Contains("净重") || header.Contains("毛重") || header.Contains("吨位") ? "DataWeight" : "DataNumber");
                sb.AppendLine($"    <Cell ss:StyleID=\"{style}\"><Data ss:Type=\"Number\">{d.ToString(CultureInfo.InvariantCulture)}</Data></Cell>");
            }
            else if (val is int i)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"DataCenter\"><Data ss:Type=\"Number\">{i}</Data></Cell>");
            }
            else if (val is bool b)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"DataCenter\"><Data ss:Type=\"String\">{(b ? "是" : "否")}</Data></Cell>");
            }
            else
            {
                string str = val.ToString() ?? "";
                string cellStyle = defaultStyle;
                if (str.Contains("[覆盖]") || str.Contains("★")) cellStyle = "HighlightRow";
                sb.AppendLine($"    <Cell ss:StyleID=\"{cellStyle}\"><Data ss:Type=\"String\">{EscapeXml(str)}</Data></Cell>");
            }
        }

        private static void AppendXmlFooter(StringBuilder sb)
        {
            sb.AppendLine("</Workbook>");
        }

        private static string EscapeXml(string? value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return SecurityElement.Escape(value) ?? string.Empty;
        }

        private static string CleanXmlSheetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Sheet1";
            // Excel 工作表名最多 31 字符，且不能包含 : \ / ? * [ ]
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            string cleaned = new(name.Where(c => !invalidChars.Contains(c)).ToArray());
            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }

        private static double GetEstimatedColumnWidth(string header)
        {
            if (header.Contains("名称") || header.Contains("描述") || header.Contains("规格") || header.Contains("工法") || header.Contains("说明") || header.Contains("建议")) return 180;
            if (header.Contains("编码") || header.Contains("工费") || header.Contains("总额") || header.Contains("合价") || header.Contains("小计")) return 115;
            if (header.Contains("单价") || header.Contains("毛重") || header.Contains("净重") || header.Contains("工程量")) return 100;
            if (header.Contains("序号") || header.Contains("单位") || header.Contains("代号") || header.Contains("套数") || header.Contains("件数")) return 60;
            return 95;
        }

        // =========================================================================
        // 3. EXCEL-COMPATIBLE HTML TABLE EXPORT API
        // =========================================================================

        /// <summary>
        /// 导出包含 Excel MSO 样式属性的 HTML 电子表格文本 (可在浏览器预览或由 Excel 打开)
        /// </summary>
        public static string ExportToHtml(AudienceProjectionResult projection)
        {
            ArgumentNullException.ThrowIfNull(projection);

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset=\"utf-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine("table { border-collapse: collapse; font-family: 微软雅黑, sans-serif; font-size: 13px; width: 100%; }");
            sb.AppendLine("th { background-color: #1F4E79; color: white; padding: 8px 12px; border: 1px solid #ccc; text-align: center; }");
            sb.AppendLine("td { padding: 6px 10px; border: 1px solid #ddd; }");
            sb.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            sb.AppendLine(".num { text-align: right; mso-number-format: '\\#\\,\\#\\#0\\.00'; }");
            sb.AppendLine(".weight { text-align: right; mso-number-format: '\\#\\,\\#\\#0\\.000'; }");
            sb.AppendLine(".center { text-align: center; }");
            sb.AppendLine(".highlight { background-color: #FFF2CC !important; font-weight: bold; color: #8A6D3B; }");
            sb.AppendLine(".summary-card { margin-top: 20px; border: 1px solid #1F4E79; padding: 15px; background-color: #FFFDE8; }");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine($"<h2>{EscapeXml(projection.PerspectiveName)}</h2>");
            sb.AppendLine("<table><thead><tr>");
            foreach (var h in projection.Headers)
            {
                sb.AppendLine($"<th>{EscapeXml(h)}</th>");
            }
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var row in projection.Rows)
            {
                bool isHighlight = row.Values.Any(v => v?.ToString()?.Contains("[覆盖]") == true || v?.ToString()?.Contains("★") == true);
                string trClass = isHighlight ? " class=\"highlight\"" : "";
                sb.AppendLine($"<tr{trClass}>");
                foreach (var h in projection.Headers)
                {
                    row.TryGetValue(h, out var v);
                    string tdClass = v is double dVal ? (h.Contains("净重") || h.Contains("毛重") || h.Contains("吨位") ? " class=\"weight\"" : " class=\"num\"")
                                   : (v is int ? " class=\"center\"" : "");
                    string cellText = v switch
                    {
                        double d => d.ToString(h.Contains("净重") || h.Contains("毛重") || h.Contains("吨位") ? "#,##0.000" : "#,##0.00", CultureInfo.InvariantCulture),
                        _ => EscapeXml(v?.ToString() ?? "")
                    };
                    sb.AppendLine($"<td{tdClass}>{cellText}</td>");
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</tbody></table>");

            // 底部核心指标大盘卡片
            if (projection.SummaryMetrics.Count > 0)
            {
                sb.AppendLine("<div class=\"summary-card\">");
                sb.AppendLine("<h3>核心指标大盘汇总</h3><ul>");
                foreach (var kvp in projection.SummaryMetrics)
                {
                    sb.AppendLine($"<li><strong>{EscapeXml(kvp.Key)}:</strong> {EscapeXml(kvp.Value?.ToString() ?? "")}</li>");
                }
                sb.AppendLine("</ul></div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        public static byte[] ExportToHtmlBytes(AudienceProjectionResult projection)
        {
            return Utf8WithBom.GetBytes(ExportToHtml(projection));
        }

        public static string ExportToHtmlSpreadsheet(AudienceProjectionResult projection) => ExportToHtml(projection);
        public static byte[] ExportToHtmlSpreadsheetBytes(AudienceProjectionResult projection) => ExportToHtmlBytes(projection);

        // =========================================================================
        // 4. 文件名生成与 MIME 类型辅助工具 (Metadata Utilities)
        // =========================================================================

        /// <summary>
        /// 根据项目代号与受众视角生成标准化中文文件名 (如 "NB-ATZX_业主审计对外清单_20261108.csv")
        /// </summary>
        public static string GetRecommendedFileName(
            BoqProject project,
            BoqAudiencePerspective perspective,
            string extension = "csv")
        {
            string pName = perspective switch
            {
                BoqAudiencePerspective.Owner => "业主审计对外清单",
                BoqAudiencePerspective.ChiefEconomist => "终极全成本穿透大盘",
                BoqAudiencePerspective.FabricationShop => "车间制作工艺排产表",
                BoqAudiencePerspective.SiteProjectDept => "现场安装吊装施工表",
                BoqAudiencePerspective.Procurement => "采办物资钢厂订货表",
                _ => "工程量清单报表"
            };
            string cleanCode = string.IsNullOrWhiteSpace(project.ProjectCode) ? "BOQ" : project.ProjectCode;
            return $"{cleanCode}_{pName}_{DateTime.Now:yyyyMMdd}.{extension.TrimStart('.')}";
        }

        /// <summary>
        /// 获取全视角多工作表工作簿建议文件名 (如 "NB-ATZX_多视角全要素商务工程量清单_20261108.xml")
        /// </summary>
        public static string GetMultiSheetRecommendedFileName(
            BoqProject project,
            string extension = "xml")
        {
            string cleanCode = string.IsNullOrWhiteSpace(project.ProjectCode) ? "BOQ" : project.ProjectCode;
            return $"{cleanCode}_多视角全要素商务工程量清单_{DateTime.Now:yyyyMMdd}.{extension.TrimStart('.')}";
        }

        /// <summary>
        /// 获取扩展名对应的标准 MIME Content-Type
        /// </summary>
        public static string GetContentType(string extension)
        {
            return extension.TrimStart('.').ToLowerInvariant() switch
            {
                "csv" => "text/csv; charset=utf-8",
                "xml" or "xls" => "application/vnd.ms-excel",
                "html" => "text/html; charset=utf-8",
                _ => "application/octet-stream"
            };
        }
    }
}
