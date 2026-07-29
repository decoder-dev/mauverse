import SwiftUI
import SafariServices

@MainActor
final class NewsViewModel: ObservableObject {
    @Published var items: [NewsItem] = []
    @Published var filter: NewsFilter = .all
    @Published var isLoading = false
    @Published var error: String?

    func load() async {
        isLoading = true
        error = nil
        defer { isLoading = false }
        do {
            items = try await APIClient.shared.get(
                "news",
                query: [URLQueryItem(name: "news_type", value: "\(filter.rawValue)")]
            )
        } catch {
            self.error = error.localizedDescription
        }
    }
}

struct NewsView: View {
    @StateObject private var model = NewsViewModel()

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    Text("Новости").font(.system(size: 34, weight: .bold, design: .rounded))

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: 9) {
                            ForEach(NewsFilter.allCases) { filter in
                                Button(filter.title) {
                                    model.filter = filter
                                    Task { await model.load() }
                                }
                                .font(.subheadline.weight(.semibold))
                                .foregroundStyle(model.filter == filter ? .white : MauTheme.ink)
                                .padding(.horizontal, 15)
                                .padding(.vertical, 10)
                                .background(model.filter == filter ? AnyShapeStyle(MauTheme.blue.gradient) : AnyShapeStyle(MauTheme.card.opacity(0.78)),
                                            in: Capsule())
                                .buttonStyle(.plain)
                            }
                        }
                    }

                    if model.isLoading {
                        LoadingOverlay(title: "Загружаем новости").frame(maxWidth: .infinity)
                    } else if let error = model.error {
                        EmptyState(icon: "wifi.exclamationmark", title: "Нет соединения", message: error)
                    } else if model.items.isEmpty {
                        EmptyState(icon: "newspaper", title: "Новостей пока нет", message: "Попробуйте выбрать другую категорию")
                    } else {
                        LazyVStack(spacing: 16) {
                            ForEach(model.items) { NewsCard(item: $0) }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 12)
            }
            .refreshable { await model.load() }
        }
        .navigationBarTitleDisplayMode(.inline)
        .task { if model.items.isEmpty { await model.load() } }
    }
}

private struct NewsCard: View {
    let item: NewsItem

    var body: some View {
        Button {
            guard let value = item.link, let url = URL(string: value) else { return }
            UIApplication.shared.open(url)
        } label: {
            VStack(alignment: .leading, spacing: 0) {
                if let source = item.image, let url = URL(string: source) {
                    AsyncImage(url: url) { phase in
                        if case .success(let image) = phase {
                            image.resizable().scaledToFill()
                        } else {
                            LinearGradient(colors: [MauTheme.lavender, .white], startPoint: .topLeading, endPoint: .bottomTrailing)
                                .overlay(Image(systemName: "photo").font(.largeTitle).foregroundStyle(MauTheme.blue))
                        }
                    }
                    .frame(height: 180)
                    .clipped()
                }
                VStack(alignment: .leading, spacing: 9) {
                    if let date = item.publish {
                        Text(date).font(.caption).foregroundStyle(MauTheme.blue)
                    }
                    Text(item.title ?? "Новость")
                        .font(.headline)
                        .multilineTextAlignment(.leading)
                        .foregroundStyle(MauTheme.ink)
                    if let description = item.description {
                        Text(description.strippingHTML)
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(3)
                            .multilineTextAlignment(.leading)
                    }
                }
                .padding(18)
            }
            .background(MauTheme.card.opacity(0.72))
            .clipShape(RoundedRectangle(cornerRadius: 24, style: .continuous))
        }
        .buttonStyle(.plain)
        .mauGlass(radius: 24)
    }
}

private extension String {
    var strippingHTML: String {
        replacingOccurrences(of: "<[^>]+>", with: "", options: .regularExpression)
            .replacingOccurrences(of: "&nbsp;", with: " ")
            .replacingOccurrences(of: "&amp;", with: "&")
    }
}

struct SafariView: UIViewControllerRepresentable {
    let url: URL
    func makeUIViewController(context: Context) -> SFSafariViewController {
        SFSafariViewController(url: url)
    }
    func updateUIViewController(_ uiViewController: SFSafariViewController, context: Context) {}
}
