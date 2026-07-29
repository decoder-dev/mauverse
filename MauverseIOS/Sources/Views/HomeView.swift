import SwiftUI

@MainActor
final class HomeViewModel: ObservableObject {
    @Published var nextLesson: ScheduleItem?
    @Published var featuredNews: NewsItem?
    @Published var isLoading = false

    func load(user: UserDTO?) async {
        isLoading = true
        defer { isLoading = false }

        async let newsTask = try? OfficialNewsService.shared.load(filter: .all)
        if let user {
            var uid = user.scheduleGroupUID
            if uid?.isEmpty != false, let name = user.groupName, !name.isEmpty {
                uid = try? await ScheduleAPIClient.shared.findGroup(named: name)?.uid
            }
            if let uid {
                let formatter = DateFormatter()
                formatter.locale = Locale(identifier: "en_US_POSIX")
                formatter.dateFormat = "yyyy-MM-dd"
                let start = Date()
                let end = Calendar.current.date(byAdding: .day, value: 14, to: start) ?? start
                if let lessons = try? await ScheduleAPIClient.shared.schedule(
                    uid: uid,
                    start: formatter.string(from: start),
                    end: formatter.string(from: end)
                ) {
                    nextLesson = lessons.sorted(by: Self.isEarlier).first
                }
            }
        }
        featuredNews = await newsTask?.first
    }

    private static func isEarlier(_ lhs: ScheduleItem, _ rhs: ScheduleItem) -> Bool {
        "\(lhs.date ?? "") \(lhs.startTime ?? "")" < "\(rhs.date ?? "") \(rhs.startTime ?? "")"
    }
}

struct HomeView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = HomeViewModel()

    private var firstName: String {
        session.user?.firstName?.split(separator: " ").first.map(String.init)
        ?? session.user?.displayName.split(separator: " ").first.map(String.init)
        ?? "Студент"
    }

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: MauSpacing.lg) {
                    header

                    if session.user?.groupName?.isEmpty != false {
                        missingGroupCard
                    } else if model.isLoading {
                        lessonSkeleton
                    } else {
                        NextLessonCard(item: model.nextLesson, group: session.user?.groupName)
                    }

                    MauSectionHeader(title: "Быстрый доступ")
                    quickActions

                    if let news = model.featuredNews {
                        HStack(alignment: .firstTextBaseline) {
                            Text("Главное в МАУ")
                                .font(.title3.bold())
                                .foregroundStyle(MauTheme.ink)
                            Spacer()
                            NavigationLink(destination: NewsView()) {
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
                    }
                }
                .padding(.horizontal, 20)
                .padding(.top, 14)
                .padding(.bottom, 24)
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
                    MauStatusPill(title: "Сессия защищена", icon: "lock.fill")
                    if let group = session.user?.groupName, !group.isEmpty {
                        Text(group)
                            .font(.caption.weight(.semibold))
                            .foregroundStyle(MauTheme.muted)
                            .lineLimit(1)
                    }
                }
            }
            Spacer()
            NavigationLink(destination: ProfileView()) {
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
            .padding(18)
        }
        .buttonStyle(.plain)
        .mauGlass(radius: MauRadius.card)
    }

    private var lessonSkeleton: some View {
        RoundedRectangle(cornerRadius: MauRadius.hero)
            .fill(MauTheme.blue.opacity(0.14))
            .frame(height: 220)
            .overlay { ProgressView().tint(MauTheme.blue) }
    }

    private var quickActions: some View {
        LazyVGrid(
            columns: [GridItem(.flexible(), spacing: 12), GridItem(.flexible(), spacing: 12)],
            spacing: 12
        ) {
            PremiumQuickCard(title: "Расписание", subtitle: "Занятия и аудитории", icon: "calendar", color: MauTheme.blue) {
                ScheduleView()
            }
            PremiumQuickCard(title: "Сервисы", subtitle: "Все инструменты МАУ", icon: "square.grid.2x2.fill", color: MauTheme.violet) {
                ServicesView()
            }
            PremiumQuickCard(title: "Новости", subtitle: "События университета", icon: "newspaper.fill", color: .orange) {
                NewsView()
            }
            PremiumQuickCard(title: "ЭИОС", subtitle: "Учебный портал", icon: "graduationcap.fill", color: .teal) {
                InAppBrowserView(url: URL(string: "https://eios.mauniver.ru/moodle/")!, title: "ЭИОС")
            }
        }
    }

    private var greeting: String {
        let hour = Calendar.current.component(.hour, from: Date())
        return switch hour {
        case 5..<12: "Доброе утро,"
        case 12..<18: "Добрый день,"
        default: "Добрый вечер,"
        }
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
                Spacer()
                Text(group ?? "")
                    .font(.caption.weight(.semibold))
                    .lineLimit(1)
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
        .padding(22)
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

private struct PremiumQuickCard<Destination: View>: View {
    let title: String
    let subtitle: String
    let icon: String
    let color: Color
    @ViewBuilder let destination: () -> Destination

    var body: some View {
        NavigationLink(destination: destination()) {
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
            .frame(maxWidth: .infinity, minHeight: 105, alignment: .topLeading)
            .padding(16)
        }
        .buttonStyle(.plain)
        .mauSurface(radius: 22)
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
            .padding(20)
        }
        .frame(maxWidth: .infinity)
        .frame(height: 250)
        .clipShape(RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
    }
}
