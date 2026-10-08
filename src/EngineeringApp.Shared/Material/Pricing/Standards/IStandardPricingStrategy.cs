using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 国际化标准体系定价策略接口 (Strategy Pattern)
/// </summary>
public interface IStandardPricingStrategy
{
    StandardSystem System { get; }

    /// <summary>根据牌号、冲击功、Z向性能及控轧工艺格式化标准材质规格字符串</summary>
    string FormatStandardSpecification(SteelGrade grade, ImpactGrade impact, ZDirectionQuality zQuality, MetallurgyProcess process);

    /// <summary>获取对应标准下的材质大盘基准单价</summary>
    double GetBasePrice(MaterialPriceSnapshot snapshot, SteelGrade grade, bool isPlate);

    /// <summary>获取冲击功与温度韧性加价</summary>
    double GetImpactSurcharge(ImpactGrade impact);

    /// <summary>获取厚度公差与精度加价</summary>
    double GetToleranceSurcharge(ToleranceClass tolerance);

    /// <summary>获取厚度方向抗撕裂 Z 向加价</summary>
    double GetZDirectionSurcharge(ZDirectionQuality zQuality);

    /// <summary>获取冶金工艺与热处理加价</summary>
    double GetMetallurgySurcharge(MetallurgyProcess process);

    /// <summary>获取质量检验认证证书加价 (如 EN 10204 3.2 见证加价)</summary>
    double GetCertificateSurcharge(InspectionCertificateType certType);

    /// <summary>获取公差规范的本地化标准名称</summary>
    string GetLocalizedToleranceName(ToleranceClass tolerance);
}
