# Project: Steel BOQ Database Architecture & Multi-Audience Reporting System

## Architecture
- **Domain & Algorithms**: `src/EngineeringApp.Shared/BOQ/`
  - `Models/BoqHierarchyModels.cs`: 6 core entity models with navigation properties and fee rate override logic. [COMPLETED - M1]
  - `Engine/BoqAggregationEngine.cs`: Hierarchical bottom-up calculation engine computing material, fabrication, installation, coating, and multi-audience projections. [COMPLETED - M2]
  - `Export/BoqExcelExporter.cs`: Excel-compatible multi-sheet CSV/XML/HTML workbook generator. [COMPLETED - M2]
  - `Data/NingboOlympicSeedData.cs`: In-memory baseline benchmark generator for Ningbo Olympic Center. [M3]
- **Persistence Layer**: `src/EngineeringApp.Server/`
  - `Data/AppDbContext.cs`: 6 BOQ entity DbSets registered with SQLite indices and foreign key cascades. [COMPLETED - M1]
  - `Data/NingboOlympicDataSeeder.cs`: Database initialization seeder injecting Ningbo Olympic Center benchmark records. [M3]
- **UI Presentation Layer**: `src/EngineeringApp.Client/`
  - `Pages/BoqWorkbench.razor`: Multi-audience perspective toolbar (Owner, Chief Economist, Fabrication Shop, Site Erection, Material Procurement), 6 KPI summary cards, 4 interactive tabs, component tree-grid. [M4]
  - `Shared/NavMenu.razor` & `Pages/WorkbenchPortal.razor`: Sidebar and portal entry links. [M4]
- **Testing & Verification**: `src/EngineeringApp.Tests/`
  - `BoqHierarchyAndAggregationTests.cs`: Comprehensive unit and regression test suite verifying 1,123,939.15 kg total net weight, 5,334.63 m² surface area, 1.15 purchasing waste multiplier, quota scaling factors, fee rate overrides, and multi-audience projections. [M5]

---

