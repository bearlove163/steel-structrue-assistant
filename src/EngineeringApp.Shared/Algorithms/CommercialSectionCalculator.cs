using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 商务软件专用的截面工程量与成本核算引擎实现
/// </summary>
public class CommercialSectionCalculator : ICommercialSectionCalculator
{
    private readonly ISectionCalculator _sectionCalculator;
    private readonly IPaintingAreaCalculator _paintingCalculator;

    public CommercialSectionCalculator(
        ISectionCalculator? sectionCalculator = null,
        IPaintingAreaCalculator? paintingCalculator = null)
    {
        _sectionCalculator = sectionCalculator ?? new ParametricSectionCalculator();
        _paintingCalculator = paintingCalculator ?? new DefaultPaintingAreaCalculator();
    }

    /// <summary>
    /// 根据参数化截面定义与商业配置，计算米重、表面积（含楼板扣除）、总重量及预估成本
    /// </summary>
    public CommercialSectionResult Calculate(SectionParameters parameters, CommercialCalculationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        options ??= new CommercialCalculationOptions();

        // 1. 计算截面力学与几何全套特性
        var prop = _sectionCalculator.Calculate(parameters);

        // 2. 根据涂装配置计算净有效涂装表面积
        double netPaintingArea = _paintingCalculator.CalculateNetPaintingArea(prop, options.Painting);

        // 3. 检查是否有匹配的国标出厂标准条目
        double? standardMass = null;
        string designation = parameters.StandardProfileName ?? $"{parameters.Type} {parameters.Width:F0}x{parameters.Height:F0}";

        if (!string.IsNullOrWhiteSpace(parameters.StandardProfileName))
        {
            var matched = StandardSteelDatabase.AllItems.FirstOrDefault(x => x.Designation == parameters.StandardProfileName);
            if (matched != null)
            {
                standardMass = matched.StandardMassKgM;
            }
        }

        // 4. 计算重量与成本
        double unitMass = standardMass ?? prop.LinearMass;
        double totalLength = Math.Max(0, options.LengthMeter) * Math.Max(0, options.Quantity);
        double totalWeight = totalLength * unitMass * (1.0 + Math.Max(0, options.SteelLossRate));
        double totalPaintingArea = totalLength * netPaintingArea;

        double steelCost = totalWeight * Math.Max(0, options.SteelUnitPricePerKg);
        double paintingCost = totalPaintingArea * Math.Max(0, options.PaintingUnitPricePerM2);

        return new CommercialSectionResult
        {
            Designation = designation,
            SectionType = parameters.Type,
            LinearMass = prop.LinearMass,
            StandardMassKgM = standardMass,
            OuterSurfaceAreaPerMeter = prop.GrossPaintingAreaPerMeter,
            TopSurfaceWidth = prop.TopSurfaceWidth,
            PaintingAreaExcludingTop = prop.PaintingAreaExcludingTop,
            NetPaintingAreaPerMeter = netPaintingArea,
            InnerSurfaceAreaPerMeter = prop.InnerPerimeter / 1000.0,
            AreaPerTon = prop.AreaPerTon,
            LengthMeter = options.LengthMeter,
            Quantity = options.Quantity,
            TotalWeightKg = totalWeight,
            TotalPaintingAreaM2 = totalPaintingArea,
            EstimatedSteelCost = steelCost,
            EstimatedPaintingCost = paintingCost,
            Properties = prop
        };
    }

    /// <summary>
    /// 根据国标型钢条目与商业配置，直接计算米重、表面积及商业工程量
    /// </summary>
    public CommercialSectionResult Calculate(StandardSteelItem standardItem, CommercialCalculationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(standardItem);

        var p = new SectionParameters
        {
            Type = standardItem.SectionType,
            Height = standardItem.Height,
            Width = standardItem.Width,
            WebThickness = standardItem.WebThickness,
            FlangeThickness = standardItem.FlangeThickness,
            OuterDiameter = standardItem.OuterDiameter,
            WallThickness = standardItem.WallThickness,
            LegWidth1 = standardItem.LegWidth1,
            LegWidth2 = standardItem.LegWidth2,
            LegThickness = standardItem.LegThickness,
            StandardProfileName = standardItem.Designation
        };

        var result = Calculate(p, options);
        result.StandardMassKgM = standardItem.StandardMassKgM;
        result.Designation = standardItem.Designation;
        return result;
    }
}
