using EngineeringApp.Shared.CostEstimation;
using EngineeringApp.Shared.Pricing;
using EngineeringApp.Shared.Quota;

namespace EngineeringApp.Shared.QuotationExport;

/// <summary>
/// 商业报价清单报表生成器
/// 将成本测算表与各构件组价结果映射转换为对外的国标清单或投标报价单
/// </summary>
public static class QuotationReportGenerator
{
    /// <summary>
    /// 将构件组价结果与最终成本测算表整合成对外的商业投标报价成果报告
    /// </summary>
    public static CommercialBidSummary Generate(
        ProjectCostSheet costSheet,
        IEnumerable<MemberPricingResult> members)
    {
        ArgumentNullException.ThrowIfNull(costSheet);
        ArgumentNullException.ThrowIfNull(members);

        var summary = new CommercialBidSummary
        {
            ProjectName = costSheet.ProjectName,
            QuotationDate = costSheet.CreatedDate
        };

        // 按 MemberRole 分类汇总构件工程量
        var groups = members.GroupBy(m => m.Role).OrderBy(g => g.Key);
        int itemIndex = 1;

        // 计算分摊比率 (将措施费、管理费、运费、税金及利润根据各构件直接工程费比重进行精准分摊)
        double totalMemberDirectCost = Math.Max(1.0, members.Sum(m => m.TotalDirectCost));
        double totalQuotationAmount = costSheet.FinalBidQuotation;

        foreach (var group in groups)
        {
            var role = group.Key;
            double groupNetTon = group.Sum(m => m.TotalNetWeightTon);
            double groupDirectCost = group.Sum(m => m.TotalDirectCost);

            // 按构件直接费比重分摊项目总报价
            double costRatio = groupDirectCost / totalMemberDirectCost;
            double allocatedTotalQuotation = totalQuotationAmount * costRatio;
            double groupUnitPrice = groupNetTon > 0 ? allocatedTotalQuotation / groupNetTon : 0;

            var billItem = new QuotationBillItem
            {
                ItemNumber = itemIndex++,
                BillCode = role.GetStandardBillCode(),
                BillName = role.GetDisplayName(),
                ItemDescription = $"包含 {string.Join("、", group.Select(m => m.AssemblyMark).Distinct())}；含主材、制作加工、除锈喷砂及涂装、运输与安装",
                Unit = "t",
                Quantity = groupNetTon,
                UnitPrice = groupUnitPrice
            };

            summary.Items.Add(billItem);
        }

        return summary;
    }
}
