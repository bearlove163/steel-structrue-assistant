using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 构件计算长度与长细比 (λx, λy, λmax) 求解引擎
/// 遵循 GB 50017-2017《钢结构设计标准》第 7.4 节 / 第 8.3 节构件长细比限值规定
/// </summary>
public static class SlendernessCalculator
{
    /// <summary>
    /// 根据截面特性与构件计算长度参数计算 λx, λy 与最大控制长细比
    /// </summary>
    public static MemberSlendernessResult Calculate(
        SectionPropertiesResult prop,
        MemberBucklingParameters buckling)
    {
        var result = new MemberSlendernessResult
        {
            L0x = Math.Max(1.0, buckling.L0x),
            L0y = Math.Max(1.0, buckling.L0y),
            AllowableLambda = buckling.AllowableSlenderness
        };

        // 水平轴与竖向轴回转半径
        // 通常截面: Iy 为强轴 (面内), Iz 为弱轴 (面外)
        double ix = prop.IyRadius > 1e-4 ? prop.IyRadius : 1.0;
        double iy = prop.IzRadius > 1e-4 ? prop.IzRadius : 1.0;

        result.RadiusIx = ix;
        result.RadiusIy = iy;

        // 面内强轴长细比 λx = L0x / ix
        result.LambdaX = result.L0x / ix;

        // 面外弱轴长细比 λy = L0y / iy
        result.LambdaY = result.L0y / iy;

        // 主惯性轴长细比 (针对角钢等主轴偏转构件)
        double i1 = prop.I1Radius > 1e-4 ? prop.I1Radius : ix;
        double i2 = prop.I2Radius > 1e-4 ? prop.I2Radius : iy;

        result.Lambda1 = result.L0x / i1;
        result.Lambda2 = result.L0y / i2;

        return result;
    }
}
