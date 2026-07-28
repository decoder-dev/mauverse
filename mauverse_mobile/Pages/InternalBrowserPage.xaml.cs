using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using mau.Controls;
using mau.Models;
using mau.Resources.Fonts;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

namespace mau.Pages;

public partial class InternalBrowserPage : ContentPage, IQueryAttributable, IDisposable
{
    private const string SameWindowScript = """
        (() => {
            const rewrite = root => {
                if (!root || !root.querySelectorAll) return;
                root.querySelectorAll('a[target="_blank"]').forEach(link => link.target = '_self');
            };

            rewrite(document);
            if (window.__mauverseSameWindowInstalled) return true;
            window.__mauverseSameWindowInstalled = true;

            document.addEventListener('click', event => {
                const element = event.target instanceof Element ? event.target : null;
                const link = element ? element.closest('a[target="_blank"]') : null;
                if (link) link.target = '_self';
            }, true);

            new MutationObserver(mutations => {
                mutations.forEach(mutation => {
                    mutation.addedNodes.forEach(node => {
                        if (node.nodeType === Node.ELEMENT_NODE) {
                            if (node.matches && node.matches('a[target="_blank"]')) node.target = '_self';
                            rewrite(node);
                        }
                    });
                });
            }).observe(document.documentElement, { childList: true, subtree: true });

            return true;
        })();
        """;

