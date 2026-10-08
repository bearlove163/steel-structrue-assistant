namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 预置多源材料价格时间快照数据 (支持时间序列与调差比对)
/// </summary>
public static class SeedPriceSnapshots
{
    private static readonly List<MaterialPriceSnapshot> _snapshots =
    [
        new MaterialPriceSnapshot
        {
            SnapshotId = "SNAP-20261008-LATEST",
            SnapshotName = "2026年10月上旬-大盘最新网价 (当前执行)",
            EffectiveDate = new DateTime(2026, 10, 8),
            SourceType = PriceSourceType.MySteelWebIndex,
            SourceSupplier = "上海我的钢铁网 MySteel 现货大盘",
            IsTaxInclusive = true,
            DefaultDelivery = DeliveryCondition.Delivered_FabricationPlant,
            PlateBaseQ235B = 3850.0,
            PlateBaseQ355B = 4050.0,
            HBeamBaseQ235B = 3950.0,
            HBeamBaseQ355B = 4150.0,
            HotRolledCoilBase = 3900.0,
            WeldedTubeBase = 4250.0,
            SeamlessTubeBase = 5250.0,
            IsLocked = false,
            Remarks = "秋季施工旺季大盘平稳微涨，华东普板报价 4050 元/吨"
        },
        new MaterialPriceSnapshot
        {
            SnapshotId = "SNAP-20260315-BASE",
            SnapshotName = "2026年03月中旬-投标定标锁价基期 (基准快照)",
            EffectiveDate = new DateTime(2026, 3, 15),
            SourceType = PriceSourceType.InternalCorporateRate,
            SourceSupplier = "工程项目投标标底与定额信息价",
            IsTaxInclusive = true,
            DefaultDelivery = DeliveryCondition.Delivered_FabricationPlant,
            PlateBaseQ235B = 3720.0,
            PlateBaseQ355B = 3920.0,
            HBeamBaseQ235B = 3850.0,
            HBeamBaseQ355B = 4050.0,
            HotRolledCoilBase = 3780.0,
            WeldedTubeBase = 4120.0,
            SeamlessTubeBase = 5050.0,
            IsLocked = true,
            Remarks = "项目合同签订基准调差日，已冻结锁定，用于竣工调差"
        },
        new MaterialPriceSnapshot
        {
            SnapshotId = "SNAP-20260701-SUMMER",
            SnapshotName = "2026年07月上旬-钢厂直发年中调价令",
            EffectiveDate = new DateTime(2026, 7, 1),
            SourceType = PriceSourceType.SteelMillDirect,
            SourceSupplier = "宝武钢铁与津西钢铁联合调价公函",
            IsTaxInclusive = true,
            DefaultDelivery = DeliveryCondition.ExWorks_Mill,
            PlateBaseQ235B = 3950.0,
            PlateBaseQ355B = 4150.0,
            HBeamBaseQ235B = 4020.0,
            HBeamBaseQ355B = 4250.0,
            HotRolledCoilBase = 4000.0,
            WeldedTubeBase = 4320.0,
            SeamlessTubeBase = 5380.0,
            IsLocked = false,
            Remarks = "原料铁矿石与焦炭上涨推动的钢厂季度指导价"
        }
    ];

    public static List<MaterialPriceSnapshot> AllSnapshots => _snapshots;

    public static MaterialPriceSnapshot Latest => _snapshots[0];

    public static MaterialPriceSnapshot GetById(string id) =>
        _snapshots.FirstOrDefault(s => string.Equals(s.SnapshotId, id, StringComparison.OrdinalIgnoreCase)) ?? Latest;
}
