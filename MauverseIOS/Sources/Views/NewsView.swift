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
                VStack(alignment: .leading, spacing: MauLayout.sectionStack) {
                    pageTitle("Новости", subtitle: "Главное в жизни университета")

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: MauLayout.gridGutter) {
                            ForEach(NewsFilter.allCases) { filter in
                                Button {
                                    withAnimation(MauMotion.snappy) {
                                        model.filter = filter
                                    }
                                    Task { await model.load() }
                                } label: {
                                    Text(filter.title)
                                        .font(.system(size: 14, weight: .semibold))
                                        .foregroundStyle(model.filter == filter ? .white : MauTheme.ink)
                                        .padding(.horizontal, 15)
                                        .padding(.vertical, 10)
                                        .background {
                                            if model.filter == filter {
                                                Capsule().fill(MauTheme.heroGradient)
                                            }
                                        }
                                        .mauGlass(radius: 22, style: .thin)
                                }
                                .mauPressable()
                                .sensoryFeedback(.selection, trigger: model.filter)
                            }
                        }
                        .mauHorizontalScrollTrailingInset()
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
                        LazyVStack(spacing: MauLayout.gridRow) {
                        if let first = model.items.first {
                            HeroNewsCard(item: first, category: model.filter == .all ? nil : model.filter.title)
                        }
                        ForEach(Array(model.items.dropFirst())) {
                            NewsCard(item: $0, category: model.filter == .all ? nil : model.filter.title)
                        }
                        }
                    }
                }
                .mauTabPageContent()
            }
            .refreshable { await model.load() }
        }
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .navigationBar)
        .task { if model.items.isEmpty { await model.load() } }
    }
}

private struct NewsCard: View {
    let item: NewsItem
    let category: String?

    @ViewBuilder
    var body: some View {
        Group {
            if let value = item.link, let url = URL(string: value) {
                NavigationLink(destination: InAppBrowserView(url: url, title: item.title ?? "Новости МАУ")) {
                    content
                }
                .buttonStyle(.plain)
            } else {
                content
            }
        }
        .mauGlass(radius: MauRadius.card, style: .regular)
    }

    private var content: some View {
        HStack(spacing: 0) {
            if let source = item.image, let url = URL(string: source) {
                AsyncImage(url: url) { phase in
                    if case .success(let image) = phase {
                        image.resizable().scaledToFill()
                            .transition(.opacity)
                    } else {
                        MauTheme.heroGradient
                            .overlay(Image(systemName: "photo").foregroundStyle(.white.opacity(0.7)))
                    }
                }
                .frame(width: 120, height: 138)
                .clipped()
            }
            VStack(alignment: .leading, spacing: 7) {
                if let category {
                    Text(category.uppercased())
                        .font(.system(size: 9, weight: .bold))
                        .tracking(0.7)
                        .foregroundStyle(MauTheme.blue)
                }
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
            .padding(MauLayout.cardPadding)
        }
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.card, style: .continuous))
    }
}

private struct HeroNewsCard: View {
    let item: NewsItem
    let category: String?

    @ViewBuilder
    var body: some View {
        Group {
            if let value = item.link, let url = URL(string: value) {
                NavigationLink(destination: InAppBrowserView(url: url, title: item.title ?? "Новости МАУ")) {
                    content
                }
                .buttonStyle(.plain)
            } else {
                content
            }
        }
        .mauGlass(radius: MauRadius.hero)
        .shadow(color: MauTheme.blue.opacity(0.12), radius: 18, y: 10)
    }

    private var content: some View {
        VStack(alignment: .leading, spacing: 0) {
            if let source = item.image, let url = URL(string: source) {
                AsyncImage(url: url) { phase in
                    if case .success(let image) = phase {
                        image
                            .resizable()
                            .scaledToFill()
                            .transition(.opacity)
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
                    if let category {
                        Text(category.uppercased())
                            .font(.caption2.bold())
                            .tracking(0.8)
                            .foregroundStyle(MauTheme.blue)
                            .lineLimit(1)
                            .layoutPriority(1)
                    }
                    if let date = item.publish {
                        Text(date)
                            .font(.caption2)
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(1)
                            .frame(maxWidth: .infinity, alignment: .trailing)
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
            .padding(MauLayout.cardPadding)
        }
        .frame(maxWidth: .infinity)
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
    }
}

private extension String {
    var cleanedForDisplay: String {
        HTMLTextCleaning.cleanRSS(self)
    }
}
