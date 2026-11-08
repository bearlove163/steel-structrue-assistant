using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EngineeringApp.Shared.BOQ.Models
{

/// <summary>
/// 项目大盘实体：包含项目级全局默认管理费率 (8%) 与利润率 (7%)
/// </summary>
public class BoqProject
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty; // 如 "NB-ATZX"
    public string ProjectName { get; set; } = string.Empty; // 如 "宁波奥林匹克体育中心主体育场与配套工程"
    public string ClientName { get; set; } = string.Empty;  // 如 "宁波市奥体投资建设有限公司 / 城投指挥部"
    public double DefaultManagementFeeRate { get; set; } = 0.08; // 8% 全局默认管理费率
    public double DefaultProfitRate { get; set; } = 0.07; // 7% 全局默认利润率
    public string ActiveMaterialSnapshotId { get; set; } = string.Empty;
    public string ActivePaintSnapshotId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public List<BoqTenderItem> TenderItems { get; set; } = new();
}

/// <summary>
/// 对外商务清单项实体：支持单项启用自定义费率覆盖 (不平衡报价策略)
/// </summary>
public class BoqTenderItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    [JsonIgnore]
    public BoqProject? Project { get; set; }

    public string ItemTag { get; set; } = string.Empty; // 如 "NH", "NF", "HL", "WH", "JLC"
    public string BillItemCode { get; set; } = string.Empty; // 如 "010101001001"
    public string BillItemName { get; set; } = string.Empty; // 如 "南侧大跨主桁架制作安装"
    public string ProjectCharacteristics { get; set; } = string.Empty; // 如 "高强钢Q460GJC、管径1200、3.0H防火"
    public string Unit { get; set; } = "t"; // 计量单位 (t / m2)
    public double TenderQuantity { get; set; } // 最终投标工程量 (t)
    public double DirectCostPerTon { get; set; } // 下层汇算的直接成本底价 (元/t)

    [NotMapped]
    public double DirectCostPricePerUnit
    {
        get => DirectCostPerTon;
        set => DirectCostPerTon = value;
    }

    // 单项独立费率覆盖配置
    public bool IsCustomRateEnabled { get; set; } = false; // 是否启用单项费率覆盖
    public double? CustomManagementFeeRate { get; set; } // 自定义管理费率 (如 0.10 即 10%)
    public double? CustomProfitRate { get; set; } // 自定义利润率 (如 0.12 即 12%)

    // 最终对外组价结果
    public double EffectiveManagementFeeRate { get; set; } = 0.08; // 当前有效管理费率
    public double EffectiveProfitRate { get; set; } = 0.07; // 当前有效利润率
    public double ComprehensiveUnitPrice { get; set; } // 最终对外综合单价 (元/t)
    public double TotalTenderPrice { get; set; } // 最终投标总合价 (元)

    // Navigation
    public List<BoqSubProject> SubProjects { get; set; } = new();

    /// <summary>
    /// 获取当前清单项的有效管理费率与利润率 (优先使用自定义覆盖，未启用则继承项目默认费率或标准 8%, 7%)
    /// </summary>
    public (double ManagementFeeRate, double ProfitRate) GetEffectiveRates(BoqProject? project = null)
    {
        var effectiveProject = project ?? Project;
        var defaultMgmt = effectiveProject?.DefaultManagementFeeRate ?? 0.08;
        var defaultProfit = effectiveProject?.DefaultProfitRate ?? 0.07;

        double mgmt = (IsCustomRateEnabled && CustomManagementFeeRate.HasValue)
            ? CustomManagementFeeRate.Value
            : defaultMgmt;

        double profit = (IsCustomRateEnabled && CustomProfitRate.HasValue)
            ? CustomProfitRate.Value
            : defaultProfit;

        EffectiveManagementFeeRate = mgmt;
        EffectiveProfitRate = profit;

        return (mgmt, profit);
    }
}

/// <summary>
/// 对内构件成本中枢实体：挂接制作定额、安装定额与防火排程
/// </summary>
public class BoqSubProject
{
    public int Id { get; set; }
    public int TenderItemId { get; set; }
    [JsonIgnore]
    public BoqTenderItem? TenderItem { get; set; }

