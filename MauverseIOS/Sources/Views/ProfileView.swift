import SwiftUI
import WebKit

struct ProfileView: View {
    @EnvironmentObject private var session: SessionStore
    @State private var showingEditor = false
    @State private var showingLogout = false

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 22) {
                    VStack(spacing: 13) {
                        ZStack {
                            Circle().fill(.white.opacity(0.16))
                            Image(systemName: "person.fill")
                                .font(.system(size: 42, weight: .medium))
                                .foregroundStyle(.white)
                        }
                        .frame(width: 86, height: 86)
                        .overlay(Circle().stroke(.white.opacity(0.25), lineWidth: 1))

                        Text(session.user?.displayName ?? "Пользователь")
                            .font(.system(size: 25, weight: .bold, design: .rounded))
                            .multilineTextAlignment(.center)
                        Text(session.user?.username ?? "")
                            .font(.subheadline)
                            .foregroundStyle(.white.opacity(0.72))
                        HStack(spacing: 8) {
                            Label("Вход сохранён", systemImage: "lock.fill")
                            if let group = session.user?.groupName, !group.isEmpty {
                                Text("•")
                                Text(group)
                            }
                        }
                        .font(.caption.weight(.semibold))
                        .foregroundStyle(.white.opacity(0.82))
                    }
                    .foregroundStyle(.white)
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 26)
                    .padding(.horizontal, 20)
                    .background(MauTheme.heroGradient, in: RoundedRectangle(cornerRadius: MauRadius.hero, style: .continuous))
                    .shadow(color: MauTheme.blue.opacity(0.2), radius: 20, y: 10)

                    VStack(alignment: .leading, spacing: 18) {
                        Label("Учебные данные", systemImage: "graduationcap.fill")
                            .font(.headline)
                            .foregroundStyle(MauTheme.blue)
                        ProfileRow(label: "Группа", value: session.user?.groupName)
                        Divider()
                        ProfileRow(label: "Специальность", value: session.user?.speciality)
                        Divider()
                        ProfileRow(label: "Зачётная книжка", value: session.user?.creditBook)
                    }
                    .padding(20)
                    .mauSurface()

                    VStack(spacing: 0) {
                        Button { showingEditor = true } label: {
                            ProfileActionRow(title: "Учебные данные", icon: "pencil", color: MauTheme.blue)
                        }
                        Divider().padding(.leading, 58)
                        NavigationLink(destination: SettingsView()) {
                            ProfileActionRow(title: "Настройки", icon: "gearshape.fill", color: MauTheme.violet)
                        }
                        Divider().padding(.leading, 58)
                        Button(role: .destructive) { showingLogout = true } label: {
                            ProfileActionRow(title: "Выйти из аккаунта", icon: "rectangle.portrait.and.arrow.right", color: .red)
                        }
                    }
                    .buttonStyle(.plain)
                    .mauSurface()

                    Text("MAUverse 1.12.4 (33)")
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                }
                .padding(20)
                .padding(.bottom, 96)
            }
        }
        .navigationTitle("Профиль")
        .navigationBarTitleDisplayMode(.inline)
        .sheet(isPresented: $showingEditor) { ProfileEditor() }
        .confirmationDialog("Выйти из аккаунта?", isPresented: $showingLogout, titleVisibility: .visible) {
            Button("Выйти", role: .destructive) { session.signOut() }
        }
    }
}

private struct ProfileActionRow: View {
    let title: String
    let icon: String
    let color: Color

    var body: some View {
        HStack(spacing: 14) {
            Image(systemName: icon)
                .font(.system(size: 17, weight: .semibold))
                .foregroundStyle(color)
                .frame(width: 34, height: 34)
                .background(color.opacity(0.11), in: RoundedRectangle(cornerRadius: 10))
            Text(title)
                .font(.subheadline.weight(.semibold))
                .foregroundStyle(title.contains("Выйти") ? .red : MauTheme.ink)
            Spacer()
            Image(systemName: "chevron.right")
                .font(.caption.bold())
                .foregroundStyle(MauTheme.muted)
        }
        .padding(.horizontal, 16)
        .padding(.vertical, 13)
    }
}

private struct ProfileRow: View {
    let label: String
    let value: String?
    var body: some View {
        HStack(alignment: .top) {
            Text(label).foregroundStyle(MauTheme.muted)
            Spacer()
            Text(value?.isEmpty == false ? value! : "Не указано")
                .fontWeight(.medium)
                .multilineTextAlignment(.trailing)
        }
        .font(.subheadline)
    }
}

