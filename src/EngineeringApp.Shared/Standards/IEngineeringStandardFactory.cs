using EngineeringApp.Shared.Material.Pricing;

namespace EngineeringApp.Shared.Standards;

/// <summary>
/// 国际化工程标准体系抽象工厂接口 (Abstract Factory Pattern)
/// 负责一键注入多国标准 (国标 GB / 欧标 EN / 美标 ASTM) 下的价格阶梯、尺寸公差、力学限值与验算表达
/// </summary>
public interface IEngineeringStandardFactory
{
    /// <summary>标准体系枚举 (GB, EN, ASTM)</summary>
    StandardSystem SystemType { get; }

    /// <summary>规范代码编号 (如 GB 50017-2017 / EN 1993-1-1 / AISC 360-22)</summary>
    string SystemCode { get; }

    /// <summary>规范体系中文全称</summary>
    string SystemName { get; }

    /// <summary>创建材料基准价与阶梯定价策略</summary>
    IStandardPricingStrategy CreatePricingStrategy();

    /// <summary>创建钢板及型钢尺寸外形公差引擎</summary>
    IToleranceRuleEngine CreateToleranceEngine();

    /// <summary>创建构件长细比与抗力分项系数限值引擎</summary>
    IStructuralLimitsEngine CreateLimitsEngine();

    /// <summary>创建标准计算书公式与验算表达格式化器</summary>
    IStandardFormulaFormatter CreateFormulaFormatter();
}

/// <summary>
/// 尺寸公差规则引擎接口
/// </summary>
public interface IToleranceRuleEngine
{
    string StandardCode { get; }
    double GetThicknessTolerance(double nominalThicknessMm, string toleranceClass = "B");
}

/// <summary>
/// 结构力学限值规则引擎接口
/// </summary>
public interface IStructuralLimitsEngine
{
    string CodeReference { get; }
    double GetTensionSlendernessLimit();
    double GetCompressionSlendernessLimit(bool isMainMember = true);
    double GetResistanceFactor();
}

/// <summary>
/// 标准计算书表达格式化器
/// </summary>
public interface IStandardFormulaFormatter
{
    string SpecificationName { get; }
    string FormatYieldStress(double fy);
    string FormatSafetyCheck(string component, double demand, double capacity, double ratio);
}
