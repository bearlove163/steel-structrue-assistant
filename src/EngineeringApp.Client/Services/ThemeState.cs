namespace EngineeringApp.Client.Services;

/// <summary>
/// 全局亮色/暗黑主题状态管理器
/// </summary>
public class ThemeState
{
    /// <summary>当前是否处于暗黑主题模式 (默认 true)</summary>
    public bool IsDarkMode { get; set; } = true;

    /// <summary>主题切换通知事件</summary>
    public event Action? OnChange;

    /// <summary>切换主题</summary>
    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
    }

    /// <summary>设置指定主题</summary>
    public void SetTheme(bool isDark)
    {
        if (IsDarkMode != isDark)
        {
            IsDarkMode = isDark;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
