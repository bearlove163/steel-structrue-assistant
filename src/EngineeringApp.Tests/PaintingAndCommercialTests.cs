using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Data;
using Xunit;

namespace EngineeringApp.Tests;

public class PaintingAndCommercialTests
{
    private readonly ParametricSectionCalculator _calc = new();
    private readonly CommercialSectionCalculator _commercialCalc = new();

    [Fact]
    public void HBeam_PaintingArea_WithAndWithoutTopSlabDeduction_IsAccurate()
    {
        // Arrange: H 300x300x10x15
        var p = new SectionParameters
        {
            Type = SectionType.HBeam,
            Height = 300,
            Width = 300,
            WebThickness = 10,
            FlangeThickness = 15
        };

        // Act
        var res = _calc.Calculate(p);

        // Assert
        // 周长 = 2 * (2 * 300 + 300 - 10) = 2 * 890 = 1780 mm
        Assert.Equal(1780.0, res.OuterPerimeter, precision: 2);
        Assert.Equal(1.780, res.GrossPaintingAreaPerMeter, precision: 3);
        Assert.Equal(300.0, res.TopSurfaceWidth);

        // 扣除顶面 (楼板贴合免涂装)
        // 净涂装面积 = (1780 - 300) / 1000 = 1.480 m²/m
        Assert.Equal(1.480, res.PaintingAreaExcludingTop, precision: 3);

        // 使用通用接口 IPaintingAreaCalculator
        var paintingCalc = new DefaultPaintingAreaCalculator();
        double netAreaWithFloor = paintingCalc.CalculateNetPaintingArea(res, PaintingCalculationOptions.BeamWithFloorSlab());
        Assert.Equal(1.480, netAreaWithFloor, precision: 3);

        // 8 米梁总涂装面积
        double totalPaintingArea = paintingCalc.CalculateTotalPaintingArea(res, 8.0, PaintingCalculationOptions.BeamWithFloorSlab());
        Assert.Equal(8.0 * 1.480, totalPaintingArea, precision: 3);
    }

    [Fact]
    public void BoxSection_RHS_TopSlabDeduction_And_InnerGalvanizing()
    {
        // Arrange: 方管/箱型 400x400x16
        var p = new SectionParameters
        {
            Type = SectionType.RHS,
            Height = 400,
            Width = 400,
            WallThickness = 16
        };

        // Act
        var res = _calc.Calculate(p);

        // Assert
        // 外周长 = 2 * (400 + 400) = 1600 mm = 1.60 m²/m
        Assert.Equal(1600.0, res.OuterPerimeter, precision: 2);
        Assert.Equal(1.600, res.GrossPaintingAreaPerMeter, precision: 3);
        Assert.Equal(400.0, res.TopSurfaceWidth);

        // 内周长 = 2 * (368 + 368) = 1472 mm
        Assert.Equal(1472.0, res.InnerPerimeter, precision: 2);

        // 1. 扣除上表面楼板密贴
        double netAreaExTop = res.CalculatePaintingArea(PaintingCalculationOptions.BeamWithFloorSlab());
        Assert.Equal((1600.0 - 400.0) / 1000.0, netAreaExTop, precision: 3); // 1.20 m²/m

        // 2. 热浸镀锌（含内腔浸润）
        double galvanizingArea = res.CalculatePaintingArea(PaintingCalculationOptions.HotDipGalvanizing());
        Assert.Equal((1600.0 + 1472.0) / 1000.0, galvanizingArea, precision: 3); // 3.072 m²/m
    }

    [Fact]
    public void CustomTopDeductionWidth_WorksCorrectly()
    {
        // 例如 300mm 宽翼缘，但实际仅有 200mm 贴合次梁或预制条板
        var p = new SectionParameters
        {
            Type = SectionType.HBeam,
            Height = 300,
            Width = 300,
            WebThickness = 10,
            FlangeThickness = 15
        };
        var res = _calc.Calculate(p);

        var customOptions = new PaintingCalculationOptions
        {
            ExcludeTopSurface = true,
            TopSurfaceDeductionWidth = 200.0 // 自定义扣除 200mm
        };

        double netArea = res.CalculatePaintingArea(customOptions);
        Assert.Equal((1780.0 - 200.0) / 1000.0, netArea, precision: 3); // 1.580 m²/m
    }

