using System;
using System.ComponentModel.DataAnnotations;

namespace EngineeringApp.Shared.Models;

/// <summary>
/// 供应商主体大类分类体系
/// </summary>
public enum SupplierCategory
{
    /// <summary>钢铁冶炼制造钢厂 (如宝武、鞍钢、首钢、沙钢、南钢等)</summary>
    [Display(Name = "钢厂制造")]
    SteelMill = 1,

    /// <summary>钢材现货贸易商/仓储物流流通商 (如欧冶云商、上海五矿、物产中大、浙商中拓等)</summary>
    [Display(Name = "贸易现货")]
    SteelTrader = 2,

    /// <summary>起重吊装机械租赁与安拆商 (如庞源租赁、中联租赁、徐工起重等)</summary>
    [Display(Name = "吊机租赁")]
    CraneRental = 3,

    /// <summary>连接与紧固件供应商 (如高强螺栓、地脚锚栓、化学锚栓、抗剪栓钉等)</summary>
    [Display(Name = "紧固件螺栓")]
    FastenerBolt = 4,

    /// <summary>特种结构与工程材料商 (如空间拉索索具、建筑减隔震支座、楼承板围护、特种焊材等)</summary>
    [Display(Name = "特种构件与材料")]
    SpecialtyMaterial = 5,

    /// <summary>工业重防腐与建筑防火涂料生产商/授权代理商 (如佐敦、阿克苏诺贝尔、中远关西、海虹老人、宣伟等)</summary>
    [Display(Name = "防腐防火涂料")]
    PaintCoating = 6,

    /// <summary>管理员自定义新型物料品类 (如阻尼减震器、抗风网架配件、新型节能保温等)</summary>
    [Display(Name = "自定义新型品类")]
    OtherCustom = 99
}

/// <summary>
/// 供应商企业资质档案与名录条目（轻量级名录字典架构）
/// </summary>
public class SupplierItem
{
    public int Id { get; set; }

    /// <summary>供应商系统唯一编号 (如 SUP-MILL-001, SUP-BLT-002, SUP-CRN-003)</summary>
    public string Code { get; set; } = "";

    /// <summary>企业工商法定全称</summary>
    public string Name { get; set; } = "";

    /// <summary>企业常用简称/品牌标识</summary>
    public string ShortName { get; set; } = "";

    /// <summary>供应商大类归属</summary>
    public SupplierCategory Category { get; set; } = SupplierCategory.SteelTrader;

    /// <summary>动态细分子类与产品标签 (支持管理员自由输入与扩展，如 '10.9S高强螺栓', '空间预应力拉索', '抗震球型钢支座', '闭口楼承板')</summary>
    public string SubCategory { get; set; } = "";

    /// <summary>业务对接联系人</summary>
    public string ContactPerson { get; set; } = "";

    /// <summary>联系电话/移动电话</summary>
    public string ContactPhone { get; set; } = "";

    /// <summary>商务邮箱/传真</summary>
    public string Email { get; set; } = "";

    /// <summary>企业所在地/主要生产基地/供货辐射省市</summary>
    public string CityRegion { get; set; } = "";

    /// <summary>合作准入状态 ('优先推荐', '合格在册', '框架协议', '考察备选', '暂停合作')</summary>
    public string CooperationStatus { get; set; } = "合格在册";

    /// <summary>资质等级、产能规模、主营优势及特别说明</summary>
    public string Remarks { get; set; } = "";

    /// <summary>钢厂品牌基准溢价 (元/吨，用于估值引擎自动核算，如 +200, +150, +40)</summary>
    public double BenchmarkPremiumPerTon { get; set; } = 0.0;

    /// <summary>入库建档时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>最后更新时间</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
