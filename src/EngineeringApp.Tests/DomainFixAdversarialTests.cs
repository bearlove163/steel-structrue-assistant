using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    /// <summary>
    /// Empirical Adversarial Stress Harness for Milestone 1 Iteration 2 Domain Fixes:
    /// 1. BoqPart.LossRate when PurchasingWasteFactor = 1.0 (strictly 0.0)
    /// 2. BoqTenderItem.GetEffectiveRates() parameterless with item.Project loaded
    /// 3. BoqPart.TotalGrossWeightKg bidirectional setter/getter symmetry and [NotMapped] decoration
    /// </summary>
    public class DomainFixAdversarialTests
    {
        #region 1. LossRate & PurchasingWasteFactor Adversarial Tests

        [Fact]
        public void LossRate_WhenPurchasingWasteFactorIsExactlyOne_IsStrictlyZero()
        {
            var part = new BoqPart
            {
                PurchasingWasteFactor = 1.0
            };

            // Empirical verification: strictly equal to 0.0
            Assert.Equal(0.0, part.LossRate);
            Assert.True(part.LossRate == 0.0, "part.LossRate must be strictly 0.0 when PurchasingWasteFactor == 1.0");
        }

        [Fact]
        public void LossRate_WhenSetToZero_SetsFactorToOneAndReadsBackStrictlyZero()
        {
            var part = new BoqPart();
            part.LossRate = 0.0;

            Assert.Equal(1.0, part.PurchasingWasteFactor);
            Assert.Equal(0.0, part.LossRate);
            Assert.True(part.LossRate == 0.0);
        }

        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(0.03, 1.03)]
        [InlineData(0.05, 1.05)]
        [InlineData(0.08, 1.08)]
        [InlineData(0.12, 1.12)]
        [InlineData(0.15, 1.15)]
        [InlineData(0.20, 1.20)]
        [InlineData(0.35, 1.35)]
        public void LossRate_RoundTripPrecision_MatchesExpectedFactor(double lossRateInput, double expectedFactor)
        {
            var part = new BoqPart
            {
                LossRate = lossRateInput
            };

            Assert.Equal(expectedFactor, part.PurchasingWasteFactor, precision: 4);
            Assert.Equal(lossRateInput, part.LossRate, precision: 4);
        }

        [Fact]
        public void LossRate_FloatingPointMicroStepping_MaintainsSymmetry()
        {
            var part = new BoqPart();
            for (int i = 0; i <= 50; i++)
            {
                double targetRate = Math.Round(i * 0.01, 2);
                part.LossRate = targetRate;

                double actualRate = part.LossRate;
                Assert.Equal(targetRate, actualRate, precision: 4);
                Assert.Equal(1.0 + targetRate, part.PurchasingWasteFactor, precision: 4);
            }
        }

        [Theory]
        [InlineData(0.99)]
        [InlineData(0.50)]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void LossRate_WhenPurchasingWasteFactorIsSubOne_FallsBackToDefaultFifteenPercent(double invalidFactor)
        {
            // Defensive behavior check: factors < 1.0 fall back to 0.15 (15%) default loss
            var part = new BoqPart
            {
                PurchasingWasteFactor = invalidFactor
            };

            Assert.Equal(0.15, part.LossRate);
        }

        [Fact]
        public void SingleGrossWeightKg_WithZeroWasteFactor_EqualsSingleNetWeight()
        {
            var part = new BoqPart
            {
                SingleNetWeight = 2500.75,
                PurchasingWasteFactor = 1.0
            };

            Assert.Equal(0.0, part.LossRate);
            Assert.Equal(2500.75, part.SingleGrossWeightKg);
        }

        #endregion

        #region 2. GetEffectiveRates Navigation Property & Precedence Adversarial Tests

        [Fact]
        public void GetEffectiveRates_Parameterless_WhenItemProjectIsLoaded_ReturnsProjectRates()
        {
            var project = new BoqProject
            {
                Id = 101,
                ProjectName = "Adversarial Test Project",
                DefaultManagementFeeRate = 0.135,
                DefaultProfitRate = 0.092
            };

            var item = new BoqTenderItem
            {
                Id = 201,
                Project = project,
                ProjectId = 101,
                IsCustomRateEnabled = false
            };

            // Call parameterless GetEffectiveRates()
            var (mgmt, profit) = item.GetEffectiveRates();

            // Must match project default rates, NOT hardcoded 0.08 / 0.07
            Assert.Equal(0.135, mgmt);
            Assert.Equal(0.092, profit);
            Assert.Equal(0.135, item.EffectiveManagementFeeRate);
            Assert.Equal(0.092, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_ExplicitProjectArgument_OverridesItemProjectNavigationProperty()
        {
            var attachedProject = new BoqProject
            {
                DefaultManagementFeeRate = 0.10,
                DefaultProfitRate = 0.08
            };

            var overridingProject = new BoqProject
            {
                DefaultManagementFeeRate = 0.05,
                DefaultProfitRate = 0.04
            };

            var item = new BoqTenderItem
            {
                Project = attachedProject,
                IsCustomRateEnabled = false
            };

            // Explicit argument overridingProject should take highest precedence over attachedProject
            var (mgmt, profit) = item.GetEffectiveRates(overridingProject);

            Assert.Equal(0.05, mgmt);
            Assert.Equal(0.04, profit);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabled_OverridesItemProject()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.12,
                DefaultProfitRate = 0.09
            };

            var item = new BoqTenderItem
            {
                Project = project,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.22,
                CustomProfitRate = 0.18
            };

            var (mgmt, profit) = item.GetEffectiveRates();

            Assert.Equal(0.22, mgmt);
            Assert.Equal(0.18, profit);
        }

        [Fact]
        public void GetEffectiveRates_WhenCustomRateEnabled_ZeroRatesAreHonoredOverProject()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.12,
                DefaultProfitRate = 0.09
            };

            var item = new BoqTenderItem
            {
                Project = project,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.0,
                CustomProfitRate = 0.0
            };

            var (mgmt, profit) = item.GetEffectiveRates();

            Assert.Equal(0.0, mgmt);
            Assert.Equal(0.0, profit);
            Assert.Equal(0.0, item.EffectiveManagementFeeRate);
            Assert.Equal(0.0, item.EffectiveProfitRate);
        }

        [Fact]
        public void GetEffectiveRates_PartialCustomRate_FallsBackToItemProjectForUnsetRate()
        {
            var project = new BoqProject
            {
                DefaultManagementFeeRate = 0.11,
                DefaultProfitRate = 0.095
            };

            var item = new BoqTenderItem
            {
                Project = project,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = 0.16,
                CustomProfitRate = null // not customized
            };

            var (mgmt, profit) = item.GetEffectiveRates();

            // Custom mgmt used, but profit falls back to project
            Assert.Equal(0.16, mgmt);
            Assert.Equal(0.095, profit);
        }

        [Fact]
        public void GetEffectiveRates_WhenProjectAndArgumentBothNull_ReturnsHardcodedDefaults()
        {
            var item = new BoqTenderItem
            {
                Project = null,
                IsCustomRateEnabled = false
            };

            var (mgmt, profit) = item.GetEffectiveRates(null);

            Assert.Equal(0.08, mgmt);
            Assert.Equal(0.07, profit);
        }

        #endregion

        #region 3. TotalGrossWeightKg Dual-Naming Symmetry & Mapping Adversarial Tests

        [Fact]
        public void TotalGrossWeightKg_SetterUpdatesTotalGrossWeight_Symmetrically()
        {
            var part = new BoqPart();

            part.TotalGrossWeightKg = 87654.321;
            Assert.Equal(87654.321, part.TotalGrossWeight);
            Assert.Equal(87654.321, part.TotalGrossWeightKg);

            part.TotalGrossWeight = 12345.678;
            Assert.Equal(12345.678, part.TotalGrossWeightKg);
            Assert.Equal(12345.678, part.TotalGrossWeight);

            part.TotalGrossWeightKg = 0.0;
            Assert.Equal(0.0, part.TotalGrossWeight);
        }

        [Fact]
        public void TotalGrossWeightKg_HasNotMappedAttribute_ToPreventEfCoreColumnDuplication()
        {
            var propInfo = typeof(BoqPart).GetProperty(nameof(BoqPart.TotalGrossWeightKg));
            Assert.NotNull(propInfo);

            var notMappedAttr = propInfo.GetCustomAttribute<NotMappedAttribute>();
            Assert.NotNull(notMappedAttr);
        }

        [Theory]
        [InlineData(nameof(BoqPart.LossRate))]
        [InlineData(nameof(BoqPart.SingleNetWeightKg))]
        [InlineData(nameof(BoqPart.SingleGrossWeightKg))]
        [InlineData(nameof(BoqPart.TotalGrossWeightKg))]
        [InlineData(nameof(BoqPart.ZPerformance))]
        [InlineData(nameof(BoqPart.QuantityPerSubProject))]
        public void BoqPart_AllAliasProperties_HaveNotMappedAttribute(string propertyName)
        {
            var propInfo = typeof(BoqPart).GetProperty(propertyName);
            Assert.NotNull(propInfo);

            var notMappedAttr = propInfo.GetCustomAttribute<NotMappedAttribute>();
            Assert.True(notMappedAttr != null, $"Property {propertyName} must have [NotMapped] attribute.");
        }

        #endregion
    }
}
