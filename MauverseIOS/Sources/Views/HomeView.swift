import SwiftUI

struct HomeView: View {
    @EnvironmentObject private var session: SessionStore

    private var firstName: String {
        session.user?.firstName?.split(separator: " ").first.map(String.init)
        ?? session.user?.displayName.split(separator: " ").first.map(String.init)
        ?? "Студент"
    }

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 22) {
                    HStack(alignment: .top) {
                        VStack(alignment: .leading, spacing: 5) {
                            Text(greeting)
                                .font(.subheadline)
                                .foregroundStyle(MauTheme.muted)
                            Text(firstName)
                                .font(.system(size: 36, weight: .bold, design: .rounded))
                        }
                        Spacer()
                        NavigationLink(destination: ProfileView()) {
                            Image(systemName: "person.crop.circle.fill")
                                .font(.system(size: 40))
                                .symbolRenderingMode(.hierarchical)
                                .foregroundStyle(MauTheme.blue)
                        }
                    }

                    HStack(spacing: 12) {
                        Image(systemName: "checkmark.icloud.fill")
                            .foregroundStyle(.green)
                        VStack(alignment: .leading, spacing: 3) {
                            Text("Данные сохранены").font(.subheadline.weight(.semibold))
                            Text("Профиль доступен после перезапуска")
                                .font(.caption)
                                .foregroundStyle(MauTheme.muted)
                        }
                    }
                    .padding(17)
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .mauGlass(radius: 20)

                    if session.user?.groupName?.isEmpty != false {
                        NavigationLink(destination: ProfileView()) {
                            HStack(spacing: 14) {
                                IconTile(systemName: "exclamationmark.triangle.fill", color: .orange)
                                VStack(alignment: .leading, spacing: 4) {
                                    Text("Укажите учебную группу").font(.headline)
                                    Text("Она нужна для загрузки расписания")
                                        .font(.caption)
                                        .foregroundStyle(MauTheme.muted)
                                }
                                Spacer()
                                Image(systemName: "chevron.right")
                            }
                            .padding(18)
                        }
                        .buttonStyle(.plain)
                        .mauGlass()
                    }

                    Text("Быстрый доступ")
                        .font(.title3.bold())

                    HStack(spacing: 12) {
                        QuickCard(title: "Расписание", icon: "calendar", color: MauTheme.blue) {
                            ScheduleView()
                        }
                        QuickCard(title: "Сервисы", icon: "square.grid.2x2", color: .purple) {
                            ServicesView()
                        }
                        QuickCard(title: "Новости", icon: "newspaper", color: .orange) {
                            NewsView()
                        }
                    }

                    NavigationLink(destination: NewsView()) {
                        HStack(spacing: 16) {
                            VStack(alignment: .leading, spacing: 7) {
                                Text("Будьте в курсе").font(.title3.bold())
                                Text("Свежие новости и события МАУ")
                                    .font(.subheadline)
                                    .foregroundStyle(MauTheme.muted)
                            }
                            Spacer()
                            Image(systemName: "arrow.up.right")
                                .font(.title3.bold())
                                .foregroundStyle(MauTheme.blue)
                        }
                        .padding(22)
                    }
                    .buttonStyle(.plain)
                    .mauGlass()
                }
                .padding(20)
                .padding(.bottom, 12)
            }
        }
        .toolbar(.hidden, for: .navigationBar)
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

private struct QuickCard<Destination: View>: View {
    let title: String
    let icon: String
    let color: Color
    @ViewBuilder let destination: () -> Destination

    var body: some View {
        NavigationLink(destination: destination()) {
            VStack(alignment: .leading, spacing: 15) {
                IconTile(systemName: icon, color: color)
                Text(title)
                    .font(.caption.weight(.semibold))
                    .foregroundStyle(MauTheme.ink)
                    .lineLimit(1)
            }
            .frame(maxWidth: .infinity, alignment: .leading)
            .padding(14)
        }
        .buttonStyle(.plain)
        .mauGlass(radius: 20)
    }
}
