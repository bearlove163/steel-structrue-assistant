namespace EngineeringApp.Shared.QuotationExport;

/// <summary>
/// 对外国标工程量清单或商业报价单明细项 (GB 50500 标准清单规范)
/// </summary>
public class QuotationBillItem
{
    public int ItemNumber { get; set; } = 1;

    /// <summary>清单项目编码 (如 010605001001 钢柱)</summary>
    public string BillCode { get; set; } = "010605001001";

    /// <summary>项目名称 (如 框架钢柱、钢梁、钢支撑)</summary>
    public string BillName { get; set; } = "框架钢柱";

    /// <summary>项目特征描述 (材质、截面规格、除锈等级、涂料体系、安装标高)</summary>
    public string ItemDescription { get; set; } = "";

    /// <summary>计量单位 (钢结构工程标准为 "t" 或 "m²")</summary>
    public string Unit { get; set; } = "t";

    /// <summary>工程数量 (净重工程量 吨 t)</summary>
    public double Quantity { get; set; }

    /// <summary>综合单价 (含税全费用综合单价 元/t)</summary>
    public double UnitPrice { get; set; }

    /// <summary>合价金额 (元)</summary>
    public double TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// 商业投标报价总表与成果报告
/// </summary>
public class CommercialBidSummary
{
    public string ProjectName { get; set; } = "";
    public string BidderCompany { get; set; } = "钢结构制造工程有限公司";
    public DateTime QuotationDate { get; set; } = DateTime.Now;

    public List<QuotationBillItem> Items { get; set; } = [];

    /// <summary>总净重 (吨 t)</summary>
    public double TotalNetWeightTon => Items.Sum(i => i.Quantity);

    /// <summary>总报价合计金额 (元)</summary>
    public double TotalQuotationAmount => Items.Sum(i => i.TotalPrice);

    /// <summary>平均吨钢综合单价 (元/t)</summary>
    public double AverageUnitPricePerTon => TotalNetWeightTon > 0 ? TotalQuotationAmount / TotalNetWeightTon : 0;
}
