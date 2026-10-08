using EngineeringApp.Shared.Material;

namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 中国国家标准 (GB / GB/T) 定价与规格策略实现
/// </summary>
public class GbPricingStrategy : IStandardPricingStrategy
{
    public StandardSystem System => StandardSystem.GB;

    public string FormatStandardSpecification(SteelGrade grade, ImpactGrade impact, ZDirectionQuality zQuality, MetallurgyProcess process)
    {
        string baseName = grade switch
        {
            SteelGrade.Q235B => "Q235",
            SteelGrade.Q345B => "Q345",
            SteelGrade.Q355B or SteelGrade.Q355C or SteelGrade.Q355D => "Q355",
            SteelGrade.Q390B => "Q390",
            SteelGrade.Q420B => "Q420",
            _ => grade.ToString()
        };

        // 冲击等级后缀
        string impactSuffix = impact switch
        {
            ImpactGrade.GradeB => "B",
            ImpactGrade.GradeC => "C",
            ImpactGrade.GradeD => "D",
            ImpactGrade.GradeE => "E",
            _ => "B"
        };

        // 正火/TMCP前缀或后缀 (如 Q355ND 或 Q355D-TMCP)
        string normPrefix = (process == MetallurgyProcess.Normalized) ? "N" : "";
        string spec = $"{baseName}{normPrefix}{impactSuffix}";

        if (zQuality != ZDirectionQuality.None)
        {
            spec += $"-{zQuality}";
        }

        if (process == MetallurgyProcess.TMCP)
        {
            spec += "-TMCP";
        }
        else if (process == MetallurgyProcess.QuenchedAndTempered)
        {
            spec += "-QT";
        }

        return spec;
    }

    public double GetBasePrice(MaterialPriceSnapshot snapshot, SteelGrade grade, bool isPlate)
    {
        if (isPlate)
        {
            return grade switch
            {
                SteelGrade.Q235B => snapshot.PlateBaseQ235B,
                SteelGrade.Q345B => snapshot.PlateBaseQ355B - 30.0,
                SteelGrade.Q355B or SteelGrade.Q355C or SteelGrade.Q355D => snapshot.PlateBaseQ355B,
                SteelGrade.Q390B => snapshot.PlateBaseQ355B + 280.0,
                SteelGrade.Q420B => snapshot.PlateBaseQ355B + 450.0,
                _ => snapshot.PlateBaseQ355B
            };
        }
        else
        {
            return grade switch
            {
                SteelGrade.Q235B => snapshot.HBeamBaseQ235B,
                _ => snapshot.HBeamBaseQ355B
            };
        }
    }

    public double GetImpactSurcharge(ImpactGrade impact) => impact switch
    {
        ImpactGrade.GradeB => 0.0,
        ImpactGrade.GradeC => 60.0,
        ImpactGrade.GradeD => 150.0,
        ImpactGrade.GradeE => 350.0,
        _ => 0.0
    };

    public double GetToleranceSurcharge(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassA => 0.0,
        ToleranceClass.ClassB => 60.0,
        ToleranceClass.ClassC => 150.0, // GB/T 709 C类保全厚度
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
        MetallurgyProcess.AsRolled => 0.0,
        MetallurgyProcess.TMCP => 100.0,
        MetallurgyProcess.Normalized => 180.0,
        MetallurgyProcess.QuenchedAndTempered => 300.0,
        _ => 0.0
    };

    public double GetCertificateSurcharge(InspectionCertificateType certType) => certType switch
    {
        InspectionCertificateType.StandardMTC => 0.0,
        InspectionCertificateType.EN10204_31 => 30.0,
        InspectionCertificateType.EN10204_32 => 450.0,
        _ => 0.0
    };

    public string GetLocalizedToleranceName(ToleranceClass tolerance) => tolerance switch
    {
        ToleranceClass.ClassA => "GB/T 709 A类公差 (常规负偏差)",
        ToleranceClass.ClassB => "GB/T 709 B类公差 (负偏差≤-0.3mm)",
        ToleranceClass.ClassC => "GB/T 709 C类公差 (保证全厚度，无负偏差)",
        _ => tolerance.ToString()
    };
}
