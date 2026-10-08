namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 标准化材料 Describe 描述字符串构造器
/// </summary>
public static class MaterialDescriptorBuilder
{
    public static string BuildPlateDescribe(
        StandardMaterialItemPrice item,
        PlatePricingParameters parameters,
        IStandardPricingStrategy strategy)
    {
        var tokens = new List<string>();

        // 1. 品类与材质牌号
        string categoryName = parameters.Standard == StandardSystem.GB ? "钢板" : "Steel Plate";
        tokens.Add($"{categoryName} {item.StandardSpecification}");

        // 2. 钢厂与产地
        tokens.Add(item.MillName);

        // 3. 截面尺寸
        tokens.Add(item.DimensionText);

        // 4. 公差精度
        tokens.Add(strategy.GetLocalizedToleranceName(parameters.Tolerance));

        // 5. 探伤与特殊认证
        var inspects = new List<string>();
        if (parameters.UT == UltrasonicInspection.ClassI) inspects.Add(parameters.Standard == StandardSystem.GB ? "一级探伤" : "UT Class I");
        else if (parameters.UT == UltrasonicInspection.ClassII) inspects.Add(parameters.Standard == StandardSystem.GB ? "二级探伤" : "UT Class II");

        if (parameters.Certificate == InspectionCertificateType.EN10204_32)
        {
            inspects.Add(parameters.Standard == StandardSystem.GB ? "EN 10204 3.2 第三方检验证书" : "EN 10204 Type 3.2 Witnessed");
        }
        else if (parameters.Certificate == InspectionCertificateType.EN10204_31)
        {
            inspects.Add("EN 10204 Type 3.1 MTC");
        }

        if (inspects.Count > 0)
        {
            tokens.Add(string.Join(" & ", inspects));
        }

        // 6. 交货与调运
        tokens.Add(item.DeliveryAndFreightText);

        // 7. 定尺状态
        string cutDesc = parameters.CutType switch
        {
            PlateDimensionCutType.FixedDimension => parameters.Standard == StandardSystem.GB ? "定宽定尺" : "Fixed Cut",
            PlateDimensionCutType.SuperWide => parameters.Standard == StandardSystem.GB ? "特宽板定尺" : "Super-Wide Fixed",
            PlateDimensionCutType.SuperLong => parameters.Standard == StandardSystem.GB ? "超长板定尺" : "Super-Long Fixed",
            PlateDimensionCutType.SmallCut => parameters.Standard == StandardSystem.GB ? "小定尺精密下料" : "Small Dimension Cut",
            _ => parameters.Standard == StandardSystem.GB ? "钢厂通用散尺" : "Mill Standard"
        };
        tokens.Add(cutDesc);

        return string.Join(" | ", tokens);
    }

    public static string BuildProfileDescribe(
        StandardMaterialItemPrice item,
        ProfilePricingParameters parameters,
        IStandardPricingStrategy strategy)
    {
        var tokens = new List<string>();

        // 1. 品类与型号
        string prefix = parameters.Category switch
        {
            ProfileCategory.HotRolled_H => parameters.Standard == StandardSystem.GB ? "热轧H型钢" : "Hot-Rolled H-Beam",
            ProfileCategory.Seamless_CHS => parameters.Standard == StandardSystem.GB ? "无缝钢管" : "Seamless Tube",
            ProfileCategory.Welded_CHS => parameters.Standard == StandardSystem.GB ? "直缝高频焊管" : "ERW Welded Pipe",
            ProfileCategory.RoundToSquare_RHS => parameters.Standard == StandardSystem.GB ? "方矩管 (圆转方冷成型)" : "RHS Box (Round-to-Square Cold Formed)",
            _ => parameters.Standard == StandardSystem.GB ? "冷弯方矩管" : "Cold-formed RHS"
        };

        tokens.Add($"{prefix} {parameters.Grade}");

        // 2. 钢厂
        tokens.Add(item.MillName);

        // 3. 规格尺寸
        tokens.Add($"{parameters.SectionDesignation} (L={parameters.LengthMeter:0.#}m)");

        // 4. 工艺特性
        if (parameters.Category == ProfileCategory.RoundToSquare_RHS)
        {
            tokens.Add(parameters.NeedsCornerStressReliefAnneal ? "角部去应力退火" : "特种冷挤压成型");
        }
        else if (parameters.Category == ProfileCategory.Seamless_CHS)
        {
            tokens.Add("热轧斜轧穿孔连轧");
        }
        else
        {
            tokens.Add("国标热轧成品");
        }

        // 5. 交付与物流
        tokens.Add(item.DeliveryAndFreightText);

        // 6. 定尺
        tokens.Add(parameters.IsFixedLength ? "定尺供货" : "散尺出厂");

        return string.Join(" | ", tokens);
    }
}
