using EngineeringApp.Shared.Material;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Quota;

namespace EngineeringApp.Shared.Pricing;

/// <summary>
/// 单个装配构件或构件批次的工料机直接费组价测算结果
/// </summary>
public class MemberPricingResult
{
    public string AssemblyMark { get; set; } = "";
    public MemberRole Role { get; set; }
    public SectionType MainSectionType { get; set; }
    public int Quantity { get; set; }

    /// <summary>总净重 (吨 t)</summary>
    public double TotalNetWeightTon { get; set; }

    /// <summary>总采购毛重 (含下料损耗 吨 t)</summary>
    public double TotalGrossWeightTon { get; set; }

    /// <summary>总涂装展开表面积 (m²)</summary>
    public double TotalPaintingAreaM2 { get; set; }

    /// <summary>主材采购材料直接费 (元)</summary>
    public double MaterialProcurementCost { get; set; }

    /// <summary>综合加工制作基准单价 (元/吨)</summary>
    public double BaseFabricationPricePerTon { get; set; }

    /// <summary>调整后的构件加工制作综合单价 (含厚板与节点加成 元/吨)</summary>
    public double AdjustedFabricationPricePerTon { get; set; }

    /// <summary>车间加工制作直接工费 (元)</summary>
    public double FabricationCost { get; set; }

    /// <summary>构件表面涂装防腐直接费 (元)</summary>
    public double CoatingCost { get; set; }

    /// <summary>直接工程费小计 (材料 + 制作 + 涂装 元)</summary>
    public double TotalDirectCost => MaterialProcurementCost + FabricationCost + CoatingCost;

    /// <summary>直接费吨钢综合单价 (元/吨净重)</summary>
    public double DirectUnitPricePerTon => TotalNetWeightTon > 0 ? TotalDirectCost / TotalNetWeightTon : 0;
}

/// <summary>
/// 构件装配BOM与工料机直接费组价计算引擎
/// </summary>
public static class MemberPricingCalculator
{
    /// <summary>
    /// 对给定的装配构件进行工料机直接成本核算
    /// </summary>
    /// <param name="assembly">装配构件明细 (含主肢、牛腿、隔板等零件)</param>
    /// <param name="coatingUnitPricePerM2">涂装防腐平米综合单价 (元/m²，可根据涂装配套方案计算)</param>
    /// <returns>构件工料机直接费核算结果</returns>
    public static MemberPricingResult Calculate(SteelAssemblyItem assembly, double coatingUnitPricePerM2 = 45.0)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var result = new MemberPricingResult
        {
            AssemblyMark = assembly.AssemblyMark,
            Role = assembly.Role,
            MainSectionType = assembly.MainSectionType,
            Quantity = assembly.Quantity,
            TotalNetWeightTon = assembly.TotalBatchNetWeightTon,
            TotalGrossWeightTon = assembly.TotalBatchGrossWeightTon,
            TotalPaintingAreaM2 = assembly.TotalBatchPaintingAreaM2
        };

        // 1. 材料采购直接费核算 (遍历各零件板厚与材质牌号)
        double totalMaterialCost = 0.0;
        foreach (var part in assembly.Parts)
        {
            var matInfo = SteelMaterialDatabase.Get(part.MaterialGrade);
            double pricePerTon = matInfo.CalculateProcurementPricePerTon(part.ThicknessMm);
            double partTotalGrossTon = (part.GrossWeightKg * part.QuantityPerAssembly * assembly.Quantity) / 1000.0;
            totalMaterialCost += partTotalGrossTon * pricePerTon;
        }
        result.MaterialProcurementCost = totalMaterialCost;

        // 2. 车间加工制作直接工费核算 (正交定额查询 + 厚度调整 + 牛腿配件加成)
        var quotaItem = FabricationQuotaDatabase.Query(assembly.MainSectionType, assembly.Role);
        result.BaseFabricationPricePerTon = quotaItem.BaseFabricationUnitPricePerTon;

        double thickFactor = QuotaDifficultyRules.CalculateThicknessFactor(assembly.MaxThicknessMm);
        double fittingSurcharge = QuotaDifficultyRules.CalculateFittingSurchargePerTon(
            assembly.CantileverBracketCount,
            assembly.FittingWeightRatio);

        double adjustedFabPrice = (quotaItem.BaseFabricationUnitPricePerTon * thickFactor) + fittingSurcharge;
        result.AdjustedFabricationPricePerTon = adjustedFabPrice;
        result.FabricationCost = result.TotalNetWeightTon * adjustedFabPrice;

        // 3. 涂装防腐直接费
        result.CoatingCost = result.TotalPaintingAreaM2 * Math.Max(0, coatingUnitPricePerM2);

        return result;
    }
}
