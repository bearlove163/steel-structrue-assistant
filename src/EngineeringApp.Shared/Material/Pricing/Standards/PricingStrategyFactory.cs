namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 标准策略工厂
/// </summary>
public static class PricingStrategyFactory
{
    private static readonly IStandardPricingStrategy _gb = new GbPricingStrategy();
    private static readonly IStandardPricingStrategy _en = new EnPricingStrategy();
    private static readonly IStandardPricingStrategy _astm = new AstmPricingStrategy();

    public static IStandardPricingStrategy GetStrategy(StandardSystem system) => system switch
    {
        StandardSystem.EN => _en,
        StandardSystem.ASTM => _astm,
        _ => _gb
    };
}
