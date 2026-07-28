using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class AndroidTlsPolicyTests
{
    [Theory]
    [InlineData("isrg_root_x1.pem", "96BCEC06264976F37460779ACF28C5A7CFE8A3C0AAE11A8FFCEE05C0BDDF08C6")]
    [InlineData("lets_encrypt_r13.pem", "D3B128216A843F8EF1321501F5DF52A5DF52939EE2C19297712CD3DE4D419354")]
    public void BundledTrustAnchorsMatchApprovedCertificates(string fileName, string fingerprint)
    {
        using var certificate = LoadCertificate(fileName);

        Assert.Equal(fingerprint, certificate.GetCertHashString(HashAlgorithmName.SHA256));
    }

    [Fact]
    public void R13CompatibilityAnchorHasRotationBudget()
    {
        using var certificate = LoadCertificate("lets_encrypt_r13.pem");

        Assert.True(
            certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow.AddDays(90),
            "Rotate or remove the R13 compatibility anchor at least 90 days before expiry.");
    }

    [Fact]
    public void ApiTrustPolicyIsHttpsOnlyAndDomainScoped()
    {
        var document = XDocument.Load(GetTlsPath("network_security_config.xml"));
        var domainConfig = Assert.Single(document.Descendants("domain-config"));
        var domain = Assert.Single(domainConfig.Elements("domain"));
        var trustSources = domainConfig
            .Descendants("certificates")
            .Select(element => (string?)element.Attribute("src"))
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal("false", (string?)domainConfig.Attribute("cleartextTrafficPermitted"));
        Assert.Equal("app.mauniver.ru", domain.Value);
        Assert.Equal("false", (string?)domain.Attribute("includeSubdomains"));
        Assert.Equal(
            new HashSet<string?> { "system", "@raw/isrg_root_x1", "@raw/lets_encrypt_r13" },
            trustSources);
    }

    private static X509Certificate2 LoadCertificate(string fileName) =>
        X509Certificate2.CreateFromPem(File.ReadAllText(GetTlsPath(fileName)));

    private static string GetTlsPath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Tls", fileName);
}
