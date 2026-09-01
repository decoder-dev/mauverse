import SwiftUI

@MainActor
final class HomeViewModel: ObservableObject {
    @Published var nextLesson: ScheduleItem?
    @Published var featuredNews: NewsItem?
    @Published var notifications: [MoodleNotification] = []
    @Published var isLoading = false
    @Published var lessonError: String?
    @Published var newsError: String?
    private var activeRequest = UUID()

    func load(user: UserDTO?) async {
        let request = UUID()
        activeRequest = request
        isLoading = true
        lessonError = nil
        newsError = nil
        defer {
            if activeRequest == request { isLoading = false }
        }

        let newsTask = Task { try await OfficialNewsService.shared.load(filter: .all) }
        let notificationTask = Task { () throws -> [MoodleNotification] in
            guard let token = user?.token, !token.isEmpty, let userId = user?.userId else { return [] }
            return try await APIClient.shared.post(
                "get_notifications",
                body: NotificationRequest(token: token, userId: userId),
                user: user,
                retryOnTransient: true
            )
        }
        if let user {
            do {
                var uid = user.scheduleGroupUID
                if uid?.isEmpty != false, let name = user.groupName, !name.isEmpty {
                    uid = try await ScheduleAPIClient.shared.findGroup(named: name)?.uid
                }
                if let uid, !uid.isEmpty {
                    let formatter = DateFormatter()
                    formatter.locale = Locale(identifier: "en_US_POSIX")
                    formatter.dateFormat = "yyyy-MM-dd"
                    let start = Date()
                    let end = Calendar.current.date(byAdding: .day, value: 14, to: start) ?? start
                    let lessons = try await ScheduleAPIClient.shared.schedule(
                    uid: uid,
                    start: formatter.string(from: start),
                    end: formatter.string(from: end)
                    )
                    guard activeRequest == request else { return }
                    nextLesson = lessons
                        .filter { Self.lessonDate($0) >= Date().addingTimeInterval(-60 * 15) }
                        .sorted(by: Self.isEarlier)
                        .first
                }
            } catch {
                guard activeRequest == request else { return }
                lessonError = error.localizedDescription
            }
        }
        do {
            let loadedNews = try await newsTask.value.first
            guard activeRequest == request else { return }
            featuredNews = loadedNews
        } catch {
            guard activeRequest == request else { return }
            newsError = error.localizedDescription
        }
        if let loaded = try? await notificationTask.value, activeRequest == request {
            notifications = Array(loaded.prefix(8))
        }
    }

    private static func isEarlier(_ lhs: ScheduleItem, _ rhs: ScheduleItem) -> Bool {
        lessonDate(lhs) < lessonDate(rhs)
    }

    private static func lessonDate(_ item: ScheduleItem) -> Date {
        let formatter = DateFormatter()
        formatter.locale = Locale(identifier: "en_US_POSIX")
        formatter.dateFormat = "yyyy-MM-dd HH:mm"
        return formatter.date(from: "\(item.date ?? "") \(item.startTime ?? "00:00")") ?? .distantFuture
    }
}

struct HomeView: View {
    @EnvironmentObject private var session: SessionStore
    @Binding var selectedTab: Int
    @StateObject private var model = HomeViewModel()

