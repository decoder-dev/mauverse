using mau.Models;
using mau.Utils.Services;
using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class BrowserSecurityTests
{
    [Theory]
    [InlineData("https://mauniver.ru/")]
    [InlineData("https://eios.mauniver.ru/moodle/")]
    [InlineData("https://deep.subdomain.mauniver.ru/path")]
    public void UniversityHttpsHostsAreInternal(string value)
    {
        var request = new BrowserRequest("Test", new Uri("https://mauniver.ru"));

        Assert.True(request.IsInternalUri(new Uri(value)));
    }

    [Theory]
    [InlineData("http://mauniver.ru/")]
    [InlineData("https://mauniver.ru.example.com/")]
    [InlineData("https://evilmauniver.ru/")]
    [InlineData("https://example.com/")]
    public void UntrustedOrCleartextHostsAreExternal(string value)
    {
        var request = new BrowserRequest("Test", new Uri("https://mauniver.ru"));

        Assert.False(request.IsInternalUri(new Uri(value)));
    }

    [Fact]
    public void ExplicitSubdomainBoundaryDoesNotGrantSiblingHosts()
    {
        var request = new BrowserRequest(
            "Eios",
            new Uri("https://eios.mauniver.ru"),
            ["eios.mauniver.ru"]);

        Assert.True(request.IsInternalUri(new Uri("https://course.eios.mauniver.ru")));
        Assert.False(request.IsInternalUri(new Uri("https://www.mauniver.ru")));
    }

    [Theory]
    [InlineData("https://mauniver.ru/files/request.PDF")]
    [InlineData("https://mauniver.ru/files/report.docx?download=1")]
    [InlineData("https://mauniver.ru/files/archive%2Ezip")]
    public void DownloadLinksAreDetected(string value)
    {
        Assert.True(BrowserDestinationRegistry.IsDownloadUri(new Uri(value)));
    }

    [Fact]
    public void UniversityDestinationsRejectExternalAndCleartextUrls()
    {
        Assert.Throws<ArgumentException>(() => BrowserDestinationRegistry.CreateUniversityForm(
            "External",
            new Uri("https://example.com/form")));
        Assert.Throws<ArgumentException>(() => BrowserDestinationRegistry.CreateUniversityNews(
            "Cleartext",
            new Uri("http://mauniver.ru/news")));
    }

    [Fact]
    public void UnknownRegistryKeyFailsClosed()
    {
        Assert.Throws<KeyNotFoundException>(() =>
            BrowserDestinationRegistry.GetRequired("unknown"));
    }
}
