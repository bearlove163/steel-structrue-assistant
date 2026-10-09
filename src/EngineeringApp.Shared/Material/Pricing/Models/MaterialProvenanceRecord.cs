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

    // ================= 建档时的单价拆解 (元/吨) =================
    public double BasePricePerTon { get; set; }
    public double MillPremiumPerTon { get; set; }
    public double ThicknessSurchargePerTon { get; set; }
    public double DimensionSurchargePerTon { get; set; }
    public double ToleranceSurchargePerTon { get; set; }
    public double PerformanceSurchargePerTon { get; set; }
    public double InspectionSurchargePerTon { get; set; }
    public double FreightPerTon { get; set; }

    /// <summary>建档综合采购单价 (元/吨)</summary>
    public double FinalPricePerTon { get; set; }

    /// <summary>标准 Describe 规范契约文本</summary>
    public string FullDescription { get; set; } = "";

    /// <summary>完整的钢板参数对象 (供跨时序回溯与重算)</summary>
    public PlatePricingParameters PlateParameters { get; set; } = new();

    /// <summary>
    /// 对当前物料出处记录，指定任意历史或未来价格快照进行时序价格回溯核算
    /// </summary>
    public StandardMaterialItemPrice CalculateAtSnapshot(
        MaterialPriceSnapshot targetSnapshot,
        IList<PlateThicknessLadder>? ladders = null,
        IList<PlateDimensionRule>? dimRules = null)
    {
        ArgumentNullException.ThrowIfNull(targetSnapshot);
        return PlatePricingRuleEngine.Calculate(PlateParameters, targetSnapshot, ladders, dimRules);
    }
}
