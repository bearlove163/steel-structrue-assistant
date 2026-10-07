using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Models;
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
    public void SectionRegistry_AllowsDynamicRegistration_OfNewSectionProfile()
    {
        // 模拟未来扩展一个“冷弯C型钢 (檩条)”算子
        var customCProfile = new TestCustomCProfile();
        SectionRegistry.Register(customCProfile);

        // 商务软件通过注册中心动态获取
        var foundProfile = SectionRegistry.GetByKey("ColdFormedC");
        Assert.NotNull(foundProfile);
        Assert.Equal("冷弯C型钢 (檩条)", foundProfile.DisplayName);

        // 验证计算
        var p = new SectionParameters { Height = 200, Width = 70, WebThickness = 20, WallThickness = 2.5 };
        var calcResult = foundProfile.Calculate(p);
        Assert.True(calcResult.OuterPerimeter > 0);
        Assert.True(calcResult.LinearMass > 0);
    }

    private sealed class TestCustomCProfile : ISectionProfile
    {
        public string TypeKey => "ColdFormedC";
        public string DisplayName => "冷弯C型钢 (檩条)";
        public string Category => "冷弯薄壁";
        public SectionType? SectionType => null;

        public SectionPropertiesResult Calculate(SectionParameters parameters)
        {
            // 简单化演示模型：h=200, b=70, c=20, t=2.5
            double h = parameters.Height;
            double b = parameters.Width;
            double c = parameters.WebThickness; // 卷边宽
            double t = parameters.WallThickness; // 壁厚

            // 展开板宽 = h + 2*b + 2*c
            double flatWidth = h + 2 * b + 2 * c;
            double area = flatWidth * t;
            double mass = (area / 1e6) * 7850.0;

            return new SectionPropertiesResult
            {
                Area = area,
                OuterPerimeter = 2.0 * flatWidth, // 薄壁双面全表面积，或外表面 flatWidth
                TopSurfaceWidth = b,
                BottomSurfaceWidth = b,
                LinearMass = mass
            };
        }
    }
}
