// ============================================================================
// 文件归属: src/EngineeringApp.Shared/BOQ/Engine/BoqAggregationEngine.cs
// 模块描述: 钢结构大型工程 BOQ 自底向上精密数学卷积核算与多受众投影引擎
// 规范依据: GB 50500、国家钢结构工程预算定额、ORIGINAL_REQUEST.md、PROJECT.md、AGENTS.md
// ============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EngineeringApp.Shared.BOQ.Models;
using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.BOQ.Engine
{
    /// <summary>
    /// 钢结构 BOQ 自底向上卷积核算引擎
    /// 涵盖零件材料综合到场价 (含1.15倍损耗毛重与采办剪刀差)、车间制作工费 (K_Thick系数)、
    /// 现场安装工费 (K_Elev、K_SiteWeld、K_Weight三因子)、防火防腐涂装排程、
    /// 对外商务分级组价 (双层费率判定) 及五大受众投影与物资采办汇总
    /// </summary>
    public static class BoqAggregationEngine
    {
        // =========================================================================
        // 1. 独立因子与加价快速核算方法 (Factor & Surcharge Calculators)
        // =========================================================================

        /// <summary>
        /// 板厚加工制作难度调节系数 (K_Thick)
        /// 遵循五阶梯难度模型：
        /// T <= 20mm: 1.00; 20 < T <= 30mm: 1.05; 30 < T <= 40mm: 1.12; 40 < T <= 60mm: 1.25; T > 60mm: 1.40
        /// </summary>
        public static double CalculateThicknessFactor(double maxThicknessMm)
        {
            if (maxThicknessMm <= 20.0) return 1.00;
            if (maxThicknessMm <= 30.0) return 1.05;
            if (maxThicknessMm <= 40.0) return 1.12;
            if (maxThicknessMm <= 60.0) return 1.25;
            return 1.40;
        }

        /// <summary>
        /// 高空作业就位标高调节系数 (K_Elev)
        /// 遵循三档阶梯模型：
        /// H <= 20m: 1.00; 20m < H <= 35m: 1.15; H > 35m: 1.30 (超高空原位散拼与操作平台摊销)
        /// </summary>
        public static double CalculateElevationFactor(double elevationM)
        {
            if (elevationM <= 20.0) return 1.00;
            if (elevationM <= 35.0) return 1.15;
            return 1.30;
        }

        /// <summary>
        /// 现场原位全熔透坡口焊工法调节系数 (K_SiteWeld)
        /// 发现场高空操作平台全熔透焊为 1.30；厂内拼装整体吊装为 1.00
        /// </summary>
        public static double CalculateSiteWeldFactor(bool isSiteWelded)
        {
            return isSiteWelded ? 1.30 : 1.00;
        }

        /// <summary>
        /// 起重机吨位与单件吊重分档系数 (K_Weight)
        /// 遵循四档起重能力分档：
        /// W < 5.0t: 1.00; 5.0t <= W <= 15.0t: 1.20; 15.0t < W <= 25.0t: 1.30; W > 25.0t: 1.45 (需大吨位履带吊或双机抬吊)
        /// </summary>
        public static double CalculateWeightFactor(double pieceWeightTon)
        {
            if (pieceWeightTon < 5.0) return 1.00;
            if (pieceWeightTon <= 15.0) return 1.20;
            if (pieceWeightTon <= 25.0) return 1.30;
            return 1.45;
        }

        /// <summary>
        /// 根据起重机吨位分档文本或单件吊重判定 K_Weight
        /// </summary>
        public static double CalculateWeightFactorFromTier(string? tier, double pieceWeightTon = 0.0)
        {
            if (!string.IsNullOrWhiteSpace(tier))
            {
                string t = tier.Trim().ToLowerInvariant();
                if (t.Contains("<5") || t.StartsWith("<5")) return 1.00;
                if (t.Contains("5-15") || t.Contains("5~15")) return 1.20;
                if (t.Contains("15-25") || t.Contains("15~25")) return 1.30;
                if (t.Contains(">25") || t.StartsWith(">25")) return 1.45;
            }

            return CalculateWeightFactor(pieceWeightTon);
        }

        /// <summary>
        /// 耐火极限等级与平米包干涂装单价映射 (元/m2)
        /// 3.0H: 145.0 元/m2 (厚型室内无机非膨胀防火涂料，加挂钢丝网)
        /// 2.5H / 2.0H: 110.0 元/m2 (中厚型无机防火涂料)
        /// 1.5H: 75.0 元/m2 (超薄型/薄型膨胀型防火涂料)
        /// 默认保底 75.0 元/m2
        /// </summary>
        public static double CalculateFireRatingRate(double fireRatingHours)
        {
            if (fireRatingHours >= 3.0) return 145.0;
            if (fireRatingHours >= 2.0) return 110.0;
            if (fireRatingHours > 0.0) return 75.0;
            return 75.0;
        }

        /// <summary>
        /// 钢板厚度行业阶梯加价 (元/t)
        /// T <= 20: 0; 20 < T <= 30: 80; 30 < T <= 40: 180; 40 < T <= 50: 350; 50 < T <= 60: 500; T > 60: 750
        /// </summary>
        public static double CalculatePartThicknessSurcharge(double thicknessMm)
        {
            if (thicknessMm <= 20.0) return 0.0;
            if (thicknessMm <= 30.0) return 80.0;
            if (thicknessMm <= 40.0) return 180.0;
            if (thicknessMm <= 50.0) return 350.0;
            if (thicknessMm <= 60.0) return 500.0;
            return 750.0;
        }

        /// <summary>
        /// 材质牌号加价 (元/t)
        /// Q460 / Q460GJC / Q460D: +750.0 元/t; Q355B / Q235B: 0.0 元/t
        /// </summary>
        public static double CalculatePartGradeSurcharge(string? grade)
        {
            if (string.IsNullOrWhiteSpace(grade)) return 0.0;
            string g = grade.Trim().ToUpperInvariant();
            if (g.Contains("460")) return 750.0;
            return 0.0;
        }

        /// <summary>
        /// 钢板 Z 向厚度方向抗层状撕裂性能加价 (元/t)
        /// Z15: +200.0; Z25: +350.0; Z35: +500.0; 无/其他: 0.0
        /// </summary>
        public static double CalculatePartZPerformanceSurcharge(string? zPerformance)
        {
            if (string.IsNullOrWhiteSpace(zPerformance)) return 0.0;
            string z = zPerformance.Trim().ToUpperInvariant();
            if (z.Contains("Z35")) return 500.0;
            if (z.Contains("Z25")) return 350.0;
            if (z.Contains("Z15")) return 200.0;
            return 0.0;
        }

        /// <summary>
        /// 默认区域运杂物流调运加价 (元/t)
        /// 默认华东区域大型钢厂直发调运标准为 +80.0 元/t
        /// </summary>
        public static double CalculatePartFreightSurcharge(string destination = "华东")
        {
            return 80.0;
        }

        // =========================================================================
        // 2. 自底向上四层级实体核算流 (Part -> SubProject -> TenderItem -> Project)
        // =========================================================================

        /// <summary>
        /// Level 1: 物理零件层材料费核算 (BoqPart)
        /// 计算 1.15 倍采购损耗毛重、多维加价综合到场单价、采购合价及采办经营创效(剪刀差利润)
        /// </summary>
        public static PartCostResult CalculatePart(BoqPart part)
        {
            ArgumentNullException.ThrowIfNull(part);

            // 1. 重量与采购毛重确定 (严密防范 0 值或倒置)
            if (part.Quantity <= 0) part.Quantity = 1;

            if (part.SingleNetWeight <= 0 && part.TotalNetWeight > 0)
            {
                part.SingleNetWeight = part.TotalNetWeight / part.Quantity;
            }
            else if (part.TotalNetWeight <= 0 && part.SingleNetWeight > 0)
            {
                part.TotalNetWeight = part.SingleNetWeight * part.Quantity;
            }

            // 确保定额采购损耗乘数不小于 1.0 (默认 1.15 即 15% 损耗)
            if (part.PurchasingWasteFactor <= 0.0)
            {
                part.PurchasingWasteFactor = 1.15;
            }
            part.TotalGrossWeight = part.TotalNetWeight * part.PurchasingWasteFactor;

            // 2. 基准大盘单价判定
            if (part.BaseNetPricePerTon <= 0.0)
            {
                string gradeUpper = (part.SteelGrade ?? "").ToUpperInvariant();
                if (gradeUpper.Contains("235"))
                {
                    part.BaseNetPricePerTon = 3920.0;
                }
                else
                {
                    // Q355B 或 Q460 基价
                    part.BaseNetPricePerTon = 4050.0;
                }
            }

            // 3. 厚度加价判定
            if (part.ThicknessSurchargePerTon <= 0.0 && part.Thickness > 0.0)
            {
                part.ThicknessSurchargePerTon = CalculatePartThicknessSurcharge(part.Thickness);
            }

            // 4. 材质加价判定
            if (part.GradeSurchargePerTon <= 0.0 && !string.IsNullOrWhiteSpace(part.SteelGrade))
            {
                part.GradeSurchargePerTon = CalculatePartGradeSurcharge(part.SteelGrade);
            }

            // 5. Z向抗撕裂性能加价判定
            if (part.ZPerformanceSurchargePerTon <= 0.0 && !string.IsNullOrWhiteSpace(part.ZDirectionGrade))
            {
                part.ZPerformanceSurchargePerTon = CalculatePartZPerformanceSurcharge(part.ZDirectionGrade);
            }

            // 6. 区域运杂物流调运费判定
            if (part.FreightSurchargePerTon <= 0.0)
            {
                part.FreightSurchargePerTon = CalculatePartFreightSurcharge();
            }

            // 7. 材料综合采购到场单价 (P_Mat = P_Base + ΔP_Thick + ΔP_Grade + ΔP_Z + ΔP_Freight)
            part.MaterialUnitPricePerTon = part.BaseNetPricePerTon
                                         + part.ThicknessSurchargePerTon
                                         + part.GradeSurchargePerTon
                                         + part.ZPerformanceSurchargePerTon
                                         + part.FreightSurchargePerTon;

            // 8. 零件材料采购总合价 = (1.15采购毛重吨位) × 综合到场单价
            double grossWeightTon = part.TotalGrossWeight / 1000.0;
            part.TotalPartMaterialCost = grossWeightTon * part.MaterialUnitPricePerTon;

            // 9. 采办经营创效 (剪刀差套利额) = (业主批复指导单价 - 实际采办单价) × 采购毛重吨位
            if (part.ApprovedPricePerTon > 0.0 || part.ActualPurchasePricePerTon > 0.0)
            {
                part.ArbitrageProfit = (part.ApprovedPricePerTon - part.ActualPurchasePricePerTon) * grossWeightTon;
            }
            else
            {
                part.ArbitrageProfit = 0.0;
            }

            return new PartCostResult
            {
                PartId = part.Id,
                PartMark = part.PartMark,
                NetWeightKg = part.TotalNetWeight,
                GrossWeightKg = part.TotalGrossWeight,
                MaterialUnitPricePerTon = part.MaterialUnitPricePerTon,
                TotalPartCost = part.TotalPartMaterialCost,
                ArbitrageProfit = part.ArbitrageProfit
            };
        }

        /// <summary>
        /// Level 2: 构件成本中枢核算 (BoqSubProject)
        /// 汇总下属零件材料费、测算车间加工制作工费 (K_Thick)、现场安装工费 (K_Elev/K_SiteWeld/K_Weight)、
        /// 防火防腐涂装费，并汇算构件直接成本底价
        /// </summary>
        public static SubProjectCostResult CalculateSubProject(
            BoqSubProject subProject,
            BoqTenderItem? parentItem = null,
            BoqProject? project = null)
        {
            ArgumentNullException.ThrowIfNull(subProject);

            if (subProject.TenderItem == null && parentItem != null)
            {
                subProject.TenderItem = parentItem;
            }

            // 1. 递归核算所有零件并归集重量与材料费
            var partResults = new List<PartCostResult>();
            double sumPartMaterialCost = 0.0;
            double sumPartsNetWeightKg = 0.0;
            double sumPartsGrossWeightKg = 0.0;
            double sumPartsSurfaceAreaM2 = 0.0;
            double maxPartThickness = 0.0;

            foreach (var part in subProject.Parts)
            {
                if (part.SubProject == null) part.SubProject = subProject;
                var pRes = CalculatePart(part);
                partResults.Add(pRes);

                sumPartMaterialCost += part.TotalPartMaterialCost;
                sumPartsNetWeightKg += part.TotalNetWeight;
                sumPartsGrossWeightKg += part.TotalGrossWeight;
                sumPartsSurfaceAreaM2 += part.TotalSurfaceAreaM2;

                if (part.Thickness > maxPartThickness)
                {
                    maxPartThickness = part.Thickness;
                }
            }

            // 零件材料费归集
            subProject.TotalPartsMaterialCost = sumPartMaterialCost;

            // 构件净重与毛重归集 (若下属零件有算量则以零件为准，否则保留构件自身设置)
            if (sumPartsNetWeightKg > 0.0)
            {
                subProject.SubProjectNetWeightKg = sumPartsNetWeightKg;
                subProject.SubProjectGrossWeightKg = sumPartsGrossWeightKg;
            }
            else if (subProject.SubProjectNetWeightKg > 0.0 && subProject.SubProjectGrossWeightKg <= 0.0)
            {
                subProject.SubProjectGrossWeightKg = subProject.SubProjectNetWeightKg * 1.15;
            }

            // 展开表面积归集
            if (subProject.SubProjectSurfaceAreaM2 <= 0.0)
            {
                if (sumPartsSurfaceAreaM2 > 0.0)
                {
                    subProject.SubProjectSurfaceAreaM2 = sumPartsSurfaceAreaM2;
                }
                else if (subProject.CoatingAreaPerUnit > 0.0)
                {
                    subProject.SubProjectSurfaceAreaM2 = subProject.CoatingAreaPerUnit * Math.Max(1, subProject.Quantity);
                }
            }

            // 最大主板厚/管壁厚判定
            if (subProject.MaxPlateThickness <= 0.0)
            {
                subProject.MaxPlateThickness = maxPartThickness;
            }

            // 单件吊装吨位判定
            if (subProject.SingleWeight <= 0.0 && subProject.SubProjectNetWeightKg > 0.0)
            {
                int qty = Math.Max(1, subProject.Quantity);
                subProject.SingleWeight = (subProject.SubProjectNetWeightKg / 1000.0) / qty;
            }

            double netTon = subProject.SubProjectNetWeightKg / 1000.0;

            // 2. 车间加工制作工费核算 (含 K_Thick 板厚加工难度调节系数)
            double kThick = CalculateThicknessFactor(subProject.MaxPlateThickness);
            var fabRule = subProject.FabricationQuotaRule;

            double baseFabRate = fabRule?.BaseLaborRatePerTon ??
                (subProject.StructuralRole == "主桁架" ? 2200.0 : 1500.0);
            double intersectRate = fabRule?.IntersectingCuttingUnitRate ?? 280.0;
            double utRate = fabRule?.UltrasonicTestingUnitRate ?? 150.0;
            double preassemblyRate = fabRule?.PreassemblyUnitRate ?? 200.0;

            // 车间综合制作单价 = (基准制作 + 相贯五轴切割 + UT探伤 + 厂内预拼装) × K_Thick
            subProject.UnitFabricationCost = (baseFabRate + intersectRate + utRate + preassemblyRate) * kThick;
            subProject.TotalFabricationCost = netTon * subProject.UnitFabricationCost;

            // 3. 现场安装工费核算 (含 K_Elev、K_SiteWeld、K_Weight 三因子乘法模型)
            double kElev = CalculateElevationFactor(subProject.AssemblyElevation);
            double kSiteWeld = CalculateSiteWeldFactor(subProject.HasFieldWelding);
            double kWeight = CalculateWeightFactorFromTier(subProject.CraneTonnageTier, subProject.SingleWeight);

            var installRule = subProject.InstallationQuotaRule;
            double baseInstallRate = installRule?.BaseLaborRatePerTon ?? 1800.0;
            double craneRate = installRule?.MainCraneRatePerTon ?? 650.0;
            double scaffoldRate = installRule?.TrestlePlatformRatePerTon ?? 300.0;

            // 现场安装综合单价 = (现场安装直接工费 + 主吊机械费 + 胎架措施费) × K_Elev × K_SiteWeld × K_Weight
            subProject.UnitInstallationCost = (baseInstallRate + craneRate + scaffoldRate) * kElev * kSiteWeld * kWeight;
            subProject.TotalInstallationCost = netTon * subProject.UnitInstallationCost;

            // 4. 防火防腐涂装排程核算
            double fireUnitPrice = CalculateFireRatingRate(subProject.FireRatingHours);
            subProject.TotalFireCoatingCost = subProject.SubProjectSurfaceAreaM2 * fireUnitPrice;

            // 5. 构件直接工程费底价汇总
            double directCost = subProject.TotalPartsMaterialCost
                              + subProject.TotalFabricationCost
                              + subProject.TotalInstallationCost
                              + subProject.TotalFireCoatingCost;

            subProject.UnitDirectCost = netTon > 0.0 ? directCost / netTon : 0.0;

            return new SubProjectCostResult
            {
                SubProjectId = subProject.Id,
                SubProjectCode = subProject.SubProjectCode,
                SubProjectName = subProject.SubProjectName,
                TotalNetWeightKg = subProject.SubProjectNetWeightKg,
                TotalGrossWeightKg = subProject.SubProjectGrossWeightKg,
                TotalSurfaceAreaM2 = subProject.SubProjectSurfaceAreaM2,
                MaterialCost = subProject.TotalPartsMaterialCost,
                FabricationCost = subProject.TotalFabricationCost,
                InstallationCost = subProject.TotalInstallationCost,
                FireCoatingCost = subProject.TotalFireCoatingCost,
                DirectCost = directCost,
                UnitDirectCostPerTon = subProject.UnitDirectCost,
                KThick = kThick,
                KElev = kElev,
                KSiteWeld = kSiteWeld,
                KWeight = kWeight,
                PartResults = partResults
            };
        }

        /// <summary>
        /// Level 3: 清单项层分级费率判定与对外商务综合组价 (BoqTenderItem)
        /// 汇算下属构件直接成本底价，判定单项自定义费率覆盖与项目默认费率，输出最终综合单价与投标总合价
        /// </summary>
        public static TenderItemCostResult CalculateTenderItem(BoqTenderItem item, BoqProject? project = null)
        {
            ArgumentNullException.ThrowIfNull(item);

            if (item.Project == null && project != null)
            {
                item.Project = project;
            }

            var effectiveProject = project ?? item.Project;

            // 1. 递归核算下属所有构件
            var subResults = new List<SubProjectCostResult>();
            double sumDirectCost = 0.0;

            foreach (var sub in item.SubProjects)
            {
                if (sub.TenderItem == null) sub.TenderItem = item;
                var subRes = CalculateSubProject(sub, item, effectiveProject);
                subResults.Add(subRes);
                sumDirectCost += subRes.DirectCost;
            }

            // 2. 直接成本总额与直接成本底价
            if (item.TenderQuantity > 0.0)
            {
                item.DirectCostPerTon = sumDirectCost / item.TenderQuantity;
            }
            else
            {
                double totalSubNetTon = subResults.Sum(s => s.TotalNetWeightKg) / 1000.0;
                item.DirectCostPerTon = totalSubNetTon > 0.0 ? sumDirectCost / totalSubNetTon : 0.0;
            }

            // 3. 费率分级判定 (调用模型原生契约，单项覆盖优先，未覆盖继承全局默认 8% 与 7%)
            var (mgmtRate, profitRate) = item.GetEffectiveRates(effectiveProject);

            // 4. 对外商务综合单价 = 直接成本底价 × (1 + 管理费率) × (1 + 利润率)
            item.ComprehensiveUnitPrice = item.DirectCostPerTon * (1.0 + mgmtRate) * (1.0 + profitRate);

            // 5. 对外投标总合价 = 投标工程量 × 综合单价
            item.TotalTenderPrice = item.TenderQuantity * item.ComprehensiveUnitPrice;

            return new TenderItemCostResult
            {
                TenderItemId = item.Id,
                ItemTag = item.ItemTag,
                BillItemCode = item.BillItemCode,
                BillItemName = item.BillItemName,
                TenderQuantityTon = item.TenderQuantity,
                DirectCostSum = sumDirectCost,
                DirectCostPerTon = item.DirectCostPerTon,
                IsCustomRateApplied = item.IsCustomRateEnabled && (item.CustomManagementFeeRate.HasValue || item.CustomProfitRate.HasValue),
                EffectiveManagementFeeRate = mgmtRate,
                EffectiveProfitRate = profitRate,
                ComprehensiveUnitPrice = item.ComprehensiveUnitPrice,
                TotalTenderPrice = item.TotalTenderPrice,
                SubProjectResults = subResults
            };
        }

        /// <summary>
        /// Level 4: 全项目顶层全成本大盘汇总 (BoqProject)
        /// 完整穿透工料机费利税、管理费、利润以及全项目采办经营创效(剪刀差利润)
        /// </summary>
        public static ProjectCostResult CalculateProject(BoqProject project)
        {
            ArgumentNullException.ThrowIfNull(project);

            var tenderResults = new List<TenderItemCostResult>();

            double totalNetWeightKg = 0.0;
            double totalGrossWeightKg = 0.0;
            double totalSurfaceAreaM2 = 0.0;
            double totalMaterialCost = 0.0;
            double totalFabricationCost = 0.0;
            double totalInstallationCost = 0.0;
            double totalFireCoatingCost = 0.0;
            double totalDirectCost = 0.0;
            double totalBidPrice = 0.0;
            double totalManagementFee = 0.0;
            double totalProfit = 0.0;
            double totalScissorProfit = 0.0;

            foreach (var item in project.TenderItems)
            {
                if (item.Project == null) item.Project = project;
                var itemRes = CalculateTenderItem(item, project);
                tenderResults.Add(itemRes);

                totalBidPrice += itemRes.TotalTenderPrice;

                // 管理费与利润额归集
                double itemDirect = itemRes.DirectCostSum;
                double mgmtAmount = itemDirect * itemRes.EffectiveManagementFeeRate;
                double profitAmount = (itemDirect + mgmtAmount) * itemRes.EffectiveProfitRate;

                totalManagementFee += mgmtAmount;
                totalProfit += profitAmount;

                foreach (var sub in itemRes.SubProjectResults)
                {
                    totalNetWeightKg += sub.TotalNetWeightKg;
                    totalGrossWeightKg += sub.TotalGrossWeightKg;
                    totalSurfaceAreaM2 += sub.TotalSurfaceAreaM2;
                    totalMaterialCost += sub.MaterialCost;
                    totalFabricationCost += sub.FabricationCost;
                    totalInstallationCost += sub.InstallationCost;
                    totalFireCoatingCost += sub.FireCoatingCost;
                    totalDirectCost += sub.DirectCost;

                    foreach (var p in sub.PartResults)
                    {
                        totalScissorProfit += p.ArbitrageProfit;
                    }
                }
            }

            return new ProjectCostResult
            {
                ProjectId = project.Id,
                ProjectCode = project.ProjectCode,
                ProjectName = project.ProjectName,
                TotalNetWeightKg = totalNetWeightKg,
                TotalGrossWeightKg = totalGrossWeightKg,
                TotalSurfaceAreaM2 = totalSurfaceAreaM2,
                TotalMaterialCost = totalMaterialCost,
                TotalFabricationCost = totalFabricationCost,
                TotalInstallationCost = totalInstallationCost,
                TotalFireCoatingCost = totalFireCoatingCost,
                TotalDirectCost = totalDirectCost,
                TotalBidPrice = totalBidPrice,
                TotalManagementFee = totalManagementFee,
                TotalProfit = totalProfit,
                TotalProcurementScissorProfit = totalScissorProfit,
                TenderItemResults = tenderResults
            };
        }

        // =========================================================================
        // 3. 别名兼容接口 (对齐 plan.md 命名规范)
        // =========================================================================

        public static PartCostResult CalculatePartMaterialCost(BoqPart part) => CalculatePart(part);
        public static SubProjectCostResult CalculateSubProjectCosts(BoqSubProject subProject) => CalculateSubProject(subProject);
        public static TenderItemCostResult RollupCostToTenderItem(BoqTenderItem item, BoqProject project) => CalculateTenderItem(item, project);
        public static ProjectCostResult AggregateProjectMatrix(BoqProject project) => CalculateProject(project);

        // =========================================================================
        // 4. 多受众投影引擎与物料汇总调度
        // =========================================================================

        /// <summary>
        /// 多受众投影总分发器：将核算大盘结果转换为面向特定受众的标准化投影视图
        /// </summary>
        public static AudienceProjectionResult ProjectToAudience(
            ProjectCostResult projectResult,
            BoqAudiencePerspective perspective,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            return perspective switch
            {
                BoqAudiencePerspective.Owner => ProjectToClientTenderMaster(projectResult, project, config),
                BoqAudiencePerspective.ChiefEconomist => ProjectToUltimateCostAnalysis(projectResult, project, config),
                BoqAudiencePerspective.FabricationShop => ProjectToFabricationWorkshop(projectResult, project, config),
                BoqAudiencePerspective.SiteProjectDept => ProjectToSiteErectionSchedule(projectResult, project, config),
                BoqAudiencePerspective.Procurement => ProjectToMaterialProcurementOrder(projectResult, project, config),
                _ => ProjectToUltimateCostAnalysis(projectResult, project, config)
            };
        }

        /// <summary>
        /// 便捷重载：直接从 BoqProject 实体核算并生成指定受众报表
        /// </summary>
        public static AudienceProjectionResult GenerateAudienceReport(
            BoqProject project,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(project);
            config ??= new BoqReportConfig();

            var projectResult = CalculateProject(project);
            return ProjectToAudience(projectResult, config.AudiencePerspective, project, config);
        }

        // -------------------------------------------------------------------------
        // 4.1 视角 1：业主 / 招投标 / 审计清单模式 (ClientTenderMaster - 彻底脱敏内部定额与底价)
        // -------------------------------------------------------------------------
        public static AudienceProjectionResult ProjectToClientTenderMaster(
            ProjectCostResult projectResult,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            var headers = new List<string>
            {
                "序号", "清单代号", "清单编码", "清单项目名称", "项目特征描述",
                "计量单位", "投标工程量", "综合单价(元)", "合价(元)", "合规审计说明"
            };

            var typedRows = new List<ClientTenderMasterRow>();
            var rows = new List<Dictionary<string, object?>>();

            int index = 1;
            foreach (var itemRes in projectResult.TenderItemResults)
            {
                var sourceItem = project?.TenderItems.FirstOrDefault(t => 
                    (itemRes.TenderItemId > 0 && t.Id == itemRes.TenderItemId) || 
                    t.ItemTag == itemRes.ItemTag);
                string charDesc = sourceItem?.ProjectCharacteristics ?? "国家标准规范钢结构深化制作与安装";
                string unit = sourceItem?.Unit ?? "t";

                var typedRow = new ClientTenderMasterRow
                {
                    Index = index,
                    ItemTag = itemRes.ItemTag,
                    BillItemCode = itemRes.BillItemCode,
                    BillItemName = itemRes.BillItemName,
                    ProjectCharacteristics = charDesc,
                    Unit = unit,
                    TenderQuantity = Math.Round(itemRes.TenderQuantityTon, 2),
                    ComprehensiveUnitPrice = Math.Round(itemRes.ComprehensiveUnitPrice, 2),
                    TotalTenderPrice = Math.Round(itemRes.TotalTenderPrice, 2),
                    Remarks = "符合建设工程工程量清单计价规范 (GB 50500)"
                };
                typedRows.Add(typedRow);

                var rowDict = new Dictionary<string, object?>
                {
                    ["序号"] = index,
                    ["清单代号"] = typedRow.ItemTag,
                    ["清单编码"] = typedRow.BillItemCode,
                    ["清单项目名称"] = typedRow.BillItemName,
                    ["项目特征描述"] = typedRow.ProjectCharacteristics,
                    ["计量单位"] = typedRow.Unit,
                    ["投标工程量"] = typedRow.TenderQuantity,
                    ["综合单价(元)"] = typedRow.ComprehensiveUnitPrice,
                    ["合价(元)"] = typedRow.TotalTenderPrice,
                    ["合规审计说明"] = typedRow.Remarks
                };
                rows.Add(rowDict);
                index++;
            }

            double totalTenderQty = projectResult.TenderItemResults.Sum(x => x.TenderQuantityTon);
            double avgUnitPrice = totalTenderQty > 0.0 ? projectResult.TotalBidPrice / totalTenderQty : 0.0;

            var metrics = new Dictionary<string, object?>
            {
                ["总投标报价(元)"] = Math.Round(projectResult.TotalBidPrice, 2),
                ["总投标工程量(t)"] = Math.Round(totalTenderQty, 2),
                ["综合平均单价(元/t)"] = Math.Round(avgUnitPrice, 2),
                ["清单项目数量"] = projectResult.TenderItemResults.Count,
                ["审计合规说明"] = "按《建设工程工程量清单计价规范》(GB 50500) 编制，内部工料机定额已彻底脱敏"
            };

            return new AudienceProjectionResult
            {
                Perspective = BoqAudiencePerspective.Owner,
                PerspectiveName = "业主审计对外清单",
                Headers = headers,
                Rows = rows,
                SummaryMetrics = metrics,
                ClientTenderRows = typedRows
            };
        }

        // -------------------------------------------------------------------------
        // 4.2 视角 2：总经济师 / 决策层 (UltimateCostAnalysis - 100%全要素成本穿透与采办剪刀差)
        // -------------------------------------------------------------------------
        public static AudienceProjectionResult ProjectToUltimateCostAnalysis(
            ProjectCostResult projectResult,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            var headers = new List<string>
            {
                "序号", "清单代号", "清单名称", "投标工程量(t)", "理论净重(t)", "1.15采购毛重(t)",
                "材料费小计(元)", "吨钢材料费(元/t)", "制作工费小计(元)", "吨钢制作费(元/t)",
                "安装工费小计(元)", "吨钢安装费(元/t)", "防火涂装小计(元)", "吨钢涂装费(元/t)",
                "直接成本总额(元)", "直接成本底价(元/t)", "费率策略", "有效管理费率(%)",
                "企业管理费(元)", "有效利润率(%)", "目标利润(元)", "对外综合单价(元/t)",
                "投标总合价(元)", "采办剪刀差创效(元)", "综合毛利总额(元)", "综合毛利率(%)", "吨钢综合净利(元/t)"
            };

            var typedRows = new List<UltimateCostAnalysisRow>();
            var rows = new List<Dictionary<string, object?>>();

            int index = 1;
            foreach (var itemRes in projectResult.TenderItemResults)
            {
                double netTon = itemRes.SubProjectResults.Sum(s => s.TotalNetWeightKg) / 1000.0;
                double grossTon = itemRes.SubProjectResults.Sum(s => s.TotalGrossWeightKg) / 1000.0;
                double matCost = itemRes.SubProjectResults.Sum(s => s.MaterialCost);
                double fabCost = itemRes.SubProjectResults.Sum(s => s.FabricationCost);
                double insCost = itemRes.SubProjectResults.Sum(s => s.InstallationCost);
                double fireCost = itemRes.SubProjectResults.Sum(s => s.FireCoatingCost);
                double directSum = itemRes.DirectCostSum;
                double qty = itemRes.TenderQuantityTon;

                double matPerTon = qty > 0.0 ? matCost / qty : 0.0;
                double fabPerTon = qty > 0.0 ? fabCost / qty : 0.0;
                double insPerTon = qty > 0.0 ? insCost / qty : 0.0;
                double firePerTon = qty > 0.0 ? fireCost / qty : 0.0;

                string rateStatus = itemRes.IsCustomRateApplied ? "★单项覆盖" : "继承默认";
                double mgmtFee = directSum * itemRes.EffectiveManagementFeeRate;
                double profitAmount = (directSum + mgmtFee) * itemRes.EffectiveProfitRate;

                // 统计该清单项下属零件的采办剪刀差创效
                double itemScissorProfit = 0.0;
                foreach (var sub in itemRes.SubProjectResults)
                {
                    itemScissorProfit += sub.PartResults.Sum(p => p.ArbitrageProfit);
                }

                double comprehensiveNetProfit = profitAmount + itemScissorProfit;
                double netMarginRate = itemRes.TotalTenderPrice > 0.0 ? comprehensiveNetProfit / itemRes.TotalTenderPrice : 0.0;
                double netProfitPerTon = qty > 0.0 ? comprehensiveNetProfit / qty : 0.0;

                var typedRow = new UltimateCostAnalysisRow
                {
                    Index = index,
                    ItemTag = itemRes.ItemTag,
                    BillItemName = itemRes.BillItemName,
                    TenderQuantity = Math.Round(qty, 2),
                    NetWeightTon = Math.Round(netTon, 3),
                    GrossWeightTon = Math.Round(grossTon, 3),
                    MaterialCostSum = Math.Round(matCost, 2),
                    MaterialTonCost = Math.Round(matPerTon, 2),
                    FabricationCostSum = Math.Round(fabCost, 2),
                    FabricationTonCost = Math.Round(fabPerTon, 2),
                    InstallationCostSum = Math.Round(insCost, 2),
                    InstallationTonCost = Math.Round(insPerTon, 2),
                    FireCoatingCostSum = Math.Round(fireCost, 2),
                    FireCoatingTonCost = Math.Round(firePerTon, 2),
                    DirectCostSum = Math.Round(directSum, 2),
                    DirectCostPerTon = Math.Round(itemRes.DirectCostPerTon, 2),
                    IsCustomRateApplied = itemRes.IsCustomRateApplied,
                    RateOverrideStatus = rateStatus,
                    EffectiveManagementFeeRate = itemRes.EffectiveManagementFeeRate,
                    ManagementFeeAmount = Math.Round(mgmtFee, 2),
                    EffectiveProfitRate = itemRes.EffectiveProfitRate,
                    TargetProfitAmount = Math.Round(profitAmount, 2),
                    ComprehensiveUnitPrice = Math.Round(itemRes.ComprehensiveUnitPrice, 2),
                    TotalTenderPrice = Math.Round(itemRes.TotalTenderPrice, 2),
                    ScissorDifferenceProfit = Math.Round(itemScissorProfit, 2),
                    ComprehensiveNetProfit = Math.Round(comprehensiveNetProfit, 2),
                    NetMarginRate = Math.Round(netMarginRate, 4),
                    ComprehensiveNetProfitPerTon = Math.Round(netProfitPerTon, 2)
                };
                typedRows.Add(typedRow);

                var rowDict = new Dictionary<string, object?>
                {
                    ["序号"] = index,
                    ["清单代号"] = typedRow.ItemTag,
                    ["清单名称"] = typedRow.BillItemName,
                    ["投标工程量(t)"] = typedRow.TenderQuantity,
                    ["理论净重(t)"] = typedRow.NetWeightTon,
                    ["1.15采购毛重(t)"] = typedRow.GrossWeightTon,
                    ["材料费小计(元)"] = typedRow.MaterialCostSum,
                    ["吨钢材料费(元/t)"] = typedRow.MaterialTonCost,
                    ["制作工费小计(元)"] = typedRow.FabricationCostSum,
                    ["吨钢制作费(元/t)"] = typedRow.FabricationTonCost,
                    ["安装工费小计(元)"] = typedRow.InstallationCostSum,
                    ["吨钢安装费(元/t)"] = typedRow.InstallationTonCost,
                    ["防火涂装小计(元)"] = typedRow.FireCoatingCostSum,
                    ["吨钢涂装费(元/t)"] = typedRow.FireCoatingTonCost,
                    ["直接成本总额(元)"] = typedRow.DirectCostSum,
                    ["直接成本底价(元/t)"] = typedRow.DirectCostPerTon,
                    ["费率策略"] = typedRow.RateOverrideStatus,
                    ["有效管理费率(%)"] = typedRow.EffectiveManagementFeeRate,
                    ["企业管理费(元)"] = typedRow.ManagementFeeAmount,
                    ["有效利润率(%)"] = typedRow.EffectiveProfitRate,
                    ["目标利润(元)"] = typedRow.TargetProfitAmount,
                    ["对外综合单价(元/t)"] = typedRow.ComprehensiveUnitPrice,
                    ["投标总合价(元)"] = typedRow.TotalTenderPrice,
                    ["采办剪刀差创效(元)"] = typedRow.ScissorDifferenceProfit,
                    ["综合毛利总额(元)"] = typedRow.ComprehensiveNetProfit,
                    ["综合毛利率(%)"] = typedRow.NetMarginRate,
                    ["吨钢综合净利(元/t)"] = typedRow.ComprehensiveNetProfitPerTon
                };
                rows.Add(rowDict);
                index++;
            }

            double totalCompProfit = projectResult.TotalProfit + projectResult.TotalProcurementScissorProfit;
            double totalNetTonAll = projectResult.TotalNetWeightTon;
            double projectMarginRate = projectResult.TotalBidPrice > 0.0 ? totalCompProfit / projectResult.TotalBidPrice : 0.0;
            double avgDirectPerTon = totalNetTonAll > 0.0 ? projectResult.TotalDirectCost / totalNetTonAll : 0.0;
            double tonProfit = totalNetTonAll > 0.0 ? totalCompProfit / totalNetTonAll : 0.0;

            var metrics = new Dictionary<string, object?>
            {
                ["全项目总投标报价(元)"] = Math.Round(projectResult.TotalBidPrice, 2),
                ["全项目直接成本总额(元)"] = Math.Round(projectResult.TotalDirectCost, 2),
                ["材料费总额(元)"] = Math.Round(projectResult.TotalMaterialCost, 2),
                ["车间制作工费总额(元)"] = Math.Round(projectResult.TotalFabricationCost, 2),
                ["现场安装工费总额(元)"] = Math.Round(projectResult.TotalInstallationCost, 2),
                ["涂装防火总费用(元)"] = Math.Round(projectResult.TotalFireCoatingCost, 2),
                ["企业管理费总额(元)"] = Math.Round(projectResult.TotalManagementFee, 2),
                ["目标利润总额(元)"] = Math.Round(projectResult.TotalProfit, 2),
                ["采办经营创效总额(剪刀差利润, 元)"] = Math.Round(projectResult.TotalProcurementScissorProfit, 2),
                ["全项目综合毛利总额(元)"] = Math.Round(totalCompProfit, 2),
                ["全项目综合利润率(%)"] = Math.Round(projectMarginRate, 4),
                ["全项目平均直接成本底价(元/t)"] = Math.Round(avgDirectPerTon, 2),
                ["全项目吨钢综合毛利(元/t)"] = Math.Round(tonProfit, 2)
            };

            return new AudienceProjectionResult
            {
                Perspective = BoqAudiencePerspective.ChiefEconomist,
                PerspectiveName = "终极全成本穿透大盘",
                Headers = headers,
                Rows = rows,
                SummaryMetrics = metrics,
                UltimateCostRows = typedRows
            };
        }

        // -------------------------------------------------------------------------
        // 4.3 视角 3：构件加工厂 / 车间调度 (FabricationWorkshop - 聚焦板厚K_Thick、相贯切割、UT探伤与工日)
        // -------------------------------------------------------------------------
        public static AudienceProjectionResult ProjectToFabricationWorkshop(
            ProjectCostResult projectResult,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            var headers = new List<string>
            {
                "序号", "所属清单项", "构件分段编码", "构件名称", "工程结构角色",
                "制作套数", "单件重(t)", "理论总净重(kg)", "理论总净重(t)", "1.15倍采购毛重(kg)",
                "最大主板厚/壁厚(mm)", "厚板难度系数K_Thick", "基准制作工费(元/t)", "数控相贯切割工序",
                "超声波探伤UT要求", "厂内预拼装形式", "车间制作单价(元/t)", "车间制作总工费(元)",
                "车间定额工日(工日)", "焊接工艺与探伤说明"
            };

            var typedRows = new List<FabricationWorkshopRow>();
            var rows = new List<Dictionary<string, object?>>();

            int index = 1;
            foreach (var itemRes in projectResult.TenderItemResults)
            {
                foreach (var subRes in itemRes.SubProjectResults)
                {
                    var sourceSub = project?.TenderItems
                        .SelectMany(t => t.SubProjects)
                        .FirstOrDefault(s => 
                            (subRes.SubProjectId > 0 && s.Id == subRes.SubProjectId) || 
                            s.SubProjectCode == subRes.SubProjectCode);

                    string role = sourceSub?.StructuralRole ?? "主体钢构件";
                    int qty = sourceSub?.Quantity ?? 1;
                    double pieceWeight = sourceSub?.SingleWeight ?? (subRes.TotalNetWeightKg / 1000.0 / Math.Max(1, qty));
                    double maxThick = sourceSub?.MaxPlateThickness ?? 20.0;
                    var rule = sourceSub?.FabricationQuotaRule;

                    double baseLabor = rule?.BaseLaborRatePerTon ?? (role == "主桁架" ? 2200.0 : 1500.0);
                    double cutSurcharge = rule?.IntersectingCuttingUnitRate ?? 280.0;
                    double utSurcharge = rule?.UltrasonicTestingUnitRate ?? 150.0;
                    double assemblySurcharge = rule?.PreassemblyUnitRate ?? 200.0;
                    double laborDaysPerTon = rule?.BaseLaborDays ?? 1.8;

                    double netTon = subRes.TotalNetWeightKg / 1000.0;
                    double totalLaborDays = netTon * laborDaysPerTon * subRes.KThick;

                    string cutDesc = "包含数控五轴相贯切割 (+280元/t)";
                    string utDesc = "一级100%全熔透超声波探伤 (+150元/t)";
                    string assemblyDesc = "空间三维预拼装 (+200元/t)";

                    string weldingNotes;
                    if (maxThick > 60.0)
                    {
                        weldingNotes = "超特厚板焊接：严格按工艺要求进行大预热(≥150℃)与后热消氢处理，100%一级全熔透探伤";
                    }
                    else if (maxThick > 40.0)
                    {
                        weldingNotes = "厚板大焊道：焊前预热(≥100℃)，多层多道低氢焊条/埋弧自动焊，层温控制与消应力处理";
                    }
                    else if (maxThick > 20.0)
                    {
                        weldingNotes = "常规厚板组对：打磨坡口表面氧化皮，分道对称焊接控制角变形";
                    }
                    else
                    {
                        weldingNotes = "常规直切下料，自动化坡口机切坡口，按设计图纸标准探伤";
                    }

                    var typedRow = new FabricationWorkshopRow
                    {
                        Index = index,
                        ItemTag = itemRes.ItemTag,
                        SubProjectCode = subRes.SubProjectCode,
                        SubProjectName = subRes.SubProjectName,
                        StructuralRole = role,
                        Quantity = qty,
                        TotalNetWeightKg = Math.Round(subRes.TotalNetWeightKg, 2),
                        TotalNetWeightTon = Math.Round(netTon, 3),
                        TotalGrossWeightKg = Math.Round(subRes.TotalGrossWeightKg, 2),
                        MaxPlateThickness = maxThick,
                        KThick = subRes.KThick,
                        BaseLaborRatePerTon = baseLabor,
                        IntersectingCutSurcharge = cutSurcharge,
                        UltrasonicTestingFeePerTon = utSurcharge,
                        TrialAssemblyFeePerTon = assemblySurcharge,
                        UnitFabricationCost = Math.Round(subRes.FabricationCost / Math.Max(0.001, netTon), 2),
                        TotalFabricationCost = Math.Round(subRes.FabricationCost, 2),
                        BaseLaborDaysPerTon = laborDaysPerTon,
                        TotalLaborDays = Math.Round(totalLaborDays, 1),
                        WeldingNotes = weldingNotes
                    };
                    typedRows.Add(typedRow);

                    var rowDict = new Dictionary<string, object?>
                    {
                        ["序号"] = index,
                        ["所属清单项"] = typedRow.ItemTag,
                        ["构件分段编码"] = typedRow.SubProjectCode,
                        ["构件名称"] = typedRow.SubProjectName,
                        ["工程结构角色"] = typedRow.StructuralRole,
                        ["制作套数"] = typedRow.Quantity,
                        ["单件重(t)"] = Math.Round(pieceWeight, 2),
                        ["理论总净重(kg)"] = typedRow.TotalNetWeightKg,
                        ["理论总净重(t)"] = typedRow.TotalNetWeightTon,
                        ["1.15倍采购毛重(kg)"] = typedRow.TotalGrossWeightKg,
                        ["最大主板厚/壁厚(mm)"] = typedRow.MaxPlateThickness,
                        ["厚板难度系数K_Thick"] = typedRow.KThick,
                        ["基准制作工费(元/t)"] = typedRow.BaseLaborRatePerTon,
                        ["数控相贯切割工序"] = cutDesc,
                        ["超声波探伤UT要求"] = utDesc,
                        ["厂内预拼装形式"] = assemblyDesc,
                        ["车间制作单价(元/t)"] = typedRow.UnitFabricationCost,
                        ["车间制作总工费(元)"] = typedRow.TotalFabricationCost,
                        ["车间定额工日(工日)"] = typedRow.TotalLaborDays,
                        ["焊接工艺与探伤说明"] = typedRow.WeldingNotes
                    };
                    rows.Add(rowDict);
                    index++;
                }
            }

            double totalShopNetTon = typedRows.Sum(r => r.TotalNetWeightTon);
            double totalShopGrossTon = typedRows.Sum(r => r.TotalGrossWeightKg) / 1000.0;
            double totalShopCost = typedRows.Sum(r => r.TotalFabricationCost);
            double totalDays = typedRows.Sum(r => r.TotalLaborDays);
            int thickCount = typedRows.Count(r => r.MaxPlateThickness > 40.0);
            double thickWeight = typedRows.Where(r => r.MaxPlateThickness > 40.0).Sum(r => r.TotalNetWeightTon);
            double thickRatio = totalShopNetTon > 0.0 ? thickWeight / totalShopNetTon : 0.0;

            var metrics = new Dictionary<string, object?>
            {
                ["车间总加工净重(t)"] = Math.Round(totalShopNetTon, 3),
                ["车间总下料毛重(t)"] = Math.Round(totalShopGrossTon, 3),
                ["车间总需求工时(工日)"] = Math.Round(totalDays, 1),
                ["车间制作工费总额(元)"] = Math.Round(totalShopCost, 2),
                ["平均车间制作单价(元/t)"] = totalShopNetTon > 0.0 ? Math.Round(totalShopCost / totalShopNetTon, 2) : 0.0,
                ["厚板构件套数(板厚>40mm)"] = thickCount,
                ["厚板构件重量占比(%)"] = Math.Round(thickRatio * 100.0, 1)
            };

            return new AudienceProjectionResult
            {
                Perspective = BoqAudiencePerspective.FabricationShop,
                PerspectiveName = "车间制作工艺排产表",
                Headers = headers,
                Rows = rows,
                SummaryMetrics = metrics,
                FabricationRows = typedRows
            };
        }

        // -------------------------------------------------------------------------
        // 4.4 视角 4：现场安装 / 机械吊装 (SiteErectionSchedule - 聚焦标高K_Elev、现场焊K_SiteWeld、吊重K_Weight)
        // -------------------------------------------------------------------------
        public static AudienceProjectionResult ProjectToSiteErectionSchedule(
            ProjectCostResult projectResult,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            var headers = new List<string>
            {
                "序号", "所属清单项", "构件分段编码", "构件名称", "现场拼装工法",
                "构件套数", "单件吊装重量(t)", "总吊装净重(t)", "实际就位标高(m)", "标高超高系数K_Elev",
                "是否高空原位全熔透焊", "现场焊工法系数K_SiteWeld", "主起重机吨位分档", "吊装重量系数K_Weight",
                "现场安装直接工费(元/t)", "主起重机械费(元/t)", "胎架操作平台措施费(元/t)", "现场安装综合单价(元/t)",
                "现场安装总费用(元)", "机械配置建议"
            };

            var typedRows = new List<SiteErectionScheduleRow>();
            var rows = new List<Dictionary<string, object?>>();

            int index = 1;
            foreach (var itemRes in projectResult.TenderItemResults)
            {
                foreach (var subRes in itemRes.SubProjectResults)
                {
                    var sourceSub = project?.TenderItems
                        .SelectMany(t => t.SubProjects)
                        .FirstOrDefault(s => 
                            (subRes.SubProjectId > 0 && s.Id == subRes.SubProjectId) || 
                            s.SubProjectCode == subRes.SubProjectCode);

                    string role = sourceSub?.StructuralRole ?? "主体钢构件";
                    int qty = sourceSub?.Quantity ?? 1;
                    double pieceWeight = sourceSub?.SingleWeight ?? (subRes.TotalNetWeightKg / 1000.0 / Math.Max(1, qty));
                    double elev = sourceSub?.AssemblyElevation ?? 15.0;
                    bool fieldWeld = sourceSub?.HasFieldWelding ?? false;
                    string craneTier = sourceSub?.CraneTonnageTier ?? "<5t";
                    string installMethod = sourceSub?.InstallationMethod ?? (fieldWeld ? "高空散拼原位组焊" : "厂内拼装整体吊装");

                    var installRule = sourceSub?.InstallationQuotaRule;
                    double baseInstallRate = installRule?.BaseLaborRatePerTon ?? 1800.0;
                    double craneRate = installRule?.MainCraneRatePerTon ?? 650.0;
                    double scaffoldRate = installRule?.TrestlePlatformRatePerTon ?? 300.0;

                    double netTon = subRes.TotalNetWeightKg / 1000.0;

                    string craneRec;
                    if (pieceWeight > 25.0)
                    {
                        craneRec = "建议配置 500t~650t 重型履带吊或双机抬吊，编制超重超高专项吊装方案";
                    }
                    else if (pieceWeight > 15.0)
                    {
                        craneRec = "建议配置 300t~400t 履带起重机，复核高空就位作业半径";
                    }
                    else if (pieceWeight >= 5.0)
                    {
                        craneRec = "建议配置 150t~260t 履带吊或全地面汽车吊配合安装";
                    }
                    else
                    {
                        craneRec = "常规散拼吊装，可使用 50t~80t 汽车吊或塔吊起吊";
                    }

                    var typedRow = new SiteErectionScheduleRow
                    {
                        Index = index,
                        ItemTag = itemRes.ItemTag,
                        SubProjectCode = subRes.SubProjectCode,
                        SubProjectName = subRes.SubProjectName,
                        StructuralRole = role,
                        Quantity = qty,
                        SingleWeightTon = Math.Round(pieceWeight, 2),
                        TotalNetWeightTon = Math.Round(netTon, 3),
                        InstallationMethod = installMethod,
                        AssemblyElevation = elev,
                        KElev = subRes.KElev,
                        HasFieldWelding = fieldWeld,
                        FieldWeldingDisplay = fieldWeld ? "是 (发现场原位焊)" : "否 (厂内组焊)",
                        KSiteWeld = subRes.KSiteWeld,
                        CraneTonnageTier = craneTier,
                        KWeight = subRes.KWeight,
                        BaseInstallationFeePerTon = baseInstallRate,
                        MainCraneRatePerTon = craneRate,
                        ScaffoldingPlatformRatePerTon = scaffoldRate,
                        UnitInstallationCost = Math.Round(subRes.InstallationCost / Math.Max(0.001, netTon), 2),
                        TotalInstallationCost = Math.Round(subRes.InstallationCost, 2),
                        CraneDeploymentRecommendation = craneRec
                    };
                    typedRows.Add(typedRow);

                    var rowDict = new Dictionary<string, object?>
                    {
                        ["序号"] = index,
                        ["所属清单项"] = typedRow.ItemTag,
                        ["构件分段编码"] = typedRow.SubProjectCode,
                        ["构件名称"] = typedRow.SubProjectName,
                        ["现场拼装工法"] = typedRow.InstallationMethod,
                        ["构件套数"] = typedRow.Quantity,
                        ["单件吊装重量(t)"] = typedRow.SingleWeightTon,
                        ["总吊装净重(t)"] = typedRow.TotalNetWeightTon,
                        ["实际就位标高(m)"] = typedRow.AssemblyElevation,
                        ["标高超高系数K_Elev"] = typedRow.KElev,
                        ["是否高空原位全熔透焊"] = typedRow.FieldWeldingDisplay,
                        ["现场焊工法系数K_SiteWeld"] = typedRow.KSiteWeld,
                        ["主起重机吨位分档"] = typedRow.CraneTonnageTier,
                        ["吊装重量系数K_Weight"] = typedRow.KWeight,
                        ["现场安装直接工费(元/t)"] = typedRow.BaseInstallationFeePerTon,
                        ["主起重机械费(元/t)"] = typedRow.MainCraneRatePerTon,
                        ["胎架操作平台措施费(元/t)"] = typedRow.ScaffoldingPlatformRatePerTon,
                        ["现场安装综合单价(元/t)"] = typedRow.UnitInstallationCost,
                        ["现场安装总费用(元)"] = typedRow.TotalInstallationCost,
                        ["机械配置建议"] = typedRow.CraneDeploymentRecommendation
                    };
                    rows.Add(rowDict);
                    index++;
                }
            }

            double totalSiteNetTon = typedRows.Sum(r => r.TotalNetWeightTon);
            double totalSiteCost = typedRows.Sum(r => r.TotalInstallationCost);
            double highAltTon = typedRows.Where(r => r.AssemblyElevation > 20.0).Sum(r => r.TotalNetWeightTon);
            double fieldWeldTon = typedRows.Where(r => r.HasFieldWelding).Sum(r => r.TotalNetWeightTon);
            int heavyCount = typedRows.Count(r => r.SingleWeightTon >= 5.0);

            var metrics = new Dictionary<string, object?>
            {
                ["现场吊装总净重(t)"] = Math.Round(totalSiteNetTon, 3),
                ["现场安装总工费(元)"] = Math.Round(totalSiteCost, 2),
                ["超高构件吊装总重(t, 标高>20m)"] = Math.Round(highAltTon, 3),
                ["超高吊装重量占比(%)"] = totalSiteNetTon > 0.0 ? Math.Round((highAltTon / totalSiteNetTon) * 100.0, 1) : 0.0,
                ["发现场原位焊接构件总重(t)"] = Math.Round(fieldWeldTon, 3),
                ["重特大构件件数(单件重>=5t)"] = heavyCount,
                ["平均现场安装单价(元/t)"] = totalSiteNetTon > 0.0 ? Math.Round(totalSiteCost / totalSiteNetTon, 2) : 0.0
            };

            return new AudienceProjectionResult
            {
                Perspective = BoqAudiencePerspective.SiteProjectDept,
                PerspectiveName = "现场安装吊装施工表",
                Headers = headers,
                Rows = rows,
                SummaryMetrics = metrics,
                SiteErectionRows = typedRows
            };
        }

        // -------------------------------------------------------------------------
        // 4.5 视角 5：物资采办 / 钢厂采购 (MaterialProcurementOrder - 跨构件归并、1.15损耗毛重、阶梯加价与剪刀差)
        // -------------------------------------------------------------------------
        public static AudienceProjectionResult ProjectToMaterialProcurementOrder(
            ProjectCostResult projectResult,
            BoqProject? project = null,
            BoqReportConfig? config = null)
        {
            ArgumentNullException.ThrowIfNull(projectResult);

            var headers = new List<string>
            {
                "序号", "钢材材质牌号", "截面形式", "规格型号", "主板厚/壁厚(mm)",
                "Z向抗撕裂性能", "下属零件总件数", "理论总净重(kg)", "理论总净重(t)", "定额损耗系数",
                "1.15采购毛重(kg)", "1.15采购毛重(t)", "材料基准到场价(元/t)", "板厚阶梯加价(元/t)",
                "材质牌号加价(元/t)", "Z向性能加价(元/t)", "区域运杂费(元/t)", "综合采购到场单价(元/t)",
                "钢厂采购预算合价(元)", "业主批复信息指导价(元/t)", "实际采办单价(元/t)", "采办经营创效剪刀差(元)",
                "钢厂订货建议"
            };

            var typedRows = new List<MaterialProcurementOrderRow>();
            var rows = new List<Dictionary<string, object?>>();

            // 提取所有物理零件
            var allParts = new List<BoqPart>();
            if (project != null)
            {
                foreach (var t in project.TenderItems)
                {
                    foreach (var s in t.SubProjects)
                    {
                        allParts.AddRange(s.Parts);
                    }
                }
            }

            // 按 (材质牌号, 截面类型, 规格型号, 厚度, Z向性能) 跨构件精细归并
            var grouped = allParts
                .GroupBy(p => new
                {
                    SteelGrade = string.IsNullOrWhiteSpace(p.SteelGrade) ? "Q355B" : p.SteelGrade.Trim(),
                    SectionType = string.IsNullOrWhiteSpace(p.SectionType) ? "PL" : p.SectionType.Trim(),
                    Specification = string.IsNullOrWhiteSpace(p.SectionSpecification) ? "" : p.SectionSpecification.Trim(),
                    Thickness = Math.Round(p.Thickness, 2),
                    ZPerformance = string.IsNullOrWhiteSpace(p.ZDirectionGrade) ? "无" : p.ZDirectionGrade.Trim()
                })
                .OrderBy(g => g.Key.SteelGrade)
                .ThenBy(g => g.Key.SectionType)
                .ThenBy(g => g.Key.Thickness);

            int index = 1;
            foreach (var g in grouped)
            {
                int partCount = g.Sum(p => Math.Max(1, p.Quantity));
                double sumNetKg = g.Sum(p => p.TotalNetWeight);
                double sumGrossKg = g.Sum(p => p.TotalGrossWeight);
                if (sumGrossKg <= 0.0) sumGrossKg = sumNetKg * 1.15;

                double netTon = sumNetKg / 1000.0;
                double grossTon = sumGrossKg / 1000.0;

                var samplePart = g.First();
                double basePrice = samplePart.BaseNetPricePerTon > 0 ? samplePart.BaseNetPricePerTon : 4050.0;
                double thickAdder = samplePart.ThicknessSurchargePerTon > 0 ? samplePart.ThicknessSurchargePerTon : CalculatePartThicknessSurcharge(g.Key.Thickness);
                double gradeAdder = samplePart.GradeSurchargePerTon > 0 ? samplePart.GradeSurchargePerTon : CalculatePartGradeSurcharge(g.Key.SteelGrade);
                double zAdder = samplePart.ZPerformanceSurchargePerTon > 0 ? samplePart.ZPerformanceSurchargePerTon : CalculatePartZPerformanceSurcharge(g.Key.ZPerformance);
                double freightAdder = samplePart.FreightSurchargePerTon > 0 ? samplePart.FreightSurchargePerTon : 80.0;

                double unitPrice = basePrice + thickAdder + gradeAdder + zAdder + freightAdder;
                double estimatedCost = grossTon * unitPrice;

                double approvedPrice = samplePart.ApprovedPricePerTon;
                double actualPrice = samplePart.ActualPurchasePricePerTon;
                double scissorProfit = g.Sum(p => p.ArbitrageProfit);
                if (scissorProfit == 0.0 && (approvedPrice > 0.0 || actualPrice > 0.0))
                {
                    scissorProfit = (approvedPrice - actualPrice) * grossTon;
                }

                string orderingNotes;
                if (g.Key.Thickness >= 40.0 && g.Key.SteelGrade.Contains("460"))
                {
                    orderingNotes = "特厚高强特种结构钢：需向大型钢厂(如宝武/沙钢)提前30天排产，注意保全厚度与心部致密性";
                }
                else if (g.Key.Thickness >= 40.0)
                {
                    orderingNotes = "特厚板定轧：注意钢厂单轧批次起订量(>=30t)，要求提供心部超声波探伤合格证明";
                }
                else
                {
                    orderingNotes = "常规规格现货/定轧：建议集中批次下达，争取钢厂直发优惠与集中物流配送";
                }

                var typedRow = new MaterialProcurementOrderRow
                {
                    Index = index,
                    MaterialGrade = g.Key.SteelGrade,
                    SectionType = g.Key.SectionType,
                    Specification = g.Key.Specification,
                    Thickness = g.Key.Thickness,
                    ZPerformance = g.Key.ZPerformance,
                    PartCount = partCount,
                    TotalNetWeightKg = Math.Round(sumNetKg, 2),
                    TotalNetWeightTon = Math.Round(netTon, 3),
                    PurchasingWasteFactor = 1.15,
                    TotalGrossWeightKg = Math.Round(sumGrossKg, 2),
                    TotalGrossWeightTon = Math.Round(grossTon, 3),
                    BaseNetPricePerTon = basePrice,
                    ThicknessSurchargePerTon = thickAdder,
                    GradeSurchargePerTon = gradeAdder,
                    ZPerformanceSurchargePerTon = zAdder,
                    FreightSurchargePerTon = freightAdder,
                    MaterialUnitPricePerTon = Math.Round(unitPrice, 2),
                    EstimatedProcurementCost = Math.Round(estimatedCost, 2),
                    ApprovedPricePerTon = approvedPrice,
                    ActualPurchasePricePerTon = actualPrice,
                    ScissorDifferenceProfit = Math.Round(scissorProfit, 2),
                    MillOrderingNotes = orderingNotes
                };
                typedRows.Add(typedRow);

                var rowDict = new Dictionary<string, object?>
                {
                    ["序号"] = index,
                    ["钢材材质牌号"] = typedRow.MaterialGrade,
                    ["截面形式"] = typedRow.SectionType,
                    ["规格型号"] = typedRow.Specification,
                    ["主板厚/壁厚(mm)"] = typedRow.Thickness,
                    ["Z向抗撕裂性能"] = typedRow.ZPerformance,
                    ["下属零件总件数"] = typedRow.PartCount,
                    ["理论总净重(kg)"] = typedRow.TotalNetWeightKg,
                    ["理论总净重(t)"] = typedRow.TotalNetWeightTon,
                    ["定额损耗系数"] = typedRow.PurchasingWasteFactor,
                    ["1.15采购毛重(kg)"] = typedRow.TotalGrossWeightKg,
                    ["1.15采购毛重(t)"] = typedRow.TotalGrossWeightTon,
                    ["材料基准到场价(元/t)"] = typedRow.BaseNetPricePerTon,
                    ["板厚阶梯加价(元/t)"] = typedRow.ThicknessSurchargePerTon,
                    ["材质牌号加价(元/t)"] = typedRow.GradeSurchargePerTon,
                    ["Z向性能加价(元/t)"] = typedRow.ZPerformanceSurchargePerTon,
                    ["区域运杂费(元/t)"] = typedRow.FreightSurchargePerTon,
                    ["综合采购到场单价(元/t)"] = typedRow.MaterialUnitPricePerTon,
                    ["钢厂采购预算合价(元)"] = typedRow.EstimatedProcurementCost,
                    ["业主批复信息指导价(元/t)"] = typedRow.ApprovedPricePerTon,
                    ["实际采办单价(元/t)"] = typedRow.ActualPurchasePricePerTon,
                    ["采办经营创效剪刀差(元)"] = typedRow.ScissorDifferenceProfit,
                    ["钢厂订货建议"] = typedRow.MillOrderingNotes
                };
                rows.Add(rowDict);
                index++;
            }

            double totalProcNetTon = typedRows.Sum(r => r.TotalNetWeightTon);
            double totalProcGrossTon = typedRows.Sum(r => r.TotalGrossWeightTon);
            double totalEstCost = typedRows.Sum(r => r.EstimatedProcurementCost);
            double totalScissor = typedRows.Sum(r => r.ScissorDifferenceProfit);
            double highThickGrossTon = typedRows.Where(r => r.Thickness >= 40.0 || r.MaterialGrade.Contains("460")).Sum(r => r.TotalGrossWeightTon);
            double avgMatPrice = totalProcGrossTon > 0.0 ? totalEstCost / totalProcGrossTon : 0.0;
            double tonScissor = totalProcGrossTon > 0.0 ? totalScissor / totalProcGrossTon : 0.0;

            var metrics = new Dictionary<string, object?>
            {
                ["采购母材理论总净重(t)"] = Math.Round(totalProcNetTon, 3),
                ["钢厂直发采购总毛重(t, 1.15)"] = Math.Round(totalProcGrossTon, 3),
                ["钢材原材料采购总预算(元)"] = Math.Round(totalEstCost, 2),
                ["高强特厚钢材订货毛重(t, Q460/T>=40)"] = Math.Round(highThickGrossTon, 3),
                ["高强特厚钢板订货占比(%)"] = totalProcGrossTon > 0.0 ? Math.Round((highThickGrossTon / totalProcGrossTon) * 100.0, 1) : 0.0,
                ["采办经营创效总额(剪刀差利润, 元)"] = Math.Round(totalScissor, 2),
                ["综合加权平均采购单价(元/t)"] = Math.Round(avgMatPrice, 2),
                ["吨钢采办创效(剪刀差利润, 元/t)"] = Math.Round(tonScissor, 2)
            };

            return new AudienceProjectionResult
            {
                Perspective = BoqAudiencePerspective.Procurement,
                PerspectiveName = "采办物资钢厂订货表",
                Headers = headers,
                Rows = rows,
                SummaryMetrics = metrics,
                MaterialProcurementRows = typedRows
            };
        }

        /// <summary>
        /// 跨构件归并材料采购汇总清单 (MaterialProcurementItem 集合)
        /// </summary>
        public static List<MaterialProcurementItem> AggregateMaterialProcurement(BoqProject project)
        {
            ArgumentNullException.ThrowIfNull(project);

            var allParts = project.TenderItems
                .SelectMany(t => t.SubProjects)
                .SelectMany(s => s.Parts)
                .ToList();

            var list = new List<MaterialProcurementItem>();

            var grouped = allParts
                .GroupBy(p => new
                {
                    SteelGrade = string.IsNullOrWhiteSpace(p.SteelGrade) ? "Q355B" : p.SteelGrade.Trim(),
                    SectionType = string.IsNullOrWhiteSpace(p.SectionType) ? "PL" : p.SectionType.Trim(),
                    Specification = string.IsNullOrWhiteSpace(p.SectionSpecification) ? "" : p.SectionSpecification.Trim(),
                    Thickness = Math.Round(p.Thickness, 2),
                    ZPerformance = string.IsNullOrWhiteSpace(p.ZDirectionGrade) ? "无" : p.ZDirectionGrade.Trim()
                })
                .OrderBy(g => g.Key.SteelGrade)
                .ThenBy(g => g.Key.SectionType)
                .ThenBy(g => g.Key.Thickness);

            foreach (var g in grouped)
            {
                int partCount = g.Sum(p => Math.Max(1, p.Quantity));
                double netKg = g.Sum(p => p.TotalNetWeight);
                double grossKg = g.Sum(p => p.TotalGrossWeight);
                if (grossKg <= 0.0) grossKg = netKg * 1.15;

                var sample = g.First();
                double unitPrice = sample.MaterialUnitPricePerTon;
                if (unitPrice <= 0.0)
                {
                    double baseP = sample.BaseNetPricePerTon > 0 ? sample.BaseNetPricePerTon : 4050.0;
                    double tAdd = CalculatePartThicknessSurcharge(g.Key.Thickness);
                    double gAdd = CalculatePartGradeSurcharge(g.Key.SteelGrade);
                    double zAdd = CalculatePartZPerformanceSurcharge(g.Key.ZPerformance);
                    unitPrice = baseP + tAdd + gAdd + zAdd + 80.0;
                }

                double cost = (grossKg / 1000.0) * unitPrice;

                list.Add(new MaterialProcurementItem
                {
                    MaterialGrade = g.Key.SteelGrade,
                    SectionType = g.Key.SectionType,
                    Specification = g.Key.Specification,
                    Thickness = g.Key.Thickness,
                    ZPerformance = g.Key.ZPerformance,
                    PartCount = partCount,
                    TotalNetWeightKg = Math.Round(netKg, 2),
                    TotalGrossWeightKg = Math.Round(grossKg, 2),
                    MaterialUnitPricePerTon = Math.Round(unitPrice, 2),
                    EstimatedMaterialCost = Math.Round(cost, 2)
                });
            }

            return list;
        }
    }
}