private struct ProfileEditor: View {
    @EnvironmentObject private var session: SessionStore
    @Environment(\.dismiss) private var dismiss
    @State private var group = ""
    @State private var creditBook = ""
    @State private var isSaving = false
    @State private var errorMessage: String?
    @State private var groupHint: String? = "Введите код группы, например ИС-21. Появятся подсказки из расписания."
    @State private var groupSuggestions: [String] = []
    @State private var isLoadingSuggestions = false
    @State private var suggestionTask: Task<Void, Never>?

    var body: some View {
        NavigationStack {
            Form {
                Section("Расписание") {
                    TextField("Учебная группа", text: $group)
                        .textInputAutocapitalization(.characters)
                        .onChange(of: group) { _, newValue in
                            scheduleSuggestions(for: newValue)
                        }
                    if let groupHint {
                        Text(groupHint)
                            .font(.footnote)
                            .foregroundStyle(MauTheme.muted)
                    }
                    if isLoadingSuggestions {
                        ProgressView()
                            .controlSize(.small)
                    } else if !groupSuggestions.isEmpty {
                        ForEach(groupSuggestions, id: \.self) { suggestion in
                            Button {
                                group = suggestion
                                groupSuggestions = []
                                groupHint = "Группа выбрана из списка расписания"
                            } label: {
                                HStack {
                                    Text(suggestion)
                                        .foregroundStyle(MauTheme.ink)
                                    Spacer()
                                    Image(systemName: "arrow.up.left")
                                        .font(.caption.bold())
                                        .foregroundStyle(MauTheme.blue)
                                }
                            }
                        }
                    }
                    if let errorMessage {
                        Text(errorMessage)
                            .font(.footnote)
                            .foregroundStyle(.red)
                    }
                }
                Section("Задолженности") {
                    TextField("Номер зачётной книжки", text: $creditBook)
                        .keyboardType(.numberPad)
                }
                Section {
                    Text("Данные сохраняются только на этом iPhone и используются для запросов к сервисам МАУ.")
                        .font(.footnote)
                        .foregroundStyle(MauTheme.muted)
                }
            }
            .navigationTitle("Учебные данные")
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .cancellationAction) {
                    Button("Отмена") { dismiss() }
                }
                ToolbarItem(placement: .confirmationAction) {
                    Button(isSaving ? "Проверяем…" : "Сохранить") { save() }
                        .disabled(isSaving)
                }
            }
            .onAppear {
                group = session.user?.groupName ?? ""
                creditBook = session.user?.creditBook ?? ""
            }
            .onDisappear {
                suggestionTask?.cancel()
            }
        }
    }

    private func scheduleSuggestions(for value: String) {
        suggestionTask?.cancel()
        let query = value.trimmingCharacters(in: .whitespacesAndNewlines)
        guard query.count >= 2 else {
            groupSuggestions = []
            isLoadingSuggestions = false
            groupHint = query.isEmpty
                ? "Введите код группы, например ИС-21. Появятся подсказки из расписания."
                : "Введите минимум 2 символа — покажем подсказки из расписания"
            return
        }

        suggestionTask = Task {
            try? await Task.sleep(for: .milliseconds(150))
            guard !Task.isCancelled else { return }
            await MainActor.run { isLoadingSuggestions = true }
            do {
                let suggestions = try await ScheduleAPIClient.shared.suggestGroups(matching: query)
                guard !Task.isCancelled else { return }
                await MainActor.run {
                    groupSuggestions = suggestions
                    isLoadingSuggestions = false
                    groupHint = suggestions.isEmpty
                        ? "Группа не найдена. Проверьте код, например ИС-21"
                        : "Выберите группу из списка или продолжите ввод"
                }
            } catch {
                guard !Task.isCancelled else { return }
                await MainActor.run {
                    groupSuggestions = []
                    isLoadingSuggestions = false
                    groupHint = "Не удалось загрузить подсказки. Проверьте соединение"
                }
            }
        }
    }

    private func save() {
        isSaving = true
        errorMessage = nil
        Task {
            do {
                let normalized = group.trimmingCharacters(in: .whitespacesAndNewlines)
                let resolved = normalized.isEmpty
                    ? nil
                    : try await ScheduleAPIClient.shared.findGroup(named: normalized)
                if !normalized.isEmpty, resolved == nil {
                    throw APIError.server("Группа не найдена в актуальном расписании")
                }
                session.update {
                    $0.groupName = resolved?.group ?? normalized
                    $0.groupId = resolved.map { String($0.groupId) }
                    $0.scheduleGroupUID = resolved?.uid
                    $0.speciality = resolved?.speciality ?? $0.speciality
                    $0.creditBook = creditBook.trimmingCharacters(in: .whitespacesAndNewlines)
                }
                dismiss()
            } catch {
                errorMessage = error.localizedDescription
            }
            isSaving = false
        }
    }
}