## Feature Inventory
| # | Feature | Description | Milestone | Source | Status |
|---|---------|-------------|-----------|--------|--------|
| 1 | BoqProject Model | Project root entity with global default management fee (8%) and profit rate (7%) | M1 | Plan §2, §6.1 | DONE |
| 2 | BoqTenderItem Model | Tender item entity with custom rate override support (`IsCustomRateEnabled`, `CustomManagementFeeRate`, `CustomProfitRate`) | M1 | Plan §2, §3.4 | DONE |
| 3 | BoqSubProject Model | Component costing entity linking fabrication & installation quota rules, fireproofing/coating specs | M1 | Plan §2, §3.2-3.3 | DONE |
| 4 | BoqPart Model | Atomic leaf part model with dimensions, density, net weight, 1.15 waste factor, material pricing | M1 | Plan §2, §3.1 | DONE |
| 5 | FabricationQuotaRule Model | Fabrication quota rules entity with base labor, intersecting cut, UT testing, preassembly fees | M1 | Plan §2, §3.2 | DONE |
| 6 | InstallationQuotaRule Model | Installation quota rules entity with base labor, crane tiers, scaffolding fees | M1 | Plan §2, §3.3 | DONE |
| 7 | AppDbContext 6 DbSets | Register 6 BOQ DbSets in Server AppDbContext with foreign keys, cascades, and SQLite indices | M1 | Plan §2, §6.2 | DONE |
| 8 | Part Material Pricing & 1.15 Multiplier | Bottom-up part material costing multiplying base price + grade/thickness/Z-markups by 1.15 gross waste factor | M2 | Plan §3.1, Request R2 | DONE |
| 9 | Thickness Ladder & Special Adders | Support for plate thickness tiers ($>40$mm +350/t, etc.) and Z15/Z25/Z35 z-direction tearing resistance | M2 | Plan §3.1 | DONE |
| 10 | Fabrication Cost & $K_{\text{Thick}}$ Factor | Component fabrication costing with $K_{\text{Thick}}$ factor (1.00~1.40), intersecting cuts, UT inspection, preassembly | M2 | Plan §3.2, Request R2 | DONE |
| 11 | Installation Cost & Multi-Factor Model | Installation costing with $K_{\text{Elev}}$ (1.00~1.30), $K_{\text{SiteWeld}}$ (1.30), crane tonnage tiers $K_{\text{Weight}}$ (1.00~1.45) | M2 | Plan §3.3, Request R2 | DONE |
| 12 | Fireproofing & Coating Schedule | Fireproofing schedule mapping (3.0H/2.5H/1.5H) and surface area coating cost evaluation | M2 | Plan §3, §3.3 | DONE |
| 13 | Scissor Difference (采办经营创效) | Material procurement profit calculation: Audited Guidance Price vs Actual Procurement Price $\times$ 1.15 Gross Weight | M2 | Plan §3.1, AGENTS.md | DONE |
| 14 | Bottom-Up Convolution | Bottom-up convolution rolling up part materials, fabrication, erection, and coating into component direct cost | M2 | Plan §3.4 | DONE |
| 15 | Fee Rate Evaluation & Tender Pricing | Two-tier fee rate evaluation (custom override vs project default) to compute final comprehensive unit price and total bid | M2 | Plan §3.4, Request R1 | DONE |
| 16 | Owner/Audit Projection | `ClientTenderMaster` projection masking internal direct costs, margins, and quota details | M2 | Plan §4.1 | DONE |
| 17 | Chief Economist Projection | `UltimateCostAnalysis` projection providing full penetration into all-element costs, margins, and scissor profits | M2 | Plan §4.1 | DONE |
| 18 | Fabrication Workshop Projection | `FabricationWorkshop` projection detailing $K_{\text{Thick}}$, cutting, UT testing, and fabrication labor | M2 | Plan §4.1 | DONE |
| 19 | Site Erection Projection | `SiteErectionSchedule` projection focusing on elevation, field welding, crane tonnage tiers, and erection costs | M2 | Plan §4.1 | DONE |
| 20 | Material Procurement Projection | `MaterialProcurementOrder` projection grouping raw materials by grade, thickness, ordering weights, and budgets | M2 | Plan §4.1 | DONE |
| 21 | Excel Multi-Perspective Exporter | `BoqExcelExporter.cs` generating multi-sheet structured Excel workbooks matching each perspective's masking rules | M2 | Plan §6.1, Request R2 | DONE |
| 22 | Shared Ningbo Olympic Benchmark Data | `NingboOlympicSeedData.cs` providing in-memory baseline benchmark dataset (NH 1,123.94t, NF 865.20t, JLC 728.30t) | M3 | Plan §2, §6.2 | DONE |
| 23 | Server Database Seeder | `NingboOlympicDataSeeder.cs` idempotent seeding in Server `AppDbContext.SeedData` | M3 | Plan §2, §6.2, Request R3 | DONE |
| 24 | Blazor WASM BoqWorkbench UI | `BoqWorkbench.razor` with 5-audience toolbar, 6 KPI cards, 4 interactive tabs, and tree-grid component | M4 | Plan §6.3, Request R4 | PLANNED |
| 25 | Interactive Fee Rate Toggle & Tree-Grid | In-browser dynamic fee rate override toggling and component parts hierarchy inspection | M4 | Plan §6.3 | PLANNED |
| 26 | Navigation Integration | Adding navigation items in `NavMenu.razor` and `WorkbenchPortal.razor` | M4 | Plan §6.3, Request R4 | PLANNED |
| 27 | Comprehensive Unit & E2E Tests | `BoqHierarchyAndAggregationTests.cs` verifying 1,123,939.15 kg total net weight, 5,334.63 m² coating area, 1.15 multiplier, quota factors, and fee overrides | M5 | Plan §6.4, §7.1, Request R6 | PLANNED |

---

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Decoupled BOQ Domain Models & Persistence | Implement `BoqHierarchyModels.cs` in Shared and register 6 DbSets with EF Core SQLite mapping in Server `AppDbContext.cs` | none | DONE |
| M2 | Hierarchical Calculation, Aggregation & Excel Engine | Implement `BoqAggregationEngine.cs` and `BoqExcelExporter.cs` in Shared | M1 | DONE |
| M3 | Real Engineering Seed Data | Implement `NingboOlympicSeedData.cs` in Shared and `NingboOlympicDataSeeder.cs` in Server | M1, M2 | DONE |
| M4 | Blazor WebAssembly Workbench UI | Implement `BoqWorkbench.razor`, update `NavMenu.razor` and `WorkbenchPortal.razor` | M1, M2, M3 | IN_PROGRESS |
| M5 | Final Verification & Adversarial Coverage Hardening | Execute 100% E2E tests, verify benchmark numbers (1,123,939.15 kg, 5,334.63 m²), adversarial hardening | M4, E2E Track | PLANNED |

---

## Interface Contracts