    public int? FabricationQuotaRuleId { get; set; }
    public int? InstallationQuotaRuleId { get; set; }

    public string SubProjectCode { get; set; } = string.Empty; // 如 "N-NHXX-48", "NFSX1"
    public string SubProjectName { get; set; } = string.Empty; // 如 "下弦大管段", "副桁架上弦段"
    public string StructuralRole { get; set; } = string.Empty; // "主桁架" / "副桁架" / "柱" / "梁"
    public string InstallationMethod { get; set; } = string.Empty; // "厂内拼装发运" / "发现场高空散拼"

    public double MaxPlateThickness { get; set; } // 构件内最大主要板厚/管壁厚 (mm)
    [NotMapped]
    public double MaxPieceThicknessMm { get => MaxPlateThickness; set => MaxPlateThickness = value; }

    public double AssemblyElevation { get; set; } // 实际安装就位标高 (m)
    [NotMapped]
    public double InstallationElevationMeter { get => AssemblyElevation; set => AssemblyElevation = value; }

    public double SingleWeight { get; set; } // 单件吊装吨位 (t)
    [NotMapped]
    public double PieceWeightTon { get => SingleWeight; set => SingleWeight = value; }

    public int Quantity { get; set; } = 1; // 工程套数/根数
    public bool HasFieldWelding { get; set; } = false; // 是否发往现场焊接 (发现场全熔透坡口焊)
    [NotMapped]
    public bool IsSiteWelded { get => HasFieldWelding; set => HasFieldWelding = value; }

    public string CraneTonnageTier { get; set; } = "<5t"; // 起重机吨位分档 (<5t, 5-15t, >25t)
    public double FireRatingHours { get; set; } = 3.0; // 设计耐火极限 (3.0H, 2.5H, 1.5H)
    [NotMapped]
    public double DesignFireResistanceHours { get => FireRatingHours; set => FireRatingHours = value; }
    public string FireProofType { get; set; } = "厚型非膨胀"; // 防火涂料类型

    public double CoatingAreaPerUnit { get; set; } // 单件展开表面积 (m2)
    public double SubProjectNetWeightKg { get; set; } // 理论总净重 (kg)
    public double SubProjectGrossWeightKg { get; set; } // 采购总毛重 (kg, 含15%损耗)
    public double SubProjectSurfaceAreaM2 { get; set; } // 展开总表面积 (m2)

    // 成本汇总字段
    public double TotalPartsMaterialCost { get; set; } // 材料总费用 (元)
    public double UnitFabricationCost { get; set; } // 加工制作工费单价 (元/t)
    public double TotalFabricationCost { get; set; } // 加工制作总费用 (元)
    public double UnitInstallationCost { get; set; } // 现场安装工费单价 (元/t)
    public double TotalInstallationCost { get; set; } // 现场安装总费用 (元)
    public double TotalFireCoatingCost { get; set; } // 防火防腐总费用 (元)
    public double UnitDirectCost { get; set; } // 直接成本底价 (元/t)

    // Navigation
    public FabricationQuotaRule? FabricationQuotaRule { get; set; }
    public InstallationQuotaRule? InstallationQuotaRule { get; set; }
    public List<BoqPart> Parts { get; set; } = new();
}

/// <summary>
/// 下料零件实体：直接对应材料费模块，包含 1.15 倍损耗毛重与加价明细
/// </summary>
public class BoqPart
{
    public int Id { get; set; }
    public int SubProjectId { get; set; }
    [JsonIgnore]
    public BoqSubProject? SubProject { get; set; }

    public string PartMark { get; set; } = string.Empty; // 如 "HLP-15", "HLP-144"
    public string SectionSpecification { get; set; } = string.Empty; // 如 "PIP1200*50", "PL50*2150"
    [NotMapped]
    public string Specification { get => SectionSpecification; set => SectionSpecification = value; }

    public string SectionType { get; set; } = "PL"; // PIP, PL, BOX, H, CHS, RHS 等
    public string SteelGrade { get; set; } = "Q355B"; // Q355B, Q460GJC
    [NotMapped]
    public string MaterialGrade { get => SteelGrade; set => SteelGrade = value; }

