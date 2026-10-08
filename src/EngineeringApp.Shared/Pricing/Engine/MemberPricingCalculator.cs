using EngineeringApp.Shared.Material;
using EngineeringApp.Shared.Material.Pricing;
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

    /// <summary>
    /// 支持指定材料价格时间快照、钢厂产地及调运目的地的全要素装配构件工料机成本核算
    /// </summary>
    public static MemberPricingResult Calculate(
        SteelAssemblyItem assembly,
        MaterialPriceSnapshot snapshot,
        string millId = "Baosteel",
        string destinationRegion = "华东-浙江制造车间",
        double coatingUnitPricePerM2 = 45.0)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(snapshot);

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

        // 1. 采用材料价格规则引擎进行多维精确算价 (含厚度加价、公差加价、钢厂溢价、调运费)
        double totalMaterialCost = 0.0;
        foreach (var part in assembly.Parts)
        {
            var plateParams = new PlatePricingParameters
            {
                Grade = part.MaterialGrade,
                ThicknessMm = part.ThicknessMm,
                SteelMillId = millId,
                Delivery = DeliveryCondition.Delivered_FabricationPlant,
                DestinationRegion = destinationRegion,
                CutType = PlateDimensionCutType.FixedDimension,
                Tolerance = ToleranceClass.ClassA
            };

            var priceItem = PlatePricingRuleEngine.Calculate(plateParams, snapshot);
            double partTotalGrossTon = (part.GrossWeightKg * part.QuantityPerAssembly * assembly.Quantity) / 1000.0;
            totalMaterialCost += partTotalGrossTon * priceItem.FinalPricePerTon;
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
