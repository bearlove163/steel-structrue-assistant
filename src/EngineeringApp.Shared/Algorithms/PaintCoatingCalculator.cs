using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 建筑钢结构涂装防腐、防火涂料及热浸镀锌 (HDG) 工程造价算量引擎
/// 符合中国《钢结构涂装工程工艺规范》、GB/T 13912-2020《钢铁制件热浸镀锌层》及国际 SSPC/NACE 标准
/// </summary>
public static class PaintCoatingCalculator
{
    /// <summary>
    /// 对给定构件截面与涂层组合配置进行精细化材料用量与工程造价核算
    /// </summary>
    /// <param name="section">截面特性分析结果 (提供展开涂装面积与吨钢比表面积)</param>
    /// <param name="layers">涂层体系配置列表 (底、中、封闭、面、防火等)</param>
    /// <param name="excludeTopSurface">是否扣除楼板贴合上表面免涂装区域</param>
    /// <param name="projectLengthM">批量估算总构件长度 (m)</param>
    /// <param name="method">施工工艺工法 (无气喷涂/空气喷涂/刷滚涂)</param>
    /// <param name="sectionType">截面几何类型 (用于修正断面过喷飞溅损耗)</param>
    /// <returns>涂装防腐防火系统综合核算报告与全过程推导记录</returns>
    public static PaintSystemCalculationResult Calculate(
        SectionPropertiesResult section,
        IEnumerable<CoatingLayerConfig> layers,
        bool excludeTopSurface = false,
        double projectLengthM = 100.0,
        CoatingApplicationMethod method = CoatingApplicationMethod.AirlessSpraying,
        SectionType sectionType = SectionType.HBeam)
    {
        double kSection = SectionShapeLossConfig.GetSectionShapeFactor(sectionType);
        double kMethod = SectionShapeLossConfig.GetMethodLossFactor(method);
        double kLabor = SectionShapeLossConfig.GetMethodLaborMultiplier(method);

        // 截面形状系数 F/V (m⁻¹) = (P_outer / A) * 1000
        double outerP = section.OuterPerimeter > 0 ? section.OuterPerimeter : section.Perimeter;
        double fOverV = section.Area > 1e-4 ? (outerP / section.Area) * 1000.0 : 0;

        var result = new PaintSystemCalculationResult
        {
            ApplicationMethod = method,
            SectionShapeLossFactor = kSection,
            MethodLossFactor = kMethod,
            SectionFactorFOverV = fOverV,
            ProjectTotalLengthM = projectLengthM > 0 ? projectLengthM : 100.0,
            TopSurfaceExcluded = excludeTopSurface,
            AreaPerTon = section.AreaPerTon
        };

        // 确定计算采用的延米展开面积 (m²/m)
        double appliedAreaPerM = excludeTopSurface && section.PaintingAreaExcludingTop > 0
            ? section.PaintingAreaExcludingTop
            : section.GrossPaintingAreaPerMeter;

        if (appliedAreaPerM <= 0)
        {
            appliedAreaPerM = Math.Max(0.01, outerP / 1000.0);
        }

        result.AppliedPaintingAreaPerMeter = appliedAreaPerM;

        double totalDftMicrons = 0;
        double fireproofThicknessMm = 0;
        double totalMatCostM2 = 0;
        double totalLaborCostM2 = 0;
        double totalUsageKgM2 = 0;

        var trace = new PaintCalculationProcessTrace
        {
            SectionShapeExplanation = GetSectionShapeExplanation(sectionType, kSection),
            MethodExplanation = GetMethodExplanation(method, kMethod, kLabor)
        };

        foreach (var cfg in layers.Where(l => l.IsEnabled))
        {
            var layerRes = CalculateLayer(cfg, appliedAreaPerM, kSection, method);
            result.Layers.Add(layerRes);

            if (cfg.LayerType == CoatingLayerType.Fireproof)
            {
                fireproofThicknessMm += cfg.Dft; // 防火涂料单位已为 mm
            }
            else
            {
                totalDftMicrons += cfg.Dft; // 常规油漆单位为 μm
            }

            totalUsageKgM2 += layerRes.ActualUsageKgPerM2;
            totalMatCostM2 += layerRes.MaterialCostPerM2;
            totalLaborCostM2 += layerRes.LaborCostPerM2;

            // 记录推导单步
            trace.Steps.Add(new PaintCalculationTraceStep
            {
                Title = $"{layerRes.LayerName}: {layerRes.ProductName}",
                Formula = "理论涂布率 C_th = (VS% × 10) / DFT / 密度; 实际单耗 = 1 / (C_th × (1 - 损耗率))",
                ValueSubstitution = $"DFT={layerRes.Dft}{(cfg.LayerType == CoatingLayerType.Fireproof ? "mm" : "μm")}, VS={cfg.VolumeSolids}%, 密度={cfg.Density}kg/L, 综合损耗率={layerRes.EffectiveLossRatePercent:F1}%",
                Conclusion = $"理论涂布率={layerRes.TheoreticalCoverageM2Kg:F2} m²/kg, 实际单耗={layerRes.ActualUsageKgPerM2:F3} kg/m², 平米直接费=¥{layerRes.TotalCostPerM2:F2}/m²"
            });
        }

        result.TotalPaintDftMicrons = totalDftMicrons;
        result.FireproofThicknessMm = fireproofThicknessMm;
        result.TotalMaterialCostPerM2 = totalMatCostM2;
        result.TotalLaborCostPerM2 = totalLaborCostM2;
        result.TotalCostPerM2 = totalMatCostM2 + totalLaborCostM2;

        // 延米指标
        result.TotalUsageKgPerMeter = totalUsageKgM2 * appliedAreaPerM;
        result.TotalCostPerMeter = result.TotalCostPerM2 * appliedAreaPerM;

        // 吨钢指标 (利用比表面积 m²/t)
        double areaPerTon = section.AreaPerTon > 0
            ? (excludeTopSurface && section.PaintingAreaExcludingTop > 0
                ? section.PaintingAreaExcludingTop / Math.Max(1e-4, section.LinearMass / 1000.0)
                : section.AreaPerTon)
            : (section.LinearMass > 0 ? appliedAreaPerM / (section.LinearMass / 1000.0) : 0);

        result.TotalUsageKgPerTon = totalUsageKgM2 * areaPerTon;
        result.TotalCostPerTon = result.TotalCostPerM2 * areaPerTon;
        result.ProcessTrace = trace;

        // 生成标准化技术规格描述
        result.FullTechnicalDescribe = GenerateSpecificationDescribe(result, section, method, sectionType);

        return result;
    }

