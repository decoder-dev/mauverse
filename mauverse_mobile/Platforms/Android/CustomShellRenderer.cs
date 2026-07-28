using Android.Content;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Text.Style;
using Android.Widget;

using Google.Android.Material.BottomNavigation;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;

using AndroidColor = Android.Graphics.Color;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace mau.Platforms.Android;

public sealed class CustomShellRenderer : ShellRenderer
{
    public CustomShellRenderer(Context? context)
        : base(context)
    {
    }

    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(
        ShellItem shellItem) =>
        new CustomShellBottomNavViewAppearanceTracker(this, shellItem.CurrentItem);

    protected override IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker() =>
        new CustomShellToolbarAppearanceTracker(this);
}

internal sealed class CustomShellBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    private BottomNavigationView? _configuredBottomView;

    public CustomShellBottomNavViewAppearanceTracker(
        IShellContext shellContext,
        ShellItem shellItem)
        : base(shellContext, shellItem)
    {
    }

    public override void SetAppearance(
        BottomNavigationView bottomView,
        IShellAppearanceElement appearance)
    {
        base.SetAppearance(bottomView, appearance);
        if (ReferenceEquals(_configuredBottomView, bottomView))
        {
            return;
        }

        _configuredBottomView = bottomView;
        bottomView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
        bottomView.ItemHorizontalTranslationEnabled = false;

        var density = bottomView.Resources?.DisplayMetrics?.Density ?? 1;
        bottomView.ItemIconSize = (int)Math.Round(24 * density);

        if (OperatingSystem.IsAndroidVersionAtLeast(28))
        {
            bottomView.SetOutlineAmbientShadowColor(AndroidColor.Transparent);
            bottomView.SetOutlineSpotShadowColor(AndroidColor.Transparent);
        }

        if (bottomView.LayoutParameters is LinearLayout.LayoutParams layoutParameters)
        {
            // A physical-pixel divider avoids the shadow that Material adds above the tab bar.
            layoutParameters.SetMargins(0, 1, 0, 0);
            bottomView.LayoutParameters = layoutParameters;
        }

        var itemBackground = new GradientDrawable();
        itemBackground.SetTintMode(global::Android.Graphics.PorterDuff.Mode.Clear);
        bottomView.ItemBackground = itemBackground;
    }
}

internal sealed class CustomShellToolbarAppearanceTracker : IShellToolbarAppearanceTracker
{
    private readonly Shell _shell;
    private IFontManager? _fontManager;
    private AndroidX.AppCompat.Widget.Toolbar? _configuredToolbar;
    private SpannableStringBuilder? _formattedTitle;
    private string? _formattedTitleText;

    public CustomShellToolbarAppearanceTracker(IShellContext shellContext)
    {
        _shell = shellContext.Shell;
        _fontManager = _shell.Handler?.MauiContext?.Services
            .GetService(typeof(IFontManager)) as IFontManager;
    }

    public void Dispose()
    {
        _formattedTitle?.Dispose();
        _formattedTitle = null;
        _formattedTitleText = null;
        _configuredToolbar = null;
        GC.SuppressFinalize(this);
    }

    public void ResetAppearance(
        AndroidX.AppCompat.Widget.Toolbar toolbar,
        IShellToolbarTracker toolbarTracker)
    {
        _configuredToolbar = null;
    }

    public void SetAppearance(
        AndroidX.AppCompat.Widget.Toolbar toolbar,
        IShellToolbarTracker toolbarTracker,
        ShellAppearance appearance)
    {
        var surfaceColor = GetThemeColor("Surface", "#FFFFFF").ToPlatform();
        var textColor = GetThemeColor("TextPrimary", "#1E1E1E").ToPlatform();
        var actionColor = GetThemeColor("Primary", "#005AE0").ToPlatform();

        toolbar.SetBackgroundColor(surfaceColor);
        toolbar.SetTitleTextColor(textColor);
        toolbar.NavigationIcon?.SetTint(actionColor);
        toolbar.OverflowIcon?.SetTint(actionColor);

        if (!ReferenceEquals(_configuredToolbar, toolbar))
        {
            _configuredToolbar = toolbar;
            toolbar.Elevation = 0;

            if (toolbar.LayoutParameters is LinearLayout.LayoutParams layoutParameters)
            {
                layoutParameters.SetMargins(0, 0, 0, 1);
                toolbar.LayoutParameters = layoutParameters;
            }
        }

        ApplyFormattedTitle(toolbar);
    }

    private void ApplyFormattedTitle(AndroidX.AppCompat.Widget.Toolbar toolbar)
    {
        var title = toolbar.TitleFormatted?.ToString() ?? string.Empty;
        if (title.Length == 0)
        {
            return;
        }

        if (!string.Equals(_formattedTitleText, title, StringComparison.Ordinal))
        {
            var nextTitle = new SpannableStringBuilder(title);
            var titleSpan = CreateTitleSpan();
            nextTitle.SetSpan(
                titleSpan,
                0,
                nextTitle.Length(),
                SpanTypes.ExclusiveExclusive);

            var previousTitle = _formattedTitle;
            _formattedTitle = nextTitle;
            _formattedTitleText = title;
            toolbar.TitleFormatted = nextTitle;
            previousTitle?.Dispose();
            return;
        }

        toolbar.TitleFormatted = _formattedTitle;
    }

    private TypefaceSpan CreateTitleSpan()
    {
        // TypefaceSpan accepts a bundled MAUI typeface only on Android 9 and newer.
        _fontManager ??= _shell.Handler?.MauiContext?.Services
            .GetService(typeof(IFontManager)) as IFontManager;

        if (OperatingSystem.IsAndroidVersionAtLeast(28) && _fontManager is not null)
        {
            var typeface = _fontManager.GetTypeface(Microsoft.Maui.Font.OfSize("MontserratBold", 20));
            if (typeface is not null)
            {
                return new TypefaceSpan(typeface);
            }
        }

        return new TypefaceSpan("sans-serif-medium");
    }

    private static MauiColor GetThemeColor(string key, string fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true &&
            value is MauiColor color)
        {
            return color;
        }

        return MauiColor.FromArgb(fallback);
    }
}
