using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Data;

/// <summary>
/// 欧洲标准型钢数据库 (EN 10025, EN 10034, EN 10210, DIN 1025)
/// 包含常用欧标 IPE, HEA, HEB, HEM 及 UPN 截面
/// </summary>
public static class EuroSteelDatabase
{
    private static readonly List<StandardSteelItem> _items = [];

    static EuroSteelDatabase()
    {
        InitializeIpe();
        InitializeHea();
        InitializeHeb();
        InitializeHem();
        InitializeUpn();
    }

    public static IReadOnlyList<StandardSteelItem> AllItems => _items;

    private static void AddH(string category, string name, double h, double b, double tw, double tf, double r,
        double aCm2, double massKgM, double iyCm4, double izCm4, double wyCm3, double wzCm3)
    {
        _items.Add(new StandardSteelItem
        {
            StandardSystem = "EN (欧标)",
            Category = category,
            Designation = name,
            SectionType = SectionType.HBeam,
            Height = h,
            Width = b,
            WebThickness = tw,
            FlangeThickness = tf,
            RootRadius = r,
            StandardAreaCm2 = aCm2,
            StandardMassKgM = massKgM,
            StandardIxCm4 = iyCm4,
            StandardIyCm4 = izCm4,
            StandardWxCm3 = wyCm3,
            StandardWyCm3 = wzCm3
        });
    }

    private static void AddC(string category, string name, double h, double b, double tw, double tf, double r,
        double aCm2, double massKgM, double iyCm4, double izCm4, double wyCm3, double wzCm3)
    {
        _items.Add(new StandardSteelItem
        {
            StandardSystem = "EN (欧标)",
            Category = category,
            Designation = name,
            SectionType = SectionType.Channel,
            Height = h,
            Width = b,
            WebThickness = tw,
            FlangeThickness = tf,
            RootRadius = r,
            StandardAreaCm2 = aCm2,
            StandardMassKgM = massKgM,
            StandardIxCm4 = iyCm4,
            StandardIyCm4 = izCm4,
            StandardWxCm3 = wyCm3,
            StandardWyCm3 = wzCm3
        });
    }

    private static void InitializeIpe()
    {
        const string cat = "EN 10034 欧标IPE工字钢";
        AddH(cat, "IPE 80", 80, 46, 3.8, 5.2, 5, 7.64, 6.0, 80.1, 8.49, 20.0, 3.69);
        AddH(cat, "IPE 100", 100, 55, 4.1, 5.7, 7, 10.3, 8.1, 171.0, 15.9, 34.2, 5.79);
        AddH(cat, "IPE 120", 120, 64, 4.4, 6.3, 7, 13.2, 10.4, 318.0, 27.7, 53.0, 8.65);
        AddH(cat, "IPE 140", 140, 73, 4.7, 6.9, 7, 16.4, 12.9, 541.0, 44.9, 77.3, 12.3);
        AddH(cat, "IPE 160", 160, 82, 5.0, 7.4, 9, 20.1, 15.8, 869.0, 68.3, 109.0, 16.7);
        AddH(cat, "IPE 180", 180, 91, 5.3, 8.0, 9, 23.9, 18.8, 1317.0, 101.0, 146.0, 22.2);
        AddH(cat, "IPE 200", 200, 100, 5.6, 8.5, 12, 28.5, 22.4, 1943.0, 142.0, 194.0, 28.5);
        AddH(cat, "IPE 220", 220, 110, 5.9, 9.2, 12, 33.4, 26.2, 2772.0, 205.0, 252.0, 37.3);
        AddH(cat, "IPE 240", 240, 120, 6.2, 9.8, 15, 39.1, 30.7, 3892.0, 284.0, 324.0, 47.3);
        AddH(cat, "IPE 270", 270, 135, 6.6, 10.2, 15, 45.9, 36.1, 5790.0, 420.0, 429.0, 62.2);
        AddH(cat, "IPE 300", 300, 150, 7.1, 10.7, 15, 53.8, 42.2, 8356.0, 604.0, 557.0, 80.5);
        AddH(cat, "IPE 330", 330, 160, 7.5, 11.5, 18, 62.6, 49.1, 11770.0, 788.0, 713.0, 98.5);
        AddH(cat, "IPE 360", 360, 170, 8.0, 12.7, 18, 72.7, 57.1, 16270.0, 1043.0, 904.0, 123.0);
        AddH(cat, "IPE 400", 400, 180, 8.6, 13.5, 21, 84.5, 66.3, 23130.0, 1318.0, 1156.0, 146.0);
        AddH(cat, "IPE 450", 450, 190, 9.4, 14.6, 21, 98.8, 77.6, 33740.0, 1676.0, 1500.0, 176.0);
        AddH(cat, "IPE 500", 500, 200, 10.2, 16.0, 21, 116.0, 90.7, 48200.0, 2142.0, 1928.0, 214.0);
        AddH(cat, "IPE 550", 550, 210, 11.1, 17.2, 24, 134.0, 106.0, 67120.0, 2668.0, 2441.0, 254.0);
        AddH(cat, "IPE 600", 600, 220, 12.0, 19.0, 24, 156.0, 122.0, 92080.0, 3387.0, 3069.0, 308.0);
    }

