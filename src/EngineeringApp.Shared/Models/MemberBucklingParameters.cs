namespace EngineeringApp.Shared.Models;

/// <summary>
/// 构件受力类型
/// </summary>
public enum MemberForceType
{
    /// <summary>受压构件 (主要构件容许长细比 [λ] = 150，次要构件 180~200)</summary>
    Compression,

    /// <summary>受拉构件 (主要构件容许长细比 [λ] = 350，次要构件 400)</summary>
    Tension
}

/// <summary>
/// 构件计算长度与长细比验算参数
/// </summary>
public class MemberBucklingParameters
{
    /// <summary>构件几何长度 L (mm)</summary>
    public double MemberLength { get; set; } = 6000.0;

    /// <summary>面内计算长度系数 μx (默认 1.0)</summary>
    public double EffectiveLengthFactorX { get; set; } = 1.0;

    /// <summary>面外计算长度系数 μy (默认 1.0)</summary>
    public double EffectiveLengthFactorY { get; set; } = 1.0;

    /// <summary>面内计算长度 L0x (mm)</summary>
    public double L0x => MemberLength * EffectiveLengthFactorX;

    /// <summary>面外计算长度 L0y (mm)</summary>
    public double L0y => MemberLength * EffectiveLengthFactorY;

    /// <summary>构件受力性质</summary>
    public MemberForceType ForceType { get; set; } = MemberForceType.Compression;

    /// <summary>规范容许长细比限值 [λ] (默认受压 150，受拉 350)</summary>
    public double AllowableSlenderness { get; set; } = 150.0;

    /// <summary>是否开启面外与长细比计算</summary>
    public bool EnableSlendernessCheck { get; set; } = true;
}

/// <summary>
/// 构件长细比与稳定性验算结果
/// </summary>
public class MemberSlendernessResult
{
    /// <summary>面内计算长度 L0x (mm)</summary>
    public double L0x { get; set; }

    /// <summary>面外计算长度 L0y (mm)</summary>
    public double L0y { get; set; }

    /// <summary>强轴回转半径 ix (mm)</summary>
    public double RadiusIx { get; set; }

    /// <summary>弱轴/面外回转半径 iy (mm)</summary>
    public double RadiusIy { get; set; }

    /// <summary>强轴长细比 λx = L0x / ix</summary>
    public double LambdaX { get; set; }

    /// <summary>弱轴/面外长细比 λy = L0y / iy</summary>
    public double LambdaY { get; set; }

    /// <summary>第一主轴长细比 λ1 = L0_max / i1</summary>
    public double Lambda1 { get; set; }

    /// <summary>第二主轴长细比 λ2 = L0_max / i2</summary>
    public double Lambda2 { get; set; }

    /// <summary>构件最大控制长细比 λmax = max(λx, λy)</summary>
    public double LambdaMax => Math.Max(LambdaX, LambdaY);

    /// <summary>规范容许长细比限值 [λ]</summary>
    public double AllowableLambda { get; set; } = 150.0;

    /// <summary>长细比利用率比值 η_λ = λmax / [λ]</summary>
    public double SlendernessRatio => AllowableLambda > 0 ? LambdaMax / AllowableLambda : 0;

    /// <summary>是否满足规范长细比限值 (λmax <= [λ])</summary>
    public bool IsSatisfied => LambdaMax <= AllowableLambda;
}
