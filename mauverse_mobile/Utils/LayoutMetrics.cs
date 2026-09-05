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
    public const double CompactTilePadding = 10;
    public const double MaxContentWidth = 760;

    /// <summary>Full page padding for ScrollView content (includes tab clearance).</summary>
    public static Thickness PagePadding =>
        new(PageHorizontal, PageTop, PageHorizontal, PageBottomTabClearance);

    /// <summary>
    /// Side/top padding for root Grids that host a scrolling CollectionView.
    /// Bottom clearance must live on the CollectionView content, not the Grid —
    /// otherwise Android permanently shrinks the list viewport by 108pt.
    /// </summary>
    public static Thickness PagePaddingNoBottom =>
        new(PageHorizontal, PageTop, PageHorizontal, 0);

    public static Thickness ScrollBottomClearance =>
        new(0, 0, 0, PageBottomTabClearance);

    public static Thickness ServiceCardPaddingThickness =>
        new(ServiceCardPadding);

    public static Thickness CompactTilePaddingThickness =>
        new(CompactTilePadding);

    public static Thickness SectionBlockMargin =>
        new(0, 0, 0, SectionStackSpacing);

    public static Thickness PageHeaderBottomMargin =>
        new(0, 0, 0, SectionHeaderSpacing);

    public static Thickness CardContentPadding =>
        new(ServiceCardPadding);

    /// <summary>Kept for binary compat; prefer ItemSpacing on horizontal CollectionViews.</summary>
    public static Thickness NotificationItemMargin =>
        new(0);

    public static Thickness ListItemBottomMargin =>
        new(0, 0, 0, GridGutter);
}
