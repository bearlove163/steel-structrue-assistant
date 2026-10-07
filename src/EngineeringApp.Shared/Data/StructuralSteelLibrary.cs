using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 全球标准型钢库集成服务 (支持 GB/T 国标、EN 欧标、AISC 美标无缝检索与切换)
/// </summary>
public static class StructuralSteelLibrary
{
    private static readonly List<StandardSteelItem> _allSections = [];

    static StructuralSteelLibrary()
    {
        // 载入国标
        _allSections.AddRange(StandardSteelDatabase.AllItems);
        // 载入欧标
        _allSections.AddRange(EuroSteelDatabase.AllItems);
        // 载入美标
        _allSections.AddRange(AiscSteelDatabase.AllItems);
    }

    public static IReadOnlyList<StandardSteelItem> AllSections => _allSections;

    public static IEnumerable<string> AllStandardSystems =>
        _allSections.Select(s => s.StandardSystem).Distinct();

    public static IEnumerable<StandardSteelItem> GetByStandardSystem(string system) =>
        _allSections.Where(s => s.StandardSystem.Equals(system, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<string> GetCategoriesBySystem(string system) =>
        _allSections.Where(s => s.StandardSystem.Equals(system, StringComparison.OrdinalIgnoreCase))
                    .Select(s => s.Category)
                    .Distinct();

    public static IEnumerable<StandardSteelItem> Filter(string? system, string? category, string? keyword)
    {
        var q = _allSections.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(system) && system != "全部标准体系")
        {
            q = q.Where(s => s.StandardSystem.Contains(system, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category) && category != "全部系列")
        {
            q = q.Where(s => s.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            q = q.Where(s => s.Designation.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                             s.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        return q;
    }
}
