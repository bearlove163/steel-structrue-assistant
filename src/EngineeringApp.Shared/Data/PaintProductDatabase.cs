using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 常用钢结构工程油漆涂料产品与价格基准数据库
/// 涵盖国际领军巨头 (佐敦、海虹老人、阿克苏诺贝尔、PPG、SKK) 与国内一线名牌 (金隅天龙、兰陵、三棵树)
/// 覆盖底漆、中间漆、封闭漆、面漆及膨胀/非膨胀防火涂料全品类
/// </summary>
public static class PaintProductDatabase
{
    private static readonly List<PaintBrandInfo> _brands =
    [
        new PaintBrandInfo
        {
            BrandId = "Jotun",
            NameCn = "佐敦涂料",
            NameEn = "Jotun Paints",
            Country = "挪威 (Norway)",
            LogoEmoji = "🐧",
            Description = "始于 1926 年挪威桑德尔福德，全球海事、海洋工程与工业钢结构重防腐领域的顶级霸主。在超长耐候环氧与膨胀型防火涂料技术上处于全球领导地位。",
            Specialty = "海洋与石化重防腐、超高层钢结构膨胀型薄型防火涂料、低温固化厚浆环氧",
            FamousProjects = ["阿联酋迪拜哈利法塔 (世界第一高楼)", "广州塔 (小蛮腰)", "法国埃菲尔铁塔长期维护", "港珠澳大桥主体工程"]
        },
        new PaintBrandInfo
        {
            BrandId = "Hempel",
            NameCn = "海虹老人",
            NameEn = "Hempel A/S",
            Country = "丹麦 (Denmark)",
            LogoEmoji = "⚓",
            Description = "始于 1915 年丹麦哥本哈根，拥有超百年工业防护传奇历史。在跨海大桥、陆上及海上风电塔筒、大型公建桥梁钢结构防腐领域占有极高市场份额。",
            Specialty = "特大型桥梁防腐、海上风电塔筒高耐候配套、高固体分环保低VOC厚浆系统",
            FamousProjects = ["杭州湾跨海大桥", "英国伦敦千禧桥", "希腊里翁-安提里翁跨海大桥", "上海浦东国际机场三期航站楼"]
        },
        new PaintBrandInfo
        {
            BrandId = "AkzoNobel",
            NameCn = "阿克苏诺贝尔 · 国际油漆",
            NameEn = "AkzoNobel (International Paint)",
            Country = "荷兰 / 英国 (Netherlands / UK)",
            LogoEmoji = "🌐",
            Description = "欧洲老牌跨国化工巨头，旗下 International (国际牌) 是工业防护涂料界历史最悠久的王牌品牌，Interzinc / Intergard 涂装体系是全球重防腐技术规范的奠基标准之一。",
            Specialty = "国际工业与海工特种防护、Interchar 膨胀防火系统、高耐久 FEVE 氟碳面漆",
            FamousProjects = ["北京国家体育场 (鸟巢)", "澳大利亚悉尼歌剧院", "英国伦敦千禧穹顶", "上海环球金融中心"]
        },
        new PaintBrandInfo
        {
            BrandId = "PPG",
            NameCn = "PPG 工业涂料",
            NameEn = "PPG Industries",
            Country = "美国 (USA)",
            LogoEmoji = "🛡️",
            Description = "创立于 1883 年美国匹兹堡，全球特种涂料与玻璃工业巨擘。旗下 Sigma Coatings 与 Steelguard 防火系统在大型交通枢纽与石化基建中享有盛誉。",
            Specialty = "大型枢纽机场航站楼、快干高产能工业涂料、超长耐火 Steelguard 防火涂装",
            FamousProjects = ["美国帝国大厦翻新", "北京大兴国际机场钢网架", "上海金茂大厦", "香港国际机场航站楼"]
        },
        new PaintBrandInfo
        {
            BrandId = "SKK",
            NameCn = "四国化研",
            NameEn = "Shikoku Kaken (SKK)",
            Country = "日本 (Japan)",
            LogoEmoji = "🏯",
            Description = "创立于 1955 年日本大阪，亚洲著名建筑与工业特种防护涂料先驱。在超高耐候无机复合陶瓷、自洁型氟碳面漆和特种阻燃防火涂料领域享有极高声誉。",
            Specialty = "超耐候陶瓷氟碳系统、高阻燃建筑钢结构防火涂装、防霉自洁纳米复合面漆",
            FamousProjects = ["日本东京晴空塔周边工程", "大阪关西国际机场", "上海世博会日本国家馆", "深圳平安金融中心辅助防护"]
        },
        new PaintBrandInfo
        {
            BrandId = "BBMG",
            NameCn = "北京金隅天龙",
            NameEn = "BBMG Fire Protection",
            Country = "中国北京 (China)",
            LogoEmoji = "🔥",
            Description = "中国北方最大的建材集团金隅旗下防火科技王牌，中国《钢结构防火涂料》国家标准参编核心企业，在超薄型、薄型及厚型钢结构防火涂料领域市场占有率名列前茅。",
            Specialty = "室内超薄膨胀型防火 (1.5h~2.0h)、大型厚型非膨胀隔热防火 (2.5h~3.0h)、高附着力防脱落",
            FamousProjects = ["北京冬奥会国家速滑馆 (冰丝带)", "北京城市副中心综合交通枢纽", "雄安高铁站大跨度钢结构", "中国尊 (中信大厦)"]
        },
        new PaintBrandInfo
        {
            BrandId = "Lanling",
            NameCn = "兰陵涂料",
            NameEn = "Lanling Chemical",
            Country = "中国江苏 (China)",
            LogoEmoji = "🏮",
            Description = "始建于 1980 年江苏常州，中国工业防腐涂料领头羊，国家级制造业单项冠军。长期主导国家电网特高压铁塔、大型冶金厂房、重型港口机械及桥梁钢结构的防腐供货。",
            Specialty = "输变电铁塔特种重防腐、高锌含量环氧底漆、电力石化钢结构长效保护",
            FamousProjects = ["准东-皖南 ±1100kV 特高压直流输电塔架", "白鹤滩特大型水电站钢闸门与网架", "泰州长江公路大桥", "宝武湛江钢铁基地"]
        },
        new PaintBrandInfo
        {
            BrandId = "3Trees",
            NameCn = "三棵树工业涂料",
            NameEn = "3TREES Industrial",
            Country = "中国福建 (China)",
            LogoEmoji = "🌲",
            Description = "中国本土高成长性民族涂料领跑者，以“道法自然”绿色健康为宗旨。其工业涂料板块专注于水性高固含钢结构防腐面漆及绿色低VOC膨胀型防火涂料技术。",
            Specialty = "低VOC环保水性工业防腐、绿色低碳建筑钢结构配套、装配式建筑配套防火防腐",
            FamousProjects = ["北京冬奥会张家口崇礼赛区工程", "京雄城际铁路配套设施", "福厦高铁厦门北站大跨度网架", "三棵树高新产业园钢结构"]
        }
    ];

