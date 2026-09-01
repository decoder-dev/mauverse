import SwiftUI

@MainActor
final class ScheduleViewModel: ObservableObject {
    @Published var items: [ScheduleItem] = []
    @Published var selectedDate = Calendar.current.startOfDay(for: Date())
    @Published var selectedTeacher: String?
    @Published var selectedRoom: String?
    @Published var isLoading = false
    @Published var error: String?
    private var activeRequest = UUID()

    let dates: [Date] = (0..<14).compactMap {
        Calendar.current.date(byAdding: .day, value: $0, to: Calendar.current.startOfDay(for: Date()))
    }

    var availableTeachers: [String] {
        Array(Set(items.compactMap { item -> String? in
            guard let teacher = item.teacher?.trimmingCharacters(in: .whitespacesAndNewlines),
                  !teacher.isEmpty else { return nil }
            return teacher
        })).sorted()
    }

    var availableRooms: [String] {
        Array(Set(items.compactMap { item -> String? in
            guard let room = item.room?.trimmingCharacters(in: .whitespacesAndNewlines),
                  !room.isEmpty else { return nil }
            return room
        })).sorted()
    }

    func selectTeacher(_ teacher: String?) {
        selectedTeacher = teacher
        if teacher != nil { selectedRoom = nil }
    }

    func selectRoom(_ room: String?) {
        selectedRoom = room
        if room != nil { selectedTeacher = nil }
    }

