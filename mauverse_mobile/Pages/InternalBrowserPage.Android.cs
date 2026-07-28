#if ANDROID
using mau.Controls;

namespace mau.Pages;

public partial class InternalBrowserPage
{
    private static partial void PausePlatformWebView(ResilientWebView webView)
    {
        if (webView.Handler?.PlatformView is Android.Webkit.WebView platformWebView)
            platformWebView.OnPause();
    }

    private static partial void ResumePlatformWebView(ResilientWebView webView)
    {
        if (webView.Handler?.PlatformView is Android.Webkit.WebView platformWebView)
            platformWebView.OnResume();
    }

    private static partial void ReleasePlatformWebView(ResilientWebView webView)
    {
        webView.Handler?.DisconnectHandler();
    }

#if DEBUG
    // Invoked only by the device regression suite; this code is absent from Release builds.
    internal void SimulateRendererCrashForTesting()
    {
        if (!_disposed &&
            BrowserWebView.Handler?.PlatformView is Android.Webkit.WebView platformWebView)
        {
            platformWebView.LoadUrl("chrome://crash");
        }
    }
#endif
}
#endif
