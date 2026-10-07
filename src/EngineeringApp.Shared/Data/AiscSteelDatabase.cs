using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 美国标准型钢数据库 (AISC 15th Edition, ASTM A6 / A500)
/// 包含常用美标 W 宽翼缘梁柱、HP 桩型、C 槽钢及 HSS 方矩管
/// </summary>
public static class AiscSteelDatabase
{
    private static readonly List<StandardSteelItem> _items = [];

    static AiscSteelDatabase()
    {
        InitializeWShapes();
        InitializeHpShapes();
        InitializeChannels();
        InitializeHss();
    }

    public static IReadOnlyList<StandardSteelItem> AllItems => _items;

    private static void AddW(string category, string name, double dMm, double bfMm, double twMm, double tfMm,
        double aCm2, double massKgM, double ixCm4, double iyCm4, double sxCm3, double syCm3)
    {
        _items.Add(new StandardSteelItem
        {
            StandardSystem = "AISC (美标)",
            Category = category,
            Designation = name,
            SectionType = SectionType.HBeam,
            Height = dMm,
            Width = bfMm,
            WebThickness = twMm,
            FlangeThickness = tfMm,
            StandardAreaCm2 = aCm2,
            StandardMassKgM = massKgM,
            StandardIxCm4 = ixCm4,
            StandardIyCm4 = iyCm4,
            StandardWxCm3 = sxCm3,
            StandardWyCm3 = syCm3
        });
    }

    private static void AddC(string category, string name, double dMm, double bfMm, double twMm, double tfMm,
        double aCm2, double massKgM, double ixCm4, double iyCm4, double sxCm3, double syCm3)
    {
        _items.Add(new StandardSteelItem
        {
            StandardSystem = "AISC (美标)",
            Category = category,
            Designation = name,
            SectionType = SectionType.Channel,
            Height = dMm,
            Width = bfMm,
            WebThickness = twMm,
            FlangeThickness = tfMm,
            StandardAreaCm2 = aCm2,
            StandardMassKgM = massKgM,
            StandardIxCm4 = ixCm4,
            StandardIyCm4 = iyCm4,
            StandardWxCm3 = sxCm3,
            StandardWyCm3 = syCm3
        });
    }

    private static void AddHss(string category, string name, double hMm, double bMm, double tMm,
        double aCm2, double massKgM, double ixCm4, double iyCm4, double sxCm3, double syCm3)
    {
        _items.Add(new StandardSteelItem
        {
            StandardSystem = "AISC (美标)",
            Category = category,
            Designation = name,
            SectionType = SectionType.RHS,
            Height = hMm,
            Width = bMm,
            WebThickness = tMm,
            FlangeThickness = tMm,
            WallThickness = tMm,
            StandardAreaCm2 = aCm2,
            StandardMassKgM = massKgM,
            StandardIxCm4 = ixCm4,
            StandardIyCm4 = iyCm4,
            StandardWxCm3 = sxCm3,
            StandardWyCm3 = syCm3
        });
    }