    func load(user: UserDTO?) async -> ScheduleGroup? {
        let request = UUID()
        activeRequest = request
        guard let user else {
            error = "Необходимо войти в аккаунт"
            items = []
            return nil
        }
        isLoading = true
        error = nil
        defer {
            if activeRequest == request { isLoading = false }
        }
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
            let loaded = try await ScheduleAPIClient.shared.schedule(
                uid: uid,
                start: formatter.string(from: dates.first ?? Date()),
                end: formatter.string(from: dates.last ?? Date())
            )
            guard activeRequest == request else { return nil }
            items = loaded
            if let selectedTeacher, !availableTeachers.contains(selectedTeacher) {
                self.selectedTeacher = nil
            }
            if let selectedRoom, !availableRooms.contains(selectedRoom) {
                self.selectedRoom = nil
            }
            return resolvedGroup
        } catch {
            guard activeRequest == request else { return nil }
            self.error = error.localizedDescription
            return nil
        }
    }

    func items(for date: Date) -> [ScheduleItem] {
        let formats = ["yyyy-MM-dd", "dd.MM.yyyy", "yyyy-MM-dd'T'HH:mm:ss"]
        return items.filter { item in
            guard let value = item.date else { return false }
            let matchesDate = formats.contains { format in
                let parser = DateFormatter()
                parser.locale = Locale(identifier: "en_US_POSIX")
                parser.dateFormat = format
                guard let parsed = parser.date(from: value) else { return false }
                return Calendar.current.isDate(parsed, inSameDayAs: date)
            }
            guard matchesDate else { return false }
            if let selectedTeacher {
                return item.teacher == selectedTeacher
            }
            if let selectedRoom {
                return item.room == selectedRoom
            }
            return true
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
                VStack(alignment: .leading, spacing: MauLayout.sectionStack) {
                    HStack(alignment: .top) {
                        VStack(alignment: .leading, spacing: 6) {
                            Text("Расписание")
                                .font(.system(size: 36, weight: .bold, design: .rounded))
                            Text(model.selectedDate.formatted(
                                .dateTime
                                    .locale(Locale(identifier: "ru_RU"))
                                    .weekday(.wide)
                                    .day()
                                    .month(.wide)
                            ))
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
                        .disabled(model.isLoading)
                    }

                    NavigationLink {
                        ProfileView()
                    } label: {
                        HStack(spacing: 12) {
                            IconTile(systemName: "person.3.fill")
                            VStack(alignment: .leading, spacing: 3) {
                                Text(session.user?.groupName ?? "Группа не указана")
                                    .font(.headline)
                                Text(session.user?.speciality ?? "Нажмите, чтобы указать группу в профиле")
                                    .font(.caption)
                                    .foregroundStyle(MauTheme.muted)
                                    .lineLimit(1)
                            }
                            Spacer()
                            Image(systemName: "chevron.right")
                                .font(.caption.bold())
                                .foregroundStyle(MauTheme.muted)
                        }
                        .padding(MauLayout.cardPadding)
                        .mauSurface(radius: 22)
                    }
                    .buttonStyle(.plain)

                    ScrollView(.horizontal, showsIndicators: false) {
                        HStack(spacing: MauLayout.gridGutter) {
                            ForEach(model.dates, id: \.self) { date in
                                DateChip(date: date, selected: Calendar.current.isDate(date, inSameDayAs: model.selectedDate))
                                    .onTapGesture { withAnimation(.snappy) { model.selectedDate = date } }
                            }
                        }
                        .padding(.vertical, 2)
                        .mauHorizontalScrollTrailingInset()
                    }

                    if !model.availableTeachers.isEmpty || !model.availableRooms.isEmpty {
                        VStack(alignment: .leading, spacing: MauLayout.sectionHeaderBottom) {
                            if !model.availableTeachers.isEmpty {
                                Text("Преподаватель")
                                    .font(.caption.weight(.semibold))
                                    .foregroundStyle(MauTheme.muted)
                                ScrollView(.horizontal, showsIndicators: false) {
                                    HStack(spacing: MauLayout.gridGutter) {
                                        FilterChip(
                                            title: "Все",
                                            selected: model.selectedTeacher == nil
                                        ) { model.selectTeacher(nil) }
                                        ForEach(model.availableTeachers, id: \.self) { teacher in
                                            FilterChip(
                                                title: teacher,
                                                selected: model.selectedTeacher == teacher
                                            ) { model.selectTeacher(teacher) }
                                        }
                                    }
                                    .mauHorizontalScrollTrailingInset()
                                }
                            }
                            if !model.availableRooms.isEmpty {
                                Text("Аудитория")
                                    .font(.caption.weight(.semibold))
                                    .foregroundStyle(MauTheme.muted)
                                ScrollView(.horizontal, showsIndicators: false) {
                                    HStack(spacing: MauLayout.gridGutter) {
                                        FilterChip(
                                            title: "Все",
                                            selected: model.selectedRoom == nil
                                        ) { model.selectRoom(nil) }
                                        ForEach(model.availableRooms, id: \.self) { room in
                                            FilterChip(
                                                title: room,
                                                selected: model.selectedRoom == room
                                            ) { model.selectRoom(room) }
                                        }
                                    }
                                    .mauHorizontalScrollTrailingInset()
                                }
                            }
                        }
                    }

                    if model.isLoading {
                        VStack(spacing: 12) {
                            ForEach(0..<3, id: \.self) { _ in
                                SkeletonCard(height: 56)
                            }
                        }
                        .transition(.opacity)
                    } else if let error = model.error {
                        VStack(spacing: 12) {
                            EmptyState(icon: "wifi.exclamationmark", title: "Не удалось загрузить", message: error)
                            Button("Повторить") { Task { await reload() } }
                                .buttonStyle(.borderedProminent)
                        }
                    } else if model.items(for: model.selectedDate).isEmpty {
                        EmptyState(icon: "cup.and.saucer.fill", title: "Занятий нет", message: "На выбранную дату расписание пустое")
                            .transition(.opacity.combined(with: .move(edge: .bottom)))
                    } else {
                        LazyVStack(spacing: 12) {
                            ForEach(model.items(for: model.selectedDate), id: \.stableID) { item in
                                LessonCard(item: item)
                                    .transition(.asymmetric(
                                        insertion: .opacity.combined(with: .move(edge: .trailing)),
                                        removal: .opacity
                                    ))
                            }
                        }
                        .animation(MauMotion.soft, value: model.selectedDate)
                    }
                }
                .mauTabPageContent()
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
                $0.groupId = String(group.groupId)
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
            Text(date.formatted(.dateTime.weekday(.abbreviated).locale(Locale(identifier: "ru_RU"))).uppercased())
                .font(.caption2.weight(.bold))
            Text(date.formatted(.dateTime.day().locale(Locale(identifier: "ru_RU"))))
                .font(.title3.bold())
        }
        .foregroundStyle(selected ? .white : MauTheme.ink)
        .frame(width: 58, height: 68)
        .background {
            if selected {
                RoundedRectangle(cornerRadius: 19, style: .continuous)
                    .fill(MauTheme.heroGradient)
            }
        }
        .mauGlass(radius: 19, style: selected ? .regular : .thin)
        .shadow(color: selected ? MauTheme.blue.opacity(0.22) : .clear, radius: 10, y: 5)
        .animation(MauMotion.snappy, value: selected)
    }
}