    [Fact]
    public void CommercialSectionCalculator_ComputesMass_Areas_AndCosts()
    {
        var p = new SectionParameters
        {
            Type = SectionType.HBeam,
            Height = 300,
            Width = 300,
            WebThickness = 10,
            FlangeThickness = 15
        };

        var commOptions = new CommercialCalculationOptions
        {
            LengthMeter = 10.0,
            Quantity = 4,
            SteelUnitPricePerKg = 4.5,
            PaintingUnitPricePerM2 = 30.0,
            SteelLossRate = 0.05, // 5% 钢材损耗
            Painting = PaintingCalculationOptions.BeamWithFloorSlab() // 扣除楼板贴合
        };

        var result = _commercialCalc.Calculate(p, commOptions);

        Assert.Equal(40.0, result.TotalLengthMeter); // 10m * 4根 = 40m
        Assert.True(result.LinearMass > 80.0);
        Assert.Equal(1.780, result.OuterSurfaceAreaPerMeter, precision: 3);
        Assert.Equal(1.480, result.NetPaintingAreaPerMeter, precision: 3);

        // 总涂装面积 = 40m * 1.48 m²/m = 59.2 m²
        Assert.Equal(40.0 * 1.480, result.TotalPaintingAreaM2, precision: 2);

        // 验证防腐成本
        Assert.Equal(result.TotalPaintingAreaM2 * 30.0, result.EstimatedPaintingCost, precision: 2);
        // 验证钢材成本
        Assert.Equal(result.TotalWeightKg * 4.5, result.EstimatedSteelCost, precision: 2);
        Assert.True(result.AreaPerTon > 0);
    }

    [Fact]
    public void CircularPipe_SprayingLoss_IsHigherThan_BoxSection()
    {
        // 验证物理规律：圆管曲率切线导致过喷飞散，其损耗修正系数应显著高于平整箱型截面
        double boxFactor = SectionShapeLossConfig.GetSectionShapeFactor(SectionType.RHS);
        double pipeFactor = SectionShapeLossConfig.GetSectionShapeFactor(SectionType.CHS);
        double angleFactor = SectionShapeLossConfig.GetSectionShapeFactor(SectionType.Angle);

        Assert.Equal(1.0, boxFactor);
        Assert.True(pipeFactor > boxFactor, "圆管喷涂损耗率应当大于箱型截面");
        Assert.True(angleFactor > pipeFactor, "角钢狭窄肢件格构喷涂损耗率应当大于圆管");
    }

    [Fact]
    public void ApplicationMethods_LossFactor_And_LaborPrice_Verification()
    {
        // 高压无气喷涂 (基准)、空气喷涂 (飞散大)、刷滚涂 (零飞散但工效慢)
        double airlessLoss = SectionShapeLossConfig.GetMethodLossFactor(CoatingApplicationMethod.AirlessSpraying);
        double airLoss = SectionShapeLossConfig.GetMethodLossFactor(CoatingApplicationMethod.AirSpraying);
        double brushLoss = SectionShapeLossConfig.GetMethodLossFactor(CoatingApplicationMethod.BrushOrRoller);

        Assert.Equal(1.0, airlessLoss);
        Assert.True(airLoss > airlessLoss, "常规空气喷涂漆雾飞散损耗应当高于高压无气喷涂");
        Assert.True(brushLoss < airlessLoss, "手工刷滚涂几乎无飞散，损耗系数应当显著低于无气喷涂");

        // 人工工费单价倍数
        double brushLaborMult = SectionShapeLossConfig.GetMethodLaborMultiplier(CoatingApplicationMethod.BrushOrRoller);
        Assert.True(brushLaborMult > 1.4, "手工刷滚涂工效低，人工工费单价倍数应当提高");
    }

