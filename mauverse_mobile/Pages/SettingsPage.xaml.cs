using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel;

using Microsoft.Extensions.Caching.Memory;

namespace mau;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(
        DbConnect context,
        ICacheService persistentCache,
        IMemoryCache memoryCache,
        IThemeService themeService)
    {
        InitializeComponent();
        BindingContext = new SettingsViewModel(context, persistentCache, memoryCache, themeService);
        Shell.SetTabBarIsVisible(this, false);
    }
}
