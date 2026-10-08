using EngineeringApp.Shared.CostEstimation;
using EngineeringApp.Shared.Material;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Pricing;
using EngineeringApp.Shared.Quota;
using EngineeringApp.Shared.QuotationExport;
using Xunit;

namespace EngineeringApp.Tests;

/// <summary>
/// 针对解耦后的材料、定额、构件BOM组价、成本测算与商务报价全链路自动化验证
/// </summary>
public class QuotaAndCostEstimationTests
{
    [Fact]
    public void TestMaterialDatabase_ThicknessSurcharge_IsAccurate()
    {
        var q355 = SteelMaterialDatabase.Get(SteelGrade.Q355B);
        Assert.Equal(4100.0, q355.BasePricePerTon);

        // 常规薄板/中板 (t <= 40mm) 无厚板加价
        double normalPrice = q355.CalculateProcurementPricePerTon(25.0);
        Assert.Equal(4100.0, normalPrice);

        // 厚板 (40mm < t <= 60mm) 触发第一档加价 (+150 元/t)
        double thickPrice = q355.CalculateProcurementPricePerTon(50.0);
        Assert.Equal(4100.0 + 150.0, thickPrice);

        // 特厚板 (t > 60mm) 触发特厚板加价
        double extraThickPrice = q355.CalculateProcurementPricePerTon(70.0);
        Assert.True(extraThickPrice > thickPrice);
    }

    [Fact]
    public void TestFabricationQuotaDatabase_OrthogonalPrices_DifferByRole()
    {
        // 关键正交原则验证：同一截面 (RHS箱型)，钢柱与钢梁加工费截然不同
        var boxColumnQuota = FabricationQuotaDatabase.Query(SectionType.RHS, MemberRole.Column);
        var boxBeamQuota = FabricationQuotaDatabase.Query(SectionType.RHS, MemberRole.Beam);

        Assert.Equal(2100.0, boxColumnQuota.BaseFabricationUnitPricePerTon);
        Assert.True(boxColumnQuota.IncludesInternalDiaphragms);

        Assert.Equal(1700.0, boxBeamQuota.BaseFabricationUnitPricePerTon);
        Assert.False(boxBeamQuota.IncludesInternalDiaphragms);

        // 圆管桁架相贯腹杆 vs 普通圆管支撑
        var chsWeb = FabricationQuotaDatabase.Query(SectionType.CHS, MemberRole.TrussWeb);
        var chsBracing = FabricationQuotaDatabase.Query(SectionType.CHS, MemberRole.Bracing);

        Assert.Equal(2300.0, chsWeb.BaseFabricationUnitPricePerTon);
        Assert.True(chsWeb.IncludesIntersectingCuts);
        Assert.Equal(1400.0, chsBracing.BaseFabricationUnitPricePerTon);
    }

    [Fact]
    public void TestSteelAssemblyItem_And_MemberPricing_FullCalculations()
    {
        // 构造一个典型的 GZ1 焊接箱型柱装配件
        // 主肢: 800x800x25, 长度 6m, 数量 2根
        // 包含: 4块 30mm 内隔板, 2个 H型钢牛腿
        var gz1 = new SteelAssemblyItem
        {
            AssemblyMark = "GZ1",
            Role = MemberRole.Column,
            MainSectionType = SectionType.RHS,
            Quantity = 2,
            OverallLengthMeter = 6.0
        };

        // 1. 添加主肢零件
        gz1.Parts.Add(new SteelPartItem
        {
            PartMark = "m1",
            PartName = "箱型主肢",
            Role = PartFunctionalRole.MainProfile,
            MaterialGrade = SteelGrade.Q355B,
            SectionType = SectionType.RHS,
            ThicknessMm = 25.0,
            LengthMeter = 6.0,
            NetWeightKg = 3600.0, // 单根净重 3.6t
            NetSurfaceAreaM2 = 19.2,
            QuantityPerAssembly = 1,
            PartLossRate = 0.05
        });

        // 2. 添加 4 块内隔板
        gz1.Parts.Add(new SteelPartItem
        {
            PartMark = "dp1",
            PartName = "电渣焊内隔板",
            Role = PartFunctionalRole.Diaphragm,
            MaterialGrade = SteelGrade.Q355B,
            SectionType = SectionType.Rectangle,
            ThicknessMm = 30.0,
            LengthMeter = 0.8,
            NetWeightKg = 140.0, // 每块 140kg
            NetSurfaceAreaM2 = 1.2,
            QuantityPerAssembly = 4,
            PartLossRate = 0.08
        });

        // 3. 添加 2 个牛腿梁段
        gz1.Parts.Add(new SteelPartItem
        {
            PartMark = "br1",
            PartName = "外伸悬臂牛腿",
            Role = PartFunctionalRole.BracketBeam,
            MaterialGrade = SteelGrade.Q355B,
            SectionType = SectionType.HBeam,
            ThicknessMm = 16.0,
            LengthMeter = 0.6,
            NetWeightKg = 120.0,
            NetSurfaceAreaM2 = 1.8,
            QuantityPerAssembly = 2,
            PartLossRate = 0.05
        });

        // 验证物理汇总
        // 单根净重 = 3600 + 140*4 + 120*2 = 3600 + 560 + 240 = 4400 kg = 4.4t
        Assert.Equal(4400.0, gz1.SingleAssemblyNetWeightKg);
        Assert.Equal(8.8, gz1.TotalBatchNetWeightTon, precision: 2);
        Assert.True(gz1.TotalBatchGrossWeightTon > gz1.TotalBatchNetWeightTon);
        Assert.Equal(2, gz1.CantileverBracketCount);
        Assert.Equal(30.0, gz1.MaxThicknessMm);

        // 执行构件级直接费组价
        var pricingResult = MemberPricingCalculator.Calculate(gz1, coatingUnitPricePerM2: 50.0);

        Assert.Equal("GZ1", pricingResult.AssemblyMark);
        Assert.Equal(8.8, pricingResult.TotalNetWeightTon, precision: 2);
        Assert.True(pricingResult.MaterialProcurementCost > 0);
        Assert.True(pricingResult.FabricationCost > 0);
        Assert.True(pricingResult.CoatingCost > 0);
        Assert.True(pricingResult.TotalDirectCost > 0);
        Assert.True(pricingResult.DirectUnitPricePerTon > 5000.0); // 钢结构直接工程费单价一般在 5500~7500 元/t
    }

