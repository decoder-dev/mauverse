using System.Text.RegularExpressions;

namespace mau.Utils;

/// <summary>
/// Formats university phone numbers for the platform dialer.
/// Local Murmansk PBX numbers like "21-38-81 (3045)" become "+78152213881;ext=3045".
/// </summary>
public static partial class PhoneNumberFormatting
{
    public const string MurmanskAreaCode = "8152";

    public static string? ToDialString(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var working = raw.Trim();
        string? extension = null;

        var paren = TrailingParenExtension().Match(working);
        if (paren.Success)
        {
            extension = paren.Groups[1].Value;
            working = working[..paren.Index].TrimEnd();
        }
        else
        {
            var labeled = TrailingLabeledExtension().Match(working);
            if (labeled.Success)
            {
                extension = labeled.Groups[1].Value;
                working = working[..labeled.Index].TrimEnd();
            }
        }

        var digits = new string(working.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
            return null;

        string? e164 = digits.Length switch
        {
            6 => $"+7{MurmanskAreaCode}{digits}",
            10 when digits.StartsWith(MurmanskAreaCode, StringComparison.Ordinal)
                || digits.StartsWith("800", StringComparison.Ordinal)
                => $"+7{digits}",
            11 when digits.StartsWith('8') => $"+7{digits[1..]}",
            11 when digits.StartsWith('7') => $"+{digits}",
            >= 10 when digits.StartsWith('7') => $"+{digits}",
            >= 10 => $"+7{digits}",
            _ => null
        };

        if (e164 is null)
            return null;

        return string.IsNullOrEmpty(extension) ? e164 : $"{e164};ext={extension}";
    }

    [GeneratedRegex(@"\((\d{2,6})\)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex TrailingParenExtension();

    [GeneratedRegex(@"(?i)(?:доб\.?|ext\.?|extension)\s*(\d{2,6})\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex TrailingLabeledExtension();
}
