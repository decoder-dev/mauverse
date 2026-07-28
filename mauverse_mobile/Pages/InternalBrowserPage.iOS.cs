#if IOS
using mau.Controls;

namespace mau.Pages;

public partial class InternalBrowserPage
{
    private static partial void PausePlatformWebView(ResilientWebView webView)
    {
        // iOS suspends WKWebView activity with the containing application.
    }

    private static partial void ResumePlatformWebView(ResilientWebView webView)
    {
        // iOS resumes WKWebView activity with the containing application.
    }

    private static partial void ReleasePlatformWebView(ResilientWebView webView)
    {
        webView.Handler?.DisconnectHandler();
    }
}
#endif
