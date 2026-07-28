import SwiftUI

struct ProfileView: View {
    @EnvironmentObject private var session: SessionStore
    @State private var showingEditor = false
    @State private var showingLogout = false

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 22) {
                    VStack(spacing: 12) {
                        Image(systemName: "person.crop.circle.fill")
                            .font(.system(size: 86))
                            .symbolRenderingMode(.hierarchical)
                            .foregroundStyle(MauTheme.blue)
                        Text(session.user?.displayName ?? "Пользователь")
                            .font(.title2.bold())
                            .multilineTextAlignment(.center)
                        Text(session.user?.username ?? "")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

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
                    .mauGlass()

                    Button { showingEditor = true } label: {
                        Label("Изменить учебные данные", systemImage: "pencil")
                            .fontWeight(.semibold)
                            .frame(maxWidth: .infinity)
                            .padding(.vertical, 16)
                    }
                    .buttonStyle(.plain)
                    .foregroundStyle(MauTheme.blue)
                    .mauGlass(radius: 19)

                    NavigationLink(destination: SettingsView()) {
                        HStack {
                            Label("Настройки", systemImage: "gearshape.fill")
                            Spacer()
                            Image(systemName: "chevron.right")
                        }
                        .padding(17)
                    }
                    .buttonStyle(.plain)
                    .foregroundStyle(MauTheme.ink)
                    .mauGlass(radius: 19)

                    Button(role: .destructive) { showingLogout = true } label: {
                        Label("Выйти", systemImage: "rectangle.portrait.and.arrow.right")
                            .frame(maxWidth: .infinity)
                            .padding(.vertical, 16)
                    }
                    .buttonStyle(.plain)
                    .mauGlass(radius: 19)

                    Text("MAUverse 1.8.5 (20)")
                        .font(.caption)
                        .foregroundStyle(MauTheme.muted)
                }
                .padding(20)
                .padding(.bottom, 12)
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

    var body: some View {
        NavigationStack {
            Form {
                Section("Расписание") {
                    TextField("Учебная группа", text: $group)
                        .textInputAutocapitalization(.characters)
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
                    Button("Сохранить") {
                        session.update {
                            $0.groupName = group.trimmingCharacters(in: .whitespacesAndNewlines)
                            $0.creditBook = creditBook.trimmingCharacters(in: .whitespacesAndNewlines)
                        }
                        dismiss()
                    }
                }
            }
            .onAppear {
                group = session.user?.groupName ?? ""
                creditBook = session.user?.creditBook ?? ""
            }
        }
    }
}

private struct SettingsView: View {
    @State private var cleared = false

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 15) {
                    VStack(alignment: .leading, spacing: 13) {
                        Label("Кэш приложения", systemImage: "internaldrive.fill").font(.headline)
                        Text("Изображения новостей и веб-страниц очищаются системой автоматически.")
                            .font(.subheadline).foregroundStyle(MauTheme.muted)
                        Button(cleared ? "Готово" : "Очистить временные данные") {
                            URLCache.shared.removeAllCachedResponses()
                            cleared = true
                        }
                        .buttonStyle(.borderedProminent)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauGlass()

                    VStack(alignment: .leading, spacing: 12) {
                        Label("О приложении", systemImage: "info.circle.fill").font(.headline)
                        Text("Нативное приложение Мурманского арктического университета.")
                            .font(.subheadline).foregroundStyle(MauTheme.muted)
                        Text("Версия 1.8.5 • сборка 20").font(.caption)
                    }
                    .frame(maxWidth: .infinity, alignment: .leading)
                    .padding(20)
                    .mauGlass()
                }
                .padding(20)
            }
        }
        .navigationTitle("Настройки")
    }
}