### M1 ↔ M2 / M3 / M4 / M5: Domain Models [IMPLEMENTED & VERIFIED]
- `BoqProject`: `Id`, `ProjectCode`, `ProjectName`, `ClientName`, `DefaultManagementFeeRate` (double, 0.08), `DefaultProfitRate` (double, 0.07), `List<BoqTenderItem> TenderItems`
- `BoqTenderItem`: `Id`, `ProjectId`, `ItemTag`, `BillItemCode`, `BillItemName`, `ProjectCharacteristics`, `Unit`, `TenderQuantity`, `DirectCostPerTon`, `IsCustomRateEnabled`, `CustomManagementFeeRate`, `CustomProfitRate`, `List<BoqSubProject> SubProjects`
  - Method `(double mgmtRate, double profitRate) GetEffectiveRates(BoqProject? project)`
- `BoqSubProject`: `Id`, `TenderItemId`, `SubProjectCode`, `SubProjectName`, `StructuralRole`, `InstallationMethod`, `MaxPlateThickness`, `AssemblyElevation`, `SingleWeight`, `Quantity`, `HasFieldWelding`, `CraneTonnageTier`, `FireRatingHours`, `CoatingAreaPerUnit`, `FabricationQuotaRuleId`, `InstallationQuotaRuleId`, `FabricationQuotaRule`, `InstallationQuotaRule`, `List<BoqPart> Parts`
- `BoqPart`: `Id`, `SubProjectId`, `PartMark`, `SectionSpecification`, `SteelGrade`, `Thickness`, `Length`, `Width`, `LinearDensity`, `Quantity`, `SingleNetWeight`, `TotalNetWeight`, `PurchasingWasteFactor` (double, 1.15), `TotalGrossWeight`, `TotalGrossWeightKg`, `LossRate`, `ZDirectionGrade`, `MaterialUnitPricePerTon`
- `FabricationQuotaRule`: `Id`, `QuotaCode`, `StructuralSystem`, `BaseLaborDays`, `BaseLaborRatePerTon`, `IntersectingCuttingUnitRate`, `UltrasonicTestingUnitRate`, `PreassemblyUnitRate`
- `InstallationQuotaRule`: `Id`, `QuotaCode`, `ApplicableRole`, `BaseLaborDays`, `BaseLaborRatePerTon`, `MainCraneRatePerTon`, `TrestlePlatformRatePerTon`

### M2 Engine Contract [IMPLEMENTED & VERIFIED]
- `BoqAggregationEngine.CalculateSubProject(BoqSubProject subProject, BoqTenderItem? parentItem = null, BoqProject? project = null)`: Returns `SubProjectCostResult`
- `BoqAggregationEngine.CalculateTenderItem(BoqTenderItem item, BoqProject project)`: Returns `TenderItemCostResult`
- `BoqAggregationEngine.CalculateProject(BoqProject project)`: Returns `ProjectCostResult`
- `BoqAggregationEngine.ProjectToAudience(ProjectCostResult result, BoqAudiencePerspective perspective)`: Returns `AudienceProjectionResult`
- `BoqAggregationEngine.GenerateAudienceReport(BoqProject project, BoqAudiencePerspective perspective, BoqReportConfig? config = null)`: Returns `AudienceProjectionResult`
- `BoqExcelExporter.ExportToCsv(AudienceProjectionResult projection)` & `ExportToExcelXml(AudienceProjectionResult projection)`: Returns byte[] or string content
- `BoqExcelExporter.ExportMultiPerspectiveXmlSpreadsheet(ProjectCostResult projectResult)`: Returns 5-sheet workbook byte[] or string

---

## Code Layout
- `src/EngineeringApp.Shared/BOQ/Models/BoqHierarchyModels.cs` (Owned by M1 - COMPLETED)
- `src/EngineeringApp.Shared/GlobalUsings.cs` (Owned by M1 - COMPLETED)
- `src/EngineeringApp.Server/Data/AppDbContext.cs` (Owned by M1 - COMPLETED)
- `src/EngineeringApp.Shared/BOQ/Engine/BoqAggregationEngine.cs` (Owned by M2 - COMPLETED)
- `src/EngineeringApp.Shared/BOQ/Export/BoqExcelExporter.cs` (Owned by M2 - COMPLETED)
- `src/EngineeringApp.Shared/BOQ/Data/NingboOlympicSeedData.cs` (Owned by M3)
- `src/EngineeringApp.Server/Data/NingboOlympicDataSeeder.cs` (Owned by M3)
- `src/EngineeringApp.Client/Pages/BoqWorkbench.razor` (Owned by M4)
- `src/EngineeringApp.Client/Shared/NavMenu.razor` (Owned by M4)
- `src/EngineeringApp.Client/Pages/WorkbenchPortal.razor` (Owned by M4)
- `src/EngineeringApp.Tests/BoqHierarchyAndAggregationTests.cs` (Owned by E2E Testing Track / M5)
