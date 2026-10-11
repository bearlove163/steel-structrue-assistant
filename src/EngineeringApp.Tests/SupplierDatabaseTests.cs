using System.Linq;
using EngineeringApp.Shared.Data;
using EngineeringApp.Shared.Material.Pricing;
using EngineeringApp.Shared.Models;
using EngineeringApp.Shared.Pricing.Coating;
using Xunit;

namespace EngineeringApp.Tests;

/// <summary>
/// 供应商库（钢厂、贸易商、吊机租赁、紧固件螺栓、特种材料[拉索/支座/楼承板]、油漆涂料）数据库与业务逻辑单元测试
/// </summary>
public class SupplierDatabaseTests
{
    [Fact]
    public void Test_SeedSuppliers_Coverage_And_Categories()
    {
        // 1. 验证基准供应商库存在且包含丰富预设
        var suppliers = SupplierDatabase.AllSuppliers;
        Assert.NotNull(suppliers);
        Assert.True(suppliers.Count >= 28, $"供应商总数应不少于 28 家，实际为 {suppliers.Count}");

        // 2. 验证五大核心品类均有覆盖
        var mills = suppliers.Where(s => s.Category == SupplierCategory.SteelMill).ToList();
        var traders = suppliers.Where(s => s.Category == SupplierCategory.SteelTrader).ToList();
        var cranes = suppliers.Where(s => s.Category == SupplierCategory.CraneRental).ToList();
        var fasteners = suppliers.Where(s => s.Category == SupplierCategory.FastenerBolt).ToList();
        var specialty = suppliers.Where(s => s.Category == SupplierCategory.SpecialtyMaterial).ToList();
        var paints = suppliers.Where(s => s.Category == SupplierCategory.PaintCoating).ToList();

        Assert.True(mills.Count >= 6, "钢厂类供应商应不少于 6 家");
        Assert.True(traders.Count >= 4, "贸易现货类供应商应不少于 4 家");
        Assert.True(cranes.Count >= 3, "吊机机械类供应商应不少于 3 家");
        Assert.True(fasteners.Count >= 4, "连接与紧固件螺栓供应商应不少于 4 家");
        Assert.True(specialty.Count >= 6, "特种构件材料供应商应不少于 6 家");
        Assert.True(paints.Count >= 5, "防腐防火涂料供应商应不少于 5 家");

        // 3. 验证具体子类代表性名录
        Assert.Contains(mills, m => m.ShortName.Contains("宝钢"));
        Assert.Contains(mills, m => m.ShortName.Contains("南钢"));
        Assert.Contains(traders, t => t.ShortName.Contains("欧冶云商"));
        Assert.Contains(cranes, c => c.ShortName.Contains("庞源") || c.Name.Contains("庞源"));
        Assert.Contains(fasteners, f => f.ShortName.Contains("标五") && f.SubCategory.Contains("高强螺栓"));
        Assert.Contains(specialty, s => s.ShortName.Contains("巨力") && s.SubCategory.Contains("拉索"));
        Assert.Contains(specialty, s => s.ShortName.Contains("欧维姆") && s.SubCategory.Contains("支座"));
        Assert.Contains(specialty, s => s.ShortName.Contains("精工") && s.SubCategory.Contains("楼承板"));
        Assert.Contains(paints, p => p.ShortName.Contains("佐敦"));
    }

