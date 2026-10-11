namespace EngineeringApp.Shared.Pricing.Coating;

using EngineeringApp.Shared.Pricing.Common;

/// <summary>
/// 预置涂装防腐防火多时期价格快照与品牌询价数据库
/// 同时作为 IBrandPricingProvider 接口的默认注入实现。
/// </summary>
public static class SeedPaintPriceSnapshots
{
    private static readonly List<PaintPriceSnapshot> _snapshots =
    [
        // ================= 快照 1: 2026年10月现行大盘信息价 (当前执行) =================
        new PaintPriceSnapshot
        {
            SnapshotId = "PAINT-SNAP-202610-LATEST",
            SnapshotName = "2026年10月上旬-华东主流涂装信息价 (当前执行)",
            EffectiveDate = new DateTime(2026, 10, 8),
            SourceType = PaintPriceSourceType.MarketWebIndex,
            SourceSupplier = "华东钢构供应链涂装集采大盘采样",
            IsTaxInclusive = true,
            IsLocked = false,
            Remarks = "秋季施工旺季涂料价格平稳，环氧树脂价格小幅回调",
            EpoxyZincRichPrimerBase = 38.0,
            InorganicZincSilicateBase = 32.0,
            AlkydPrimerBase = 18.0,
            EpoxyMioIntermediateBase = 26.0,
            PolyurethaneTopcoatBase = 42.0,
            FluorocarbonTopcoatBase = 95.0,
            ThinFireproofBase = 22.0,
            ThickFireproofBase = 3.6,
            ZincIngotBasePerTon = 23000.0,
            GalvanizingProcessingPerTon = 1250.0,
            AirlessSprayingLaborBase = 16.0,
            BrushRollerLaborBase = 26.0,
            BrandQuotations =
            [
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "底漆", ProductModel = "Penguard Primer 环氧富锌", UnitPrice = 43.0, SupplierName = "佐敦涂料(张家港)直接报价", QuoteDate = new DateTime(2026, 10, 5) },
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "中间漆", ProductModel = "Penguard MIO 环氧云铁", UnitPrice = 29.0, SupplierName = "佐敦涂料(张家港)直接报价", QuoteDate = new DateTime(2026, 10, 5) },
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "面漆", ProductModel = "Hardtop XP 脂肪族聚氨酯", UnitPrice = 48.0, SupplierName = "佐敦涂料(张家港)直接报价", QuoteDate = new DateTime(2026, 10, 5) },
                new BrandQuoteItem { BrandName = "阿克苏诺贝尔", MaterialCategory = "底漆", ProductModel = "Intergard 251 环氧富锌", UnitPrice = 44.0, SupplierName = "国际油漆(International)华东总代", QuoteDate = new DateTime(2026, 10, 6) },
                new BrandQuoteItem { BrandName = "阿克苏诺贝尔", MaterialCategory = "面漆", ProductModel = "Interthane 990 聚氨酯面漆", UnitPrice = 49.0, SupplierName = "国际油漆(International)华东总代", QuoteDate = new DateTime(2026, 10, 6) },
                new BrandQuoteItem { BrandName = "海虹老人 Hempel", MaterialCategory = "底漆", ProductModel = "Hempadur Zinc 17360", UnitPrice = 42.5, SupplierName = "海虹老人涂料昆山直供", QuoteDate = new DateTime(2026, 10, 6) },
                new BrandQuoteItem { BrandName = "中远关西", MaterialCategory = "底漆", ProductModel = "SD锌霸 环氧富锌底漆", UnitPrice = 36.0, SupplierName = "中远关西涂料上海协议商", QuoteDate = new DateTime(2026, 10, 7) },
                new BrandQuoteItem { BrandName = "中远关西", MaterialCategory = "面漆", ProductModel = "Retan 6000 聚氨酯面漆", UnitPrice = 39.0, SupplierName = "中远关西涂料上海协议商", QuoteDate = new DateTime(2026, 10, 7) },
                new BrandQuoteItem { BrandName = "双虎涂料", MaterialCategory = "底漆", ProductModel = "SH-70 工业重防腐底漆", UnitPrice = 31.0, SupplierName = "武汉双虎涂料直发经销单", QuoteDate = new DateTime(2026, 10, 8) }
            ]
        },

        // ================= 快照 2: 2026年07月夏季原料上涨期 =================
        new PaintPriceSnapshot
        {
            SnapshotId = "PAINT-SNAP-202607-SUMMER",
            SnapshotName = "2026年07月上旬-夏季原材料上涨高峰期",
            EffectiveDate = new DateTime(2026, 7, 1),
            SourceType = PaintPriceSourceType.ManufacturerDirect,
            SourceSupplier = "主要防腐涂料厂商夏季联合调价通知函",
            IsTaxInclusive = true,
            IsLocked = false,
            Remarks = "环氧树脂和高纯锌粉价格涨幅显著，锌锭达24500元/吨",
            EpoxyZincRichPrimerBase = 42.0,
            InorganicZincSilicateBase = 35.0,
            AlkydPrimerBase = 19.5,
            EpoxyMioIntermediateBase = 28.5,
            PolyurethaneTopcoatBase = 45.0,
            FluorocarbonTopcoatBase = 105.0,
            ThinFireproofBase = 24.0,
            ThickFireproofBase = 3.8,
            ZincIngotBasePerTon = 24500.0,
            GalvanizingProcessingPerTon = 1350.0,
            AirlessSprayingLaborBase = 17.5,
            BrushRollerLaborBase = 28.0,
            BrandQuotations =
            [
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "底漆", ProductModel = "Penguard Primer 环氧富锌", UnitPrice = 47.0, SupplierName = "佐敦涂料调价公函", QuoteDate = new DateTime(2026, 7, 1) },
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "面漆", ProductModel = "Hardtop XP 脂肪族聚氨酯", UnitPrice = 52.0, SupplierName = "佐敦涂料调价公函", QuoteDate = new DateTime(2026, 7, 1) },
                new BrandQuoteItem { BrandName = "阿克苏诺贝尔", MaterialCategory = "底漆", ProductModel = "Intergard 251", UnitPrice = 48.0, SupplierName = "国际油漆夏季通函", QuoteDate = new DateTime(2026, 7, 1) },
                new BrandQuoteItem { BrandName = "中远关西", MaterialCategory = "底漆", ProductModel = "SD锌霸 环氧富锌底漆", UnitPrice = 39.0, SupplierName = "中远关西通函", QuoteDate = new DateTime(2026, 7, 1) }
            ]
        },

        // ================= 快照 3: 2026年03月投标定标锁价基期 (基准快照，已锁定) =================
        new PaintPriceSnapshot
        {
            SnapshotId = "PAINT-SNAP-202603-BASE",
            SnapshotName = "2026年03月中旬-重大工程投标锁价基期 (已锁定基准)",
            EffectiveDate = new DateTime(2026, 3, 15),
            SourceType = PaintPriceSourceType.ContractLockedBaseline,
            SourceSupplier = "工程项目投标澄清与定标合同清包单价",
            IsTaxInclusive = true,
            IsLocked = true,
            Remarks = "项目商务标底锁定调差基准日，已冻结锁定，用于竣工结算调差",
            EpoxyZincRichPrimerBase = 34.0,
            InorganicZincSilicateBase = 29.0,
            AlkydPrimerBase = 16.5,
            EpoxyMioIntermediateBase = 23.0,
            PolyurethaneTopcoatBase = 38.0,
            FluorocarbonTopcoatBase = 88.0,
            ThinFireproofBase = 19.5,
            ThickFireproofBase = 3.2,
            ZincIngotBasePerTon = 21500.0,
            GalvanizingProcessingPerTon = 1180.0,
            AirlessSprayingLaborBase = 14.5,
            BrushRollerLaborBase = 23.0,
            BrandQuotations =
            [
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "底漆", ProductModel = "Penguard Primer 环氧富锌", UnitPrice = 39.0, SupplierName = "投标入围锁价函", QuoteDate = new DateTime(2026, 3, 15) },
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "中间漆", ProductModel = "Penguard MIO 环氧云铁", UnitPrice = 25.5, SupplierName = "投标入围锁价函", QuoteDate = new DateTime(2026, 3, 15) },
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "面漆", ProductModel = "Hardtop XP 脂肪族聚氨酯", UnitPrice = 43.0, SupplierName = "投标入围锁价函", QuoteDate = new DateTime(2026, 3, 15) },
                new BrandQuoteItem { BrandName = "阿克苏诺贝尔", MaterialCategory = "底漆", ProductModel = "Intergard 251", UnitPrice = 40.0, SupplierName = "投标入围锁价函", QuoteDate = new DateTime(2026, 3, 15) },
                new BrandQuoteItem { BrandName = "中远关西", MaterialCategory = "底漆", ProductModel = "SD锌霸 环氧富锌底漆", UnitPrice = 32.5, SupplierName = "投标协议锁价单", QuoteDate = new DateTime(2026, 3, 15) },
                new BrandQuoteItem { BrandName = "双虎涂料", MaterialCategory = "底漆", ProductModel = "SH-70 工业重防腐底漆", UnitPrice = 28.0, SupplierName = "定额集采基期单", QuoteDate = new DateTime(2026, 3, 15) }
            ]
        },

        // ================= 快照 4: 2025年11月年度框架集采协议价 =================
        new PaintPriceSnapshot
        {
            SnapshotId = "PAINT-SNAP-202511-ANNUAL",
            SnapshotName = "2025年11月下旬-集团年度战略集采框架价",
            EffectiveDate = new DateTime(2025, 11, 20),
            SourceType = PaintPriceSourceType.CorporateInternalRate,
            SourceSupplier = "钢结构制造基地年度涂料招标中标协议",
            IsTaxInclusive = true,
            IsLocked = false,
            Remarks = "万吨级年采购量大宗集采协议，享受厂家直供最优惠折扣",
            EpoxyZincRichPrimerBase = 32.0,
            InorganicZincSilicateBase = 27.5,
            AlkydPrimerBase = 15.0,
            EpoxyMioIntermediateBase = 21.5,
            PolyurethaneTopcoatBase = 36.0,
            FluorocarbonTopcoatBase = 82.0,
            ThinFireproofBase = 18.0,
            ThickFireproofBase = 3.0,
            ZincIngotBasePerTon = 20800.0,
            GalvanizingProcessingPerTon = 1100.0,
            AirlessSprayingLaborBase = 13.5,
            BrushRollerLaborBase = 21.0,
            BrandQuotations =
            [
                new BrandQuoteItem { BrandName = "佐敦 Jotun", MaterialCategory = "底漆", ProductModel = "Penguard Primer 环氧富锌", UnitPrice = 36.5, SupplierName = "年度集采协议", QuoteDate = new DateTime(2025, 11, 20) },
                new BrandQuoteItem { BrandName = "阿克苏诺贝尔", MaterialCategory = "底漆", ProductModel = "Intergard 251", UnitPrice = 37.5, SupplierName = "年度集采协议", QuoteDate = new DateTime(2025, 11, 20) },
                new BrandQuoteItem { BrandName = "中远关西", MaterialCategory = "底漆", ProductModel = "SD锌霸 环氧富锌底漆", UnitPrice = 30.0, SupplierName = "年度集采协议", QuoteDate = new DateTime(2025, 11, 20) },
                new BrandQuoteItem { BrandName = "双虎涂料", MaterialCategory = "底漆", ProductModel = "SH-70 工业重防腐底漆", UnitPrice = 26.5, SupplierName = "年度集采协议", QuoteDate = new DateTime(2025, 11, 20) }
            ]
        }
    ];

    public static List<PaintPriceSnapshot> AllSnapshots => _snapshots;

    public static PaintPriceSnapshot Latest => _snapshots[0];

    public static PaintPriceSnapshot BaseSnapshot => _snapshots.FirstOrDefault(s => s.IsLocked) ?? _snapshots[2];

    public static PaintPriceSnapshot GetById(string id) =>
        _snapshots.FirstOrDefault(s => string.Equals(s.SnapshotId, id, StringComparison.OrdinalIgnoreCase)) ?? Latest;

    /// <summary>
    /// 向指定快照（默认向当前快照）录入一条新品牌询价记录
    /// </summary>
    public static void RegisterBrandQuote(string snapshotId, BrandQuoteItem quote)
    {
        var target = GetById(snapshotId);
        target.AddOrUpdateBrandQuote(quote);
    }
}
