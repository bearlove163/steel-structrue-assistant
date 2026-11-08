global using EngineeringApp.Shared.Material;
global using EngineeringApp.Shared.Quota;
global using EngineeringApp.Shared.Pricing;
global using EngineeringApp.Shared.CostEstimation;
global using EngineeringApp.Shared.QuotationExport;

// BOQ 领域模型与引擎全局引用
global using EngineeringApp.Shared.BOQ.Models;
global using EngineeringApp.Shared.BOQ.Engine;
global using EngineeringApp.Shared.BOQ.Export;

// BOQ 6 大实体全局类型别名兼容 (Entity Naming Compatibility)
global using BoqProjectEntity = EngineeringApp.Shared.BOQ.Models.BoqProject;
global using BoqTenderItemEntity = EngineeringApp.Shared.BOQ.Models.BoqTenderItem;
global using BoqSubProjectEntity = EngineeringApp.Shared.BOQ.Models.BoqSubProject;
global using BoqPartEntity = EngineeringApp.Shared.BOQ.Models.BoqPart;
global using FabricationQuotaRuleEntity = EngineeringApp.Shared.BOQ.Models.FabricationQuotaRule;
global using InstallationQuotaRuleEntity = EngineeringApp.Shared.BOQ.Models.InstallationQuotaRule;
