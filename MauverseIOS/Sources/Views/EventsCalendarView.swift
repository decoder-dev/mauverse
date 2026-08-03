import SwiftUI

@MainActor
final class EventsCalendarViewModel: ObservableObject {
    @Published var items: [NewsItem] = []
    @Published var isLoading = false
    @Published var error: String?

    func load() async {
        isLoading = items.isEmpty
        error = nil
        do {
            items = try await OfficialNewsService.shared.load(filter: .calendar)
        } catch {
            if items.isEmpty {
                self.error = error.localizedDescription
            }
        }
        isLoading = false
    }
}

struct EventsCalendarView: View {
    @StateObject private var model = EventsCalendarViewModel()

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 18) {
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Календарь событий")
                            .font(.system(size: 30, weight: .bold, design: .rounded))
                        Text("Анонсы пресс-центра МАУ")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    if let url = URL(string: UniversityPortalURLs.eventsCalendar) {
                        NavigationLink {
                            InAppBrowserView(url: url, title: "Календарь на сайте")
                        } label: {
                            HStack {
                                Label("Открыть на mauniver.ru", systemImage: "safari.fill")
                                    .font(.subheadline.weight(.semibold))
                                Spacer()
                                Image(systemName: "arrow.up.right")
                                    .font(.caption.bold())
                            }
                            .foregroundStyle(MauTheme.blue)
                            .padding(16)
                            .mauSurface(radius: 18)
                        }
                        .buttonStyle(.plain)
                    }

                    if model.isLoading {
                        SkeletonCard()
                        SkeletonCard()
                    } else if let error = model.error {
                        VStack(spacing: 12) {
                            EmptyState(
                                icon: "calendar.badge.exclamationmark",
                                title: "Календарь недоступен",
                                message: error
                            )
                            Button("Повторить") { Task { await model.load() } }
                                .buttonStyle(.borderedProminent)
                        }
                    } else if model.items.isEmpty {
                        EmptyState(
                            icon: "calendar",
                            title: "Событий пока нет",
                            message: "Загляните позже или откройте календарь на сайте"
                        )
                    } else {
                        LazyVStack(spacing: 12) {
                            ForEach(model.items) { item in
                                eventRow(item)
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 30)
            }
            .refreshable { await model.load() }
        }
        .navigationTitle("Календарь")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
        .task { if model.items.isEmpty { await model.load() } }
    }

    @ViewBuilder
    private func eventRow(_ item: NewsItem) -> some View {
        if let link = item.link, let url = URL(string: link) {
            NavigationLink {
                InAppBrowserView(url: url, title: item.title ?? "Событие")
            } label: {
                VStack(alignment: .leading, spacing: 8) {
                    Text(item.title ?? "Событие")
                        .font(.subheadline.weight(.semibold))
                        .foregroundStyle(MauTheme.ink)
                        .multilineTextAlignment(.leading)
                    if let publish = item.publish, !publish.isEmpty {
                        Text(publish)
                            .font(.caption)
                            .foregroundStyle(MauTheme.blue)
                    }
                    if let description = item.description, !description.isEmpty {
                        Text(HTMLTextCleaning.cleanRSS(description))
                            .font(.caption)
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(3)
                            .multilineTextAlignment(.leading)
                    }
                }
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(16)
                .mauGlass(radius: MauRadius.compact)
            }
            .buttonStyle(.plain)
        }
    }
}
