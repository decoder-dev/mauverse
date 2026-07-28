using mau.Utils.Services;

using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class ThemePaletteTests
{
    [Fact]
    public void OfficialBrandColorsArePreservedExactly()
    {
        Assert.Equal("#008CFA", ThemePalette.BrandBlue);
        Assert.Equal("#0064BE", ThemePalette.BrandBlueClassic);
        Assert.Equal("#005AE0", ThemePalette.BrandBlueDark);
        Assert.Equal("#F6FAFE", ThemePalette.BrandBluePale);
        Assert.Equal("#F9423A", ThemePalette.BrandRed);
        Assert.Equal("#D31107", ThemePalette.BrandRedDark);
        Assert.Equal("#FCF0F0", ThemePalette.BrandRedPale);
        Assert.Equal("#14898F", ThemePalette.BrandTeal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SemanticTextAndActionColorsMeetNormalTextContrast(bool darkTheme)
    {
        var palette = darkTheme ? ThemePalette.Dark : ThemePalette.Light;

        AssertContrast(palette, "TextPrimary", "Surface");
        AssertContrast(palette, "TextSecondary", "Surface");
        AssertContrast(palette, "TextMuted", "Surface");
        AssertContrast(palette, "Primary", "Surface");
        AssertContrast(palette, "PrimaryAccentText", "PrimaryAccent");
        AssertContrast(palette, "Primary", "PrimaryLight");
        AssertContrast(palette, "Success", "SuccessLight");
        AssertContrast(palette, "Warning", "WarningLight");
        AssertContrast(palette, "Error", "ErrorLight");
        AssertContrast(palette, "PrimaryPanelMuted", "PrimaryPanelBackground");
    }

    private static void AssertContrast(
        IReadOnlyDictionary<string, string> palette,
        string foregroundKey,
        string backgroundKey)
    {
        var foreground = palette[foregroundKey];
        var background = palette[backgroundKey];
        var ratio = GetContrastRatio(foreground, background);

        Assert.True(
            ratio >= 4.5,
            $"{foregroundKey} {foreground} on {backgroundKey} {background} has only {ratio:F2}:1 contrast.");
    }

    private static double GetContrastRatio(string foreground, string background)
    {
        var foregroundLuminance = GetRelativeLuminance(foreground);
        var backgroundLuminance = GetRelativeLuminance(background);
        var lighter = Math.Max(foregroundLuminance, backgroundLuminance);
        var darker = Math.Min(foregroundLuminance, backgroundLuminance);
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double GetRelativeLuminance(string hexColor)
    {
        var red = ParseChannel(hexColor, 1);
        var green = ParseChannel(hexColor, 3);
        var blue = ParseChannel(hexColor, 5);
        return (0.2126 * red) + (0.7152 * green) + (0.0722 * blue);
    }

    private static double ParseChannel(string hexColor, int startIndex)
    {
        var channel = Convert.ToInt32(hexColor.Substring(startIndex, 2), 16) / 255d;
        return channel <= 0.04045
            ? channel / 12.92
            : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }
}
