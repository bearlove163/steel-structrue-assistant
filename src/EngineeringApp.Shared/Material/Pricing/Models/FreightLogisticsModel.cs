namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 采购交货状态与贸易条款
/// </summary>
public enum DeliveryCondition
{
    /// <summary>钢厂出厂车板自提价 (不含干线运费)</summary>
    ExWorks_Mill,

    /// <summary>送抵钢构件制造车间落地价 (含干线调运与工厂卸车下力费)</summary>
    Delivered_FabricationPlant,

    /// <summary>送抵工程项目施工现场直达价 (含现场通行及下沉短驳费)</summary>
    Delivered_JobSite,

    /// <summary>出口集港船上交货离岸价 (FOB 中国主要港口，含短驳/港杂/海运绑扎)</summary>
    FOB_ChinesePort
}

public static class DeliveryConditionExtensions
{
    public static string GetDisplayName(this DeliveryCondition condition) => condition switch
    {
        DeliveryCondition.ExWorks_Mill => "钢厂出厂车板价 (自提)",
        DeliveryCondition.Delivered_FabricationPlant => "送抵制造车间落地价 (含调运)",
        DeliveryCondition.Delivered_JobSite => "送抵施工现场直达价 (含现场短驳)",
        DeliveryCondition.FOB_ChinesePort => "出口港口集港价 (FOB 中国港)",
        _ => condition.ToString()
    };
}

/// <summary>
/// 产地至需求地干线调运费费率记录
/// </summary>
public class FreightRouteRate
{
    public string RouteId { get; set; } = "";
    public string OriginMillBase { get; set; } = "上海宝山基地";
    public string DestinationRegion { get; set; } = "华东-浙江制造车间";
    public double EstimatedFreightPerTon { get; set; } = 50.0;
    public string TransportMode { get; set; } = "公路短途板车";
    public string Description { get; set; } = "";
}
