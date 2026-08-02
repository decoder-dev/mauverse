using mau.Models;
using mau.Utils.Services;
using Xunit;

namespace Mauverse.Mobile.Tests;

public sealed class UniversityPortalTests
{
    [Fact]
    public void StudentPortalUrlsStayOnUniversityHosts()
    {
        Assert.Contains(
            UniversityPortalUrls.StudentUrls,
            url => url.Contains("/structure/divs/studof/", StringComparison.Ordinal));
        Assert.Contains(
            UniversityPortalUrls.StudentUrls,
            url => url.Contains("/student/faq/", StringComparison.Ordinal));
        Assert.Contains(
            UniversityPortalUrls.StudentUrls,
            url => url.Contains("lib.mauniver.ru", StringComparison.Ordinal));

        Assert.All(UniversityPortalUrls.StudentUrls, AssertUniversityHttps);
    }

    [Fact]
    public void ApplicantPortalUrlsIncludeAdmissionPortal()
    {
        Assert.Contains(
            UniversityPortalUrls.ApplicantUrls,
            url => url.StartsWith("https://priem.mauniver.ru", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            UniversityPortalUrls.ApplicantUrls,
            url => url.Contains("/structure/branches/", StringComparison.Ordinal));

        Assert.All(UniversityPortalUrls.ApplicantUrls, AssertUniversityHttps);
    }

    [Theory]
    [InlineData(BrowserDestinationRegistry.LibraryKey, "lib.mauniver.ru")]
    [InlineData(BrowserDestinationRegistry.EventsCalendarKey, "mauniver.ru")]
    [InlineData(BrowserDestinationRegistry.PrivacyPolicyKey, "mauniver.ru")]
    [InlineData(BrowserDestinationRegistry.SvedenKey, "mauniver.ru")]
    [InlineData(BrowserDestinationRegistry.CampusNavigatorSiteKey, "mauniver.ru")]
    [InlineData(BrowserDestinationRegistry.OfficialSiteKey, "mauniver.ru")]
    public void KnownPortalDestinationsResolveToUniversityHosts(string key, string hostFragment)
    {
        var request = BrowserDestinationRegistry.GetRequired(key);

        Assert.Contains(hostFragment, request.Uri.IdnHost, StringComparison.OrdinalIgnoreCase);
        Assert.True(request.IsInternalUri(request.Uri));
    }

    [Fact]
    public void CampusBuildingSupportsBranchMapCities()
    {
        var branch = new CampusBuilding(
            "Филиал в г. Апатиты",
            "ул. Лесная, 29",
            "МАУ Филиал в г. Апатиты, ул. Лесная, 29, Апатиты",
            "apatity");

        Assert.Equal("apatity", branch.MapCity);
        Assert.Contains("Апатиты", branch.SearchQuery, StringComparison.Ordinal);
    }

    static void AssertUniversityHttps(string url)
    {
        Assert.True(Uri.TryCreate(url, UriKind.Absolute, out var uri));
        Assert.Equal(Uri.UriSchemeHttps, uri.Scheme);
        Assert.True(BrowserRequest.IsUniversityHost(uri.IdnHost));
        Assert.True(BrowserDestinationRegistry.IsUniversityUri(uri));
    }
}
