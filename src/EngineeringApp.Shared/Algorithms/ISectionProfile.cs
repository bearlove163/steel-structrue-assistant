using EngineeringApp.Shared.Models;

namespace EngineeringApp.Shared.Algorithms;

/// <summary>
/// 截面算子插件接口（策略模式）
/// 允许未来无限扩充新截面类型（如冷弯C/Z型钢、双拼角钢、闭口压型板等），
/// 商务软件只要更新类库或在启动时注册新算子，即可无缝同步支持，无需改动现有系统核心架构。
/// </summary>
public interface ISectionProfile
{
    /// <summary>截面类型代号关键字 (如 "Rectangle", "HBeam", "ColdFormedC")</summary>
    string TypeKey { get; }

    /// <summary>截面中文显示名称 (如 "冷弯卷边C型钢 (檩条)", "双拼槽钢")，方便商务软件直接绑定下拉列表</summary>
    string DisplayName { get; }

    /// <summary>所属截面大类 (如 "经典实心", "闭口管材", "热轧型钢", "冷弯薄壁", "组合拼合")</summary>
    string Category { get; }

    /// <summary>对应的标准 SectionType 枚举值（若为扩展非内置截面，可对应 Custom 或扩展值）</summary>
    SectionType? SectionType { get; }

    /// <summary>
    /// 截面几何、力学、外表面积、展开宽度及米重综合计算
    /// </summary>
    SectionPropertiesResult Calculate(SectionParameters parameters);
}
