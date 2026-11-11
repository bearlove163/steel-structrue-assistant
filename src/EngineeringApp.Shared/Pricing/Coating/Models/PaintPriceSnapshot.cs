namespace EngineeringApp.Shared.Pricing.Coating;

/// <summary>
/// 涂装防腐与防火价格历史基准快照 (含时间标签、通用品类寄存基价与品牌实体询价库)
/// </summary>
public class PaintPriceSnapshot : EngineeringApp.Shared.Common.Temporal.IVersionSnapshot<PaintPriceSnapshot>
{
    public string SnapshotId { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string SnapshotName { get; set; } = "2026年10月上旬-大盘涂装信息价";
    public string EntityCategory { get; set; } = "PaintCoating";
    public DateTime RecordedAt { get; set; } = DateTime.Now;
    public string Operator { get; set; } = "防腐造价工程师";
    public EngineeringApp.Shared.Material.Pricing.StandardSystem Standard { get; set; } = EngineeringApp.Shared.Material.Pricing.StandardSystem.GB;
    public PaintPriceSnapshot StateData { get => this; set { } }

    /// <summary>价格基准生效日期 (时间标签)</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    /// <summary>数据来源分类</summary>
    public PaintPriceSourceType SourceType { get; set; } = PaintPriceSourceType.MarketWebIndex;

    /// <summary>来源机构/供应商说明</summary>
    public string SourceSupplier { get; set; } = "华东钢结构工程造价信息大盘采样";

    /// <summary>是否含 13% 增值税</summary>
    public bool IsTaxInclusive { get; set; } = true;

    /// <summary>是否已锁定为合同定标基期 (锁定后作为调差基准日，不参与自动覆盖)</summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>快照备注说明</summary>
    public string Remarks { get; set; } = "";

    // ================= 1. 寄存通用品类大盘基准价 (作为行业定额与大盘参考，元/kg) =================
    /// <summary>环氧富锌底漆 (70%含锌量) 基准价</summary>
    public double EpoxyZincRichPrimerBase { get; set; } = 38.0;

    /// <summary>无机富锌底漆 (醇溶性/水性) 基准价</summary>
    public double InorganicZincSilicateBase { get; set; } = 32.0;

    /// <summary>醇酸防锈底漆 (普碳钢常规防锈) 基准价</summary>
    public double AlkydPrimerBase { get; set; } = 18.0;

    /// <summary>环氧云铁厚浆中间漆 (MIO) 基准价</summary>
    public double EpoxyMioIntermediateBase { get; set; } = 26.0;

    /// <summary>丙烯酸脂肪族聚氨酯面漆 基准价</summary>
    public double PolyurethaneTopcoatBase { get; set; } = 42.0;

    /// <summary>氟碳树脂高耐候面漆 基准价</summary>
    public double FluorocarbonTopcoatBase { get; set; } = 95.0;

    /// <summary>室内膨胀型超薄/薄型钢结构防火涂料 (耐火 1.0~2.5h) 基准价</summary>
    public double ThinFireproofBase { get; set; } = 22.0;

    /// <summary>室内非膨胀型厚型防火涂料 (耐火 2.0~3.0h) 基准价</summary>
    public double ThickFireproofBase { get; set; } = 3.6;

    /// <summary>热浸镀锌 1# 优质锌锭上海有色网基准价 (元/吨)</summary>
    public double ZincIngotBasePerTon { get; set; } = 23000.0;

    /// <summary>浸锌酸洗钝化、助镀剂与加热能耗加工工费基准 (元/吨钢)</summary>
    public double GalvanizingProcessingPerTon { get; set; } = 1250.0;

    // ================= 施工工费基准 (元/m²) =================
    /// <summary>高压无气喷涂施工工费基准</summary>
    public double AirlessSprayingLaborBase { get; set; } = 16.0;

    /// <summary>手工刷涂 / 滚涂施工工费基准</summary>
    public double BrushRollerLaborBase { get; set; } = 26.0;

    // ================= 2. 品牌实体真实询报价明细库 (随快照版本保存) =================
    /// <summary>该快照周期内记录的具体品牌真实询报价列表</summary>
    public List<BrandQuoteItem> BrandQuotations { get; set; } = [];

    /// <summary>
    /// 获取指定品牌与品类的具体单价。
    /// 优先匹配真实品牌询价明细；若该品牌尚未录入，则平滑降级采用寄存品类基准价。
    /// </summary>
    public double GetPriceForBrandOrCategory(string brandName, string category)
    {
        if (!string.IsNullOrWhiteSpace(brandName))
        {
            var matchedQuote = BrandQuotations.FirstOrDefault(q =>
                (q.BrandName.Contains(brandName, StringComparison.OrdinalIgnoreCase) ||
                 brandName.Contains(q.BrandName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(category) ||
                 q.MaterialCategory.Contains(category, StringComparison.OrdinalIgnoreCase) ||
                 category.Contains(q.MaterialCategory, StringComparison.OrdinalIgnoreCase)));

            if (matchedQuote != null && matchedQuote.UnitPrice > 0)
            {
                return matchedQuote.UnitPrice;
            }
        }

        return category switch
        {
            "底漆" => EpoxyZincRichPrimerBase,
            "中间漆" => EpoxyMioIntermediateBase,
            "面漆" => PolyurethaneTopcoatBase,
            "防火涂料" => ThinFireproofBase,
            "厚型防火" => ThickFireproofBase,
            _ => EpoxyZincRichPrimerBase
        };
    }

    /// <summary>
    /// 动态添加或更新一条品牌真实询价记录
    /// </summary>
    public void AddOrUpdateBrandQuote(BrandQuoteItem quote)
    {
        ArgumentNullException.ThrowIfNull(quote);

        var existing = BrandQuotations.FirstOrDefault(q =>
            string.Equals(q.BrandName, quote.BrandName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(q.MaterialCategory, quote.MaterialCategory, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.UnitPrice = quote.UnitPrice;
            existing.ProductModel = quote.ProductModel;
            existing.SupplierName = quote.SupplierName;
            existing.QuoteDate = quote.QuoteDate;
            existing.Remarks = quote.Remarks;
        }
        else
        {
            BrandQuotations.Add(quote);
        }
    }
}
