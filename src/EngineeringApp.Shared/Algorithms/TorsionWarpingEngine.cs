namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面圣维南自由扭转常数 J (It) 与 扇性惯性矩/翘曲常数 Iw (Cw) 计算引擎
/// 基于经典弹性理论与薄壁构件分析规范 (AISC / Eurocode 3 / 钢结构设计标准 GB 50017)
/// </summary>
public static class TorsionWarpingEngine
{
    /// <summary>
    /// 实心矩形截面扭转常数
    /// </summary>
    public static (double J, double Iw) CalculateRectangle(double width, double height)
    {
        double b = Math.Max(width, height);
        double t = Math.Min(width, height);
        // 经典圣维南矩形级数近似公式
        double j = b * Math.Pow(t, 3) * (1.0 / 3.0 - 0.21 * (t / b) * (1.0 - Math.Pow(t / b, 4) / 12.0));
        return (Math.Max(0, j), 0);
    }

    /// <summary>
    /// 实心圆截面
    /// </summary>
    public static (double J, double Iw) CalculateCircle(double d)
    {
        double j = Math.PI * Math.Pow(d, 4) / 32.0;
        return (j, 0);
    }

    /// <summary>
    /// 空心圆管 (CHS)
    /// </summary>
    public static (double J, double Iw) CalculateCHS(double d, double t)
    {
        double dOuter = d;
        double dInner = Math.Max(0, d - 2.0 * t);
        double j = Math.PI * (Math.Pow(dOuter, 4) - Math.Pow(dInner, 4)) / 32.0;
        return (j, 0);
    }

    /// <summary>
    /// 空心方矩管 / 箱型截面 (RHS) - 布雷特 (Bredt) 第一公式
    /// </summary>
    public static (double J, double Iw) CalculateRHS(double b, double h, double t)
    {
        double bMid = Math.Max(0, b - t);
        double hMid = Math.Max(0, h - t);
        double am = bMid * hMid; // 中面所围面积
        // 沿中线回路积分 ∮ ds / t = 2 * (bMid + hMid) / t
        double perimeterMid = 2.0 * (bMid + hMid);
        double j = perimeterMid > 1e-6 ? 4.0 * am * am * t / perimeterMid : 0;
        return (j, 0);
    }

    /// <summary>
    /// H型钢 / 双对称工字钢
    /// </summary>
    public static (double J, double Iw) CalculateHBeam(double b, double h, double tw, double tf)
    {
        double hw = Math.Max(0, h - 2.0 * tf);
        // 圣维南扭转常数 J = 2 * (1/3 * b * tf³) + 1/3 * hw * tw³ + 根部加强系数
        double j = (2.0 / 3.0) * b * Math.Pow(tf, 3) + (1.0 / 3.0) * hw * Math.Pow(tw, 3);

        // 翘曲常数 Iw = Iz_flange * (h - tf)² / 2 = (tf * b³ / 12) * (h - tf)² / 2 = tf * b³ * (h - tf)² / 24
        double hDistance = Math.Max(0, h - tf);
        double iw = tf * Math.Pow(b, 3) * Math.Pow(hDistance, 2) / 24.0;

        return (j, iw);
    }

    /// <summary>
    /// 槽钢 (Channel)
    /// </summary>
    public static (double J, double Iw) CalculateChannel(double b, double h, double tw, double tf)
    {
        double hw = Math.Max(0, h - 2.0 * tf);
        double j = (2.0 / 3.0) * b * Math.Pow(tf, 3) + (1.0 / 3.0) * hw * Math.Pow(tw, 3);

        // Eurocode / AISC 经典单轴对称槽钢翘曲常数公式
        double hMid = Math.Max(0, h - tf);
        double bMid = Math.Max(0, b - tw / 2.0);
        double denom = hMid + 6.0 * bMid;
        double iw = denom > 1e-6 ? (Math.Pow(hMid, 2) * Math.Pow(bMid, 3) * tf / 12.0) * ((2.0 * hMid + 3.0 * bMid) / denom) : 0;

        return (j, iw);
    }

    /// <summary>
    /// 角钢 (Angle)
    /// </summary>
    public static (double J, double Iw) CalculateAngle(double b1, double b2, double t)
    {
        // 开口交于一点的薄壁杆件，剪切中心在两肢交点，Iw 几乎为零
        double j = (1.0 / 3.0) * (b1 + b2 - t) * Math.Pow(t, 3);
        return (j, 0);
    }

    /// <summary>
    /// T型钢
    /// </summary>
    public static (double J, double Iw) CalculateTSection(double b, double h, double tw, double tf)
    {
        double hw = Math.Max(0, h - tf);
        double j = (1.0 / 3.0) * b * Math.Pow(tf, 3) + (1.0 / 3.0) * hw * Math.Pow(tw, 3);
        // T型截面翘曲常数较小
        double iw = Math.Pow(b, 3) * Math.Pow(tf, 3) / 144.0;
        return (j, iw);
    }

    /// <summary>
    /// 十字形截面 (Cruciform)
    /// </summary>
    public static (double J, double Iw) CalculateCruciform(double h, double b, double tw, double tf)
    {
        // 沿中心交汇
        double j = (1.0 / 3.0) * (h * Math.Pow(tw, 3) + (b - tw) * Math.Pow(tf, 3));
        return (j, 0);
    }
}
