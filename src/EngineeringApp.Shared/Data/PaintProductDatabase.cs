using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 常用钢结构工程油漆涂料产品与价格基准数据库
/// 涵盖底漆、中间漆、封闭漆、面漆及膨胀/非膨胀防火涂料
/// </summary>
public static class PaintProductDatabase
{
    private static readonly List<PaintProductItem> _products =
    [
        // 1. 底漆 (Primer)
        new PaintProductItem
        {
            Id = 1,
            LayerType = CoatingLayerType.Primer,
            Name = "环氧富锌底漆 (含锌量70%)",
            Brand = "佐敦/国际牌/中远关西",
            Description = "重防腐钢结构标准底漆，阴极保护防锈，耐盐雾性能卓越",
            VolumeSolidsPercent = 65.0,
            DensityKgL = 1.48,
            DefaultDft = 70.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 38.0,
            LaborUnitPricePerM2 = 12.0
        },
        new PaintProductItem
        {
            Id = 2,
            LayerType = CoatingLayerType.Primer,
            Name = "无机硅酸锌车间底漆",
            Brand = "海虹老人/PPG",
            Description = "快干型耐高温无机富锌底漆，耐候及耐磨性能优异",
            VolumeSolidsPercent = 62.0,
            DensityKgL = 1.55,
            DefaultDft = 75.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 42.0,
            LaborUnitPricePerM2 = 14.0
        },
        new PaintProductItem
        {
            Id = 3,
            LayerType = CoatingLayerType.Primer,
            Name = "环氧磷酸锌防锈底漆",
            Brand = "国产优质通用",
            Description = "经济型环保防锈底漆，无重金属毒性，附着力强",
            VolumeSolidsPercent = 58.0,
            DensityKgL = 1.35,
            DefaultDft = 60.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 28.0,
            LaborUnitPricePerM2 = 10.0
        },
        new PaintProductItem
        {
            Id = 4,
            LayerType = CoatingLayerType.Primer,
            Name = "红丹醇酸防锈底漆",
            Brand = "传统通用型",
            Description = "普通室内轻型钢构件经济防锈底漆",
            VolumeSolidsPercent = 52.0,
            DensityKgL = 1.30,
            DefaultDft = 50.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 22.0,
            LaborUnitPricePerM2 = 9.0
        },

        // 2. 中间漆 (Intermediate)
        new PaintProductItem
        {
            Id = 5,
            LayerType = CoatingLayerType.Intermediate,
            Name = "环氧云铁厚浆中间漆 (MIO)",
            Brand = "佐敦/阿克苏诺贝尔",
            Description = "片状云母氧化铁阻隔屏蔽水汽，增加漆膜厚度与耐候屏蔽性",
            VolumeSolidsPercent = 60.0,
            DensityKgL = 1.42,
            DefaultDft = 100.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 30.0,
            LaborUnitPricePerM2 = 12.0
        },
        new PaintProductItem
        {
            Id = 6,
            LayerType = CoatingLayerType.Intermediate,
            Name = "高固体分环氧厚浆中间漆",
            Brand = "海虹老人",
            Description = "低VOC环保型厚膜中间漆，一道成膜厚度大",
            VolumeSolidsPercent = 75.0,
            DensityKgL = 1.45,
            DefaultDft = 120.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 34.0,
            LaborUnitPricePerM2 = 13.0
        },

        // 3. 封闭漆 (Sealer)
        new PaintProductItem
        {
            Id = 7,
            LayerType = CoatingLayerType.Sealer,
            Name = "环氧封闭清漆",
            Brand = "PPG/中远关西",
            Description = "无机富锌或热喷锌铝表面封闭孔隙，防止气泡并极大增强层间附着力",
            VolumeSolidsPercent = 45.0,
            DensityKgL = 1.15,
            DefaultDft = 30.0,
            LossRatePercent = 20.0,
            MaterialUnitPricePerKg = 42.0,
            LaborUnitPricePerM2 = 8.0
        },
        new PaintProductItem
        {
            Id = 8,
            LayerType = CoatingLayerType.Sealer,
            Name = "聚氨酯渗透型封闭底漆",
            Brand = "立邦工业漆",
            Description = "高渗透性双组份封闭底漆，封闭基底微裂隙与气孔",
            VolumeSolidsPercent = 40.0,
            DensityKgL = 1.10,
            DefaultDft = 25.0,
            LossRatePercent = 20.0,
            MaterialUnitPricePerKg = 45.0,
            LaborUnitPricePerM2 = 9.0
        },

        // 4. 面漆 (Topcoat)
        new PaintProductItem
        {
            Id = 9,
            LayerType = CoatingLayerType.Topcoat,
            Name = "脂肪族丙烯酸聚氨酯面漆",
            Brand = "佐敦/中远/海虹",
            Description = "优异保光保色性、抗紫外线耐候性，工业民用钢结构主力面漆",
            VolumeSolidsPercent = 55.0,
            DensityKgL = 1.25,
            DefaultDft = 60.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 45.0,
            LaborUnitPricePerM2 = 15.0
        },
        new PaintProductItem
        {
            Id = 10,
            LayerType = CoatingLayerType.Topcoat,
            Name = "氟碳重防腐面漆 (FEVE)",
            Brand = "大金/三爱富/阿克苏",
            Description = "超耐候超级防腐面漆，耐候寿命超20年以上，用于机场桥梁等高耐久工程",
            VolumeSolidsPercent = 50.0,
            DensityKgL = 1.20,
            DefaultDft = 50.0,
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 78.0,
            LaborUnitPricePerM2 = 22.0
        },
        new PaintProductItem
        {
            Id = 11,
            LayerType = CoatingLayerType.Topcoat,
            Name = "醇酸调和磁漆",
            Brand = "传统工业漆",
            Description = "室内一般钢结构简易面漆，成本低，光泽度好",
            VolumeSolidsPercent = 48.0,
            DensityKgL = 1.18,
            DefaultDft = 40.0,
            LossRatePercent = 25.0,
            MaterialUnitPricePerKg = 24.0,
            LaborUnitPricePerM2 = 10.0
        },

        // 5. 防火涂料 (Fireproof Coating)
        new PaintProductItem
        {
            Id = 12,
            LayerType = CoatingLayerType.Fireproof,
            Name = "室内膨胀型钢结构防火涂料 (耐火1.5h)",
            Brand = "北京金隅/兰陵/圣戈班",
            Description = "遇火发泡膨胀形成碳化隔热层，涂层薄(约1.5-2.0mm)，装饰性好",
            VolumeSolidsPercent = 70.0,
            DensityKgL = 1.28,
            DefaultDft = 1.8, // mm
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 26.0,
            LaborUnitPricePerM2 = 25.0
        },
        new PaintProductItem
        {
            Id = 13,
            LayerType = CoatingLayerType.Fireproof,
            Name = "室内膨胀型钢结构防火涂料 (耐火2.0h)",
            Brand = "金隅/天龙/武警防火所监制",
            Description = "一级耐火等级梁柱常用，设计厚度约2.2-2.5mm",
            VolumeSolidsPercent = 72.0,
            DensityKgL = 1.30,
            DefaultDft = 2.4, // mm
            LossRatePercent = 30.0,
            MaterialUnitPricePerKg = 28.0,
            LaborUnitPricePerM2 = 30.0
        },
        new PaintProductItem
        {
            Id = 14,
            LayerType = CoatingLayerType.Fireproof,
            Name = "室外/厚型非膨胀型钢结构防火涂料 (耐火2.5h-3.0h)",
            Brand = "宏达/金盾/川消",
            Description = "无机隔热厚型涂料，不老化抗风雨耐水，涂层厚度通常15-25mm",
            VolumeSolidsPercent = 85.0,
            DensityKgL = 0.65, // 容重较轻
            DefaultDft = 18.0, // mm
            LossRatePercent = 35.0,
            MaterialUnitPricePerKg = 12.0,
            LaborUnitPricePerM2 = 45.0
        }
    ];

