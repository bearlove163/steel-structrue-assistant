namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板厚度加价阶梯规则模型 (可由用户自定义配置并持久化至数据库)
/// </summary>
public class PlateThicknessLadder
{
    public int Id { get; set; }
    
    /// <summary>适用最小板厚 (mm, 包含)</summary>
    public double MinThicknessMm { get; set; }
    
    /// <summary>适用最大板厚 (mm, 不包含，例如 20 代表 < 20.0)</summary>
    public double MaxThicknessMm { get; set; }
    
    /// <summary>行业标准参考基准加价 (元/吨)</summary>
    public double BenchmarkSurcharge { get; set; }
    
    /// <summary>用户自定义加价 (元/吨，若为 null 则采用参考基准加价)</summary>
    public double? CustomSurcharge { get; set; }
    
    /// <summary>规则阶梯区间业务说明</summary>
    public string Description { get; set; } = "";

    /// <summary>生效加价 (元/吨)</summary>
    public double EffectiveSurcharge => CustomSurcharge ?? BenchmarkSurcharge;

    /// <summary>是否已被用户自定义设置</summary>
    public bool IsCustomized => CustomSurcharge.HasValue;

    /// <summary>判断厚度是否命中该阶梯</summary>
    public bool Matches(double thicknessMm)
    {
        return thicknessMm >= MinThicknessMm && thicknessMm < MaxThicknessMm;
    }

    /// <summary>
    /// 获取全国钢铁行业公认的标准厚度加价阶梯基准列表
    /// </summary>
    public static List<PlateThicknessLadder> GetDefaultLadders() =>
    [
        new PlateThicknessLadder
        {
            Id = 1,
            MinThicknessMm = 0.0,
            MaxThicknessMm = 8.0,
            BenchmarkSurcharge = 120.0,
            Description = "极薄规格板 (t < 8mm)：薄辊轧制与慢速下料加价"
        },
        new PlateThicknessLadder
        {
            Id = 2,
            MinThicknessMm = 8.0,
            MaxThicknessMm = 14.0,
            BenchmarkSurcharge = 50.0,
            Description = "次基准常用板 (8mm ≤ t < 14mm)：中板常规规格加价"
        },
        new PlateThicknessLadder
        {
            Id = 3,
            MinThicknessMm = 14.0,
            MaxThicknessMm = 20.01,
            BenchmarkSurcharge = 0.0,
            Description = "行业黄金基价点 (14mm ≤ t ≤ 20mm)：全国钢厂大盘出厂零加价基准"
        },
        new PlateThicknessLadder
        {
            Id = 4,
            MinThicknessMm = 20.01,
            MaxThicknessMm = 40.01,
            BenchmarkSurcharge = 80.0,
            Description = "常用中厚板 (20mm < t ≤ 40mm)：重载梁柱主力规格加价"
        },
        new PlateThicknessLadder
        {
            Id = 5,
            MinThicknessMm = 40.01,
            MaxThicknessMm = 60.01,
            BenchmarkSurcharge = 180.0,
            Description = "特厚板 (40mm < t ≤ 60mm)：大压下量与心部致密性工艺加价"
        },
        new PlateThicknessLadder
        {
            Id = 6,
            MinThicknessMm = 60.01,
            MaxThicknessMm = 100.01,
            BenchmarkSurcharge = 380.0,
            Description = "超厚板 (60mm < t ≤ 100mm)：特厚坯连铸连轧、心部探伤与偏析控制加价"
        },
        new PlateThicknessLadder
        {
            Id = 7,
            MinThicknessMm = 100.01,
            MaxThicknessMm = 999.0,
            BenchmarkSurcharge = 650.0,
            Description = "极厚板 (t > 100mm)：大型模铸特大型钢锭、电渣重熔锻压轧制加价"
        }
    ];
}
