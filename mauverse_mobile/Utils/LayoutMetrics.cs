namespace mau.Utils;

/// <summary>
/// Shared screen layout constants aligned with iOS MauLayout.
/// Two-column grids use: cardWidth = (contentWidth - gridGutter) / 2.
/// </summary>
public static class LayoutMetrics
{
    public const double PageHorizontal = 28;
    public const double PageTop = 20;
    public const double PageBottomTabClearance = 108;
    public const double GridGutter = 12;
    public const double GridRowSpacing = 12;
    public const double SectionStackSpacing = 22;
    public const double SectionHeaderSpacing = 10;
    public const double ServiceCardPadding = 16;
    public const double ServiceCardMinHeight = 140;
    public const double MaxContentWidth = 760;

    public static Thickness PagePadding =>
        new(PageHorizontal, PageTop, PageHorizontal, PageBottomTabClearance);

    public static Thickness ServiceCardPaddingThickness =>
        new(ServiceCardPadding);

    public static Thickness SectionBlockMargin =>
        new(0, 0, 0, SectionStackSpacing);

    public static Thickness PageHeaderBottomMargin =>
        new(0, 0, 0, SectionHeaderSpacing);
}
