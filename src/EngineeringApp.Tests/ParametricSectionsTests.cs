using EngineeringApp.Shared.Algorithms;
using EngineeringApp.Shared.Models;
using Xunit;

namespace EngineeringApp.Tests;

public class ParametricSectionsTests
{
    private readonly ParametricSectionCalculator _calculator = new();

    [Fact]
    public void Test_Rectangle_Exact_Formulas()
    {
        var p = new SectionParameters
        {
            Type = SectionType.Rectangle,
            Width = 200,
            Height = 400,
            Material = new MaterialProfile { Density = 7850 }
        };

        var res = _calculator.Calculate(p);

        Assert.Equal(80000.0, res.Area, 2);
        Assert.Equal(1200.0, res.Perimeter, 2);
        Assert.Equal(100.0, res.Yc, 2);
        Assert.Equal(200.0, res.Zc, 2);

        // Iy = 1/12 * 200 * 400³ = 1,066,666,666.67 mm⁴
        double expectedIy = (1.0 / 12.0) * 200.0 * Math.Pow(400.0, 3);
        Assert.Equal(expectedIy, res.Iy, 1);

        // Iz = 1/12 * 400 * 200³ = 266,666,666.67 mm⁴
        double expectedIz = (1.0 / 12.0) * 400.0 * Math.Pow(200.0, 3);
        Assert.Equal(expectedIz, res.Iz, 1);

        Assert.Equal(0.0, res.Iyz, 4);

        // Wpl_y = 1/4 * 200 * 400² = 8,000,000 mm³
        Assert.Equal(8000000.0, res.Wply, 1);
        // Wpl_z = 1/4 * 400 * 200² = 4,000,000 mm³
        Assert.Equal(4000000.0, res.Wplz, 1);

        // 形状系数 矩形应该为 1.5
        Assert.Equal(1.5, res.GammaY, 2);
        Assert.Equal(1.5, res.GammaZ, 2);

        // 米重 m = (80000 / 1e6) * 7850 = 628 kg/m
        Assert.Equal(628.0, res.LinearMass, 2);
    }

    [Fact]
    public void Test_Circle_Exact_Formulas()
    {
        var p = new SectionParameters
        {
            Type = SectionType.Circle,
            OuterDiameter = 200
        };

        var res = _calculator.Calculate(p);

        double expectedArea = Math.PI * 100.0 * 100.0;
        Assert.Equal(expectedArea, res.Area, 2);

        double expectedIy = Math.PI * Math.Pow(200.0, 4) / 64.0;
        Assert.Equal(expectedIy, res.Iy, 2);
        Assert.Equal(expectedIy, res.Iz, 2);

        // Wpl = 4/3 * R³
        double expectedWpl = (4.0 / 3.0) * Math.Pow(100.0, 3);
        Assert.Equal(expectedWpl, res.Wply, 2);
    }

    [Fact]
    public void Test_CHS_Pipe_Formulas()
    {
        var p = new SectionParameters
        {
            Type = SectionType.CHS,
            OuterDiameter = 219,
            WallThickness = 8
        };

        var res = _calculator.Calculate(p);

        double ro = 219.0 / 2.0;
        double ri = (219.0 - 16.0) / 2.0;
        double expectedArea = Math.PI * (ro * ro - ri * ri);

        Assert.Equal(expectedArea, res.Area, 1);
        Assert.True(res.Iy > 0);
        Assert.True(res.TorsionConstantJ > 0);
    }

    [Fact]
    public void Test_HBeam_Formulas()
    {
        var p = new SectionParameters
        {
            Type = SectionType.HBeam,
            Width = 300,
            Height = 300,
            WebThickness = 10,
            FlangeThickness = 15
        };

        var res = _calculator.Calculate(p);

        double hw = 300.0 - 30.0;
        double expectedArea = 2.0 * 300.0 * 15.0 + hw * 10.0;
        Assert.Equal(expectedArea, res.Area, 2);

        // 强轴惯性矩
        double expectedIy = (1.0 / 12.0) * (300.0 * Math.Pow(300.0, 3) - 290.0 * Math.Pow(270.0, 3));
        Assert.Equal(expectedIy, res.Iy, 1);

        // 翘曲常数 Iw 必须大于 0
        Assert.True(res.WarpingConstantIw > 0);
        Assert.True(res.TorsionConstantJ > 0);
    }

    [Fact]
    public void Test_Angle_Principal_Axes()
    {
        var p = new SectionParameters
        {
            Type = SectionType.Angle,
            LegWidth1 = 100,
            LegWidth2 = 100,
            LegThickness = 10
        };

        var res = _calculator.Calculate(p);

        Assert.True(res.Area > 0);
        // 等边角钢的主轴偏角应该恰好为 45° 或 -45°
        Assert.Equal(45.0, Math.Abs(res.PrincipalAngleDeg), 1);
        Assert.True(res.I1 > res.I2);
    }
}
