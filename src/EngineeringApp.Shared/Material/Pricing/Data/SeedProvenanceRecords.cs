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

        // 1. 用户关注的典型规格：Q355B 22mm 南钢 广东现场 (批次 1: 2026-03-15 询价)
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
            DisplayTitle = "Q355B 常用中厚板 t=22mm (首期询价批次)",
            Category = "Plate",
            StandardSpecification = price1.StandardSpecification,
            DimensionText = price1.DimensionText,
            SnapshotId = baseSnap.SnapshotId,
            SnapshotName = baseSnap.SnapshotName,
            EffectiveDate = baseSnap.EffectiveDate,
            RecordedAt = DateTime.Parse("2026-03-15 09:30:00"),
            InquiryBatchId = "INQ-20260315-LOT01",
            InquiryDate = DateTime.Parse("2026-03-15 09:30:00"),
            InquiryVendor = "南钢南京总厂华南直销处",
            RevisionNumber = 1,
            PricingPrincipleNote = "2026Q1出厂常规阶梯与定尺原则",
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
            PlateParameters = p1,
            RevisionLogs =
            [
                new MaterialPriceRevisionLog
                {
                    Id = 1,
                    RecordId = 1,
                    RevisionNumber = 1,
                    RevisedAt = DateTime.Parse("2026-03-15 09:30:00"),
                    RevisedBy = "投标部/李工",
                    Reason = "投标阶段首轮集中询价核定",
                    PreviousPricePerTon = 0,
                    NewPricePerTon = price1.FinalPricePerTon,
                    ChangeDetails = "首发建档入库，基价 ¥3920，厚度 +¥60，定尺 +¥60，运费 ¥200",
                    AppliedPrinciple = "2026Q1出厂常规阶梯与定尺原则"
                }
            ]
        });

        // 2. 同一工程但询价时间差异：Q355B 22mm 南钢 广东现场 (批次 2: 2026-03-25 询价，前几天询价运费微调)
        var p2 = new PlatePricingParameters
        {
            Standard = StandardSystem.GB,
            Grade = SteelGrade.Q355B,
            ThicknessMm = 22.0,
            WidthMm = 2500.0,
            LengthMm = 12000.0,
            CutType = PlateDimensionCutType.FixedDimension,
            Tolerance = ToleranceClass.ClassA,
            Flatness = FlatnessClass.Normal_N,
            SteelMillId = "NISCO",
            Delivery = DeliveryCondition.Delivered_JobSite,
            DestinationRegion = "华南-广东工程现场",
            CustomFreightPerTon = 180.0 // 询价时间不同，大批量运费协调让利20元/t
        };
        var price2 = PlatePricingRuleEngine.Calculate(p2, baseSnap);
        _records.Add(new MaterialProvenanceRecord
        {
            Id = 2,
            MaterialTag = MaterialProvenanceTagBuilder.GeneratePlateTag(p2),
            DisplayTitle = "Q355B 常用中厚板 t=22mm (二期追加批次/时间差异)",
            Category = "Plate",
            StandardSpecification = price2.StandardSpecification,
            DimensionText = price2.DimensionText,
            SnapshotId = baseSnap.SnapshotId,
            SnapshotName = baseSnap.SnapshotName,
            EffectiveDate = baseSnap.EffectiveDate,
            RecordedAt = DateTime.Parse("2026-03-25 14:15:00"),
            InquiryBatchId = "INQ-20260325-LOT02",
            InquiryDate = DateTime.Parse("2026-03-25 14:15:00"),
            InquiryVendor = "华南大宗钢材现货仓",
            RevisionNumber = 1,
            PricingPrincipleNote = "2026Q1出厂常规阶梯 (集中运输物流返利¥20/t)",
            ProjectReference = "粤港澳大湾区钢构主厂房项目",
            MillName = price2.MillName,
            DeliveryAndFreightText = price2.DeliveryAndFreightText,
            BasePricePerTon = price2.BasePricePerTon,
            MillPremiumPerTon = price2.MillPremiumPerTon,
            ThicknessSurchargePerTon = price2.ThicknessSurchargePerTon,
            DimensionSurchargePerTon = price2.DimensionSurchargePerTon,
            ToleranceSurchargePerTon = price2.ToleranceSurchargePerTon,
            PerformanceSurchargePerTon = price2.PerformanceSurchargePerTon,
            InspectionSurchargePerTon = price2.InspectionSurchargePerTon,
            FreightPerTon = 180.0,
            FinalPricePerTon = price2.FinalPricePerTon,
            FullDescription = price2.FullDescription,
            PlateParameters = p2,
            RevisionLogs =
            [
                new MaterialPriceRevisionLog
                {
                    Id = 2,
                    RecordId = 2,
                    RevisionNumber = 1,
                    RevisedAt = DateTime.Parse("2026-03-25 14:15:00"),
                    RevisedBy = "采购部/张主管",
                    Reason = "二期追加构件分批询价（运费批量协商让利）",
                    PreviousPricePerTon = 0,
                    NewPricePerTon = price2.FinalPricePerTon,
                    ChangeDetails = "首发建档入库，实际调运费协议价由 ¥200 降为 ¥180/t",
                    AppliedPrinciple = "2026Q1出厂常规阶梯 (集中运输物流返利¥20/t)"
                }
            ]
        });

        // 3. 加价原则变更案例：Q355D-Z25-TMCP t=50mm 宝钢 浙江车间 C类保全厚度 (经历 Rev 2 原则变更)
        var p3 = new PlatePricingParameters
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
            DestinationRegion = "华东-浙江制造车间",
            CustomThicknessSurcharge = 210.0 // 钢厂加价原则上调为210
        };
        var price3 = PlatePricingRuleEngine.Calculate(p3, baseSnap);
        _records.Add(new MaterialProvenanceRecord
        {
            Id = 3,
            MaterialTag = MaterialProvenanceTagBuilder.GeneratePlateTag(p3),
            DisplayTitle = "Q355D-Z25 特厚重型柱板 t=50mm (宝钢/浙江落地)",
            Category = "Plate",
            StandardSpecification = price3.StandardSpecification,
            DimensionText = price3.DimensionText,
            SnapshotId = baseSnap.SnapshotId,
            SnapshotName = baseSnap.SnapshotName,
            EffectiveDate = baseSnap.EffectiveDate,
            RecordedAt = DateTime.Parse("2026-03-15 10:15:00"),
            InquiryBatchId = "INQ-20260315-LOT01",
            InquiryDate = DateTime.Parse("2026-03-15 10:15:00"),
            InquiryVendor = "宝武钢铁股份直销部",
            RevisionNumber = 2,
            PricingPrincipleNote = "宝武特厚板加价调整公函 (厚板探伤成本附加+¥30)",
            ProjectReference = "杭州奥体中心重钢管桁架工程",
            MillName = price3.MillName,
            DeliveryAndFreightText = price3.DeliveryAndFreightText,
            BasePricePerTon = price3.BasePricePerTon,
            MillPremiumPerTon = price3.MillPremiumPerTon,
            ThicknessSurchargePerTon = 210.0,
            ContractThicknessSurchargePerTon = 180.0,
            DimensionSurchargePerTon = price3.DimensionSurchargePerTon,
            ToleranceSurchargePerTon = price3.ToleranceSurchargePerTon,
            PerformanceSurchargePerTon = price3.PerformanceSurchargePerTon,
            InspectionSurchargePerTon = price3.InspectionSurchargePerTon,
            FreightPerTon = price3.FreightPerTon,
            FinalPricePerTon = price3.FinalPricePerTon,
            FullDescription = price3.FullDescription,
            PlateParameters = p3,
            RevisionLogs =
            [
                new MaterialPriceRevisionLog
                {
                    Id = 3,
                    RecordId = 3,
                    RevisionNumber = 1,
                    RevisedAt = DateTime.Parse("2026-03-15 10:15:00"),
                    RevisedBy = "投标部/陈工",
                    Reason = "投标阶段按2026Q1常规厚板加价阶梯计价",
                    PreviousPricePerTon = 0,
                    NewPricePerTon = 5280.0,
                    ChangeDetails = "初次建档：基价 ¥3920，厚度加价基准 +¥180，总价 ¥5280/t",
                    AppliedPrinciple = "2026Q1钢厂常规阶梯加价"
                },
                new MaterialPriceRevisionLog
                {
                    Id = 4,
                    RecordId = 3,
                    RevisionNumber = 2,
                    RevisedAt = DateTime.Parse("2026-04-02 16:30:00"),
                    RevisedBy = "成本合约部/王经理",
                    Reason = "钢厂特厚板加价原则新规生效（厚度加价阶梯由+¥180调为+¥210）",
                    PreviousPricePerTon = 5280.0,
                    NewPricePerTon = 5310.0,
                    ChangeDetails = "厚度加价变更: ¥180 -> ¥210 (+¥30/t); 综合单价: ¥5280 -> ¥5310 (+¥30/t)",
                    AppliedPrinciple = "宝武特厚板加价调整公函 (厚板探伤成本附加+¥30)"
                }
            ]
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
