import Combine
import SwiftUI
import WebKit

final class InAppBrowserController: NSObject, ObservableObject, WKNavigationDelegate, WKUIDelegate {
    @Published private(set) var title: String
    @Published private(set) var currentURL: URL?
    @Published private(set) var progress = 0.0
    @Published private(set) var isLoading = false
    @Published private(set) var canGoBack = false
    @Published private(set) var canGoForward = false
    @Published private(set) var errorMessage: String?

    let webView: WKWebView
    private let initialURL: URL
    private let fallbackTitle: String
    private var observations: [NSKeyValueObservation] = []

    init(url: URL, title: String) {
        initialURL = url
        fallbackTitle = title
        self.title = title

        let configuration = WKWebViewConfiguration()
        configuration.websiteDataStore = .default()
        configuration.applicationNameForUserAgent = "MAUverse/1.12.5"
        configuration.defaultWebpagePreferences.allowsContentJavaScript = true
        configuration.allowsInlineMediaPlayback = true
        webView = WKWebView(frame: .zero, configuration: configuration)

        super.init()

        webView.navigationDelegate = self
        webView.uiDelegate = self
        webView.allowsBackForwardNavigationGestures = true
        webView.allowsLinkPreview = true
        webView.isOpaque = false
        webView.backgroundColor = .clear
        webView.scrollView.backgroundColor = .clear

        observations = [
            webView.observe(\.estimatedProgress, options: [.new]) { [weak self] webView, _ in
                DispatchQueue.main.async { self?.progress = webView.estimatedProgress }
            },
            webView.observe(\.title, options: [.new]) { [weak self] webView, _ in
                DispatchQueue.main.async {
                    self?.title = webView.title?.trimmingCharacters(in: .whitespacesAndNewlines).nilIfEmpty
                        ?? self?.fallbackTitle
                        ?? "Браузер"
                }
            },
            webView.observe(\.url, options: [.new]) { [weak self] webView, _ in
                DispatchQueue.main.async { self?.currentURL = webView.url }
            },
            webView.observe(\.canGoBack, options: [.new]) { [weak self] webView, _ in
                DispatchQueue.main.async { self?.canGoBack = webView.canGoBack }
            },
            webView.observe(\.canGoForward, options: [.new]) { [weak self] webView, _ in
                DispatchQueue.main.async { self?.canGoForward = webView.canGoForward }
            }
        ]
        loadInitialPage()
    }

    func loadInitialPage() {
        errorMessage = nil
        webView.load(URLRequest(url: initialURL, cachePolicy: .useProtocolCachePolicy, timeoutInterval: 45))
    }

    func goBack() {
        guard webView.canGoBack else { return }
        webView.goBack()
    }

    func goForward() {
        guard webView.canGoForward else { return }
        webView.goForward()
    }

    func reloadOrStop() {
        if webView.isLoading {
            webView.stopLoading()
        } else if webView.url == nil {
            loadInitialPage()
        } else {
            errorMessage = nil
            webView.reload()
        }
    }

    func webView(_ webView: WKWebView, didStartProvisionalNavigation navigation: WKNavigation!) {
        isLoading = true
        errorMessage = nil
    }

    func webView(_ webView: WKWebView, didFinish navigation: WKNavigation!) {
        isLoading = false
        progress = 1
        updateNavigationState(webView)
    }

    func webView(
        _ webView: WKWebView,
        didFail navigation: WKNavigation!,
        withError error: Error
    ) {
        handle(error, webView: webView)
    }

    func webView(
        _ webView: WKWebView,
        didFailProvisionalNavigation navigation: WKNavigation!,
        withError error: Error
    ) {
        handle(error, webView: webView)
    }

    func webView(
        _ webView: WKWebView,
        decidePolicyFor navigationAction: WKNavigationAction,
        decisionHandler: @escaping (WKNavigationActionPolicy) -> Void
    ) {
        guard let url = navigationAction.request.url else {
            decisionHandler(.cancel)
            return
        }
        let scheme = url.scheme?.lowercased()
        if scheme == "http" || scheme == "https" || scheme == "about" {
            decisionHandler(.allow)
        } else {
            if UIApplication.shared.canOpenURL(url) {
                UIApplication.shared.open(url)
            }
            decisionHandler(.cancel)
        }
    }

    func webView(
        _ webView: WKWebView,
        createWebViewWith configuration: WKWebViewConfiguration,
        for navigationAction: WKNavigationAction,
        windowFeatures: WKWindowFeatures
    ) -> WKWebView? {
        if navigationAction.targetFrame == nil, let requestURL = navigationAction.request.url {
            webView.load(URLRequest(url: requestURL))
        }
        return nil
    }