    /// <summary>
    /// 计算单个涂层的理论涂布率、实际消耗量及平米单价
    /// </summary>
    public static CoatingLayerResult CalculateLayer(
        CoatingLayerConfig cfg,
        double appliedAreaPerM,
        double sectionShapeFactor = 1.0,
        CoatingApplicationMethod method = CoatingApplicationMethod.AirlessSpraying)
    {
        double kMethodLoss = SectionShapeLossConfig.GetMethodLossFactor(method);
        double kMethodLabor = SectionShapeLossConfig.GetMethodLaborMultiplier(method);

        var res = new CoatingLayerResult
        {
            LayerType = cfg.LayerType,
            LayerName = cfg.GetDisplayName(),
            ProductName = string.IsNullOrWhiteSpace(cfg.ProductName) ? cfg.GetDisplayName() : cfg.ProductName,
            Dft = cfg.Dft
        };

        // 有效干膜厚度 (微米 μm)
        double dftMicrons = cfg.LayerType == CoatingLayerType.Fireproof
            ? Math.Max(0.1, cfg.Dft * 1000.0) // mm 转 μm
            : Math.Max(1.0, cfg.Dft);

        double vs = Math.Clamp(cfg.VolumeSolids, 5.0, 100.0);
        double density = Math.Max(0.5, cfg.Density);

        // 综合损耗率 = 基准损耗率 * 断面系数 * 工法系数
        double baseLoss = Math.Clamp(cfg.LossRate, 0.0, 80.0) / 100.0;
        double effectiveLoss = Math.Clamp(baseLoss * sectionShapeFactor * kMethodLoss, 0.05, 0.75);
        res.EffectiveLossRatePercent = effectiveLoss * 100.0;

        // 理论涂布率 (m²/L) = (VS% × 10) / DFT(μm)
        double theoreticalCoverageL = (vs * 10.0) / dftMicrons;

        // 理论涂布率 (m²/kg) = (m²/L) / 密度(kg/L)
        double theoreticalCoverageKg = theoreticalCoverageL / density;
        res.TheoreticalCoverageM2Kg = theoreticalCoverageKg;

        // 实际消耗量 (kg/m²) = 1 / (理论涂布率 × (1 - 损耗率))
        double actualUsageKgPerM2 = 1.0 / (Math.Max(1e-4, theoreticalCoverageKg) * (1.0 - effectiveLoss));
        res.ActualUsageKgPerM2 = actualUsageKgPerM2;

        // 成本计算 (工费受工法系数修正，例如刷滚工费提高)
        res.MaterialCostPerM2 = actualUsageKgPerM2 * Math.Max(0, cfg.MaterialPrice);
        res.LaborCostPerM2 = Math.Max(0, cfg.LaborPrice) * kMethodLabor;

        // 延米用量与造价
        res.LinearUsageKgPerMeter = actualUsageKgPerM2 * appliedAreaPerM;
        res.LinearCostPerMeter = res.TotalCostPerM2 * appliedAreaPerM;

        return res;
    }