    [Fact]
    public void TestEndToEnd_CostEstimation_And_QuotationExport()
    {
        // 模拟工程项目构件清单：包含 1 批框架柱 (GZ1) 与 1 批主框架梁 (GL1)
        var columnAssembly = new SteelAssemblyItem
        {
            AssemblyMark = "GZ1",
            Role = MemberRole.Column,
            MainSectionType = SectionType.RHS,
            Quantity = 4,
            OverallLengthMeter = 6.0
        };
        columnAssembly.Parts.Add(new SteelPartItem
        {
            PartName = "箱型柱主肢",
            Role = PartFunctionalRole.MainProfile,
            MaterialGrade = SteelGrade.Q355B,
            ThicknessMm = 28.0,
            NetWeightKg = 2500.0,
            NetSurfaceAreaM2 = 15.0,
            QuantityPerAssembly = 1,
            PartLossRate = 0.05
        });

        var beamAssembly = new SteelAssemblyItem
        {
            AssemblyMark = "GL1",
            Role = MemberRole.Beam,
            MainSectionType = SectionType.HBeam,
            Quantity = 8,
            OverallLengthMeter = 9.0
        };
        beamAssembly.Parts.Add(new SteelPartItem
        {
            PartName = "H型钢主梁",
            Role = PartFunctionalRole.MainProfile,
            MaterialGrade = SteelGrade.Q355B,
            ThicknessMm = 16.0,
            NetWeightKg = 1200.0,
            NetSurfaceAreaM2 = 12.0,
            QuantityPerAssembly = 1,
            PartLossRate = 0.04
        });

        var memberPricingList = new List<MemberPricingResult>
        {
            MemberPricingCalculator.Calculate(columnAssembly, coatingUnitPricePerM2: 45.0),
            MemberPricingCalculator.Calculate(beamAssembly, coatingUnitPricePerM2: 45.0)
        };

        // 柱净重 = 4 * 2.5t = 10.0t
        // 梁净重 = 8 * 1.2t = 9.6t
        // 总净重 = 19.6t
        Assert.Equal(19.6, memberPricingList.Sum(m => m.TotalNetWeightTon), precision: 2);

        // 套入公司固定格式成本测算表
        var options = CompanyCostTemplate.SubcontractWithErection("某工业园区钢结构厂房一期工程");
        options.ClientName = "XX投资建设有限公司";

        var costSheet = CostEstimationEngine.Calculate(memberPricingList, options);

        // 验证成本测算表财务逻辑
        Assert.Equal("某工业园区钢结构厂房一期工程", costSheet.ProjectName);
        Assert.Equal(19.6, costSheet.TotalSteelNetWeightTon, precision: 2);
        Assert.True(costSheet.MaterialProcurementCost > 0);
        Assert.True(costSheet.WorkshopFabricationCost > 0);
        Assert.True(costSheet.SurfaceCoatingCost > 0);
        Assert.True(costSheet.TransportationCost > 0);
        Assert.True(costSheet.SiteErectionCost > 0);
        Assert.True(costSheet.TotalDirectCost > 0);

        // 措施费与技术服务
        Assert.True(costSheet.DetailingBimFee > 0);
        Assert.True(costSheet.InspectionTestingFee > 0);
        Assert.Equal(50000.0, costSheet.TemporarySupportFee);

        // 公司管理费与垫资成本
        Assert.True(costSheet.OverheadCost > 0);
        Assert.True(costSheet.FinancingCost > 0);

        // 保本底价与最终对外报价
        Assert.True(costSheet.FactoryCostFloor > costSheet.TotalDirectCost);
        Assert.True(costSheet.FinalBidQuotation > costSheet.FactoryCostFloor);
        Assert.True(costSheet.FinalUnitPricePerTon > costSheet.CostFloorPerTon);

        // 输出对外商业报价清单
        var bidSummary = QuotationReportGenerator.Generate(costSheet, memberPricingList);

        Assert.Equal(costSheet.ProjectName, bidSummary.ProjectName);
        Assert.Equal(2, bidSummary.Items.Count); // 包含 010605 钢柱与 010606 钢梁两个清单大类
        Assert.Equal(19.6, bidSummary.TotalNetWeightTon, precision: 2);
        Assert.Equal(costSheet.FinalBidQuotation, bidSummary.TotalQuotationAmount, precision: 1);
        Assert.Equal(costSheet.FinalUnitPricePerTon, bidSummary.AverageUnitPricePerTon, precision: 1);
    }
}