    public double Thickness { get; set; } // mm
    [NotMapped]
    public double PlateThicknessMm { get => Thickness; set => Thickness = value; }
    public double WallThicknessMm { get; set; }
    public double DiameterOrWidthMm { get; set; }
    [NotMapped]
    public double Width { get => DiameterOrWidthMm; set => DiameterOrWidthMm = value; }

    public double Length { get; set; } // mm
    [NotMapped]
    public double CutLengthMm { get => Length; set => Length = value; }

    public double LinearDensity { get; set; } // kg/m
    [NotMapped]
    public double LinearWeightKgM { get => LinearDensity; set => LinearDensity = value; }

    public int Quantity { get; set; } = 1;
    [NotMapped]
    public int QuantityPerSubProject { get => Quantity; set => Quantity = value; }

    public double SingleNetWeight { get; set; } // kg
    [NotMapped]
    public double SingleNetWeightKg { get => SingleNetWeight; set => SingleNetWeight = value; }
    public double TotalNetWeight { get; set; } // kg
    public double PurchasingWasteFactor { get; set; } = 1.15; // 默认 1.15 采购损耗系数
    [NotMapped]
    public double LossRate
    {
        get => PurchasingWasteFactor >= 1.0 ? Math.Round(PurchasingWasteFactor - 1.0, 4) : 0.15;
        set => PurchasingWasteFactor = 1.0 + value;
    }
    public double TotalGrossWeight { get; set; } // kg (TotalNetWeight * PurchasingWasteFactor)
    [NotMapped]
    public double TotalGrossWeightKg { get => TotalGrossWeight; set => TotalGrossWeight = value; }
    [NotMapped]
    public double SingleGrossWeightKg => SingleNetWeight * PurchasingWasteFactor;

    public double SingleSurfaceAreaM2 { get; set; }
    public double TotalSurfaceAreaM2 { get; set; }

    public string ZDirectionGrade { get; set; } = "无"; // "无", "Z15", "Z25", "Z35"
    [NotMapped]
    public string ZPerformance { get => ZDirectionGrade; set => ZDirectionGrade = value; }
    public bool IsSiteWelded { get; set; } = false;
    public string BillCategory { get; set; } = string.Empty;

    // 定价明细字段
    public string MaterialPricingKey { get; set; } = string.Empty;
    public string MaterialPriceSnapshotId { get; set; } = string.Empty;
    public double BaseNetPricePerTon { get; set; }
    public double ThicknessSurchargePerTon { get; set; }
    public double GradeSurchargePerTon { get; set; }
    public double ZPerformanceSurchargePerTon { get; set; }
    public double FreightSurchargePerTon { get; set; }
    public double MaterialUnitPricePerTon { get; set; } // 综合材料单价 (元/t)
    [NotMapped]
    public double ComprehensiveMaterialUnitPricePerTon
    {
        get => MaterialUnitPricePerTon;
        set => MaterialUnitPricePerTon = value;
    }
    public double TotalPartMaterialCost { get; set; } // 材料合价 (元) = TotalGrossWeight / 1000 * MaterialUnitPricePerTon

    // 采办经营创效 (剪刀差)
    public double ApprovedPricePerTon { get; set; } // 业主批复信息价 (元/t)
    public double ActualPurchasePricePerTon { get; set; } // 施工方实际采办价 (元/t)
    public double ArbitrageProfit { get; set; } // 剪刀差利润 = (Approved - Actual) * (TotalGrossWeight / 1000)
}

/// <summary>
/// 车间加工制作定额规则实体
/// </summary>
public class FabricationQuotaRule
{
    public int Id { get; set; }
    public string QuotaCode { get; set; } = string.Empty; // 如 "FAB_SPACE_TRUSS_MAIN"
    public string QuotaName { get; set; } = string.Empty; // 如 "大跨空间管桁架主肢车间制作"
    public string StructuralSystem { get; set; } = string.Empty; // "空间管桁架", "副桁架", "箱型柱"
    [NotMapped]
    public string ApplicableCategory { get => StructuralSystem; set => StructuralSystem = value; }

    public double BaseLaborDays { get; set; } = 1.8; // 基准加工工时 (工日/t)
    [NotMapped]
    public double BaseLaborDaysPerTon { get => BaseLaborDays; set => BaseLaborDays = value; }

