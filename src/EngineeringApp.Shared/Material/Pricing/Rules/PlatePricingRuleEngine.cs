namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板多维立体加价核算规则引擎
/// </summary>
public static class PlatePricingRuleEngine
{
    /// <summary>
    /// 对给定的钢板加价参数及价格快照进行全要素采购价格核算
    /// </summary>
    public static StandardMaterialItemPrice Calculate(PlatePricingParameters parameters, MaterialPriceSnapshot? snapshot = null)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        snapshot ??= SeedPriceSnapshots.Latest;

        var strategy = PricingStrategyFactory.GetStrategy(parameters.Standard);
        var mill = SeedSteelMills.GetById(parameters.SteelMillId);

        var result = new StandardMaterialItemPrice
        {
            Standard = parameters.Standard,
            MaterialType = "Plate",
            Grade = parameters.Grade,
            PriceEffectiveDate = snapshot.EffectiveDate,
            IsTaxInclusive = snapshot.IsTaxInclusive,
            MillName = mill.Name
        };

        // 1. 基准大盘单价
        result.BasePricePerTon = strategy.GetBasePrice(snapshot, parameters.Grade, isPlate: true);

        // 2. 钢厂品牌溢价
        result.MillPremiumPerTon = mill.BrandPremiumPerTon;

        // 3. 规格厚度加价阶梯 (行业标准中厚板加价单)
        result.ThicknessSurchargePerTon = CalculateThicknessSurcharge(parameters.ThicknessMm);

        // 4. 定尺与超限加价
        result.DimensionSurchargePerTon = CalculateDimensionSurcharge(parameters);

        // 5. 公差精度加价 (含保全厚度与平整度)
        double tolSurcharge = strategy.GetToleranceSurcharge(parameters.Tolerance);
        if (parameters.Flatness == FlatnessClass.High_H)
        {
            tolSurcharge += 90.0; // 超平整校平加价
        }
        result.ToleranceSurchargePerTon = tolSurcharge;

        // 6. 性能加价 (冲击韧性 + Z向抗撕裂 + TMCP/正火)
        double perfSurcharge = strategy.GetImpactSurcharge(parameters.Impact)
                             + strategy.GetZDirectionSurcharge(parameters.ZDirection)
                             + strategy.GetMetallurgySurcharge(parameters.Metallurgy);
        result.PerformanceSurchargePerTon = perfSurcharge;

        // 7. 探伤与质保认证加价
        double utSurcharge = parameters.UT switch
        {
            UltrasonicInspection.ClassI => 220.0,
            UltrasonicInspection.ClassII => 120.0,
            _ => 0.0
        };
        utSurcharge += strategy.GetCertificateSurcharge(parameters.Certificate);
        result.InspectionSurchargePerTon = utSurcharge;

        // 8. 工艺成型加价 (平板加工为 0)
        result.ProcessSurchargePerTon = 0.0;

        // 9. 物流调运费
        result.FreightPerTon = FreightCalculator.Calculate(parameters.SteelMillId, parameters.Delivery, parameters.DestinationRegion);

        // 10. 格式化标准材质标注
        result.StandardSpecification = strategy.FormatStandardSpecification(
            parameters.Grade, parameters.Impact, parameters.ZDirection, parameters.Metallurgy);

        // 11. 规格尺寸文本
        result.DimensionText = $"t={parameters.ThicknessMm:0.##}mm ({parameters.WidthMm:0}×{parameters.LengthMm:0})";

        // 12. 格式化交付与调运文本
        result.DeliveryAndFreightText = FormatDeliveryText(parameters.Delivery, parameters.DestinationRegion, result.FreightPerTon);

        // 13. 生成标准化 Describe 字符串
        result.FullDescription = MaterialDescriptorBuilder.BuildPlateDescribe(result, parameters, strategy);

        // 14. 价格拆解公式明细
        result.PriceBreakdownSummary = FormatBreakdownSummary(result);

        return result;
    }

    /// <summary>
    /// 标准行业厚度加价阶梯表 (元/吨)
    /// </summary>
    public static double CalculateThicknessSurcharge(double thicknessMm)
    {
        if (thicknessMm < 8.0) return 120.0;      // 薄板薄辊加价
        if (thicknessMm < 14.0) return 50.0;     // 8~12mm 次基准
        if (thicknessMm <= 20.0) return 0.0;     // 14~20mm 行业黄金基价点 (0元)
        if (thicknessMm <= 40.0) return 80.0;    // 22~40mm 常用厚板加价
        if (thicknessMm <= 60.0) return 180.0;   // 42~60mm 特厚板加价
        if (thicknessMm <= 100.0) return 380.0;  // 62~100mm 超厚板心部质量保证
        return 650.0;                            // >100mm 极厚板大型锭轧加价
    }

    private static double CalculateDimensionSurcharge(PlatePricingParameters p)
    {
        double surcharge = 0.0;
        if (p.CutType == PlateDimensionCutType.FixedDimension)
        {
            surcharge += 60.0;
        }
        else if (p.CutType == PlateDimensionCutType.SmallCut)
        {
            surcharge += 100.0;
        }

        // 超宽超长加价
        if (p.CutType == PlateDimensionCutType.SuperWide || p.WidthMm > 2800.0)
        {
            surcharge += (p.WidthMm > 3200.0) ? 280.0 : 160.0;
        }

        if (p.CutType == PlateDimensionCutType.SuperLong || p.LengthMm > 15000.0)
        {
            surcharge += 180.0;
        }

        return surcharge;
    }

    private static string FormatDeliveryText(DeliveryCondition delivery, string destination, double freight)
    {
        if (delivery == DeliveryCondition.ExWorks_Mill)
        {
            return "钢厂车板出厂价 (自提)";
        }
        if (delivery == DeliveryCondition.FOB_ChinesePort)
        {
            return $"FOB 中国港口集港价 (调运/杂费¥{freight:0}/t)";
        }
        return $"{destination}交货 (调运¥{freight:0}/t)";
    }

    private static string FormatBreakdownSummary(StandardMaterialItemPrice r)
    {
        var parts = new List<string> { $"基价 {r.BasePricePerTon:0}" };
        if (r.MillPremiumPerTon != 0) parts.Add($"钢厂溢价 {(r.MillPremiumPerTon > 0 ? "+" : "")}{r.MillPremiumPerTon:0}");
        if (r.ThicknessSurchargePerTon > 0) parts.Add($"厚度 +{r.ThicknessSurchargePerTon:0}");
        if (r.DimensionSurchargePerTon > 0) parts.Add($"定尺超限 +{r.DimensionSurchargePerTon:0}");
        if (r.ToleranceSurchargePerTon > 0) parts.Add($"公差 +{r.ToleranceSurchargePerTon:0}");
        if (r.PerformanceSurchargePerTon > 0) parts.Add($"性能 +{r.PerformanceSurchargePerTon:0}");
        if (r.InspectionSurchargePerTon > 0) parts.Add($"探伤/认证 +{r.InspectionSurchargePerTon:0}");
        if (r.ProcessSurchargePerTon > 0) parts.Add($"工艺 +{r.ProcessSurchargePerTon:0}");
        if (r.FreightPerTon > 0) parts.Add($"调运 +{r.FreightPerTon:0}");

        return $"{string.Join(" + ", parts)} = {r.FinalPricePerTon:0.##} 元/吨";
    }
}
