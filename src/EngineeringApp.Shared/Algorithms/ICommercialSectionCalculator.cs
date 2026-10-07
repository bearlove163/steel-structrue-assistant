using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 商务软件专用的截面工程量与成本核算引擎接口
/// </summary>
public interface ICommercialSectionCalculator
{
    /// <summary>
    /// 根据参数化截面定义与商业配置，计算米重、表面积（含楼板扣除）、总重量及预估成本
    /// </summary>
    CommercialSectionResult Calculate(SectionParameters parameters, CommercialCalculationOptions? options = null);

    /// <summary>
    /// 根据国标型钢条目与商业配置，计算米重、表面积及商业工程量
    /// </summary>
    CommercialSectionResult Calculate(StandardSteelItem standardItem, CommercialCalculationOptions? options = null);
}
