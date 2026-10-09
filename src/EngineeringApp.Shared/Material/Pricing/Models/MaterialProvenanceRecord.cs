using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板与型材特征唯一性物料标签生成器 (Material Fingerprint & Provenance Tag Builder)
/// </summary>
public static class MaterialProvenanceTagBuilder
{
    public static string GeneratePlateTag(PlatePricingParameters p)
    {
        ArgumentNullException.ThrowIfNull(p);

        // 构造标准化特征字符串
        var rawSpec = new StringBuilder();
        rawSpec.Append($"{p.Standard}_{p.Grade}_T{p.ThicknessMm:0.##}_W{p.WidthMm:0}_L{p.LengthMm:0}_");
        rawSpec.Append($"{p.CutType}_{p.Tolerance}_{p.Flatness}_{p.Impact}_{p.ZDirection}_{p.Metallurgy}_{p.UT}_{p.Certificate}_");
        rawSpec.Append($"{p.SteelMillId}_{p.Delivery}_{p.DestinationRegion}");

        // 计算 6 位短哈希校验码，确保全局唯一性
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawSpec.ToString()));
        string shortHash = Convert.ToHexString(hashBytes)[..6];

        // 格式化人类可读的工程物料特征标签
        string destShort = p.DestinationRegion.Contains("广东") ? "GD"
                         : p.DestinationRegion.Contains("浙江") ? "ZJ"
                         : p.DestinationRegion.Contains("FOB") ? "FOB"
                         : "LOC";

        string tolCode = p.Tolerance == ToleranceClass.ClassC ? "TOL_C"
                       : p.Tolerance == ToleranceClass.ClassB ? "TOL_B" : "TOL_A";

        return $"PL-{p.Grade}-T{p.ThicknessMm:0.##}-{p.WidthMm:0}x{p.LengthMm:0}-{tolCode}-{p.SteelMillId}-{destShort}-{shortHash}";
    }
}

/// <summary>
/// 跨时序回溯时加价原则应用模式 (Pricing Principle Retrospection Mode)
/// </summary>
public enum RetrospectionPrincipleMode
{
    /// <summary>
    /// 【模式 A：合同锁定加价原则】：沿用原询价建档时约定的加价原则 (固定加价)，仅回溯大盘基价波动
    /// </summary>
    ContractFixedPrinciples,

    /// <summary>
    /// 【模式 B：目标时序现行原则】：厚度/定尺/公差等加价规则完全按照目标快照时期的最新规则矩阵重算
    /// </summary>
    TargetDateCurrentPrinciples,

    /// <summary>
    /// 【模式 C：最新修订与实付议价】：直接以该物料最新修订履历中的实付加价为准
    /// </summary>
    LatestRevisedOverride
}

/// <summary>
/// 物料价格与加价原则变更履历 (Material Price & Principle Revision Audit Log)
/// 记录由于“加价原则变更”、“询价时间差异”或“二次议价”引发的修改过程与痕迹
/// </summary>
public class MaterialPriceRevisionLog
{
    public int Id { get; set; }
    public int RecordId { get; set; }
    public int RevisionNumber { get; set; }
    public DateTime RevisedAt { get; set; } = DateTime.Now;
    public string RevisedBy { get; set; } = "造价工程师";
    public string Reason { get; set; } = ""; // 变更原因，如“加价原则调整”、“不同询价日重核”、“现货加价政策变更”
    public double PreviousPricePerTon { get; set; }
    public double NewPricePerTon { get; set; }
    public double PriceDelta => NewPricePerTon - PreviousPricePerTon;
    public string ChangeDetails { get; set; } = ""; // 明细对比文本
    public string AppliedPrinciple { get; set; } = ""; // 本次适用的加价原则版本
}

/// <summary>
/// 材料特征出处记录模型 (Material Provenance Record)
/// 记录每张特定特性的钢板/型材在特定时间节点的完整成本拆解出处，支持跨时序回溯与项目成本重算
/// </summary>
public class MaterialProvenanceRecord
{
    public int Id { get; set; }

    /// <summary>物料特征唯一性标签 (材料指纹编码)</summary>
    public string MaterialTag { get; set; } = "";

    /// <summary>物料显示名称 (如 "Q355B 钢板 t=22mm (南钢/广东落地)")</summary>
    public string DisplayTitle { get; set; } = "";