    private var firstName: String {
        session.user?.greetingName ?? "Студент"
    }

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: MauLayout.sectionStack) {
                    header

                    if session.user?.groupName?.isEmpty != false {
                        missingGroupCard
                    } else if model.isLoading {
                        lessonSkeleton
                    } else if let error = model.lessonError {
                        HomeDataErrorCard(
                            title: "Расписание недоступно",
                            message: error,
                            icon: "calendar.badge.exclamationmark",
                            retry: { Task { await model.load(user: session.user) } }
                        )
                    } else {
                        Button { selectedTab = 1 } label: {
                            NextLessonCard(item: model.nextLesson, group: session.user?.groupName)
                        }
                        .buttonStyle(.plain)
                    }

                    MauSectionHeader(title: "Быстрый доступ")
                    quickActions

                    if !model.notifications.isEmpty {
                        MauSectionHeader(title: "Уведомления портала")
                        ScrollView(.horizontal, showsIndicators: false) {
                            LazyHStack(spacing: MauLayout.gridRow) {
                                ForEach(model.notifications) { notification in
                                    MoodleNotificationCard(notification: notification)
                                }
                            }
                            .mauHorizontalScrollTrailingInset()
                        }
                    }

                    if let news = model.featuredNews {
                        HStack(alignment: .firstTextBaseline) {
                            Text("Главное в МАУ")
                                .font(.title3.bold())
                                .foregroundStyle(MauTheme.ink)
                            Spacer()
                            Button { selectedTab = 3 } label: {
                                HStack(spacing: 4) {
                                    Text("Все новости")
                                    Image(systemName: "chevron.right")
                                        .font(.caption.bold())
                                }
                                .font(.subheadline.weight(.semibold))
                                .foregroundStyle(MauTheme.blue)
                            }
                        }
                        FeaturedNewsCard(item: news)
                    } else if let error = model.newsError {
                        HomeDataErrorCard(
                            title: "Новости не обновились",
                            message: error,
                            icon: "newspaper",
                            retry: { Task { await model.load(user: session.user) } }
                        )
                    }
                }
                .mauTabPageContent()
            }
            .refreshable { await model.load(user: session.user) }
        }
        .toolbar(.hidden, for: .navigationBar)
        .task { await model.load(user: session.user) }
    }

    private var header: some View {
        HStack(alignment: .top, spacing: 14) {
            VStack(alignment: .leading, spacing: 5) {
                Text(greeting)
                    .font(.subheadline.weight(.medium))
                    .foregroundStyle(MauTheme.muted)
                Text(firstName)
                    .font(.system(size: 38, weight: .bold, design: .rounded))
                    .foregroundStyle(MauTheme.ink)
                HStack(spacing: 8) {
                    MauStatusPill(title: "Вход сохранён", icon: "lock.fill")
                    if let group = session.user?.groupName, !group.isEmpty {
                        Text(group)
                            .font(.caption.weight(.semibold))
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(1)
                    }
                }
            }
            Spacer()
            Button { selectedTab = 4 } label: {
                ZStack {
                    Circle().fill(MauTheme.blue.opacity(0.13))
                    Image(systemName: "person.crop.circle.fill")
                        .font(.system(size: 38))
                        .symbolRenderingMode(.hierarchical)
                        .foregroundStyle(MauTheme.blue)
                }
                .frame(width: 52, height: 52)
            }
        }
    }

    private var missingGroupCard: some View {
        NavigationLink(destination: ProfileView()) {
            HStack(spacing: 15) {
                IconTile(systemName: "person.3.fill", color: .orange)
                VStack(alignment: .leading, spacing: 4) {
                    Text("Добавьте учебную группу").font(.headline)
                    Text("Покажем ближайшее занятие и актуальное расписание")
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                }
                Spacer()
                Image(systemName: "chevron.right").foregroundStyle(MauTheme.muted)
            }
            .padding(MauLayout.cardPadding)
        }
        .buttonStyle(.plain)
        .mauGlass(radius: MauRadius.card)
    }

    private var lessonSkeleton: some View {
        SkeletonCard(height: 120)
            .frame(height: 220)
    }

    private var quickActions: some View {
        MauGlassStack {
            LazyVGrid(
                columns: MauLayout.twoColumnGrid,
                spacing: MauLayout.gridRow
            ) {
                PremiumQuickCard(title: "Расписание", subtitle: "Занятия и аудитории", icon: "calendar", color: MauTheme.blue) {
                    withAnimation(MauMotion.soft) { selectedTab = 1 }
                }
                PremiumQuickCard(title: "Сервисы", subtitle: "Все инструменты МАУ", icon: "square.grid.2x2.fill", color: MauTheme.violet) {
                    withAnimation(MauMotion.soft) { selectedTab = 2 }
                }
                PremiumQuickCard(title: "Новости", subtitle: "События университета", icon: "newspaper.fill", color: .orange) {
                    withAnimation(MauMotion.soft) { selectedTab = 3 }
                }
                NavigationLink {
                    InAppBrowserView(url: URL(string: "https://eios.mauniver.ru/moodle/")!, title: "Учебный портал")
                } label: {
                    PremiumQuickCardContent(
                        title: "Учёба",
                        subtitle: "Учебный портал",
                        icon: "graduationcap.fill",
                        color: .teal
                    )
                }
                .mauPressable()
            }
        }
    }

    private var greeting: String {
        let hour = Calendar.current.component(.hour, from: Date())
        return switch hour {
        case 5..<12: "Доброе утро,"
        case 12..<18: "Добрый день,"
        case 18..<23: "Добрый вечер,"
        default: "Доброй ночи,"
        }
    }
}

