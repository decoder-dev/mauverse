using mau.Utils.Services.Interface;

namespace mau;

public partial class App : Application
{
    private readonly IThemeService _themeService;

    public App(IThemeService themeService)
    {
        InitializeComponent();
        _themeService = themeService;
        _themeService.Apply(this, _themeService.CurrentMode, persist: false);
        RequestedThemeChanged += OnRequestedThemeChanged;
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs args) =>
        _themeService.RefreshSystemTheme(this, args.RequestedTheme);

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
