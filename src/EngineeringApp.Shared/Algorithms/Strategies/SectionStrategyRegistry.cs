using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms.Strategies;

/// <summary>
/// 截面几何力学计算策略注册表 (Registry Pattern)
/// 遵循开闭原则 (OCP)，支持无侵入式注册新型截面计算策略或组合构件
/// </summary>
public static class SectionStrategyRegistry
{
    private static readonly Dictionary<SectionType, ISectionGeometryStrategy> _strategies = new();

    static SectionStrategyRegistry()
    {
        // 注册全套11种开箱即用的标准几何力学计算策略
        RegisterStrategy(new RectangleSectionStrategy());
        RegisterStrategy(new PlateSectionStrategy());
        RegisterStrategy(new CircleSectionStrategy());
        RegisterStrategy(new ChsSectionStrategy());
        RegisterStrategy(new RhsSectionStrategy());
        RegisterStrategy(new HBeamSectionStrategy());
        RegisterStrategy(new ChannelSectionStrategy());
        RegisterStrategy(new AngleSectionStrategy());
        RegisterStrategy(new TSectionStrategy());
        RegisterStrategy(new CruciformSectionStrategy());
        RegisterStrategy(new PolygonSectionStrategy());
    }

    /// <summary>注册或覆盖特定截面类型的计算策略</summary>
    public static void RegisterStrategy(ISectionGeometryStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        _strategies[strategy.SupportedType] = strategy;
    }

    /// <summary>获取已注册策略</summary>
    public static ISectionGeometryStrategy GetStrategy(SectionType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }
        return _strategies[SectionType.Rectangle];
    }

    /// <summary>根据截面类型分发执行计算策略</summary>
    public static void ExecuteCalculation(SectionParameters parameters, SectionPropertiesResult result, MaterialProfile material)
    {
        var strategy = GetStrategy(parameters.Type);
        strategy.Calculate(parameters, result, material);
    }
}