    private static readonly List<PaintProductItem> _products =
    [
        // ==================== 1. 底漆 (Primer) ====================
        new PaintProductItem
        {
            Id = 1,
            BrandId = "Jotun",
            ModelCode = "Penguard Pro",
            LayerType = CoatingLayerType.Primer,
            Name = "佐敦 Penguard Pro 双组份环氧防锈底漆",
            Brand = "佐敦 (Jotun)",
            Description = "高固体分双组份纯环氧防锈底漆，具有极高的机械强度与卓越的附着力。",
            Features = "优异耐盐雾性，可耐水汽浸泡，与聚氨酯及防火涂层兼容性极好",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 65.0,
            DensityKgL = 1.45,
            DefaultDft = 70.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 42.0,
            LaborUnitPricePerM2 = 12.0
        },
        new PaintProductItem
        {
            Id = 2,
            BrandId = "Jotun",
            ModelCode = "Barrier 80",
            LayerType = CoatingLayerType.Primer,
            Name = "佐敦 Barrier 80 环氧富锌底漆 (80%锌粉)",
            Brand = "佐敦 (Jotun)",
            Description = "顶级高锌含量环氧底漆，符合 SSPC Paint 20 第1级标准，提供强大的阴极电化学保护。",
            Features = "锌粉含量达 80%，重防腐工程标配，自修复划痕能力优异",
            SupportedMethods = "高压无气喷涂",
            VolumeSolidsPercent = 60.0,
            DensityKgL = 2.45,
            DefaultDft = 60.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 48.0,
            LaborUnitPricePerM2 = 13.0
        },
        new PaintProductItem
        {
            Id = 3,
            BrandId = "Hempel",
            ModelCode = "Hempadur 17360",
            LayerType = CoatingLayerType.Primer,
            Name = "海虹老人 Hempadur Zinc 17360 环氧富锌底漆",
            Brand = "海虹老人 (Hempel)",
            Description = "工业重防腐两罐装富锌环氧底漆，快干硬化，施工宽容度大。",
            Features = "符合 ISO 12944 高耐久体系要求，耐候防锈寿命长，重型钢结构通用",
            SupportedMethods = "高压无气喷涂 / 空气喷涂",
            VolumeSolidsPercent = 65.0,
            DensityKgL = 2.20,
            DefaultDft = 70.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 46.0,
            LaborUnitPricePerM2 = 13.0
        },
        new PaintProductItem
        {
            Id = 4,
            BrandId = "AkzoNobel",
            ModelCode = "Interzinc 52",
            LayerType = CoatingLayerType.Primer,
            Name = "阿克苏诺贝尔 Interzinc 52 高性能环氧富锌底漆",
            Brand = "阿克苏诺贝尔 (AkzoNobel)",
            Description = "全球著名的 International 两组份富锌环氧底漆，专为重腐蚀环境下的钢结构维护与新建设计。",
            Features = "全球重大基建与海工首选，极强的阴极保护与层间附着力",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 59.0,
            DensityKgL = 2.40,
            DefaultDft = 65.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 49.0,
            LaborUnitPricePerM2 = 14.0
        },
        new PaintProductItem
        {
            Id = 5,
            BrandId = "PPG",
            ModelCode = "SigmaZinc 109",
            LayerType = CoatingLayerType.Primer,
            Name = "PPG SigmaZinc 109 优质环氧富锌底漆",
            Brand = "PPG 工业涂料",
            Description = "高固体分双组份聚酰胺固化富锌底漆，用于恶劣大气环境钢结构防腐。",
            Features = "快干耐磨，抗阴极剥离，优异的低温固化表现",
            SupportedMethods = "高压无气喷涂",
            VolumeSolidsPercent = 64.0,
            DensityKgL = 2.30,
            DefaultDft = 70.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 45.0,
            LaborUnitPricePerM2 = 13.0
        },
        new PaintProductItem
        {
            Id = 6,
            BrandId = "Lanling",
            ModelCode = "LL-EZ80",
            LayerType = CoatingLayerType.Primer,
            Name = "兰陵 LL-EZ 环氧富锌防腐底漆 (70%锌粉)",
            Brand = "兰陵涂料 (Lanling)",
            Description = "国内电力铁塔、桥梁及冶金钢结构主力防锈底漆，性价比高，品质稳定。",
            Features = "国标优质产品，电化学保护好，适合国内大宗钢结构工程集采",
            SupportedMethods = "高压无气喷涂 / 刷涂",
            VolumeSolidsPercent = 62.0,
            DensityKgL = 1.95,
            DefaultDft = 70.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 34.0,
            LaborUnitPricePerM2 = 11.0
        },
        new PaintProductItem
        {
            Id = 7,
            BrandId = "3Trees",
            ModelCode = "3T-WPrimer",
            LayerType = CoatingLayerType.Primer,
            Name = "三棵树水性钢结构防锈底漆 (环保低VOC)",
            Brand = "三棵树工业涂料",
            Description = "单组份改性水性丙烯酸环保防锈底漆，气味极低，无溶剂易燃风险。",
            Features = "绿色三星建材认证，适用于室内钢结构、装配式建筑及环保要求极高项目",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 48.0,
            DensityKgL = 1.25,
            DefaultDft = 50.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 28.0,
            LaborUnitPricePerM2 = 10.0
        },

        // ==================== 2. 中间漆 (Intermediate) ====================
        new PaintProductItem
        {
            Id = 8,
            BrandId = "Jotun",
            ModelCode = "Jotamastic 90",
            LayerType = CoatingLayerType.Intermediate,
            Name = "佐敦 Jotamastic 90 高固含环氧厚浆中间漆",
            Brand = "佐敦 (Jotun)",
            Description = "两组份多胺固化厚浆型环氧漆，极高固体分，能大幅提高漆膜屏蔽水汽渗透能力。",
            Features = "固体分达 80%，单道成膜厚度大，重防腐 C4/C5 经典中间层",
            SupportedMethods = "高压无气喷涂",
            VolumeSolidsPercent = 80.0,
            DensityKgL = 1.55,
            DefaultDft = 120.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 38.0,
            LaborUnitPricePerM2 = 12.0
        },
        new PaintProductItem
        {
            Id = 9,
            BrandId = "Hempel",
            ModelCode = "Hempadur MIO 47950",
            LayerType = CoatingLayerType.Intermediate,
            Name = "海虹老人 Hempadur MIO 47950 环氧云铁厚浆中漆",
            Brand = "海虹老人 (Hempel)",
            Description = "含片状云母氧化铁 (MIO) 填料的高性能环氧中间漆，形成密闭迷宫屏障阻隔腐蚀介质。",
            Features = "经典云铁中间漆，层间重涂间隔窗口长，大幅提升面漆附着力",
            SupportedMethods = "高压无气喷涂 / 刷涂",
            VolumeSolidsPercent = 65.0,
            DensityKgL = 1.50,
            DefaultDft = 100.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 32.0,
            LaborUnitPricePerM2 = 12.0
        },
        new PaintProductItem
        {
            Id = 10,
            BrandId = "AkzoNobel",
            ModelCode = "Intergard 475HS",
            LayerType = CoatingLayerType.Intermediate,
            Name = "阿克苏诺贝尔 Intergard 475HS 高固含环氧云铁中漆",
            Brand = "阿克苏诺贝尔 (AkzoNobel)",
            Description = "低 VOC、高固体份双组份环氧云铁厚浆中间漆，设计用于重腐蚀工业环境。",
            Features = "国际工程标配中间漆，优异的屏蔽阻隔性与坚韧机械强度",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 80.0,
            DensityKgL = 1.60,
            DefaultDft = 125.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 39.0,
            LaborUnitPricePerM2 = 13.0
        },
        new PaintProductItem
        {
            Id = 11,
            BrandId = "Lanling",
            ModelCode = "LL-MIO100",
            LayerType = CoatingLayerType.Intermediate,
            Name = "兰陵环氧云铁厚浆防锈中间漆 (通用型)",
            Brand = "兰陵涂料 (Lanling)",
            Description = "国内重工业与电网常用厚浆云铁漆，片状云铁颗粒致密均匀，抗化学介质腐蚀佳。",
            Features = "经济耐用，与国内各大底漆和面漆配套兼容性极强",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 60.0,
            DensityKgL = 1.40,
            DefaultDft = 100.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 26.0,
            LaborUnitPricePerM2 = 11.0
        },

        // ==================== 3. 封闭漆 (Sealer) ====================
        new PaintProductItem
        {
            Id = 12,
            BrandId = "Jotun",
            ModelCode = "Penguard Clear Sealer",
            LayerType = CoatingLayerType.Sealer,
            Name = "佐敦 Penguard Clear 双组份环氧封闭清漆",
            Brand = "佐敦 (Jotun)",
            Description = "专门用于无机富锌、热喷锌铝表面封闭微细气孔的高流动性清漆，杜绝面漆起泡。",
            Features = "深层渗透孔隙，防止锌层粉化，极大增强后续漆膜层间结合力",
            SupportedMethods = "高压无气喷涂 / 刷涂",
            VolumeSolidsPercent = 45.0,
            DensityKgL = 1.15,
            DefaultDft = 30.0,
            LossRatePercent = 20.0,
            MaterialUnitPricePerKg = 45.0,
            LaborUnitPricePerM2 = 9.0
        },
        new PaintProductItem
        {
            Id = 13,
            BrandId = "Hempel",
            ModelCode = "Hempadur Sealer 05990",
            LayerType = CoatingLayerType.Sealer,
            Name = "海虹老人 Hempadur 05990 低粘度环氧封闭底漆",
            Brand = "海虹老人 (Hempel)",
            Description = "低粘度双组份环氧清漆，专为热喷金属 (TSA/TSZ) 或多孔基底表面封孔研发。",
            Features = "渗透力极强，固化迅速，有效消除后续厚涂时的针孔缺陷",
            SupportedMethods = "空气喷涂 / 滚涂刷涂",
            VolumeSolidsPercent = 40.0,
            DensityKgL = 1.10,
            DefaultDft = 25.0,
            LossRatePercent = 20.0,
            MaterialUnitPricePerKg = 43.0,
            LaborUnitPricePerM2 = 9.0
        },

        // ==================== 4. 面漆 (Topcoat) ====================
        new PaintProductItem
        {
            Id = 14,
            BrandId = "Jotun",
            ModelCode = "Hardtop XP",
            LayerType = CoatingLayerType.Topcoat,
            Name = "佐敦 Hardtop XP 脂肪族聚氨酯耐候面漆",
            Brand = "佐敦 (Jotun)",
            Description = "全球销量领先的双组份聚氨酯面漆，优良的保光保色性能与耐化学介质飞溅性。",
            Features = "出色的抗紫外线老化性能，光泽柔和丰满，工业民用钢结构主力首选",
            SupportedMethods = "高压无气喷涂 / 空气喷涂",
            VolumeSolidsPercent = 63.0,
            DensityKgL = 1.35,
            DefaultDft = 60.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 52.0,
            LaborUnitPricePerM2 = 16.0
        },
        new PaintProductItem
        {
            Id = 15,
            BrandId = "Hempel",
            ModelCode = "Hempathane 55210",
            LayerType = CoatingLayerType.Topcoat,
            Name = "海虹老人 Hempathane Topcoat 55210 丙烯酸聚氨酯面漆",
            Brand = "海虹老人 (Hempel)",
            Description = "双组份高光亮丙烯酸聚氨酯面漆，脂肪族异氰酸酯固化，具有优异的耐候性。",
            Features = "高光泽、耐擦洗、耐磨损，适合严酷大气腐蚀条件下的外露钢构",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 53.0,
            DensityKgL = 1.25,
            DefaultDft = 50.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 48.0,
            LaborUnitPricePerM2 = 15.0
        },
        new PaintProductItem
        {
            Id = 16,
            BrandId = "AkzoNobel",
            ModelCode = "Interthane 990",
            LayerType = CoatingLayerType.Topcoat,
            Name = "阿克苏诺贝尔 Interthane 990 脂肪族聚氨酯面漆",
            Brand = "阿克苏诺贝尔 (AkzoNobel)",
            Description = "行业标杆级双组份丙烯酸聚氨酯面漆，提供无与伦比的色泽持久性与耐粉化性。",
            Features = "全球重大标志性工程首选面漆，在强紫外线照射下数年不失光不褪色",
            SupportedMethods = "高压无气喷涂 / 空气喷涂",
            VolumeSolidsPercent = 57.0,
            DensityKgL = 1.30,
            DefaultDft = 55.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 55.0,
            LaborUnitPricePerM2 = 16.0
        },
        new PaintProductItem
        {
            Id = 17,
            BrandId = "SKK",
            ModelCode = "Cerami Clean FEVE",
            LayerType = CoatingLayerType.Topcoat,
            Name = "四国化研 (SKK) 陶瓷氟碳重防腐超耐候面漆",
            Brand = "四国化研 (SKK)",
            Description = "日本顶级 FEVE 四氟树脂复合纳米陶瓷微粒，耐候保光寿命可达 25 年以上。",
            Features = "超长耐久年限，耐酸雨强紫外线，超自洁抗污性，大型机场高铁枢纽高端配置",
            SupportedMethods = "空气喷涂 / 高压无气喷涂",
            VolumeSolidsPercent = 52.0,
            DensityKgL = 1.22,
            DefaultDft = 45.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 95.0,
            LaborUnitPricePerM2 = 24.0
        },
        new PaintProductItem
        {
            Id = 18,
            BrandId = "Lanling",
            ModelCode = "LL-PU60",
            LayerType = CoatingLayerType.Topcoat,
            Name = "兰陵脂肪族聚氨酯面漆 (高保色通用型)",
            Brand = "兰陵涂料 (Lanling)",
            Description = "国内大型工业构件与电厂钢架配套通用聚氨酯面漆，性价比高，遮盖力好。",
            Features = "漆膜坚硬平整，良好的耐水与耐机油性能，适合常规工业工程",
            SupportedMethods = "高压无气喷涂 / 刷涂滚涂",
            VolumeSolidsPercent = 50.0,
            DensityKgL = 1.20,
            DefaultDft = 50.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 38.0,
            LaborUnitPricePerM2 = 13.0
        },

        // ==================== 5. 防火涂料 (Fireproof) ====================
        new PaintProductItem
        {
            Id = 19,
            BrandId = "Jotun",
            ModelCode = "Steelmaster 1200WF",
            LayerType = CoatingLayerType.Fireproof,
            Name = "佐敦 Steelmaster 1200WF 水性超薄膨胀型防火涂料",
            Brand = "佐敦 (Jotun)",
            Description = "单组份水性超薄型膨胀防火涂料，经 BS 476 / EN 13381 严格认证，耐火极限最高达 2.0h。",
            Features = "超薄美观，环保无异味，遇火急剧膨胀达 50 倍致密碳化隔热泡沫",
            SupportedMethods = "高压无气喷涂",
            VolumeSolidsPercent = 71.0,
            DensityKgL = 1.38,
            DefaultDft = 1.8, // mm
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 36.0,
            LaborUnitPricePerM2 = 28.0
        },
        new PaintProductItem
        {
            Id = 20,
            BrandId = "AkzoNobel",
            ModelCode = "Interchar 1120",
            LayerType = CoatingLayerType.Fireproof,
            Name = "阿克苏诺贝尔 Interchar 1120 水性膨胀型防火涂料",
            Brand = "阿克苏诺贝尔 (AkzoNobel)",
            Description = "高性能单组份水性膨胀型防火涂料，专为商办大楼外露钢梁、钢柱建筑美观设计。",
            Features = "可直接涂覆装饰性聚氨酯面漆，提供长达 2 小时的可靠耐火防护",
            SupportedMethods = "高压无气喷涂 / 滚涂修补",
            VolumeSolidsPercent = 68.0,
            DensityKgL = 1.34,
            DefaultDft = 2.0, // mm
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 38.0,
            LaborUnitPricePerM2 = 30.0
        },
        new PaintProductItem
        {
            Id = 21,
            BrandId = "BBMG",
            ModelCode = "TL-S1520",
            LayerType = CoatingLayerType.Fireproof,
            Name = "金隅天龙室内超薄型膨胀防火涂料 (耐火1.5h-2.0h)",
            Brand = "北京金隅天龙",
            Description = "中国建筑钢结构防火涂料国家标准参编产品，遇火发泡膨胀碳化倍数高，隔热效能优异。",
            Features = "国标检测甲级，厚度通常仅需 1.5~2.2mm，极大节省构件静载自重与空间",
            SupportedMethods = "高压无气喷涂 / 刮涂抹涂",
            VolumeSolidsPercent = 72.0,
            DensityKgL = 1.30,
            DefaultDft = 2.0, // mm
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 28.0,
            LaborUnitPricePerM2 = 28.0
        },
        new PaintProductItem
        {
            Id = 22,
            BrandId = "BBMG",
            ModelCode = "TL-H2530",
            LayerType = CoatingLayerType.Fireproof,
            Name = "金隅天龙室内厚型非膨胀隔热防火涂料 (耐火2.5h-3.0h)",
            Brand = "北京金隅天龙",
            Description = "无机矿物隔热厚涂料，主要用于高层建筑承重钢柱、大型工业厂房耐火 3.0h 严苛要求。",
            Features = "容重轻、耐候不老化、遇火不发烟无毒气，厚度一般为 15~25mm",
            SupportedMethods = "特种重型喷涂机喷涂 / 抹涂",
            VolumeSolidsPercent = 85.0,
            DensityKgL = 0.65, // 容重较轻
            DefaultDft = 20.0, // mm
            LossRatePercent = 35.0,
            MaterialUnitPricePerKg = 12.0,
            LaborUnitPricePerM2 = 45.0
        },
        new PaintProductItem
        {
            Id = 23,
            BrandId = "Lanling",
            ModelCode = "LL-FW20",
            LayerType = CoatingLayerType.Fireproof,
            Name = "兰陵室内薄型钢结构防火涂料 (耐火2.0h)",
            Brand = "兰陵涂料 (Lanling)",
            Description = "电力与冶金厂房钢结构专用薄型防火涂料，附着力强，耐火膨胀隔热稳定。",
            Features = "适合大面积机械化喷涂，与兰陵防锈底漆具有经国家消防质检认证的配套证书",
            SupportedMethods = "高压无气喷涂",
            VolumeSolidsPercent = 70.0,
            DensityKgL = 1.32,
            DefaultDft = 2.2, // mm
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 26.0,
            LaborUnitPricePerM2 = 26.0
        }
    ];

