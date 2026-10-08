namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 型材商业价格与工艺核算规则引擎 (覆盖热轧、无缝管及圆转方冷成型)
/// </summary>
public static class ProfilePricingRuleEngine
{
    public static StandardMaterialItemPrice Calculate(ProfilePricingParameters parameters, MaterialPriceSnapshot? snapshot = null)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        snapshot ??= SeedPriceSnapshots.Latest;

        var strategy = PricingStrategyFactory.GetStrategy(parameters.Standard);
        var mill = SeedSteelMills.GetById(parameters.SteelMillId);

        var result = new StandardMaterialItemPrice
        {
            Standard = parameters.Standard,
            MaterialType = parameters.Category switch
            {
                ProfileCategory.HotRolled_H => "Profile_H",
                ProfileCategory.Seamless_CHS => "Profile_CHS_Seamless",
                ProfileCategory.Welded_CHS => "Profile_CHS_Welded",
                ProfileCategory.RoundToSquare_RHS => "Profile_RHS_R2S",
                _ => "Profile_RHS"
            },
            Grade = parameters.Grade,
            PriceEffectiveDate = snapshot.EffectiveDate,
            IsTaxInclusive = snapshot.IsTaxInclusive,
            MillName = mill.Name,
            DimensionText = $"{parameters.SectionDesignation} (L={parameters.LengthMeter:0.#}m)"
        };

        // 1. 基准价格与工艺成型加工费核算
        switch (parameters.Category)
        {
            case ProfileCategory.HotRolled_H:
                result.BasePricePerTon = strategy.GetBasePrice(snapshot, parameters.Grade, isPlate: false);
                result.MillPremiumPerTon = mill.BrandPremiumPerTon;
                // 大规格H型钢截面加价 (如截面高度>400或翼缘宽度>250)
                if (parameters.HeightMm >= 400.0 || parameters.WidthMm >= 250.0)
                {
                    result.ThicknessSurchargePerTon = 70.0;
                }
                result.ProcessSurchargePerTon = 0.0; // 热轧直接成品
                break;

            case ProfileCategory.Seamless_CHS:
                result.BasePricePerTon = snapshot.SeamlessTubeBase;
                result.MillPremiumPerTon = mill.BrandPremiumPerTon;
                if (parameters.WallThicknessMm >= 16.0)
                {
                    result.ThicknessSurchargePerTon = 120.0; // 特厚壁无缝管
                }
                result.ProcessSurchargePerTon = 0.0;
                break;

            case ProfileCategory.Welded_CHS:
                result.BasePricePerTon = snapshot.WeldedTubeBase;
                result.MillPremiumPerTon = mill.BrandPremiumPerTon;
                result.ProcessSurchargePerTon = 0.0;
                break;

            case ProfileCategory.StandardColdFormed_RHS:
                result.BasePricePerTon = snapshot.HotRolledCoilBase;
                result.MillPremiumPerTon = mill.BrandPremiumPerTon;
                result.ProcessSurchargePerTon = 450.0; // 常规方矩管冷弯成型工费
                break;

            case ProfileCategory.RoundToSquare_RHS:
                // ★ 重点：圆转方工艺核算 (热卷基料 + 厚卷加价 + 特种多道次模具强压成型费)
                result.BasePricePerTon = snapshot.HotRolledCoilBase;
                result.MillPremiumPerTon = mill.BrandPremiumPerTon;
                
                // 厚卷加价
                if (parameters.WallThicknessMm >= 14.0)
                {
                    result.ThicknessSurchargePerTon = 120.0;
                }
                else if (parameters.WallThicknessMm >= 10.0)
                {
                    result.ThicknessSurchargePerTon = 60.0;
                }

                // 圆转方模具冷挤压加工工费 (大截面、大壁厚加工硬化强，模具损耗极高)
                double r2sProcessCost = (parameters.WallThicknessMm >= 14.0 || parameters.WidthMm >= 350.0) ? 750.0 : 600.0;
                if (parameters.NeedsCornerStressReliefAnneal)
                {
                    r2sProcessCost += 180.0; // 角部退火热处理去应力
                }
                result.ProcessSurchargePerTon = r2sProcessCost;
                break;
        }

        // 2. 超长定尺加价 (>12m 加价 60元/吨)
        if (parameters.IsFixedLength && parameters.LengthMeter > 12.0)
        {
            result.DimensionSurchargePerTon = 60.0;
        }

        // 3. 物流调运费
        result.FreightPerTon = FreightCalculator.Calculate(parameters.SteelMillId, parameters.Delivery, parameters.DestinationRegion);

        // 4. 标准材质标注与描述
        result.StandardSpecification = $"{parameters.Grade} 国标热轧/成型";
        result.DeliveryAndFreightText = $"{parameters.DestinationRegion}交货 (调运¥{result.FreightPerTon:0}/t)";
        result.FullDescription = MaterialDescriptorBuilder.BuildProfileDescribe(result, parameters, strategy);

        // 5. 价格拆解明细
        result.PriceBreakdownSummary = FormatBreakdownSummary(result);

        return result;
    }

    private static string FormatBreakdownSummary(StandardMaterialItemPrice r)
    {
        var parts = new List<string> { $"基料/基价 {r.BasePricePerTon:0}" };
        if (r.MillPremiumPerTon != 0) parts.Add($"钢厂溢价 {(r.MillPremiumPerTon > 0 ? "+" : "")}{r.MillPremiumPerTon:0}");
        if (r.ThicknessSurchargePerTon > 0) parts.Add($"规格/厚度 +{r.ThicknessSurchargePerTon:0}");
        if (r.DimensionSurchargePerTon > 0) parts.Add($"定尺超长 +{r.DimensionSurchargePerTon:0}");
        if (r.ProcessSurchargePerTon > 0) parts.Add($"成型工艺费 +{r.ProcessSurchargePerTon:0}");
        if (r.FreightPerTon > 0) parts.Add($"调运 +{r.FreightPerTon:0}");

        return $"{string.Join(" + ", parts)} = {r.FinalPricePerTon:0.##} 元/吨";
    }
}
