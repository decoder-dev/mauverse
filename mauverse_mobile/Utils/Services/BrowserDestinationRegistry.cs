using mau.Models;

namespace mau.Utils.Services;

public static class BrowserDestinationRegistry
{
    public const string InternalBrowserRoute = "browser/internal";
    public const string EiosKey = "eios";
    public const string MauDigitalServicesKey = "mau-digital-services";

    private static readonly Dictionary<string, BrowserRequest> KnownDestinations =
        new Dictionary<string, BrowserRequest>(StringComparer.OrdinalIgnoreCase)
        {
            [EiosKey] = new(
                "ЭИОС",
                new Uri("https://eios.mauniver.ru/moodle/", UriKind.Absolute)),
            [MauDigitalServicesKey] = new(
                "Цифровые сервисы МАУ",
                new Uri("https://mauniver.ru/services/", UriKind.Absolute))
        };

    private static readonly HashSet<string> DownloadExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".7z",
        ".apk",
        ".csv",
        ".doc",
        ".docx",
        ".epub",
        ".gz",
        ".odf",
        ".ods",
        ".odt",
        ".pdf",
        ".ppt",
        ".pptx",
        ".rar",
        ".rtf",
        ".tar",
        ".xls",
        ".xlsx",
        ".zip"
    };

    public static BrowserRequest GetRequired(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A browser destination key is required.", nameof(key));

        return KnownDestinations.TryGetValue(key.Trim(), out var request)
            ? request
            : throw new KeyNotFoundException($"Unknown browser destination '{key}'.");
    }

    public static bool TryGet(string key, out BrowserRequest? request)
    {
        request = null;
        return !string.IsNullOrWhiteSpace(key) && KnownDestinations.TryGetValue(key.Trim(), out request);
    }

    public static BrowserRequest CreateUniversityForm(string title, Uri uri) =>
        CreateUniversityRequest(title, uri);

    public static BrowserRequest CreateUniversityNews(string title, Uri uri) =>
        CreateUniversityRequest(title, uri);

    public static BrowserRequest CreateUniversityNotification(string title, Uri uri) =>
        CreateUniversityRequest(title, uri);

    public static bool IsUniversityUri(Uri? uri) =>
        BrowserRequest.IsHttpOrHttps(uri) && BrowserRequest.IsUniversityHost(uri!.IdnHost);

    public static bool IsDownloadUri(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!BrowserRequest.IsHttpOrHttps(uri))
            return false;

        var extension = Path.GetExtension(Uri.UnescapeDataString(uri.AbsolutePath));
        return !string.IsNullOrEmpty(extension) && DownloadExtensions.Contains(extension);
    }

    private static BrowserRequest CreateUniversityRequest(string title, Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !BrowserRequest.IsUniversityHost(uri.IdnHost))
        {
            throw new ArgumentException(
                "University browser destinations must use HTTPS on mauniver.ru.",
                nameof(uri));
        }

        return new BrowserRequest(title, uri);
    }
}
