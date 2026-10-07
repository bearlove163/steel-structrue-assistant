using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 中国国家标准热轧型钢与常用钢管数据库 (GB/T 11263, GB/T 706, GB/T 6728, GB/T 3091)
/// </summary>
public static class StandardSteelDatabase
{
    private static readonly List<StandardSteelItem> _items = [];

    static StandardSteelDatabase()
    {
        InitializeHBeams();
        InitializeIBeams();
        InitializeChannels();
        InitializeAngles();
        InitializeHollowSections();
    }

    public static IReadOnlyList<StandardSteelItem> AllItems => _items;

    public static IEnumerable<string> Categories => _items.Select(x => x.Category).Distinct();

    private static void InitializeHBeams()
    {
        // GB/T 11263 热轧H型钢 HW (宽翼缘)
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 100×100×6×8", 100, 100, 6, 8, 10, 17.2, 13.5, 383, 134, 76.5, 26.7);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 125×125×6.5×9", 125, 125, 6.5, 9, 10, 23.8, 18.7, 839, 293, 134, 46.9);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 150×150×7×10", 150, 150, 7, 10, 11, 31.55, 24.8, 1640, 563, 219, 75.1);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 175×175×7.5×11", 175, 175, 7.5, 11, 12, 40.5, 31.8, 2900, 984, 331, 112);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 200×200×8×12", 200, 200, 8, 12, 13, 50.5, 39.7, 4720, 1600, 472, 160);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 200×204×12×12", 200, 204, 12, 12, 13, 58.7, 46.1, 5140, 1700, 514, 167);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 250×250×9×14", 250, 250, 9, 14, 16, 68.3, 53.6, 10800, 3650, 861, 292);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 250×255×14×14", 250, 255, 14, 14, 16, 80.8, 63.4, 11900, 3880, 955, 304);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 300×300×10×15", 300, 300, 10, 15, 18, 88.5, 69.5, 20400, 6750, 1360, 450);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 350×350×12×19", 350, 350, 12, 19, 20, 123.6, 97.0, 39800, 13600, 2280, 777);
        AddH("GB/T 11263 热轧H型钢(HW)", "HW 400×400×13×21", 400, 400, 13, 21, 22, 155.4, 122.0, 66600, 22400, 3330, 1120);

        // GB/T 11263 热轧H型钢 HM (中翼缘)
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 150×100×6×9", 148, 100, 6, 9, 11, 26.8, 21.1, 1020, 151, 138, 30.1);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 200×150×6×9", 194, 150, 6, 9, 13, 39.0, 30.6, 2690, 507, 277, 67.6);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 250×175×7×11", 244, 175, 7, 11, 16, 56.2, 44.1, 6120, 984, 502, 113);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 300×200×8×12", 294, 200, 8, 12, 18, 72.4, 56.8, 11300, 1600, 771, 160);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 350×250×9×14", 340, 250, 9, 14, 20, 101.5, 79.7, 22800, 3650, 1340, 292);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 400×300×10×16", 390, 300, 10, 16, 22, 136.7, 107.0, 41700, 7200, 2140, 480);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 500×300×11×18", 488, 300, 11, 18, 26, 163.5, 128.0, 71000, 8110, 2910, 541);
        AddH("GB/T 11263 热轧H型钢(HM)", "HM 600×300×12×20", 588, 300, 12, 20, 28, 192.5, 151.0, 118000, 9020, 4020, 601);

        // GB/T 11263 热轧H型钢 HN (窄翼缘)
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 150×75×5×7", 150, 75, 5, 7, 8, 17.85, 14.0, 666, 49.5, 88.8, 13.2);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 200×100×5.5×8", 198, 99, 5.5, 8, 11, 23.6, 18.5, 1580, 129, 160, 26.0);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 250×125×6×9", 250, 125, 6, 9, 12, 37.7, 29.6, 4050, 294, 324, 47.0);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 300×150×6.5×9", 300, 150, 6.5, 9, 13, 46.8, 36.7, 7210, 508, 481, 67.7);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 350×175×7×11", 350, 175, 7, 11, 14, 63.7, 50.0, 13600, 984, 775, 112);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 400×200×8×13", 400, 200, 8, 13, 16, 84.1, 66.0, 23700, 1740, 1190, 174);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 450×200×9×14", 450, 200, 9, 14, 18, 96.8, 76.0, 33500, 1870, 1490, 187);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 500×200×10×16", 500, 200, 10, 16, 20, 114.2, 89.6, 47800, 2140, 1910, 214);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 600×200×11×17", 600, 200, 11, 17, 22, 134.4, 106.0, 77600, 2280, 2590, 228);
        AddH("GB/T 11263 热轧H型钢(HN)", "HN 700×300×13×24", 700, 300, 13, 24, 28, 235.5, 185.0, 201000, 10800, 5760, 722);
    }

    private static void InitializeIBeams()
    {
        // GB/T 706 普通工字钢
        AddI("10", 100, 68, 4.5, 7.6, 6.5, 14.3, 11.3, 245, 33.0, 49.0, 9.7);
        AddI("12.6", 126, 74, 5.0, 8.4, 7.0, 18.1, 14.2, 488, 48.8, 77.5, 13.2);
        AddI("14", 140, 80, 5.5, 9.1, 7.5, 21.5, 16.9, 712, 63.5, 102, 15.9);
        AddI("16", 160, 88, 6.0, 9.9, 8.0, 26.1, 20.5, 1130, 93.1, 141, 21.2);
        AddI("18", 180, 94, 6.5, 10.7, 8.5, 30.6, 24.1, 1660, 122, 185, 26.0);
        AddI("20a", 200, 100, 7.0, 11.4, 9.0, 35.5, 27.9, 2370, 158, 237, 31.6);
        AddI("20b", 200, 102, 9.0, 11.4, 9.0, 39.5, 31.1, 2500, 169, 250, 33.1);
        AddI("22a", 220, 110, 7.5, 12.3, 9.5, 42.0, 33.1, 3400, 226, 309, 41.1);
        AddI("25a", 250, 116, 8.0, 13.0, 10.0, 48.5, 38.1, 5020, 283, 402, 48.8);
        AddI("28a", 280, 122, 8.5, 13.7, 10.5, 55.4, 43.5, 7110, 353, 508, 57.9);
        AddI("32a", 320, 130, 9.5, 15.0, 11.5, 67.1, 52.7, 11100, 483, 692, 74.3);
        AddI("36a", 360, 136, 10.0, 15.8, 12.0, 76.5, 60.0, 15800, 606, 876, 89.1);
        AddI("40a", 400, 142, 10.5, 16.5, 12.5, 86.1, 67.6, 21700, 746, 1090, 105);
        AddI("45a", 450, 150, 11.5, 18.0, 13.5, 102.0, 80.4, 32200, 999, 1430, 133);
        AddI("50a", 500, 158, 12.0, 20.0, 14.0, 119.0, 93.6, 46500, 1340, 1860, 170);
    }

    private static void InitializeChannels()
    {
        // GB/T 706 热轧普通槽钢
        AddC("8", 80, 43, 5.0, 8.0, 7.5, 10.2, 8.05, 101, 16.6, 25.3, 5.75);
        AddC("10", 100, 48, 5.3, 8.5, 8.0, 12.7, 10.0, 198, 25.6, 39.7, 7.82);
        AddC("12", 120, 53, 5.5, 9.0, 8.5, 15.4, 12.1, 364, 38.8, 60.6, 10.7);
        AddC("14a", 140, 58, 6.0, 9.5, 9.0, 18.5, 14.5, 564, 54.2, 80.5, 13.7);
        AddC("16a", 160, 63, 6.5, 10.0, 9.5, 21.9, 17.2, 866, 73.1, 108, 17.3);
        AddC("18a", 180, 68, 7.0, 10.5, 10.0, 25.7, 20.2, 1270, 96.5, 141, 21.4);
        AddC("20a", 200, 73, 7.0, 11.0, 10.5, 28.8, 22.6, 1780, 123, 178, 25.7);
        AddC("22a", 220, 77, 7.0, 11.5, 11.0, 31.8, 25.0, 2390, 150, 218, 29.8);
        AddC("25a", 250, 78, 7.0, 12.0, 11.5, 34.9, 27.4, 3180, 169, 254, 31.9);
        AddC("28a", 280, 82, 7.5, 12.5, 12.0, 40.0, 31.4, 4520, 222, 323, 39.5);
        AddC("32a", 320, 88, 8.0, 13.0, 12.5, 48.5, 38.1, 6960, 308, 435, 51.7);
    }

    private static void InitializeAngles()
    {
        // GB/T 706 热轧等边角钢
        AddL("L 50×5", 50, 50, 5, 5.5, 4.80, 3.77, 11.2, 11.2, 3.14, 3.14);
        AddL("L 63×6", 63, 63, 6, 7.0, 7.29, 5.72, 27.1, 27.1, 6.07, 6.07);
        AddL("L 75×6", 75, 75, 6, 8.0, 8.78, 6.90, 46.7, 46.7, 8.75, 8.75);
        AddL("L 90×8", 90, 90, 8, 9.5, 13.9, 10.9, 105, 105, 16.3, 16.3);
        AddL("L 100×10", 100, 100, 10, 10.0, 19.3, 15.1, 179, 179, 24.7, 24.7);
        AddL("L 125×10", 125, 125, 10, 11.5, 24.4, 19.1, 359, 359, 39.9, 39.9);
        AddL("L 140×12", 140, 140, 12, 13.0, 32.5, 25.5, 597, 597, 59.8, 59.8);
        AddL("L 160×14", 160, 160, 14, 14.0, 43.1, 33.8, 1030, 1030, 90.0, 90.0);
        AddL("L 180×16", 180, 180, 16, 16.0, 55.4, 43.5, 1670, 1670, 131, 131);
        AddL("L 200×20", 200, 200, 20, 18.0, 76.5, 60.1, 2830, 2830, 200, 200);
    }

    private static void InitializeHollowSections()
    {
        // 常用结构方矩管 (GB/T 6728)
        AddRhs("□ 100×100×5", 100, 100, 5, 18.4, 14.4, 279, 279, 55.8, 55.8);
        AddRhs("□ 150×150×6", 150, 150, 6, 34.1, 26.8, 1180, 1180, 157, 157);
        AddRhs("□ 200×200×8", 200, 200, 8, 59.7, 46.9, 3620, 3620, 362, 362);
        AddRhs("□ 250×250×10", 250, 250, 10, 93.3, 73.2, 8910, 8910, 713, 713);
        AddRhs("□ 300×300×10", 300, 300, 10, 113.3, 89.0, 15800, 15800, 1050, 1050);
        AddRhs("□ 200×100×6", 200, 100, 6, 33.4, 26.2, 1720, 563, 172, 113);
        AddRhs("□ 300×200×8", 300, 200, 8, 74.4, 58.4, 9950, 5320, 663, 532);

        // 常用结构圆管 (GB/T 3091 / GB/T 8162)
        AddChs("○ Φ114×4.5", 114, 4.5, 15.5, 12.2, 237, 237, 41.6, 41.6);
        AddChs("○ Φ168×6", 168, 6.0, 30.5, 24.0, 1010, 1010, 120, 120);
        AddChs("○ Φ219×6", 219, 6.0, 40.1, 31.5, 2280, 2280, 208, 208);
        AddChs("○ Φ219×8", 219, 8.0, 53.0, 41.6, 2970, 2970, 271, 271);
        AddChs("○ Φ273×8", 273, 8.0, 66.6, 52.3, 5850, 5850, 429, 429);
        AddChs("○ Φ325×10", 325, 10.0, 98.9, 77.6, 12400, 12400, 763, 763);
        AddChs("○ Φ426×12", 426, 12.0, 156.1, 122.5, 33700, 33700, 1580, 1580);
    }

    private static void AddH(string cat, string des, double h, double b, double tw, double tf, double r, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = cat,
            Designation = des,
            SectionType = SectionType.HBeam,
            Height = h,
            Width = b,
            WebThickness = tw,
            FlangeThickness = tf,
            RootRadius = r,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }

    private static void AddI(string des, double h, double b, double tw, double tf, double r, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = "GB/T 706 普通工字钢",
            Designation = "I " + des,
            SectionType = SectionType.HBeam,
            Height = h,
            Width = b,
            WebThickness = tw,
            FlangeThickness = tf,
            RootRadius = r,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }

    private static void AddC(string des, double h, double b, double tw, double tf, double r, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = "GB/T 706 普通槽钢",
            Designation = "[" + des,
            SectionType = SectionType.Channel,
            Height = h,
            Width = b,
            WebThickness = tw,
            FlangeThickness = tf,
            RootRadius = r,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }

    private static void AddL(string des, double b1, double b2, double t, double r, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = "GB/T 706 等边角钢",
            Designation = des,
            SectionType = SectionType.Angle,
            Height = b1,
            Width = b2,
            LegWidth1 = b1,
            LegWidth2 = b2,
            LegThickness = t,
            RootRadius = r,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }

    private static void AddRhs(string des, double h, double b, double t, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = "GB/T 6728 矩形空心型钢(RHS)",
            Designation = des,
            SectionType = SectionType.RHS,
            Height = h,
            Width = b,
            WebThickness = t,
            FlangeThickness = t,
            WallThickness = t,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }

    private static void AddChs(string des, double d, double t, double a, double m, double ix, double iy, double wx, double wy)
    {
        _items.Add(new StandardSteelItem
        {
            Category = "GB/T 3091/8162 圆形空心钢管(CHS)",
            Designation = des,
            SectionType = SectionType.CHS,
            Height = d,
            Width = d,
            OuterDiameter = d,
            WallThickness = t,
            StandardAreaCm2 = a,
            StandardMassKgM = m,
            StandardIxCm4 = ix,
            StandardIyCm4 = iy,
            StandardWxCm3 = wx,
            StandardWyCm3 = wy
        });
    }
}
