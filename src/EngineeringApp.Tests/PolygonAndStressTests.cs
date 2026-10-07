using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Models;
using Xunit;

namespace EngineeringApp.Tests;

public class PolygonIntegrationTests
{
    [Fact]
    public void Test_Polygon_Matches_Analytical_Rectangle()
    {
        double b = 150.0;
        double h = 250.0;

        List<Point2D> rectLoop =
        [
            new(0, 0),
            new(b, 0),
            new(b, h),
            new(0, h)
        ];

        var res = GreenTheoremPolygonEngine.CalculateProperties(rectLoop);

        double expectedArea = b * h;
        double expectedIy = (1.0 / 12.0) * b * Math.Pow(h, 3);
        double expectedIz = (1.0 / 12.0) * h * Math.Pow(b, 3);

        Assert.Equal(expectedArea, res.Area, 3);
        Assert.Equal(b / 2.0, res.Yc, 3);
        Assert.Equal(h / 2.0, res.Zc, 3);
        Assert.Equal(expectedIy, res.Iy, 1);
        Assert.Equal(expectedIz, res.Iz, 1);
        Assert.Equal(0.0, res.Iyz, 3);
    }

    [Fact]
    public void Test_Polygon_With_Inner_Hole()
    {
        // 200x200 外框，带 100x100 居中孔
        List<Point2D> outer =
        [
            new(0, 0),
            new(200, 0),
            new(200, 200),
            new(0, 200)
        ];

        List<Point2D> hole =
        [
            new(50, 50),
            new(150, 50),
            new(150, 150),
            new(50, 150)
        ];

        var res = GreenTheoremPolygonEngine.CalculateProperties(outer, [hole]);

        double expectedArea = 200 * 200 - 100 * 100; // 30,000
        Assert.Equal(expectedArea, res.Area, 2);
        Assert.Equal(100.0, res.Yc, 2);
        Assert.Equal(100.0, res.Zc, 2);

        double expectedI = (1.0 / 12.0) * (200 * Math.Pow(200, 3) - 100 * Math.Pow(100, 3));
        Assert.Equal(expectedI, res.Iy, 1);
        Assert.Equal(expectedI, res.Iz, 1);
    }
}

public class StressAnalysisTests
{
    [Fact]
    public void Test_Combined_Axial_And_Bending_Stress()
    {
        var calc = new ParametricSectionCalculator();
        var p = new SectionParameters
        {
            Type = SectionType.Rectangle,
            Width = 200,
            Height = 400,
            Material = new MaterialProfile { YieldStrength = 235 }
        };
        var prop = calc.Calculate(p);

        var load = new LoadCase
        {
            AxialForceN = -800, // 800 kN 压力
            BendingMomentMy = 80, // 80 kN·m 弯矩
            BendingMomentMz = 0
        };

        var stressRes = StressAnalysisEngine.Analyze(prop, load, p.Material);

        // 轴心正应力: -800,000 / 80,000 = -10 MPa
        // 弯曲极限应力: My / Wy = 80 * 1e6 / (1/6 * 200 * 400²) = 80e6 / 5.3333e6 = 15 MPa
        // 顶边缘受压 (z' = +200): -10 - 15 = -25 MPa
        // 底边缘受拉 (z' = -200): -10 + 15 = +5 MPa
        Assert.Equal(5.0, stressRes.MaxTensileStress, 1);
        Assert.Equal(-25.0, stressRes.MinCompressiveStress, 1);
        Assert.Equal(25.0 / 235.0, stressRes.StressRatio, 2);

        // 必须存在中和轴
        Assert.NotNull(stressRes.NeutralAxisPoint1);
        Assert.NotNull(stressRes.NeutralAxisPoint2);
    }
}

public class StandardSteelDatabaseTests
{
    [Fact]
    public void Test_Database_Contains_Valid_Standard_Sections()
    {
        var items = StandardSteelDatabase.AllItems;
        Assert.NotEmpty(items);
        Assert.True(items.Count >= 40);

        // 验证有 HW, HM, HN, I, [, L
        Assert.Contains(items, x => x.Designation.StartsWith("HW"));
        Assert.Contains(items, x => x.Designation.StartsWith("HN"));
        Assert.Contains(items, x => x.Designation.StartsWith("I"));
        Assert.Contains(items, x => x.Designation.StartsWith("["));
        Assert.Contains(items, x => x.Designation.StartsWith("L"));

        foreach (var item in items)
        {
            Assert.True(item.Height > 0);
            Assert.True(item.StandardAreaCm2 > 0);
            Assert.True(item.StandardMassKgM > 0);
        }
    }
}
