using mau.Models;

namespace mau.Utils.Services.Interface;

public interface IAppNavigationService
{
    Task NavigateAsync(
        string route,
        bool animated = true,
        IReadOnlyDictionary<string, object>? parameters = null);

    Task GoBackAsync();

    Task OpenKnownBrowserAsync(string key);

    Task OpenBrowserAsync(BrowserRequest request);

    Task<bool> OpenExternalAsync(Uri uri);
}
