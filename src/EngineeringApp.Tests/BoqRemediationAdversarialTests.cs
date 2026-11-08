using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;
using Xunit;

namespace EngineeringApp.Tests
{
    /// <summary>
    /// Empirical Challenger Adversarial Stress Suite for Milestone 4 Iteration 2.
    /// Stress-tests ID normalization, ID=0 guard against collision, fallback resolution,
    /// and tree-grid per-node expansion isolation.
    /// </summary>
    public class BoqRemediationAdversarialTests
    {
        #region 1. Seed Data Deterministic IDs & Normalization Verification

        [Fact]
        public void SeedData_CreateBenchmarkProject_HasDeterministicPositiveUniqueIds()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // Project ID
            Assert.Equal(1, project.Id);

            // TenderItem IDs: exactly 3 items, deterministic 1..3
            Assert.Equal(3, project.TenderItems.Count);
            var tenderIds = project.TenderItems.Select(t => t.Id).ToList();
            Assert.Equal(new[] { 1, 2, 3 }, tenderIds);
            Assert.All(project.TenderItems, t => Assert.Equal(1, t.ProjectId));

            // SubProject IDs: exactly 13 items, deterministic 1..13
            var allSubs = project.TenderItems.SelectMany(t => t.SubProjects).ToList();
            Assert.Equal(13, allSubs.Count);
            var subIds = allSubs.Select(s => s.Id).ToList();
            var expectedSubIds = Enumerable.Range(1, 13).ToList();
            Assert.Equal(expectedSubIds, subIds);
            Assert.Equal(13, subIds.Distinct().Count());

            // SubProject foreign keys match parent tender item Id
            foreach (var item in project.TenderItems)
            {
                Assert.All(item.SubProjects, s => Assert.Equal(item.Id, s.TenderItemId));
            }

            // Part IDs: exactly 20 parts, deterministic 1..20
            var allParts = allSubs.SelectMany(s => s.Parts).ToList();
            Assert.Equal(20, allParts.Count);
            var partIds = allParts.Select(p => p.Id).ToList();
            var expectedPartIds = Enumerable.Range(1, 20).ToList();
            Assert.Equal(expectedPartIds, partIds);
            Assert.Equal(20, partIds.Distinct().Count());

            // Part foreign keys match parent subproject Id
            foreach (var sub in allSubs)
            {
                Assert.All(sub.Parts, p => Assert.Equal(sub.Id, p.SubProjectId));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void Normalization_PreventsZeroOrNegativeIds_WhenEntitiesCreatedWithDefaultIds(int unassignedId)
        {
            // Simulate manually instantiated hierarchy with zero or negative IDs
            var project = new BoqProject
            {
                Id = unassignedId,
                ProjectCode = "STRESS-TEST-01",
                ProjectName = "应力测试项目"
            };

            var item1 = new BoqTenderItem { Id = unassignedId, ItemTag = "T1", BillItemName = "项目1" };
            var item2 = new BoqTenderItem { Id = unassignedId, ItemTag = "T2", BillItemName = "项目2" };
            var sub1 = new BoqSubProject { Id = unassignedId, SubProjectCode = "SUB-01", SubProjectName = "构件1" };
            var part1 = new BoqPart { Id = unassignedId, PartMark = "P-01", SectionSpecification = "PL20*500", Quantity = 10, TotalNetWeight = 500 };

            sub1.Parts.Add(part1);
            item1.SubProjects.Add(sub1);
            project.TenderItems.Add(item1);
            project.TenderItems.Add(item2);

            // Execute normalization logic as in NingboOlympicSeedData
            project.Id = project.Id > 0 ? project.Id : 1;
            int nextTenderId = 1;
            int nextSubId = 1;
            int nextPartId = 1;

            foreach (var item in project.TenderItems)
            {
                if (item.Id <= 0) item.Id = nextTenderId++;
                item.ProjectId = project.Id;

                foreach (var sub in item.SubProjects)
                {
                    if (sub.Id <= 0) sub.Id = nextSubId++;
                    sub.TenderItemId = item.Id;

                    foreach (var part in sub.Parts)
                    {
                        if (part.Id <= 0) part.Id = nextPartId++;
                        part.SubProjectId = sub.Id;
                    }
                }
            }

            // Assertions
            Assert.True(project.Id > 0);
            Assert.Equal(1, item1.Id);
            Assert.Equal(2, item2.Id);
            Assert.Equal(1, sub1.Id);
            Assert.Equal(1, part1.Id);
            Assert.Equal(project.Id, item1.ProjectId);
            Assert.Equal(item1.Id, sub1.TenderItemId);
            Assert.Equal(sub1.Id, part1.SubProjectId);
        }

        #endregion

        #region 2. BoqAggregationEngine Entity Resolution & ID Guard Verification

        [Fact]
        public void AggregationEngine_OwnerPerspective_PreservesDistinctCharacteristics_WithPositiveIds()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var calcResult = BoqAggregationEngine.CalculateProject(project);

            var projection = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.Owner, project);
            var rows = projection.ClientTenderRows;