    private static readonly HashSet<string> ExternalSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeMailto,
        "geo",
        "intent",
        "tel"
    };

    private readonly IAppNavigationService _navigationService;
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly CancellationToken _disposeToken;
    private BrowserRequest? _request;
    private Uri? _currentUri;
    private Uri? _lastInternalUri;
    private bool _hasLoadedPage;
    private bool _isLoading;
    private bool _wasAttached;
    private bool _disposed;
    private bool _webViewProcessTerminated;
    private TaskCompletionSource? _webBackCompletion;
    private int _backInProgress;
    private int _externalOpenInProgress;

    public InternalBrowserPage(IAppNavigationService navigationService)
    {
        _navigationService = navigationService;
        _disposeToken = _disposeCancellation.Token;
        BackCommand = new Command(ExecuteBackCommand);
        InitializeComponent();
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    public ICommand BackCommand { get; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (_disposed)
            return;

        if (!query.TryGetValue(BrowserRequest.NavigationParameterKey, out var value) ||
            value is not BrowserRequest request)
        {
            ShowErrorState(
                "Ссылка недоступна",
                "Не удалось получить адрес страницы. Вернитесь назад и повторите попытку.",
                canRetry: false);
            return;
        }

        _request = request;
        _currentUri = request.Uri;
        Title = request.Title;
        UpdateDomain(request.Uri);

        if (!request.IsInternalUri(request.Uri) || BrowserDestinationRegistry.IsDownloadUri(request.Uri))
        {
            if (request.ExternalNavigationPolicy == BrowserExternalNavigationPolicy.OpenSystem)
                _ = OpenExternalAndCloseAsync(request.Uri);
            else
                ShowBlockedState();

            return;
        }

        _lastInternalUri = request.Uri;
        _hasLoadedPage = false;
        ErrorState.IsVisible = false;
        BrowserWebView.Source = new UrlWebViewSource { Url = request.Uri.AbsoluteUri };
        UpdateNavigationButtons();
    }

    protected override bool OnBackButtonPressed()
    {
        if (_disposed)
            return base.OnBackButtonPressed();

        ExecuteBackCommand();
        return true;
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is not null)
            _wasAttached = true;
        else if (_wasAttached)
            Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        DetachWebViewEvents(BrowserWebView);
        Interlocked.Exchange(ref _webBackCompletion, null)?.TrySetCanceled(_disposeToken);
        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
        ReleasePlatformWebView(BrowserWebView);
        GC.SuppressFinalize(this);
    }

    private void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        try
        {
            if (_disposed || !Uri.TryCreate(e.Url, UriKind.Absolute, out var uri))
            {
                e.Cancel = true;
                _ = ShowSnackbarSafelyAsync("Небезопасная или некорректная ссылка заблокирована");
                return;
            }

            if (BrowserRequest.IsHttpOrHttps(uri))
            {
                if (BrowserDestinationRegistry.IsDownloadUri(uri) || _request?.IsInternalUri(uri) != true)
                {
                    e.Cancel = true;
                    HandleExternalNavigation(uri);
                    return;
                }

                _currentUri = uri;
                _lastInternalUri = uri;
                UpdateDomain(uri);
                BeginLoading();
                if (!_hasLoadedPage)
                    ErrorState.IsVisible = false;

                return;
            }

            e.Cancel = true;
            if (ExternalSchemes.Contains(uri.Scheme))
                HandleExternalNavigation(uri);
            else
                _ = ShowSnackbarSafelyAsync("Ссылка с неподдерживаемой схемой заблокирована");
        }
        catch (Exception exception)
        {
            e.Cancel = true;
            Debug.WriteLine(exception);
            _ = ShowSnackbarSafelyAsync("Не удалось обработать ссылку");
        }
    }

    private async void OnWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        try
        {
            if (_disposed)
                return;

            EndLoading();
            if (e.Result == WebNavigationResult.Success)
            {
                _hasLoadedPage = true;
                ErrorState.IsVisible = false;
                if (Uri.TryCreate(e.Url, UriKind.Absolute, out var navigatedUri) &&
                    BrowserRequest.IsHttpOrHttps(navigatedUri))
                {
                    _currentUri = navigatedUri;
                    _lastInternalUri = navigatedUri;
                    UpdateDomain(navigatedUri);
                }

                await InjectSameWindowBehaviorAsync(_disposeToken);
            }
            else if (!_hasLoadedPage)
            {
                ShowLoadFailureState();
            }

            UpdateNavigationButtons();
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            EndLoading();
            if (!_hasLoadedPage)
                ShowLoadFailureState();
        }
        finally
        {
            Interlocked.Exchange(ref _webBackCompletion, null)?.TrySetResult();
        }
    }

    private void OnWebViewProcessTerminated(object? sender, EventArgs e)
    {
        if (_disposed)
            return;

        _webViewProcessTerminated = true;
        _hasLoadedPage = false;
        Interlocked.Exchange(ref _webBackCompletion, null)?.TrySetResult();
        ShowErrorState(
            "Браузер был перезапущен",
            "Android освободил ресурсы страницы. Нажмите «Повторить», чтобы продолжить.",
            canRetry: _lastInternalUri is not null);
    }

    private void OnWebBackClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!_webViewProcessTerminated && BrowserWebView.CanGoBack)
                BrowserWebView.GoBack();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            _ = ShowSnackbarSafelyAsync("Не удалось перейти назад");
        }
    }

    private void OnWebForwardClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!_webViewProcessTerminated && BrowserWebView.CanGoForward)
                BrowserWebView.GoForward();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            _ = ShowSnackbarSafelyAsync("Не удалось перейти вперед");
        }
    }

    private void OnReloadClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_disposed || _lastInternalUri is null)
                return;

            ErrorState.IsVisible = false;
            EnsureUsableWebView();
            if (BrowserWebView.Source is null)
                BrowserWebView.Source = new UrlWebViewSource { Url = _lastInternalUri.AbsoluteUri };
            else
                BrowserWebView.Reload();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            _ = ShowSnackbarSafelyAsync("Не удалось обновить страницу");
        }
    }

    private async void OnStopClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_disposed || !_isLoading)
                return;

            _disposeToken.ThrowIfCancellationRequested();
            await BrowserWebView.EvaluateJavaScriptAsync("window.stop(); true;");
            EndLoading();
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            EndLoading();
            await ShowSnackbarSafelyAsync("Не удалось остановить загрузку");
        }
    }

    private void OnRetryClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_disposed || _lastInternalUri is null)
                return;

            ErrorState.IsVisible = false;
            EnsureUsableWebView();
            BrowserWebView.Source = new UrlWebViewSource { Url = _lastInternalUri.AbsoluteUri };
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            _ = ShowSnackbarSafelyAsync("Не удалось повторить загрузку");
        }
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_disposed && ErrorState.IsVisible && !_hasLoadedPage)
                ShowLoadFailureState();
        });
    }

    private async void ExecuteBackCommand()
    {
        if (_disposed || Interlocked.CompareExchange(ref _backInProgress, 1, 0) != 0)
            return;

        try
        {
            if (!_webViewProcessTerminated && BrowserWebView.CanGoBack)
            {
                var completion = new TaskCompletionSource(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                Interlocked.Exchange(ref _webBackCompletion, completion)?.TrySetResult();
                BrowserWebView.GoBack();
                await completion.Task.WaitAsync(TimeSpan.FromSeconds(10), _disposeToken);
                return;
            }

            await _navigationService.GoBackAsync();
        }
        catch (TimeoutException exception)
        {
            Debug.WriteLine(exception);
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            await ShowSnackbarSafelyAsync("Не удалось вернуться назад");
        }
        finally
        {
            Volatile.Write(ref _backInProgress, 0);
        }
    }

    private void HandleExternalNavigation(Uri uri)
    {
        if (_request?.ExternalNavigationPolicy == BrowserExternalNavigationPolicy.Block)
        {
            _ = ShowSnackbarSafelyAsync("Переход за пределы mauniver.ru заблокирован");
            return;
        }

        _ = OpenExternalSafelyAsync(uri);
    }

    private async Task<bool> OpenExternalSafelyAsync(Uri uri)
    {
        if (_disposed || Interlocked.CompareExchange(ref _externalOpenInProgress, 1, 0) != 0)
            return false;

        try
        {
            _disposeToken.ThrowIfCancellationRequested();
            var opened = await _navigationService.OpenExternalAsync(uri);
            if (!opened)
                await ShowSnackbarSafelyAsync("Не найдено приложение для открытия ссылки");

            return opened;
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            await ShowSnackbarSafelyAsync("Не удалось открыть ссылку во внешнем приложении");
            return false;
        }
        finally
        {
            Volatile.Write(ref _externalOpenInProgress, 0);
        }
    }

    private async Task OpenExternalAndCloseAsync(Uri uri)
    {
        try
        {
            if (await OpenExternalSafelyAsync(uri) && !_disposed)
                await _navigationService.GoBackAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            await ShowSnackbarSafelyAsync("Не удалось вернуться назад");
        }
    }

    private async Task InjectSameWindowBehaviorAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await BrowserWebView.EvaluateJavaScriptAsync(SameWindowScript);
        cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task ShowSnackbarSafelyAsync(string message)
    {
        try
        {
            if (_disposed)
                return;

            var snackbar = Snackbar.Make(message, duration: TimeSpan.FromSeconds(4));
            await snackbar.Show(_disposeToken);
        }
        catch (OperationCanceledException) when (_disposeToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private void BeginLoading()
    {
        _isLoading = true;
        LoadingProgress.Progress = 0.25;
        LoadingProgress.IsVisible = true;
        ReloadButton.IsVisible = false;
        StopButton.IsVisible = true;
        UpdateNavigationButtons();
    }

    private void EndLoading()
    {
        _isLoading = false;
        LoadingProgress.Progress = 1;
        LoadingProgress.IsVisible = false;
        StopButton.IsVisible = false;
        ReloadButton.IsVisible = true;
        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        var canUseWebView = !_disposed && !_webViewProcessTerminated;
        WebBackButton.IsEnabled = canUseWebView && BrowserWebView.CanGoBack;
        WebForwardButton.IsEnabled = canUseWebView && BrowserWebView.CanGoForward;
        ReloadButton.IsEnabled = !_isLoading && _lastInternalUri is not null;
    }

    internal void PauseForAppLifecycle()
    {
        if (!_disposed && !_webViewProcessTerminated)
            PausePlatformWebView(BrowserWebView);
    }

    internal void ResumeForAppLifecycle()
    {
        if (!_disposed && !_webViewProcessTerminated)
            ResumePlatformWebView(BrowserWebView);
    }

    private void EnsureUsableWebView()
    {
        if (!_webViewProcessTerminated)
            return;

        var previousWebView = BrowserWebView;
        DetachWebViewEvents(previousWebView);
        BrowserHost.Children.Remove(previousWebView);
        ReleasePlatformWebView(previousWebView);

        var replacement = CreateWebView();
        BrowserHost.Children.Add(replacement);
        BrowserWebView = replacement;
        _webViewProcessTerminated = false;
        UpdateNavigationButtons();
    }

    private ResilientWebView CreateWebView()
    {
        var webView = new ResilientWebView
        {
            AutomationId = "InternalBrowserWebView",
            ZIndex = 0
        };
        webView.Navigating += OnWebViewNavigating;
        webView.Navigated += OnWebViewNavigated;
        webView.ProcessTerminated += OnWebViewProcessTerminated;
        return webView;
    }

    private void DetachWebViewEvents(ResilientWebView webView)
    {
        webView.Navigating -= OnWebViewNavigating;
        webView.Navigated -= OnWebViewNavigated;
        webView.ProcessTerminated -= OnWebViewProcessTerminated;
    }

    private static partial void PausePlatformWebView(ResilientWebView webView);

    private static partial void ResumePlatformWebView(ResilientWebView webView);

    private static partial void ReleasePlatformWebView(ResilientWebView webView);

    private void UpdateDomain(Uri uri)
    {
        var isSecure = string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        DomainIcon.Text = isSecure ? FluentUI.lock_closed_16_regular : FluentUI.shield_error_24_regular;
        DomainIcon.SetDynamicResource(Label.TextColorProperty, isSecure ? "Success" : "Warning");
        SemanticProperties.SetDescription(DomainIcon, isSecure
            ? "Защищенное соединение"
            : "Незащищенное соединение");
        DomainLabel.Text = isSecure
            ? $"Защищенный домен | {uri.IdnHost}"
            : $"Незащищенное соединение | {uri.IdnHost}";
    }

    private void ShowLoadFailureState()
    {
        var isOffline = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
        ErrorIcon.Text = isOffline ? FluentUI.wifi_off_24_regular : FluentUI.shield_error_24_regular;
        ErrorTitle.Text = isOffline ? "Нет подключения к интернету" : "Не удалось открыть страницу";
        ErrorMessage.Text = isOffline
            ? "Подключитесь к сети и повторите попытку."
            : "Сайт временно недоступен. Повторите попытку позднее.";
        RetryButton.IsVisible = _lastInternalUri is not null;
        ErrorState.IsVisible = !_hasLoadedPage;
    }

    private void ShowBlockedState()
    {
        ErrorIcon.Text = FluentUI.shield_error_24_regular;
        ShowErrorState(
            "Переход заблокирован",
            "Этот адрес нельзя открыть внутри приложения.",
            canRetry: false);
    }

    private void ShowErrorState(string title, string message, bool canRetry)
    {
        EndLoading();
        ErrorTitle.Text = title;
        ErrorMessage.Text = message;
        RetryButton.IsVisible = canRetry;
        ErrorState.IsVisible = true;
    }
}
