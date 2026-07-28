using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Content.PM;
using Android.OS;
using Android.Views;

using AndroidX.Core.View;

using Microsoft.Maui.Platform;

using System.ComponentModel;

namespace mau.Platforms.Android;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    LaunchMode = LaunchMode.SingleTop,
    WindowSoftInputMode = SoftInput.AdjustResize,
    ConfigurationChanges = ConfigChanges.ScreenSize |
                           ConfigChanges.Orientation |
                           ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout |
                           ConfigChanges.SmallestScreenSize |
                           ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly SafeAreaInsetsListener _safeAreaInsetsListener = new();
    private global::Android.Views.View? _insetsContentView;
    private bool _isThemeListenerAttached;
    private bool _isDestroyed;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        ApplySystemBarInsets();
        ApplySystemBarAppearance();
        SubscribeToThemeChanges();
    }

    protected override void OnResume()
    {
        base.OnResume();
        (Shell.Current?.CurrentPage as Pages.InternalBrowserPage)?.ResumeForAppLifecycle();
        SubscribeToThemeChanges();
        ApplySystemBarAppearance();
    }

#if DEBUG
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);

        if (intent?.GetBooleanExtra("simulate_webview_crash", false) != true)
            return;

        intent.RemoveExtra("simulate_webview_crash");
        RunOnUiThread(() =>
            (Shell.Current?.CurrentPage as Pages.InternalBrowserPage)?
                .SimulateRendererCrashForTesting());
    }
#endif

    protected override void OnPause()
    {
        (Shell.Current?.CurrentPage as Pages.InternalBrowserPage)?.PauseForAppLifecycle();
        base.OnPause();
    }

    public override void OnConfigurationChanged(Configuration newConfig)
    {
        base.OnConfigurationChanged(newConfig);
        ApplySystemBarInsets();
        ApplySystemBarAppearance();
    }

    protected override void OnDestroy()
    {
        _isDestroyed = true;
        UnsubscribeFromThemeChanges();
        if (_insetsContentView is not null)
        {
            ViewCompat.SetOnApplyWindowInsetsListener(_insetsContentView, null);
            _insetsContentView = null;
        }
        _safeAreaInsetsListener.Dispose();
        base.OnDestroy();
    }

    private void ApplySystemBarInsets()
    {
        if (Window is null)
            return;

        // Android 15 enforces edge-to-edge; Shell handles the top inset while this listener
        // keeps content clear of side/bottom system bars and the IME.
        WindowCompat.SetDecorFitsSystemWindows(Window, false);
        var content = Window.DecorView.FindViewById(global::Android.Resource.Id.Content);
        if (content is null)
            return;

        _insetsContentView = content;
        ViewCompat.SetOnApplyWindowInsetsListener(content, _safeAreaInsetsListener);
        ViewCompat.RequestApplyInsets(content);
    }

    private void SubscribeToThemeChanges()
    {
        if (_isThemeListenerAttached)
        {
            return;
        }

        if (Microsoft.Maui.Controls.Application.Current is not { } application)
        {
            return;
        }

        application.PropertyChanged += OnApplicationPropertyChanged;
        application.RequestedThemeChanged += OnRequestedThemeChanged;
        _isThemeListenerAttached = true;
    }

    private void UnsubscribeFromThemeChanges()
    {
        if (!_isThemeListenerAttached)
        {
            return;
        }

        if (Microsoft.Maui.Controls.Application.Current is not { } application)
        {
            return;
        }

        application.PropertyChanged -= OnApplicationPropertyChanged;
        application.RequestedThemeChanged -= OnRequestedThemeChanged;
        _isThemeListenerAttached = false;
    }

    private void OnApplicationPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Microsoft.Maui.Controls.Application.UserAppTheme))
            QueueSystemBarAppearanceUpdate();
    }

    private void OnRequestedThemeChanged(object? sender, AppThemeChangedEventArgs args) =>
        QueueSystemBarAppearanceUpdate();

    private void QueueSystemBarAppearanceUpdate()
    {
        if (Window?.DecorView is { } decorView)
        {
            decorView.Post(() =>
            {
                if (!_isDestroyed)
                    ApplySystemBarAppearance();
            });
        }
    }

    private void ApplySystemBarAppearance()
    {
        if (_isDestroyed || Window is null)
            return;

        var surface = GetThemeColor("Surface", "#FFFFFF");
        var platformSurface = surface.ToPlatform();
        Window.DecorView.SetBackgroundColor(platformSurface);
        // Android 15 owns transparent system bars, so the decor background supplies their color there.
        if (!OperatingSystem.IsAndroidVersionAtLeast(35))
        {
            Window.SetStatusBarColor(platformSurface);
            Window.SetNavigationBarColor(platformSurface);
        }

        var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
        var useDarkIcons = GetRelativeLuminance(surface) > 0.5;
        if (controller is not null)
        {
            controller.AppearanceLightStatusBars = useDarkIcons;
            controller.AppearanceLightNavigationBars = useDarkIcons;
        }
    }

    static Color GetThemeColor(string key, string fallback)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Resources.TryGetValue(key, out var value) == true &&
            value is Color color)
        {
            return color;
        }

        return Color.FromArgb(fallback);
    }

    static double GetRelativeLuminance(Color color)
    {
        static double Linearize(float component) =>
            component <= 0.04045f
                ? component / 12.92
                : Math.Pow((component + 0.055) / 1.055, 2.4);

        return 0.2126 * Linearize(color.Red) +
               0.7152 * Linearize(color.Green) +
               0.0722 * Linearize(color.Blue);
    }

    sealed class SafeAreaInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
    {
        public WindowInsetsCompat? OnApplyWindowInsets(
            global::Android.Views.View? view,
            WindowInsetsCompat? insets)
        {
            if (view is null || insets is null)
                return insets;

            var systemBars = insets.GetInsets(
                WindowInsetsCompat.Type.SystemBars() |
                WindowInsetsCompat.Type.DisplayCutout());
            if (systemBars is null)
            {
                return insets;
            }

            var bottomInset = systemBars.Bottom;
            if (insets.IsVisible(WindowInsetsCompat.Type.Ime()))
            {
                var ime = insets.GetInsets(WindowInsetsCompat.Type.Ime());
                if (ime is not null)
                {
                    bottomInset = Math.Max(bottomInset, ime.Bottom);
                }
            }

            // Shell's AppBarLayout already consumes the status-bar inset. Applying it again
            // here creates a second, empty bar above every page.
            if (view.PaddingLeft != systemBars.Left ||
                view.PaddingTop != 0 ||
                view.PaddingRight != systemBars.Right ||
                view.PaddingBottom != bottomInset)
            {
                view.SetPadding(systemBars.Left, 0, systemBars.Right, bottomInset);
            }

            return insets;
        }
    }
}
