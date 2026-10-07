using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Models;
using Xunit;

namespace EngineeringApp.Tests;

public class PaintAndDatabaseTests
{
    [Fact]
    public void TestEuroSteelDatabase_ContainsIpeAndHea()
    {
        var euroSections = EuroSteelDatabase.AllItems;
        Assert.NotEmpty(euroSections);
        Assert.Contains(euroSections, s => s.Designation == "IPE 300");
        Assert.Contains(euroSections, s => s.Designation == "HE 200 A");
        Assert.Contains(euroSections, s => s.Designation == "HE 300 B");
        Assert.Contains(euroSections, s => s.Designation == "UPN 200");

        var ipe300 = euroSections.First(s => s.Designation == "IPE 300");
        Assert.Equal(300, ipe300.Height);
        Assert.Equal(150, ipe300.Width);
        Assert.Equal(7.1, ipe300.WebThickness);
        Assert.Equal(10.7, ipe300.FlangeThickness);
    }

    [Fact]
    public void TestAiscSteelDatabase_ContainsWShapes()
    {
        var aiscSections = AiscSteelDatabase.AllItems;
        Assert.NotEmpty(aiscSections);
        Assert.Contains(aiscSections, s => s.Designation == "W14×90");
        Assert.Contains(aiscSections, s => s.Designation == "W24×68");
        Assert.Contains(aiscSections, s => s.Designation == "C10×15.3");
        Assert.Contains(aiscSections, s => s.Designation == "HSS 8×8×1/2");

        var w14x90 = aiscSections.First(s => s.Designation == "W14×90");
        Assert.Equal(356, w14x90.Height);
        Assert.Equal(369, w14x90.Width);
    }

    [Fact]
    public void TestStructuralSteelLibrary_GlobalFilter()
    {
        var all = StructuralSteelLibrary.AllSections;
        Assert.True(all.Count > 100);

        var euro = StructuralSteelLibrary.Filter("欧标", null, "IPE").ToList();
        Assert.NotEmpty(euro);
        Assert.All(euro, s => Assert.Equal("EN (欧标)", s.StandardSystem));

        var aisc = StructuralSteelLibrary.Filter("美标", null, "W").ToList();
        Assert.NotEmpty(aisc);
        Assert.All(aisc, s => Assert.Equal("AISC (美标)", s.StandardSystem));
    }

    [Fact]
    public void TestPaintCoatingCalculator_PrimerAndIntermediate()
    {
        var calculator = new ParametricSectionCalculator();
        var hbeam = calculator.Calculate(new SectionParameters
        {
            Type = SectionType.HBeam,
            Height = 300,
            Width = 300,
            WebThickness = 10,
            FlangeThickness = 15
        });

        var layers = PaintCoatingCalculator.CreatePreset("底漆 + 中漆");
        var res = PaintCoatingCalculator.Calculate(hbeam, layers, excludeTopSurface: false, projectLengthM: 100);

        Assert.Equal(2, res.Layers.Count);
        Assert.True(res.TotalPaintDftMicrons > 150);
        Assert.True(res.TotalCostPerM2 > 0);
        Assert.True(res.TotalCostPerMeter > 0);
        Assert.True(res.TotalCostPerTon > 0);
        Assert.True(res.ProjectTotalCostYuan > 0);
    }

    [Fact]
    public void TestPaintCoatingCalculator_WithFireproof()
    {
        var calculator = new ParametricSectionCalculator();
        var hbeam = calculator.Calculate(new SectionParameters
        {
            Type = SectionType.HBeam,
            Height = 300,
            Width = 300,
            WebThickness = 10,
            FlangeThickness = 15
        });

        var layers = PaintCoatingCalculator.CreatePreset("底漆 + 中漆 + 防火涂料");
        var res = PaintCoatingCalculator.Calculate(hbeam, layers, excludeTopSurface: true, projectLengthM: 50);

        Assert.Equal(3, res.Layers.Count);
        Assert.True(res.FireproofThicknessMm > 1.0);
        Assert.True(res.TopSurfaceExcluded);
        Assert.True(res.AppliedPaintingAreaPerMeter < hbeam.GrossPaintingAreaPerMeter);
    }
}
