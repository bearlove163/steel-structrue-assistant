using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Standards;

/// <summary>
/// 国际化工程标准抽象工厂注册与提供者
/// 支持一键获取或遍历已注册的多国标准工程工厂
/// </summary>
public static class EngineeringStandardFactoryProvider
{
    private static readonly Dictionary<StandardSystem, IEngineeringStandardFactory> _factories = new()
    {
        [StandardSystem.GB] = new GbEngineeringStandardFactory(),
        [StandardSystem.EN] = new EnEngineeringStandardFactory(),
        [StandardSystem.ASTM] = new AstmEngineeringStandardFactory()
    };

    /// <summary>
    /// 获取指定体系的工程标准抽象工厂，默认回退至中国国标 GB
    /// </summary>
    public static IEngineeringStandardFactory GetFactory(StandardSystem system) =>
        _factories.TryGetValue(system, out var factory) ? factory : _factories[StandardSystem.GB];

    /// <summary>
    /// 获取所有支持的工程标准体系工厂
    /// </summary>
    public static IReadOnlyCollection<IEngineeringStandardFactory> AllFactories => _factories.Values;
}