    /// <summary>
    /// 钢结构热浸镀锌 (HDG) 专项精确算量与商业造价核算
    /// 深度贯彻行业规律：吨加工报价由比表面积(m²/t)折算锌耗，小构件高吨价 vs 重型构件低吨价
    /// </summary>
    public static HotDipGalvanizingResult CalculateHotDipGalvanizing(
        SectionPropertiesResult section,
        HotDipGalvanizingOptions options)
    {
        var res = new HotDipGalvanizingResult
        {
            TargetDftMicrons = Math.Max(30.0, options.DftMicrons),
            LinearMassKgM = Math.Max(0.1, section.LinearMass),
            ProjectLengthM = Math.Max(1.0, options.ProjectLengthM)
        };

        // 确定参与浸锌的表面积 (m²/m)
        // 若为开孔管、角钢或内外双面镀锌，计入内外总周长
        double outerP = section.OuterPerimeter > 0 ? section.OuterPerimeter : section.Perimeter;
        double appliedPerimeter = options.IsBothSidesGalvanized
            ? (outerP + section.InnerPerimeter)
            : outerP;

        if (appliedPerimeter <= 0) appliedPerimeter = outerP;
        double appliedAreaM2PerM = appliedPerimeter / 1000.0;
        res.AppliedAreaPerMeter = appliedAreaM2PerM;

        // 吨钢比表面积 S_ton (m²/t)
        double sTon = appliedAreaM2PerM / (res.LinearMassKgM / 1000.0);
        res.SpecificSurfaceAreaTon = sTon;

        // 单位面积纯锌附着量 W_zn (g/m²) = 厚度(μm) * 锌密度 7.14 g/cm³
        double zincWeightGM2 = res.TargetDftMicrons * 7.14;
        res.ZincWeightPerM2 = zincWeightGM2;

        // 每吨钢耗锌量 (kg/t) = S_ton * W_zn * 10^-3 * (1 + 锌损率)
        double zincLossMultiplier = 1.0 + (Math.Clamp(options.ZincLossRatePercent, 5.0, 40.0) / 100.0);
        double zincPerTonKg = sTon * (zincWeightGM2 / 1000.0) * zincLossMultiplier;
        res.ZincConsumptionKgPerTon = zincPerTonKg;

        // 锌锭纯度价格调整
        double baseZincPriceTon = Math.Max(10000.0, options.ZincPricePerTon);
        double effectiveZincPrice = options.ZincGrade switch
        {
            ZincIngotGrade.Grade0_HighPurity_99995 => baseZincPriceTon + 500.0, // 0# 特级高纯锌溢价
            ZincIngotGrade.Grade1_Standard_9999 => baseZincPriceTon,           // 1# 优质纯锌基准
            ZincIngotGrade.Secondary_CrudeZinc => baseZincPriceTon - 800.0,    // 再生粗锌折让
            _ => baseZincPriceTon
        };
        res.EffectiveZincPricePerTon = effectiveZincPrice;
        res.ZincGradeName = options.ZincGrade switch
        {
            ZincIngotGrade.Grade0_HighPurity_99995 => "0# 特级高纯锌 (Zn ≥ 99.995%)",
            ZincIngotGrade.Grade1_Standard_9999 => "1# 优质结构纯锌 (Zn ≥ 99.99%)",
            ZincIngotGrade.Secondary_CrudeZinc => "次级再生粗锌 (Zn 98.5%~99.5%)",
            _ => "国标优质锌"
        };

        // 吨钢锌材费用 (元/吨钢)
        double zincMatCostTon = zincPerTonKg * (effectiveZincPrice / 1000.0);
        res.ZincMaterialCostPerTon = zincMatCostTon;

        // 吨钢基础酸洗、助镀与能耗工费 (元/吨钢)
        // 小件构件挂具人工繁琐，若比表面积很大于40，则加收绑扎工费
        double baseLaborFee = options.BaseAcidAndEnergyCostPerTon;
        if (sTon > 45.0)
        {
            baseLaborFee += 250.0; // 小件挂具附加
        }
        res.BaseProcessingCostPerTon = baseLaborFee;

        // 超长双浸倒头与复杂内腔工艺溢价
        double surcharge = 0.0;
        if (options.IsDoubleDipOverlength)
        {
            surcharge += baseLaborFee * 0.25; // 超长需两次倒头浸锌 +25%
        }
        if (options.HasComplexCavities)
        {
            surcharge += baseLaborFee * 0.15; // 复杂闭腔开孔排气 +15%
        }
        res.OverlengthSurchargePerTon = surcharge;

        // 综合吨单价 (元/吨)
        double totalPriceTon = zincMatCostTon + baseLaborFee + surcharge;
        res.TotalPricePerTon = totalPriceTon;

        // 延米单价 (元/m)
        res.LinearPricePerMeter = totalPriceTon * (res.LinearMassKgM / 1000.0);

        // 每平方米折算单价 (元/m²)
        res.PricePerM2 = appliedAreaM2PerM > 1e-4 ? res.LinearPricePerMeter / appliedAreaM2PerM : 0;

        // 行业机理解析与说明
        res.MechanismExplanation = $"【热浸镀锌商业定价本质】当前截面延米自重 {res.LinearMassKgM:F1}kg/m，浸锌比表面积高达 {sTon:F1}m²/t。" +
            (sTon >= 40.0
                ? "本构件属于典型的【高比表面积轻型构件】(类似输变电铁塔小角钢或薄壁圆管)，1吨钢带出锌液高达 " + $"{zincPerTonKg:F1}kg (增重率 {res.WeightIncreasePercent:F1}%)，因此纯锌材料费占比较大，导致市场吨报价居高不下。"
                : "本构件属于典型的【低比表面积重型构件】(大型梁柱厚板)，1吨钢带出锌液仅 " + $"{zincPerTonKg:F1}kg (增重率 {res.WeightIncreasePercent:F1}%)，纯锌消耗少，基础吨加工费较为经济。") +
            (options.IsDoubleDipOverlength ? " 注意：单件长度已勾选超限双浸，已计入 25% 倒头工艺溢价。" : "");

        res.FullDescribeText = $"热浸镀锌技术规格: GB/T 13912-2020 | 纯度: {res.ZincGradeName} | 设计厚度: {res.TargetDftMicrons:F0}μm ({zincWeightGM2:F0}g/m²) | " +
            $"比表面积: {sTon:F1}m²/t | 耗锌量: {zincPerTonKg:F1}kg/t (增重率 {res.WeightIncreasePercent:F1}%) | 综合单价: ¥{totalPriceTon:F0}/吨 (¥{res.LinearPricePerMeter:F2}/m, ¥{res.PricePerM2:F2}/m²)";

        return res;
    }

