using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;

namespace mau.Platforms.iOS;

/// <summary>
/// Keeps Shell on the native translucent UIKit appearance. On iOS 26 UIKit
/// presents these standard bars as Liquid Glass automatically.
/// </summary>
public sealed class LiquidGlassShellRenderer : ShellRenderer
{
    protected override IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker() =>
        new LiquidGlassTabBarAppearanceTracker();

    protected override IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker() =>
        new LiquidGlassNavBarAppearanceTracker();
}

sealed class LiquidGlassTabBarAppearanceTracker : IShellTabBarAppearanceTracker
{
    public void SetAppearance(UITabBarController controller, ShellAppearance appearance)
    {
        var tabBar = controller.TabBar;
        var nativeAppearance = new UITabBarAppearance();
        nativeAppearance.ConfigureWithDefaultBackground();
        nativeAppearance.ShadowColor = UIColor.Clear;

        tabBar.StandardAppearance = nativeAppearance;
        tabBar.ScrollEdgeAppearance = nativeAppearance;
        tabBar.Translucent = true;
        tabBar.TintColor = appearance.ForegroundColor?.ToPlatform();
        tabBar.UnselectedItemTintColor = appearance.UnselectedColor?.ToPlatform();
    }

    public void ResetAppearance(UITabBarController controller) =>
        SetDefaultAppearance(controller.TabBar);

    public void UpdateLayout(UITabBarController controller)
    {
    }

    public void Dispose()
    {
    }

    static void SetDefaultAppearance(UITabBar tabBar)
    {
        var appearance = new UITabBarAppearance();
        appearance.ConfigureWithDefaultBackground();
        appearance.ShadowColor = UIColor.Clear;
        tabBar.StandardAppearance = appearance;
        tabBar.ScrollEdgeAppearance = appearance;
        tabBar.Translucent = true;
    }
}

sealed class LiquidGlassNavBarAppearanceTracker : IShellNavBarAppearanceTracker
{
    public void SetAppearance(UINavigationController controller, ShellAppearance appearance)
    {
        var navBar = controller.NavigationBar;
        var nativeAppearance = new UINavigationBarAppearance();
        nativeAppearance.ConfigureWithDefaultBackground();
        nativeAppearance.ShadowColor = UIColor.Clear;

        if (appearance.TitleColor is not null)
        {
            nativeAppearance.TitleTextAttributes =
                new UIStringAttributes { ForegroundColor = appearance.TitleColor.ToPlatform() };
            nativeAppearance.LargeTitleTextAttributes =
                new UIStringAttributes { ForegroundColor = appearance.TitleColor.ToPlatform() };
        }

        navBar.StandardAppearance = nativeAppearance;
        navBar.CompactAppearance = nativeAppearance;
        navBar.ScrollEdgeAppearance = nativeAppearance;
        navBar.Translucent = true;
        navBar.TintColor = appearance.ForegroundColor?.ToPlatform();
    }

    public void ResetAppearance(UINavigationController controller)
    {
        var appearance = new UINavigationBarAppearance();
        appearance.ConfigureWithDefaultBackground();
        appearance.ShadowColor = UIColor.Clear;
        controller.NavigationBar.StandardAppearance = appearance;
        controller.NavigationBar.CompactAppearance = appearance;
        controller.NavigationBar.ScrollEdgeAppearance = appearance;
        controller.NavigationBar.Translucent = true;
    }

    public void SetHasShadow(UINavigationController controller, bool hasShadow)
    {
        // Liquid Glass uses its material boundary instead of a legacy shadow.
    }

    public void UpdateLayout(UINavigationController controller)
    {
    }

    public void Dispose()
    {
    }
}
