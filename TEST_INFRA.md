# E2E Test Infra: Steel BOQ Database & Multi-Audience Reporting System

## Test Philosophy
- Opaque-box, requirement-driven. Derived from `ORIGINAL_REQUEST.md`, `doc/steel_boq_database_and_reporting_plan.md`, and `AGENTS.md`.
- Methodology: Category-Partition + Boundary Value Analysis (BVA) + Pairwise Combinations + Real-World Workloads.

## Feature Inventory Mapping
| # | Feature | Source (Requirement) | Tier 1 (Coverage) | Tier 2 (Boundary) | Tier 3 (Pairwise) | Tier 4 (Workload) |
|---|---------|----------------------|:-----------------:|:-----------------:|:-----------------:|:-----------------:|
| 1 | Domain Entity Hierarchy | R1 / Plan §2 | >= 5 | >= 5 | ✓ | ✓ |
| 2 | Two-Tier Fee Rate Overrides | R1 / Plan §3.4 | >= 5 | >= 5 | ✓ | ✓ |
| 3 | Material Pricing & 1.15 Multiplier | R2 / Plan §3.1 | >= 5 | >= 5 | ✓ | ✓ |
| 4 | Fabrication $K_{\text{Thick}}$ & Quotas | R2 / Plan §3.2 | >= 5 | >= 5 | ✓ | ✓ |
| 5 | Installation $K_{\text{Elev}}$, $K_{\text{SiteWeld}}$, Crane Tiers | R2 / Plan §3.3 | >= 5 | >= 5 | ✓ | ✓ |
| 6 | Coating & Fireproofing Scheduling | R2 / Plan §3.3 | >= 5 | >= 5 | ✓ | ✓ |
| 7 | Bottom-up Convolution & Pricing | R2 / Plan §3.4 | >= 5 | >= 5 | ✓ | ✓ |
| 8 | Multi-Audience Projections (5 views) | R2 / Plan §4.1 | >= 5 | >= 5 | ✓ | ✓ |
| 9 | Excel Multi-Perspective Export | R2 / Plan §6.1 | >= 5 | >= 5 | ✓ | ✓ |
| 10 | Ningbo Olympic Benchmark Calibration | R3, R6 / Plan §6.2 | >= 5 | >= 5 | ✓ | ✓ |
| 11 | EF Core SQLite Persistence | R3 / Plan §6.2 | >= 5 | >= 5 | ✓ | ✓ |

## Test Architecture
- Test Framework: xUnit (.NET 9.0) in `src/EngineeringApp.Tests/`
- Target Test Suite: `src/EngineeringApp.Tests/BoqHierarchyAndAggregationTests.cs`
- Test Runner: `dotnet test --filter FullyQualifiedName~Boq`
- Expected: All tests pass with 0 failures, 0 warnings.

## Real-World Application Scenarios (Tier 4)
| # | Scenario | Features Exercised | Target Benchmark Values |
|---|----------|--------------------|-------------------------|
| 1 | Ningbo Olympic Main Truss (NH) Bottom-Up Rollup | F1-F8, F10 | Net Weight = 1,123,939.15 kg, Coating Area = 5,334.63 m², Waste Factor = 1.15 |
| 2 | Ningbo Secondary Truss (NF) Project Default Rates | F1, F2, F7, F8 | Inherits 8% Management, 7% Profit, gross weight 1.15x |
| 3 | Ningbo Check-in Center (JLC) Light Steel Structure | F1, F3, F7, F8 | 728.30 t target with profile and plate convolution |
| 4 | Multi-Audience Masking Security Verification | F8, F9 | Client/Audit view hides internal direct costs and margins |
| 5 | Extreme Geometry & Quota Scaling Verification | F4, F5, F6 | Max thickness > 60mm ($K_{\text{Thick}}=1.40$), Elevation > 35m ($K_{\text{Elev}}=1.30$), Field Welding ($K_{\text{SiteWeld}}=1.30$), Weight > 25t ($K_{\text{Weight}}=1.45$) |

## Coverage Thresholds
- Tier 1: >= 5 test cases per core feature (Happy path)
- Tier 2: >= 5 test cases per core feature (Edge cases, negative values, 0 quantities, boundary limits)
- Tier 3: Pairwise combinations of fee rate overrides, quota factors, and structural roles
- Tier 4: Realistic benchmark applications (Ningbo NH, NF, JLC, Full Project)
