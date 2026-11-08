using System;
using System.Collections.Generic;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    public class BoqHierarchyModelStressTests
    {
        #region 1. GetEffectiveRates Edge Scenario Tests

        [Fact]
        public void GetEffectiveRates_WhenProjectIsNullAndCustomRateDisabled_ReturnsDefaultRates()
        {
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = false
            };

            var (mgmt, profit) = item.GetEffectiveRates(null);

            Assert.Equal(0.08, mgmt);
            Assert.Equal(0.07, profit);
            Assert.Equal(0.08, item.EffectiveManagementFeeRate);
            Assert.Equal(0.07, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenProjectHasCustomDefaultsAndCustomRateDisabled_ReturnsProjectDefaults()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.12,
                DefaultProfitRate = 0.09
            };
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = false
            };

            var (mgmt, profit) = item.GetEffectiveRates(project);

            Assert.Equal(0.12, mgmt);
            Assert.Equal(0.09, profit);
            Assert.Equal(0.12, item.EffectiveManagementFeeRate);
            Assert.Equal(0.09, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateDisabledButCustomValuesPopulated_IgnoresCustomValues()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = false,
                CustomManagementFeeRate = 0.25,
                CustomProfitRate = 0.30
            };

            var (mgmt, profit) = item.GetEffectiveRates(project);

            // Because IsCustomRateEnabled is false, custom values must be ignored
            Assert.Equal(0.08, mgmt);
            Assert.Equal(0.07, profit);
            Assert.Equal(0.08, item.EffectiveManagementFeeRate);
            Assert.Equal(0.07, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabledWithValidOverrides_ReturnsOverrides()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.10,
                CustomProfitRate = 0.12
            };

            var (mgmt, profit) = item.GetEffectiveRates(project);

            Assert.Equal(0.10, mgmt);
            Assert.Equal(0.12, profit);
            Assert.Equal(0.10, item.EffectiveManagementFeeRate);
            Assert.Equal(0.12, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabledWithZeroRates_PreservesZero()
        {
            // In strategic bidding, 0% management fee or 0% profit can be intentionally quoted
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.0,
                CustomProfitRate = 0.0
            };

            var (mgmt, profit) = item.GetEffectiveRates(project);

            Assert.Equal(0.0, mgmt);
            Assert.Equal(0.0, profit);
            Assert.Equal(0.0, item.EffectiveManagementFeeRate);
            Assert.Equal(0.0, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabledWithNegativeRates_PreservesNegative()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.08,
                DefaultProfitRate = 0.07
            };
            var item = new BoqTenderItem
            {
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = -0.05,
                CustomProfitRate = -0.02
            };

            var (mgmt, profit) = item.GetEffectiveRates(project);

            Assert.Equal(-0.05, mgmt);
            Assert.Equal(-0.02, profit);
            Assert.Equal(-0.05, item.EffectiveManagementFeeRate);
            Assert.Equal(-0.02, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabledWithPartialNull_FallsBackPerRateIndependently()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.09,
                DefaultProfitRate = 0.06
            };

            // Case A: Custom mgmt set, custom profit null
            var itemA = new BoqTenderItem
            {
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.15,
                CustomProfitRate = null
            };
            var (mgmtA, profitA) = itemA.GetEffectiveRates(project);
            Assert.Equal(0.15, mgmtA);
            Assert.Equal(0.06, profitA); // falls back to project default

            // Case B: Custom mgmt null, custom profit set
            var itemB = new BoqTenderItem
            {
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = null,
                CustomProfitRate = 0.11
            };
            var (mgmtB, profitB) = itemB.GetEffectiveRates(project);
            Assert.Equal(0.09, mgmtB); // falls back to project default
            Assert.Equal(0.11, profitB);
        }

        [Fact]
        public void GetEffectiveRates_WhenProjectArgumentNull_InspectsNavigationProperty()
        {
            // Stress test: does GetEffectiveRates() inspect item.Project when project parameter is null?
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.15,
                DefaultProfitRate = 0.12
            };
            var item = new BoqTenderItem
            {
                Project = project,
                IsCustomRateEnabled = false
            };

            var (mgmt, profit) = item.GetEffectiveRates(); // null argument

            // Inspects item.Project navigation property when parameter is null:
            Assert.Equal(0.15, mgmt);
            Assert.Equal(0.12, profit);

            // Passing item.Project explicitly produces project rates:
            var (mgmtExplicit, profitExplicit) = item.GetEffectiveRates(item.Project);
            Assert.Equal(0.15, mgmtExplicit);
            Assert.Equal(0.12, profitExplicit);
        }

        #endregion

        #region 2. BoqPart Waste Factor & Gross Weight Tests

        [Fact]
        public void BoqPart_DefaultWasteFactor_IsOnePointOneFive()
        {
            var part = new BoqPart();
            Assert.Equal(1.15, part.PurchasingWasteFactor);
            Assert.Equal(0.15, part.LossRate, precision: 4);
        }

        [Fact]
        public void BoqPart_SingleGrossWeightKg_CalculatesCorrectly()
        {
            var part = new BoqPart
            {
                SingleNetWeight = 1000.0,
                PurchasingWasteFactor = 1.15
            };

            Assert.Equal(1150.0, part.SingleGrossWeightKg);
        }

        [Fact]
        public void BoqPart_SingleGrossWeightKg_WithZeroWeight_IsZero()
        {
            var part = new BoqPart
            {
                SingleNetWeight = 0.0,
                PurchasingWasteFactor = 1.15
            };

            Assert.Equal(0.0, part.SingleGrossWeightKg);
        }

        [Fact]
        public void BoqPart_LossRate_SetCustomRate_UpdatesPurchasingWasteFactor()
        {
            var part = new BoqPart();
            part.LossRate = 0.20; // 20% waste

            Assert.Equal(1.20, part.PurchasingWasteFactor, precision: 5);
            Assert.Equal(0.20, part.LossRate, precision: 5);
        }

        [Fact]
        public void LossRate_WhenSetToZero_ExposesEdgeCaseBehavior()
        {
            // Stress test: setting LossRate = 0.0 (no waste)
            var part = new BoqPart();
            part.LossRate = 0.0;

            // PurchasingWasteFactor is 1.0 + 0.0 = 1.0
            Assert.Equal(1.0, part.PurchasingWasteFactor);

            // Corrected behavior: when PurchasingWasteFactor >= 1.0, LossRate returns 0.0
            Assert.Equal(0.0, part.LossRate);

            // Direct check on PurchasingWasteFactor = 1.0
            var partDirect = new BoqPart { PurchasingWasteFactor = 1.0 };
            Assert.Equal(0.0, partDirect.LossRate);
        }

        #endregion

        #region 3. Dual-Naming Property Aliases Stress Tests

        [Fact]
        public void BoqTenderItem_DirectCostPricePerUnit_SyncsWithDirectCostPerTon()
        {
            var item = new BoqTenderItem();
            item.DirectCostPricePerUnit = 4500.5;
            Assert.Equal(4500.5, item.DirectCostPerTon);

            item.DirectCostPerTon = 4800.0;
            Assert.Equal(4800.0, item.DirectCostPricePerUnit);
        }

        [Fact]
        public void BoqSubProject_DualPropertyAliases_SyncSymmetrically()
        {
            var sub = new BoqSubProject();

            // MaxPieceThicknessMm <-> MaxPlateThickness
            sub.MaxPieceThicknessMm = 50.0;
            Assert.Equal(50.0, sub.MaxPlateThickness);
            sub.MaxPlateThickness = 60.0;
            Assert.Equal(60.0, sub.MaxPieceThicknessMm);

            // InstallationElevationMeter <-> AssemblyElevation
            sub.InstallationElevationMeter = 32.5;
            Assert.Equal(32.5, sub.AssemblyElevation);
            sub.AssemblyElevation = 45.0;
            Assert.Equal(45.0, sub.InstallationElevationMeter);

            // PieceWeightTon <-> SingleWeight
            sub.PieceWeightTon = 18.2;
            Assert.Equal(18.2, sub.SingleWeight);
            sub.SingleWeight = 25.0;
            Assert.Equal(25.0, sub.PieceWeightTon);

            // IsSiteWelded <-> HasFieldWelding
            sub.IsSiteWelded = true;
            Assert.True(sub.HasFieldWelding);
            sub.HasFieldWelding = false;
            Assert.False(sub.IsSiteWelded);

            // DesignFireResistanceHours <-> FireRatingHours
            sub.DesignFireResistanceHours = 2.5;
            Assert.Equal(2.5, sub.FireRatingHours);
            sub.FireRatingHours = 3.0;
            Assert.Equal(3.0, sub.DesignFireResistanceHours);
        }

        [Fact]
        public void BoqPart_DualPropertyAliases_SyncSymmetrically()
        {
            var part = new BoqPart();

            // Specification <-> SectionSpecification
            part.Specification = "PIP1200*50";
            Assert.Equal("PIP1200*50", part.SectionSpecification);
            part.SectionSpecification = "PL50*2150";
            Assert.Equal("PL50*2150", part.Specification);

            // MaterialGrade <-> SteelGrade
            part.MaterialGrade = "Q460GJC";
            Assert.Equal("Q460GJC", part.SteelGrade);
            part.SteelGrade = "Q355B";
            Assert.Equal("Q355B", part.MaterialGrade);

            // PlateThicknessMm <-> Thickness
            part.PlateThicknessMm = 50.0;
            Assert.Equal(50.0, part.Thickness);
            part.Thickness = 65.0;
            Assert.Equal(65.0, part.PlateThicknessMm);

            // Width <-> DiameterOrWidthMm
            part.Width = 1200.0;
            Assert.Equal(1200.0, part.DiameterOrWidthMm);
            part.DiameterOrWidthMm = 2150.0;
            Assert.Equal(2150.0, part.Width);

            // CutLengthMm <-> Length
            part.CutLengthMm = 6500.0;
            Assert.Equal(6500.0, part.Length);
            part.Length = 12000.0;
            Assert.Equal(12000.0, part.CutLengthMm);

            // LinearWeightKgM <-> LinearDensity
            part.LinearWeightKgM = 1418.0;
            Assert.Equal(1418.0, part.LinearDensity);
            part.LinearDensity = 1500.0;
            Assert.Equal(1500.0, part.LinearWeightKgM);

            // QuantityPerSubProject <-> Quantity
            part.QuantityPerSubProject = 4;
            Assert.Equal(4, part.Quantity);
            part.Quantity = 8;
            Assert.Equal(8, part.QuantityPerSubProject);

            // SingleNetWeightKg <-> SingleNetWeight
            part.SingleNetWeightKg = 9217.0;
            Assert.Equal(9217.0, part.SingleNetWeight);
            part.SingleNetWeight = 10000.0;
            Assert.Equal(10000.0, part.SingleNetWeightKg);

            // ZPerformance <-> ZDirectionGrade
            part.ZPerformance = "Z25";
            Assert.Equal("Z25", part.ZDirectionGrade);
            part.ZDirectionGrade = "Z35";
            Assert.Equal("Z35", part.ZPerformance);

            // ComprehensiveMaterialUnitPricePerTon <-> MaterialUnitPricePerTon
            part.ComprehensiveMaterialUnitPricePerTon = 5600.0;
            Assert.Equal(5600.0, part.MaterialUnitPricePerTon);
            part.MaterialUnitPricePerTon = 5800.0;
            Assert.Equal(5800.0, part.ComprehensiveMaterialUnitPricePerTon);

            // TotalGrossWeightKg <-> TotalGrossWeight
            part.TotalGrossWeightKg = 1150.0;
            Assert.Equal(1150.0, part.TotalGrossWeight);
            part.TotalGrossWeight = 1200.0;
            Assert.Equal(1200.0, part.TotalGrossWeightKg);
        }

        [Fact]
        public void FabricationQuotaRule_DualPropertyAliases_SyncSymmetrically()
        {
            var rule = new FabricationQuotaRule();

            rule.ApplicableCategory = "空间管桁架";
            Assert.Equal("空间管桁架", rule.StructuralSystem);
            rule.StructuralSystem = "箱型柱";
            Assert.Equal("箱型柱", rule.ApplicableCategory);

            rule.BaseLaborDaysPerTon = 2.0;
            Assert.Equal(2.0, rule.BaseLaborDays);
            rule.BaseLaborDays = 1.8;
            Assert.Equal(1.8, rule.BaseLaborDaysPerTon);

            rule.BaseFabricationFeePerTon = 1600.0;
            Assert.Equal(1600.0, rule.BaseLaborRatePerTon);
            rule.BaseLaborRatePerTon = 1500.0;
            Assert.Equal(1500.0, rule.BaseFabricationFeePerTon);

            rule.IntersectingCutSurcharge = 300.0;
            Assert.Equal(300.0, rule.IntersectingCuttingUnitRate);
            rule.IntersectingCuttingUnitRate = 280.0;
            Assert.Equal(280.0, rule.IntersectingCutSurcharge);

            rule.UltrasonicTestingFeePerTon = 180.0;
            Assert.Equal(180.0, rule.UltrasonicTestingUnitRate);
            rule.UltrasonicTestingUnitRate = 150.0;
            Assert.Equal(150.0, rule.UltrasonicTestingFeePerTon);

            rule.TrialAssemblyFeePerTon = 220.0;
            Assert.Equal(220.0, rule.PreassemblyUnitRate);
            rule.PreassemblyUnitRate = 200.0;
            Assert.Equal(200.0, rule.TrialAssemblyFeePerTon);
        }

        [Fact]
        public void InstallationQuotaRule_DualPropertyAliases_SyncSymmetrically()
        {
            var rule = new InstallationQuotaRule();

            rule.BaseInstallationLaborDaysPerTon = 3.0;
            Assert.Equal(3.0, rule.BaseLaborDays);
            rule.BaseLaborDays = 2.5;
            Assert.Equal(2.5, rule.BaseInstallationLaborDaysPerTon);

            rule.BaseInstallationFeePerTon = 1900.0;
            Assert.Equal(1900.0, rule.BaseLaborRatePerTon);
            rule.BaseLaborRatePerTon = 1800.0;
            Assert.Equal(1800.0, rule.BaseInstallationFeePerTon);

            rule.BaseCraneCostPerTon = 700.0;
            Assert.Equal(700.0, rule.MainCraneRatePerTon);
            rule.MainCraneRatePerTon = 650.0;
            Assert.Equal(650.0, rule.BaseCraneCostPerTon);

            rule.BaseScaffoldingFeePerTon = 350.0;
            Assert.Equal(350.0, rule.TrestlePlatformRatePerTon);
            rule.TrestlePlatformRatePerTon = 300.0;
            Assert.Equal(300.0, rule.BaseScaffoldingFeePerTon);
        }

        #endregion

        #region 4. Audience Perspective & Enum Mapping Tests

        [Fact]
        public void BoqAudiencePerspective_EnumValuesMatchExpectedIntegers()
        {
            Assert.Equal(0, (int)BoqAudiencePerspective.Owner);
            Assert.Equal(1, (int)BoqAudiencePerspective.ChiefEconomist);
            Assert.Equal(2, (int)BoqAudiencePerspective.FabricationShop);
            Assert.Equal(3, (int)BoqAudiencePerspective.SiteProjectDept);
            Assert.Equal(4, (int)BoqAudiencePerspective.Procurement);

            // Backward compatibility aliases
            Assert.Equal(0, (int)BoqAudiencePerspective.OwnerAudit);
            Assert.Equal(0, (int)BoqAudiencePerspective.ClientTenderMaster);
            Assert.Equal(1, (int)BoqAudiencePerspective.UltimateCostAnalysis);
            Assert.Equal(2, (int)BoqAudiencePerspective.FabricationWorkshop);
            Assert.Equal(3, (int)BoqAudiencePerspective.SiteErectionSchedule);
            Assert.Equal(3, (int)BoqAudiencePerspective.SiteErection);
            Assert.Equal(3, (int)BoqAudiencePerspective.Site);
            Assert.Equal(4, (int)BoqAudiencePerspective.MaterialProcurementOrder);
        }

        [Fact]
        public void BoqReportConfig_AudienceModeAndPerspective_MapBiDirectionally()
        {
            var config = new BoqReportConfig();

            config.AudiencePerspective = BoqAudiencePerspective.Owner;
            Assert.Equal(BoqReportAudienceMode.ClientTenderMaster, config.AudienceMode);

            config.AudienceMode = BoqReportAudienceMode.UltimateCostAnalysis;
            Assert.Equal(BoqAudiencePerspective.ChiefEconomist, config.AudiencePerspective);

            config.AudienceMode = BoqReportAudienceMode.MaterialProcurementOrder;
            Assert.Equal(BoqAudiencePerspective.Procurement, config.AudiencePerspective);
        }

        #endregion

        #region 5. Navigation Collections Initialization

        [Fact]
        public void NavigationProperties_InitializedToEmptyCollections_PreventNullRef()
        {
            var project = new BoqProject();
            Assert.NotNull(project.TenderItems);
            Assert.Empty(project.TenderItems);

            var tenderItem = new BoqTenderItem();
            Assert.NotNull(tenderItem.SubProjects);
            Assert.Empty(tenderItem.SubProjects);

            var subProject = new BoqSubProject();
            Assert.NotNull(subProject.Parts);
            Assert.Empty(subProject.Parts);
        }

        #endregion
    }
}