    /// <summary>
    /// 获取常用行业施工方案预设配置 (包含 5 大经典工程配套)
    /// </summary>
    public static List<CoatingLayerConfig> CreatePreset(string presetName) => presetName switch
    {
        "底漆 + 中漆" =>
        [
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Barrier 80", LayerType = CoatingLayerType.Primer, ProductName = "佐敦 Barrier 80 环氧富锌底漆", Dft = 70, VolumeSolids = 65, Density = 2.45, LossRate = 25, MaterialPrice = 48, LaborPrice = 13 },
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Jotamastic 90", LayerType = CoatingLayerType.Intermediate, ProductName = "佐敦 Jotamastic 90 环氧云铁中漆", Dft = 100, VolumeSolids = 80, Density = 1.55, LossRate = 25, MaterialPrice = 38, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Topcoat, IsEnabled = false, ProductName = "脂肪族聚氨酯面漆", Dft = 60, VolumeSolids = 60, Density = 1.30, LossRate = 30, MaterialPrice = 50, LaborPrice = 15 },
            new CoatingLayerConfig { BrandId = "BBMG", LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 中漆 + 面漆" or "标准工业厂房重防腐" =>
        [
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Barrier 80", LayerType = CoatingLayerType.Primer, ProductName = "佐敦 Barrier 80 环氧富锌底漆 (80%锌粉)", Dft = 70, VolumeSolids = 65, Density = 2.45, LossRate = 25, MaterialPrice = 48, LaborPrice = 13 },
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Jotamastic 90", LayerType = CoatingLayerType.Intermediate, ProductName = "佐敦 Jotamastic 90 高固含环氧云铁中漆", Dft = 100, VolumeSolids = 80, Density = 1.55, LossRate = 25, MaterialPrice = 38, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Hardtop XP", LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "佐敦 Hardtop XP 脂肪族聚氨酯耐候面漆", Dft = 60, VolumeSolids = 63, Density = 1.35, LossRate = 30, MaterialPrice = 52, LaborPrice = 16 },
            new CoatingLayerConfig { BrandId = "BBMG", LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 封闭漆 + 面漆" or "沿海高腐蚀 C4/C5 氟碳体系" =>
        [
            new CoatingLayerConfig { BrandId = "AkzoNobel", ModelCode = "Interzinc 52", LayerType = CoatingLayerType.Primer, ProductName = "阿克苏 Interzinc 52 高性能环氧富锌底漆", Dft = 75, VolumeSolids = 60, Density = 2.40, LossRate = 25, MaterialPrice = 49, LaborPrice = 14 },
            new CoatingLayerConfig { BrandId = "AkzoNobel", ModelCode = "Intergard 475HS", LayerType = CoatingLayerType.Intermediate, IsEnabled = false, ProductName = "阿克苏 Intergard 475HS 环氧厚浆中间漆", Dft = 100, VolumeSolids = 80, Density = 1.60, LossRate = 25, MaterialPrice = 39, LaborPrice = 13 },
            new CoatingLayerConfig { BrandId = "Hempel", ModelCode = "05990", LayerType = CoatingLayerType.Sealer, IsEnabled = true, ProductName = "海虹老人 Hempadur 环氧封闭清漆", Dft = 30, VolumeSolids = 40, Density = 1.10, LossRate = 20, MaterialPrice = 43, LaborPrice = 9 },
            new CoatingLayerConfig { BrandId = "SKK", ModelCode = "Cerami Clean", LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "四国化研 (SKK) 陶瓷氟碳重防腐超耐候面漆", Dft = 50, VolumeSolids = 52, Density = 1.22, LossRate = 30, MaterialPrice = 95, LaborPrice = 24 },
            new CoatingLayerConfig { BrandId = "BBMG", LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 中漆 + 防火涂料" =>
        [
            new CoatingLayerConfig { BrandId = "Hempel", ModelCode = "17360", LayerType = CoatingLayerType.Primer, ProductName = "海虹老人 Hempadur 环氧防锈底漆", Dft = 60, VolumeSolids = 65, Density = 2.20, LossRate = 25, MaterialPrice = 46, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Hempel", ModelCode = "47950", LayerType = CoatingLayerType.Intermediate, ProductName = "海虹老人 Hempadur MIO 云铁中间漆", Dft = 80, VolumeSolids = 65, Density = 1.50, LossRate = 25, MaterialPrice = 32, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 25, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Topcoat, IsEnabled = false, ProductName = "丙烯酸面漆", Dft = 50, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 42, LaborPrice = 14 },
            new CoatingLayerConfig { BrandId = "BBMG", ModelCode = "TL-S1520", LayerType = CoatingLayerType.Fireproof, IsEnabled = true, ProductName = "金隅天龙室内超薄膨胀防火涂料 (2.0h)", Dft = 2.2, VolumeSolids = 72, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 30 }
        ],

        "底漆 + 中漆 + 防火涂料 + 面漆" or "商业建筑室内 2.0h 膨胀防火体系" or "底+中+防火+面" =>
        [
            new CoatingLayerConfig { BrandId = "Hempel", ModelCode = "17360", LayerType = CoatingLayerType.Primer, ProductName = "海虹老人 Hempadur 环氧防锈底漆", Dft = 60, VolumeSolids = 65, Density = 2.20, LossRate = 25, MaterialPrice = 46, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Hempel", ModelCode = "47950", LayerType = CoatingLayerType.Intermediate, ProductName = "海虹老人 Hempadur MIO 云铁中间漆", Dft = 80, VolumeSolids = 65, Density = 1.50, LossRate = 25, MaterialPrice = 32, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 25, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "BBMG", ModelCode = "TL-S1520", LayerType = CoatingLayerType.Fireproof, IsEnabled = true, ProductName = "金隅天龙室内超薄膨胀防火涂料 (2.0h)", Dft = 2.2, VolumeSolids = 72, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 30 },
            new CoatingLayerConfig { BrandId = "Jotun", ModelCode = "Hardtop XP", LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "丙烯酸聚氨酯装饰面漆", Dft = 50, VolumeSolids = 60, Density = 1.30, LossRate = 30, MaterialPrice = 50, LaborPrice = 15 }
        ],

        "轻钢厂房常温防腐" or "底漆 + 面漆" =>
        [
            new CoatingLayerConfig { BrandId = "Lanling", LayerType = CoatingLayerType.Primer, ProductName = "兰陵防锈底漆 (醇酸/环氧)", Dft = 50, VolumeSolids = 55, Density = 1.30, LossRate = 25, MaterialPrice = 24, LaborPrice = 9 },
            new CoatingLayerConfig { BrandId = "Lanling", LayerType = CoatingLayerType.Intermediate, IsEnabled = false, ProductName = "环氧云铁中间漆", Dft = 80, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Lanling", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭清漆", Dft = 25, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "Lanling", LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "兰陵高光醇酸磁漆 / 调和漆", Dft = 40, VolumeSolids = 50, Density = 1.20, LossRate = 25, MaterialPrice = 28, LaborPrice = 10 },
            new CoatingLayerConfig { BrandId = "BBMG", LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "超高层钢柱 3.0h 厚型非膨胀防火体系" =>
        [
            new CoatingLayerConfig { BrandId = "AkzoNobel", ModelCode = "Interzinc 52", LayerType = CoatingLayerType.Primer, ProductName = "无机硅酸锌 / 环氧重防腐底漆", Dft = 75, VolumeSolids = 60, Density = 2.40, LossRate = 25, MaterialPrice = 49, LaborPrice = 14 },
            new CoatingLayerConfig { BrandId = "AkzoNobel", ModelCode = "Intergard 475HS", LayerType = CoatingLayerType.Intermediate, IsEnabled = false, ProductName = "环氧厚浆中间漆", Dft = 80, VolumeSolids = 70, Density = 1.50, LossRate = 25, MaterialPrice = 36, LaborPrice = 12 },
            new CoatingLayerConfig { BrandId = "Jotun", LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { BrandId = "Lanling", LayerType = CoatingLayerType.Topcoat, IsEnabled = false, ProductName = "保护面漆", Dft = 50, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 45, LaborPrice = 15 },
            new CoatingLayerConfig { BrandId = "BBMG", ModelCode = "TL-H2530", LayerType = CoatingLayerType.Fireproof, IsEnabled = true, ProductName = "金隅天龙室内厚型非膨胀无机防火涂料 (3.0h)", Dft = 25.0, VolumeSolids = 85, Density = 0.65, LossRate = 35, MaterialPrice = 12, LaborPrice = 48 }
        ],

        _ => CreatePreset("标准工业厂房重防腐")
    };

    private static string GetSectionShapeExplanation(SectionType type, double factor) => type switch
    {
        SectionType.CHS or SectionType.Circle =>
            $"【曲面断面喷涂损耗放大 (K={factor:F2})】：圆管/圆形由于圆弧曲面无垂直靶面，喷枪扫掠时边缘切线方向大量漆雾直接飞散逸出目标，产生严重过喷 (Overspray)，损耗率显著高于平整箱型约 35%~40%。",
        SectionType.Angle =>
            $"【角钢狭窄肢件穿透损耗 (K={factor:F2})】：角钢肢件宽度小、空间网架空隙多，喷幅大部分落空穿透，飞溅损耗高达 45%~55%。",
        SectionType.HBeam =>
            $"【直角阴角涡流轻微放大 (K={factor:F2})】：腹板与翼缘内拐角直角处喷涂气流易产生反射反弹涡流，损耗略高于纯平板约 15%。",
        SectionType.RHS or SectionType.Rectangle =>
            $"【平整箱型基准最低损耗 (K={factor:F2})】：箱型与方管四面均为开阔平整靶面，喷幅有效着漆率最高，反弹极小，损耗仅 20%~25%。",
        _ => $"断面损耗修正系数为 K={factor:F2}。"
    };

    private static string GetMethodExplanation(CoatingApplicationMethod method, double kLoss, double kLabor) => method switch
    {
        CoatingApplicationMethod.AirlessSpraying =>
            $"【高压无气喷涂 (基准工效)】：出漆量大、雾化均匀、一道成膜厚，适用于车间与大面积施工，损耗系数 K={kLoss:F2}，工效高人工单价基准。",
        CoatingApplicationMethod.AirSpraying =>
            $"【常规空气喷涂 (飞散偏高)】：雾化极细腻平整，但压缩气流带出大量漆雾在空气中飞扬弥散，损耗扩大至 K={kLoss:F2}，人工单价微调 ×{kLabor:F2}。",
        CoatingApplicationMethod.BrushOrRoller =>
            $"【手工刷涂/滚涂 (零飞溅但工效低)】：漆料直接涂布转移至基底，几乎无飞散损耗 (损耗系数仅 K={kLoss:F2})，非常省料；但工效低下无法快速建厚膜，人工工费单价显著提高 ×{kLabor:F2}。",
        _ => "常规工法。"
    };

    private static string GenerateSpecificationDescribe(
        PaintSystemCalculationResult result,
        SectionPropertiesResult section,
        CoatingApplicationMethod method,
        SectionType sectionType)
    {
        string layersStr = string.Join(" + ", result.Layers.Select(l =>
            $"{l.ProductName}({(l.LayerType == CoatingLayerType.Fireproof ? l.Dft + "mm" : l.Dft + "μm")})"));

        string methodStr = method switch
        {
            CoatingApplicationMethod.AirlessSpraying => "高压无气喷涂",
            CoatingApplicationMethod.AirSpraying => "空气喷涂",
            CoatingApplicationMethod.BrushOrRoller => "手工刷涂/滚涂",
            _ => "喷涂"
        };

        return $"涂装防腐与防火技术规格: 截面 {sectionType} (自重 {section.LinearMass:F1}kg/m, 外表面积 {result.AppliedPaintingAreaPerMeter:F3}m²/m, 形状系数 F/V={result.SectionFactorFOverV:F1}m⁻¹, 吨钢比表面积 {result.AreaPerTon:F1}m²/t) | " +
               $"工法: {methodStr} | 配套体系: [{layersStr}] | 总干膜厚度: {result.TotalPaintDftMicrons:F0}μm" +
               (result.FireproofThicknessMm > 0 ? $" + 防火厚度 {result.FireproofThicknessMm:F1}mm" : "") +
               $" | 综合单价: ¥{result.TotalCostPerM2:F2}/m² (延米 ¥{result.TotalCostPerMeter:F2}/m, 吨钢 ¥{result.TotalCostPerTon:F0}/t) | 扣除楼板贴合面: {(result.TopSurfaceExcluded ? "是" : "否")}";
    }
}
