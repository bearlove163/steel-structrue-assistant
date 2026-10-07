using System.Collections.Concurrent;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面算子注册中心与工厂（支持动态扩展与向后兼容）
/// 商务软件可直接通过此注册中心获取当前类库支持的所有截面清单，并实现新截面热插拔。
/// </summary>
public static class SectionRegistry
{
    private static readonly ConcurrentDictionary<string, ISectionProfile> _profilesByKey = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<SectionType, ISectionProfile> _profilesByType = new();

    static SectionRegistry()
    {
        // 注册现有内置算子桥接
        var defaultCalc = new ParametricSectionCalculator();
        RegisterBuiltIn(SectionType.Rectangle, "Rectangle", "矩形截面", "实心截面", defaultCalc);
        RegisterBuiltIn(SectionType.Circle, "Circle", "实心圆", "实心截面", defaultCalc);
        RegisterBuiltIn(SectionType.CHS, "CHS", "空心圆管 (CHS)", "闭口管材", defaultCalc);
        RegisterBuiltIn(SectionType.RHS, "RHS", "矩形方管 / 箱型 (RHS)", "闭口管材", defaultCalc);
        RegisterBuiltIn(SectionType.HBeam, "HBeam", "H型钢 / 工字钢", "热轧型钢", defaultCalc);
        RegisterBuiltIn(SectionType.Channel, "Channel", "槽钢 (Channel)", "热轧型钢", defaultCalc);
        RegisterBuiltIn(SectionType.Angle, "Angle", "角钢 (Angle/L)", "热轧型钢", defaultCalc);
        RegisterBuiltIn(SectionType.TSection, "TSection", "T型钢", "型钢", defaultCalc);
        RegisterBuiltIn(SectionType.Cruciform, "Cruciform", "十字形截面", "组合截面", defaultCalc);
        RegisterBuiltIn(SectionType.Polygon, "Polygon", "任意多边形", "自定义几何", defaultCalc);
    }

    /// <summary>
    /// 注册一个截面算子（支持覆盖或追加新算子）
    /// </summary>
    public static void Register(ISectionProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _profilesByKey[profile.TypeKey] = profile;
        if (profile.SectionType.HasValue)
        {
            _profilesByType[profile.SectionType.Value] = profile;
        }
    }

    /// <summary>
    /// 获取当前已注册的所有截面算子
    /// </summary>
    public static IReadOnlyCollection<ISectionProfile> GetAllProfiles() => _profilesByKey.Values.ToList();

    /// <summary>
    /// 根据关键字获取截面算子
    /// </summary>
    public static ISectionProfile? GetByKey(string typeKey)
    {
        if (string.IsNullOrWhiteSpace(typeKey)) return null;
        _profilesByKey.TryGetValue(typeKey, out var profile);
        return profile;
    }

    /// <summary>
    /// 根据 SectionType 枚举获取截面算子
    /// </summary>
    public static ISectionProfile? GetByType(SectionType type)
    {
        _profilesByType.TryGetValue(type, out var profile);
        return profile;
    }

    private static void RegisterBuiltIn(SectionType type, string key, string name, string category, ISectionCalculator calc)
    {
        var profile = new BuiltInProfileAdapter(type, key, name, category, calc);
        Register(profile);
    }

    private sealed class BuiltInProfileAdapter : ISectionProfile
    {
        private readonly ISectionCalculator _calculator;

        public BuiltInProfileAdapter(SectionType type, string key, string displayName, string category, ISectionCalculator calculator)
        {
            SectionType = type;
            TypeKey = key;
            DisplayName = displayName;
            Category = category;
            _calculator = calculator;
        }

        public string TypeKey { get; }
        public string DisplayName { get; }
        public string Category { get; }
        public SectionType? SectionType { get; }

        public SectionPropertiesResult Calculate(SectionParameters parameters)
        {
            parameters.Type = SectionType ?? parameters.Type;
            return _calculator.Calculate(parameters);
        }
    }
}
