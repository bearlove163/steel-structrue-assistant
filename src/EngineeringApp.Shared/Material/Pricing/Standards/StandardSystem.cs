namespace EngineeringApp.Shared.Material.Pricing;

/// <summary>
/// 国际化工程标准体系 (标准域)
/// </summary>
public enum StandardSystem
{
    /// <summary>中国国家标准 (GB / GB/T)</summary>
    GB,

    /// <summary>欧洲标准 (EN / EN 10025 / EN 10029)</summary>
    EN,

    /// <summary>美国标准 (ASTM / AISC / ASTM A6)</summary>
    ASTM
}

public static class StandardSystemExtensions
{
    public static string GetDisplayName(this StandardSystem system) => system switch
    {
        StandardSystem.GB => "中国国标 (GB/T)",
        StandardSystem.EN => "欧洲标准 (EN)",
        StandardSystem.ASTM => "美国标准 (ASTM)",
        _ => system.ToString()
    };
}
