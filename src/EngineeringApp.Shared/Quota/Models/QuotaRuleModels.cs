using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Quota;

/// <summary>
/// 构件加工制造定额基准工价条目
/// </summary>
public class FabricationQuotaItem
{
    public SectionType SectionType { get; set; }
    public MemberRole Role { get; set; }

    /// <summary>基础加工制造直接工费基准 (元/吨)</summary>
    public double BaseFabricationUnitPricePerTon { get; set; } = 1500.0;

    /// <summary>定额基准工时 (工日/吨)</summary>
    public double BaseLaborDaysPerTon { get; set; } = 1.8;

    /// <summary>典型制造主工序描述</summary>
    public string ProcessDescription { get; set; } = "";

    /// <summary>是否默认包含内部电渣焊隔板工序</summary>
    public bool IncludesInternalDiaphragms { get; set; } = false;

    /// <summary>是否默认包含相贯线五轴切割与复杂坡口工序</summary>
    public bool IncludesIntersectingCuts { get; set; } = false;
}

/// <summary>
/// 钢结构加工制造工艺难度与损耗调节规则
/// </summary>
public static class QuotaDifficultyRules
{
    /// <summary>
    /// 根据构件最大主要板厚 (mm) 计算厚板加工难度系数 K_Thick
    /// 厚板需要预热、消氢、多层多道焊及超声波探伤，工费显著递增
    /// </summary>
    public static double CalculateThicknessFactor(double maxThicknessMm)
    {
        if (maxThicknessMm <= 20.0) return 1.0;
        if (maxThicknessMm <= 30.0) return 1.05;
        if (maxThicknessMm <= 40.0) return 1.12;
        if (maxThicknessMm <= 60.0) return 1.25;
        return 1.40; // 特厚板 (>60mm)
    }

    /// <summary>
    /// 计算节点与牛腿复杂度附加制作工费 (元/吨)
    /// </summary>
    /// <param name="bracketCount">外伸牛腿数量</param>
    /// <param name="fittingWeightRatioPercent">节点板/加劲肋占整构件总重量百分比 (如 12 表示 12%)</param>
    public static double CalculateFittingSurchargePerTon(int bracketCount, double fittingWeightRatioPercent)
    {
        double surcharge = 0.0;

        // 牛腿装配与熔透坡口焊附加
        if (bracketCount > 0)
        {
            surcharge += Math.Min(600.0, bracketCount * 80.0);
        }

        // 节点板超重惩罚 (节点板装配焊接人工占比高，常规通常在 8%~12% 之间)
        if (fittingWeightRatioPercent > 15.0)
        {
            surcharge += (fittingWeightRatioPercent - 15.0) * 15.0;
        }

        return surcharge;
    }
}