    public double BaseLaborRatePerTon { get; set; } = 1500.0; // 基准直接加工工费 (元/t)
    [NotMapped]
    public double BaseFabricationFeePerTon { get => BaseLaborRatePerTon; set => BaseLaborRatePerTon = value; }

    public double IntersectingCuttingUnitRate { get; set; } = 280.0; // 数控相贯五轴切割与坡口附加费 (元/t)
    [NotMapped]
    public double IntersectingCutSurcharge { get => IntersectingCuttingUnitRate; set => IntersectingCuttingUnitRate = value; }

    public double UltrasonicTestingUnitRate { get; set; } = 150.0; // 全熔透超声波探伤费 (元/t)
    [NotMapped]
    public double UltrasonicTestingFeePerTon { get => UltrasonicTestingUnitRate; set => UltrasonicTestingUnitRate = value; }

    public double PreassemblyUnitRate { get; set; } = 200.0; // 厂内预拼装费 (元/t)
    [NotMapped]
    public double TrialAssemblyFeePerTon { get => PreassemblyUnitRate; set => PreassemblyUnitRate = value; }
}

/// <summary>
/// 现场安装定额规则实体
/// </summary>
public class InstallationQuotaRule
{
    public int Id { get; set; }
    public string QuotaCode { get; set; } = string.Empty; // 如 "INS_SPACE_TRUSS_H30M"
    public string QuotaName { get; set; } = string.Empty; // 如 "空间管桁架高空原位散装全熔透焊"
    public string ApplicableRole { get; set; } = string.Empty; // "主桁架", "副桁架", "柱"

    public double BaseLaborDays { get; set; } = 2.5; // 基准安装工时 (工日/t)
    [NotMapped]
    public double BaseInstallationLaborDaysPerTon { get => BaseLaborDays; set => BaseLaborDays = value; }

    public double BaseLaborRatePerTon { get; set; } = 1800.0; // 基准现场安装工费 (元/t)
    [NotMapped]
    public double BaseInstallationFeePerTon { get => BaseLaborRatePerTon; set => BaseLaborRatePerTon = value; }

    public double MainCraneRatePerTon { get; set; } = 650.0; // 主起重机械费 (元/t)
    [NotMapped]
    public double BaseCraneCostPerTon { get => MainCraneRatePerTon; set => MainCraneRatePerTon = value; }

    public double TrestlePlatformRatePerTon { get; set; } = 300.0; // 拼装胎架与操作平台摊销费 (元/t)
    [NotMapped]
    public double BaseScaffoldingFeePerTon { get => TrestlePlatformRatePerTon; set => TrestlePlatformRatePerTon = value; }

    public double ElevationSurchargeFactor { get; set; } = 1.0; // 标高递增系数
}

/// <summary>
/// 报表受众视角枚举
/// </summary>
public enum BoqAudiencePerspective
{
    Owner = 0,                  // 业主 / 招投标 / 审计清单模式 (脱敏底价)
    ChiefEconomist = 1,         // 核心成果: 公司经营层 / 总经济师 (终极全成本穿透大盘)
    FabricationShop = 2,        // 构件加工厂与车间制作排产模式
    SiteProjectDept = 3,        // 现场项目部 / 吊装队长 (现场安装与机械吊装模式)
    Procurement = 4,            // 采办物资部 / 钢厂采购订货模式

    // 方案与文档兼容别名
    OwnerAudit = 0,
    ClientTenderMaster = 0,
    UltimateCostAnalysis = 1,
    FabricationWorkshop = 2,
    SiteErectionSchedule = 3,
    SiteErection = 3,
    Site = 3,
    MaterialProcurementOrder = 4
}

/// <summary>
/// 方案受众模式兼容枚举
/// </summary>
public enum BoqReportAudienceMode
{
    ClientTenderMaster = 0,
    UltimateCostAnalysis = 1,
    FabricationWorkshop = 2,
    SiteErectionSchedule = 3,
    MaterialProcurementOrder = 4
}

/// <summary>
/// 开放式报表配置类
/// </summary>
public class BoqReportConfig
{
    public BoqAudiencePerspective AudiencePerspective { get; set; } = BoqAudiencePerspective.ChiefEconomist;