    /// <summary>材料品类 ("Plate", "Profile_RHS_R2S", "Profile_H")</summary>
    public string Category { get; set; } = "Plate";

    /// <summary>标准规范描述 (如 "Q355B-C类公差-TMCP")</summary>
    public string StandardSpecification { get; set; } = "";

    /// <summary>规格尺寸标注 (如 "t=22mm (2500×12000)")</summary>
    public string DimensionText { get; set; } = "";

    /// <summary>建档关联价格快照编号</summary>
    public string SnapshotId { get; set; } = "";

    /// <summary>快照名称 (如 "2026年3月锁价基准期" 或 "2026年10月最新市场网价")</summary>
    public string SnapshotName { get; set; } = "";

    /// <summary>价格生效日期 (时间标签)</summary>
    public DateTime EffectiveDate { get; set; } = DateTime.Today;

    /// <summary>入库归档时间</summary>
    public DateTime RecordedAt { get; set; } = DateTime.Now;

    /// <summary>所属工程项目或构件批次引用 (如 "PROJECT-GZ-2026")</summary>
    public string ProjectReference { get; set; } = "默认工程项目";

    /// <summary>钢厂品牌名称</summary>
    public string MillName { get; set; } = "";

    /// <summary>交货目的地与物流描述</summary>
    public string DeliveryAndFreightText { get; set; } = "";

    // ================= 询价时间差异与批次管理 =================
    /// <summary>询价批次编号 (如 "INQ-20260315-LOT01")</summary>
    public string InquiryBatchId { get; set; } = "INQ-LOT-01";

    /// <summary>具体询价时间 (可不同于大盘基准快照日期，体现同一工程不同时间批次询价差异)</summary>
    public DateTime InquiryDate { get; set; } = DateTime.Today;

    /// <summary>询价渠道/报价供应商 (如 "宝武直供", "南钢现货仓", "华东分销商")</summary>
    public string InquiryVendor { get; set; } = "主流钢厂直供通道";

    /// <summary>当前修订版本号 (初始询价为 Rev 1，每次修改后累加递增)</summary>
    public int RevisionNumber { get; set; } = 1;

    /// <summary>当期适用的加价原则版本或政策依据说明 (如 "2026Q1钢厂加价公函", "2026-10现货紧平衡新规")</summary>
    public string PricingPrincipleNote { get; set; } = "行业出厂常规阶梯与定尺规则";

    /// <summary>过程修改与调价历史履历 (Audit Trail & Revision History)</summary>
    public List<MaterialPriceRevisionLog> RevisionLogs { get; set; } = [];

    // ================= 建档时的单价拆解 (元/吨) =================
    public double BasePricePerTon { get; set; }
    public double MillPremiumPerTon { get; set; }
    public double ThicknessSurchargePerTon { get; set; }
    public double DimensionSurchargePerTon { get; set; }
    public double ToleranceSurchargePerTon { get; set; }
    public double PerformanceSurchargePerTon { get; set; }
    public double InspectionSurchargePerTon { get; set; }
    public double FreightPerTon { get; set; }

    /// <summary>初始合同约定的厚度加价 (用于模式A合同锁定回溯)</summary>
    public double ContractThicknessSurchargePerTon { get; set; }
    /// <summary>初始合同约定的调运费 (用于模式A合同锁定回溯)</summary>
    public double ContractFreightPerTon { get; set; }

    /// <summary>建档综合采购单价 (元/吨)</summary>
    public double FinalPricePerTon { get; set; }

    /// <summary>标准 Describe 规范契约文本</summary>
    public string FullDescription { get; set; } = "";

    /// <summary>完整的钢板参数对象 (供跨时序回溯与重算)</summary>
    public PlatePricingParameters PlateParameters { get; set; } = new();