    [Fact]
    public void HotDipGalvanizing_LightAngle_Vs_HeavyHBeam_TonnagePricing_Verification()
    {
        // 1. 输变电铁塔轻型角钢 L50x5
        var angleSection = new SectionPropertiesResult
        {
            Area = 480.0,
            OuterPerimeter = 200.0,
            LinearMass = 3.77
        };

        // 2. 重型大截面 H型钢 HW 350x350x12x19
        var heavyHSection = new SectionPropertiesResult
        {
            Area = 17190.0,
            OuterPerimeter = 2050.0,
            LinearMass = 135.0
        };

        var options = new HotDipGalvanizingOptions
        {
            ZincGrade = ZincIngotGrade.Grade1_Standard_9999,
            ZincPricePerTon = 23000.0,
            DftMicrons = 85.0
        };

        var angleHdg = PaintCoatingCalculator.CalculateHotDipGalvanizing(angleSection, options);
        var heavyHdg = PaintCoatingCalculator.CalculateHotDipGalvanizing(heavyHSection, options);

        // 验证：轻角钢比表面积远大于重型H型钢
        Assert.True(angleHdg.SpecificSurfaceAreaTon > heavyHdg.SpecificSurfaceAreaTon * 3.0,
            $"角钢比表面积 ({angleHdg.SpecificSurfaceAreaTon:F1}) 应当是重型H型钢 ({heavyHdg.SpecificSurfaceAreaTon:F1}) 的3倍以上");

        // 验证：轻角钢每吨耗锌量远大于重型H型钢
        Assert.True(angleHdg.ZincConsumptionKgPerTon > heavyHdg.ZincConsumptionKgPerTon * 2.5,
            $"角钢每吨耗锌 ({angleHdg.ZincConsumptionKgPerTon:F1}kg) 远大于重型H型钢 ({heavyHdg.ZincConsumptionKgPerTon:F1}kg)");

        // 验证：轻角钢吨报价显著昂贵于重型H型钢
        Assert.True(angleHdg.TotalPricePerTon > heavyHdg.TotalPricePerTon,
            $"输变电铁塔轻角钢吨加工费 ({angleHdg.TotalPricePerTon:F0}) 应当显著高于重型H型钢 ({heavyHdg.TotalPricePerTon:F0})");
    }

