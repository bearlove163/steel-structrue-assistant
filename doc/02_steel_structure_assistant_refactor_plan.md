# 钢结构助手 (Steel Structure Assistant) 全景商务报价架构与内核解耦规划 (修订版 v3.0)

## 一、钢结构商务报价软件的真实完整业务链剖析

在钢结构制造企业（如中建钢构、精工钢构、东南网架等）与专业深化造价咨询业务中，一套标准的**商业化钢结构商务报价系统**，其真正的数据与业务流转逻辑如下：

```mermaid
graph TD
    subgraph 1. 基础数据底座 (Foundation)
        MAT["【材料库 Material】<br/>钢材牌号 · 板材/型钢基价 · 涂料物性与单价"]
        SEC["【截面微内核 Section】<br/>纯几何与力学 · 米重(kg/m) · 外表面积(m²/m) · 型钢库"]
        QUO["【定额规则库 Quota】<br/>(SectionType x MemberRole) 正交加工工价 · 厚板系数 · 损耗定额"]
    end

    subgraph 2. 构件装配与工料机组价 (BOM Pricing)
        BOM["【构件装配 BOM】<br/>GZ1 (箱型柱) = 主肢 + 牛腿 + 隔板 + 底板<br/>GL1 (钢梁) = H型主梁 + 节点板"]
        PART_PRICE["【构件级工料机直接费】<br/>主材费 + 加工制作费 + 抛丸防腐涂装费"]
    end

    subgraph 3. 公司固定格式成本测算 (Cost Estimation - 核心枢纽)
        COST_TEMPLATE["【公司固定成本测算模板 / Sheet】<br/>套用企业固定测算表格格式与费用构成模型"]
        DIRECT["直接工程成本: 原材料 + 车间加工 + 抛丸涂装 + 运输包装 + 工地吊装"]
        INDIRECT["间接费与措施费: 深化设计BIM + 探伤检测 + 施工胎架措施"]
        OVERHEAD["企业费率与资金成本: 厂区管理费分摊 + 财务垫资利息 + 税金"]
        MARGIN["利润与底价分析: 保本底价 · 目标毛利 · 商务竞争调价"]
    end

    subgraph 4. 商业报价文件输出 (Quotation Export)
        EXPORT["【对外商务报价成果】<br/>国标工程量清单报表 (010605钢柱/010606钢梁)<br/>投标单价分析表 · Excel/PDF报价书导出"]
    end

    MAT --> BOM
    SEC --> BOM
    QUO --> BOM
    BOM --> PART_PRICE

    PART_PRICE --> COST_TEMPLATE
    COST_TEMPLATE --> DIRECT
    COST_TEMPLATE --> INDIRECT
    COST_TEMPLATE --> OVERHEAD
    COST_TEMPLATE --> MARGIN

    DIRECT --> EXPORT
    INDIRECT --> EXPORT
    OVERHEAD --> EXPORT
    MARGIN --> EXPORT

    style 1. 基础数据底座 (Foundation) fill:#e8f5e9,stroke:#2e7d32
    style 2. 构件装配与工料机组价 (BOM Pricing) fill:#e3f2fd,stroke:#1565c0
    style 3. 公司固定格式成本测算 (Cost Estimation - 核心枢纽) fill:#fff3e0,stroke:#e65100
    style 4. 商业报价文件输出 (Quotation Export) fill:#f3e5f5,stroke:#7b1fa2
```

---

## 二、定位重塑：“截面特性” vs “成本测算” vs “商务报价”

用户指出的核心业务逻辑极为精准：
1. **截面特性 (Section Core)**：只是最底层的一个**纯数学/物理微内核**。它的工作仅仅是输入几何尺寸，吐出截面积、理论米重、展开涂装周长，为称重和算量提供基础数据，不参与任何商务决算。
2. **构件组价 (BOM Pricing)**：把钢柱、钢梁拆成板件，算出一根构件的净重、毛重、加工工费和涂装费。
3. **成本测算 (Cost Estimation - 核心枢纽)**：
   * **这是企业真正的核心秘密与报价底牌**！
   * 每一个钢结构公司都有一套**固定的成本测算表格（Excel 格式或公司内定模板）**。
   * 它负责将构件清单汇总的工料机量，套入公司自有的财务模型中：
     * **材料采购费**（钢板、型钢出厂价 + 运费加价 + 采购损耗）
     * **车间制作费**（车间加工基准工费 $\times$ 难度系数 + 辅材电力折旧）
     * **除锈与防腐涂装费**（抛丸 Sa2.5 级 + 油漆涂料材料费 + 喷涂人工费）
     * **物流运输费**（出厂到工地的距离 $\times$ 吨运价，超长/超宽特种运输附加）
     * **工地安装费**（现场吊装机械台班、垂直运输、高空拼装）
     * **技术措施与检测**（深化设计费、探伤检验费、预拼装措施）
     * **企业管理费与资金利息**（管理费率分摊、垫资财务成本、税金增值税率）
     * **测算出“公司保本成本线”并设定“目标毛利率”**。
