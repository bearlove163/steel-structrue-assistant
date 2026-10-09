using EngineeringApp.Shared.Material;
using EngineeringApp.Shared.Material.Pricing;
using EngineeringApp.Shared.Models;
using Xunit;

namespace EngineeringApp.Tests;

public class MaterialPricingTests
{
    [Fact]
    public void PlatePricing_GbStandard_CalculatesAccurately()
    {
        // 场景：宝钢 Q355D, 50mm 厚板, C类保证全厚度, Z25向性能, TMCP控轧, 一级探伤, 运抵浙江车间, 定宽定尺
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355D,
            ThicknessMm = 50.0,
            WidthMm = 2500.0,
            LengthMm = 12000.0,
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.Delivered_FabricationPlant,
            DestinationRegion = "华东-浙江制造车间",
            CutType = PlateDimensionCutType.FixedDimension,
            Tolerance = ToleranceClass.ClassC, // 保全厚度 +150
            Flatness = FlatnessClass.Normal_N,
            Impact = ImpactGrade.GradeD,       // D级 -20℃ +150
            ZDirection = ZDirectionQuality.Z25, // Z25 +250
            Metallurgy = MetallurgyProcess.TMCP, // TMCP +100
            UT = UltrasonicInspection.ClassI,    // 一级探伤 +220
            Certificate = InspectionCertificateType.StandardMTC
        };

        var snapshot = SeedPriceSnapshots.Latest; // 基价 Q355B: 4050
        var result = PlatePricingRuleEngine.Calculate(p, snapshot);

        // 验证各加价项
        Assert.Equal(4050.0, result.BasePricePerTon);
        Assert.Equal(200.0, result.MillPremiumPerTon);       // 宝钢溢价 200
        Assert.Equal(180.0, result.ThicknessSurchargePerTon); // 50mm 厚板加价 180
        Assert.Equal(60.0, result.DimensionSurchargePerTon);  // 定尺 60
        Assert.Equal(150.0, result.ToleranceSurchargePerTon); // C类保全厚度 150
        Assert.Equal(500.0, result.PerformanceSurchargePerTon); // 150(D级) + 250(Z25) + 100(TMCP) = 500
        Assert.Equal(220.0, result.InspectionSurchargePerTon); // 一级探伤 220
        Assert.Equal(50.0, result.FreightPerTon);             // 浙江车间调运费 50

        // 最终综合单价 = 4050 + 200 + 180 + 60 + 150 + 500 + 220 + 0 + 50 = 5410 元/吨
        Assert.Equal(5410.0, result.FinalPricePerTon);