    [NotMapped]
    public BoqReportAudienceMode AudienceMode
    {
        get => (BoqReportAudienceMode)(int)AudiencePerspective;
        set => AudiencePerspective = (BoqAudiencePerspective)(int)value;
    }

    public bool ShowGrossWeight { get; set; } = true; // 是否按 1.15 采购毛重口径
    public bool MaskInternalDirectCosts { get; set; } = false; // 是否对内部直接成本脱敏
    public bool IncludeFireProtectionSummary { get; set; } = true; // 是否包含耐火防火汇总
    public bool EnableCustomRateHighlight { get; set; } = true; // 是否高亮独立覆盖费率
    public List<string> VisibleColumns { get; set; } = new(); // 动态可见列选择器
}

/// <summary>
/// 单个零件的核算结果 DTO
/// </summary>
public class PartCostResult
{
    public int PartId { get; set; }
    public string PartMark { get; set; } = string.Empty;
    public double NetWeightKg { get; set; }
    public double GrossWeightKg { get; set; } // NetWeightKg * PurchasingWasteFactor
    public double MaterialUnitPricePerTon { get; set; }
    public double TotalPartCost { get; set; }
    public double ArbitrageProfit { get; set; } // 采办剪刀差创效
}

/// <summary>
/// 构件级的核算结果 DTO
/// </summary>
public class SubProjectCostResult
{
    public int SubProjectId { get; set; }
    public string SubProjectCode { get; set; } = string.Empty;
    public string SubProjectName { get; set; } = string.Empty;
    public double TotalNetWeightKg { get; set; }
    public double TotalGrossWeightKg { get; set; }
    public double TotalSurfaceAreaM2 { get; set; }
    public double MaterialCost { get; set; }
    public double FabricationCost { get; set; }
    public double InstallationCost { get; set; }
    public double FireCoatingCost { get; set; }
    public double DirectCost { get; set; }
    public double UnitDirectCostPerTon { get; set; }
    public double KThick { get; set; } = 1.0;
    public double KElev { get; set; } = 1.0;
    public double KSiteWeld { get; set; } = 1.0;
    public double KWeight { get; set; } = 1.0;
    public List<PartCostResult> PartResults { get; set; } = new();
}

/// <summary>
/// 清单项级的核算结果 DTO
/// </summary>
public class TenderItemCostResult
{
    public int TenderItemId { get; set; }
    public string ItemTag { get; set; } = string.Empty;
    public string BillItemCode { get; set; } = string.Empty;
    public string BillItemName { get; set; } = string.Empty;
    public double TenderQuantityTon { get; set; }
    public double DirectCostSum { get; set; }
    public double DirectCostPerTon { get; set; }
    public bool IsCustomRateApplied { get; set; }
    public double EffectiveManagementFeeRate { get; set; }
    public double EffectiveProfitRate { get; set; }
    public double ComprehensiveUnitPrice { get; set; }
    public double TotalTenderPrice { get; set; }
    public List<SubProjectCostResult> SubProjectResults { get; set; } = new();
}

/// <summary>
/// 项目级全局全成本穿透大盘结果 DTO
/// </summary>
public class ProjectCostResult
{
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public double TotalNetWeightKg { get; set; }
    public double TotalNetWeightTon => TotalNetWeightKg / 1000.0;
    public double TotalGrossWeightKg { get; set; }
    public double TotalGrossWeightTon => TotalGrossWeightKg / 1000.0;
    public double TotalSurfaceAreaM2 { get; set; }
    public double TotalMaterialCost { get; set; }
    public double TotalFabricationCost { get; set; }
    public double TotalInstallationCost { get; set; }
    public double TotalFireCoatingCost { get; set; }
    public double TotalDirectCost { get; set; }
    public double TotalBidPrice { get; set; }
    public double TotalManagementFee { get; set; }
    public double TotalProfit { get; set; }
    public double TotalProcurementScissorProfit { get; set; } // 采办剪刀差创效总利润
    public List<TenderItemCostResult> TenderItemResults { get; set; } = new();
}

