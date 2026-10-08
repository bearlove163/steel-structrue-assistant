namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 全国及国际常用钢厂品牌与产地基地基准数据
/// </summary>
public static class SeedSteelMills
{
    private static readonly List<SteelMillBrand> _mills =
    [
        new SteelMillBrand
        {
            Id = "Baosteel",
            Name = "宝钢股份 (宝武集团)",
            ShortName = "宝钢",
            ProductionBase = "上海宝山 / 广东湛江",
            Tier = 1,
            BrandPremiumPerTon = 200.0,
            SpecialtyDescription = "中厚板极品、TMCP高强控轧、Z向特厚板、重大重点公建指定名录",
            SupportsThirdPartyCertification = true,
            IsRecommended = true
        },
        new SteelMillBrand
        {
            Id = "NISCO",
            Name = "南钢 (南京钢铁)",
            ShortName = "南钢",
            ProductionBase = "江苏南京",
            Tier = 1,
            BrandPremiumPerTon = 150.0,
            SpecialtyDescription = "国内中厚板特钢龙头、探伤保级能力极强、高建钢与耐候钢标杆",
            SupportsThirdPartyCertification = true,
            IsRecommended = true
        },
        new SteelMillBrand
        {
            Id = "Jinxi",
            Name = "津西钢铁 (河北津西)",
            ShortName = "津西型钢",
            ProductionBase = "河北唐山迁西",
            Tier = 2,
            BrandPremiumPerTon = 40.0,
            SpecialtyDescription = "中国热轧H型钢产量最大、规格最全的型钢专业厂，型钢大盘基准",
            SupportsThirdPartyCertification = true,
            IsRecommended = true
        },
        new SteelMillBrand
        {
            Id = "MaSteel",
            Name = "马钢股份 (宝武集团)",
            ShortName = "马钢",
            ProductionBase = "安徽马鞍山",
            Tier = 1,
            BrandPremiumPerTon = 80.0,
            SpecialtyDescription = "国内热轧H型钢与重轨老牌骨干钢厂，全系列中大规格热轧型钢",
            SupportsThirdPartyCertification = true,
            IsRecommended = true
        },
        new SteelMillBrand
        {
            Id = "Shagang",
            Name = "沙钢集团 (江苏沙钢)",
            ShortName = "沙钢",
            ProductionBase = "江苏张家港",
            Tier = 2,
            BrandPremiumPerTon = 0.0,
            SpecialtyDescription = "华东普板与热卷现货主力龙头、物流发货极快、民营集采基准",
            SupportsThirdPartyCertification = true,
            IsRecommended = true
        },
        new SteelMillBrand
        {
            Id = "Ansteel",
            Name = "鞍钢 / 本钢集团",
            ShortName = "鞍钢",
            ProductionBase = "辽宁鞍山 / 营口鲅鱼圈",
            Tier = 1,
            BrandPremiumPerTon = 80.0,
            SpecialtyDescription = "北方重型中厚板与高强度桥梁钢基地、出海口直装散货船",
            SupportsThirdPartyCertification = true,
            IsRecommended = false
        },
        new SteelMillBrand
        {
            Id = "Xiangsteel",
            Name = "华菱钢铁 (湘钢)",
            ShortName = "湘钢",
            ProductionBase = "湖南湘潭",
            Tier = 1,
            BrandPremiumPerTon = 120.0,
            SpecialtyDescription = "超宽厚板、高层建筑抗震钢、海工及风电塔筒特厚板",
            SupportsThirdPartyCertification = true,
            IsRecommended = false
        },
        new SteelMillBrand
        {
            Id = "WISCO",
            Name = "武钢有限 (宝武集团)",
            ShortName = "武钢",
            ProductionBase = "湖北武汉",
            Tier = 1,
            BrandPremiumPerTon = 100.0,
            SpecialtyDescription = "华中低合金高强板与结构重轨基地",
            SupportsThirdPartyCertification = true,
            IsRecommended = false
        },
        new SteelMillBrand
        {
            Id = "GeneralMill",
            Name = "区域常规民营钢厂",
            ShortName = "民营钢厂",
            ProductionBase = "本地冶炼产地",
            Tier = 3,
            BrandPremiumPerTon = -50.0,
            SpecialtyDescription = "常规普碳结构板材、工业厂房集采低价直供",
            SupportsThirdPartyCertification = false,
            IsRecommended = false
        }
    ];

    public static List<SteelMillBrand> AllMills => _mills;

    public static SteelMillBrand GetById(string id) =>
        _mills.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.OrdinalIgnoreCase)) ?? _mills[0];
}