        // 验证 Describe 规范字符串
        Assert.Contains("钢板", result.FullDescription);
        Assert.Contains("Q355D-Z25-TMCP", result.FullDescription);
        Assert.Contains("宝钢", result.FullDescription);
        Assert.Contains("C类公差", result.FullDescription);
        Assert.Contains("一级探伤", result.FullDescription);
        Assert.Contains("调运¥50/t", result.FullDescription);
    }

    [Fact]
    public void Freight_DestinationVariance_CalculatesAccurately()
    {
        // 验证用户特别提到的：同样宝钢采购，浙江调运50 vs 广东调运200，差价正好是 150元/吨
        var pZhejiang = new PlatePricingParameters
        {
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.Delivered_FabricationPlant,
            DestinationRegion = "华东-浙江制造车间",
            ThicknessMm = 20.0
        };

        var pGuangdong = new PlatePricingParameters
        {
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.Delivered_JobSite,
            DestinationRegion = "华南-广东工程现场",
            ThicknessMm = 20.0
        };

        var resZhejiang = PlatePricingRuleEngine.Calculate(pZhejiang);
        var resGuangdong = PlatePricingRuleEngine.Calculate(pGuangdong);

        Assert.Equal(50.0, resZhejiang.FreightPerTon);
        Assert.Equal(200.0, resGuangdong.FreightPerTon);
        Assert.Equal(150.0, resGuangdong.FinalPricePerTon - resZhejiang.FinalPricePerTon);
    }

    [Fact]
    public void Profile_RoundToSquareColdFormed_CalculatesAccurately()
    {
        // 场景：沙钢热卷基料 + □400×400×16 圆转方冷挤压管 + 角部退火 + 运抵浙江车间
        var p = new ProfilePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            Category = ProfileCategory.RoundToSquare_RHS,
            Process = ProfileFormingProcess.RoundToSquareColdFormed,
            SectionDesignation = "□400×400×16",
            HeightMm = 400.0,
            WidthMm = 400.0,
            WallThicknessMm = 16.0,
            LengthMeter = 12.0,
            SteelMillId = "Shagang",
            Delivery = DeliveryCondition.Delivered_FabricationPlant,
            DestinationRegion = "华东-浙江制造车间",
            NeedsCornerStressReliefAnneal = true, // 退火 +180
            IsFixedLength = true
        };

        var snapshot = SeedPriceSnapshots.Latest; // 热卷基价 3900
        var result = ProfilePricingRuleEngine.Calculate(p, snapshot);

        Assert.Equal(3900.0, result.BasePricePerTon);
        Assert.Equal(0.0, result.MillPremiumPerTon); // 沙钢民营基准溢价 0
        Assert.Equal(120.0, result.ThicknessSurchargePerTon); // 16mm 厚卷加价 120
        // 圆转方加工费 (大壁厚大截面 750 + 退火 180 = 930)
        Assert.Equal(930.0, result.ProcessSurchargePerTon);
        Assert.Equal(60.0, result.FreightPerTon); // 沙钢至浙江调运 60

        // 最终价 = 3900 + 0 + 120 + 0 + 930 + 60 = 5010 元/吨
        Assert.Equal(5010.0, result.FinalPricePerTon);
        Assert.Contains("圆转方冷成型", result.FullDescription);
        Assert.Contains("去应力退火", result.FullDescription);
    }

    [Fact]
    public void International_EnStandardStrategy_CalculatesAccurately()
    {
        // 场景：出口欧洲重大工程，宝钢 S355J2+N, EN 10029 Class C, EN 10204 3.2 第三方检验证书, FOB集港
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.EN,
            Grade = SteelGrade.S355J2,
            ThicknessMm = 50.0,
            WidthMm = 2500.0,
            LengthMm = 12000.0,
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.FOB_ChinesePort,
            DestinationRegion = "上海港国际集港码头 (FOB)",
            CutType = PlateDimensionCutType.FixedDimension,
            Tolerance = ToleranceClass.ClassC, // EN 10029 Class C +150
            Impact = ImpactGrade.GradeD,       // J2 (-20℃ 27J) +150
            ZDirection = ZDirectionQuality.Z25, // Z25 +250
            Metallurgy = MetallurgyProcess.Normalized, // +N +180
            Certificate = InspectionCertificateType.EN10204_32 // 3.2 证书见证 +450
        };

        var result = PlatePricingRuleEngine.Calculate(p);

        Assert.Equal(150.0, result.ToleranceSurchargePerTon);
        Assert.Equal(450.0, result.InspectionSurchargePerTon);
        Assert.Equal(220.0, result.FreightPerTon); // FOB 集港港杂 220
        Assert.Contains("EN 10025 S355J2+N-Z25", result.StandardSpecification);
        Assert.Contains("EN 10029 Class C", result.FullDescription);
        Assert.Contains("EN 10204 Type 3.2", result.FullDescription);
        Assert.Contains("FOB", result.FullDescription);
    }

    [Fact]
    public void VarianceAnalysis_BaseVsLatest_ShowsCostDelta()
    {
        // 验证项目锁价基期与最新网价的比对
        var baseSnap = SeedPriceSnapshots.GetById("SNAP-20260315-BASE"); // 基价 3920
        var latestSnap = SeedPriceSnapshots.Latest; // 基价 4050

        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            ThicknessMm = 20.0,
            SteelMillId = "Shagang"
        };

        var baseQuote = PlatePricingRuleEngine.Calculate(p, baseSnap);
        var latestQuote = PlatePricingRuleEngine.Calculate(p, latestSnap);

        double deltaPricePerTon = latestQuote.FinalPricePerTon - baseQuote.FinalPricePerTon;

        // 差价应为 4050 - 3920 = 130 元/吨
        Assert.Equal(130.0, deltaPricePerTon);
    }

    [Fact]
    public void MemberPricingCalculator_WithSnapshot_CalculatesBOMAccurately()
    {
        // 场景：构建一个 GZ1 箱型柱装配件，包含主肢板与内隔板
        var assembly = new EngineeringApp.Shared.Pricing.SteelAssemblyItem
        {
            AssemblyMark = "GZ1",
            Role = MemberRole.Column,
            MainSectionType = SectionType.RHS,
            Quantity = 2,
            OverallLengthMeter = 6.0,
            Parts =
            [
                new EngineeringApp.Shared.Pricing.SteelPartItem
                {
                    PartMark = "w1",
                    PartName = "主肢翼板",
                    MaterialGrade = SteelGrade.Q355B,
                    ThicknessMm = 30.0, // 30mm 厚板
                    LengthMeter = 6.0,
                    NetWeightKg = 800.0,
                    QuantityPerAssembly = 2,
                    PartLossRate = 0.05
                },
                new EngineeringApp.Shared.Pricing.SteelPartItem
                {
                    PartMark = "dp1",
                    PartName = "电渣焊内隔板",
                    MaterialGrade = SteelGrade.Q355B,
                    ThicknessMm = 20.0,
                    LengthMeter = 0.5,
                    NetWeightKg = 50.0,
                    QuantityPerAssembly = 4,
                    PartLossRate = 0.08
                }
            ]
        };

        var snapshot = SeedPriceSnapshots.Latest;
        var pricingResult = EngineeringApp.Shared.Pricing.MemberPricingCalculator.Calculate(
            assembly, snapshot, millId: "Baosteel", destinationRegion: "华东-浙江制造车间");

        Assert.True(pricingResult.MaterialProcurementCost > 0);
        Assert.True(pricingResult.FabricationCost > 0);
        Assert.True(pricingResult.TotalDirectCost > 0);
        Assert.True(pricingResult.DirectUnitPricePerTon > 0);
    }

    [Fact]
    public void PlatePricing_CustomThicknessSurcharge_OverridesLadderAndExplainsPrinciple()
    {
        // 场景：50mm 厚板，行业标准阶梯为 40~60mm (+180 元/t)
        // 用户自主将厚度加价微调设定为 130 元/t
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            ThicknessMm = 50.0,
            SteelMillId = "Baosteel",
            CustomThicknessSurcharge = 130.0
        };

        var result = PlatePricingRuleEngine.Calculate(p);

        Assert.Equal(180.0, result.BenchmarkThicknessSurcharge);
        Assert.Equal(130.0, result.ThicknessSurchargePerTon);
        Assert.True(result.IsThicknessSurchargeCustomized);
        Assert.Contains("用户自主微调", result.ThicknessPrincipleExplanation);
        Assert.Contains("180", result.ThicknessPrincipleExplanation);
    }

    [Fact]
    public void PlatePricing_CustomDimensionSurcharge_OverridesDimensionRules()
    {
        // 场景：定宽定尺(60) + 特宽板2900mm(160)，规则合计参考为 220 元/t
        // 用户自主将定尺/超限加价微调设定为 150 元/t
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            WidthMm = 2900.0,
            CutType = PlateDimensionCutType.FixedDimension,
            CustomDimensionSurcharge = 150.0
        };

        var result = PlatePricingRuleEngine.Calculate(p);

        Assert.Equal(220.0, result.BenchmarkDimensionSurcharge);
        Assert.Equal(150.0, result.DimensionSurchargePerTon);
        Assert.True(result.IsDimensionSurchargeCustomized);
        Assert.Contains("用户自主微调", result.DimensionPrincipleExplanation);
    }

    [Fact]
    public void PlatePricing_CustomMillPremium_OverridesBenchmarkAndTracksTemporal()
    {
        // 场景：宝钢参考溢价为 200 元/t
        // 用户根据当前采购期实际议价，自主设定为 110 元/t
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            SteelMillId = "Baosteel",
            CustomMillPremium = 110.0
        };

        var result = PlatePricingRuleEngine.Calculate(p);

        Assert.Equal(200.0, result.BenchmarkMillPremium);
        Assert.Equal(110.0, result.MillPremiumPerTon);
        Assert.True(result.IsMillPremiumCustomized);
    }

    [Fact]
    public void PlatePricing_CustomFreight_OverridesPresetFreight()
    {
        // 场景：广东工程现场预设路线运费为 200 元/t
        // 用户联系回程货运车队，将实际调运费调整为 160 元/t
        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.Delivered_JobSite,
            DestinationRegion = "华南-广东工程现场",
            CustomFreightPerTon = 160.0
        };

        var result = PlatePricingRuleEngine.Calculate(p);

        Assert.Equal(200.0, result.BenchmarkFreight);
        Assert.Equal(160.0, result.FreightPerTon);
        Assert.True(result.IsFreightCustomized);
    }

    [Fact]
    public void PlatePricing_CustomThicknessLadderMatrix_AppliesConfiguredLadders()
    {
        // 场景：用户修改全局阶梯矩阵，将 40~60mm 特厚板从默认 180 调整为 210
        var customLadders = PlateThicknessLadder.GetDefaultLadders();
        var targetLadder = customLadders.First(l => l.Matches(50.0));
        targetLadder.CustomSurcharge = 210.0;

        var p = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            ThicknessMm = 50.0,
            SteelMillId = "Baosteel"
        };

        var result = PlatePricingRuleEngine.Calculate(p, customLadders: customLadders);

        Assert.Equal(210.0, result.ThicknessSurchargePerTon);
        Assert.True(result.IsThicknessSurchargeCustomized);
    }
}
