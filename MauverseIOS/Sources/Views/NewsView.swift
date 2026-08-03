import SwiftUI

@MainActor
final class NewsViewModel: ObservableObject {
    @Published var items: [NewsItem] = []
    @Published var filter: NewsFilter = .all
    @Published var isLoading = false
    @Published var error: String?
    private var activeRequest = UUID()
    private var loadedFilter: NewsFilter?

    func load() async {
        let request = UUID()
        activeRequest = request
        let requestedFilter = filter
        if loadedFilter != requestedFilter {
            items = []
        }
        isLoading = items.isEmpty
        error = nil
        do {
            let loaded = try await OfficialNewsService.shared.load(filter: requestedFilter)
            guard activeRequest == request, filter == requestedFilter else { return }
            items = loaded
            loadedFilter = requestedFilter
        } catch {
            guard activeRequest == request else { return }
            if (error as? URLError)?.code != .cancelled, items.isEmpty {
                self.error = Self.friendlyMessage(for: error)
            }
        }
        if activeRequest == request { isLoading = false }
    }

    private static func friendlyMessage(for error: Error) -> String {
        if let urlError = error as? URLError {
            return switch urlError.code {
            case .notConnectedToInternet: "Проверьте подключение к интернету"
            case .timedOut: "Сайт МАУ отвечает слишком долго. Попробуйте ещё раз"
            default: "Не удалось обновить ленту. Потяните экран ещё раз"
            }
        }
        return error.localizedDescription
    }
}

struct NewsView: View {
    @StateObject private var model = NewsViewModel()

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Новости")
                            .font(.system(size: 36, weight: .bold, design: .rounded))
                        Text("Главное в жизни университета")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: 9) {
                            ForEach(NewsFilter.allCases) { filter in
                                Button(filter.title) {
                                    model.filter = filter
                                    Task { await model.load() }
                                }
                                .font(.system(size: 14, weight: .semibold))
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
                        VStack(spacing: 16) {
                            SkeletonCard()
                            SkeletonCard()
                        }
                    } else if let error = model.error {
                        VStack(spacing: 12) {
                            EmptyState(icon: "wifi.exclamationmark", title: "Новости недоступны", message: error)
                            Button("Повторить") { Task { await model.load() } }
                                .buttonStyle(.borderedProminent)
                        }
                    } else if model.items.isEmpty {
                        EmptyState(icon: "newspaper", title: "Новостей пока нет", message: "Попробуйте выбрать другую категорию")
                    } else {
                        LazyVStack(spacing: 14) {
                            if let first = model.items.first {
                                HeroNewsCard(item: first, category: model.filter.title)
                            }
                            ForEach(Array(model.items.dropFirst())) {
                                NewsCard(item: $0, category: model.filter.title)
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 96)
            }
            .refreshable { await model.load() }
        }
        .navigationBarTitleDisplayMode(.inline)
        .task { if model.items.isEmpty { await model.load() } }
    }
}

private struct NewsCard: View {
    let item: NewsItem
    let category: String

    @ViewBuilder
    var body: some View {
        if let value = item.link, let url = URL(string: value) {
            NavigationLink(destination: InAppBrowserView(url: url, title: item.title ?? "Новости МАУ")) {
                content
            }
            .buttonStyle(.plain)
            .mauGlass(radius: 24)
        } else {
            content.mauGlass(radius: 24)
        }
    }

    private var content: some View {
        HStack(spacing: 0) {
            if let source = item.image, let url = URL(string: source) {
                AsyncImage(url: url) { phase in
                    if case .success(let image) = phase {
                        image.resizable().scaledToFill()
                    } else {
                        MauTheme.heroGradient
                            .overlay(Image(systemName: "photo").foregroundStyle(.white.opacity(0.7)))
                    }
                }
                .frame(width: 120, height: 138)
                .clipped()
            }
            VStack(alignment: .leading, spacing: 7) {
                Text(category.uppercased())
                    .font(.system(size: 9, weight: .bold))
                    .tracking(0.7)
                    .foregroundStyle(MauTheme.blue)
                Text(item.title ?? "Новость")
                    .font(.system(size: 15, weight: .semibold))
                    .multilineTextAlignment(.leading)
                    .foregroundStyle(MauTheme.ink)
                    .lineLimit(3)
                    .minimumScaleFactor(0.9)
                Spacer(minLength: 0)
                if let date = item.publish {
                    Text(date)
                        .font(.caption2)
                        .foregroundStyle(MauTheme.muted)
                }
            }
            .frame(maxWidth: .infinity, minHeight: 106, alignment: .topLeading)
            .padding(16)
        }
        .background(MauTheme.card.opacity(0.74))
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.card, style: .continuous))
    }
}

private struct HeroNewsCard: View {
    let item: NewsItem
    let category: String

    @ViewBuilder
    var body: some View {
        if let value = item.link, let url = URL(string: value) {
            NavigationLink(destination: InAppBrowserView(url: url, title: item.title ?? "Новости МАУ")) {
                content
            }
            .buttonStyle(.plain)
        } else {
            content
        }
    }

    private var content: some View {
        VStack(alignment: .leading, spacing: 0) {
            if let source = item.image, let url = URL(string: source) {
                AsyncImage(url: url) { phase in
                    if case .success(let image) = phase {
                        image
                            .resizable()
                            .scaledToFill()
                    } else {
                        MauTheme.heroGradient
                            .overlay {
                                Image(systemName: "newspaper.fill")
                                    .font(.system(size: 38))
                                    .foregroundStyle(.white.opacity(0.72))
                            }
                    }
                }
                .frame(maxWidth: .infinity)
                .frame(height: 205)
                .clipped()
            } else {
                MauTheme.heroGradient
                    .frame(height: 205)
            }

            VStack(alignment: .leading, spacing: 9) {
                HStack {
                    Text(category.uppercased())
                        .font(.caption2.bold())
                        .tracking(0.8)
                        .foregroundStyle(MauTheme.blue)
                    Spacer()
                    if let date = item.publish {
                        Text(date)
                            .font(.caption2)
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(1)
                    }
                }
                Text(item.title ?? "Новости университета")
                    .font(.system(size: 19, weight: .bold, design: .rounded))
                    .foregroundStyle(MauTheme.ink)
                    .multilineTextAlignment(.leading)
                    .lineLimit(4)
                    .fixedSize(horizontal: false, vertical: true)
                if let description = item.description {
                    Text(description.cleanedForDisplay)
                        .font(.system(size: 13, weight: .regular))
                        .foregroundStyle(MauTheme.muted)
                        .lineLimit(2)
                        .multilineTextAlignment(.leading)
                }
            }
            .padding(20)
        }
        .frame(maxWidth: .infinity)
        .background(MauTheme.card.opacity(0.82))
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
        .overlay {
            RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous)
                .stroke(Color.primary.opacity(0.07), lineWidth: 0.75)
        }
        .shadow(color: .black.opacity(0.12), radius: 18, y: 10)
    }
}

private extension String {
    var cleanedForDisplay: String {
        HTMLTextCleaning.cleanRSS(self)
    }
}
