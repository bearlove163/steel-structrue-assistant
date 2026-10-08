namespace EngineeringApp.Shared.Material;

/// <summary>
/// 结构钢材采购与材质基准信息实体
/// </summary>
public class SteelMaterialInfo
{
    public SteelGrade Grade { get; set; }
    public string Name { get; set; } = "";
    public string StandardCode { get; set; } = "GB/T 1591";
    public double YieldStrengthMpa { get; set; } = 355.0;
    public double DensityKgM3 { get; set; } = 7850.0;

    /// <summary>钢厂基准出厂采购指导价 (元/吨)</summary>
    public double BasePricePerTon { get; set; } = 4100.0;

    /// <summary>厚板加价梯队基准 (mm)：超过此厚度触发厚度加价</summary>
    public double ThicknessThresholdMm { get; set; } = 40.0;

    /// <summary>厚板加价单价 (元/吨)</summary>
    public double ThickPlateSurchargePerTon { get; set; } = 150.0;

    /// <summary>
    /// 根据实际板厚计算考虑厚度加价后的材料采购单价 (元/吨)
    /// </summary>
    public double CalculateProcurementPricePerTon(double maxThicknessMm)
    {
        double price = BasePricePerTon;
        if (maxThicknessMm > 60.0)
        {
            price += ThickPlateSurchargePerTon * 2.2; // 特厚板加价
        }
        else if (maxThicknessMm > ThicknessThresholdMm)
        {
            price += ThickPlateSurchargePerTon;
        }
        return price;
    }
}

/// <summary>
/// 全球常用结构钢材标准材质数据库
/// </summary>
public static class SteelMaterialDatabase
{
    private static readonly Dictionary<SteelGrade, SteelMaterialInfo> _materials = new()
    {
        [SteelGrade.Q235B] = new SteelMaterialInfo
        {
            Grade = SteelGrade.Q235B,
            Name = "Q235B 普碳结构钢",
            StandardCode = "GB/T 700",
            YieldStrengthMpa = 235.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 3850.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 120.0
        },
        [SteelGrade.Q345B] = new SteelMaterialInfo
        {
            Grade = SteelGrade.Q345B,
            Name = "Q345B 低合金高强度钢",
            StandardCode = "GB/T 1591-2008",
            YieldStrengthMpa = 345.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4050.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 150.0
        },
        [SteelGrade.Q355B] = new SteelMaterialInfo
        {
            Grade = SteelGrade.Q355B,
            Name = "Q355B 低合金高强度钢",
            StandardCode = "GB/T 1591-2018",
            YieldStrengthMpa = 355.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4100.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 150.0
        },
        [SteelGrade.Q355C] = new SteelMaterialInfo
        {
            Grade = SteelGrade.Q355C,
            Name = "Q355C 优质耐候高强钢",
            StandardCode = "GB/T 1591-2018",
            YieldStrengthMpa = 355.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4250.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 150.0
        },
        [SteelGrade.Q420B] = new SteelMaterialInfo
        {
            Grade = SteelGrade.Q420B,
            Name = "Q420B 高性能结构钢",
            StandardCode = "GB/T 1591-2018",
            YieldStrengthMpa = 420.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4600.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 200.0
        },
        [SteelGrade.S235JR] = new SteelMaterialInfo
        {
            Grade = SteelGrade.S235JR,
            Name = "EN S235JR 欧标结构钢",
            StandardCode = "EN 10025-2",
            YieldStrengthMpa = 235.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4150.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 150.0
        },
        [SteelGrade.S355JR] = new SteelMaterialInfo
        {
            Grade = SteelGrade.S355JR,
            Name = "EN S355JR 欧标高强钢",
            StandardCode = "EN 10025-2",
            YieldStrengthMpa = 355.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4350.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 160.0
        },
        [SteelGrade.A36] = new SteelMaterialInfo
        {
            Grade = SteelGrade.A36,
            Name = "ASTM A36 美标碳素钢",
            StandardCode = "ASTM A36/A36M",
            YieldStrengthMpa = 250.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4200.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 150.0
        },
        [SteelGrade.A992] = new SteelMaterialInfo
        {
            Grade = SteelGrade.A992,
            Name = "ASTM A992 美标建筑型钢专用钢",
            StandardCode = "ASTM A992/A992M",
            YieldStrengthMpa = 345.0,
            DensityKgM3 = 7850.0,
            BasePricePerTon = 4450.0,
            ThicknessThresholdMm = 40.0,
            ThickPlateSurchargePerTon = 160.0
        }
    };

    public static IReadOnlyCollection<SteelMaterialInfo> AllMaterials => _materials.Values;

    public static SteelMaterialInfo Get(SteelGrade grade)
    {
        if (_materials.TryGetValue(grade, out var info)) return info;
        return _materials[SteelGrade.Q355B];
    }
}