    public static IReadOnlyList<PaintProductItem> AllProducts => _products;

    public static IEnumerable<PaintProductItem> GetByType(CoatingLayerType type) =>
        _products.Where(p => p.LayerType == type);

    /// <summary>
    /// 新增油漆或防火涂料产品
    /// </summary>
    public static void AddProduct(PaintProductItem item)
    {
        item.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        _products.Add(item);
    }

    /// <summary>
    /// 删除指定涂料产品
    /// </summary>
    public static bool RemoveProduct(int id)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id);
        if (existing != null)
        {
            return _products.Remove(existing);
        }
        return false;
    }

    /// <summary>
    /// 批量更新产品列表
    /// </summary>
    public static void UpdateProducts(IEnumerable<PaintProductItem> items)
    {
        foreach (var item in items)
        {
            var existing = _products.FirstOrDefault(p => p.Id == item.Id);
            if (existing != null)
            {
                existing.Name = item.Name;
                existing.Brand = item.Brand;
                existing.LayerType = item.LayerType;
                existing.Description = item.Description;
                existing.VolumeSolidsPercent = item.VolumeSolidsPercent;
                existing.DensityKgL = item.DensityKgL;
                existing.DefaultDft = item.DefaultDft;
                existing.LossRatePercent = item.LossRatePercent;
                existing.MaterialUnitPricePerKg = item.MaterialUnitPricePerKg;
                existing.LaborUnitPricePerM2 = item.LaborUnitPricePerM2;
            }
        }
    }

    /// <summary>
    /// 导出涂料数据库为标准 JSON
    /// </summary>
    public static string ExportToJson()
    {
        return System.Text.Json.JsonSerializer.Serialize(_products, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    /// <summary>
    /// 导入标准 JSON 涂料数据
    /// </summary>
    public static bool ImportFromJson(string json, bool overwrite = false)
    {
        try
        {
            var items = System.Text.Json.JsonSerializer.Deserialize<List<PaintProductItem>>(json);
            if (items == null || items.Count == 0) return false;

            if (overwrite)
            {
                _products.Clear();
                int nextId = 1;
                foreach (var it in items)
                {
                    it.Id = nextId++;
                    _products.Add(it);
                }
            }
            else
            {
                foreach (var it in items)
                {
                    AddProduct(it);
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 恢复出厂默认基准数据
    /// </summary>
    public static void ResetToDefaults()
    {
        _products.Clear();
        _products.AddRange(GetDefaultPresetList());
    }

    private static List<PaintProductItem> GetDefaultPresetList() =>
    [
        new PaintProductItem { Id = 1, LayerType = CoatingLayerType.Primer, Name = "环氧富锌底漆 (含锌量70%)", Brand = "佐敦/国际牌/中远关西", Description = "重防腐钢结构标准底漆，阴极保护防锈，耐盐雾性能卓越", VolumeSolidsPercent = 65.0, DensityKgL = 1.48, DefaultDft = 70.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 38.0, LaborUnitPricePerM2 = 12.0 },
        new PaintProductItem { Id = 2, LayerType = CoatingLayerType.Primer, Name = "无机硅酸锌车间底漆", Brand = "海虹老人/PPG", Description = "快干型耐高温无机富锌底漆，耐候及耐磨性能优异", VolumeSolidsPercent = 62.0, DensityKgL = 1.55, DefaultDft = 75.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 42.0, LaborUnitPricePerM2 = 14.0 },
        new PaintProductItem { Id = 3, LayerType = CoatingLayerType.Primer, Name = "环氧磷酸锌防锈底漆", Brand = "国产优质通用", Description = "经济型环保防锈底漆，无重金属毒性，附着力强", VolumeSolidsPercent = 58.0, DensityKgL = 1.35, DefaultDft = 60.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 28.0, LaborUnitPricePerM2 = 10.0 },
        new PaintProductItem { Id = 4, LayerType = CoatingLayerType.Primer, Name = "红丹醇酸防锈底漆", Brand = "传统通用型", Description = "普通室内轻型钢构件经济防锈底漆", VolumeSolidsPercent = 52.0, DensityKgL = 1.30, DefaultDft = 50.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 22.0, LaborUnitPricePerM2 = 9.0 },
        new PaintProductItem { Id = 5, LayerType = CoatingLayerType.Intermediate, Name = "环氧云铁厚浆中间漆 (MIO)", Brand = "佐敦/阿克苏诺贝尔", Description = "片状云母氧化铁阻隔屏蔽水汽，增加漆膜厚度与耐候屏蔽性", VolumeSolidsPercent = 60.0, DensityKgL = 1.42, DefaultDft = 100.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 30.0, LaborUnitPricePerM2 = 12.0 },
        new PaintProductItem { Id = 6, LayerType = CoatingLayerType.Intermediate, Name = "高固体分环氧厚浆中间漆", Brand = "海虹老人", Description = "低VOC环保型厚膜中间漆，一道成膜厚度大", VolumeSolidsPercent = 75.0, DensityKgL = 1.45, DefaultDft = 120.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 34.0, LaborUnitPricePerM2 = 13.0 },
        new PaintProductItem { Id = 7, LayerType = CoatingLayerType.Sealer, Name = "环氧封闭漆", Brand = "佐敦/兰陵", Description = "专用于热喷铝、热喷锌或无机富锌涂层的多孔封闭渗透", VolumeSolidsPercent = 45.0, DensityKgL = 1.15, DefaultDft = 30.0, LossRatePercent = 20.0, MaterialUnitPricePerKg = 42.0, LaborUnitPricePerM2 = 8.0 },
        new PaintProductItem { Id = 8, LayerType = CoatingLayerType.Sealer, Name = "聚氨酯双组份封闭底漆", Brand = "国际牌 (IP)", Description = "快干渗透型封闭底漆，附着力与耐化学品优异", VolumeSolidsPercent = 50.0, DensityKgL = 1.18, DefaultDft = 35.0, LossRatePercent = 20.0, MaterialUnitPricePerKg = 48.0, LaborUnitPricePerM2 = 10.0 },
        new PaintProductItem { Id = 9, LayerType = CoatingLayerType.Topcoat, Name = "脂肪族丙烯酸聚氨酯面漆", Brand = "中远关西/佐敦/老人", Description = "优异保光保色性、抗紫外线耐黄变，广泛用于工业与建筑外观", VolumeSolidsPercent = 55.0, DensityKgL = 1.25, DefaultDft = 60.0, LossRatePercent = 30.0, MaterialUnitPricePerKg = 45.0, LaborUnitPricePerM2 = 15.0 },
        new PaintProductItem { Id = 10, LayerType = CoatingLayerType.Topcoat, Name = "氟碳重防腐面漆 (FEVE)", Brand = "大金/三爱富/阿克苏", Description = "超耐候超级防腐面漆，耐候寿命超20年以上，用于机场桥梁等高耐久工程", VolumeSolidsPercent = 50.0, DensityKgL = 1.20, DefaultDft = 50.0, LossRatePercent = 30.0, MaterialUnitPricePerKg = 78.0, LaborUnitPricePerM2 = 22.0 },
        new PaintProductItem { Id = 11, LayerType = CoatingLayerType.Topcoat, Name = "醇酸调和磁漆", Brand = "传统工业漆", Description = "室内一般钢结构简易面漆，成本低，光泽度好", VolumeSolidsPercent = 48.0, DensityKgL = 1.18, DefaultDft = 40.0, LossRatePercent = 25.0, MaterialUnitPricePerKg = 24.0, LaborUnitPricePerM2 = 10.0 },
        new PaintProductItem { Id = 12, LayerType = CoatingLayerType.Fireproof, Name = "室内膨胀型钢结构防火涂料 (耐火1.5h)", Brand = "北京金隅/兰陵/圣戈班", Description = "遇火发泡膨胀形成碳化隔热层，涂层薄(约1.5-2.0mm)，装饰性好", VolumeSolidsPercent = 70.0, DensityKgL = 1.28, DefaultDft = 1.8, LossRatePercent = 30.0, MaterialUnitPricePerKg = 26.0, LaborUnitPricePerM2 = 25.0 },
        new PaintProductItem { Id = 13, LayerType = CoatingLayerType.Fireproof, Name = "室内膨胀型钢结构防火涂料 (耐火2.0h)", Brand = "金隅/天龙/武警防火所监制", Description = "一级耐火等级梁柱常用，设计厚度约2.2-2.5mm", VolumeSolidsPercent = 72.0, DensityKgL = 1.30, DefaultDft = 2.4, LossRatePercent = 30.0, MaterialUnitPricePerKg = 28.0, LaborUnitPricePerM2 = 30.0 },
        new PaintProductItem { Id = 14, LayerType = CoatingLayerType.Fireproof, Name = "室外/厚型非膨胀型钢结构防火涂料 (耐火2.5h-3.0h)", Brand = "宏达/金盾/川消", Description = "无机隔热厚型涂料，不老化抗风雨耐水，涂层厚度通常15-25mm", VolumeSolidsPercent = 85.0, DensityKgL = 0.65, DefaultDft = 18.0, LossRatePercent = 35.0, MaterialUnitPricePerKg = 12.0, LaborUnitPricePerM2 = 45.0 }
    ];
}
