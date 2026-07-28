using System.Reflection;

namespace mau.Utils.Services;

public static class ApiConfiguration
{
    public static Uri BaseUri { get; } = LoadBaseUri();

    static Uri LoadBaseUri()
    {
        var rawValue = typeof(ApiConfiguration).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => attribute.Key == "MauverseApiBaseUrl")
            ?.Value;

        if (!Uri.TryCreate(rawValue, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new InvalidOperationException("MauverseApiBaseUrl must be an absolute HTTPS URL");

        return uri.AbsoluteUri.EndsWith('/')
            ? uri
            : new Uri($"{uri.AbsoluteUri}/");
    }
}
