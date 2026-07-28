using Android.Webkit;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

using AWebView = Android.Webkit.WebView;

namespace mau.Platforms.Android;

public sealed class ResilientWebViewHandler : WebViewHandler
{
    private static readonly IPropertyMapper<IWebView, ResilientWebViewHandler> ResilientMapper =
        new PropertyMapper<IWebView, ResilientWebViewHandler>(Mapper)
        {
            [nameof(WebViewClient)] = MapResilientWebViewClient
        };

    private bool _renderProcessTerminated;

    public ResilientWebViewHandler()
        : base(ResilientMapper, CommandMapper)
    {
    }

    internal void MarkRenderProcessTerminated() => _renderProcessTerminated = true;

    protected override void DisconnectHandler(AWebView platformView)
    {
        if (!_renderProcessTerminated)
        {
            try
            {
                platformView.OnPause();
            }
            catch (Java.Lang.Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        base.DisconnectHandler(platformView);
        platformView.RemoveAllViews();
        platformView.Destroy();
    }

    private static void MapResilientWebViewClient(
        ResilientWebViewHandler handler,
        IWebView webView) =>
        handler.PlatformView.SetWebViewClient(new ResilientMauiWebViewClient(handler));
}

internal sealed class ResilientMauiWebViewClient : MauiWebViewClient
{
    private readonly WeakReference<ResilientWebViewHandler> _handler;

    public ResilientMauiWebViewClient(ResilientWebViewHandler handler)
        : base(handler)
    {
        _handler = new WeakReference<ResilientWebViewHandler>(handler);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("android26.0")]
    public override bool OnRenderProcessGone(
        AWebView? view,
        RenderProcessGoneDetail? detail)
    {
        if (_handler.TryGetTarget(out var handler))
            handler.MarkRenderProcessTerminated();

        _ = base.OnRenderProcessGone(view, detail);

        // Android terminates the host app when this callback returns false.
        return true;
    }
}
