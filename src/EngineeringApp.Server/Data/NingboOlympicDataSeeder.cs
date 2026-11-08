// ============================================================================
// 文件归属: src/EngineeringApp.Server/Data/NingboOlympicDataSeeder.cs
// 模块功能: 宁波奥林匹克体育中心主体育场与配套工程 BOQ 数据库基准种子装载器
// 规范依据: ORIGINAL_REQUEST.md (R3), PROJECT.md (Feature 23), AGENTS.md
// ============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using EngineeringApp.Shared.BOQ.Data;
using EngineeringApp.Shared.BOQ.Engine;
using EngineeringApp.Shared.BOQ.Models;

namespace EngineeringApp.Server.Data
{
    /// <summary>
    /// 宁波奥体中心 BOQ 基准数据库幂等填充装载器
    /// </summary>
    public static class NingboOlympicDataSeeder
    {
        public const string BenchmarkProjectCode = NingboOlympicSeedData.BenchmarkProjectCode;

        /// <summary>
        /// 幂等注入车间加工制作定额、现场安装定额、以及宁波奥体工程量清单层级数据集
        /// </summary>
        public static void Seed(AppDbContext db)
        {
            ArgumentNullException.ThrowIfNull(db);

            // 1. 幂等探测：若宁波奥体项目已入库，则无需重复执行
            if (db.BoqProjects.Any(p => p.ProjectCode == BenchmarkProjectCode))
            {
                return;
            }

            // 2. 填充车间制作定额规则字典 (Fabrication Quota Rules)
            var defaultFabRules = NingboOlympicSeedData.GetDefaultFabricationQuotaRules();
            foreach (var rule in defaultFabRules)
            {
                if (!db.FabricationQuotaRules.Any(r => r.QuotaCode == rule.QuotaCode))
                {
                    db.FabricationQuotaRules.Add(new FabricationQuotaRule
                    {
                        QuotaCode = rule.QuotaCode,
                        QuotaName = rule.QuotaName,
                        StructuralSystem = rule.StructuralSystem,
                        BaseLaborDays = rule.BaseLaborDays,
                        BaseLaborRatePerTon = rule.BaseLaborRatePerTon,
                        IntersectingCuttingUnitRate = rule.IntersectingCuttingUnitRate,
                        UltrasonicTestingUnitRate = rule.UltrasonicTestingUnitRate,
                        PreassemblyUnitRate = rule.PreassemblyUnitRate
                    });
                }
            }
            db.SaveChanges();

            // 3. 填充现场安装定额规则字典 (Installation Quota Rules)
            var defaultInsRules = NingboOlympicSeedData.GetDefaultInstallationQuotaRules();
            foreach (var rule in defaultInsRules)
            {
                if (!db.InstallationQuotaRules.Any(r => r.QuotaCode == rule.QuotaCode))
                {
                    db.InstallationQuotaRules.Add(new InstallationQuotaRule
                    {
                        QuotaCode = rule.QuotaCode,
                        QuotaName = rule.QuotaName,
                        ApplicableRole = rule.ApplicableRole,
                        BaseLaborDays = rule.BaseLaborDays,
                        BaseLaborRatePerTon = rule.BaseLaborRatePerTon,
                        MainCraneRatePerTon = rule.MainCraneRatePerTon,
                        TrestlePlatformRatePerTon = rule.TrestlePlatformRatePerTon,
                        ElevationSurchargeFactor = rule.ElevationSurchargeFactor
                    });
                }
            }
            db.SaveChanges();

            // 4. 构建定额编码至自增数据库主键的映射字典，杜绝外键与导航属性追踪冲突
            var fabDict = db.FabricationQuotaRules.ToDictionary(r => r.QuotaCode, r => r.Id);
            var insDict = db.InstallationQuotaRules.ToDictionary(r => r.QuotaCode, r => r.Id);

            // 5. 生成宁波奥体工程基准模型树并预先执行自底向上卷积核算
            var project = NingboOlympicSeedData.CreateBenchmarkProject();

            // 再次确保卷积引擎计算执行完整，将构件与清单项核算底数完全水合写入实体字段
            BoqAggregationEngine.CalculateProject(project);

            // 6. 重置实体主键并精确绑定外键，断开未追踪导航实体
            project.Id = 0;

            foreach (var item in project.TenderItems)
            {
                item.Id = 0;
                item.ProjectId = 0;
                item.Project = null;

                foreach (var sub in item.SubProjects)
                {
                    sub.Id = 0;
                    sub.TenderItemId = 0;
                    sub.TenderItem = null;

                    // 映射车间制作定额外键并断开未追踪导航属性
                    string? fabCode = sub.FabricationQuotaRule?.QuotaCode;
                    if (!string.IsNullOrEmpty(fabCode) && fabDict.TryGetValue(fabCode, out int fabId))
                    {
                        sub.FabricationQuotaRuleId = fabId;
                    }
                    sub.FabricationQuotaRule = null; // 显式断开，防止 EF Core 重复插入字典表

                    // 映射现场安装定额外键并断开未追踪导航属性
                    string? insCode = sub.InstallationQuotaRule?.QuotaCode;
                    if (!string.IsNullOrEmpty(insCode) && insDict.TryGetValue(insCode, out int insId))
                    {
                        sub.InstallationQuotaRuleId = insId;
                    }
                    sub.InstallationQuotaRule = null; // 显式断开

                    foreach (var part in sub.Parts)
                    {
                        part.Id = 0;
                        part.SubProjectId = 0;
                        part.SubProject = null;
                    }
                }
            }

            // 7. 写入数据库大盘
            db.BoqProjects.Add(project);
            db.SaveChanges();
        }

        /// <summary>
        /// 公开兼容方法别名
        /// </summary>
        public static void SeedBoqBenchmarkData(AppDbContext db) => Seed(db);
    }
}