    /// <summary>
    /// 对当前物料出处记录，指定任意历史或未来价格快照及加价原则模式进行时序价格回溯核算
    /// </summary>
    public StandardMaterialItemPrice CalculateAtSnapshot(
        MaterialPriceSnapshot targetSnapshot,
        IList<PlateThicknessLadder>? ladders = null,
        IList<PlateDimensionRule>? dimRules = null,
        RetrospectionPrincipleMode principleMode = RetrospectionPrincipleMode.TargetDateCurrentPrinciples)
    {
        ArgumentNullException.ThrowIfNull(targetSnapshot);

        if (principleMode == RetrospectionPrincipleMode.ContractFixedPrinciples)
        {
            // 模式 A：合同锁价加价模式。加价项沿用原合同/初次询价时约定的固定加价，仅大盘基价随目标快照浮动
            var calculated = PlatePricingRuleEngine.Calculate(PlateParameters, targetSnapshot, ladders, dimRules);
            calculated.MillPremiumPerTon = MillPremiumPerTon;
            calculated.ThicknessSurchargePerTon = ContractThicknessSurchargePerTon > 0 ? ContractThicknessSurchargePerTon : ThicknessSurchargePerTon;
            calculated.DimensionSurchargePerTon = DimensionSurchargePerTon;
            calculated.ToleranceSurchargePerTon = ToleranceSurchargePerTon;
            calculated.PerformanceSurchargePerTon = PerformanceSurchargePerTon;
            calculated.InspectionSurchargePerTon = InspectionSurchargePerTon;
            calculated.FreightPerTon = ContractFreightPerTon > 0 ? ContractFreightPerTon : FreightPerTon;
            return calculated;
        }

        if (principleMode == RetrospectionPrincipleMode.LatestRevisedOverride)
        {
            // 模式 C：最新修订与实付议价模式。直接以该物料最新修订履历中的实付加价与最新协商运费为准
            var calculated = PlatePricingRuleEngine.Calculate(PlateParameters, targetSnapshot, ladders, dimRules);
            calculated.MillPremiumPerTon = MillPremiumPerTon;
            calculated.ThicknessSurchargePerTon = ThicknessSurchargePerTon;
            calculated.DimensionSurchargePerTon = DimensionSurchargePerTon;
            calculated.ToleranceSurchargePerTon = ToleranceSurchargePerTon;
            calculated.PerformanceSurchargePerTon = PerformanceSurchargePerTon;
            calculated.InspectionSurchargePerTon = InspectionSurchargePerTon;
            calculated.FreightPerTon = FreightPerTon;
            return calculated;
        }

        // 模式 B：目标时序现行原则模式。厚度/定尺等加价规则完全按照目标快照时期的最新规则矩阵重算
        var dynamicParams = new PlatePricingParameters
        {
            Standard = PlateParameters.Standard,
            Grade = PlateParameters.Grade,
            ThicknessMm = PlateParameters.ThicknessMm,
            WidthMm = PlateParameters.WidthMm,
            LengthMm = PlateParameters.LengthMm,
            CutType = PlateParameters.CutType,
            Tolerance = PlateParameters.Tolerance,
            Flatness = PlateParameters.Flatness,
            Impact = PlateParameters.Impact,
            ZDirection = PlateParameters.ZDirection,
            Metallurgy = PlateParameters.Metallurgy,
            UT = PlateParameters.UT,
            Certificate = PlateParameters.Certificate,
            SteelMillId = PlateParameters.SteelMillId,
            Delivery = PlateParameters.Delivery,
            DestinationRegion = PlateParameters.DestinationRegion,
            CustomThicknessSurcharge = null,
            CustomDimensionSurcharge = null,
            CustomMillPremium = PlateParameters.CustomMillPremium,
            CustomFreightPerTon = PlateParameters.CustomFreightPerTon
        };
        return PlatePricingRuleEngine.Calculate(dynamicParams, targetSnapshot, ladders, dimRules);
    }

    /// <summary>
    /// 记录一次价格或原则变更过程，生成审计履历并递增版本号
    /// </summary>
    public void AddRevision(string reason, string revisedBy, double newPricePerTon, string changeDetails, string appliedPrinciple)
    {
        var log = new MaterialPriceRevisionLog
        {
            Id = RevisionLogs.Count + 1,
            RecordId = Id,
            RevisionNumber = RevisionNumber + 1,
            RevisedAt = DateTime.Now,
            RevisedBy = string.IsNullOrWhiteSpace(revisedBy) ? "造价工程师" : revisedBy,
            Reason = string.IsNullOrWhiteSpace(reason) ? "工程询价单价修正" : reason,
            PreviousPricePerTon = FinalPricePerTon,
            NewPricePerTon = newPricePerTon,
            ChangeDetails = changeDetails,
            AppliedPrinciple = appliedPrinciple
        };

        RevisionLogs.Add(log);
        RevisionNumber = log.RevisionNumber;
        FinalPricePerTon = newPricePerTon;
        PricingPrincipleNote = appliedPrinciple;
    }
}
