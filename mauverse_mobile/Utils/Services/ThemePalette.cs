namespace mau.Utils.Services;

internal static class ThemePalette
{
    internal const string BrandWhite = "#FFFFFF";
    internal const string BrandBlue = "#008CFA";
    internal const string BrandBlueClassic = "#0064BE";
    internal const string BrandBlueDark = "#005AE0";
    internal const string BrandBluePale = "#F6FAFE";
    internal const string BrandRed = "#F9423A";
    internal const string BrandRedDark = "#D31107";
    internal const string BrandRedPale = "#FCF0F0";
    internal const string BrandGray = "#F1F5F9";
    internal const string BrandGrayDark = "#E2E8F0";
    internal const string BrandBlack = "#1E1E1E";
    internal const string BrandTeal = "#14898F";

    internal static IReadOnlyDictionary<string, string> Light { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Background"] = BrandBluePale,
            ["Surface"] = BrandWhite,
            ["SurfaceMuted"] = BrandGray,
            ["BorderSubtle"] = BrandGrayDark,
            ["TextPrimary"] = BrandBlack,
            ["TextSecondary"] = "#475569",
            ["TextMuted"] = "#64748B",
            ["Primary"] = BrandBlueDark,
            ["PrimaryAccent"] = BrandBlue,
            ["PrimaryAccentText"] = BrandBlack,
            ["PrimaryDark"] = BrandBlueDark,
            ["PrimaryLight"] = BrandBluePale,
            ["Success"] = "#0E7378",
            ["SuccessDarkForeground"] = "#2AA7AE",
            ["SuccessLight"] = "#E8F7F7",
            ["Info"] = BrandBlueDark,
            ["Warning"] = "#9A4D00",
            ["WarningDarkForeground"] = "#FFB45C",
            ["WarningLight"] = "#FFF4E5",
            ["Error"] = BrandRedDark,
            ["ErrorDarkForeground"] = BrandRed,
            ["ErrorLight"] = BrandRedPale,
            ["Disable"] = BrandGrayDark,
            ["PrimaryPanelBackground"] = BrandBlueDark,
            ["PrimaryPanelMuted"] = "#D8EAFF"
        };

    internal static IReadOnlyDictionary<string, string> Dark { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Background"] = "#111111",
            ["Surface"] = BrandBlack,
            ["SurfaceMuted"] = "#292929",
            ["BorderSubtle"] = "#3F3F3F",
            ["TextPrimary"] = BrandWhite,
            ["TextSecondary"] = "#CBD5E1",
            ["TextMuted"] = "#94A3B8",
            ["Primary"] = BrandBlue,
            ["PrimaryAccent"] = BrandBlue,
            ["PrimaryAccentText"] = BrandBlack,
            ["PrimaryDark"] = BrandBlueDark,
            ["PrimaryLight"] = "#07233D",
            ["Success"] = "#2AA7AE",
            ["SuccessDarkForeground"] = "#2AA7AE",
            ["SuccessLight"] = "#123536",
            ["Info"] = BrandBlue,
            ["Warning"] = "#FFB45C",
            ["WarningDarkForeground"] = "#FFB45C",
            ["WarningLight"] = "#3A2712",
            ["Error"] = BrandRed,
            ["ErrorDarkForeground"] = BrandRed,
            ["ErrorLight"] = "#351312",
            ["Disable"] = "#3F3F3F",
            ["PrimaryPanelBackground"] = BrandBlueDark,
            ["PrimaryPanelMuted"] = "#D8EAFF"
        };
}