    [Fact]
    public void Test_Filter_By_Category_And_Dynamic_SubCategory()
    {
        // 1. 按品类过滤
        var boltList = SupplierDatabase.Filter(SupplierCategory.FastenerBolt).ToList();
        Assert.All(boltList, b => Assert.Equal(SupplierCategory.FastenerBolt, b.Category));

        // 2. 动态提取所有子类标签
        var allSubCats = SupplierDatabase.GetAllSubCategories();
        Assert.NotEmpty(allSubCats);
        Assert.Contains(allSubCats, s => s.Contains("高强螺栓"));
        Assert.Contains(allSubCats, s => s.Contains("拉索"));
        Assert.Contains(allSubCats, s => s.Contains("支座"));
        Assert.Contains(allSubCats, s => s.Contains("楼承板"));

        // 3. 按细分子类过滤
        var cableSuppliers = SupplierDatabase.Filter(SupplierCategory.SpecialtyMaterial, subCategory: "拉索").ToList();
        Assert.NotEmpty(cableSuppliers);
        Assert.All(cableSuppliers, s => Assert.Contains("拉索", s.SubCategory));

        var bearingSuppliers = SupplierDatabase.Filter(SupplierCategory.SpecialtyMaterial, subCategory: "支座").ToList();
        Assert.NotEmpty(bearingSuppliers);
        Assert.All(bearingSuppliers, s => Assert.Contains("支座", s.SubCategory));
    }

    [Fact]
    public void Test_Keyword_Search()
    {
        // 按企业名称搜索
        var searchBao = SupplierDatabase.Filter(null, null, "宝山钢铁").ToList();
        Assert.Single(searchBao);
        Assert.Equal("SUP-MILL-001", searchBao[0].Code);

        // 按城市区域搜索
        var searchShanghai = SupplierDatabase.Filter(null, null, "上海").ToList();
        Assert.True(searchShanghai.Count >= 3);

        // 按联系人搜索
        var searchPerson = SupplierDatabase.Filter(null, null, "王志强").ToList();
        Assert.Single(searchPerson);
    }

    [Fact]
    public void Test_CRUD_And_Reset()
    {
        // 1. 添加新自定义品类供应商 (如阻尼器或新型螺栓厂商)
        var newSup = new SupplierItem
        {
            Name = "上海智能阻尼减震实业有限公司",
            ShortName = "智能阻尼",
            Category = SupplierCategory.OtherCustom,
            SubCategory = "调频质量阻尼器 TMD / 屈曲约束支撑 BRB",
            ContactPerson = "方总",
            ContactPhone = "021-88889999",
            CityRegion = "上海松江",
            CooperationStatus = "考察备选",
            Remarks = "超高层风振与地震阻尼控制专精特新供应商"
        };

        SupplierDatabase.AddSupplier(newSup);
        Assert.True(newSup.Id > 0);
        Assert.StartsWith("SUP-CUS", newSup.Code);

        var retrieved = SupplierDatabase.GetById(newSup.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("上海智能阻尼减震实业有限公司", retrieved.Name);

        // 2. 修改更新
        retrieved.CooperationStatus = "战略合作";
        retrieved.ContactPerson = "方总工";
        bool updated = SupplierDatabase.UpdateSupplier(retrieved);
        Assert.True(updated);

        var updatedItem = SupplierDatabase.GetById(newSup.Id);
        Assert.NotNull(updatedItem);
        Assert.Equal("战略合作", updatedItem.CooperationStatus);
        Assert.Equal("方总工", updatedItem.ContactPerson);

        // 3. 删除
        bool removed = SupplierDatabase.RemoveSupplier(newSup.Id);
        Assert.True(removed);
        Assert.Null(SupplierDatabase.GetById(newSup.Id));

        // 4. 重置基准数据
        SupplierDatabase.ResetToDefaults();
        Assert.True(SupplierDatabase.AllSuppliers.Count >= 28);
    }

    [Fact]
    public void Test_Json_Export_And_Import()
    {
        // 导出
        string json = SupplierDatabase.ExportToJson();
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("宝山钢铁", json);
        Assert.Contains("SUP-MILL-001", json);

        // 导入追加测试
        string testImportJson = @"[
            {
                ""Id"": 9999,
                ""Code"": ""SUP-TEST-999"",
                ""Name"": ""北京装配式空间结构配件科技有限公司"",
                ""ShortName"": ""北京装配科技"",
                ""Category"": 5,
                ""SubCategory"": ""特种铝合金网壳节点 / 空间铸钢节点"",
                ""ContactPerson"": ""赵工"",
                ""ContactPhone"": ""010-12345678"",
                ""CityRegion"": ""北京大兴"",
                ""CooperationStatus"": ""合格在册"",
                ""Remarks"": ""大跨度大温差节点铸造测试数据""
            }
        ]";

        bool importSuccess = SupplierDatabase.ImportFromJson(testImportJson, overwrite: false);
        Assert.True(importSuccess);

        var imported = SupplierDatabase.AllSuppliers.FirstOrDefault(s => s.Code == "SUP-TEST-999");
        Assert.NotNull(imported);
        Assert.Equal("北京装配式空间结构配件科技有限公司", imported.Name);

        // 清理测试数据
        SupplierDatabase.RemoveSupplier(imported.Id);
    }

