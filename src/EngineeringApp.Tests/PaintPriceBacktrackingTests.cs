using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Pricing.Coating;
using Xunit;

namespace EngineeringApp.Tests;

public class PaintPriceBacktrackingTests
{
    [Fact]
    public void Test_Plate_AreaPerTon_InverselyProportionalToThickness()
    {
        // 验证钢板比表面积反比定律 S_plate = 254.78 / t (m²/t)
        double s10 = PaintPriceBacktrackingEngine.CalculatePlateAreaPerTon(10.0);
        double s20 = PaintPriceBacktrackingEngine.CalculatePlateAreaPerTon(20.0);
        double s40 = PaintPriceBacktrackingEngine.CalculatePlateAreaPerTon(40.0);

        Assert.Equal(25.48, s10, 2);
        Assert.Equal(12.74, s20, 2);
        Assert.Equal(6.37, s40, 2);

        // 验证 20mm 钢板比表面积恰为 10mm 钢板的一半
        Assert.Equal(s10 / 2.0, s20, 3);
        Assert.Equal(s20 / 2.0, s40, 3);

        // 验证 ParametricSectionCalculator 物理截面算出的比表面积一致
        var calc = new ParametricSectionCalculator();
        var p10 = new SectionParameters
        {
            Type = SectionType.Plate,
            Height = 10.0,
            Width = 1000.0,
            LegThickness = 10.0
        };
        var res10 = calc.Calculate(p10);

        // 理论延米双面全表面积 2.000 + 0.02 = 2.02 m²/m，米重 78.5 kg/m
        Assert.True(res10.LinearMass > 78.0 && res10.LinearMass < 79.0);
        Assert.True(res10.GrossPaintingAreaPerMeter >= 2.0);
        Assert.True(res10.AreaPerTon > 25.0 && res10.AreaPerTon < 26.0);
    }

    [Fact]
    public void Test_Snapshot_DefaultBasePrices_And_BrandQuotes()
    {
        var allSnaps = SeedPaintPriceSnapshots.AllSnapshots;
        Assert.NotNull(allSnaps);
        Assert.True(allSnaps.Count >= 4);

        var latest = SeedPaintPriceSnapshots.Latest;
        Assert.NotNull(latest);
        Assert.Equal(38.0, latest.EpoxyZincRichPrimerBase);
        Assert.Equal(26.0, latest.EpoxyMioIntermediateBase);
        Assert.Equal(42.0, latest.PolyurethaneTopcoatBase);

        // 测试品牌实体询价值读取 (佐敦底漆应为 43.0，面漆应为 48.0)
        double jotunPrimer = latest.GetPriceForBrandOrCategory("佐敦 Jotun", "底漆");
        double jotunTopcoat = latest.GetPriceForBrandOrCategory("佐敦 Jotun", "面漆");
        Assert.Equal(43.0, jotunPrimer);
        Assert.Equal(48.0, jotunTopcoat);

        // 未录入品牌应平滑降级采用品类基准价
        double unknownPrimer = latest.GetPriceForBrandOrCategory("未知品牌涂料", "底漆");
        Assert.Equal(latest.EpoxyZincRichPrimerBase, unknownPrimer);
    }

    [Fact]
    public void Test_Register_NewBrandQuote_And_ImmediateRecall()
    {
        var targetSnap = SeedPaintPriceSnapshots.Latest;
        string brandName = "宣伟Sherwin-Test";

        var newQuote = new BrandQuoteItem
        {
            BrandName = brandName,
            MaterialCategory = "底漆",
            ProductModel = "Sherwin-Zinc 800",
            UnitPrice = 46.5,
            SupplierName = "宣伟直发测试",
            QuoteDate = DateTime.Today
        };

        targetSnap.AddOrUpdateBrandQuote(newQuote);

        double retrievedPrice = targetSnap.GetPriceForBrandOrCategory(brandName, "底漆");
        Assert.Equal(46.5, retrievedPrice);
    }

