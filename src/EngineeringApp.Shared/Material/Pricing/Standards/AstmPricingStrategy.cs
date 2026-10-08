using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 美国标准 (ASTM / AISC) 定价与规格策略实现
/// </summary>
public class AstmPricingStrategy : IStandardPricingStrategy
{
    public StandardSystem System => StandardSystem.ASTM;

    public string FormatStandardSpecification(SteelGrade grade, ImpactGrade impact, ZDirectionQuality zQuality, MetallurgyProcess process)
    {
        string baseName = grade switch
        {
            SteelGrade.A36 or SteelGrade.Q235B => "ASTM A36",
            SteelGrade.A572_Gr50 or SteelGrade.Q355B or SteelGrade.Q355D => "ASTM A572 Gr.50",
            SteelGrade.A992 => "ASTM A992",
            _ => "ASTM A572 Gr.50"
        };

        string spec = baseName;

        if (impact != ImpactGrade.GradeB)
        {
            spec += " (CVN Tested)";
        }

        if (zQuality != ZDirectionQuality.None)
        {
            spec += $"-{zQuality}";
        }

        if (process == MetallurgyProcess.Normalized)
        {
            spec += " (Normalized)";
        }
        else if (process == MetallurgyProcess.TMCP)
        {
            spec += " (TMCP)";
        }

        return spec;
    }

    public double GetBasePrice(MaterialPriceSnapshot snapshot, SteelGrade grade, bool isPlate)
    {
        double baseRate = isPlate ? snapshot.PlateBaseQ355B : snapshot.HBeamBaseQ355B;
        return grade switch
        {
            SteelGrade.A36 or SteelGrade.Q235B => isPlate ? snapshot.PlateBaseQ235B + 120.0 : snapshot.HBeamBaseQ235B + 120.0,
            SteelGrade.A992 => snapshot.HBeamBaseQ355B + 160.0,
            _ => baseRate + 150.0 // 美标订货认证与检验溢价
        };
    }

    public double GetImpactSurcharge(ImpactGrade impact) => impact switch
    {
        ImpactGrade.GradeB => 0.0,
        _ => 180.0 // ASTM 夏比V型缺口(CVN)低温补充试验费
    };

    public double GetToleranceSurcharge(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassC => 160.0, // ASTM A6 严格限制厚度负公差
        ToleranceClass.ClassB => 70.0,
        _ => 0.0
    };

    public double GetZDirectionSurcharge(ZDirectionQuality zQuality) => zQuality switch
    {
        ZDirectionQuality.Z15 => 160.0,
        ZDirectionQuality.Z25 => 260.0,
        ZDirectionQuality.Z35 => 420.0,
        _ => 0.0
    };

    public double GetMetallurgySurcharge(MetallurgyProcess process) => process switch
    {
        MetallurgyProcess.Normalized => 180.0,
        MetallurgyProcess.TMCP => 120.0,
        MetallurgyProcess.QuenchedAndTempered => 320.0,
        _ => 0.0
    };

    public double GetCertificateSurcharge(InspectionCertificateType certType) => certType switch
    {
        InspectionCertificateType.StandardMTC => 0.0,
        InspectionCertificateType.EN10204_31 => 40.0,
        InspectionCertificateType.EN10204_32 => 480.0,
        _ => 0.0
    };

    public string GetLocalizedToleranceName(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassC => "ASTM A6 Restricted Thickness (Guaranteed Full)",
        ToleranceClass.ClassB => "ASTM A6 Strict Underrun Limit (≤0.01 in)",
        _ => "ASTM A6 Standard Table A1.1 Permissible Variations"
    };
}