private struct MoodleNotificationCard: View {
    let notification: MoodleNotification

    var body: some View {
        Group {
            if let destination = notification.destination {
                NavigationLink {
                    InAppBrowserView(url: destination, title: notification.title)
                } label: {
                    content
                }
            } else {
                content
            }
        }
        .buttonStyle(.plain)
    }

    private var content: some View {
        VStack(alignment: .leading, spacing: 9) {
            Label("ЭИОС", systemImage: "bell.badge.fill")
                .font(.caption.bold())
                .foregroundStyle(MauTheme.blue)
            Text(notification.title)
                .font(.subheadline.weight(.semibold))
                .foregroundStyle(MauTheme.ink)
                .lineLimit(3)
            Text(notification.subtitle)
                .font(.caption)
                .foregroundStyle(MauTheme.muted)
                .lineLimit(2)
            Spacer(minLength: 0)
            if let value = notification.timeCreatedString, !value.isEmpty {
                Text(value)
                    .font(.caption2)
                    .foregroundStyle(MauTheme.muted)
            }
        }
        .padding(MauLayout.cardPadding)
        .frame(width: 272, alignment: .topLeading)
        .frame(minHeight: 164, alignment: .topLeading)
        .mauSurface(radius: 22)
    }
}

private struct HomeDataErrorCard: View {
    let title: String
    let message: String
    let icon: String
    var retry: (() -> Void)?

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            HStack(spacing: 14) {
                IconTile(systemName: icon, color: .orange)
                VStack(alignment: .leading, spacing: 4) {
                    Text(title).font(.headline)
                    Text(message)
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                        .lineLimit(3)
                }
                Spacer()
            }
            if let retry {
                Button("Повторить", action: retry)
                    .font(.subheadline.weight(.semibold))
                    .foregroundStyle(MauTheme.blue)
            }
        }
        .padding(MauLayout.cardPadding)
        .mauSurface(radius: 22)
    }
}

private struct NextLessonCard: View {
    let item: ScheduleItem?
    let group: String?