private struct FilterChip: View {
    let title: String
    let selected: Bool
    let action: () -> Void

    var body: some View {
        Button {
            withAnimation(MauMotion.snappy) { action() }
        } label: {
            Text(title)
                .font(.caption.weight(.semibold))
                .foregroundStyle(selected ? .white : MauTheme.ink)
                .lineLimit(1)
                .padding(.horizontal, 12)
                .padding(.vertical, 8)
                .background {
                    if selected {
                        Capsule().fill(MauTheme.blue.gradient)
                    }
                }
                .mauGlass(radius: 20, style: .thin)
        }
        .mauPressable()
        .sensoryFeedback(.selection, trigger: selected)
    }
}

private struct LessonCard: View {
    let item: ScheduleItem
    private var kind: LessonKind { LessonKind(value: item.pairType) }

    var body: some View {
        HStack(alignment: .top, spacing: 15) {
            VStack(spacing: 2) {
                Text(item.startTime ?? "—").font(.subheadline.bold())
                Text(item.endTime ?? "").font(.caption2).foregroundStyle(MauTheme.muted)
            }
            .frame(width: 52)

            VStack(spacing: 0) {
                Circle()
                    .fill(kind.color)
                    .frame(width: 12, height: 12)
                    .overlay(Circle().stroke(kind.color.opacity(0.22), lineWidth: 6))
                Rectangle()
                    .fill(kind.color.opacity(0.2))
                    .frame(width: 2, height: 64)
            }
            .padding(.top, 4)

            VStack(alignment: .leading, spacing: 6) {
                HStack(alignment: .top) {
                    Text(item.name ?? "Занятие")
                        .font(.headline)
                    Spacer(minLength: 8)
                    Text(kind.title)
                        .font(.caption2.bold())
                        .foregroundStyle(kind.color)
                        .padding(.horizontal, 8)
                        .padding(.vertical, 5)
                        .background(kind.color.opacity(0.11), in: Capsule())
                }
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
        .padding(MauLayout.cardPadding)
        .mauGlass(radius: MauRadius.card, style: .interactive)
    }
}

private struct LessonKind {
    let title: String
    let color: Color

    init(value: String?) {
        let normalized = value?.lowercased() ?? ""
        if normalized.contains("экзам") || normalized.contains("зач") {
            title = value ?? "Контроль"
            color = .red
        } else if normalized.contains("лабо") {
            title = value ?? "Лабораторная"
            color = MauTheme.violet
        } else if normalized.contains("практ") {
            title = value ?? "Практика"
            color = .orange
        } else if normalized.contains("консульт") {
            title = value ?? "Консультация"
            color = .teal
        } else {
            title = value?.isEmpty == false ? value! : "Лекция"
            color = MauTheme.blue
        }
    }
}
