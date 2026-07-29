import SwiftUI

@MainActor
final class ScheduleViewModel: ObservableObject {
    @Published var items: [ScheduleItem] = []
    @Published var selectedDate = Calendar.current.startOfDay(for: Date())
    @Published var isLoading = false
    @Published var error: String?

    let dates: [Date] = (0..<14).compactMap {
        Calendar.current.date(byAdding: .day, value: $0, to: Calendar.current.startOfDay(for: Date()))
    }

    func load(user: UserDTO?) async -> ScheduleGroup? {
        guard let user else { return nil }
        isLoading = true
        error = nil
        defer { isLoading = false }
        let formatter = DateFormatter()
        formatter.locale = Locale(identifier: "en_US_POSIX")
        formatter.dateFormat = "yyyy-MM-dd"
        do {
            var resolvedGroup: ScheduleGroup?
            var uid = user.scheduleGroupUID
            if uid?.isEmpty != false, let groupName = user.groupName, !groupName.isEmpty {
                resolvedGroup = try await ScheduleAPIClient.shared.findGroup(named: groupName)
                uid = resolvedGroup?.uid
            }
            guard let uid, !uid.isEmpty else {
                throw APIError.server("Выберите существующую учебную группу в профиле")
            }
            items = try await ScheduleAPIClient.shared.schedule(
                uid: uid,
                start: formatter.string(from: dates.first ?? Date()),
                end: formatter.string(from: dates.last ?? Date())
            )
            return resolvedGroup
        } catch {
            self.error = error.localizedDescription
            return nil
        }
    }

    func items(for date: Date) -> [ScheduleItem] {
        let formats = ["yyyy-MM-dd", "dd.MM.yyyy", "yyyy-MM-dd'T'HH:mm:ss"]
        return items.filter { item in
            guard let value = item.date else { return false }
            return formats.contains { format in
                let parser = DateFormatter()
                parser.locale = Locale(identifier: "en_US_POSIX")
                parser.dateFormat = format
                guard let parsed = parser.date(from: value) else { return false }
                return Calendar.current.isDate(parsed, inSameDayAs: date)
            }
        }
        .sorted { ($0.startTime ?? "") < ($1.startTime ?? "") }
    }
}

struct ScheduleView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = ScheduleViewModel()

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 20) {
                    HStack(alignment: .top) {
                        VStack(alignment: .leading, spacing: 6) {
                            Text("Расписание")
                                .font(.system(size: 34, weight: .bold, design: .rounded))
                            Text("Занятия на ближайшие две недели")
                                .font(.subheadline)
                                .foregroundStyle(MauTheme.muted)
                        }
                        Spacer()
                        Button { Task { await reload() } } label: {
                            Image(systemName: "arrow.clockwise")
                                .font(.headline)
                                .frame(width: 44, height: 44)
                        }
                        .buttonStyle(.plain)
                        .mauGlass(radius: 16)
                    }

                    HStack(spacing: 12) {
                        IconTile(systemName: "person.3.fill")
                        VStack(alignment: .leading, spacing: 3) {
                            Text(session.user?.groupName ?? "Группа не указана")
                                .font(.headline)
                            Text(session.user?.speciality ?? "Изменить можно в профиле")
                                .font(.caption)
                                .foregroundStyle(MauTheme.muted)
                                .lineLimit(1)
                        }
                        Spacer()
                    }
                    .padding(17)
                    .mauGlass(radius: 22)

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: 10) {
                            ForEach(model.dates, id: \.self) { date in
                                DateChip(date: date, selected: Calendar.current.isDate(date, inSameDayAs: model.selectedDate))
                                    .onTapGesture { withAnimation(.snappy) { model.selectedDate = date } }
                            }
                        }
                        .padding(.vertical, 2)
                    }

                    if model.isLoading {
                        LoadingOverlay(title: "Загружаем расписание")
                            .frame(maxWidth: .infinity)
                    } else if let error = model.error {
                        EmptyState(icon: "wifi.exclamationmark", title: "Не удалось загрузить", message: error)
                    } else if model.items(for: model.selectedDate).isEmpty {
                        EmptyState(icon: "cup.and.saucer.fill", title: "Занятий нет", message: "На выбранную дату расписание пустое")
                    } else {
                        LazyVStack(spacing: 12) {
                            ForEach(model.items(for: model.selectedDate), id: \.stableID) { item in
                                LessonCard(item: item)
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 12)
            }
            .refreshable { await reload() }
        }
        .navigationBarTitleDisplayMode(.inline)
        .task { if model.items.isEmpty { await reload() } }
    }

    private func reload() async {
        if let group = await model.load(user: session.user) {
            session.update {
                $0.scheduleGroupUID = group.uid
                $0.groupId = group.groupId
                $0.groupName = group.group
                $0.speciality = group.speciality
            }
        }
    }
}

private struct DateChip: View {
    let date: Date
    let selected: Bool

    var body: some View {
        VStack(spacing: 5) {
            Text(date.formatted(.dateTime.weekday(.abbreviated)).uppercased())
                .font(.caption2.weight(.bold))
            Text(date.formatted(.dateTime.day()))
                .font(.title3.bold())
        }
        .foregroundStyle(selected ? .white : MauTheme.ink)
        .frame(width: 58, height: 68)
        .background(selected ? AnyShapeStyle(MauTheme.blue.gradient) : AnyShapeStyle(MauTheme.card.opacity(0.82)),
                    in: RoundedRectangle(cornerRadius: 19, style: .continuous))
    }
}

private struct LessonCard: View {
    let item: ScheduleItem

    var body: some View {
        HStack(alignment: .top, spacing: 15) {
            VStack(spacing: 2) {
                Text(item.startTime ?? "—").font(.subheadline.bold())
                Text(item.endTime ?? "").font(.caption2).foregroundStyle(MauTheme.muted)
            }
            .frame(width: 52)

            RoundedRectangle(cornerRadius: 2)
                .fill(MauTheme.blue)
                .frame(width: 4, height: 72)

            VStack(alignment: .leading, spacing: 6) {
                Text(item.name ?? "Занятие")
                    .font(.headline)
                if let teacher = item.teacher, !teacher.isEmpty {
                    Label(teacher, systemImage: "person.fill")
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                }
                HStack {
                    if let room = item.room, !room.isEmpty {
                        Label(room, systemImage: "mappin.and.ellipse")
                    }
                    if let type = item.pairType, !type.isEmpty {
                        Text(type)
                    }
                }
                .font(.caption)
                .foregroundStyle(MauTheme.blue)
            }
            Spacer()
        }
        .padding(17)
        .mauGlass(radius: 22)
    }
}
