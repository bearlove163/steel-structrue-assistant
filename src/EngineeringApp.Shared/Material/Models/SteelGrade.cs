namespace EngineeringApp.Shared.Material;

/// <summary>
/// 结构钢材标准材质牌号 (收录中国 GB、欧洲 EN 及美国 ASTM 常见结构钢)
/// </summary>
public enum SteelGrade
{
    // --- 中国国标 GB / GB/T ---
    Q235B,
    Q345B,
    Q355B,
    Q355C,
    Q355D,
    Q390B,
    Q420B,

    // --- 欧洲标准 EN 10025 ---
    S235JR,
    S355JR,
    S355J2,
    S460N,

    // --- 美国标准 ASTM ---
    A36,
    A572_Gr50,
    A992,

    // --- 特殊与自定义材料 ---
    StainlessSteel_S30408,
    Aluminum_6061T6,
    Custom
}

public static class SteelGradeExtensions
{
    public static string GetStandardName(this SteelGrade grade) => grade switch
    {
        SteelGrade.Q235B => "Q235B 普碳结构钢 (GB/T 700)",
        SteelGrade.Q345B => "Q345B 低合金高强度结构钢 (GB/T 1591-2008)",
        SteelGrade.Q355B => "Q355B 低合金高强度结构钢 (GB/T 1591-2018)",
        SteelGrade.Q355C => "Q355C 低合金高强钢 (0℃冲击)",
        SteelGrade.Q355D => "Q355D 低合金高强钢 (-20℃冲击)",
        SteelGrade.Q390B => "Q390B 高强度结构钢",
        SteelGrade.Q420B => "Q420B 高强度结构钢",
        SteelGrade.S235JR => "EN S235JR 欧标结构钢",
        SteelGrade.S355JR => "EN S355JR 欧标高强钢",
        SteelGrade.S355J2 => "EN S355J2 欧标耐低温钢",
        SteelGrade.S460N => "EN S460N 欧标超高强钢",
        SteelGrade.A36 => "ASTM A36 美标碳素钢",
        SteelGrade.A572_Gr50 => "ASTM A572 Grade 50 美标高强低合金钢",
        SteelGrade.A992 => "ASTM A992 美标建筑型钢专用高强钢",
        SteelGrade.StainlessSteel_S30408 => "S30408 奥氏体不锈钢 (06Cr19Ni10)",
        SteelGrade.Aluminum_6061T6 => "6061-T6 结构铝合金",
        _ => "自定义材质"
    };

    public static double GetDefaultYieldStrength(this SteelGrade grade) => grade switch
    {
        SteelGrade.Q235B => 235.0,
        SteelGrade.Q345B => 345.0,
        SteelGrade.Q355B or SteelGrade.Q355C or SteelGrade.Q355D => 355.0,
        SteelGrade.Q390B => 390.0,
        SteelGrade.Q420B => 420.0,
        SteelGrade.S235JR => 235.0,
        SteelGrade.S355JR or SteelGrade.S355J2 => 355.0,
        SteelGrade.S460N => 460.0,
        SteelGrade.A36 => 250.0,
        SteelGrade.A572_Gr50 or SteelGrade.A992 => 345.0,
        SteelGrade.StainlessSteel_S30408 => 205.0,
        SteelGrade.Aluminum_6061T6 => 240.0,
        _ => 345.0
    };
}