private struct SettingsView: View {
    @State private var cleared = false
    @AppStorage("mauverse.appearance") private var appearance = AppTheme.system.rawValue

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 15) {
                    VStack(alignment: .leading, spacing: 13) {
                        Label("Оформление", systemImage: "circle.lefthalf.filled").font(.headline)
                        Picker("Тема", selection: $appearance) {
                            ForEach(AppTheme.allCases) { theme in
                                Text(theme.title).tag(theme.rawValue)
                            }
                        }
                        .pickerStyle(.segmented)
                        Text("В тёмной теме используется белый текст, в светлой — чёрный.")
                            .font(.footnote)
                            .foregroundStyle(MauTheme.muted)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauSurface()

                    VStack(alignment: .leading, spacing: 13) {
                        Label("Кэш приложения", systemImage: "internaldrive.fill").font(.headline)
                        Text("Изображения новостей и веб-страниц очищаются системой автоматически.")
                            .font(.subheadline).foregroundStyle(MauTheme.muted)
                        Button(cleared ? "Готово" : "Очистить временные данные") {
                            clearCaches()
                        }
                        .buttonStyle(.borderedProminent)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauSurface()

                    VStack(alignment: .leading, spacing: 12) {
                        Label("Университет", systemImage: "building.columns.fill").font(.headline)
                        settingsLink("Официальный сайт", url: UniversityPortalURLs.officialSite)
                        Divider()
                        settingsLink("Политика персональных данных", url: UniversityPortalURLs.privacyPolicy)
                        Divider()
                        settingsLink("Сведения об образовательной организации", url: UniversityPortalURLs.sveden)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauSurface()

                    VStack(alignment: .leading, spacing: 12) {
                        Label("О приложении", systemImage: "info.circle.fill").font(.headline)
                        Text("Нативное приложение Мурманского арктического университета.")
                            .font(.subheadline).foregroundStyle(MauTheme.muted)
                        Divider()
                        HStack {
                            Text("Разработчик")
                                .foregroundStyle(MauTheme.muted)
                            Spacer()
                            NavigationLink(
                                destination: InAppBrowserView(
                                    url: URL(string: "https://github.com/decoder-dev")!,
                                    title: "decoder-dev"
                                )
                            ) {
                                HStack(spacing: 5) {
                                    Text("decoder-dev")
                                    Image(systemName: "arrow.up.right")
                                        .font(.caption2.bold())
                                }
                                .font(.subheadline.weight(.semibold))
                                .foregroundStyle(MauTheme.blue)
                            }
                        }
                        Text("Версия 1.12.4 • сборка 33").font(.caption)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauSurface()
                }
                .padding(20)
                .padding(.bottom, 30)
            }
        }
        .navigationTitle("Настройки")
        .toolbar(.hidden, for: .tabBar)
    }

    @ViewBuilder
    private func settingsLink(_ title: String, url: String) -> some View {
        if let destination = URL(string: url) {
            NavigationLink {
                InAppBrowserView(url: destination, title: title)
            } label: {
                HStack {
                    Text(title)
                        .foregroundStyle(MauTheme.ink)
                        .multilineTextAlignment(.leading)
                    Spacer()
                    Image(systemName: "arrow.up.right")
                        .font(.caption2.bold())
                        .foregroundStyle(MauTheme.blue)
                }
                .font(.subheadline.weight(.semibold))
            }
        }
    }

    private func clearCaches() {
        URLCache.shared.removeAllCachedResponses()
        let dataStore = WKWebsiteDataStore.default()
        dataStore.fetchDataRecords(ofTypes: WKWebsiteDataStore.allWebsiteDataTypes()) { records in
            dataStore.removeData(
                ofTypes: WKWebsiteDataStore.allWebsiteDataTypes(),
                for: records
            ) {
                Task { @MainActor in cleared = true }
            }
        }
        Task {
            await OfficialNewsService.shared.clearCache()
            await ScheduleAPIClient.shared.clearCache()
        }
    }
}
