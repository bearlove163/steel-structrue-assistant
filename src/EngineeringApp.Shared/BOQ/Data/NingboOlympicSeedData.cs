// ============================================================================
// 文件归属: src/EngineeringApp.Shared/BOQ/Data/NingboOlympicSeedData.cs
// 模块功能: 宁波奥林匹克体育中心主体育场与配套工程 BOQ 基准算量数据集生成器
// 规范依据: ORIGINAL_REQUEST.md (R3, R6), PROJECT.md (Feature 22), AGENTS.md
// ============================================================================

using System;
using System.Collections.Generic;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Shared.BOQ.Data
{
    /// <summary>
    /// 宁波奥体中心基准工程量清单与定额基准数据源 (单一事实来源 SSOT)
    /// </summary>
    public static class NingboOlympicSeedData
    {
        // 核心基准常量
        public const string BenchmarkProjectCode = "NB-ATZX";
        public const string BenchmarkProjectName = "宁波奥林匹克体育中心主体育场与配套工程";
        public const string BenchmarkClientName = "宁波市奥林匹克体育中心投资建设有限公司 / 城投指挥部";

        public const double DefaultManagementFeeRate = 0.08; // 8% 全局默认管理费率
        public const double DefaultProfitRate = 0.07;         // 7% 全局默认利润率

        public const double BenchmarkWasteFactor = 1.15;
        public const double BenchmarkPurchasingWasteFactor = 1.15;

        // 南侧主桁架 (NH) 基准指标
        public const double BenchmarkNhNetWeightKg = 1123939.15;
        public const double BenchmarkNhSurfaceAreaM2 = 5334.63;
        public const double BenchmarkNhGrossWeightKg = 1292530.02; // 1,123,939.15 * 1.15 = 1,292,530.0225 => rounds to 1,292,530.02
        public const double BenchmarkNhTenderQuantityTon = 1123.94;
        public const double BenchmarkNhCustomMgmtRate = 0.10;
        public const double BenchmarkNhCustomProfitRate = 0.12;
        public const double BenchmarkNhFireRatingHours = 3.0;

        // 南侧副桁架 (NF) 基准指标
        public const double BenchmarkNfNetWeightKg = 865200.00;
        public const double BenchmarkNfTenderQuantityTon = 865.20;
        public const double BenchmarkNfGrossWeightKg = 994980.00;
        public const double BenchmarkNfSurfaceAreaM2 = 4720.00;
        public const double BenchmarkNfFireRatingHours = 2.0;

        // 第一检录处 (JLC) 基准指标
        public const double BenchmarkJlcNetWeightKg = 728300.00;
        public const double BenchmarkJlcTenderQuantityTon = 728.30;
        public const double BenchmarkJlcGrossWeightKg = 837545.00;
        public const double BenchmarkJlcSurfaceAreaM2 = 7850.00;
        public const double BenchmarkJlcCustomMgmtRate = 0.05;
        public const double BenchmarkJlcCustomProfitRate = 0.04;
        public const double BenchmarkJlcFireRatingHours = 1.5;

        /// <summary>
        /// 获取标准车间制作定额规则字典
        /// </summary>
        public static List<FabricationQuotaRule> GetDefaultFabricationQuotaRules()
        {
            return new List<FabricationQuotaRule>
            {
                new FabricationQuotaRule
                {
                    QuotaCode = "FAB_SPACE_TRUSS_MAIN",
                    QuotaName = "大跨空间管桁架主肢车间制作",
                    StructuralSystem = "空间管桁架",
                    BaseLaborDays = 1.8,
                    BaseLaborRatePerTon = 2200.0,
                    IntersectingCuttingUnitRate = 280.0,
                    UltrasonicTestingUnitRate = 160.0,
                    PreassemblyUnitRate = 200.0
                },
                new FabricationQuotaRule
                {
                    QuotaCode = "FAB_SPACE_TRUSS_SUB",
                    QuotaName = "大跨空间副桁架车间制作",
                    StructuralSystem = "副桁架",
                    BaseLaborDays = 1.5,
                    BaseLaborRatePerTon = 1600.0,
                    IntersectingCuttingUnitRate = 200.0,
                    UltrasonicTestingUnitRate = 120.0,
                    PreassemblyUnitRate = 150.0
                },
                new FabricationQuotaRule
                {
                    QuotaCode = "FAB_LIGHT_STEEL",
                    QuotaName = "轻型门式刚架车间制作",
                    StructuralSystem = "轻钢门式刚架",
                    BaseLaborDays = 1.0,
                    BaseLaborRatePerTon = 1100.0,
                    IntersectingCuttingUnitRate = 0.0,
                    UltrasonicTestingUnitRate = 80.0,
                    PreassemblyUnitRate = 100.0
                }
            };
        }

        public static List<FabricationQuotaRule> GetDefaultFabricationRules() => GetDefaultFabricationQuotaRules();

        /// <summary>
        /// 获取标准现场安装定额规则字典
        /// </summary>
        public static List<InstallationQuotaRule> GetDefaultInstallationQuotaRules()
        {
            return new List<InstallationQuotaRule>
            {
                new InstallationQuotaRule
                {
                    QuotaCode = "INS_SPACE_TRUSS_H30M",
                    QuotaName = "空间管桁架高空原位散装全熔透焊",
                    ApplicableRole = "主桁架",
                    BaseLaborDays = 2.8,
                    BaseLaborRatePerTon = 2100.0,
                    MainCraneRatePerTon = 850.0,
                    TrestlePlatformRatePerTon = 350.0,
                    ElevationSurchargeFactor = 1.0
                },
                new InstallationQuotaRule
                {
                    QuotaCode = "INS_SPACE_TRUSS_SUB",
                    QuotaName = "空间副桁架厂内拼装整体吊装",
                    ApplicableRole = "副桁架",
                    BaseLaborDays = 1.8,
                    BaseLaborRatePerTon = 1400.0,
                    MainCraneRatePerTon = 500.0,
                    TrestlePlatformRatePerTon = 200.0,
                    ElevationSurchargeFactor = 1.0
                },
                new InstallationQuotaRule
                {
                    QuotaCode = "INS_LIGHT_STEEL",
                    QuotaName = "轻型钢结构汽车吊装配安装",
                    ApplicableRole = "轻钢刚架",
                    BaseLaborDays = 1.2,
                    BaseLaborRatePerTon = 900.0,
                    MainCraneRatePerTon = 350.0,
                    TrestlePlatformRatePerTon = 150.0,
                    ElevationSurchargeFactor = 1.0
                }
            };
        }

        public static List<InstallationQuotaRule> GetDefaultInstallationRules() => GetDefaultInstallationQuotaRules();

        /// <summary>
        /// 创建并水合完整的宁波奥体中心基准工程大盘模型 (包含 3 大清单项、13 个构件、18 个零件，并已执行自底向上卷积核算)
        /// </summary>
        public static BoqProject CreateBenchmarkProject()
        {
            var fabRules = GetDefaultFabricationQuotaRules();
            var insRules = GetDefaultInstallationQuotaRules();

            var fabMain = fabRules[0];
            var fabSub = fabRules[1];
            var fabLight = fabRules[2];

            var insHigh = insRules[0];
            var insSub = insRules[1];
            var insLight = insRules[2];

            var project = new BoqProject
            {
                Id = 1,
                ProjectCode = BenchmarkProjectCode,
                ProjectName = BenchmarkProjectName,
                ClientName = BenchmarkClientName,
                DefaultManagementFeeRate = DefaultManagementFeeRate,
                DefaultProfitRate = DefaultProfitRate,
                ActiveMaterialSnapshotId = "SNAP-2026-Q1",
                ActivePaintSnapshotId = "PAINT-2026-Q1"
            };

            // ========================================================================
            // 1. 清单项 NH: 主体育场南侧大跨主桁架制作安装 (开启单项自定义费率: 10% 管理费, 12% 利润)
            // ========================================================================
            var itemNH = new BoqTenderItem
            {
                Id = 1,
                ProjectId = 1,
                Project = project,
                ItemTag = "NH",
                BillItemCode = "010101001001",
                BillItemName = "南侧大跨主桁架制作安装",
                ProjectCharacteristics = "高强结构钢Q460GJC-Z15/Z25/Z35、大管径1200x50、3.0H厚型防火涂装、现场高空原位散装",
                Unit = "t",
                TenderQuantity = BenchmarkNhTenderQuantityTon,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = BenchmarkNhCustomMgmtRate,
                CustomProfitRate = BenchmarkNhCustomProfitRate
            };

            // NH - 构件 1: N-NHXX-48 (空间相贯节点段, 净重 322.70 kg, 表面积 4.24 m2)
            var subNH48 = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NHXX-48",
                SubProjectName = "主桁架空间相贯节点段",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 16.0,
                AssemblyElevation = 36.0,
                SingleWeight = 0.3227,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "<5t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNH48.Parts.Add(new BoqPart
            {
                SubProject = subNH48,
                PartMark = "HLP-48-1",
                SectionSpecification = "PIP219*10",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 10.0,
                DiameterOrWidthMm = 219.0,
                Length = 2400.0,
                Quantity = 2,
                SingleNetWeight = 123.60,
                TotalNetWeight = 247.20,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 247.20 * 1.15,
                SingleSurfaceAreaM2 = 1.65,
                TotalSurfaceAreaM2 = 3.30,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            subNH48.Parts.Add(new BoqPart
            {
                SubProject = subNH48,
                PartMark = "HLP-48-2",
                SectionSpecification = "PL16*250",
                SectionType = "PL",
                SteelGrade = "Q355B",
                Thickness = 16.0,
                DiameterOrWidthMm = 250.0,
                Length = 600.0,
                Quantity = 4,
                SingleNetWeight = 18.875,
                TotalNetWeight = 75.50,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 75.50 * 1.15,
                SingleSurfaceAreaM2 = 0.235,
                TotalSurfaceAreaM2 = 0.94,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            itemNH.SubProjects.Add(subNH48);

            // NH - 构件 2: N-NHXX-49 (特厚下弦主肢大管段, 净重 12,850.00 kg, 表面积 40.33 m2)
            var subNH49 = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NHXX-49",
                SubProjectName = "特厚下弦主肢大管段",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 50.0,
                AssemblyElevation = 38.5,
                SingleWeight = 12.85,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "5-15t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNH49.Parts.Add(new BoqPart
            {
                SubProject = subNH49,
                PartMark = "HLP-15",
                SectionSpecification = "PIP1200*50",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 1200.0,
                Length = 9000.0,
                Quantity = 1,
                SingleNetWeight = 11350.00,
                TotalNetWeight = 11350.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 11350.00 * 1.15,
                SingleSurfaceAreaM2 = 33.93,
                TotalSurfaceAreaM2 = 33.93,
                ZDirectionGrade = "Z15",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 150.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6050.0,
                ActualPurchasePricePerTon = 5550.0
            });
            subNH49.Parts.Add(new BoqPart
            {
                SubProject = subNH49,
                PartMark = "HLP-144",
                SectionSpecification = "PL50*1200",
                SectionType = "PL",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 1200.0,
                Length = 1000.0,
                Quantity = 4,
                SingleNetWeight = 375.00,
                TotalNetWeight = 1500.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 1500.00 * 1.15,
                SingleSurfaceAreaM2 = 1.60,
                TotalSurfaceAreaM2 = 6.40,
                ZDirectionGrade = "Z15",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 150.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6050.0,
                ActualPurchasePricePerTon = 5550.0
            });
            itemNH.SubProjects.Add(subNH49);

            // NH - 构件 3: N-NHXX-50 (空间主弦杆对接段, 净重 4,170.00 kg, 表面积 14.60 m2)
            var subNH50 = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NHXX-50",
                SubProjectName = "空间主弦杆对接段",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 40.0,
                AssemblyElevation = 42.0,
                SingleWeight = 4.17,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "<5t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNH50.Parts.Add(new BoqPart
            {
                SubProject = subNH50,
                PartMark = "HLP-50-1",
                SectionSpecification = "PIP1000*35",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 35.0,
                DiameterOrWidthMm = 1000.0,
                Length = 3800.0,
                Quantity = 1,
                SingleNetWeight = 3596.00,
                TotalNetWeight = 3596.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 3596.00 * 1.15,
                SingleSurfaceAreaM2 = 11.94,
                TotalSurfaceAreaM2 = 11.94,
                ZDirectionGrade = "Z15",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 250.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 150.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6050.0,
                ActualPurchasePricePerTon = 5550.0
            });
            subNH50.Parts.Add(new BoqPart
            {
                SubProject = subNH50,
                PartMark = "HLP-50-2",
                SectionSpecification = "PL30*920",
                SectionType = "PL",
                SteelGrade = "Q355B",
                Thickness = 30.0,
                DiameterOrWidthMm = 920.0,
                Length = 920.0,
                Quantity = 2,
                SingleNetWeight = 287.00,
                TotalNetWeight = 574.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 574.00 * 1.15,
                SingleSurfaceAreaM2 = 1.33,
                TotalSurfaceAreaM2 = 2.66,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                ThicknessSurchargePerTon = 150.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            itemNH.SubProjects.Add(subNH50);

            // NH - 构件 4: N-NH-CHORD-MAIN (主桁架大跨特厚弦杆总成, 净重 658,392.00 kg, 表面积 1,915.11 m2)
            var subNHChord = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NH-CHORD-MAIN",
                SubProjectName = "主桁架大跨特厚弦杆总成",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 50.0,
                AssemblyElevation = 35.5,
                SingleWeight = 28.50,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = ">25t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNHChord.Parts.Add(new BoqPart
            {
                SubProject = subNHChord,
                PartMark = "HLP-CH-01",
                SectionSpecification = "PIP1200*50",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 1200.0,
                Length = 12000.0,
                Quantity = 24,
                SingleNetWeight = 17016.00,
                TotalNetWeight = 408384.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 408384.00 * 1.15,
                SingleSurfaceAreaM2 = 45.23875,
                TotalSurfaceAreaM2 = 1085.73,
                ZDirectionGrade = "Z25",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 200.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6150.0,
                ActualPurchasePricePerTon = 5600.0
            });
            subNHChord.Parts.Add(new BoqPart
            {
                SubProject = subNHChord,
                PartMark = "HLP-CH-02",
                SectionSpecification = "PIP1000*40",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 40.0,
                DiameterOrWidthMm = 1000.0,
                Length = 12000.0,
                Quantity = 22,
                SingleNetWeight = 11364.00,
                TotalNetWeight = 250008.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 250008.00 * 1.15,
                SingleSurfaceAreaM2 = 37.699,
                TotalSurfaceAreaM2 = 829.38,
                ZDirectionGrade = "Z25",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 250.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 200.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6150.0,
                ActualPurchasePricePerTon = 5600.0
            });
            itemNH.SubProjects.Add(subNHChord);

            // NH - 构件 5: N-NH-WEB-DIAG (主桁架空间腹杆斜撑系统, 净重 235,689.00 kg, 表面积 1,484.53 m2)
            var subNHDiag = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NH-WEB-DIAG",
                SubProjectName = "主桁架空间腹杆斜撑系统",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 25.0,
                AssemblyElevation = 30.0,
                SingleWeight = 3.20,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "<5t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNHDiag.Parts.Add(new BoqPart
            {
                SubProject = subNHDiag,
                PartMark = "HLP-WD-01",
                SectionSpecification = "PIP600*25",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 25.0,
                DiameterOrWidthMm = 600.0,
                Length = 8500.0,
                Quantity = 52,
                SingleNetWeight = 3016.26923076923,
                TotalNetWeight = 156846.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 156846.00 * 1.15,
                SingleSurfaceAreaM2 = 16.021,
                TotalSurfaceAreaM2 = 833.09,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                ThicknessSurchargePerTon = 100.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            subNHDiag.Parts.Add(new BoqPart
            {
                SubProject = subNHDiag,
                PartMark = "HLP-WD-02",
                SectionSpecification = "PIP450*16",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 16.0,
                DiameterOrWidthMm = 450.0,
                Length = 7200.0,
                Quantity = 64,
                SingleNetWeight = 1231.921875,
                TotalNetWeight = 78843.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 78843.00 * 1.15,
                SingleSurfaceAreaM2 = 10.17875,
                TotalSurfaceAreaM2 = 651.44,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            itemNH.SubProjects.Add(subNHDiag);

            // NH - 构件 6: N-NH-WEB-VERT (主桁架空间坚杆与立柱, 净重 119,139.00 kg, 表面积 719.43 m2)
            var subNHVert = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NH-WEB-VERT",
                SubProjectName = "主桁架空间坚杆与立柱",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 30.0,
                AssemblyElevation = 25.0,
                SingleWeight = 7.80,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "5-15t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNHVert.Parts.Add(new BoqPart
            {
                SubProject = subNHVert,
                PartMark = "HLP-WV-01",
                SectionSpecification = "PIP500*20",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 20.0,
                DiameterOrWidthMm = 500.0,
                Length = 6500.0,
                Quantity = 36,
                SingleNetWeight = 1535.30555555556,
                TotalNetWeight = 55271.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 55271.00 * 1.15,
                SingleSurfaceAreaM2 = 10.21028,
                TotalSurfaceAreaM2 = 367.57,
                ZDirectionGrade = "无",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4200.0,
                ThicknessSurchargePerTon = 50.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4850.0,
                ActualPurchasePricePerTon = 4500.0
            });
            subNHVert.Parts.Add(new BoqPart
            {
                SubProject = subNHVert,
                PartMark = "HLP-WV-02",
                SectionSpecification = "PIP800*30",
                SectionType = "PIP",
                SteelGrade = "Q460GJC",
                Thickness = 30.0,
                DiameterOrWidthMm = 800.0,
                Length = 7000.0,
                Quantity = 16,
                SingleNetWeight = 3991.75,
                TotalNetWeight = 63868.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 63868.00 * 1.15,
                SingleSurfaceAreaM2 = 21.99125,
                TotalSurfaceAreaM2 = 351.86,
                ZDirectionGrade = "Z25",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 150.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 200.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6150.0,
                ActualPurchasePricePerTon = 5600.0
            });
            itemNH.SubProjects.Add(subNHVert);

            // NH - 构件 7: N-NH-NODE-GUSSET (空间相贯厚板节点嵌板, 净重 93,376.45 kg, 表面积 1,156.39 m2)
            var subNHNode = new BoqSubProject
            {
                TenderItem = itemNH,
                SubProjectCode = "N-NH-NODE-GUSSET",
                SubProjectName = "空间相贯厚板节点嵌板",
                StructuralRole = "主桁架",
                InstallationMethod = "发现场高空散拼",
                MaxPlateThickness = 60.0,
                AssemblyElevation = 34.0,
                SingleWeight = 4.20,
                Quantity = 1,
                HasFieldWelding = true,
                CraneTonnageTier = "<5t",
                FireRatingHours = 3.0,
                FireProofType = "厚型非膨胀",
                FabricationQuotaRule = fabMain,
                InstallationQuotaRule = insHigh
            };
            subNHNode.Parts.Add(new BoqPart
            {
                SubProject = subNHNode,
                PartMark = "HLP-NG-01",
                SectionSpecification = "PL50*1500",
                SectionType = "PL",
                SteelGrade = "Q460GJC",
                Thickness = 50.0,
                DiameterOrWidthMm = 1500.0,
                Length = 1800.0,
                Quantity = 88,
                SingleNetWeight = 706.50,
                TotalNetWeight = 62172.00,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 62172.00 * 1.15,
                SingleSurfaceAreaM2 = 8.10,
                TotalSurfaceAreaM2 = 712.80,
                ZDirectionGrade = "Z35",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 300.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6400.0,
                ActualPurchasePricePerTon = 5800.0
            });
            subNHNode.Parts.Add(new BoqPart
            {
                SubProject = subNHNode,
                PartMark = "HLP-NG-02",
                SectionSpecification = "PL60*2000",
                SectionType = "PL",
                SteelGrade = "Q460GJC",
                Thickness = 60.0,
                DiameterOrWidthMm = 2000.0,
                Length = 2200.0,
                Quantity = 32,
                SingleNetWeight = 975.1390625,
                TotalNetWeight = 31204.45,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 31204.45 * 1.15,
                SingleSurfaceAreaM2 = 13.8621875,
                TotalSurfaceAreaM2 = 443.59,
                ZDirectionGrade = "Z35",
                IsSiteWelded = true,
                BaseNetPricePerTon = 4250.0,
                ThicknessSurchargePerTon = 350.0,
                GradeSurchargePerTon = 750.0,
                ZPerformanceSurchargePerTon = 300.0,
                FreightSurchargePerTon = 50.0,
                ApprovedPricePerTon = 6400.0,
                ActualPurchasePricePerTon = 5800.0
            });
            itemNH.SubProjects.Add(subNHNode);

            project.TenderItems.Add(itemNH);

            // ========================================================================
            // 2. 清单项 NF: 主体育场南侧副桁架系统 (继承全局默认费率: 8% 管理费, 7% 利润)
            // ========================================================================
            var itemNF = new BoqTenderItem
            {
                Id = 2,
                ProjectId = 1,
                Project = project,
                ItemTag = "NF",
                BillItemCode = "010101001002",
                BillItemName = "南侧大跨副桁架系统制作安装",
                ProjectCharacteristics = "空间管桁架副桁架体系、Q355B、2.0H防火涂装、厂内拼装整体吊装",
                Unit = "t",
                TenderQuantity = BenchmarkNfTenderQuantityTon,
                IsCustomRateEnabled = false
            };

            // NF - 构件 1: N-NF-CHORD-UP (副桁架上弦标准节, 净重 346,080.00 kg)
            var subNFUp = new BoqSubProject
            {
                TenderItem = itemNF,
                SubProjectCode = "N-NF-CHORD-UP",
                SubProjectName = "副桁架上弦标准节",
                StructuralRole = "副桁架",
                InstallationMethod = "厂内拼装整体吊装",
                MaxPlateThickness = 20.0,
                AssemblyElevation = 18.0,
                SingleWeight = 4.5,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 2.0,
                FireProofType = "薄型非膨胀",
                FabricationQuotaRule = fabSub,
                InstallationQuotaRule = insSub
            };
            subNFUp.Parts.Add(new BoqPart
            {
                SubProject = subNFUp,
                PartMark = "NFP-UP-01",
                SectionSpecification = "PIP500*20",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 20.0,
                DiameterOrWidthMm = 500.0,
                Length = 12000.0,
                Quantity = 40,
                TotalNetWeight = 346080.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 346080.0 * 1.15,
                TotalSurfaceAreaM2 = 1720.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4800.0,
                ActualPurchasePricePerTon = 4550.0
            });
            itemNF.SubProjects.Add(subNFUp);

            // NF - 构件 2: N-NF-CHORD-LOW (副桁架下弦标准节, 净重 285,120.00 kg)
            var subNFLow = new BoqSubProject
            {
                TenderItem = itemNF,
                SubProjectCode = "N-NF-CHORD-LOW",
                SubProjectName = "副桁架下弦标准节",
                StructuralRole = "副桁架",
                InstallationMethod = "厂内拼装整体吊装",
                MaxPlateThickness = 16.0,
                AssemblyElevation = 16.0,
                SingleWeight = 4.0,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 2.0,
                FireProofType = "薄型非膨胀",
                FabricationQuotaRule = fabSub,
                InstallationQuotaRule = insSub
            };
            subNFLow.Parts.Add(new BoqPart
            {
                SubProject = subNFLow,
                PartMark = "NFP-LOW-01",
                SectionSpecification = "PIP400*16",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 16.0,
                DiameterOrWidthMm = 400.0,
                Length = 12000.0,
                Quantity = 40,
                TotalNetWeight = 285120.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 285120.0 * 1.15,
                TotalSurfaceAreaM2 = 1480.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4800.0,
                ActualPurchasePricePerTon = 4550.0
            });
            itemNF.SubProjects.Add(subNFLow);

            // NF - 构件 3: N-NF-WEB-DIAG (副桁架腹杆与系杆组, 净重 234,000.00 kg)
            var subNFDiag = new BoqSubProject
            {
                TenderItem = itemNF,
                SubProjectCode = "N-NF-WEB-DIAG",
                SubProjectName = "副桁架腹杆与系杆组",
                StructuralRole = "副桁架",
                InstallationMethod = "厂内拼装整体吊装",
                MaxPlateThickness = 10.0,
                AssemblyElevation = 17.0,
                SingleWeight = 3.0,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 2.0,
                FireProofType = "薄型非膨胀",
                FabricationQuotaRule = fabSub,
                InstallationQuotaRule = insSub
            };
            subNFDiag.Parts.Add(new BoqPart
            {
                SubProject = subNFDiag,
                PartMark = "NFP-WD-01",
                SectionSpecification = "PIP273*10",
                SectionType = "PIP",
                SteelGrade = "Q355B",
                Thickness = 10.0,
                DiameterOrWidthMm = 273.0,
                Length = 8000.0,
                Quantity = 60,
                TotalNetWeight = 234000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 234000.0 * 1.15,
                TotalSurfaceAreaM2 = 1520.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 4200.0,
                GradeSurchargePerTon = 200.0,
                FreightSurchargePerTon = 80.0,
                ApprovedPricePerTon = 4800.0,
                ActualPurchasePricePerTon = 4550.0
            });
            itemNF.SubProjects.Add(subNFDiag);

            project.TenderItems.Add(itemNF);

            // ========================================================================
            // 3. 清单项 JLC: 第一检录处轻型钢结构 (自定义费率覆盖: 5% 管理费, 4% 利润)
            // ========================================================================
            var itemJLC = new BoqTenderItem
            {
                Id = 3,
                ProjectId = 1,
                Project = project,
                ItemTag = "JLC",
                BillItemCode = "010101001005",
                BillItemName = "第一检录处轻型钢结构制作安装",
                ProjectCharacteristics = "门式轻钢刚架体系、Q235B、1.5H薄型膨胀防火涂装、汽车吊装配",
                Unit = "t",
                TenderQuantity = BenchmarkJlcTenderQuantityTon,
                IsCustomRateEnabled = true,
                CustomManagementFeeRate = BenchmarkJlcCustomMgmtRate,
                CustomProfitRate = BenchmarkJlcCustomProfitRate
            };

            // JLC - 构件 1: JLC-COL-01 (轻钢门式刚架柱, 净重 285,000.00 kg)
            var subJLCCol = new BoqSubProject
            {
                TenderItem = itemJLC,
                SubProjectCode = "JLC-COL-01",
                SubProjectName = "轻钢门式刚架柱",
                StructuralRole = "柱",
                InstallationMethod = "汽车吊拼装",
                MaxPlateThickness = 16.0,
                AssemblyElevation = 9.5,
                SingleWeight = 1.8,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 1.5,
                FireProofType = "超薄型膨胀",
                FabricationQuotaRule = fabLight,
                InstallationQuotaRule = insLight
            };
            subJLCCol.Parts.Add(new BoqPart
            {
                SubProject = subJLCCol,
                PartMark = "JLC-P-COL",
                SectionSpecification = "H600*300*10*16",
                SectionType = "H",
                SteelGrade = "Q235B",
                Thickness = 16.0,
                DiameterOrWidthMm = 300.0,
                Length = 9000.0,
                Quantity = 100,
                TotalNetWeight = 285000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 327750.0,
                TotalSurfaceAreaM2 = 2250.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 3950.0,
                GradeSurchargePerTon = 0.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4350.0,
                ActualPurchasePricePerTon = 4200.0
            });
            itemJLC.SubProjects.Add(subJLCCol);

            // JLC - 构件 2: JLC-BEAM-01 (刚架变截面梁, 净重 263,300.00 kg)
            var subJLCBeam = new BoqSubProject
            {
                TenderItem = itemJLC,
                SubProjectCode = "JLC-BEAM-01",
                SubProjectName = "刚架变截面梁",
                StructuralRole = "梁",
                InstallationMethod = "汽车吊拼装",
                MaxPlateThickness = 12.0,
                AssemblyElevation = 10.5,
                SingleWeight = 1.5,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 1.5,
                FireProofType = "超薄型膨胀",
                FabricationQuotaRule = fabLight,
                InstallationQuotaRule = insLight
            };
            subJLCBeam.Parts.Add(new BoqPart
            {
                SubProject = subJLCBeam,
                PartMark = "JLC-P-BM",
                SectionSpecification = "H(400-750)*250*8*12",
                SectionType = "H",
                SteelGrade = "Q235B",
                Thickness = 12.0,
                DiameterOrWidthMm = 250.0,
                Length = 12000.0,
                Quantity = 100,
                TotalNetWeight = 263300.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 302795.0,
                TotalSurfaceAreaM2 = 2480.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 3950.0,
                GradeSurchargePerTon = 0.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4350.0,
                ActualPurchasePricePerTon = 4200.0
            });
            itemJLC.SubProjects.Add(subJLCBeam);

            // JLC - 构件 3: JLC-PURLIN-01 (屋面与墙面檩条支撑, 净重 180,000.00 kg)
            var subJLCPurlin = new BoqSubProject
            {
                TenderItem = itemJLC,
                SubProjectCode = "JLC-PURLIN-01",
                SubProjectName = "屋面与墙面檩条支撑",
                StructuralRole = "支撑",
                InstallationMethod = "散装装配",
                MaxPlateThickness = 2.5,
                AssemblyElevation = 11.0,
                SingleWeight = 0.2,
                Quantity = 1,
                HasFieldWelding = false,
                CraneTonnageTier = "<5t",
                FireRatingHours = 1.5,
                FireProofType = "超薄型膨胀",
                FabricationQuotaRule = fabLight,
                InstallationQuotaRule = insLight
            };
            subJLCPurlin.Parts.Add(new BoqPart
            {
                SubProject = subJLCPurlin,
                PartMark = "JLC-P-PRL",
                SectionSpecification = "C220*75*20*2.5",
                SectionType = "C",
                SteelGrade = "Q235B",
                Thickness = 2.5,
                DiameterOrWidthMm = 220.0,
                Length = 6000.0,
                Quantity = 1000,
                TotalNetWeight = 180000.0,
                PurchasingWasteFactor = 1.15,
                TotalGrossWeight = 207000.0,
                TotalSurfaceAreaM2 = 3120.0,
                ZDirectionGrade = "无",
                IsSiteWelded = false,
                BaseNetPricePerTon = 3950.0,
                GradeSurchargePerTon = 0.0,
                FreightSurchargePerTon = 100.0,
                ApprovedPricePerTon = 4350.0,
                ActualPurchasePricePerTon = 4200.0
            });
            itemJLC.SubProjects.Add(subJLCPurlin);

            project.TenderItems.Add(itemJLC);

            // ========================================================================
            // 确保所有在内存实例化的项目、清单项、构件与零件具备确定性非零主键与外键 ID
            // ========================================================================
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

            // ========================================================================
            // 4. 执行自底向上全成本卷积核算，将所有字段水合填充至对象模型中
            // ========================================================================
            BoqAggregationEngine.CalculateProject(project);

            return project;
        }

        public static BoqProject CreateBenchmarkProjectWithFullHierarchy() => CreateBenchmarkProject();
        public static BoqProject GenerateBaselineProject() => CreateBenchmarkProject();
        public static BoqProject CreateSampleProject() => CreateBenchmarkProject();
    }
}
