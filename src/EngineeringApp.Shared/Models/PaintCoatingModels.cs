namespace EngineeringApp.Shared.Models;

/// <summary>
/// 涂装防腐与防火涂层层次分类
/// </summary>
public enum CoatingLayerType
{
    /// <summary>底漆 (Primer: 环氧富锌/无机富锌/醇酸防锈底漆)</summary>
    Primer,

    /// <summary>中间漆 (Intermediate: 环氧云铁厚浆中间漆)</summary>
    Intermediate,

    /// <summary>封闭漆 (Sealer: 环氧封闭漆/聚氨酯封闭底漆)</summary>
    Sealer,

    /// <summary>面漆 (Topcoat: 丙烯酸聚氨酯/氟碳/醇酸面漆)</summary>
    Topcoat,

    /// <summary>防火涂料 (Fireproof: 室内膨胀型超薄型/厚型钢结构防火涂料)</summary>
    Fireproof
}

/// <summary>
/// 现场施工涂装工艺工法
/// </summary>
public enum CoatingApplicationMethod
{
    /// <summary>高压无气喷涂 (Airless Spraying) - 工业主流，雾化均匀，厚浆首选，损耗适中</summary>
    AirlessSpraying,

    /// <summary>空气喷涂 (Air Spraying) - 雾化细腻平整，但气流飞溅扩散大，损耗偏高</summary>
    AirSpraying,

    /// <summary>手工刷涂/滚涂 (Brush & Roller) - 零飞溅无飘散损耗极低，但工效低且工费单价显著偏高</summary>
    BrushOrRoller
}

/// <summary>
/// 中国国标锌锭纯度等级 (GB/T 470《锌锭》)
/// </summary>
public enum ZincIngotGrade
{
    /// <summary>0# 特级高纯锌 (Zn ≥ 99.995%) - 特高压特重冰区铁塔、铁路接触网、外贸工程强制指定，溢价 +¥500/t</summary>
    Grade0_HighPurity_99995,

    /// <summary>1# 优质结构纯锌 (Zn ≥ 99.99%) - 工业重防腐钢结构通用主力基准锌</summary>
    Grade1_Standard_9999,

    /// <summary>再生粗锌 / 次级配比锌 (Zn 98.5%~99.5%) - 含铅铁杂质，易出暗斑，折让 -¥800/t</summary>
    Secondary_CrudeZinc
}

/// <summary>
/// 截面断面几何与工法损耗率修正引擎
/// </summary>
public static class SectionShapeLossConfig
{
    /// <summary>
    /// 获取断面形状对喷涂飞溅损耗的影响系数 K_section (以平整箱型/平板为 1.0 基准)
    /// </summary>
    public static double GetSectionShapeFactor(SectionType type) => type switch
    {
        SectionType.RHS => 1.00,       // 箱型/方管：平整垂直靶面宽阔，飞溅最少
        SectionType.Rectangle => 1.00, // 矩形平整面
        SectionType.Plate => 1.00,     // 钢板双面平整面，飞溅损耗最少
        SectionType.HBeam => 1.15,     // H型钢/工字钢：腹板翼缘直角阴角气流涡流反弹
        SectionType.Channel => 1.20,   // 槽钢：单侧翼缘内凹
        SectionType.TSection => 1.22,  // T型钢
        SectionType.Circle => 1.38,    // 圆形：曲面切线喷雾大量逸散落空，损耗显著大于箱型
        SectionType.CHS => 1.38,       // 圆管：曲率表面无垂直靶面，喷枪扫掠边缘切线飞散大
        SectionType.Angle => 1.60,     // 角钢：肢件狭窄，网格间隙密集，喷雾大面积落空穿透
        SectionType.Cruciform => 1.45, // 十字形：多直角凹槽
        SectionType.Polygon => 1.25,   // 任意多边形
        _ => 1.15
    };

    /// <summary>
    /// 获取施工工法对飞散损耗的影响系数 K_method (以高压无气喷涂为 1.0 基准)
    /// </summary>
    public static double GetMethodLossFactor(CoatingApplicationMethod method) => method switch
    {
        CoatingApplicationMethod.AirlessSpraying => 1.00, // 高压无气喷涂 (基准)
        CoatingApplicationMethod.AirSpraying => 1.28,     // 空气喷涂：压缩气流带动漆雾飘扬，损耗扩大
        CoatingApplicationMethod.BrushOrRoller => 0.45,   // 手工刷滚涂：直接涂布，几乎无飞溅损耗 (损耗率仅 10%~15%)
        _ => 1.00
    };

