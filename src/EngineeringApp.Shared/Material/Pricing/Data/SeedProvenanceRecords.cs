namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 预置典型工程物料出处记录库 (Seed Provenance Records)
/// </summary>
public static class SeedProvenanceRecords
{
    private static readonly List<MaterialProvenanceRecord> _records = [];

    static SeedProvenanceRecords()
    {
        var baseSnap = SeedPriceSnapshots.GetById("SNAP-20260315-BASE");

        // 1. 用户关注的典型规格：Q355B 22mm 南钢 广东现场
        var p1 = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            ThicknessMm = 22.0,
            WidthMm = 2500.0,
            LengthMm = 12000.0,
            CutType = PlateDimensionCutType.FixedDimension,
            Tolerance = ToleranceClass.ClassA,
            Flatness = FlatnessClass.Normal_N,
            SteelMillId = "NISCO", // 南钢
            Delivery = DeliveryCondition.Delivered_JobSite,
            DestinationRegion = "华南-广东工程现场"
        };
        var price1 = PlatePricingRuleEngine.Calculate(p1, baseSnap);
        _records.Add(new MaterialProvenanceRecord
        {
            Id = 1,
            MaterialTag = MaterialProvenanceTagBuilder.GeneratePlateTag(p1),
            DisplayTitle = "Q355B 常用中厚板 t=22mm (南钢/广东现场)",
            Category = "Plate",
            StandardSpecification = price1.StandardSpecification,
            DimensionText = price1.DimensionText,
            SnapshotId = baseSnap.SnapshotId,
            SnapshotName = baseSnap.SnapshotName,
            EffectiveDate = baseSnap.EffectiveDate,
            RecordedAt = DateTime.Parse("2026-03-15 09:30:00"),
            ProjectReference = "粤港澳大湾区钢构主厂房项目",
            MillName = price1.MillName,
            DeliveryAndFreightText = price1.DeliveryAndFreightText,
            BasePricePerTon = price1.BasePricePerTon,
            MillPremiumPerTon = price1.MillPremiumPerTon,
            ThicknessSurchargePerTon = price1.ThicknessSurchargePerTon,
            DimensionSurchargePerTon = price1.DimensionSurchargePerTon,
            ToleranceSurchargePerTon = price1.ToleranceSurchargePerTon,
            PerformanceSurchargePerTon = price1.PerformanceSurchargePerTon,
            InspectionSurchargePerTon = price1.InspectionSurchargePerTon,
            FreightPerTon = price1.FreightPerTon,
            FinalPricePerTon = price1.FinalPricePerTon,
            FullDescription = price1.FullDescription,
            PlateParameters = p1
        });

        // 2. 重型柱翼缘板：Q355D-Z25-TMCP t=50mm 宝钢 浙江车间 C类保全厚度
        var p2 = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355D,
            ThicknessMm = 50.0,
            WidthMm = 2500.0,
            LengthMm = 12000.0,
            CutType = PlateDimensionCutType.FixedDimension,
            Tolerance = ToleranceClass.ClassC,
            Flatness = FlatnessClass.Normal_N,
            Impact = ImpactGrade.GradeD,
            ZDirection = ZDirectionQuality.Z25,
            Metallurgy = MetallurgyProcess.TMCP,
            UT = UltrasonicInspection.ClassI,
            SteelMillId = "Baosteel",
            Delivery = DeliveryCondition.Delivered_FabricationPlant,
            DestinationRegion = "华东-浙江制造车间"
        };
        var price2 = PlatePricingRuleEngine.Calculate(p2, baseSnap);
        _records.Add(new MaterialProvenanceRecord
        {
            Id = 2,
            MaterialTag = MaterialProvenanceTagBuilder.GeneratePlateTag(p2),
            DisplayTitle = "Q355D-Z25 特厚重型柱板 t=50mm (宝钢/浙江落地)",
            Category = "Plate",
            StandardSpecification = price2.StandardSpecification,
            DimensionText = price2.DimensionText,
            SnapshotId = baseSnap.SnapshotId,
            SnapshotName = baseSnap.SnapshotName,
            EffectiveDate = baseSnap.EffectiveDate,
            RecordedAt = DateTime.Parse("2026-03-15 10:15:00"),
            ProjectReference = "杭州奥体中心重钢管桁架工程",
            MillName = price2.MillName,
            DeliveryAndFreightText = price2.DeliveryAndFreightText,
            BasePricePerTon = price2.BasePricePerTon,
            MillPremiumPerTon = price2.MillPremiumPerTon,
            ThicknessSurchargePerTon = price2.ThicknessSurchargePerTon,
            DimensionSurchargePerTon = price2.DimensionSurchargePerTon,
            ToleranceSurchargePerTon = price2.ToleranceSurchargePerTon,
            PerformanceSurchargePerTon = price2.PerformanceSurchargePerTon,
            InspectionSurchargePerTon = price2.InspectionSurchargePerTon,
            FreightPerTon = price2.FreightPerTon,
            FinalPricePerTon = price2.FinalPricePerTon,
            FullDescription = price2.FullDescription,
            PlateParameters = p2
        });
    }

    public static List<MaterialProvenanceRecord> AllRecords => _records;

    public static void AddRecord(MaterialProvenanceRecord record)
    {
        if (record.Id <= 0)
        {
            record.Id = _records.Count > 0 ? _records.Max(r => r.Id) + 1 : 1;
        }
        _records.Insert(0, record);
    }
}
