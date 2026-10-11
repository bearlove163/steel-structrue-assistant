namespace EngineeringApp.Shared.Pricing.Coating;

/// <summary>
/// 工程防腐清单明细项 (用于沉淀规格防腐算量结果，支撑构件级 BOM 组价、定额汇总与历史调差)
/// </summary>
public class ProjectCoatingScheduleItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>构件部位 / 工程编号 (如 "底层主梁", "屋面桁架", "外墙钢板")</summary>
    public string PartName { get; set; } = "结构主体";

    /// <summary>构件规格分类 (如 "钢板板材", "热轧H型钢", "方矩管/箱型", "圆管 CHS", "角钢/槽钢")</summary>
    public string Category { get; set; } = "钢板板材";

    /// <summary>规格代号描述 (如 "t=10mm (Q355B)", "H 400×200×8×13", "Box 500×500×20")</summary>
    public string SpecName { get; set; } = "";

    /// <summary>板厚或主要壁厚 (mm)</summary>
    public double ThicknessMm { get; set; } = 10.0;

    /// <summary>展开比表面积指标 (m²/t)</summary>
    public double AreaPerTon { get; set; } = 25.48;

    /// <summary>工程构件计划吨位 (t)</summary>
    public double ProjectTons { get; set; } = 10.0;

    /// <summary>总展开涂装面积 (m²)</summary>
    public double TotalAreaM2 => AreaPerTon * ProjectTons;

    /// <summary>每吨钢涂料总消耗量 (kg/t)</summary>
    public double PaintUsageKgPerTon { get; set; } = 7.5;

    /// <summary>工程总油漆计划采购量 (kg)</summary>
    public double TotalPaintKg => PaintUsageKgPerTon * ProjectTons;

    // ================= 历史多时期组价与调差指标 =================
    /// <summary>历史基期 (如投标锁价期) 综合单价 (元/t)</summary>
    public double BaseCostPerTon { get; set; }

    /// <summary>现行目标期 (如结算施工期) 综合单价 (元/t)</summary>
    public double TargetCostPerTon { get; set; }

    /// <summary>历史基期工程防腐总价 (元)</summary>
    public double BaseTotalCost => BaseCostPerTon * ProjectTons;

    /// <summary>目标期现行工程防腐总价 (元)</summary>
    public double TargetTotalCost => TargetCostPerTon * ProjectTons;

    /// <summary>吨钢调差价差 (元/t)</summary>
    public double VariancePerTon => TargetCostPerTon - BaseCostPerTon;

    /// <summary>工程总调差金额 (元)</summary>
    public double VarianceTotalCost => TargetTotalCost - BaseTotalCost;

    /// <summary>调差浮动百分比 (%)</summary>
    public double VariancePercentage => BaseCostPerTon > 0 ? (VariancePerTon / BaseCostPerTon * 100.0) : 0;
}