    private static void InitializeHea()
    {
        const string cat = "EN 10034 欧标HEA宽翼缘";
        AddH(cat, "HE 100 A", 96, 100, 5.0, 8.0, 12, 21.2, 16.7, 349.0, 134.0, 72.8, 26.8);
        AddH(cat, "HE 120 A", 114, 120, 5.0, 8.0, 12, 25.3, 19.9, 606.0, 231.0, 106.0, 38.5);
        AddH(cat, "HE 140 A", 133, 140, 5.5, 8.5, 12, 31.4, 24.7, 1033.0, 389.0, 155.0, 55.6);
        AddH(cat, "HE 160 A", 152, 160, 6.0, 9.0, 15, 38.8, 30.4, 1673.0, 616.0, 220.0, 76.9);
        AddH(cat, "HE 180 A", 171, 180, 6.0, 9.5, 15, 45.3, 35.5, 2510.0, 925.0, 294.0, 103.0);
        AddH(cat, "HE 200 A", 190, 200, 6.5, 10.0, 18, 53.8, 42.3, 3692.0, 1336.0, 389.0, 134.0);
        AddH(cat, "HE 220 A", 210, 220, 7.0, 11.0, 18, 64.3, 50.5, 5410.0, 1955.0, 515.0, 178.0);
        AddH(cat, "HE 240 A", 230, 240, 7.5, 12.0, 21, 76.8, 60.3, 7763.0, 2769.0, 675.0, 231.0);
        AddH(cat, "HE 260 A", 250, 260, 7.5, 12.5, 24, 86.8, 68.2, 10450.0, 3668.0, 836.0, 282.0);
        AddH(cat, "HE 280 A", 270, 280, 8.0, 13.0, 24, 97.3, 76.4, 13670.0, 4763.0, 1010.0, 340.0);
        AddH(cat, "HE 300 A", 290, 300, 8.5, 14.0, 27, 112.5, 88.3, 18260.0, 6310.0, 1260.0, 421.0);
        AddH(cat, "HE 340 A", 330, 300, 9.5, 16.5, 27, 133.0, 105.0, 27690.0, 7436.0, 1678.0, 496.0);
        AddH(cat, "HE 400 A", 390, 300, 11.0, 19.0, 27, 159.0, 125.0, 45070.0, 8564.0, 2311.0, 571.0);
        AddH(cat, "HE 500 A", 490, 300, 12.0, 23.0, 27, 198.0, 155.0, 86970.0, 10370.0, 3550.0, 691.0);
        AddH(cat, "HE 600 A", 590, 300, 13.0, 25.0, 27, 226.0, 178.0, 141200.0, 11270.0, 4787.0, 751.0);
    }

