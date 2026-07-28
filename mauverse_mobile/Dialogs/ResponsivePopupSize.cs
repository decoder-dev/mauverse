namespace mau.Dialogs;

internal static class ResponsivePopupSize
{
    const double HorizontalMargin = 32;
    const double VerticalMargin = 96;

    public static Size Fit(double desiredWidth, double desiredHeight)
    {
        var display = DeviceDisplay.Current.MainDisplayInfo;
        var density = Math.Max(display.Density, 1);
        var availableWidth = Math.Max(1, display.Width / density - HorizontalMargin);
        var availableHeight = Math.Max(1, display.Height / density - VerticalMargin);

        return new Size(
            Math.Min(desiredWidth, availableWidth),
            Math.Min(desiredHeight, availableHeight));
    }

    public static void Apply(
        CommunityToolkit.Maui.Views.Popup popup,
        double desiredWidth,
        double desiredHeight)
    {
        var size = Fit(desiredWidth, desiredHeight);
        popup.WidthRequest = size.Width;
        popup.HeightRequest = size.Height;
    }
}
