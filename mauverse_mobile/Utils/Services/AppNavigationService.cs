using mau.Models;
using mau.Utils.Services.Interface;

namespace mau.Utils.Services;

public sealed class AppNavigationService : IAppNavigationService, IDisposable
{
    private readonly SemaphoreSlim _shellTransitionGate = new(1, 1);
    private int _externalLaunchInProgress;

    public Task NavigateAsync(
        string route,
        bool animated = true,
        IReadOnlyDictionary<string, object>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(route))
            throw new ArgumentException("A Shell route is required.", nameof(route));

        return TryRunShellTransitionAsync(async () =>
        {
            var shell = Shell.Current ?? throw new InvalidOperationException("Shell is not available.");
            if (parameters is null || parameters.Count == 0)
            {
                await shell.GoToAsync(route, animated);
                return;
            }

            var navigationParameters = new ShellNavigationQueryParameters();
            foreach (var parameter in parameters)
                navigationParameters[parameter.Key] = parameter.Value;

            await shell.GoToAsync(route, animated, navigationParameters);
        });
    }

    public Task GoBackAsync() => RunRequiredShellTransitionAsync(async () =>
    {
        var shell = Shell.Current ?? throw new InvalidOperationException("Shell is not available.");
        await shell.GoToAsync("..", true);
    });

    public Task OpenKnownBrowserAsync(string key) =>
        OpenBrowserAsync(BrowserDestinationRegistry.GetRequired(key));

    public Task OpenBrowserAsync(BrowserRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.IsInternalUri(request.Uri) || BrowserDestinationRegistry.IsDownloadUri(request.Uri))
            return OpenExternalAsync(request.Uri);

        IReadOnlyDictionary<string, object> parameters = new Dictionary<string, object>
        {
            [BrowserRequest.NavigationParameterKey] = request
        };

        return NavigateAsync(BrowserDestinationRegistry.InternalBrowserRoute, animated: true, parameters);
    }

    public async Task<bool> OpenExternalAsync(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri || !IsExternallySupportedScheme(uri.Scheme))
            throw new ArgumentException("The URI scheme cannot be opened externally.", nameof(uri));

        var opened = false;
        if (Interlocked.CompareExchange(ref _externalLaunchInProgress, 1, 0) != 0)
            return false;

        try
        {
            async Task<bool> OpenAsync() => BrowserRequest.IsHttpOrHttps(uri)
                // Custom Tabs keep Back navigation attached to the MAUverse task on single-task browsers.
                ? await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred)
                : await Launcher.Default.OpenAsync(uri);

            opened = MainThread.IsMainThread
                ? await OpenAsync()
                : await MainThread.InvokeOnMainThreadAsync(OpenAsync);
        }
        finally
        {
            Volatile.Write(ref _externalLaunchInProgress, 0);
        }

        return opened;
    }

    private async Task TryRunShellTransitionAsync(Func<Task> transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        // Forward taps are stale once another Shell animation has started.
        if (!await _shellTransitionGate.WaitAsync(0))
            return;

        await RunTransitionCoreAsync(transition);
    }

    private async Task RunRequiredShellTransitionAsync(Func<Task> transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        await _shellTransitionGate.WaitAsync();
        await RunTransitionCoreAsync(transition);
    }

    private async Task RunTransitionCoreAsync(Func<Task> transition)
    {
        try
        {
            if (MainThread.IsMainThread)
                await transition();
            else
                await MainThread.InvokeOnMainThreadAsync(transition);
        }
        finally
        {
            _shellTransitionGate.Release();
        }
    }

    private static bool IsExternallySupportedScheme(string scheme) =>
        string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(scheme, Uri.UriSchemeMailto, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(scheme, "tel", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(scheme, "geo", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(scheme, "intent", StringComparison.OrdinalIgnoreCase);

    public void Dispose()
    {
        _shellTransitionGate.Dispose();
        GC.SuppressFinalize(this);
    }
}
