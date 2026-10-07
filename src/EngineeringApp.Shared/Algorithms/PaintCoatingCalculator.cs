using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 建筑钢结构涂装防腐与防火涂料工程造价算量引擎
/// 符合中国《钢结构涂装工程工艺规范》及国际防腐涂装 SSPC/NACE 理论涂布率标准
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
    /// <returns>涂装防腐防火系统综合核算报告</returns>
    public static PaintSystemCalculationResult Calculate(
        SectionPropertiesResult section,
        IEnumerable<CoatingLayerConfig> layers,
        bool excludeTopSurface = false,
        double projectLengthM = 100.0)
    {
        var result = new PaintSystemCalculationResult
        {
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
            appliedAreaPerM = Math.Max(0.01, section.Perimeter / 1000.0);
        }

        result.AppliedPaintingAreaPerMeter = appliedAreaPerM;

        double totalDftMicrons = 0;
        double fireproofThicknessMm = 0;
        double totalMatCostM2 = 0;
        double totalLaborCostM2 = 0;
        double totalUsageKgM2 = 0;

        foreach (var cfg in layers.Where(l => l.IsEnabled))
        {
            var layerRes = CalculateLayer(cfg, appliedAreaPerM);
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
                ? section.PaintingAreaExcludingTop / (section.LinearMass / 1000.0)
                : section.AreaPerTon)
            : (section.LinearMass > 0 ? appliedAreaPerM / (section.LinearMass / 1000.0) : 0);

        result.TotalUsageKgPerTon = totalUsageKgM2 * areaPerTon;
        result.TotalCostPerTon = result.TotalCostPerM2 * areaPerTon;

        return result;
    }

    /// <summary>
    /// 计算单个涂层的理论涂布率、实际消耗量及平米单价
    /// </summary>
    public static CoatingLayerResult CalculateLayer(CoatingLayerConfig cfg, double appliedAreaPerM)
    {
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
        double lossRate = Math.Clamp(cfg.LossRate, 0.0, 80.0) / 100.0;

        // 理论涂布率 (m²/L) = (VS% × 10) / DFT(μm)
        double theoreticalCoverageL = (vs * 10.0) / dftMicrons;

        // 理论涂布率 (m²/kg) = (m²/L) / 密度(kg/L)
        double theoreticalCoverageKg = theoreticalCoverageL / density;
        res.TheoreticalCoverageM2Kg = theoreticalCoverageKg;

        // 实际消耗量 (kg/m²) = 1 / (理论涂布率 × (1 - 损耗率))
        double actualUsageKgPerM2 = 1.0 / (Math.Max(1e-4, theoreticalCoverageKg) * (1.0 - lossRate));
        res.ActualUsageKgPerM2 = actualUsageKgPerM2;

        // 成本计算
        res.MaterialCostPerM2 = actualUsageKgPerM2 * Math.Max(0, cfg.MaterialPrice);
        res.LaborCostPerM2 = Math.Max(0, cfg.LaborPrice);

        // 延米用量与造价
        res.LinearUsageKgPerMeter = actualUsageKgPerM2 * appliedAreaPerM;
        res.LinearCostPerMeter = res.TotalCostPerM2 * appliedAreaPerM;

        return res;
    }

    /// <summary>
    /// 获取常用行业施工方案预设配置
    /// </summary>
    public static List<CoatingLayerConfig> CreatePreset(string presetName) => presetName switch
    {
        "底漆 + 中漆" =>
        [
            new CoatingLayerConfig { LayerType = CoatingLayerType.Primer, ProductName = "环氧富锌底漆", Dft = 70, VolumeSolids = 65, Density = 1.45, LossRate = 25, MaterialPrice = 38, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Intermediate, ProductName = "环氧云铁中间漆", Dft = 100, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Topcoat, IsEnabled = false, ProductName = "丙烯酸聚氨酯面漆", Dft = 60, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 45, LaborPrice = 15 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料(2.0h)", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 中漆 + 面漆" =>
        [
            new CoatingLayerConfig { LayerType = CoatingLayerType.Primer, ProductName = "环氧富锌底漆", Dft = 70, VolumeSolids = 65, Density = 1.45, LossRate = 25, MaterialPrice = 38, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Intermediate, ProductName = "环氧云铁中间漆", Dft = 100, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "丙烯酸聚氨酯面漆", Dft = 60, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 45, LaborPrice = 15 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料(2.0h)", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 封闭漆 + 面漆" =>
        [
            new CoatingLayerConfig { LayerType = CoatingLayerType.Primer, ProductName = "无机富锌底漆", Dft = 75, VolumeSolids = 62, Density = 1.50, LossRate = 25, MaterialPrice = 42, LaborPrice = 14 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Intermediate, IsEnabled = false, ProductName = "环氧云铁中间漆", Dft = 80, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Sealer, IsEnabled = true, ProductName = "环氧封闭漆", Dft = 35, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 10 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "氟碳重防腐面漆", Dft = 50, VolumeSolids = 50, Density = 1.20, LossRate = 30, MaterialPrice = 75, LaborPrice = 20 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Fireproof, IsEnabled = false, ProductName = "室内膨胀型防火涂料", Dft = 2.0, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 25 }
        ],

        "底漆 + 中漆 + 防火涂料" =>
        [
            new CoatingLayerConfig { LayerType = CoatingLayerType.Primer, ProductName = "环氧防锈底漆", Dft = 60, VolumeSolids = 58, Density = 1.35, LossRate = 25, MaterialPrice = 32, LaborPrice = 10 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Intermediate, ProductName = "环氧云铁中间漆", Dft = 80, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Topcoat, IsEnabled = false, ProductName = "丙烯酸面漆", Dft = 50, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 42, LaborPrice = 14 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Fireproof, IsEnabled = true, ProductName = "室内膨胀型防火涂料 (2.0h)", Dft = 2.2, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 32 }
        ],

        "底漆 + 中漆 + 防火涂料 + 面漆" =>
        [
            new CoatingLayerConfig { LayerType = CoatingLayerType.Primer, ProductName = "环氧富锌底漆", Dft = 70, VolumeSolids = 65, Density = 1.45, LossRate = 25, MaterialPrice = 38, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Intermediate, ProductName = "环氧云铁中间漆", Dft = 80, VolumeSolids = 60, Density = 1.40, LossRate = 25, MaterialPrice = 30, LaborPrice = 12 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Sealer, IsEnabled = false, ProductName = "环氧封闭漆", Dft = 30, VolumeSolids = 45, Density = 1.15, LossRate = 20, MaterialPrice = 42, LaborPrice = 8 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Fireproof, IsEnabled = true, ProductName = "膨胀型钢结构防火涂料 (1.5h)", Dft = 1.8, VolumeSolids = 70, Density = 1.30, LossRate = 30, MaterialPrice = 28, LaborPrice = 30 },
            new CoatingLayerConfig { LayerType = CoatingLayerType.Topcoat, IsEnabled = true, ProductName = "丙烯酸聚氨酯耐候面漆", Dft = 50, VolumeSolids = 55, Density = 1.25, LossRate = 30, MaterialPrice = 48, LaborPrice = 16 }
        ],

        _ => CreatePreset("底漆 + 中漆 + 面漆")
    };
}