    private static void InitializeHeb()
    {
        const string cat = "EN 10034 欧标HEB中宽翼缘";
        AddH(cat, "HE 100 B", 100, 100, 6.0, 10.0, 12, 26.0, 20.4, 450.0, 167.0, 89.9, 33.5);
        AddH(cat, "HE 120 B", 120, 120, 6.5, 11.0, 12, 34.0, 26.7, 864.0, 318.0, 144.0, 52.9);
        AddH(cat, "HE 140 B", 140, 140, 7.0, 12.0, 12, 43.0, 33.7, 1509.0, 550.0, 216.0, 78.5);
        AddH(cat, "HE 160 B", 160, 160, 8.0, 13.0, 15, 54.3, 42.6, 2492.0, 889.0, 311.0, 111.0);
        AddH(cat, "HE 180 B", 180, 180, 8.5, 14.0, 15, 65.3, 51.2, 3831.0, 1363.0, 426.0, 151.0);
        AddH(cat, "HE 200 B", 200, 200, 9.0, 15.0, 18, 78.1, 61.3, 5696.0, 2003.0, 570.0, 200.0);
        AddH(cat, "HE 220 B", 220, 220, 9.5, 16.0, 18, 91.0, 71.5, 8091.0, 2843.0, 736.0, 258.0);
        AddH(cat, "HE 240 B", 240, 240, 10.0, 17.0, 21, 106.0, 83.2, 11260.0, 3923.0, 938.0, 327.0);
        AddH(cat, "HE 260 B", 260, 260, 10.0, 17.5, 24, 118.0, 93.0, 14920.0, 5135.0, 1150.0, 395.0);
        AddH(cat, "HE 300 B", 300, 300, 11.0, 19.0, 27, 149.0, 117.0, 25170.0, 8563.0, 1680.0, 571.0);
        AddH(cat, "HE 340 B", 340, 300, 12.0, 21.5, 27, 171.0, 134.0, 36660.0, 9690.0, 2160.0, 646.0);
        AddH(cat, "HE 400 B", 400, 300, 13.5, 24.0, 27, 198.0, 155.0, 57680.0, 10820.0, 2880.0, 721.0);
        AddH(cat, "HE 500 B", 500, 300, 14.5, 28.0, 27, 239.0, 187.0, 107200.0, 12620.0, 4290.0, 842.0);
    }

    private static void InitializeHem()
    {
        const string cat = "EN 10034 欧标HEM特重翼缘";
        AddH(cat, "HE 100 M", 120, 106, 12.0, 20.0, 12, 53.2, 41.8, 1143.0, 399.0, 190.0, 75.3);
        AddH(cat, "HE 140 M", 160, 146, 13.0, 22.0, 12, 80.6, 63.2, 3291.0, 1144.0, 411.0, 157.0);
        AddH(cat, "HE 200 M", 220, 206, 15.0, 25.0, 18, 131.0, 103.0, 10640.0, 3651.0, 967.0, 354.0);
        AddH(cat, "HE 300 M", 340, 310, 21.0, 39.0, 27, 303.0, 238.0, 59200.0, 19400.0, 3480.0, 1250.0);
        AddH(cat, "HE 400 M", 432, 307, 21.0, 40.0, 27, 326.0, 256.0, 104100.0, 19340.0, 4820.0, 1260.0);
    }

    private static void InitializeUpn()
    {
        const string cat = "DIN 1026 欧标UPN槽钢";
        AddC(cat, "UPN 50", 50, 38, 5.0, 7.0, 7, 7.12, 5.59, 26.4, 9.12, 10.6, 3.75);
        AddC(cat, "UPN 80", 80, 45, 6.0, 8.0, 8, 11.0, 8.64, 106.0, 19.4, 26.5, 6.36);
        AddC(cat, "UPN 100", 100, 50, 6.0, 8.5, 8.5, 13.5, 10.6, 206.0, 29.3, 41.2, 8.49);
        AddC(cat, "UPN 120", 120, 55, 7.0, 9.0, 9, 17.0, 13.4, 364.0, 43.2, 60.7, 11.1);
        AddC(cat, "UPN 140", 140, 60, 7.0, 10.0, 10, 20.4, 16.0, 605.0, 62.7, 86.4, 14.8);
        AddC(cat, "UPN 160", 160, 65, 7.5, 10.5, 10.5, 24.0, 18.8, 925.0, 85.3, 116.0, 18.3);
        AddC(cat, "UPN 180", 180, 70, 8.0, 11.0, 11, 28.0, 22.0, 1350.0, 114.0, 150.0, 22.4);
        AddC(cat, "UPN 200", 200, 75, 8.5, 11.5, 11.5, 32.2, 25.3, 1910.0, 148.0, 191.0, 27.0);
        AddC(cat, "UPN 220", 220, 80, 9.0, 12.5, 12.5, 37.4, 29.4, 2690.0, 197.0, 245.0, 33.6);
        AddC(cat, "UPN 240", 240, 85, 9.5, 13.0, 13, 42.3, 33.2, 3600.0, 248.0, 300.0, 39.6);
        AddC(cat, "UPN 300", 300, 100, 10.0, 16.0, 16, 58.8, 46.2, 8030.0, 495.0, 535.0, 67.8);
    }
}
