using mau.Utils.Services.Interface;

namespace mau.Utils.Services;

public sealed class ThemeService : IThemeService
{
    private const string PreferenceKey = "appearance.theme";

    public ThemeMode CurrentMode { get; private set; } = LoadThemeMode();

    public void Apply(Application application, ThemeMode mode, bool persist = true)
    {
        CurrentMode = mode;
        if (persist)
            Preferences.Default.Set(PreferenceKey, mode.ToString());

        application.UserAppTheme = mode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => AppTheme.Unspecified
        };

        var appliedTheme = mode switch
        {
            ThemeMode.Light => AppTheme.Light,
            ThemeMode.Dark => AppTheme.Dark,
            _ => application.RequestedTheme
        };

        ApplyPalette(application.Resources, appliedTheme);
    }

    public void RefreshSystemTheme(Application application, AppTheme requestedTheme)
    {
        if (CurrentMode == ThemeMode.System)
            ApplyPalette(application.Resources, requestedTheme);
    }

    static ThemeMode LoadThemeMode()
    {
        var storedValue = Preferences.Default.Get(PreferenceKey, ThemeMode.System.ToString());
        return Enum.TryParse<ThemeMode>(storedValue, out var mode) ? mode : ThemeMode.System;
    }

    static void ApplyPalette(ResourceDictionary resources, AppTheme theme)
    {
        var palette = theme == AppTheme.Dark ? ThemePalette.Dark : ThemePalette.Light;
        foreach (var (key, color) in palette)
            Set(resources, key, color);
    }

    static void Set(ResourceDictionary resources, string key, string hexColor) =>
        resources[key] = Color.FromArgb(hexColor);
}
