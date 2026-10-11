using System;
using System.Collections.Generic;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 全国工程供应链标杆供应商基准数据库 (涵盖钢厂、贸易商、吊机商、紧固件螺栓、特种材料[拉索/支座/楼承板]、油漆涂料)
/// </summary>
public static class SupplierDatabase
{
    private static readonly List<SupplierItem> _seedSuppliers =
    [
        // ================= 1. 钢厂制造 (SteelMill) =================
        new SupplierItem
        {
            Id = 1,
            Code = "SUP-MILL-001",
            Name = "宝山钢铁股份有限公司 (宝武集团)",
            ShortName = "宝钢股份",
            Category = SupplierCategory.SteelMill,
            SubCategory = "中厚板/TMCP控轧/高建钢/Z向特厚板",
            ContactPerson = "王志强 (直供部处长)",
            ContactPhone = "021-26648888",
            Email = "direct_sales@baosteel.com",
            CityRegion = "上海宝山 / 广东湛江",
            CooperationStatus = "战略合作",
            Remarks = "超高层公建与国家重大基建指定首选钢厂，探伤保级率最高，Z35级特厚板标杆"
        },
        new SupplierItem
        {
            Id = 2,
            Code = "SUP-MILL-002",
            Name = "南京钢铁股份有限公司 (南钢集团)",
            ShortName = "南钢股份",
            Category = SupplierCategory.SteelMill,
            SubCategory = "中厚特钢/耐候耐火钢/桥梁结构板",
            ContactPerson = "陈经理 (销售公司)",
            ContactPhone = "025-57072222",
            Email = "sales@nisco.cn",
            CityRegion = "江苏南京",
            CooperationStatus = "战略合作",
            Remarks = "全国中厚板特钢龙头，高层建筑结构用钢Q390GJ/Q420GJ供货量领先"
        },
        new SupplierItem
        {
            Id = 3,
            Code = "SUP-MILL-003",
            Name = "河北津西钢铁集团股份有限公司",
            ShortName = "津西型钢",
            Category = SupplierCategory.SteelMill,
            SubCategory = "热轧H型钢/工字钢/槽钢",
            ContactPerson = "张部长 (型钢经销处)",
            ContactPhone = "0315-8738888",
            Email = "jinxi_steel@jinxi.com",
            CityRegion = "河北唐山迁西",
            CooperationStatus = "合格在册",
            Remarks = "中国热轧H型钢规格最齐全的型钢专业厂，大盘型钢定价基准钢厂"
        },
        new SupplierItem
        {
            Id = 4,
            Code = "SUP-MILL-004",
            Name = "马鞍山钢铁股份有限公司 (宝武马钢)",
            ShortName = "马钢股份",
            Category = SupplierCategory.SteelMill,
            SubCategory = "重型H型钢/特厚缘型钢/重轨",
            ContactPerson = "李工 (华东营销中心)",
            ContactPhone = "0555-2882222",
            Email = "sales@masteel.com.cn",
            CityRegion = "安徽马鞍山",
            CooperationStatus = "优先推荐",
            Remarks = "国内第一条重型H型钢生产线，全截面覆盖欧标美标国标轧制"
        },
        new SupplierItem
        {
            Id = 5,
            Code = "SUP-MILL-005",
            Name = "江苏沙钢集团有限公司",
            ShortName = "沙钢集团",
            Category = SupplierCategory.SteelMill,
            SubCategory = "热轧宽厚卷板/优质碳素钢",
            ContactPerson = "刘经理 (华东销售部)",
            ContactPhone = "0512-58568888",
            Email = "shagang_group@shagang.com",
            CityRegion = "江苏张家港",
            CooperationStatus = "合格在册",
            Remarks = "特大型民营钢铁联合企业，普碳热轧卷板现货流通量巨大，交期灵活"
        },
        new SupplierItem
        {
            Id = 6,
            Code = "SUP-MILL-006",
            Name = "鞍钢股份有限公司",
            ShortName = "鞍钢股份",
            Category = SupplierCategory.SteelMill,
            SubCategory = "极厚钢板/桥梁船用钢/低温韧性钢",
            ContactPerson = "赵处长 (北方直销处)",
            ContactPhone = "0412-6722888",
            Email = "angang_heavy@ansteel.cn",
            CityRegion = "辽宁鞍山 / 营口鲅鱼圈",
            CooperationStatus = "优先推荐",
            Remarks = "东北特厚板与低温高韧性钢核心研发生产基地，具备超大宽厚板轧制实力"
        },

        // ================= 2. 钢材现货贸易商 (SteelTrader) =================
        new SupplierItem
        {
            Id = 7,
            Code = "SUP-TRD-001",
            Name = "欧冶云商股份有限公司",
            ShortName = "欧冶云商",
            Category = SupplierCategory.SteelTrader,
            SubCategory = "全品类现货电商/供应链仓储物流",
            ContactPerson = "周总监 (大客户工程部)",
            ContactPhone = "021-96166",
            Email = "engineering@ouyeel.com",
            CityRegion = "上海市 / 全国中心仓",
            CooperationStatus = "战略合作",
            Remarks = "宝武集团旗下第三方钢铁生态圈电商平台，支持多钢厂资源联运与现货仓锁单"
        },
        new SupplierItem
        {
            Id = 8,
            Code = "SUP-TRD-002",
            Name = "五矿发展股份有限公司 (上海五矿)",
            ShortName = "五矿发展",
            Category = SupplierCategory.SteelTrader,
            SubCategory = "大型重点工程直供/大宗现货配供/垫资供应链",
            ContactPerson = "孙经理 (华东基建中心)",
            ContactPhone = "021-68865588",
            Email = "minmetals_sh@minmetals.com",
            CityRegion = "上海浦东 / 华东枢纽",
            CooperationStatus = "优先推荐",
            Remarks = "央企五矿主力商贸流通板块，具备卓越资金垫资与多钢厂大单直供组织能力"
        },
        new SupplierItem
        {
            Id = 9,
            Code = "SUP-TRD-003",
            Name = "浙商中拓集团股份有限公司",
            ShortName = "浙商中拓",
            Category = SupplierCategory.SteelTrader,
            SubCategory = "基建供应链集成服务/现货仓储剪切加工",
            ContactPerson = "郑部长 (工程物资部)",
            ContactPhone = "0571-87661188",
            Email = "service@zndec.com",
            CityRegion = "浙江杭州 / 华东仓",
            CooperationStatus = "优先推荐",
            Remarks = "浙江交投旗下上市公司，专注基础设施工程材料配送，服务网点覆盖全国各大枢纽"
        },
        new SupplierItem
        {
            Id = 10,
            Code = "SUP-TRD-004",
            Name = "物产中大物流投资有限公司",
            ShortName = "物产中大",
            Category = SupplierCategory.SteelTrader,
            SubCategory = "工业板材现货/型钢组合配送",
            ContactPerson = "钱经理 (供应链事业部)",
            ContactPhone = "0571-85777000",
            Email = "steel_logistics@wuchanzhongda.cn",
            CityRegion = "浙江杭州 / 江苏江阴",
            CooperationStatus = "合格在册",
            Remarks = "世界500强省属物产中大旗下核心流通板块，长三角库存量充裕"
        },

        // ================= 3. 吊机机械租赁 (CraneRental) =================
        new SupplierItem
        {
            Id = 11,
            Code = "SUP-CRN-001",
            Name = "上海庞源机械工程有限公司",
            ShortName = "庞源租赁",
            Category = SupplierCategory.CraneRental,
            SubCategory = "重型平头塔机/动臂塔机/履带式起重机租赁",
            ContactPerson = "吴总 (华东租赁运营部)",
            ContactPhone = "021-59750000",
            Email = "pangyuan_crane@sh-pangyuan.com",
            CityRegion = "上海嘉定 / 全国分子公司",
            CooperationStatus = "战略合作",
            Remarks = "陕建机股份全资子公司，全球第一大建筑塔式起重机租赁商，备有机型涵盖自100t·m至3200t·m"
        },
        new SupplierItem
        {
            Id = 12,
            Code = "SUP-CRN-002",
            Name = "中联重科建筑起重机械服务公司",
            ShortName = "中联租赁",
            Category = SupplierCategory.CraneRental,
            SubCategory = "超大型动臂塔吊/千吨级履带吊成套安拆",
            ContactPerson = "蒋经理 (工程机械租赁部)",
            ContactPhone = "0731-88923888",
            Email = "zoomlion_rental@zoomlion.com",
            CityRegion = "湖南长沙 / 华东基地",
            CooperationStatus = "优先推荐",
            Remarks = "超高层与特大型钢结构网壳吊装首选设备提供商，机况优良，自带特种资质安拆队伍"
        },
        new SupplierItem
        {
            Id = 13,
            Code = "SUP-CRN-003",
            Name = "徐州工程机械集团起重成套租赁服务部",
            ShortName = "徐工起重服务",
            Category = SupplierCategory.CraneRental,
            SubCategory = "全地面起重机(汽车吊)/特种履带吊",
            ContactPerson = "徐主任 (特种工程部)",
            ContactPhone = "0516-87888888",
            Email = "xcmg_crane_service@xcmg.com",
            CityRegion = "江苏徐州 / 全国网点",
            CooperationStatus = "合格在册",
            Remarks = "覆盖50t至1200t全地面汽车吊与履带吊，适用于场区拼装与大悬挑钢梁整体抬吊"
        },

        // ================= 4. 连接与紧固件 (FastenerBolt) =================
        new SupplierItem
        {
            Id = 14,
            Code = "SUP-BLT-001",
            Name = "上海标准件五厂有限公司",
            ShortName = "上海标五",
            Category = SupplierCategory.FastenerBolt,
            SubCategory = "10.9S大六角高强螺栓/扭剪型高强螺栓/抗剪栓钉",
            ContactPerson = "黄科长 (技术营销部)",
            ContactPhone = "021-65415555",
            Email = "biaowu_sales@biaowu.com",
            CityRegion = "上海杨浦 / 奉贤基地",
            CooperationStatus = "战略合作",
            Remarks = "中国钢结构高强螺栓知名品牌，国标GB/T 1228/3632主要起草单位之一，公建首选品牌"
        },
        new SupplierItem
        {
            Id = 15,
            Code = "SUP-BLT-002",
            Name = "晋亿实业股份有限公司",
            ShortName = "晋亿实业",
            Category = SupplierCategory.FastenerBolt,
            SubCategory = "全规格钢结构高强螺栓/地脚锚栓/特种紧固件",
            ContactPerson = "蔡经理 (工程配套部)",
            ContactPhone = "0573-84185000",
            Email = "gem_year@gem-year.net",
            CityRegion = "浙江嘉善",
            CooperationStatus = "优先推荐",
            Remarks = "国内紧固件行业龙头上市公司，全自动热处理线，品质受控能力极强，高跌落抗疲劳"
        },
        new SupplierItem
        {
            Id = 16,
            Code = "SUP-BLT-003",
            Name = "七丰精工科技股份有限公司",
            ShortName = "七丰精工",
            Category = SupplierCategory.FastenerBolt,
            SubCategory = "高精度抗疲劳螺栓/航空与高铁特种紧固件",
            ContactPerson = "高工 (技术直供部)",
            ContactPhone = "0573-86851111",
            Email = "qifeng_precision@qf-fastener.com",
            CityRegion = "浙江海盐",
            CooperationStatus = "合格在册",
            Remarks = "专精特新小巨人企业，专注于动载冲击与大振动环境特种抗剪抗滑移紧固连接"
        },
        new SupplierItem
        {
            Id = 17,
            Code = "SUP-BLT-004",
            Name = "河北永年特种紧固件产业集聚供货中心",
            ShortName = "永年紧固件直配",
            Category = SupplierCategory.FastenerBolt,
            SubCategory = "粗规格地脚锚栓/化学锚栓/热浸镀锌螺栓",
            ContactPerson = "杨经理 (华北物流调配)",
            ContactPhone = "0310-6881234",
            Email = "yongnian_bolts@yn-fastener.cn",
            CityRegion = "河北邯郸永年",
            CooperationStatus = "合格在册",
            Remarks = "全国最大标准件集散基地，粗直径Q235/Q355预埋地脚螺栓加工成本优势显著，现货极快"
        },

        // ================= 5. 特种构件与材料 (SpecialtyMaterial) =================
        new SupplierItem
        {
            Id = 18,
            Code = "SUP-MAT-001",
            Name = "巨力索具股份有限公司",
            ShortName = "巨力索具",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "空间拉索/预应力封闭索/高钒索/索夹锚具",
            ContactPerson = "杨总监 (空间结构事业部)",
            ContactPhone = "0312-8608888",
            Email = "juli_cable@juli-rope.com",
            CityRegion = "河北保定徐水",
            CooperationStatus = "战略合作",
            Remarks = "大跨度体育场馆索穹顶、索网幕墙知名企业，具备全套预拉伸试验线与张拉施工资质"
        },
        new SupplierItem
        {
            Id = 19,
            Code = "SUP-MAT-002",
            Name = "江苏法尔胜泓昇集团有限公司",
            ShortName = "法尔胜索具",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "斜拉索/空间悬索/不锈钢装饰索/张拉连接件",
            ContactPerson = "沈工 (桥梁建筑工程部)",
            ContactPhone = "0510-86118888",
            Email = "cable_tech@fastengroup.com",
            CityRegion = "江苏江阴",
            CooperationStatus = "优先推荐",
            Remarks = "国内著名索结构与光纤钢绳集团，抗疲劳与防腐PE护套护层工艺业内领先"
        },
        new SupplierItem
        {
            Id = 20,
            Code = "SUP-MAT-003",
            Name = "柳州欧维姆机械股份有限公司 (OVM)",
            ShortName = "柳州欧维姆",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "抗震球型钢支座/建筑隔震橡胶支座/预应力锚具",
            ContactPerson = "廖部长 (建筑工程事业部)",
            ContactPhone = "0772-3888888",
            Email = "sales@ovm.cn",
            CityRegion = "广西柳州",
            CooperationStatus = "战略合作",
            Remarks = "中国预应力与结构减隔震龙头企业，万吨级大跨支座研发试验能力，重大公建优选"
        },
        new SupplierItem
        {
            Id = 21,
            Code = "SUP-MAT-004",
            Name = "株洲时代新材料科技股份有限公司",
            ShortName = "时代新材 (中车)",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "摩擦摆减隔震支座/粘滞阻尼器/铅芯橡胶支座",
            ContactPerson = "唐经理 (减隔震营销处)",
            ContactPhone = "0731-22837777",
            Email = "seismic@teg.cn",
            CityRegion = "湖南株洲",
            CooperationStatus = "优先推荐",
            Remarks = "中国中车旗下高科技新材料上市公司，在高烈度抗震设防与超大位移变形支座领域处于领先地位"
        },
        new SupplierItem
        {
            Id = 22,
            Code = "SUP-MAT-005",
            Name = "精工绿筑科技集团有限公司 (长江精工)",
            ShortName = "精工绿筑",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "闭口型楼承板/钢筋桁架楼承板/装配式楼盖",
            ContactPerson = "何经理 (绿筑技术部)",
            ContactPhone = "0575-84138888",
            Email = "greentech@600496.com",
            CityRegion = "浙江绍兴柯桥",
            CooperationStatus = "战略合作",
            Remarks = "长江精工旗下装配式绿色建筑核心品牌，YXB65-185闭口板等型号产销领先，耐火性能出众"
        },
        new SupplierItem
        {
            Id = 23,
            Code = "SUP-MAT-006",
            Name = "多维联合集团有限公司",
            ShortName = "多维联合",
            Category = SupplierCategory.SpecialtyMaterial,
            SubCategory = "开口型压型金属楼承板/聚氨酯夹芯屋面板",
            ContactPerson = "崔经理 (围护系统部)",
            ContactPhone = "010-60501188",
            Email = "duowei_deck@duowei.com",
            CityRegion = "北京通州 / 河北廊坊",
            CooperationStatus = "合格在册",
            Remarks = "高端工业厂房与大跨场馆围护系统领军企业，压型钢板版型齐全，耐腐蚀镀层工艺优"
        },

        // ================= 6. 防腐与防火涂料 (PaintCoating) =================
        new SupplierItem
        {
            Id = 24,
            Code = "SUP-PNT-001",
            Name = "佐敦涂料 (张家港) 有限公司",
            ShortName = "佐敦涂料 (Jotun)",
            Category = SupplierCategory.PaintCoating,
            SubCategory = "环氧富锌底漆/环氧云铁中间漆/脂肪族聚氨酯面漆",
            ContactPerson = "彭总监 (工业防腐部)",
            ContactPhone = "0512-58937777",
            Email = "industrial_cn@jotun.com",
            CityRegion = "江苏张家港 / 挪威",
            CooperationStatus = "战略合作",
            Remarks = "全球知名重防腐涂料品牌，Penguard/Hardtop体系广泛应用于机场枢纽与标志性公共建筑"
        },
        new SupplierItem
        {
            Id = 25,
            Code = "SUP-PNT-002",
            Name = "阿克苏诺贝尔涂料 (中国) 投资有限公司",
            ShortName = "阿克苏诺贝尔 (International)",
            Category = SupplierCategory.PaintCoating,
            SubCategory = "国际油漆Intergard/Interfine特种耐候氟碳漆",
            ContactPerson = "梁经理 (大客户事业部)",
            ContactPhone = "021-22205000",
            Email = "pc_china@akzonobel.com",
            CityRegion = "上海市 / 英国",
            CooperationStatus = "战略合作",
            Remarks = "旗下International品牌为重工业与海洋工程防腐标杆，保光保色与防腐年限极高"
        },
        new SupplierItem
        {
            Id = 26,
            Code = "SUP-PNT-003",
            Name = "中远关西涂料化工有限公司",
            ShortName = "中远关西 (Kansai)",
            Category = SupplierCategory.PaintCoating,
            SubCategory = "重防腐底漆/改性环氧漆/氟碳高耐候面漆",
            ContactPerson = "林部长 (工程营销中心)",
            ContactPhone = "022-83955555",
            Email = "kansai_sales@cosco-kansai.com",
            CityRegion = "天津 / 上海 / 日本关西",
            CooperationStatus = "优先推荐",
            Remarks = "中远海运旗下重防腐骨干企业，长三角与珠三角桥梁、大型钢网架涂装成熟业绩极多"
        },
        new SupplierItem
        {
            Id = 27,
            Code = "SUP-PNT-004",
            Name = "海虹老人涂料 (中国) 有限公司",
            ShortName = "海虹老人 (Hempel)",
            Category = SupplierCategory.PaintCoating,
            SubCategory = "全天候重防腐漆/高固体分环氧漆",
            ContactPerson = "方经理 (华东销售处)",
            ContactPhone = "020-84888888",
            Email = "hempel_china@hempel.com",
            CityRegion = "广东广州 / 丹麦",
            CooperationStatus = "合格在册",
            Remarks = "百年欧洲重防腐品牌，低VOC高固体分环保工业涂料系列完备"
        },
        new SupplierItem
        {
            Id = 28,
            Code = "SUP-PNT-005",
            Name = "宣伟 (上海) 涂料有限公司",
            ShortName = "宣伟防火涂料 (Sherwin)",
            Category = SupplierCategory.PaintCoating,
            SubCategory = "室内外超薄型/厚型钢结构防火涂料/环氧膨胀型",
            ContactPerson = "马总 (工程特防部)",
            ContactPhone = "021-59552222",
            Email = "fire_coatings@sherwin.com",
            CityRegion = "上海嘉定 / 美国",
            CooperationStatus = "优先推荐",
            Remarks = "FIRETEX系列膨胀型钢结构防火涂料，耐火极限可达1.5h~3.0h，通过国家应急管理部严格认证"
        }
    ];

