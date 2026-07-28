using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

using Microsoft.Extensions.Caching.Memory;

namespace mau.ViewModel;

public partial class SettingsViewModel : BaseViewModel
{
    readonly ICacheService _persistentCache;
    readonly IMemoryCache _memoryCache;
    readonly IThemeService _themeService;

    public SettingsViewModel(
        DbConnect context,
        ICacheService persistentCache,
        IMemoryCache memoryCache,
        IThemeService themeService) : base(context)
    {
        _persistentCache = persistentCache;
        _memoryCache = memoryCache;
        _themeService = themeService;
        _selectedThemeIndex = (int)_themeService.CurrentMode;
        UpdateThemeSelection(_selectedThemeIndex);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "The instance member is part of the compiled XAML binding contract.")]
    public string AppNameLabel => "MAUverse";

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "The instance member is part of the compiled XAML binding contract.")]
    public string AppVersionLabel =>
        $"Версия {AppInfo.Current.VersionString}  •  сборка {AppInfo.Current.BuildString}";

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "The instance member is part of the compiled XAML binding contract.")]
    public string AppCreditsLabel => "Сделано УИТ с любовью к студентам";

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "The instance member is part of the compiled XAML binding contract.")]
    public string PlatformLabel =>
        $"{DeviceInfo.Current.Platform} {DeviceInfo.Current.VersionString}";

    [ObservableProperty]
    int _selectedThemeIndex;

    [ObservableProperty]
    bool _isSystemThemeSelected;

    [ObservableProperty]
    bool _isLightThemeSelected;

    [ObservableProperty]
    bool _isDarkThemeSelected;

    [ObservableProperty]
    bool _isCacheBusy;

    [ObservableProperty]
    string _cacheStatusLabel = "Подсчитываем объем...";

    partial void OnSelectedThemeIndexChanged(int value)
    {
        if (value < 0 || value > (int)ThemeMode.Dark || Application.Current is null)
            return;

        UpdateThemeSelection(value);
        _themeService.Apply(Application.Current, (ThemeMode)value);
    }

    [RelayCommand]
    void SelectTheme(string? value)
    {
        if (Enum.TryParse<ThemeMode>(value, out var mode))
            SelectedThemeIndex = (int)mode;
    }

    [RelayCommand]
    async Task LoadSettings(CancellationToken cancellationToken)
    {
        if (IsCacheBusy)
            return;

        IsCacheBusy = true;
        try
        {
            var statistics = await _persistentCache.GetStatisticsAsync(cancellationToken);
            CacheStatusLabel = FormatCacheStatistics(statistics);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            CacheStatusLabel = "Не удалось подсчитать кэш";
        }
        finally
        {
            IsCacheBusy = false;
        }
    }

    [RelayCommand]
    async Task ClearContentCache(CancellationToken cancellationToken)
    {
        if (IsCacheBusy)
            return;

        IsCacheBusy = true;
        try
        {
            await _persistentCache.ClearAsync(cancellationToken);
            if (_memoryCache is MemoryCache memoryCache)
                memoryCache.Compact(1);

            CacheStatusLabel = "Кэш пуст";
            await AppShell.DisplaySnackbarAsync("Временные данные удалены");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось очистить временные данные");
        }
        finally
        {
            IsCacheBusy = false;
        }
    }

    [RelayCommand]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "RelayCommand generation requires the instance command contract used by XAML.")]
    async Task OpenSystemSettings()
    {
        try
        {
            AppInfo.Current.ShowSettingsUI();
        }
        catch (FeatureNotSupportedException)
        {
            await AppShell.DisplaySnackbarAsync("Системные настройки недоступны");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть системные настройки");
        }
    }

    [RelayCommand]
    async Task CopyDiagnostics()
    {
        try
        {
            var display = DeviceDisplay.Current.MainDisplayInfo;
            var diagnostics = string.Join(
                Environment.NewLine,
                $"MAUverse {AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})",
                $"Платформа: {PlatformLabel}",
                $"Устройство: {DeviceInfo.Current.Manufacturer} {DeviceInfo.Current.Model}",
                $"Экран: {display.Width:0}x{display.Height:0}, density {display.Density:0.##}",
                $"Тема: {_themeService.CurrentMode}",
                $"API: {ApiConfiguration.BaseUri.Host}");

            await Clipboard.Default.SetTextAsync(diagnostics);
            await AppShell.DisplaySnackbarAsync("Сведения скопированы");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось скопировать сведения");
        }
    }

    void UpdateThemeSelection(int value)
    {
        IsSystemThemeSelected = value == (int)ThemeMode.System;
        IsLightThemeSelected = value == (int)ThemeMode.Light;
        IsDarkThemeSelected = value == (int)ThemeMode.Dark;
    }

    static string FormatCacheStatistics(CacheStatistics statistics)
    {
        if (statistics.FileCount == 0)
            return "Кэш пуст";

        var size = statistics.SizeBytes switch
        {
            < 1024 => $"{statistics.SizeBytes} Б",
            < 1024 * 1024 => $"{statistics.SizeBytes / 1024d:0.#} КБ",
            _ => $"{statistics.SizeBytes / 1024d / 1024d:0.#} МБ"
        };
        return $"{size}  •  файлов: {statistics.FileCount}";
    }

    protected override void CancelPendingOperations()
    {
        LoadSettingsCommand.Cancel();
        ClearContentCacheCommand.Cancel();
    }
}
