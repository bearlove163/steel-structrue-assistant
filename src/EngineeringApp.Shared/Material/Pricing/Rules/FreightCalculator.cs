namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 智能物流调运费测算引擎
/// </summary>
public static class FreightCalculator
{
    /// <summary>
    /// 根据钢厂产地、交货状态与需求地智能解算吨钢调运费 (元/吨)
    /// </summary>
    public static double Calculate(string millId, DeliveryCondition delivery, string destinationRegion)
    {
        return SeedFreightRoutes.MatchFreight(millId, delivery, destinationRegion);
    }
}