    /// <summary>
    /// 获取施工工法对人工单价的影响系数 (刷滚涂工效低，人工单价显著偏高)
    /// </summary>
    public static double GetMethodLaborMultiplier(CoatingApplicationMethod method) => method switch
    {
        CoatingApplicationMethod.AirlessSpraying => 1.00, // 基准工效 (300~600 m²/人日)
        CoatingApplicationMethod.AirSpraying => 1.15,     // 空气喷涂 (150~250 m²/人日)
        CoatingApplicationMethod.BrushOrRoller => 1.65,   // 刷滚涂人工极慢 (40~80 m²/人日)，工费提高约 65%
        _ => 1.00
    };
}

/// <summary>
/// 油漆与涂装品牌档案库条目
/// </summary>
public class PaintBrandInfo
{
    public string BrandId { get; set; } = "";
    public string NameCn { get; set; } = "";
    public string NameEn { get; set; } = "";
    public string Country { get; set; } = "";
    public string LogoEmoji { get; set; } = "🎨";
    public string Description { get; set; } = "";
    public string Specialty { get; set; } = "";
    public List<string> FamousProjects { get; set; } = [];
}

/// <summary>
/// 油漆与涂料产品库条目
/// </summary>
public class PaintProductItem
{
    public int Id { get; set; }
    public string BrandId { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public CoatingLayerType LayerType { get; set; }
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "工业标准品";

    /// <summary>强关联供应商实体主键ID (外键映射供应商库)</summary>
    public int? SupplierId { get; set; }

    public string Description { get; set; } = "";
    public string Features { get; set; } = "";
    public string SupportedMethods { get; set; } = "高压无气喷涂 / 刷涂滚涂";

    /// <summary>固体体积含量 (Volume Solids %，如 65 表示 65%)</summary>
    public double VolumeSolidsPercent { get; set; } = 60.0;

    /// <summary>涂料比重 (kg/L)</summary>
    public double DensityKgL { get; set; } = 1.35;

    /// <summary>推荐设计干膜厚度 (微米 μm，防火涂料为 mm)</summary>
    public double DefaultDft { get; set; } = 75.0;

    /// <summary>基准施工损耗率估计 (%，如 25 表示 25%)</summary>
    public double LossRatePercent { get; set; } = 25.0;

    /// <summary>涂料材料单价 (元/kg)</summary>
    public double MaterialUnitPricePerKg { get; set; } = 35.0;

    /// <summary>现场喷涂施工工费单价 (元/m²)</summary>
    public double LaborUnitPricePerM2 { get; set; } = 15.0;
}

/// <summary>
/// 单层涂料设计计算输入参数
/// </summary>
public class CoatingLayerConfig
{
    public CoatingLayerType LayerType { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string BrandId { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string ProductName { get; set; } = "";

    /// <summary>设计干膜厚度 (μm，防火涂料单位为 mm)</summary>
    public double Dft { get; set; } = 70.0;

    /// <summary>固体体积含量 (%)</summary>
    public double VolumeSolids { get; set; } = 60.0;

    /// <summary>涂料密度 (kg/L)</summary>
    public double Density { get; set; } = 1.35;

    /// <summary>基准施工损耗率 (%)</summary>
    public double LossRate { get; set; } = 25.0;

    /// <summary>材料单价 (元/kg)</summary>
    public double MaterialPrice { get; set; } = 35.0;

    /// <summary>施工人工与辅材单价 (元/m²)</summary>
    public double LaborPrice { get; set; } = 15.0;

    public string GetDisplayName() => LayerType switch
    {
        CoatingLayerType.Primer => "底漆 (Primer)",
        CoatingLayerType.Intermediate => "中间漆 (Intermediate)",
        CoatingLayerType.Sealer => "封闭漆 (Sealer)",
        CoatingLayerType.Topcoat => "面漆 (Topcoat)",
        CoatingLayerType.Fireproof => "防火涂料 (Fireproof)",
        _ => LayerType.ToString()
    };
}

/// <summary>
/// 单层涂装计算结果详情
/// </summary>
public class CoatingLayerResult
{
    public CoatingLayerType LayerType { get; set; }
    public string LayerName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public double Dft { get; set; }

    /// <summary>经截面与工法修正后的有效损耗率 (%)</summary>
    public double EffectiveLossRatePercent { get; set; }

    /// <summary>理论涂布率 (m²/kg)</summary>
    public double TheoreticalCoverageM2Kg { get; set; }

    /// <summary>实际消耗量指标 (kg/m²)</summary>
    public double ActualUsageKgPerM2 { get; set; }

    /// <summary>每平方米材料成本 (元/m²)</summary>
    public double MaterialCostPerM2 { get; set; }

    /// <summary>每平方米施工工费 (元/m²)</summary>
    public double LaborCostPerM2 { get; set; }

    /// <summary>每平方米综合单价 (元/m²)</summary>
    public double TotalCostPerM2 => MaterialCostPerM2 + LaborCostPerM2;

    /// <summary>延米涂料用量 (kg/m)</summary>
    public double LinearUsageKgPerMeter { get; set; }

    /// <summary>延米涂装综合造价 (元/m)</summary>
    public double LinearCostPerMeter { get; set; }
}

/// <summary>
/// 涂装计算推导单步追踪记录
/// </summary>
public class PaintCalculationTraceStep
{
    public string Title { get; set; } = "";
    public string Formula { get; set; } = "";
    public string ValueSubstitution { get; set; } = "";
    public string Conclusion { get; set; } = "";
}

/// <summary>
/// 涂装防腐防火透明推导全过程记录
/// </summary>
public class PaintCalculationProcessTrace
{
    public List<PaintCalculationTraceStep> Steps { get; set; } = [];
    public string SectionShapeExplanation { get; set; } = "";
    public string MethodExplanation { get; set; } = "";
}

/// <summary>
/// 涂装防腐防火系统综合计算成果
/// </summary>
public class PaintSystemCalculationResult
{
    /// <summary>采用的施工工法</summary>
    public CoatingApplicationMethod ApplicationMethod { get; set; } = CoatingApplicationMethod.AirlessSpraying;

    /// <summary>截面断面损耗修正系数 K_section</summary>
    public double SectionShapeLossFactor { get; set; } = 1.0;

    /// <summary>工法损耗修正系数 K_method</summary>
    public double MethodLossFactor { get; set; } = 1.0;

    /// <summary>截面形状系数 / 比表面积 F/V (m⁻¹)</summary>
    public double SectionFactorFOverV { get; set; }

    /// <summary>参与计算的有效图层列表</summary>
    public List<CoatingLayerResult> Layers { get; set; } = [];

    /// <summary>采用的构件涂装面积基准 (m²/m)</summary>
    public double AppliedPaintingAreaPerMeter { get; set; }

    /// <summary>是否扣除了楼板贴合上表面</summary>
    public bool TopSurfaceExcluded { get; set; }

    /// <summary>截面吨钢展开比表面积 (m²/t)</summary>
    public double AreaPerTon { get; set; }

    /// <summary>总干膜厚度 (微米 μm，不含防火涂料)</summary>
    public double TotalPaintDftMicrons { get; set; }

    /// <summary>防火涂料厚度 (mm)</summary>
    public double FireproofThicknessMm { get; set; }

    /// <summary>每平方米综合造价合计 (元/m²)</summary>
    public double TotalCostPerM2 { get; set; }

    /// <summary>每平方米材料费合计 (元/m²)</summary>
    public double TotalMaterialCostPerM2 { get; set; }

    /// <summary>每平方米工费合计 (元/m²)</summary>
    public double TotalLaborCostPerM2 { get; set; }

    /// <summary>延米涂料总用量 (kg/m)</summary>
    public double TotalUsageKgPerMeter { get; set; }

    /// <summary>延米涂装综合总造价 (元/m)</summary>
    public double TotalCostPerMeter { get; set; }

    /// <summary>吨钢涂料总消耗量 (kg/t)</summary>
    public double TotalUsageKgPerTon { get; set; }

    /// <summary>吨钢涂装综合总造价 (元/t)</summary>
    public double TotalCostPerTon { get; set; }

    /// <summary>输入构件总长度 (m)</summary>
    public double ProjectTotalLengthM { get; set; } = 100.0;

    /// <summary>工程总涂料采购重量 (kg)</summary>
    public double ProjectTotalPaintKg => TotalUsageKgPerMeter * ProjectTotalLengthM;

    /// <summary>工程涂装防腐总造价 (元)</summary>
    public double ProjectTotalCostYuan => TotalCostPerMeter * ProjectTotalLengthM;

    /// <summary>透明计算推导记录</summary>
    public PaintCalculationProcessTrace? ProcessTrace { get; set; }

    /// <summary>标准化 Describe 技术规格输出</summary>
    public string FullTechnicalDescribe { get; set; } = "";
}

/// <summary>
/// 钢结构热浸镀锌 (HDG) 算量输入选项
/// </summary>
public class HotDipGalvanizingOptions
{
    /// <summary>国标锌锭纯度等级 (GB/T 470)</summary>
    public ZincIngotGrade ZincGrade { get; set; } = ZincIngotGrade.Grade1_Standard_9999;

    /// <summary>锌锭现货网价基准 (元/吨，默认按上海有色网基准约 23,000 元/吨)</summary>
    public double ZincPricePerTon { get; set; } = 23000.0;

    /// <summary>目标设计热浸镀锌层厚度 (μm，按 GB/T 13912 通常 70/85/100 μm)</summary>
    public double DftMicrons { get; set; } = 85.0;

    /// <summary>闭口截面或型钢是否内外双面全浸锌 (角钢/开孔管内外皆镀，默认 true)</summary>
    public bool IsBothSidesGalvanized { get; set; } = true;

    /// <summary>构件单根长度是否超过镀锌锅 (&gt;12.5m) 需倒头进行二次双浸 (Double Dip)</summary>
    public bool IsDoubleDipOverlength { get; set; } = false;

    /// <summary>是否属于复杂闭腔构件 (需专门开设工艺排气孔/排锌防爆孔)</summary>
    public bool HasComplexCavities { get; set; } = false;

    /// <summary>基础酸洗钝化、助镀剂与天然气加热能耗工费 (元/吨钢)</summary>
    public double BaseAcidAndEnergyCostPerTon { get; set; } = 1250.0;

    /// <summary>下池打捞锌渣、锌灰及挂锌不均匀损耗率 (%)</summary>
    public double ZincLossRatePercent { get; set; } = 18.0;

    /// <summary>批量构件总长度 (m，默认按 1 延米基准计价)</summary>
    public double ProjectLengthM { get; set; } = 1.0;
}

/// <summary>
/// 钢结构热浸镀锌 (HDG) 精确算量与造价结果
/// </summary>
public class HotDipGalvanizingResult
{
    public string ZincGradeName { get; set; } = "1# 优质结构纯锌 (99.99%)";
    public double EffectiveZincPricePerTon { get; set; }
    public double TargetDftMicrons { get; set; }

    /// <summary>截面吨钢比表面积 S_ton (m²/t)</summary>
    public double SpecificSurfaceAreaTon { get; set; }

    /// <summary>延米参与浸锌表面积 (m²/m)</summary>
    public double AppliedAreaPerMeter { get; set; }

    /// <summary>构件延米线密度 / 自重 (kg/m)</summary>
    public double LinearMassKgM { get; set; }

    /// <summary>单位面积理论纯锌附着量 (g/m²)</summary>
    public double ZincWeightPerM2 { get; set; }

    /// <summary>每吨钢耗锌量 (kg/吨钢，含锌损)</summary>
    public double ZincConsumptionKgPerTon { get; set; }

    /// <summary>浸锌增重比例 (%) = 每吨耗锌量 / 1000 * 100%</summary>
    public double WeightIncreasePercent => (ZincConsumptionKgPerTon / 1000.0) * 100.0;

    /// <summary>每吨钢纯锌材料消耗费用 (元/吨)</summary>
    public double ZincMaterialCostPerTon { get; set; }

    /// <summary>每吨钢基础酸洗能耗加工费 (元/吨)</summary>
    public double BaseProcessingCostPerTon { get; set; }

    /// <summary>超长双浸及工艺难度溢价 (元/吨)</summary>
    public double OverlengthSurchargePerTon { get; set; }

    /// <summary>每吨钢热浸镀锌最终综合加工单价 (元/吨)</summary>
    public double TotalPricePerTon { get; set; }

    /// <summary>延米热浸镀锌综合造价 (元/m)</summary>
    public double LinearPricePerMeter { get; set; }

    /// <summary>每平方米表面积折算综合单价 (元/m²)</summary>
    public double PricePerM2 { get; set; }

    /// <summary>工程批量总钢构件重量 (吨)</summary>
    public double ProjectTotalTons => (LinearMassKgM * ProjectLengthM) / 1000.0;

    /// <summary>工程总耗锌重量 (kg)</summary>
    public double ProjectTotalZincKg => ZincConsumptionKgPerTon * ProjectTotalTons;

    /// <summary>工程热浸镀锌总造价 (元)</summary>
    public double ProjectTotalCostYuan => TotalPricePerTon * ProjectTotalTons;

    /// <summary>输入总长度 (m)</summary>
    public double ProjectLengthM { get; set; } = 100.0;

    /// <summary>行业机理分析说明</summary>
    public string MechanismExplanation { get; set; } = "";

    /// <summary>标准化 Describe 描述</summary>
    public string FullDescribeText { get; set; } = "";
}
