namespace EngineeringApp.Shared.Pricing.Coating;

using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Algorithms;

/// <summary>
/// 涂装防腐价格历史回溯、截面/板材规格用量推导与工程调差测算核心引擎
/// </summary>
public static class PaintPriceBacktrackingEngine
{
    /// <summary>钢材密度常数 (7850 kg/m³)</summary>
    public const double SteelDensityKgM3 = 7850.0;

    /// <summary>
    /// 根据钢板厚度推导理论双面展开比表面积 S_plate (m²/t)
    /// 物理严谨推导：1平方米厚度为t(mm)的双面板材，表面积2m²，重量为 7.85*t kg (0.00785*t 吨)
    /// S = 2 / (0.00785 * t) = 254.777 / t (m²/t)
    /// </summary>
    public static double CalculatePlateAreaPerTon(double thicknessMm)
    {
        double t = Math.Max(0.5, thicknessMm);
        return 254.777 / t;
    }

    /// <summary>
    /// 将选定历史快照的价格（寄存基准价与品牌实体询价值）注入涂层配置列表中
    /// </summary>
    public static void ApplySnapshotPrices(
        IEnumerable<CoatingLayerConfig> layers,
        PaintPriceSnapshot snapshot,
        CoatingApplicationMethod method = CoatingApplicationMethod.AirlessSpraying)
    {
        ArgumentNullException.ThrowIfNull(layers);
        ArgumentNullException.ThrowIfNull(snapshot);

        double laborBase = method switch
        {
            CoatingApplicationMethod.BrushOrRoller => snapshot.BrushRollerLaborBase,
            _ => snapshot.AirlessSprayingLaborBase
        };

        foreach (var layer in layers)
        {
            // 优先根据具体选定品牌读取真实询价值，若无则平滑降级采用该品类的寄存大盘基价
            double matPrice = layer.LayerType switch
            {
                CoatingLayerType.Primer => snapshot.GetPriceForBrandOrCategory(layer.BrandId, "底漆"),
                CoatingLayerType.Intermediate => snapshot.GetPriceForBrandOrCategory(layer.BrandId, "中间漆"),
                CoatingLayerType.Sealer => snapshot.GetPriceForBrandOrCategory(layer.BrandId, "中间漆"),
                CoatingLayerType.Topcoat => snapshot.GetPriceForBrandOrCategory(layer.BrandId, "面漆"),
                CoatingLayerType.Fireproof => layer.Dft >= 5.0 
                    ? snapshot.ThickFireproofBase 
                    : snapshot.ThinFireproofBase,
                _ => snapshot.EpoxyZincRichPrimerBase
            };

            layer.MaterialPrice = matPrice;
            layer.LaborPrice = laborBase;
        }
    }

    /// <summary>
    /// 为给定的截面/板材规格生成包含基期和目标期双快照造价与调差指标的清单项
    /// </summary>
    public static ProjectCoatingScheduleItem BuildScheduleItem(
        string category,
        string specName,
        double thicknessMm,
        double areaPerTon,
        double projectTons,
        SectionType sectionType,
        IEnumerable<CoatingLayerConfig> baseLayers,
        PaintPriceSnapshot baseSnap,
        PaintPriceSnapshot targetSnap,
        CoatingApplicationMethod method = CoatingApplicationMethod.AirlessSpraying,
        string partName = "结构主体")
    {
        // 1. 克隆两套涂层配置分别注入基期快照和目标期快照价格
        var layersForBase = CloneLayers(baseLayers);
        ApplySnapshotPrices(layersForBase, baseSnap, method);

        var layersForTarget = CloneLayers(baseLayers);
        ApplySnapshotPrices(layersForTarget, targetSnap, method);

        // 2. 模拟伪截面以计算每平米综合造价与耗漆量
        double linearMass = 1000.0 / Math.Max(0.1, areaPerTon);
        var dummySection = new SectionPropertiesResult
        {
            OuterPerimeter = 1000.0,
            Perimeter = 1000.0,
            TopSurfaceWidth = 0.0,
            LinearMass = linearMass,
            Area = (linearMass / SteelDensityKgM3) * 1e6
        };

        var baseCalc = PaintCoatingCalculator.Calculate(dummySection, layersForBase, false, 1.0, method, sectionType);
        var targetCalc = PaintCoatingCalculator.Calculate(dummySection, layersForTarget, false, 1.0, method, sectionType);

        // 吨钢造价 = 每平米造价 * 吨钢展开面积
        double baseCostTon = baseCalc.TotalCostPerM2 * areaPerTon;
        double targetCostTon = targetCalc.TotalCostPerM2 * areaPerTon;
        double usageKgTon = targetCalc.Layers.Sum(l => l.ActualUsageKgPerM2) * areaPerTon;

        return new ProjectCoatingScheduleItem
        {
            PartName = partName,
            Category = category,
            SpecName = specName,
            ThicknessMm = thicknessMm,
            AreaPerTon = areaPerTon,
            ProjectTons = Math.Max(0.1, projectTons),
            PaintUsageKgPerTon = usageKgTon,
            BaseCostPerTon = baseCostTon,
            TargetCostPerTon = targetCostTon
        };
    }

