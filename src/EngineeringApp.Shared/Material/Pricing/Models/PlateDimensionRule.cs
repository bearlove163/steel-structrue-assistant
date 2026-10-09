namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 钢板定尺与尺寸超限加价规则模型 (可由用户自定义配置并持久化至数据库)
/// </summary>
public class PlateDimensionRule
{
    public int Id { get; set; }

    /// <summary>规则唯一标识代号</summary>
    public string RuleCode { get; set; } = "";

    /// <summary>规则名称</summary>
    public string Name { get; set; } = "";

    /// <summary>行业参考基准加价 (元/吨)</summary>
    public double BenchmarkSurcharge { get; set; }

    /// <summary>用户自定义加价 (元/吨，若为 null 则采用参考基准)</summary>
    public double? CustomSurcharge { get; set; }

    /// <summary>规则触发条件说明</summary>
    public string ConditionDescription { get; set; } = "";

    /// <summary>生效加价 (元/吨)</summary>
    public double EffectiveSurcharge => CustomSurcharge ?? BenchmarkSurcharge;

    /// <summary>是否已被用户自定义设置</summary>
    public bool IsCustomized => CustomSurcharge.HasValue;

    /// <summary>
    /// 获取全国钢铁行业公认的定尺与超宽超长基准规则库
    /// </summary>
    public static List<PlateDimensionRule> GetDefaultRules() =>
    [
        new PlateDimensionRule
        {
            Id = 1,
            RuleCode = "FixedCut",
            Name = "定宽定尺加价",
            BenchmarkSurcharge = 60.0,
            ConditionDescription = "常规工程开平板定宽定尺锯切剪切费 (无散尺损耗)"
        },
        new PlateDimensionRule
        {
            Id = 2,
            RuleCode = "SmallCut",
            Name = "小定尺精密下料",
            BenchmarkSurcharge = 100.0,
            ConditionDescription = "单张板长 < 4000mm 剪切刀次翻倍与余料边角损耗补偿"
        },
        new PlateDimensionRule
        {
            Id = 3,
            RuleCode = "SuperWide_2800",
            Name = "特宽板 (2800mm < 宽 ≤ 3200mm)",
            BenchmarkSurcharge = 160.0,
            ConditionDescription = "超越常规 2500mm 板宽，需特大宽厚板轧机专属轧制"
        },
        new PlateDimensionRule
        {
            Id = 4,
            RuleCode = "SuperWide_3200",
            Name = "超宽板 (板宽 > 3200mm)",
            BenchmarkSurcharge = 280.0,
            ConditionDescription = "国内极少数 5m/5.5m 极宽轧机专轧，超限板宽超宽运输"
        },
        new PlateDimensionRule
        {
            Id = 5,
            RuleCode = "SuperLong_15m",
            Name = "超长板 (板长 > 15000mm)",
            BenchmarkSurcharge = 180.0,
            ConditionDescription = "单张超长 15m 冷却平直度控制与大件公路超限运输护航"
        }
    ];
}