    [Fact]
    public void ZincIngot_PurityGrades_PricingDifference_Verification()
    {
        var section = new SectionPropertiesResult { Area = 5000, OuterPerimeter = 1000, LinearMass = 39.25 };

        var opt0 = new HotDipGalvanizingOptions { ZincGrade = ZincIngotGrade.Grade0_HighPurity_99995 };
        var opt1 = new HotDipGalvanizingOptions { ZincGrade = ZincIngotGrade.Grade1_Standard_9999 };
        var optSecondary = new HotDipGalvanizingOptions { ZincGrade = ZincIngotGrade.Secondary_CrudeZinc };

        var res0 = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, opt0);
        var res1 = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, opt1);
        var resSec = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, optSecondary);

        Assert.True(res0.EffectiveZincPricePerTon > res1.EffectiveZincPricePerTon, "0# 特纯锌价格应当高于 1# 结构纯锌");
        Assert.True(res1.EffectiveZincPricePerTon > resSec.EffectiveZincPricePerTon, "1# 纯锌价格应当高于再生粗锌");
        Assert.True(res0.TotalPricePerTon > res1.TotalPricePerTon);
    }

    [Fact]
    public void PaintBrandDatabase_Contains_GlobalAndDomesticGiants_And_QueriesWork()
    {
        var brands = PaintProductDatabase.AllBrands;
        Assert.Contains(brands, b => b.BrandId == "Jotun");
        Assert.Contains(brands, b => b.BrandId == "Hempel");
        Assert.Contains(brands, b => b.BrandId == "AkzoNobel");
        Assert.Contains(brands, b => b.BrandId == "SKK");
        Assert.Contains(brands, b => b.BrandId == "BBMG");
        Assert.Contains(brands, b => b.BrandId == "Lanling");

        var jotunProducts = PaintProductDatabase.GetProductsByBrand("Jotun").ToList();
        Assert.NotEmpty(jotunProducts);
        Assert.Contains(jotunProducts, p => p.ModelCode.Contains("Penguard") || p.ModelCode.Contains("Barrier"));

        var fireproofProducts = PaintProductDatabase.GetByType(CoatingLayerType.Fireproof).ToList();
        Assert.NotEmpty(fireproofProducts);
    }

    [Fact]
    public void PaintCoatingCalculator_Generates_ProcessTrace_And_Describe()
    {
        var section = new SectionPropertiesResult
        {
            Area = 11800,
            OuterPerimeter = 1760,
            LinearMass = 93.0
        };

        var layers = PaintCoatingCalculator.CreatePreset("标准工业厂房重防腐");
        var res = PaintCoatingCalculator.Calculate(
            section,
            layers,
            excludeTopSurface: true,
            method: CoatingApplicationMethod.AirlessSpraying,
            sectionType: SectionType.HBeam);

        Assert.NotNull(res.ProcessTrace);
        Assert.NotEmpty(res.ProcessTrace.Steps);
        Assert.Contains("直角阴角", res.ProcessTrace.SectionShapeExplanation);
        Assert.Contains("高压无气喷涂", res.ProcessTrace.MethodExplanation);
        Assert.NotEmpty(res.FullTechnicalDescribe);
        Assert.True(res.SectionFactorFOverV > 0);
    }

    [Fact]
    public void HotDipGalvanizing_DoubleDip_And_ComplexCavity_Surcharges()
    {
        var section = new SectionPropertiesResult { Area = 12000, OuterPerimeter = 1800, LinearMass = 94.2 };

        var optBase = new HotDipGalvanizingOptions { IsDoubleDipOverlength = false, HasComplexCavities = false };
        var optDoubleDip = new HotDipGalvanizingOptions { IsDoubleDipOverlength = true, HasComplexCavities = false };
        var optCavity = new HotDipGalvanizingOptions { IsDoubleDipOverlength = false, HasComplexCavities = true };

        var resBase = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, optBase);
        var resDoubleDip = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, optDoubleDip);
        var resCavity = PaintCoatingCalculator.CalculateHotDipGalvanizing(section, optCavity);

        Assert.True(resDoubleDip.TotalPricePerTon > resBase.TotalPricePerTon, "超长倒头双浸应当加收工艺溢价");
        Assert.True(resDoubleDip.OverlengthSurchargePerTon > 0);
        Assert.True(resCavity.TotalPricePerTon > resBase.TotalPricePerTon, "复杂闭腔开孔排气应当加收难度费");
    }

    [Fact]
    public void PaintProductDatabase_Crud_And_Serialization()
    {
        int initialCount = PaintProductDatabase.AllProducts.Count;
        Assert.True(initialCount > 20);

        string json = PaintProductDatabase.ExportToJson();
        Assert.NotEmpty(json);
        Assert.Contains("佐敦", json);
        Assert.Contains("海虹老人", json);
        Assert.Contains("阿克苏", json);

        var newItem = new PaintProductItem
        {
            BrandId = "TestBrand",
            Name = "自动化测试特种涂料",
            LayerType = CoatingLayerType.Primer,
            MaterialUnitPricePerKg = 50.0
        };

        PaintProductDatabase.AddProduct(newItem);
        Assert.Equal(initialCount + 1, PaintProductDatabase.AllProducts.Count);

        bool removed = PaintProductDatabase.RemoveProduct(newItem.Id);
        Assert.True(removed);
        Assert.Equal(initialCount, PaintProductDatabase.AllProducts.Count);
    }
}

