using System.Collections.ObjectModel;

namespace mau.Models;

public enum BrowserExternalNavigationPolicy
{
    OpenSystem = 0,
    Block = 1
}

public sealed class BrowserRequest
{
    public const string NavigationParameterKey = "BrowserRequest";
    public const string UniversityRootHost = "mauniver.ru";

    private static readonly IReadOnlyList<string> DefaultAllowedHosts =
        Array.AsReadOnly([UniversityRootHost]);

    public BrowserRequest(
        string title,
        Uri uri,
        IEnumerable<string>? allowedUniversityHosts = null,
        BrowserExternalNavigationPolicy externalNavigationPolicy = BrowserExternalNavigationPolicy.OpenSystem)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("A browser title is required.", nameof(title));

        ArgumentNullException.ThrowIfNull(uri);
        if (!IsHttpOrHttps(uri))
            throw new ArgumentException("The browser URI must be an absolute HTTP or HTTPS URI.", nameof(uri));

        Title = title.Trim();
        Uri = uri;
        AllowedUniversityHosts = NormalizeAllowedHosts(allowedUniversityHosts);
        ExternalNavigationPolicy = externalNavigationPolicy;
    }

    public string Title { get; }

    public Uri Uri { get; }

    public IReadOnlyList<string> AllowedUniversityHosts { get; }

    public BrowserExternalNavigationPolicy ExternalNavigationPolicy { get; }

    public bool IsInternalUri(Uri uri)
    {
        if (!uri.IsAbsoluteUri ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            return false;

        var candidateHost = uri.IdnHost;
        return AllowedUniversityHosts.Any(allowedHost => IsHostWithinBoundary(candidateHost, allowedHost));
    }

    public static bool IsHttpOrHttps(Uri? uri) =>
        uri is { IsAbsoluteUri: true } &&
        (string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));

    public static bool IsUniversityHost(string? host) =>
        !string.IsNullOrWhiteSpace(host) && IsHostWithinBoundary(host, UniversityRootHost);

    public static bool IsHostWithinBoundary(string host, string boundary)
    {
        if (string.Equals(host, boundary, StringComparison.OrdinalIgnoreCase))
            return true;

        return host.EndsWith('.' + boundary, StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> NormalizeAllowedHosts(IEnumerable<string>? hosts)
    {
        if (hosts is null)
            return DefaultAllowedHosts;

        var normalizedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var host in hosts)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Allowed hosts cannot contain empty values.", nameof(hosts));

            var normalized = host.Trim().TrimEnd('.');
            if (Uri.CheckHostName(normalized) != UriHostNameType.Dns || !IsUniversityHost(normalized))
            {
                throw new ArgumentException(
                    $"Allowed host '{host}' must be mauniver.ru or one of its subdomains.",
                    nameof(hosts));
            }

            normalizedHosts.Add(normalized.ToLowerInvariant());
        }

        if (normalizedHosts.Count == 0)
            throw new ArgumentException("At least one allowed university host is required.", nameof(hosts));

        return new ReadOnlyCollection<string>(normalizedHosts.Order(StringComparer.Ordinal).ToArray());
    }
}
