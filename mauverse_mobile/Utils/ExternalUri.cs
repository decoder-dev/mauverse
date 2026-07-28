namespace mau.Utils;

public static class ExternalUri
{
    public static bool TryCreateHttp(string? value, out Uri uri)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var candidate) &&
            (candidate.Scheme == Uri.UriSchemeHttps || candidate.Scheme == Uri.UriSchemeHttp))
        {
            uri = candidate;
            return true;
        }

        uri = null!;
        return false;
    }
}