    private static readonly List<SupplierItem> _dynamicSuppliers = [.. _seedSuppliers];

    public static IReadOnlyList<SupplierItem> AllSuppliers => _dynamicSuppliers;

    public static SupplierItem? GetById(int id) =>
        _dynamicSuppliers.FirstOrDefault(s => s.Id == id);

    public static IEnumerable<SupplierItem> Filter(SupplierCategory? category, string? subCategory = null, string? keyword = null)
    {
        var query = _dynamicSuppliers.AsEnumerable();

        if (category.HasValue && (int)category.Value > 0)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(subCategory) && subCategory != "全部")
        {
            query = query.Where(s => s.SubCategory.Contains(subCategory, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLowerInvariant();
            query = query.Where(s =>
                s.Name.ToLowerInvariant().Contains(kw) ||
                s.ShortName.ToLowerInvariant().Contains(kw) ||
                s.Code.ToLowerInvariant().Contains(kw) ||
                s.SubCategory.ToLowerInvariant().Contains(kw) ||
                s.ContactPerson.ToLowerInvariant().Contains(kw) ||
                s.CityRegion.ToLowerInvariant().Contains(kw));
        }

        return query.OrderBy(s => s.Category).ThenBy(s => s.Id);
    }

    public static List<string> GetAllSubCategories(SupplierCategory? category = null)
    {
        var query = _dynamicSuppliers.AsEnumerable();
        if (category.HasValue && (int)category.Value > 0)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        return query
            .SelectMany(s => s.SubCategory.Split(new[] { '/', '、', ',', '，' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .OrderBy(s => s)
            .ToList();
    }

    public static void AddSupplier(SupplierItem item)
    {
        item.Id = _dynamicSuppliers.Count > 0 ? _dynamicSuppliers.Max(s => s.Id) + 1 : 1;
        if (string.IsNullOrWhiteSpace(item.Code))
        {
            var prefix = item.Category switch
            {
                SupplierCategory.SteelMill => "SUP-MILL",
                SupplierCategory.SteelTrader => "SUP-TRD",
                SupplierCategory.CraneRental => "SUP-CRN",
                SupplierCategory.FastenerBolt => "SUP-BLT",
                SupplierCategory.SpecialtyMaterial => "SUP-MAT",
                SupplierCategory.PaintCoating => "SUP-PNT",
                _ => "SUP-CUS"
            };
            item.Code = $"{prefix}-{item.Id:D3}";
        }
        item.CreatedAt = DateTime.Now;
        item.UpdatedAt = DateTime.Now;
        _dynamicSuppliers.Add(item);
    }

    public static bool UpdateSupplier(SupplierItem item)
    {
        var target = _dynamicSuppliers.FirstOrDefault(s => s.Id == item.Id);
        if (target == null) return false;

        target.Name = item.Name;
        target.ShortName = item.ShortName;
        target.Category = item.Category;
        target.SubCategory = item.SubCategory;
        target.ContactPerson = item.ContactPerson;
        target.ContactPhone = item.ContactPhone;
        target.Email = item.Email;
        target.CityRegion = item.CityRegion;
        target.CooperationStatus = item.CooperationStatus;
        target.Remarks = item.Remarks;
        target.UpdatedAt = DateTime.Now;
        return true;
    }

    public static bool RemoveSupplier(int id)
    {
        var target = _dynamicSuppliers.FirstOrDefault(s => s.Id == id);
        return target != null && _dynamicSuppliers.Remove(target);
    }

    public static string ExportToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(_dynamicSuppliers, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public static bool ImportFromJson(string json, bool overwrite = false)
    {
        try
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<SupplierItem>>(json);
            if (list == null || list.Count == 0) return false;

            if (overwrite)
            {
                _dynamicSuppliers.Clear();
            }

            foreach (var item in list)
            {
                if (!_dynamicSuppliers.Any(s => s.Id == item.Id || s.Code.Equals(item.Code, StringComparison.OrdinalIgnoreCase)))
                {
                    _dynamicSuppliers.Add(item);
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
        _dynamicSuppliers.Clear();
        _dynamicSuppliers.AddRange(_seedSuppliers);
    }
}
