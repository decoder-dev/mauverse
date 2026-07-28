namespace mau.Utils.Services.Interface;

public enum ThemeMode
{
    System,
    Light,
    Dark
}

public interface IThemeService
{
    ThemeMode CurrentMode { get; }

    void Apply(Application application, ThemeMode mode, bool persist = true);

    void RefreshSystemTheme(Application application, AppTheme requestedTheme);
}