    private static void InitializeWShapes()
    {
        const string cat = "AISC W宽翼缘型钢 (Wide-Flange)";
        AddW(cat, "W8×10", 200, 100, 4.3, 5.2, 19.1, 14.9, 1280, 87, 128, 17);
        AddW(cat, "W8×18", 207, 133, 5.8, 8.4, 34.2, 26.8, 2580, 330, 249, 49.7);
        AddW(cat, "W8×24", 201, 165, 6.2, 10.2, 45.7, 35.9, 3450, 753, 342, 91.3);
        AddW(cat, "W10×22", 259, 146, 6.1, 9.1, 41.9, 32.7, 4910, 475, 379, 65.1);
        AddW(cat, "W10×33", 247, 202, 7.4, 11.0, 62.6, 49.1, 7120, 1520, 577, 151);
        AddW(cat, "W10×49", 253, 254, 8.6, 14.2, 92.9, 72.9, 11300, 3880, 893, 305);
        AddW(cat, "W12×26", 310, 165, 5.8, 9.7, 49.4, 38.7, 8490, 720, 547, 87.3);
        AddW(cat, "W12×50", 310, 205, 9.4, 16.3, 94.2, 74.4, 16400, 2340, 1060, 228);
        AddW(cat, "W12×72", 311, 306, 10.9, 17.0, 136.0, 107.0, 24800, 8120, 1600, 531);
        AddW(cat, "W14×30", 352, 171, 6.9, 9.8, 57.1, 44.6, 12100, 816, 688, 95.4);
        AddW(cat, "W14×68", 356, 255, 10.5, 18.3, 129.0, 101.0, 30100, 5040, 1690, 395);
        AddW(cat, "W14×90", 356, 369, 11.2, 18.0, 171.0, 134.0, 41600, 15100, 2340, 819);
        AddW(cat, "W16×40", 407, 178, 7.7, 12.8, 76.1, 59.5, 21600, 1200, 1060, 135);
        AddW(cat, "W18×35", 450, 152, 7.6, 10.8, 66.5, 52.1, 21200, 637, 942, 83.8);
        AddW(cat, "W18×50", 457, 190, 9.0, 14.5, 94.8, 74.4, 33300, 1670, 1460, 176);
        AddW(cat, "W21×44", 525, 165, 8.9, 11.4, 83.9, 65.5, 35100, 862, 1340, 104);
        AddW(cat, "W21×68", 537, 210, 10.9, 17.4, 129.0, 101.0, 61600, 2680, 2300, 255);
        AddW(cat, "W24×55", 599, 178, 10.0, 12.8, 105.0, 81.8, 56200, 1210, 1880, 136);
        AddW(cat, "W24×68", 603, 228, 10.5, 14.9, 130.0, 101.0, 76200, 2930, 2520, 257);
        AddW(cat, "W24×84", 612, 229, 11.9, 19.6, 159.0, 125.0, 98600, 3930, 3220, 343);
        AddW(cat, "W27×84", 678, 253, 11.7, 16.3, 160.0, 125.0, 119000, 4410, 3510, 349);
        AddW(cat, "W30×90", 750, 264, 12.0, 15.5, 170.0, 134.0, 150000, 4790, 4000, 363);
        AddW(cat, "W36×135", 903, 304, 15.2, 20.1, 257.0, 201.0, 325000, 9530, 7200, 627);
    }

    private static void InitializeHpShapes()
    {
        const string cat = "AISC HP桩型钢 (Bearing Piles)";
        AddW(cat, "HP10×42", 246, 256, 10.5, 10.7, 80.0, 62.5, 8740, 3000, 711, 234);
        AddW(cat, "HP12×53", 299, 306, 11.0, 11.0, 100.0, 78.9, 16400, 5290, 1100, 346);
        AddW(cat, "HP14×73", 345, 371, 12.8, 12.8, 138.0, 109.0, 30300, 11000, 1760, 593);
        AddW(cat, "HP14×89", 351, 378, 15.6, 15.6, 168.0, 132.0, 37600, 13500, 2140, 714);
    }

    private static void InitializeChannels()
    {
        const string cat = "AISC C型槽钢 (American Standard Channels)";
        AddC(cat, "C6×8.2", 152, 49, 5.1, 8.7, 15.4, 12.2, 545, 29, 71.8, 8.0);
        AddC(cat, "C8×11.5", 203, 57, 5.6, 9.9, 21.7, 17.1, 1360, 55, 134, 12.8);
        AddC(cat, "C10×15.3", 254, 66, 6.1, 11.1, 28.8, 22.8, 2800, 95, 220, 18.9);
        AddC(cat, "C12×20.7", 305, 75, 7.2, 12.7, 39.2, 30.8, 5370, 161, 352, 28.5);
        AddC(cat, "C15×33.9", 381, 86, 10.2, 16.5, 64.5, 50.4, 13100, 337, 688, 48.7);
    }

    private static void InitializeHss()
    {
        const string cat = "AISC HSS矩形/方矩管 (Hollow Structural Sections)";
        AddHss(cat, "HSS 4×4×1/4", 101.6, 101.6, 6.4, 21.9, 17.2, 346, 346, 68.2, 68.2);
        AddHss(cat, "HSS 6×6×3/8", 152.4, 152.4, 9.5, 49.7, 39.0, 1710, 1710, 224, 224);
        AddHss(cat, "HSS 8×8×1/2", 203.2, 203.2, 12.7, 88.5, 69.5, 5490, 5490, 541, 541);
        AddHss(cat, "HSS 8×4×1/4", 203.2, 101.6, 6.4, 35.1, 27.5, 1870, 616, 184, 121);
        AddHss(cat, "HSS 10×6×3/8", 254.0, 152.4, 9.5, 67.7, 53.2, 6240, 2830, 492, 372);
        AddHss(cat, "HSS 12×8×1/2", 304.8, 203.2, 12.7, 114.0, 89.5, 15400, 8120, 1010, 799);
    }
}
