using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 欧洲标准 (EN / EN 10025 / EN 10029) 定价与规格策略实现
/// </summary>
public class EnPricingStrategy : IStandardPricingStrategy
{
    public StandardSystem System => StandardSystem.EN;

    public string FormatStandardSpecification(SteelGrade grade, ImpactGrade impact, ZDirectionQuality zQuality, MetallurgyProcess process)
    {
        string baseName = grade switch
        {
            SteelGrade.S235JR or SteelGrade.Q235B => "S235",
            SteelGrade.S355JR or SteelGrade.S355J2 or SteelGrade.Q355B or SteelGrade.Q355D => "S355",
            SteelGrade.S460N or SteelGrade.Q420B => "S460",
            _ => "S355"
        };

        string impactSuffix = impact switch
        {
            ImpactGrade.GradeB => "JR",
            ImpactGrade.GradeC => "J0",
            ImpactGrade.GradeD => "J2",
            ImpactGrade.GradeE => "K2",
            _ => "JR"
        };

        string processSuffix = process switch
        {
            MetallurgyProcess.Normalized => "+N",
            MetallurgyProcess.TMCP => "+M",
            MetallurgyProcess.QuenchedAndTempered => "+Q",
            _ => "+AR"
        };

        string spec = $"EN 10025 {baseName}{impactSuffix}{processSuffix}";

        if (zQuality != ZDirectionQuality.None)
        {
            spec += $"-{zQuality}";
        }

        return spec;
    }

    public double GetBasePrice(MaterialPriceSnapshot snapshot, SteelGrade grade, bool isPlate)
    {
        double baseRate = isPlate ? snapshot.PlateBaseQ355B : snapshot.HBeamBaseQ355B;
        return grade switch
        {
            SteelGrade.S235JR or SteelGrade.Q235B => isPlate ? snapshot.PlateBaseQ235B + 100.0 : snapshot.HBeamBaseQ235B + 100.0,
            SteelGrade.S460N or SteelGrade.Q420B => baseRate + 550.0,
            _ => baseRate + 120.0 // 欧标出口标牌认证基础溢价
        };
    }

    public double GetImpactSurcharge(ImpactGrade impact) => impact switch
    {
        ImpactGrade.GradeB => 0.0,
        ImpactGrade.GradeC => 60.0,
        ImpactGrade.GradeD => 150.0, // J2 (-20℃ 27J)
        ImpactGrade.GradeE => 280.0, // K2 (-20℃ 40J)
        _ => 0.0
    };

    public double GetToleranceSurcharge(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassA => 0.0,
        ToleranceClass.ClassB => 60.0,
        ToleranceClass.ClassC => 150.0, // EN 10029 Class C (Zero underrun)
        _ => 0.0
    };

    public double GetZDirectionSurcharge(ZDirectionQuality zQuality) => zQuality switch
    {
        ZDirectionQuality.Z15 => 150.0,
        ZDirectionQuality.Z25 => 250.0,
        ZDirectionQuality.Z35 => 400.0,
        _ => 0.0
    };

    public double GetMetallurgySurcharge(MetallurgyProcess process) => process switch
    {
        MetallurgyProcess.Normalized => 180.0,
        MetallurgyProcess.TMCP => 100.0,
        MetallurgyProcess.QuenchedAndTempered => 300.0,
        _ => 0.0
    };

    public double GetCertificateSurcharge(InspectionCertificateType certType) => certType switch
    {
        InspectionCertificateType.StandardMTC => 0.0,
        InspectionCertificateType.EN10204_31 => 30.0,
        InspectionCertificateType.EN10204_32 => 450.0, // BV / DNV / TÜV 第三方独立见证签发
        _ => 0.0
    };

    public string GetLocalizedToleranceName(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassA => "EN 10029 Class A (Standard lower deviation)",
        ToleranceClass.ClassB => "EN 10029 Class B (Fixed lower dev -0.3mm)",
        ToleranceClass.ClassC => "EN 10029 Class C (Zero minus, Full thickness)",
        _ => tolerance.ToString()
    };
}