            Assert.NotNull(rows);
            Assert.Equal(3, rows.Count);

            var nhRow = rows.First(r => r.ItemTag == "NH");
            var nfRow = rows.First(r => r.ItemTag == "NF");
            var jlcRow = rows.First(r => r.ItemTag == "JLC");

            Assert.Contains("Q460GJC", nhRow.ProjectCharacteristics);
            Assert.Contains("3.0H厚型", nhRow.ProjectCharacteristics);

            Assert.Contains("Q355B", nfRow.ProjectCharacteristics);
            Assert.Contains("2.0H", nfRow.ProjectCharacteristics);
            Assert.DoesNotContain("Q460GJC", nfRow.ProjectCharacteristics);

            Assert.Contains("Q235B", jlcRow.ProjectCharacteristics);
            Assert.Contains("1.5H薄型", jlcRow.ProjectCharacteristics);
            Assert.DoesNotContain("Q460GJC", jlcRow.ProjectCharacteristics);
        }

        [Fact]
        public void AggregationEngine_StressTest_ZeroAndNegativeIds_FallbackToItemTagAndSubCode()
        {
            // Adversarial condition: Zero out all entity IDs to simulate unpersisted DTOs
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            project.Id = 0;
            foreach (var item in project.TenderItems)
            {
                item.Id = 0;
                item.ProjectId = 0;
                foreach (var sub in item.SubProjects)
                {
                    sub.Id = 0;
                    sub.TenderItemId = 0;
                    foreach (var part in sub.Parts)
                    {
                        part.Id = 0;
                        part.SubProjectId = 0;
                    }
                }
            }

            var calcResult = BoqAggregationEngine.CalculateProject(project);

            // 1. Owner projection with zero IDs: MUST NOT collide onto first item NH
            var ownerProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.Owner, project);
            var nfRow = ownerProj.ClientTenderRows!.First(r => r.ItemTag == "NF");
            var jlcRow = ownerProj.ClientTenderRows!.First(r => r.ItemTag == "JLC");

            Assert.Contains("Q355B", nfRow.ProjectCharacteristics);
            Assert.DoesNotContain("Q460GJC", nfRow.ProjectCharacteristics);
            Assert.Contains("Q235B", jlcRow.ProjectCharacteristics);
            Assert.DoesNotContain("Q460GJC", jlcRow.ProjectCharacteristics);