    public static IReadOnlyList<PaintBrandInfo> AllBrands => _brands;
    public static IReadOnlyList<PaintProductItem> AllProducts => _products;

    public static PaintBrandInfo? GetBrandById(string brandId) =>
        _brands.FirstOrDefault(b => b.BrandId.Equals(brandId, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<PaintProductItem> GetProductsByBrand(string brandId) =>
        _products.Where(p => p.BrandId.Equals(brandId, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<PaintProductItem> GetByType(CoatingLayerType type) =>
        _products.Where(p => p.LayerType == type);

    public static IEnumerable<PaintProductItem> SearchProducts(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return _products;
        var kw = keyword.Trim().ToLowerInvariant();
        return _products.Where(p =>
            p.Name.ToLowerInvariant().Contains(kw) ||
            p.Brand.ToLowerInvariant().Contains(kw) ||
            p.ModelCode.ToLowerInvariant().Contains(kw) ||
            p.Description.ToLowerInvariant().Contains(kw));
    }

    public static void AddProduct(PaintProductItem item)
    {
        item.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        _products.Add(item);
    }

    public static bool RemoveProduct(int id)
    {
        var target = _products.FirstOrDefault(p => p.Id == id);
        return target != null && _products.Remove(target);
    }

    public static void UpdateProducts(IEnumerable<PaintProductItem> items)
    {
        _products.Clear();
        _products.AddRange(items);
    }

    public static string ExportToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(_products, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public static bool ImportFromJson(string json, bool overwrite = false)
    {
        try
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<PaintProductItem>>(json);
            if (list == null || list.Count == 0) return false;
            if (overwrite)
            {
                _products.Clear();
            }
            foreach (var item in list)
            {
                if (!_products.Any(p => p.Id == item.Id))
                {
                    _products.Add(item);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void ResetToDefaults()
    {
        // 保持当前预设
    }
}
