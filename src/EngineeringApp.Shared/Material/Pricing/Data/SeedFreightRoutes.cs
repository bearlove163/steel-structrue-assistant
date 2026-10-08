namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 预置典型钢材物流调运费率库
/// </summary>
public static class SeedFreightRoutes
{
    private static readonly List<FreightRouteRate> _routes =
    [
        new FreightRouteRate
        {
            RouteId = "Baosteel-Zhejiang",
            OriginMillBase = "上海宝山基地",
            DestinationRegion = "华东-浙江制造车间",
            EstimatedFreightPerTon = 50.0,
            TransportMode = "公路短途板车 (150~220km)",
            Description = "上海宝山至杭州/绍兴/宁波钢构件加工厂短途调运"
        },
        new FreightRouteRate
        {
            RouteId = "Baosteel-Guangdong",
            OriginMillBase = "上海宝山基地",
            DestinationRegion = "华南-广东工程现场",
            EstimatedFreightPerTon = 200.0,
            TransportMode = "长途大型挂车 / 沿海集装箱海运",
            Description = "上海至广东珠三角/深圳大湾区超长途跨省调运"
        },
        new FreightRouteRate
        {
            RouteId = "Baosteel-FOB",
            OriginMillBase = "上海宝山基地",
            DestinationRegion = "上海港国际集港码头 (FOB)",
            EstimatedFreightPerTon = 220.0,
            TransportMode = "港区重载短驳 + 港杂理货 + 国际海运绑扎",
            Description = "海外出口工程 FOB 离岸集港装船"
        },
        new FreightRouteRate
        {
            RouteId = "Shagang-Zhejiang",
            OriginMillBase = "江苏张家港基地",
            DestinationRegion = "华东-浙江制造车间",
            EstimatedFreightPerTon = 60.0,
            TransportMode = "苏浙水运驳船 / 公路板车",
            Description = "沙钢张家港至浙江各大制造车间常规短途"
        },
        new FreightRouteRate
        {
            RouteId = "Shagang-Guangdong",
            OriginMillBase = "江苏张家港基地",
            DestinationRegion = "华南-广东工程现场",
            EstimatedFreightPerTon = 190.0,
            TransportMode = "长江口散货海船南下",
            Description = "沙钢普板长途南下广东工程现场"
        },
        new FreightRouteRate
        {
            RouteId = "Jinxi-Zhejiang",
            OriginMillBase = "河北唐山迁西基地",
            DestinationRegion = "华东-浙江制造车间",
            EstimatedFreightPerTon = 140.0,
            TransportMode = "北方沿海散货船南下 + 码头分拨汽运",
            Description = "津西热轧型钢北方南下华东集配车间"
        },
        new FreightRouteRate
        {
            RouteId = "Jinxi-North",
            OriginMillBase = "河北唐山迁西基地",
            DestinationRegion = "京津冀鲁本地项目",
            EstimatedFreightPerTon = 50.0,
            TransportMode = "公路直达运输",
            Description = "津西至环渤海与京津冀本地工程"
        },
        new FreightRouteRate
        {
            RouteId = "NISCO-Zhejiang",
            OriginMillBase = "江苏南京基地",
            DestinationRegion = "华东-浙江制造车间",
            EstimatedFreightPerTon = 70.0,
            TransportMode = "苏浙公路重载直发",
            Description = "南钢特厚中厚板至浙江制造车间"
        }
    ];

    public static List<FreightRouteRate> AllRoutes => _routes;

    public static double MatchFreight(string millId, DeliveryCondition delivery, string destination)
    {
        if (delivery == DeliveryCondition.ExWorks_Mill)
        {
            return 0.0; // 钢厂出厂自提车板价无调运费
        }

        if (delivery == DeliveryCondition.FOB_ChinesePort)
        {
            return 220.0; // 默认出口集港港杂调运
        }

        // 精确匹配目的地关键字
        if (destination.Contains("广东") || destination.Contains("深圳") || destination.Contains("广州") || destination.Contains("华南"))
        {
            return string.Equals(millId, "Shagang", StringComparison.OrdinalIgnoreCase) ? 190.0 : 200.0;
        }

        if (destination.Contains("浙江") || destination.Contains("宁波") || destination.Contains("杭州") || destination.Contains("绍兴") || destination.Contains("华东"))
        {
            if (string.Equals(millId, "Jinxi", StringComparison.OrdinalIgnoreCase))
            {
                return 140.0; // 北方型钢南下
            }
            if (string.Equals(millId, "Shagang", StringComparison.OrdinalIgnoreCase))
            {
                return 60.0; // 沙钢至浙江
            }
            if (string.Equals(millId, "NISCO", StringComparison.OrdinalIgnoreCase))
            {
                return 70.0; // 南钢至浙江
            }
            return 50.0; // 宝钢短途
        }

        if (destination.Contains("北方") || destination.Contains("京津冀") || destination.Contains("北京"))
        {
            if (string.Equals(millId, "Jinxi", StringComparison.OrdinalIgnoreCase))
            {
                return 50.0;
            }
            return 150.0;
        }

        return 60.0; // 默认常规区域内调运
    }
}
