namespace EngineeringApp.Shared.Pricing.Common;

using EngineeringApp.Shared.Pricing.Coating;

/// <summary>
/// 全局供应链品牌库与多品类询报价提供者标准接口
/// 为后续独立规划的“企业级供应链多材料品牌矩阵模块”预留标准数据接入底座。
/// 支持品类划分：Coating (涂料防腐), SteelPlate (钢板板材), SteelProfile (热轧型钢/钢管), Fastener (高强螺栓/焊材)
/// </summary>
public interface IBrandPricingProvider
{
    /// <summary>材料品类领域代码 (如 "Coating", "SteelPlate", "SteelProfile")</summary>
    string DomainCategory { get; }

    /// <summary>获取指定历史快照或时期的所有品牌实体询价记录</summary>
    IEnumerable<BrandQuoteItem> GetBrandQuotes(string snapshotId);

    /// <summary>直接向指定历史快照登记新品牌及询价记录</summary>
    void RegisterNewBrandQuote(string snapshotId, BrandQuoteItem quote);
}
