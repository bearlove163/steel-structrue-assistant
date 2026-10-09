using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 标准化材料价格项核算结果与对外契约模型
/// </summary>
public class StandardMaterialItemPrice
{
    public StandardSystem Standard { get; set; } = StandardSystem.GB;

    /// <summary>材料品类分类 ("Plate", "Profile_H", "Profile_RHS_R2S", "Profile_CHS")</summary>
    public string MaterialType { get; set; } = "Plate";

    /// <summary>材质牌号</summary>
    public SteelGrade Grade { get; set; } = SteelGrade.Q355B;

    /// <summary>标准材质规范标注 (如 "Q355ND-Z25-TMCP" 或 "EN S355J2+N")</summary>
    public string StandardSpecification { get; set; } = "";

    /// <summary>钢厂品牌与产地基地</summary>
    public string MillName { get; set; } = "宝钢股份 (上海宝山)";

    /// <summary>规格尺寸标注文本 (如 "t=50mm (2500×12000)" 或 "HM 300×200×9×14")</summary>
    public string DimensionText { get; set; } = "";

    /// <summary>交货与调运描述文本</summary>
    public string DeliveryAndFreightText { get; set; } = "";

    /// <summary>【核心】工业级标准化 Describe 描述规范文本</summary>
    public string FullDescription { get; set; } = "";

    /// <summary>价格基准生效日期 (时间标签)</summary>
    public DateTime PriceEffectiveDate { get; set; } = DateTime.Today;

    /// <summary>是否含税 (13% 增值税)</summary>
    public bool IsTaxInclusive { get; set; } = true;

    // ================= 成本构成透明拆解 (元/吨) =================
    /// <summary>大盘基准价格</summary>
    public double BasePricePerTon { get; set; }

    /// <summary>钢厂品牌溢价</summary>
    public double MillPremiumPerTon { get; set; }
    public double BenchmarkMillPremium { get; set; }
    public bool IsMillPremiumCustomized { get; set; }

    /// <summary>厚度/规格加价</summary>
    public double ThicknessSurchargePerTon { get; set; }
    public double BenchmarkThicknessSurcharge { get; set; }
    public bool IsThicknessSurchargeCustomized { get; set; }
    public string ThicknessPrincipleExplanation { get; set; } = "";

    /// <summary>定尺/超长超宽加价</summary>
    public double DimensionSurchargePerTon { get; set; }
    public double BenchmarkDimensionSurcharge { get; set; }
    public bool IsDimensionSurchargeCustomized { get; set; }
    public string DimensionPrincipleExplanation { get; set; } = "";

    /// <summary>公差精度与保证全厚度加价 (GB/T 709 / EN 10029 Class C)</summary>
    public double ToleranceSurchargePerTon { get; set; }

    /// <summary>材质与性能加价 (冲击等级 + Z向抗撕裂 + TMCP/正火)</summary>
    public double PerformanceSurchargePerTon { get; set; }

    /// <summary>探伤与质保认证加价 (一级/二级探伤 + EN 10204 3.2证书)</summary>
    public double InspectionSurchargePerTon { get; set; }

    /// <summary>工艺成型工费 (圆转方冷成型费 / 无缝管连轧费)</summary>
    public double ProcessSurchargePerTon { get; set; }

    /// <summary>干线物流调运费 (产地至车间/现场)</summary>
    public double FreightPerTon { get; set; }
    public double BenchmarkFreight { get; set; }
    public bool IsFreightCustomized { get; set; }

    /// <summary>【核心】最终综合采购单价 (元/吨)</summary>
    public double FinalPricePerTon =>
        BasePricePerTon +
        MillPremiumPerTon +
        ThicknessSurchargePerTon +
        DimensionSurchargePerTon +
        ToleranceSurchargePerTon +
        PerformanceSurchargePerTon +
        InspectionSurchargePerTon +
        ProcessSurchargePerTon +
        FreightPerTon;

    /// <summary>价格分解公式汇总明细 (如 "基价4050 + 宝钢200 + 厚度180 + ... = 5370")</summary>
    public string PriceBreakdownSummary { get; set; } = "";
}