    var body: some View {
        VStack(alignment: .leading, spacing: 20) {
            HStack {
                Label("БЛИЖАЙШЕЕ ЗАНЯТИЕ", systemImage: "sparkles")
                    .font(.caption2.bold())
                    .tracking(0.8)
                    .layoutPriority(1)
                Text(group ?? "")
                    .font(.caption.weight(.semibold))
                    .lineLimit(1)
                    .frame(maxWidth: .infinity, alignment: .trailing)
            }
            .foregroundStyle(.white.opacity(0.82))

            if let item {
                VStack(alignment: .leading, spacing: 8) {
                    Text(item.name ?? "Занятие")
                        .font(.system(size: 25, weight: .bold, design: .rounded))
                        .lineLimit(2)
                    Text([item.startTime, item.endTime].compactMap { $0 }.joined(separator: " — "))
                        .font(.system(size: 32, weight: .bold, design: .rounded))
                }
                HStack(spacing: 16) {
                    if let room = item.room {
                        Label(room, systemImage: "mappin.and.ellipse")
                    }
                    if let type = item.pairType {
                        Label(type, systemImage: "book.closed.fill")
                    }
                }
                .font(.caption.weight(.semibold))
                .lineLimit(1)
            } else {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Расписание свободно")
                        .font(.system(size: 25, weight: .bold, design: .rounded))
                    Text("Новых занятий на ближайшие две недели нет")
                        .font(.subheadline)
                        .foregroundStyle(.white.opacity(0.76))
                }
            }
        }
        .foregroundStyle(.white)
        .frame(maxWidth: .infinity, minHeight: 176, alignment: .topLeading)
        .padding(20)
        .background(MauTheme.heroGradient, in: RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
        .overlay(alignment: .topTrailing) {
            Circle()
                .fill(.white.opacity(0.12))
                .frame(width: 130, height: 130)
                .blur(radius: 2)
                .offset(x: 42, y: -45)
        }
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
        .shadow(color: MauTheme.blue.opacity(0.24), radius: 22, y: 12)
    }
}

private struct PremiumQuickCard: View {
    let title: String
    let subtitle: String
    let icon: String
    let color: Color
    let action: () -> Void

    var body: some View {
        Button(action: action) {
            PremiumQuickCardContent(title: title, subtitle: subtitle, icon: icon, color: color)
        }
        .mauPressable()
    }
}

private struct PremiumQuickCardContent: View {
    let title: String
    let subtitle: String
    let icon: String
    let color: Color

    var body: some View {
        VStack(alignment: .leading, spacing: 15) {
            HStack {
                IconTile(systemName: icon, color: color)
                Spacer()
                Image(systemName: "arrow.up.right")
                    .font(.caption.bold())
                    .foregroundStyle(MauTheme.muted)
            }
            VStack(alignment: .leading, spacing: 3) {
                Text(title).font(.headline)
                Text(subtitle)
                    .font(.caption)
                    .foregroundStyle(MauTheme.muted)
                    .lineLimit(1)
            }
        }
        .foregroundStyle(MauTheme.ink)
        .frame(maxWidth: .infinity, minHeight: MauLayout.cardMinHeight, alignment: .topLeading)
        .padding(MauLayout.cardPadding)
        .mauGlass(radius: MauRadius.card, style: .interactive)
    }
}

private struct FeaturedNewsCard: View {
    let item: NewsItem

    @ViewBuilder
    var body: some View {
        if let link = item.link, let url = URL(string: link) {
            NavigationLink(destination: InAppBrowserView(url: url, title: item.title ?? "Новости МАУ")) {
                content
            }
            .buttonStyle(.plain)
        } else {
            content
        }
    }

    private var content: some View {
        ZStack(alignment: .bottomLeading) {
            AsyncImage(url: URL(string: item.image ?? "")) { phase in
                if case .success(let image) = phase {
                    image
                        .resizable()
                        .scaledToFill()
                        .frame(maxWidth: .infinity, maxHeight: .infinity)
                        .clipped()
                } else {
                    MauTheme.heroGradient
                }
            }
            .frame(height: 250)
            .clipped()

            LinearGradient(colors: [.clear, .black.opacity(0.86)], startPoint: .top, endPoint: .bottom)

            VStack(alignment: .leading, spacing: 8) {
                Text(item.publish ?? "МАУ")
                    .font(.caption.bold())
                    .foregroundStyle(MauTheme.cyan)
                Text(item.title ?? "Новости университета")
                    .font(.title3.bold())
                    .foregroundStyle(.white)
                    .lineLimit(3)
            }
            .padding(MauLayout.cardPadding)
        }
        .frame(maxWidth: .infinity)
        .frame(height: 250)
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
    }
}