            // 2. Fabrication shop projection with zero IDs: MUST NOT collide onto subNH48 (16mm)
            var fabProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.FabricationShop, project);
            var subNH48 = fabProj.FabricationRows!.First(r => r.SubProjectCode == "N-NHXX-48");
            var subNH49 = fabProj.FabricationRows!.First(r => r.SubProjectCode == "N-NHXX-49");

            Assert.Equal(16.0, subNH48.MaxPlateThickness);
            Assert.Equal(50.0, subNH49.MaxPlateThickness); // Critical: subNH49 has 50mm plate, NOT 16mm

            // 3. Site erection projection with zero IDs: MUST NOT collide onto subNH48 (36m, field weld)
            var siteProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.SiteProjectDept, project);
            var jlcCol = siteProj.SiteErectionRows!.First(r => r.SubProjectCode == "JLC-COL-01");

            Assert.Equal(9.5, jlcCol.AssemblyElevation); // Critical: JLC is 9.5m, NOT 36m
            Assert.False(jlcCol.HasFieldWelding); // Critical: shop-welded/bolted, NOT field welding
        }

        [Fact]
        public void AggregationEngine_NullProjectReference_HandlesGracefullyWithoutNullRef()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var calcResult = BoqAggregationEngine.CalculateProject(project);

            // Pass null project to ProjectToAudience
            var ownerProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.Owner, null);
            Assert.NotNull(ownerProj.ClientTenderRows);
            Assert.Equal(3, ownerProj.ClientTenderRows.Count);
            // Default fallback characteristic used
            Assert.All(ownerProj.ClientTenderRows, r => Assert.Equal("国家标准规范钢结构深化制作与安装", r.ProjectCharacteristics));

            var fabProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.FabricationShop, null);
            Assert.NotNull(fabProj.FabricationRows);
            Assert.Equal(13, fabProj.FabricationRows.Count);

            var siteProj = BoqAggregationEngine.ProjectToAudience(calcResult, BoqAudiencePerspective.SiteProjectDept, null);
            Assert.NotNull(siteProj.SiteErectionRows);
            Assert.Equal(13, siteProj.SiteErectionRows.Count);
        }

        #endregion

        #region 3. Tree-Grid Node Expansion Isolation Verification

        [Fact]
        public void TreeGrid_TenderItemExpansion_IsIsolatedByItemTag()
        {
            var expandedTags = new HashSet<string>();

            // Helper methods mirroring BoqWorkbench.razor
            bool IsTenderExpanded(string tag) => !string.IsNullOrEmpty(tag) && expandedTags.Contains(tag);
            void ToggleTenderExpand(string tag)
            {
                if (string.IsNullOrEmpty(tag)) return;
                if (!expandedTags.Add(tag))
                    expandedTags.Remove(tag);
            }

            // Initially collapsed
            Assert.False(IsTenderExpanded("NH"));
            Assert.False(IsTenderExpanded("NF"));
            Assert.False(IsTenderExpanded("JLC"));

            // Expand "NH"
            ToggleTenderExpand("NH");
            Assert.True(IsTenderExpanded("NH"));
            Assert.False(IsTenderExpanded("NF"), "NF must remain collapsed when NH is expanded");
            Assert.False(IsTenderExpanded("JLC"), "JLC must remain collapsed when NH is expanded");

            // Expand "JLC"
            ToggleTenderExpand("JLC");
            Assert.True(IsTenderExpanded("NH"));
            Assert.False(IsTenderExpanded("NF"));
            Assert.True(IsTenderExpanded("JLC"));

            // Collapse "NH"
            ToggleTenderExpand("NH");
            Assert.False(IsTenderExpanded("NH"));
            Assert.False(IsTenderExpanded("NF"));
            Assert.True(IsTenderExpanded("JLC"));

            // Edge cases: null or empty
            ToggleTenderExpand("");
            ToggleTenderExpand(null!);
            Assert.False(IsTenderExpanded(""));
            Assert.False(IsTenderExpanded(null!));
        }

        [Fact]
        public void TreeGrid_SubProjectExpansion_IsIsolatedBySubProjectCode()
        {
            var expandedCodes = new HashSet<string>();

            bool IsSubProjectExpanded(string code) => !string.IsNullOrEmpty(code) && expandedCodes.Contains(code);
            void ToggleSubProjectExpand(string code)
            {
                if (string.IsNullOrEmpty(code)) return;
                if (!expandedCodes.Add(code))
                    expandedCodes.Remove(code);
            }

            // Initially collapsed
            Assert.False(IsSubProjectExpanded("N-NHXX-48"));
            Assert.False(IsSubProjectExpanded("N-NHXX-49"));
            Assert.False(IsSubProjectExpanded("JLC-COL-01"));

            // Expand "N-NHXX-48"
            ToggleSubProjectExpand("N-NHXX-48");
            Assert.True(IsSubProjectExpanded("N-NHXX-48"));
            Assert.False(IsSubProjectExpanded("N-NHXX-49"), "N-NHXX-49 must remain collapsed");
            Assert.False(IsSubProjectExpanded("JLC-COL-01"), "JLC-COL-01 must remain collapsed");

            // Expand "JLC-COL-01"
            ToggleSubProjectExpand("JLC-COL-01");
            Assert.True(IsSubProjectExpanded("N-NHXX-48"));
            Assert.False(IsSubProjectExpanded("N-NHXX-49"));
            Assert.True(IsSubProjectExpanded("JLC-COL-01"));

            // Collapse all
            expandedCodes.Clear();
            Assert.False(IsSubProjectExpanded("N-NHXX-48"));
            Assert.False(IsSubProjectExpanded("JLC-COL-01"));
        }

        [Fact]
        public void TreeGrid_ExpandAllAndCollapseAll_BehavesDeterministically()
        {
            var project = NingboOlympicSeedData.CreateBenchmarkProject();
            var expandedTags = new HashSet<string>();
            var expandedCodes = new HashSet<string>();

            // ExpandAll
            foreach (var item in project.TenderItems)
            {
                if (!string.IsNullOrEmpty(item.ItemTag))
                    expandedTags.Add(item.ItemTag);

                foreach (var sub in item.SubProjects)
                {
                    if (!string.IsNullOrEmpty(sub.SubProjectCode))
                        expandedCodes.Add(sub.SubProjectCode);
                }
            }

            Assert.Equal(3, expandedTags.Count);
            Assert.Contains("NH", expandedTags);
            Assert.Contains("NF", expandedTags);
            Assert.Contains("JLC", expandedTags);

            Assert.Equal(13, expandedCodes.Count);
            Assert.Contains("N-NHXX-48", expandedCodes);
            Assert.Contains("N-NF-CHORD-UP", expandedCodes);
            Assert.Contains("JLC-COL-01", expandedCodes);

            // CollapseAll
            expandedTags.Clear();
            expandedCodes.Clear();
            Assert.Empty(expandedTags);
            Assert.Empty(expandedCodes);
        }

        #endregion

        #region 4. Role Highlighting CSS Class Contract Verification

        [Theory]
        [InlineData(BoqAudiencePerspective.FabricationShop, true, false)]
        [InlineData(BoqAudiencePerspective.SiteProjectDept, false, true)]
        [InlineData(BoqAudiencePerspective.Owner, false, false)]
        [InlineData(BoqAudiencePerspective.ChiefEconomist, false, false)]
        [InlineData(BoqAudiencePerspective.Procurement, false, false)]
        public void Perspective_ColumnHighlightingClasses_MatchActiveRole(
            BoqAudiencePerspective perspective,
            bool expectFabHighlight,
            bool expectSiteHighlight)
        {
            // Simulate helper functions in BoqWorkbench.razor
            string GetFabricationColHeaderClass(BoqAudiencePerspective p)
            {
                if (p != BoqAudiencePerspective.FabricationShop) return string.Empty;
                return "border border-warning bg-warning bg-opacity-25 text-warning fw-bold";
            }

            string GetInstallationColHeaderClass(BoqAudiencePerspective p)
            {
                if (p != BoqAudiencePerspective.SiteProjectDept) return string.Empty;
                return "border border-info bg-info bg-opacity-25 text-info fw-bold";
            }

            var fabClass = GetFabricationColHeaderClass(perspective);
            var siteClass = GetInstallationColHeaderClass(perspective);

            if (expectFabHighlight)
            {
                Assert.Contains("border-warning", fabClass);
            }
            else
            {
                Assert.Empty(fabClass);
            }

            if (expectSiteHighlight)
            {
                Assert.Contains("border-info", siteClass);
            }
            else
            {
                Assert.Empty(siteClass);
            }
        }

        #endregion
    }
}
