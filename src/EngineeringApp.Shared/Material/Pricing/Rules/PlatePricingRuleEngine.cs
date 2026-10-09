namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板多维立体加价核算规则引擎
/// </summary>
public static class PlatePricingRuleEngine
{
    /// <summary>
    /// 对给定的钢板加价参数及价格快照进行全要素采购价格核算
    /// </summary>
    public static StandardMaterialItemPrice Calculate(
        PlatePricingParameters parameters,
        MaterialPriceSnapshot? snapshot = null,
        IList<PlateThicknessLadder>? customLadders = null,
        IList<PlateDimensionRule>? customDimRules = null)
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

        // 2. 钢厂品牌溢价 (支持用户直接修改或采用参考基准)
        result.BenchmarkMillPremium = mill.BrandPremiumPerTon;
        if (parameters.CustomMillPremium.HasValue)
        {
            result.MillPremiumPerTon = parameters.CustomMillPremium.Value;
            result.IsMillPremiumCustomized = Math.Abs(result.MillPremiumPerTon - result.BenchmarkMillPremium) > 0.001;
        }
        else
        {
            result.MillPremiumPerTon = result.BenchmarkMillPremium;
            result.IsMillPremiumCustomized = false;
        }

        // 3. 规格厚度加价阶梯 (支持阶梯匹配、用户自定义及透明规则解释)
        var ladders = (customLadders != null && customLadders.Count > 0)
            ? customLadders
            : PlateThicknessLadder.GetDefaultLadders();

        var matchedLadder = ladders.FirstOrDefault(l => l.Matches(parameters.ThicknessMm))
            ?? (parameters.ThicknessMm <= 0 ? ladders[0] : ladders[^1]);

        result.BenchmarkThicknessSurcharge = matchedLadder.BenchmarkSurcharge;

        if (parameters.CustomThicknessSurcharge.HasValue)
        {
            result.ThicknessSurchargePerTon = parameters.CustomThicknessSurcharge.Value;
            result.IsThicknessSurchargeCustomized = Math.Abs(result.ThicknessSurchargePerTon - result.BenchmarkThicknessSurcharge) > 0.001;
            result.ThicknessPrincipleExplanation = $"【用户自主微调】¥{result.ThicknessSurchargePerTon:0.##}/t (命中标准: {matchedLadder.Description}, 行业参考: ¥{matchedLadder.BenchmarkSurcharge:0.##}/t)";
        }
        else
        {
            result.ThicknessSurchargePerTon = matchedLadder.EffectiveSurcharge;
            result.IsThicknessSurchargeCustomized = matchedLadder.IsCustomized;
            result.ThicknessPrincipleExplanation = $"命中规则: {matchedLadder.Description} (基准加价: ¥{matchedLadder.EffectiveSurcharge:0.##}/t)";
        }

        // 4. 定尺与超限加价 (支持规则匹配与自主设定)
        var dimRules = (customDimRules != null && customDimRules.Count > 0)
            ? customDimRules
            : PlateDimensionRule.GetDefaultRules();

        var (calcDimSurcharge, dimExplanation) = EvaluateDimensionRules(parameters, dimRules);
        result.BenchmarkDimensionSurcharge = calcDimSurcharge;

        if (parameters.CustomDimensionSurcharge.HasValue)
        {
            result.DimensionSurchargePerTon = parameters.CustomDimensionSurcharge.Value;
            result.IsDimensionSurchargeCustomized = Math.Abs(result.DimensionSurchargePerTon - result.BenchmarkDimensionSurcharge) > 0.001;
            result.DimensionPrincipleExplanation = $"【用户自主微调】¥{result.DimensionSurchargePerTon:0.##}/t (规则计算参考: ¥{result.BenchmarkDimensionSurcharge:0.##}/t, {dimExplanation})";
        }
        else
        {
            result.DimensionSurchargePerTon = result.BenchmarkDimensionSurcharge;
            result.IsDimensionSurchargeCustomized = false;
            result.DimensionPrincipleExplanation = dimExplanation;
        }

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

        // 9. 物流调运费 (支持用户直接修改或采用路线参考值)
        double benchmarkFreight = FreightCalculator.Calculate(parameters.SteelMillId, parameters.Delivery, parameters.DestinationRegion);
        result.BenchmarkFreight = benchmarkFreight;
        if (parameters.CustomFreightPerTon.HasValue)
        {
            result.FreightPerTon = parameters.CustomFreightPerTon.Value;
            result.IsFreightCustomized = Math.Abs(result.FreightPerTon - result.BenchmarkFreight) > 0.001;
        }
        else
        {
            result.FreightPerTon = benchmarkFreight;
            result.IsFreightCustomized = false;
        }

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
    public static double CalculateThicknessSurcharge(double thicknessMm, IList<PlateThicknessLadder>? ladders = null)
    {
        var list = (ladders != null && ladders.Count > 0) ? ladders : PlateThicknessLadder.GetDefaultLadders();
        var match = list.FirstOrDefault(l => l.Matches(thicknessMm));
        if (match != null) return match.EffectiveSurcharge;
        if (thicknessMm < 8.0) return 120.0;
        return 650.0;
    }

    private static (double Surcharge, string Explanation) EvaluateDimensionRules(PlatePricingParameters p, IList<PlateDimensionRule> rules)
    {
        double total = 0.0;
        var details = new List<string>();

        // 定尺判定
        if (p.CutType == PlateDimensionCutType.FixedDimension)
        {
            var rule = rules.FirstOrDefault(r => r.RuleCode == "FixedCut") ?? new PlateDimensionRule { BenchmarkSurcharge = 60.0 };
            double val = rule.EffectiveSurcharge;
            total += val;
            details.Add($"定宽定尺(+¥{val:0})");
        }
        else if (p.CutType == PlateDimensionCutType.SmallCut)
        {
            var rule = rules.FirstOrDefault(r => r.RuleCode == "SmallCut") ?? new PlateDimensionRule { BenchmarkSurcharge = 100.0 };
            double val = rule.EffectiveSurcharge;
            total += val;
            details.Add($"小定尺精密下料(+¥{val:0})");
        }

        // 超宽判定 (根据宽度 mm 触发)
        if (p.WidthMm > 3200.0)
        {
            var rule = rules.FirstOrDefault(r => r.RuleCode == "SuperWide_3200") ?? new PlateDimensionRule { BenchmarkSurcharge = 280.0 };
            double val = rule.EffectiveSurcharge;
            total += val;
            details.Add($"超宽板宽度{p.WidthMm:0}mm>3200(+¥{val:0})");
        }
        else if (p.CutType == PlateDimensionCutType.SuperWide || p.WidthMm > 2800.0)
        {
            var rule = rules.FirstOrDefault(r => r.RuleCode == "SuperWide_2800") ?? new PlateDimensionRule { BenchmarkSurcharge = 160.0 };
            double val = rule.EffectiveSurcharge;
            total += val;
            details.Add($"特宽板宽度{p.WidthMm:0}mm>2800(+¥{val:0})");
        }

        // 超长判定
        if (p.CutType == PlateDimensionCutType.SuperLong || p.LengthMm > 15000.0)
        {
            var rule = rules.FirstOrDefault(r => r.RuleCode == "SuperLong_15m") ?? new PlateDimensionRule { BenchmarkSurcharge = 180.0 };
            double val = rule.EffectiveSurcharge;
            total += val;
            details.Add($"超长板长度{p.LengthMm:0}mm>15m(+¥{val:0})");
        }

        string explanation = details.Count > 0
            ? string.Join("，", details)
            : "标准散尺尺寸，无定尺/超限加价 (¥0)";

        return (total, explanation);
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