    /// <summary>
    /// 生成标准钢板厚度规格序列字典 (6mm ~ 80mm)
    /// </summary>
    public static List<(double Thickness, string Name, double AreaPerTon)> GetStandardPlateSpecs()
    {
        double[] thicknesses = [6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 30, 35, 40, 50, 60, 80];
        var list = new List<(double, string, double)>();
        foreach (var t in thicknesses)
        {
            list.Add((t, $"t = {t:0.##} mm 钢板", CalculatePlateAreaPerTon(t)));
        }
        return list;
    }

    /// <summary>
    /// 生成典型型钢截面规格列表
    /// </summary>
    public static List<(string Category, string Name, double Thickness, double AreaPerTon, SectionType Type)> GetTypicalProfileSpecs()
    {
        return
        [
            ("热轧H型钢", "H 200×100×5.5×8 (轻梁)", 8.0, 40.5, SectionType.HBeam),
            ("热轧H型钢", "H 400×200×8×13 (主梁)", 13.0, 26.8, SectionType.HBeam),
            ("热轧H型钢", "H 700×300×13×24 (重框架)", 24.0, 14.5, SectionType.HBeam),
            ("热轧H型钢", "H 1000×400×16×32 (特大梁)", 32.0, 10.4, SectionType.HBeam),
            ("方矩管/箱型", "Box 300×300×12", 12.0, 11.2, SectionType.RHS),
            ("方矩管/箱型", "Box 500×500×20", 20.0, 6.7, SectionType.RHS),
            ("圆管 CHS", "CHS Φ114×4 (小管)", 4.0, 32.8, SectionType.CHS),
            ("圆管 CHS", "CHS Φ219×8 (立柱)", 8.0, 16.5, SectionType.CHS),
            ("圆管 CHS", "CHS Φ500×14 (大管)", 14.0, 9.4, SectionType.CHS),
            ("角钢/槽钢", "Angle L 50×5 (支撑)", 5.0, 50.4, SectionType.Angle),
            ("角钢/槽钢", "Angle L 100×10 (角钢)", 10.0, 26.5, SectionType.Angle),
            ("角钢/槽钢", "Channel [ 160×63×6.5", 6.5, 33.2, SectionType.Channel)
        ];
    }

    private static List<CoatingLayerConfig> CloneLayers(IEnumerable<CoatingLayerConfig> source)
    {
        var list = new List<CoatingLayerConfig>();
        foreach (var l in source)
        {
            list.Add(new CoatingLayerConfig
            {
                LayerType = l.LayerType,
                IsEnabled = l.IsEnabled,
                BrandId = l.BrandId,
                ModelCode = l.ModelCode,
                ProductName = l.ProductName,
                Dft = l.Dft,
                VolumeSolids = l.VolumeSolids,
                Density = l.Density,
                LossRate = l.LossRate,
                MaterialPrice = l.MaterialPrice,
                LaborPrice = l.LaborPrice
            });
        }
        return list;
    }
}