    [Fact]
    public void Test_ApplySnapshotPrices_UpdatesLayers()
    {
        var layers = PaintCoatingCalculator.CreatePreset("标准工业厂房重防腐");
        var baseSnap = SeedPaintPriceSnapshots.BaseSnapshot;

        PaintPriceBacktrackingEngine.ApplySnapshotPrices(layers, baseSnap, CoatingApplicationMethod.AirlessSpraying);

        // 基期快照中佐敦底漆询价为 39.0
        var primer = layers.FirstOrDefault(l => l.LayerType == CoatingLayerType.Primer);
        Assert.NotNull(primer);
        Assert.Equal(39.0, primer.MaterialPrice);
        Assert.Equal(baseSnap.AirlessSprayingLaborBase, primer.LaborPrice);

        // 测试若无具体品牌，则平滑降级为基期品类大盘基价 (34.0)
        primer.BrandId = "";
        PaintPriceBacktrackingEngine.ApplySnapshotPrices(layers, baseSnap, CoatingApplicationMethod.AirlessSpraying);
        Assert.Equal(34.0, primer.MaterialPrice);
    }

    [Fact]
    public void Test_BuildScheduleItem_CalculatesTonsAreaAndPriceVariance()
    {
        var baseSnap = SeedPaintPriceSnapshots.BaseSnapshot; // 2026-03 基期
        var targetSnap = SeedPaintPriceSnapshots.Latest;     // 2026-10 现行期
        var layers = PaintCoatingCalculator.CreatePreset("标准工业厂房重防腐");

        // 为 100t t=10mm 钢板构建清单项
        var item = PaintPriceBacktrackingEngine.BuildScheduleItem(
            category: "钢板板材",
            specName: "t = 10mm 钢板",
            thicknessMm: 10.0,
            areaPerTon: 25.48,
            projectTons: 100.0,
            sectionType: SectionType.Plate,
            baseLayers: layers,
            baseSnap: baseSnap,
            targetSnap: targetSnap,
            method: CoatingApplicationMethod.AirlessSpraying,
            partName: "基础底板");

        Assert.Equal("基础底板", item.PartName);
        Assert.Equal(100.0, item.ProjectTons);
        Assert.Equal(25.48, item.AreaPerTon);
        Assert.Equal(2548.0, item.TotalAreaM2, 1);
        Assert.True(item.PaintUsageKgPerTon > 0);
        Assert.True(item.TotalPaintKg > 0);

        // 现行价格高于基期价格，调差应为正数
        Assert.True(item.TargetCostPerTon > item.BaseCostPerTon);
        Assert.True(item.VariancePerTon > 0);
        Assert.True(item.VarianceTotalCost > 0);
        Assert.True(item.VariancePercentage > 0);

        // 验证合价与单价乘积
        Assert.Equal(item.BaseCostPerTon * 100.0, item.BaseTotalCost, 2);
        Assert.Equal(item.TargetCostPerTon * 100.0, item.TargetTotalCost, 2);
        Assert.Equal(item.TargetTotalCost - item.BaseTotalCost, item.VarianceTotalCost, 2);
    }

    [Fact]
    public void Test_PlateSpecs_And_TypicalProfileSpecs_Collections()
    {
        var plates = PaintPriceBacktrackingEngine.GetStandardPlateSpecs();
        Assert.NotEmpty(plates);
        Assert.Equal(17, plates.Count);

        // 随着板厚递增，比表面积严格单调递减
        for (int i = 0; i < plates.Count - 1; i++)
        {
            Assert.True(plates[i].Thickness < plates[i + 1].Thickness);
            Assert.True(plates[i].AreaPerTon > plates[i + 1].AreaPerTon);
        }

        var profiles = PaintPriceBacktrackingEngine.GetTypicalProfileSpecs();
        Assert.NotEmpty(profiles);
        Assert.Contains(profiles, p => p.Type == SectionType.HBeam);
        Assert.Contains(profiles, p => p.Type == SectionType.RHS);
        Assert.Contains(profiles, p => p.Type == SectionType.CHS);
        Assert.Contains(profiles, p => p.Type == SectionType.Angle);
        Assert.Contains(profiles, p => p.Type == SectionType.Channel);
    }
}