    [Fact]
    public void Test_Downstream_Material_And_Coating_SupplierId_Integration()
    {
        // 验证材料出处模型关联 SupplierId
        var record = new MaterialProvenanceRecord
        {
            Id = 101,
            MaterialTag = "TAG-Q355B-TEST",
            DisplayTitle = "Q355B 钢板 (宝钢)",
            InquiryVendor = "宝钢股份",
            SupplierId = 1
        };
        Assert.Equal(1, record.SupplierId);

        // 验证涂料询价模型关联 SupplierId
        var quote = new BrandQuoteItem
        {
            BrandName = "佐敦 Jotun",
            SupplierName = "佐敦涂料 (张家港) 有限公司",
            SupplierId = 24,
            UnitPrice = 48.0
        };
        Assert.Equal(24, quote.SupplierId);
    }

    [Fact]
    public void Test_SteelMill_Pricing_Engine_Dynamic_Integration()
    {
        // 1. 验证 SupplierDatabase.GetSteelMillsAsBrands() 成功将供应商库的钢厂转换为 SteelMillBrand
        var brands = SupplierDatabase.GetSteelMillsAsBrands();
        Assert.NotEmpty(brands);
        var baosteel = brands.FirstOrDefault(b => b.Name.Contains("宝钢"));
        Assert.NotNull(baosteel);
        Assert.Equal(200.0, baosteel.BrandPremiumPerTon);

        // 2. 验证 SeedSteelMills.AllMills 动态获取供应商库的钢厂
        var allMills = SeedSteelMills.AllMills;
        Assert.Contains(allMills, m => m.Id == "SUP-MILL-001");

        // 3. 验证 SeedSteelMills.GetById 支持历史 ID "Baosteel" 映射至供应商库的宝钢
        var millById = SeedSteelMills.GetById("Baosteel");
        Assert.NotNull(millById);
        Assert.Equal(200.0, millById.BrandPremiumPerTon);

        // 4. 验证在供应商库中动态新增一家新钢厂，估值引擎能即时识别其基准溢价
        var newMill = new SupplierItem
        {
            Name = "沙钢新特板智造基地",
            ShortName = "沙钢特板",
            Category = SupplierCategory.SteelMill,
            SubCategory = "超宽中厚板",
            CityRegion = "江苏张家港",
            BenchmarkPremiumPerTon = 110.0
        };
        SupplierDatabase.AddSupplier(newMill);

        try
        {
            var dynamicMill = SeedSteelMills.GetById(newMill.Code);
            Assert.NotNull(dynamicMill);
            Assert.Equal("沙钢新特板智造基地", dynamicMill.Name);
            Assert.Equal(110.0, dynamicMill.BrandPremiumPerTon);

            // 5. 验证 PlatePricingRuleEngine 自动调用新钢厂并计入溢价
            var result = PlatePricingRuleEngine.Calculate(new PlatePricingParameters
            {
                Standard = StandardSystem.GB,
                Grade = EngineeringApp.Shared.Material.SteelGrade.Q355B,
                ThicknessMm = 20,
                SteelMillId = newMill.Code
            });
            Assert.Equal(110.0, result.BenchmarkMillPremium);
            Assert.Equal(110.0, result.MillPremiumPerTon);
            Assert.Contains("沙钢新特板智造基地", result.FullDescription);
        }
        finally
        {
            SupplierDatabase.RemoveSupplier(newMill.Id);
        }
    }
}
