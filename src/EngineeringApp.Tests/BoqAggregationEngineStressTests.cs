using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    /// <summary>
    /// 对 BoqAggregationEngine.cs 的对抗性压力测试与边界极端值验证
    /// 覆盖 6 大核心规范：
    /// 1. 1.15 损耗系数毛重与材料费卷积
    /// 2. K_Thick 板厚加工系数边界跃迁 (20, 20.1, 30, 40, 60, 65mm)
    /// 3. K_Elev (20, 35, 40m)、K_SiteWeld 及 4 档起重机吨位 K_Weight (<5t, 5-15t, 15-25t, >25t)
    /// 4. 采办经营创效 (剪刀差利润) 正负/零值及跨层卷积
    /// 5. 双层费率策略：IsCustomRateEnabled = true 覆盖 vs false 继承
    /// 6. 极限空集合、0 工程量、null 项目防御性容错
    /// </summary>
    public class BoqAggregationEngineStressTests
    {
        #region 1. 1.15 采购损耗系数 (Purchasing Waste Multiplier) 压力测试

        [Fact]
        public void CalculatePart_DefaultWasteFactor_CalculatesExact1_15Multiplier()
        {
            var part = new BoqPart
            {
                PartMark = "PL-TEST-01",
                Quantity = 2,
                SingleNetWeight = 5000.0, // 5t each, total 10t
                SteelGrade = "Q355B",
                Thickness = 16.0,
                PurchasingWasteFactor = 0.0 // 未设置或0，应自动触发保底 1.15
            };

            var result = BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(10000.0, part.TotalNetWeight);
            Assert.Equal(1.15, part.PurchasingWasteFactor);
            Assert.Equal(11500.0, part.TotalGrossWeight, precision: 3);
            Assert.Equal(11500.0, result.GrossWeightKg, precision: 3);

            // 基准价格 4050, 厚度16加价0, 牌号加价0, 区域运杂80 => 4130 元/t
            Assert.Equal(4130.0, part.MaterialUnitPricePerTon);
            double expectedCost = (11500.0 / 1000.0) * 4130.0; // 11.5 * 4130 = 47,495.0
            Assert.Equal(expectedCost, part.TotalPartMaterialCost, precision: 2);
            Assert.Equal(expectedCost, result.TotalPartCost, precision: 2);
        }

        [Fact]
        public void CalculatePart_CustomWasteFactor_RespectsConfiguredValue()
        {
            var part = new BoqPart
            {
                PartMark = "CURVE-PL-02",
                Quantity = 1,
                SingleNetWeight = 2000.0,
                SteelGrade = "Q355B",
                Thickness = 25.0,
                PurchasingWasteFactor = 1.25 // 特殊弯曲异形构件 25% 损耗
            };

            var result = BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(2000.0, part.TotalNetWeight);
            Assert.Equal(1.25, part.PurchasingWasteFactor);
            Assert.Equal(2500.0, part.TotalGrossWeight, precision: 3);
            Assert.Equal(2500.0, result.GrossWeightKg, precision: 3);
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(-0.15)]
        [InlineData(0.0)]
        public void CalculatePart_ZeroOrNegativeWasteFactor_ResetsTo1_15Default(double invalidFactor)
        {
            var part = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 1000.0,
                PurchasingWasteFactor = invalidFactor
            };

            BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(1.15, part.PurchasingWasteFactor);
            Assert.Equal(1150.0, part.TotalGrossWeight, precision: 3);
        }

        [Fact]
        public void CalculateSubProject_RollsUp1_15GrossWeightFromParts()
        {
            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-TRUSS-01",
                Quantity = 1,
                Parts = new List<BoqPart>
                {
                    new BoqPart { Quantity = 1, SingleNetWeight = 10000.0, PurchasingWasteFactor = 1.15 },
                    new BoqPart { Quantity = 2, SingleNetWeight = 5000.0, PurchasingWasteFactor = 1.15 }
                }
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            // Net weight = 10,000 + 10,000 = 20,000 kg
            Assert.Equal(20000.0, sub.SubProjectNetWeightKg, precision: 2);
            // Gross weight = 20,000 * 1.15 = 23,000 kg
            Assert.Equal(23000.0, sub.SubProjectGrossWeightKg, precision: 2);
            Assert.Equal(20000.0, res.TotalNetWeightKg, precision: 2);
            Assert.Equal(23000.0, res.TotalGrossWeightKg, precision: 2);
        }

        [Fact]
        public void CalculateSubProject_WhenNoPartsProvided_CalculatesGrossWeightFromNetWeightWith1_15Factor()
        {
            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-NO-PARTS",
                Quantity = 1,
                SubProjectNetWeightKg = 50000.0,
                SubProjectGrossWeightKg = 0.0 // 未设置，应自动乘 1.15
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            Assert.Equal(50000.0, sub.SubProjectNetWeightKg);
            Assert.Equal(57500.0, sub.SubProjectGrossWeightKg, precision: 2);
            Assert.Equal(57500.0, res.TotalGrossWeightKg, precision: 2);
        }

        #endregion

        #region 2. K_Thick 板厚加工制作难度调节系数边界跃迁测试

        [Theory]
        [InlineData(0.0, 1.00)]
        [InlineData(10.0, 1.00)]
        [InlineData(19.999, 1.00)]
        [InlineData(20.0, 1.00)]   // 边界点 20mm 处于第一档
        [InlineData(20.001, 1.05)] // 跃迁进入第二档
        [InlineData(20.1, 1.05)]   // 20.1mm 要求验证点
        [InlineData(25.0, 1.05)]
        [InlineData(30.0, 1.05)]   // 边界点 30mm 处于第二档
        [InlineData(30.001, 1.12)] // 跃迁进入第三档
        [InlineData(35.0, 1.12)]
        [InlineData(40.0, 1.12)]   // 边界点 40mm 处于第三档
        [InlineData(40.001, 1.25)] // 跃迁进入第四档
        [InlineData(50.0, 1.25)]
        [InlineData(60.0, 1.25)]   // 边界点 60mm 处于第四档
        [InlineData(60.001, 1.40)] // 跃迁进入第五档 (特厚板)
        [InlineData(65.0, 1.40)]   // 65mm 要求验证点
        [InlineData(100.0, 1.40)]
        public void CalculateThicknessFactor_BoundaryTransitions(double thicknessMm, double expectedFactor)
        {
            double actual = BoqAggregationEngine.CalculateThicknessFactor(thicknessMm);
            Assert.Equal(expectedFactor, actual, precision: 4);
        }

        [Fact]
        public void CalculateSubProject_AppliesKThickToFabricationRate()
        {
            // 验证 65mm 特厚板是否正确按 1.40 倍放大车间制作工费
            var subThick = new BoqSubProject
            {
                SubProjectCode = "SUB-HEAVY-PL",
                StructuralRole = "主桁架",
                MaxPlateThickness = 65.0, // 65mm => K_Thick = 1.40
                SubProjectNetWeightKg = 10000.0, // 10t
                FabricationQuotaRule = new FabricationQuotaRule
                {
                    BaseLaborRatePerTon = 2000.0,
                    IntersectingCuttingUnitRate = 300.0,
                    UltrasonicTestingUnitRate = 200.0,
                    PreassemblyUnitRate = 100.0
                }
            };

            var res = BoqAggregationEngine.CalculateSubProject(subThick);

            // Base rate sum = 2000 + 300 + 200 + 100 = 2600.0 元/t
            // Unit fabrication cost = 2600 * 1.40 = 3640.0 元/t
            Assert.Equal(1.40, res.KThick);
            Assert.Equal(3640.0, subThick.UnitFabricationCost, precision: 2);
            // 10t * 3640 = 36,400.0 元
            Assert.Equal(36400.0, subThick.TotalFabricationCost, precision: 2);
        }

        #endregion

        #region 3. K_Elev、K_SiteWeld 及 4 档起重机吨位 K_Weight 压力测试

        [Theory]
        [InlineData(0.0, 1.00)]
        [InlineData(15.0, 1.00)]
        [InlineData(20.0, 1.00)]   // 20m 边界点处于第一档
        [InlineData(20.001, 1.15)] // 跃迁进入第二档
        [InlineData(30.0, 1.15)]
        [InlineData(35.0, 1.15)]   // 35m 边界点处于第二档
        [InlineData(35.001, 1.30)] // 跃迁进入第三档 (超高空)
        [InlineData(40.0, 1.30)]   // 40m 要求验证点
        [InlineData(80.0, 1.30)]
        public void CalculateElevationFactor_BoundaryTransitions(double elevationM, double expectedFactor)
        {
            double actual = BoqAggregationEngine.CalculateElevationFactor(elevationM);
            Assert.Equal(expectedFactor, actual, precision: 4);
        }

        [Theory]
        [InlineData(false, 1.00)]
        [InlineData(true, 1.30)]
        public void CalculateSiteWeldFactor_Transitions(bool isSiteWelded, double expectedFactor)
        {
            double actual = BoqAggregationEngine.CalculateSiteWeldFactor(isSiteWelded);
            Assert.Equal(expectedFactor, actual, precision: 4);
        }

        [Theory]
        [InlineData(0.0, 1.00)]
        [InlineData(4.999, 1.00)]
        [InlineData(5.0, 1.20)]    // 边界点 5.0t 处于 5-15t 档
        [InlineData(10.0, 1.20)]
        [InlineData(15.0, 1.20)]   // 边界点 15.0t 处于 5-15t 档
        [InlineData(15.001, 1.30)] // 跃迁进入 15-25t 档
        [InlineData(20.0, 1.30)]
        [InlineData(25.0, 1.30)]   // 边界点 25.0t 处于 15-25t 档
        [InlineData(25.001, 1.45)] // 跃迁进入 >25t 档 (重特大构件)
        [InlineData(50.0, 1.45)]
        public void CalculateWeightFactor_PieceWeightBoundaryTransitions(double pieceWeightTon, double expectedFactor)
        {
            double actual = BoqAggregationEngine.CalculateWeightFactor(pieceWeightTon);
            Assert.Equal(expectedFactor, actual, precision: 4);
        }

        [Theory]
        [InlineData("<5t", 1.00)]
        [InlineData("<5", 1.00)]
        [InlineData("5-15t", 1.20)]
        [InlineData("5~15t", 1.20)]
        [InlineData("15-25t", 1.30)]
        [InlineData("15~25t", 1.30)]
        [InlineData(">25t", 1.45)]
        [InlineData(">25", 1.45)]
        public void CalculateWeightFactorFromTier_StringTiers(string tier, double expectedFactor)
        {
            double actual = BoqAggregationEngine.CalculateWeightFactorFromTier(tier);
            Assert.Equal(expectedFactor, actual, precision: 4);
        }

        [Fact]
        public void CalculateSubProject_CompoundInstallationFactors_MultipliesKElev_KSiteWeld_KWeight()
        {
            // 综合验证：H=40m (K_Elev=1.30), 现场原位焊=true (K_SiteWeld=1.30), 单件重30t (>25t => K_Weight=1.45)
            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-SUPER-HEAVY",
                SubProjectNetWeightKg = 30000.0, // 30t
                Quantity = 1,
                AssemblyElevation = 40.0, // 40m => 1.30
                HasFieldWelding = true,   // true => 1.30
                CraneTonnageTier = ">25t",// >25t => 1.45
                InstallationQuotaRule = new InstallationQuotaRule
                {
                    BaseLaborRatePerTon = 1800.0,
                    MainCraneRatePerTon = 650.0,
                    TrestlePlatformRatePerTon = 300.0
                }
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            Assert.Equal(1.30, res.KElev);
            Assert.Equal(1.30, res.KSiteWeld);
            Assert.Equal(1.45, res.KWeight);

            // Base rate sum = 1800 + 650 + 300 = 2750.0 元/t
            // Compound multiplier = 1.30 * 1.30 * 1.45 = 2.4505
            // Expected unit installation cost = 2750 * 2.4505 = 6,738.875 元/t
            double expectedUnitRate = 2750.0 * 1.30 * 1.30 * 1.45;
            Assert.Equal(expectedUnitRate, sub.UnitInstallationCost, precision: 2);

            // Total = 30t * 6738.875 = 202,166.25 元
            double expectedTotal = 30.0 * expectedUnitRate;
            Assert.Equal(expectedTotal, sub.TotalInstallationCost, precision: 2);
        }

        #endregion

        #region 4. 采办经营创效 (剪刀差利润) 压力测试

        [Fact]
        public void CalculatePart_PositiveScissorProfit_CalculatesSpreadMultipliedByGrossWeight()
        {
            var part = new BoqPart
            {
                PartMark = "P-ARBITRAGE-01",
                Quantity = 1,
                SingleNetWeight = 10000.0, // 10t net
                PurchasingWasteFactor = 1.15, // 11.5t gross
                ApprovedPricePerTon = 5200.0, // 业主批复价
                ActualPurchasePricePerTon = 4400.0 // 采办锁定执行价 (单吨剪刀差 +800元/t)
            };

            var res = BoqAggregationEngine.CalculatePart(part);

            // ArbitrageProfit = (5200 - 4400) * 11.5 = 800 * 11.5 = 9,200.0 元
            Assert.Equal(9200.0, part.ArbitrageProfit, precision: 2);
            Assert.Equal(9200.0, res.ArbitrageProfit, precision: 2);
        }

        [Fact]
        public void CalculatePart_NegativeScissorProfit_CapturesProcurementCostOverrun()
        {
            var part = new BoqPart
            {
                PartMark = "P-OVERRUN-02",
                Quantity = 1,
                SingleNetWeight = 20000.0, // 20t net => 23t gross
                PurchasingWasteFactor = 1.15,
                ApprovedPricePerTon = 4300.0, // 业主批复指导价
                ActualPurchasePricePerTon = 4600.0 // 实际采办价超支 -300元/t
            };

            var res = BoqAggregationEngine.CalculatePart(part);

            // ArbitrageProfit = (4300 - 4600) * 23.0 = -300 * 23.0 = -6,900.0 元
            Assert.Equal(-6900.0, part.ArbitrageProfit, precision: 2);
            Assert.Equal(-6900.0, res.ArbitrageProfit, precision: 2);
        }

        [Fact]
        public void CalculatePart_ZeroPrices_ResultsInZeroScissorProfit()
        {
            var part = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 5000.0,
                ApprovedPricePerTon = 0.0,
                ActualPurchasePricePerTon = 0.0
            };

            var res = BoqAggregationEngine.CalculatePart(part);
            Assert.Equal(0.0, part.ArbitrageProfit);
            Assert.Equal(0.0, res.ArbitrageProfit);
        }

        [Fact]
        public void CalculateProject_AggregatesScissorDifferenceAcrossAllParts()
        {
            var part1 = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 10000.0, // 11.5t gross
                PurchasingWasteFactor = 1.15,
                ApprovedPricePerTon = 5000.0,
                ActualPurchasePricePerTon = 4200.0 // +800 * 11.5 = +9,200
            };
            var part2 = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 10000.0, // 11.5t gross
                PurchasingWasteFactor = 1.15,
                ApprovedPricePerTon = 4500.0,
                ActualPurchasePricePerTon = 4700.0 // -200 * 11.5 = -2,300
            };

            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-SCISSOR",
                Parts = new List<BoqPart> { part1, part2 }
            };

            var item = new BoqTenderItem
            {
                ItemTag = "ITEM-01",
                TenderQuantity = 20.0,
                SubProjects = new List<BoqSubProject> { sub }
            };

            var project = new BoqProject
            {
                ProjectCode = "PROJ-SCISSOR",
                TenderItems = new List<BoqTenderItem> { item }
            };

            var projRes = BoqAggregationEngine.CalculateProject(project);

            // Total Scissor Profit = 9,200 - 2,300 = 6,900.0 元
            Assert.Equal(6900.0, projRes.TotalProcurementScissorProfit, precision: 2);

            // 验证总经济师大盘投影中的剪刀差归集
            var projection = BoqAggregationEngine.ProjectToUltimateCostAnalysis(projRes, project);
            Assert.NotNull(projection.UltimateCostRows);
            var row = projection.UltimateCostRows!.First();
            Assert.Equal(6900.0, row.ScissorDifferenceProfit, precision: 2);
            Assert.Equal(6900.0, Convert.ToDouble(projection.SummaryMetrics["采办经营创效总额(剪刀差利润, 元)"]), precision: 2);
        }

        #endregion

        #region 5. 双层费率策略判定 (IsCustomRateEnabled = true 覆盖 vs false 继承)

        [Fact]
        public void CalculateTenderItem_WhenCustomRateDisabled_InheritsProjectDefaults()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var part = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 10000.0,
                BaseNetPricePerTon = 4050.0,
                FreightSurchargePerTon = 80.0,
                PurchasingWasteFactor = 1.15
            };

            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-DEFAULT-RATE",
                StructuralRole = "次构件",
                MaxPlateThickness = 16.0,
                AssemblyElevation = 15.0,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                Parts = new List<BoqPart> { part }
            };

            var item = new BoqTenderItem
            {
                TenderQuantity = 10.0,
                IsCustomRateEnabled = false,
                CustomManagementFeeRate = 0.20, // 即使配置了高费率，但未启用应被完全忽略
                CustomProfitRate = 0.25,
                SubProjects = new List<BoqSubProject> { sub }
            };

            var res = BoqAggregationEngine.CalculateTenderItem(item, project);

            Assert.False(res.IsCustomRateApplied);
            Assert.Equal(0.08, res.EffectiveManagementFeeRate);
            Assert.Equal(0.07, res.EffectiveProfitRate);

            // Material = 11.5t * 4130 = 47,495.0
            // Fab = 10t * 2130 = 21,300.0
            // Install = 10t * 2750 = 27,500.0
            // Direct sum = 96,295.0 => Direct cost per ton = 9,629.50 元/t
            Assert.Equal(9629.50, item.DirectCostPerTon, precision: 2);

            // Comprehensive unit price = 9629.50 * (1 + 0.08) * (1 + 0.07) = 9629.50 * 1.08 * 1.07 = 11,127.85 元/t
            double expectedUnitPrice = 9629.50 * 1.08 * 1.07;
            Assert.Equal(expectedUnitPrice, item.ComprehensiveUnitPrice, precision: 2);
            Assert.Equal(expectedUnitPrice * 10.0, item.TotalTenderPrice, precision: 2);
        }

        [Fact]
        public void CalculateTenderItem_WhenCustomRateEnabled_OverridesProjectDefaults()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var part = new BoqPart
            {
                Quantity = 1,
                SingleNetWeight = 10000.0,
                BaseNetPricePerTon = 4050.0,
                FreightSurchargePerTon = 80.0,
                PurchasingWasteFactor = 1.15
            };

            var sub = new BoqSubProject
            {
                SubProjectCode = "SUB-CUSTOM-RATE",
                StructuralRole = "次构件",
                MaxPlateThickness = 16.0,
                AssemblyElevation = 15.0,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                Parts = new List<BoqPart> { part }
            };

            var item = new BoqTenderItem
            {
                TenderQuantity = 10.0,
                IsCustomRateEnabled = true, // 启用不平衡报价覆盖
                CustomManagementFeeRate = 0.12, // 12% 管理费
                CustomProfitRate = 0.15,        // 15% 利润率
                SubProjects = new List<BoqSubProject> { sub }
            };

            var res = BoqAggregationEngine.CalculateTenderItem(item, project);

            Assert.True(res.IsCustomRateApplied);
            Assert.Equal(0.12, res.EffectiveManagementFeeRate);
            Assert.Equal(0.15, res.EffectiveProfitRate);

            // Direct cost per ton = 9,629.50
            // Comprehensive price = 9,629.50 * 1.12 * 1.15 = 12,402.80 元/t
            double expectedUnitPrice = 9629.50 * 1.12 * 1.15;
            Assert.Equal(expectedUnitPrice, item.ComprehensiveUnitPrice, precision: 2);
            Assert.Equal(expectedUnitPrice * 10.0, item.TotalTenderPrice, precision: 2);
        }

        [Fact]
        public void CalculateTenderItem_PartialCustomRateOverride_MixesOverrideAndDefault()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var item = new BoqTenderItem
            {
                TenderQuantity = 1.0,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.15, // 管理费覆盖
                CustomProfitRate = null          // 利润率未填，应继承项目默认 0.07
            };

            var res = BoqAggregationEngine.CalculateTenderItem(item, project);

            Assert.True(res.IsCustomRateApplied);
            Assert.Equal(0.15, res.EffectiveManagementFeeRate);
            Assert.Equal(0.07, res.EffectiveProfitRate);
        }

        [Fact]
        public void CalculateTenderItem_ZeroCustomRateOverride_PreservesZeroPercentStrategically()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };

            var item = new BoqTenderItem
            {
                TenderQuantity = 10.0,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.0, // 战略保本报价：0%管理费
                CustomProfitRate = 0.0          // 0%利润
            };

            var res = BoqAggregationEngine.CalculateTenderItem(item, project);

            Assert.True(res.IsCustomRateApplied);
            Assert.Equal(0.0, res.EffectiveManagementFeeRate);
            Assert.Equal(0.0, res.EffectiveProfitRate);
            Assert.Equal(item.DirectCostPerTon, item.ComprehensiveUnitPrice);
        }

        #endregion

        #region 6. 极限空集合、0 工程量、null 项目防御性容错测试

        [Fact]
        public void CalculatePart_NullPart_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BoqAggregationEngine.CalculatePart(null!));
        }

        [Fact]
        public void CalculatePart_ZeroQuantityAndZeroWeights_HandlesDefensivelyWithoutException()
        {
            var part = new BoqPart
            {
                Quantity = 0,
                SingleNetWeight = 0.0,
                TotalNetWeight = 0.0
            };

            var res = BoqAggregationEngine.CalculatePart(part);

            Assert.Equal(1, part.Quantity); // 应防御性规避 0 除数
            Assert.Equal(0.0, part.TotalNetWeight);
            Assert.Equal(0.0, part.TotalGrossWeight);
            Assert.Equal(0.0, part.TotalPartMaterialCost);
            Assert.Equal(0.0, res.TotalPartCost);
        }

        [Fact]
        public void CalculatePart_NullStrings_HandlesDefensively()
        {
            var part = new BoqPart
            {
                PartMark = null!,
                SteelGrade = null!,
                SectionSpecification = null!,
                ZDirectionGrade = null!
            };

            var res = BoqAggregationEngine.CalculatePart(part);

            Assert.NotNull(res);
            Assert.True(part.BaseNetPricePerTon > 0.0);
        }

        [Fact]
        public void CalculateSubProject_NullSubProject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BoqAggregationEngine.CalculateSubProject(null!));
        }

        [Fact]
        public void CalculateSubProject_EmptyPartsAndZeroWeights_DoesNotThrowDivideByZero()
        {
            var sub = new BoqSubProject
            {
                Quantity = 0,
                SubProjectNetWeightKg = 0.0,
                SubProjectGrossWeightKg = 0.0,
                Parts = new List<BoqPart>()
            };

            var res = BoqAggregationEngine.CalculateSubProject(sub);

            Assert.NotNull(res);
            Assert.Equal(0.0, res.TotalNetWeightKg);
            Assert.Equal(0.0, res.DirectCost);
            Assert.Equal(0.0, res.UnitDirectCostPerTon);
            Assert.Empty(res.PartResults);
        }

        [Fact]
        public void CalculateTenderItem_NullItem_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BoqAggregationEngine.CalculateTenderItem(null!));
        }

        [Fact]
        public void CalculateTenderItem_EmptySubProjectsAndZeroTenderQuantity_HandlesSafely()
        {
            var item = new BoqTenderItem
            {
                TenderQuantity = 0.0,
                SubProjects = new List<BoqSubProject>()
            };

            var res = BoqAggregationEngine.CalculateTenderItem(item, null);

            Assert.NotNull(res);
            Assert.Equal(0.0, res.DirectCostSum);
            Assert.Equal(0.0, res.DirectCostPerTon);
            Assert.Equal(0.0, res.ComprehensiveUnitPrice);
            Assert.Equal(0.0, res.TotalTenderPrice);
        }

        [Fact]
        public void CalculateProject_NullProject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BoqAggregationEngine.CalculateProject(null!));
        }

        [Fact]
        public void CalculateProject_EmptyProject_ReturnsAllZeroTotalsWithoutExceptions()
        {
            var project = new BoqProject
            {
                ProjectCode = "EMPTY-PROJ",
                TenderItems = new List<BoqTenderItem>()
            };

            var res = BoqAggregationEngine.CalculateProject(project);

            Assert.NotNull(res);
            Assert.Equal(0.0, res.TotalNetWeightKg);
            Assert.Equal(0.0, res.TotalGrossWeightKg);
            Assert.Equal(0.0, res.TotalDirectCost);
            Assert.Equal(0.0, res.TotalBidPrice);
            Assert.Equal(0.0, res.TotalProcurementScissorProfit);
            Assert.Empty(res.TenderItemResults);
        }

        [Fact]
        public void ProjectToAudience_AllFivePerspectives_HandleEmptyProjectSafelyWithoutNaNOrDivideByZero()
        {
            var emptyProject = new BoqProject
            {
                ProjectCode = "EMPTY-AUDIENCE-TEST",
                TenderItems = new List<BoqTenderItem>()
            };

            var emptyResult = BoqAggregationEngine.CalculateProject(emptyProject);

            var perspectives = new[]
            {
                BoqAudiencePerspective.Owner,
                BoqAudiencePerspective.ChiefEconomist,
                BoqAudiencePerspective.FabricationShop,
                BoqAudiencePerspective.SiteProjectDept,
                BoqAudiencePerspective.Procurement
            };

            foreach (var p in perspectives)
            {
                var report = BoqAggregationEngine.ProjectToAudience(emptyResult, p, emptyProject);
                Assert.NotNull(report);
                Assert.Empty(report.Rows);
                Assert.NotEmpty(report.Headers);
                Assert.NotEmpty(report.SummaryMetrics);

                // 校验所有数值度量没有 NaN 或 Infinity
                foreach (var kvp in report.SummaryMetrics)
                {
                    if (kvp.Value is double dVal)
                    {
                        Assert.False(double.IsNaN(dVal), $"Metric '{kvp.Key}' resulted in NaN");
                        Assert.False(double.IsInfinity(dVal), $"Metric '{kvp.Key}' resulted in Infinity");
                    }
                }
            }
        }

        [Fact]
        public void AggregateMaterialProcurement_NullProject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => BoqAggregationEngine.AggregateMaterialProcurement(null!));
        }

        [Fact]
        public void AggregateMaterialProcurement_EmptyProject_ReturnsEmptyList()
        {
            var project = new BoqProject
            {
                TenderItems = new List<BoqTenderItem>()
            };

            var list = BoqAggregationEngine.AggregateMaterialProcurement(project);
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        #endregion
    }
}