/// <summary>
/// 多受众投影报表结果 (供 UI 与 Excel 导出) DTO
/// </summary>
public class AudienceProjectionResult
{
    public BoqAudiencePerspective Perspective { get; set; }
    public string PerspectiveName { get; set; } = string.Empty;
    public List<string> Headers { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public Dictionary<string, object?> SummaryMetrics { get; set; } = new();

    // Strongly-Typed Row Collections (One populated according to perspective)
    public List<ClientTenderMasterRow>? ClientTenderRows { get; set; }
    public List<UltimateCostAnalysisRow>? UltimateCostRows { get; set; }
    public List<FabricationWorkshopRow>? FabricationRows { get; set; }
    public List<SiteErectionScheduleRow>? SiteErectionRows { get; set; }
    public List<MaterialProcurementOrderRow>? MaterialProcurementRows { get; set; }
}

/// <summary>
/// 物资采办汇总项 DTO
/// </summary>
public class MaterialProcurementItem
{
    public string MaterialGrade { get; set; } = string.Empty;
    public string SectionType { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public double Thickness { get; set; }
    public string ZPerformance { get; set; } = "无";
    public int PartCount { get; set; }
    public double TotalNetWeightKg { get; set; }
    public double TotalGrossWeightKg { get; set; }
    public double MaterialUnitPricePerTon { get; set; }
    public double EstimatedMaterialCost { get; set; }
}

/// <summary>
/// 业主 / 招投标 / 审计清单行 DTO (脱敏内部定额与直接费底价)
/// </summary>
public class ClientTenderMasterRow
{
    public int Index { get; set; }
    public string BillItemCode { get; set; } = string.Empty; // 清单编码 (e.g. 010101001001)
    public string ItemTag { get; set; } = string.Empty;       // 清单代号 (e.g. NH, NF, JLC)
    public string BillItemName { get; set; } = string.Empty;   // 项目名称
    public string ProjectCharacteristics { get; set; } = string.Empty; // 项目特征及工作内容
    public string Unit { get; set; } = "t";                  // 计量单位 (t)
    public double TenderQuantity { get; set; }               // 投标工程量 (t)
    public double ComprehensiveUnitPrice { get; set; }       // 综合单价 (元/t)
    public double TotalTenderPrice { get; set; }             // 合价 (元)
    public string Remarks { get; set; } = string.Empty;      // 备注说明
}

/// <summary>
/// 总经济师 / 公司经营层终极全成本穿透大盘行 DTO (100%全要素穿透与采办剪刀差)
/// </summary>
public class UltimateCostAnalysisRow
{
    public int Index { get; set; }
    public string ItemTag { get; set; } = string.Empty;
    public string BillItemName { get; set; } = string.Empty;
    public double TenderQuantity { get; set; }               // 投标工程量 (t)
    public double NetWeightTon { get; set; }                 // 理论净重 (t)
    public double GrossWeightTon { get; set; }               // 1.15采购毛重 (t)
    public double MaterialCostSum { get; set; }              // 材料费小计 (元)
    public double MaterialTonCost { get; set; }              // 吨钢材料费 (元/t)
    public double FabricationCostSum { get; set; }           // 制作工费小计 (元)
    public double FabricationTonCost { get; set; }           // 吨钢制作费 (元/t)
    public double InstallationCostSum { get; set; }          // 安装工费小计 (元)
    public double InstallationTonCost { get; set; }          // 吨钢安装费 (元/t)
    public double FireCoatingCostSum { get; set; }           // 防火涂装小计 (元)
    public double FireCoatingTonCost { get; set; }           // 吨钢涂装费 (元/t)
    public double DirectCostSum { get; set; }                // 直接成本总额 (元)
    public double DirectCostPerTon { get; set; }             // 直接成本底价 (元/t)
    public bool IsCustomRateApplied { get; set; }            // 是否启用独立费率覆盖
    public string RateOverrideStatus { get; set; } = string.Empty; // "★单项覆盖" / "继承默认"
    public double EffectiveManagementFeeRate { get; set; }   // 有效管理费率 (如 0.10)
    public double ManagementFeeAmount { get; set; }          // 管理费额 (元)
    public double EffectiveProfitRate { get; set; }          // 有效利润率 (如 0.12)
    public double TargetProfitAmount { get; set; }           // 目标利润额 (元)
    public double ComprehensiveUnitPrice { get; set; }       // 对外综合单价 (元/t)
    public double TotalTenderPrice { get; set; }             // 最终投标总合价 (元)
    public double ScissorDifferenceProfit { get; set; }      // 采办剪刀差创效 (元)
    public double ComprehensiveNetProfit { get; set; }       // 综合毛利总额 = 目标利润 + 剪刀差
    public double NetMarginRate { get; set; }                // 综合毛利率 (%)
    public double ComprehensiveNetProfitPerTon { get; set; } // 吨钢综合净利 (元/t)
}

/// <summary>
/// 构件加工厂与车间制作排产行 DTO (聚焦板厚难度系数K_Thick、相贯切割、UT探伤与工日)
/// </summary>
public class FabricationWorkshopRow
{
    public int Index { get; set; }
    public string SubProjectCode { get; set; } = string.Empty; // 构件编码 (e.g. N-NHXX-48)
    public string SubProjectName { get; set; } = string.Empty; // 构件名称描述
    public string ItemTag { get; set; } = string.Empty;        // 所属清单 (e.g. NH)
    public string StructuralRole { get; set; } = string.Empty; // 工程角色 (主桁架/副桁架/柱/梁)
    public int Quantity { get; set; }                          // 构件套数/件数
    public double TotalNetWeightKg { get; set; }              // 总理论净重 (kg)
    public double TotalNetWeightTon { get; set; }             // 总理论净重 (t)
    public double TotalGrossWeightKg { get; set; }            // 采购下料毛重 (kg, 1.15倍)
    public double MaxPlateThickness { get; set; }             // 最大主板厚/管壁厚 (mm)
    public double KThick { get; set; }                         // 板厚加工难度调节系数 (1.00~1.40)
    public double BaseLaborRatePerTon { get; set; }           // 基准制作工费 (元/t)
    public double IntersectingCutSurcharge { get; set; }      // 相贯线切割费 (元/t)
    public double UltrasonicTestingFeePerTon { get; set; }    // UT无损探伤费 (元/t)
    public double TrialAssemblyFeePerTon { get; set; }        // 厂内预拼装费 (元/t)
    public double UnitFabricationCost { get; set; }           // 车间综合制作单价 (元/t)
    public double TotalFabricationCost { get; set; }          // 车间制作总工费 (元)
    public double BaseLaborDaysPerTon { get; set; }           // 基准工日定额 (工日/t)
    public double TotalLaborDays { get; set; }                // 车间总定额工日 (工日)
    public string WeldingNotes { get; set; } = string.Empty;  // 焊接工艺难点与预热消氢说明
}

/// <summary>
/// 现场项目部与机械吊装施工排程行 DTO (聚焦标高K_Elev、现场焊K_SiteWeld、吊重K_Weight)
/// </summary>
public class SiteErectionScheduleRow
{
    public int Index { get; set; }
    public string SubProjectCode { get; set; } = string.Empty; // 构件编码 (e.g. N-NHXX-49)
    public string SubProjectName { get; set; } = string.Empty; // 构件名称描述
    public string ItemTag { get; set; } = string.Empty;        // 所属清单项 (NH)
    public string StructuralRole { get; set; } = string.Empty; // 工程角色
    public int Quantity { get; set; }                          // 构件件数
    public double SingleWeightTon { get; set; }               // 单件吊装重量 (t)
    public double TotalNetWeightTon { get; set; }             // 吊装总重量 (t)
    public string InstallationMethod { get; set; } = string.Empty; // 拼装工法 (散拼 / 厂内组焊)
    public double AssemblyElevation { get; set; }             // 安装就位标高 (m)
    public double KElev { get; set; }                         // 标高超高调节系数 (1.00~1.30)
    public bool HasFieldWelding { get; set; }                 // 是否现场焊接 (发现场)
    public string FieldWeldingDisplay { get; set; } = string.Empty; // "是 (发现场)" / "否"
    public double KSiteWeld { get; set; }                     // 现场全熔透坡口焊系数 (1.30 或 1.00)
    public string CraneTonnageTier { get; set; } = string.Empty; // 起重机吨位分档 (<5t, 5-15t, >25t)
    public double KWeight { get; set; }                       // 起吊重力分档系数 (1.00~1.45)
    public double BaseInstallationFeePerTon { get; set; }     // 现场安装直接工费 (元/t)
    public double MainCraneRatePerTon { get; set; }           // 主吊机械台班费 (元/t)
    public double ScaffoldingPlatformRatePerTon { get; set; } // 胎架与操作平台摊销 (元/t)
    public double UnitInstallationCost { get; set; }          // 现场安装综合单价 (元/t)
    public double TotalInstallationCost { get; set; }         // 现场安装总费用 (元)
    public string CraneDeploymentRecommendation { get; set; } = string.Empty; // 机械配置建议
}

/// <summary>
/// 物资采办部与钢厂订货汇总行 DTO (跨构件母材归并、1.15倍采购损耗毛重、阶梯加价与剪刀差)
/// </summary>
public class MaterialProcurementOrderRow
{
    public int Index { get; set; }
    public string MaterialGrade { get; set; } = string.Empty; // 钢材材质 (Q460GJC, Q355B, Q235B)
    public string SectionType { get; set; } = string.Empty;   // 截面形式 (PL, PIP, BOX, H)
    public string Specification { get; set; } = string.Empty; // 规格型号 (PL50*2150, PIP1200*50)
    public double Thickness { get; set; }                     // 主板厚/壁厚 (mm)
    public string ZPerformance { get; set; } = "无";          // Z向性能 (无, Z15, Z25, Z35)
    public int PartCount { get; set; }                        // 归并零件总数量
    public double TotalNetWeightKg { get; set; }              // 理论净重 (kg)
    public double TotalNetWeightTon { get; set; }             // 理论净重 (t)
    public double PurchasingWasteFactor { get; set; } = 1.15; // 采购损耗乘数 (1.15)
    public double TotalGrossWeightKg { get; set; }            // 钢厂订货毛重 (kg, 净重 * 1.15)
    public double TotalGrossWeightTon { get; set; }           // 钢厂订货毛重 (t)
    public double BaseNetPricePerTon { get; set; }            // 基准到场价 (元/t)
    public double ThicknessSurchargePerTon { get; set; }      // 厚度加价 (元/t)
    public double GradeSurchargePerTon { get; set; }          // 材质加价 (元/t)
    public double ZPerformanceSurchargePerTon { get; set; }   // Z向性能加价 (元/t)
    public double FreightSurchargePerTon { get; set; }        // 运杂物流加价 (元/t)
    public double MaterialUnitPricePerTon { get; set; }       // 综合采购到场单价 (元/t)
    public double EstimatedProcurementCost { get; set; }      // 钢厂采购预算合价 (元)
    public double ApprovedPricePerTon { get; set; }           // 业主批复信息指导价 (元/t)
    public double ActualPurchasePricePerTon { get; set; }     // 施工企业实际采办单价 (元/t)
    public double ScissorDifferenceProfit { get; set; }       // 采办经营创效(剪刀差利润, 元)
    public string MillOrderingNotes { get; set; } = string.Empty; // 钢厂起订量与交货建议
}

/// <summary>
/// 钢材材质牌号交叉透视分析行 DTO (Tab 1 三维矩阵之材质维度)
/// </summary>
public class SteelGradeAnalyticsRow
{
    public string GradeName { get; set; } = string.Empty; // 如 "Q460GJC", "Q355B", "Q235B"
    public int PartCount { get; set; }                    // 归并零件总项数/件数
    public double TotalNetWeightTon { get; set; }         // 理论净重 (t)
    public double TotalGrossWeightTon { get; set; }       // 采购毛重 (1.15倍损耗, t)
    public double TotalMaterialCost { get; set; }         // 材料到场综合合价 (元)
    public double TotalScissorProfit { get; set; }        // 采办经营创效剪刀差利润 (元)
}
}

namespace EngineeringApp.Shared.BOQ.Engine
{
    // 占位标记：确保在 M2 引擎实现前 global using EngineeringApp.Shared.BOQ.Engine 正常解析
    internal static class EngineNamespaceMarker { }
}

namespace EngineeringApp.Shared.BOQ.Export
{
    // 占位标记：确保在 M2 导出器实现前 global using EngineeringApp.Shared.BOQ.Export 正常解析
    internal static class ExportNamespaceMarker { }
}