4. **输出报价文件 (Quotation Export)**：
   * 测算完成后，按照招标方要求或商务标准格式，生成最终的对外投标报价书（如国标清单计价规范格式、Excel 报价单）。

---

## 三、解耦后的系统架构与模块拓扑 (Target Decoupled Architecture)

系统划分为五大高内聚、低耦合的领域模块，严格单向流转：

```
[Layer 1: 基础微内核]
  ├── Material (材料物性与价格基准) ── 零外部依赖
  └── Section (截面力学与表面展开)  ── 仅依赖 Material (算米重)

[Layer 2: 制造规则]
  └── Quota (加工定额与工艺难度矩阵) ── 依赖 Section, Material

[Layer 3: 构件组价]
  └── Pricing (构件BOM装配与工料机直接费) ── 依赖 Quota, Section, Material

[Layer 4: 成本测算 - 核心枢纽]
  └── CostEstimation (公司固定格式成本测算引擎) ── 依赖 Pricing

[Layer 5: 报价输出]
  └── QuotationExport (商业报表与清单导出) ── 依赖 CostEstimation
```

### 1. 各模块代码目录与职责明细

```
src/EngineeringApp.Shared/
├── Material/                         # 【模块 1：材料域】
│   ├── Models/
│   │   ├── SteelGrade.cs             # Q235B, Q355B, Q355C, S355JR, A992
│   │   └── MaterialProfile.cs        # 密度 (7850), 弹性模量, 屈服强度
│   └── Data/
│       └── SteelMaterialDatabase.cs  # 材料库与出厂基础价格
│
├── Section/                          # 【模块 2：截面域 (完全纯净的共享计算内核)】
│   ├── Models/
│   │   ├── SectionType.cs            # RHS, CHS, HBeam, Channel, Angle 等
│   │   ├── SectionParameters.cs      # 几何参数
│   │   └── SectionPropertiesResult.cs# 纯净力学输出 (面积, 米重, 周长, 惯性矩)
│   ├── Calculators/
│   │   ├── ParametricSectionCalculator.cs
│   │   └── GreenTheoremPolygonEngine.cs
│   └── Databases/
│       ├── StandardSteelDatabase.cs  # 国标 GB/T
│       ├── EuroSteelDatabase.cs      # 欧标 EN
│       ├── AiscSteelDatabase.cs      # 美标 AISC
│       └── StructuralSteelLibrary.cs # 全球型钢检索门面
│
├── Quota/                            # 【模块 3：定额与工艺规则域】
│   ├── Models/
│   │   ├── MemberRole.cs             # 构件工程角色 (钢柱, 钢梁, 支撑, 桁架)
│   │   └── QuotaRuleModels.cs        # 难度系数模型
│   └── Data/
│       └── FabricationQuotaDatabase.cs# (SectionType x MemberRole) 正交基准工费
│
├── Pricing/                          # 【模块 4：构件BOM与工料机组价域】
│   ├── Assembly/
│   │   ├── SteelAssemblyItem.cs      # 装配构件 (GZ1, GL1)
│   │   ├── SteelPartItem.cs          # 零件板件明细 (主肢, 牛腿, 隔板, 柱底板)
│   │   └── PartFunctionalRole.cs
│   ├── Coating/
│   │   ├── PaintCoatingModels.cs     # 涂层体系 (底/中/面/防火)
│   │   ├── PaintingCalculationOptions.cs
│   │   ├── PaintCoatingCalculator.cs
│   │   └── SectionPaintingExtensions.cs # 截面涂装扩展方法 (无缝挂接)
│   └── Engine/
│       └── MemberPricingCalculator.cs # 构件直接工料机组价计算器
│
├── CostEstimation/                   # 【模块 5：公司固定格式成本测算域 (NEW 核心枢纽)】
│   ├── Models/
│   │   ├── ProjectCostSheet.cs       # 完整的成本测算表上下文模型
│   │   ├── DirectCostItem.cs         # 直接费 (材料/制造/涂装/运输/安装)
│   │   ├── IndirectCostItem.cs       # 间接费与措施费 (深化/检测/胎架)
│   │   ├── OverheadRateConfig.cs     # 公司固定费率参数 (管理费率/垫资利息/税率)
│   │   └── ProfitAnalysisResult.cs   # 成本底价、目标毛利与保本测算结果
│   └── Engine/
│       ├── CostEstimationEngine.cs   # 公司固定格式套算核心引擎
│       └── CompanyCostTemplate.cs    # 公司标准测算表格模板定义 (结构化表格项)
│
└── QuotationExport/                  # 【模块 6：商务报价输出域】
    ├── Models/
    │   ├── QuotationBillItem.cs      # 对外清单项 (010605 钢柱, 010606 钢梁)
    │   └── CommercialBidSummary.cs   # 最终商务投标报价总表
    └── Exporters/
        └── QuotationReportGenerator.cs# 报价文件数据生成器 (支持Excel/PDF导出结构)
```