    private func handle(_ error: Error, webView: WKWebView) {
        if (error as? URLError)?.code == .cancelled { return }
        isLoading = false
        errorMessage = (error as? URLError)?.code == .notConnectedToInternet
            ? "Нет подключения к интернету"
            : "Не удалось открыть страницу"
        updateNavigationState(webView)
    }

    private func updateNavigationState(_ webView: WKWebView) {
        currentURL = webView.url
        canGoBack = webView.canGoBack
        canGoForward = webView.canGoForward
    }
}

struct InAppBrowserView: View {
    @StateObject private var browser: InAppBrowserController

    init(url: URL, title: String) {
        _browser = StateObject(wrappedValue: InAppBrowserController(url: url, title: title))
    }

    var body: some View {
        ZStack {
            MauBackground()
            BrowserWebView(webView: browser.webView)

            if let error = browser.errorMessage {
                VStack(spacing: 16) {
                    EmptyState(
                        icon: "wifi.exclamationmark",
                        title: "Страница недоступна",
                        message: error
                    )
                    Button("Попробовать снова") { browser.reloadOrStop() }
                        .buttonStyle(.borderedProminent)
                }
                .padding(24)
            }
        }
        .safeAreaInset(edge: .top, spacing: 0) {
            BrowserLocationBar(browser: browser)
        }
        .safeAreaInset(edge: .bottom, spacing: 0) {
            BrowserToolbar(browser: browser)
        }
        .navigationTitle(browser.title)
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
    }
}

private struct BrowserWebView: UIViewRepresentable {
    let webView: WKWebView

    func makeUIView(context: Context) -> WKWebView { webView }
    func updateUIView(_ uiView: WKWebView, context: Context) {}
}

private struct BrowserLocationBar: View {
    @ObservedObject var browser: InAppBrowserController

    var body: some View {
        VStack(spacing: 0) {
            HStack(spacing: 8) {
                Image(systemName: browser.currentURL?.scheme == "https" ? "lock.fill" : "globe")
                    .font(.caption2)
                    .foregroundStyle(browser.currentURL?.scheme == "https" ? .green : MauTheme.muted)
                Text(browser.currentURL?.host ?? "Загрузка…")
                    .font(.caption.weight(.semibold))
                    .foregroundStyle(MauTheme.muted)
                    .lineLimit(1)
                Spacer()
            }
            .padding(.horizontal, 16)
            .padding(.vertical, 9)
            .background(.ultraThinMaterial)

            if browser.isLoading {
                ProgressView(value: browser.progress)
                    .progressViewStyle(.linear)
                    .tint(MauTheme.blue)
            }
        }
    }
}

private struct BrowserToolbar: View {
    @ObservedObject var browser: InAppBrowserController

    var body: some View {
        HStack {
            toolbarButton("chevron.left", enabled: browser.canGoBack, action: browser.goBack)
            Spacer()
            toolbarButton("chevron.right", enabled: browser.canGoForward, action: browser.goForward)
            Spacer()
            toolbarButton(browser.isLoading ? "xmark" : "arrow.clockwise", action: browser.reloadOrStop)
            Spacer()
            if let url = browser.currentURL {
                ShareLink(item: url) {
                    Image(systemName: "square.and.arrow.up")
                        .frame(width: 44, height: 44)
                }
                .foregroundStyle(MauTheme.ink)
            } else {
                toolbarButton("square.and.arrow.up", enabled: false, action: {})
            }
            Spacer()
            Menu {
                if let url = browser.currentURL {
                    Button {
                        UIPasteboard.general.url = url
                    } label: {
                        Label("Скопировать ссылку", systemImage: "doc.on.doc")
                    }
                }
                Button {
                    browser.loadInitialPage()
                } label: {
                    Label("На первую страницу", systemImage: "house")
                }
            } label: {
                Image(systemName: "ellipsis")
                    .frame(width: 44, height: 44)
            }
            .foregroundStyle(MauTheme.ink)
        }
        .font(.system(size: 18, weight: .semibold))
        .padding(.horizontal, 18)
        .padding(.vertical, 7)
        .mauGlass(radius: 28)
        .padding(.horizontal, 12)
        .padding(.bottom, 8)
    }

    private func toolbarButton(
        _ systemName: String,
        enabled: Bool = true,
        action: @escaping () -> Void
    ) -> some View {
        Button(action: action) {
            Image(systemName: systemName)
                .frame(width: 44, height: 44)
        }
        .disabled(!enabled)
        .foregroundStyle(enabled ? MauTheme.ink : MauTheme.muted.opacity(0.4))
    }
}

private extension String {
    var nilIfEmpty: String? { isEmpty ? nil : self }
}
