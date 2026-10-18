namespace EngineeringApp.Shared.Material.Pricing;

using EngineeringApp.Shared.Standards;

/// <summary>
/// 标准策略工厂 (向后兼容委托至标准抽象工厂提供者)
/// </summary>
public static class PricingStrategyFactory
{
    public static IStandardPricingStrategy GetStrategy(StandardSystem system) =>
        EngineeringStandardFactoryProvider.GetFactory(system).CreatePricingStrategy();
}