---

## 四、成本测算模型 (`ProjectCostSheet`) 设计草案

为了实现“将算出的各种东西，按照公司固定格式套进去”，成本测算模型设计如下：

```csharp
namespace EngineeringApp.Shared.CostEstimation.Models;

/// <summary>
/// 公司固定格式钢结构工程项目成本测算表模型
/// </summary>
public class ProjectCostSheet
{
    public string ProjectName { get; set; } = "标准钢结构工程成本测算";

    // 1. 工程量与基础指标汇总 (来自 Pricing 模块)
    public double TotalSteelNetWeightTon { get; set; }     // 钢材总净重 (t)
    public double TotalSteelGrossWeightTon { get; set; }   // 钢材总采购毛重 (含下料损耗 t)
    public double TotalPaintingAreaM2 { get; set; }        // 涂装防腐总表面积 (m²)

    // 2. 直接工程成本 (Direct Costs)
    public double MaterialProcurementCost { get; set; }    // ① 钢材原料采购费 (元)
    public double WorkshopFabricationCost { get; set; }    // ② 车间加工制作直接工费 (元)
    public double SurfaceCoatingCost { get; set; }         // ③ 抛丸防腐与防火涂装费 (元)
    public double TransportationCost { get; set; }         // ④ 物流运杂费 (出厂运费 元)
    public double SiteErectionCost { get; set; }           // ⑤ 工地安装吊装直接费 (元)

    public double TotalDirectCost => MaterialProcurementCost + WorkshopFabricationCost + 
                                     SurfaceCoatingCost + TransportationCost + SiteErectionCost;

    // 3. 措施费与技术服务 (Measures & Engineering)
    public double DetailingBimFee { get; set; }            // 深化设计与BIM放样费 (元/t)
    public double InspectionTestingFee { get; set; }       // 探伤检验与试验费 (元/t)
    public double TemporarySupportFee { get; set; }        // 临时支撑胎架措施费 (元)

    // 4. 企业固定费率与财务成本 (Company Fixed Overhead & Financial)
    public double OverheadRatePercent { get; set; } = 3.5; // 企业车间管理费率 (%)
    public double FinancingCostRatePercent { get; set; } = 2.0; // 垫资与资金占用利息率 (%)
    public double TaxRatePercent { get; set; } = 9.0;      // 建筑工程增值税率 (%)

    // 5. 测算底价与目标利润 (Cost Floor & Target Margin)
    public double FactoryCostFloor { get; set; }           // 公司保本底价 (元)
    public double TargetGrossMarginPercent { get; set; } = 8.0; // 目标毛利率 (%)
    public double FinalBidQuotation { get; set; }          // 最终对外报价总金额 (元)
    public double FinalUnitPricePerTon => TotalSteelNetWeightTon > 0 
        ? FinalBidQuotation / TotalSteelNetWeightTon 
        : 0;                                               // 综合单价 (元/t)
}
```

---

## 五、实施落地路线 (Action Roadmap)

1. **第一阶段：内核纯净化与基础分层**
   * 建立 `Material/`、`Section/` 目录；
   * 将 `SectionPropertiesResult` 中的涂装与商业代码剥离至扩展方法，让截面内核变成纯净的几何物理计算器；
   * 确保现有 20 项单元测试全部绿灯通过。
2. **第二阶段：正交定额与构件装配组价**
   * 建立 `Quota/`（构件角色 `MemberRole` 与加工工费矩阵）；
   * 建立 `Pricing/`（构件装配 BOM，主肢+牛腿+隔板，涂装多层配套算量）。
3. **第三阶段：公司固定格式成本测算引擎 (`CostEstimation`)**
   * 建立 `CostEstimation/` 模块与 `ProjectCostSheet` 测算表数据结构；
   * 编写 `CostEstimationEngine`：自动抓取 BOM 的总重量与总面积，套入公司的直接费、间接费、运费、措施费、管理费、税金与目标利润模型。
4. **第四阶段：报价文件输出 (`QuotationExport`)**
   * 输出标准的投标报价清单汇总数据结构，为未来直接导出 Excel 表格留出无缝接口。

---

## 六、验证与质量保障

* **无损回归测试**：原有截面计算、格林公式、型钢库检索、涂料计算 100% 保持原有结果与精度；
* **业务流程端到端测试**：
  * 编写 `EndToEndQuotationTests.cs`：
  * 输入截面（如箱型柱和H型梁） $\to$ 组装成构件 $\to$ 套入正交定额 $\to$ 套入公司固定成本测算表 $\to$ 验证算出的保本底价、综合单价（元/吨）与最终报价清单完全吻合。
