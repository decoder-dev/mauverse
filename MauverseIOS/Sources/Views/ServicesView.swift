import SwiftUI

enum MauService: String, CaseIterable, Identifiable {
    case eios, forms, messenger, campus, debts, digital, teachers, departments
    var id: String { rawValue }

    var title: String {
        switch self {
        case .eios: "ЭИОС"
        case .forms: "Онлайн-формы"
        case .messenger: "Мессенджер ЭИОС"
        case .campus: "Навигатор по корпусам"
        case .debts: "Учебные задолженности"
        case .digital: "Цифровые сервисы МАУ"
        case .teachers: "Контакты преподавателей"
        case .departments: "Подразделения и телефоны"
        }
    }

    var icon: String {
        switch self {
        case .eios: "graduationcap.fill"
        case .forms: "doc.text.fill"
        case .messenger: "message.fill"
        case .campus: "map.fill"
        case .debts: "exclamationmark.circle.fill"
        case .digital: "network"
        case .teachers: "person.text.rectangle.fill"
        case .departments: "building.2.fill"
        }
    }

    var color: Color {
        switch self {
        case .eios, .teachers: MauTheme.blue
        case .forms, .departments: .purple
        case .messenger, .digital: .teal
        case .campus: .orange
        case .debts: .red
        }
    }
}

struct ServicesView: View {
    private let columns = [GridItem(.flexible(), spacing: 13), GridItem(.flexible(), spacing: 13)]

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(alignment: .leading, spacing: 20) {
                    Text("Сервисы").font(.system(size: 34, weight: .bold, design: .rounded))
                    Text("Полезные инструменты для учёбы и жизни в университете")
                        .font(.subheadline)
                        .foregroundStyle(MauTheme.muted)

                    LazyVGrid(columns: columns, spacing: 13) {
                        ForEach(MauService.allCases) { service in
                            NavigationLink(destination: ServiceDestination(service: service)) {
                                VStack(alignment: .leading, spacing: 18) {
                                    IconTile(systemName: service.icon, color: service.color)
                                    Text(service.title)
                                        .font(.subheadline.weight(.semibold))
                                        .foregroundStyle(MauTheme.ink)
                                        .multilineTextAlignment(.leading)
                                        .frame(maxWidth: .infinity, minHeight: 38, alignment: .topLeading)
                                }
                                .padding(17)
                                .frame(maxWidth: .infinity, minHeight: 132, alignment: .topLeading)
                            }
                            .buttonStyle(.plain)
                            .mauGlass(radius: 23)
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 12)
            }
        }
        .navigationBarTitleDisplayMode(.inline)
    }
}

private struct ServiceDestination: View {
    let service: MauService

    @ViewBuilder
    var body: some View {
        switch service {
        case .eios:
            WebServiceView(title: service.title, url: "https://eios.mauniver.ru/moodle/")
        case .digital:
            WebServiceView(title: service.title, url: "https://www.mauniver.ru/services/student/")
        case .messenger:
            WebServiceView(title: service.title, url: "https://eios.mauniver.ru/moodle/message/index.php")
        case .forms:
            FormsView()
        case .campus:
            CampusView()
        case .debts:
            DebtsView()
        case .teachers:
            TeacherContactsView()
        case .departments:
            DepartmentsView()
        }
    }
}

private struct WebServiceView: View {
    let title: String
    let url: String

    var body: some View {
        if let destination = URL(string: url) {
            SafariView(url: destination)
                .ignoresSafeArea()
                .navigationTitle(title)
                .navigationBarTitleDisplayMode(.inline)
        } else {
            EmptyState(icon: "link.badge.plus", title: "Ссылка недоступна", message: url)
        }
    }
}

private struct FormsView: View {
    private let forms = [
        ("Справка об обучении", "doc.badge.plus"),
        ("Выписка об успеваемости", "list.clipboard.fill"),
        ("Обращение в деканат", "envelope.fill")
    ]

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 13) {
                    ForEach(forms, id: \.0) { form in
                        HStack(spacing: 14) {
                            IconTile(systemName: form.1)
                            Text(form.0).font(.headline)
                            Spacer()
                            Image(systemName: "chevron.right").foregroundStyle(MauTheme.muted)
                        }
                        .padding(17)
                        .mauGlass(radius: 22)
                    }
                    Text("Отправка формы откроется после выбора типа документа в ЭИОС.")
                        .font(.footnote)
                        .foregroundStyle(MauTheme.muted)
                        .padding()
                }
                .padding(20)
            }
        }
        .navigationTitle("Онлайн-формы")
        .navigationBarTitleDisplayMode(.large)
    }
}

private struct Campus: Identifiable {
    let id = UUID()
    let code: String
    let address: String
}

private struct CampusView: View {
    private let south = [
        Campus(code: "А", address: "Спортивная, 13/6"),
        Campus(code: "Б", address: "Колхозная, 2"),
        Campus(code: "В", address: "Спортивная, 13"),
        Campus(code: "Г", address: "Советская, 8А"),
        Campus(code: "Д", address: "Советская, 8"),
        Campus(code: "Е", address: "Советская, 12А"),
        Campus(code: "К", address: "Спортивная, 9"),
        Campus(code: "Л1 / Л2", address: "Кирова, 1"),
        Campus(code: "М", address: "Советская, 17"),
        Campus(code: "Н", address: "Спортивная, 11"),
        Campus(code: "П", address: "Советская, 10"),
        Campus(code: "С", address: "Советская, 14"),
        Campus(code: "Э", address: "Горького, 14")
    ]
    private let north = [
        Campus(code: "Е15", address: "Капитана Егорова, 15"),
        Campus(code: "Е16", address: "Капитана Егорова, 16"),
        Campus(code: "К9", address: "Коммуны, 9"),
        Campus(code: "Л57", address: "проспект Ленина, 57")
    ]

    var body: some View {
        ZStack {
            MauBackground()
            List {
                CampusSection(title: "Южный кампус", campuses: south)
                CampusSection(title: "Северный кампус", campuses: north)
            }
            .scrollContentBackground(.hidden)
        }
        .navigationTitle("Корпуса")
    }
}

private struct CampusSection: View {
    let title: String
    let campuses: [Campus]
    var body: some View {
        Section(title) {
            ForEach(campuses) { campus in
                Button {
                    let query = "Мурманск, \(campus.address)"
                    let encoded = query.addingPercentEncoding(withAllowedCharacters: .urlPathAllowed) ?? query
                    if let url = URL(string: "https://2gis.ru/murmansk/search/\(encoded)") {
                        UIApplication.shared.open(url)
                    }
                } label: {
                    HStack {
                        Text(campus.code)
                            .font(.headline)
                            .foregroundStyle(MauTheme.blue)
                            .frame(width: 54, alignment: .leading)
                        Text(campus.address).foregroundStyle(MauTheme.ink)
                        Spacer()
                        Image(systemName: "location.fill").foregroundStyle(MauTheme.blue)
                    }
                }
            }
        }
    }
}

@MainActor
private final class DebtsModel: ObservableObject {
    @Published var semesters: [Semester] = []
    @Published var isLoading = false
    @Published var error: String?

    func load(user: UserDTO?) async {
        guard let user, let creditBook = user.creditBook, !creditBook.isEmpty else {
            error = "Укажите номер зачётной книжки в профиле"
            return
        }
        isLoading = true
        error = nil
        defer { isLoading = false }
        do {
            semesters = try await APIClient.shared.post(
                "get_semesters",
                body: DebtRequest(creditBook: creditBook),
                user: user
            )
        } catch { self.error = error.localizedDescription }
    }
}

private struct DebtsView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = DebtsModel()

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 15) {
                    if model.isLoading {
                        LoadingOverlay(title: "Проверяем задолженности")
                    } else if let error = model.error {
                        EmptyState(icon: "exclamationmark.circle", title: "Нет данных", message: error)
                    } else if model.semesters.isEmpty {
                        EmptyState(icon: "checkmark.seal.fill", title: "Задолженностей нет", message: "Данные по указанной зачётной книжке не найдены")
                    } else {
                        ForEach(model.semesters) { semester in
                            VStack(alignment: .leading, spacing: 12) {
                                Text(semester.semester ?? "Семестр \(semester.semesterNumber ?? 0)").font(.headline)
                                Text(semester.semesterSubtitle ?? "")
                                    .font(.caption).foregroundStyle(MauTheme.muted)
                                ForEach(semester.debts ?? []) { debt in
                                    HStack {
                                        VStack(alignment: .leading) {
                                            Text(debt.discipline ?? "Дисциплина")
                                            Text(debt.markType ?? "Задолженность")
                                                .font(.caption).foregroundStyle(.red)
                                        }
                                        Spacer()
                                    }
                                    .padding(12)
                                    .background(Color.red.opacity(0.06), in: RoundedRectangle(cornerRadius: 14))
                                }
                            }
                            .padding(18)
                            .mauGlass()
                        }
                    }
                }
                .padding(20)
            }
        }
        .navigationTitle("Задолженности")
        .task { await model.load(user: session.user) }
    }
}

@MainActor
private final class TeachersModel: ObservableObject {
    @Published var teachers: [Teacher] = []
    @Published var isLoading = false
    @Published var error: String?

    func search(_ text: String, user: UserDTO?) async {
        guard !text.trimmingCharacters(in: .whitespaces).isEmpty else { return }
        isLoading = true
        error = nil
        defer { isLoading = false }
        do {
            teachers = try await APIClient.shared.post("get_teachers", body: TeacherRequest(name: text), user: user)
        } catch { self.error = error.localizedDescription }
    }
}

private struct TeacherContactsView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = TeachersModel()
    @State private var query = ""

    var body: some View {
        ZStack {
            MauBackground()
            VStack(spacing: 14) {
                HStack {
                    TextField("Фамилия преподавателя", text: $query)
                        .textInputAutocapitalization(.words)
                    Button { Task { await model.search(query, user: session.user) } } label: {
                        Image(systemName: "magnifyingglass").font(.headline)
                    }
                }
                .padding(16)
                .mauGlass(radius: 20)

                if model.isLoading { LoadingOverlay(title: "Ищем") }
                else if let error = model.error { EmptyState(icon: "wifi.exclamationmark", title: "Ошибка", message: error) }
                else {
                    ScrollView {
                        LazyVStack(spacing: 12) {
                            ForEach(model.teachers, id: \.stableID) { teacher in
                                ContactCard(
                                    title: teacher.fullName ?? teacher.name ?? "Преподаватель",
                                    subtitle: teacher.post,
                                    phone: teacher.phone,
                                    email: teacher.email
                                )
                            }
                        }
                    }
                }
            }
            .padding(20)
        }
        .navigationTitle("Преподаватели")
    }
}

@MainActor
private final class DepartmentsModel: ObservableObject {
    @Published var departments: [Department] = []
    @Published var contacts: [Telephone] = []
    @Published var error: String?

    func load(user: UserDTO?) async {
        do { departments = try await APIClient.shared.get("get_depts_json", user: user) }
        catch { self.error = error.localizedDescription }
    }

    func contacts(for department: Department, user: UserDTO?) async {
        do {
            contacts = try await APIClient.shared.post(
                "get_contacts_json",
                body: DepartmentRequest(departmentId: department.id ?? 0, name: department.name ?? ""),
                user: user
            )
        } catch { self.error = error.localizedDescription }
    }
}

private struct DepartmentsView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = DepartmentsModel()
    @State private var selected: Department?

    var body: some View {
        ZStack {
            MauBackground()
            List(model.departments) { department in
                Button {
                    selected = department
                    Task { await model.contacts(for: department, user: session.user) }
                } label: {
                    HStack {
                        Text(department.name ?? "Подразделение").foregroundStyle(MauTheme.ink)
                        Spacer()
                        Image(systemName: "chevron.right").foregroundStyle(MauTheme.muted)
                    }
                }
            }
            .scrollContentBackground(.hidden)
        }
        .navigationTitle("Подразделения")
        .task { await model.load(user: session.user) }
        .sheet(item: $selected) { department in
            NavigationStack {
                ScrollView {
                    LazyVStack(spacing: 12) {
                        if let error = model.error {
                            EmptyState(icon: "wifi.exclamationmark", title: "Ошибка", message: error)
                        }
                        ForEach(model.contacts) { contact in
                            ContactCard(
                                title: contact.person ?? contact.title ?? "Контакт",
                                subtitle: [contact.building2, contact.room].compactMap { $0 }.joined(separator: ", "),
                                phone: contact.phone,
                                email: contact.depEmail
                            )
                        }
                    }
                    .padding(20)
                }
                .background(MauBackground())
                .navigationTitle(department.name ?? "Контакты")
                .navigationBarTitleDisplayMode(.inline)
            }
        }
    }
}

private struct ContactCard: View {
    let title: String
    let subtitle: String?
    let phone: String?
    let email: String?

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text(title).font(.headline)
            if let subtitle, !subtitle.isEmpty {
                Text(subtitle).font(.caption).foregroundStyle(MauTheme.muted)
            }
            HStack {
                if let phone, !phone.isEmpty {
                    Link(destination: URL(string: "tel:\(phone.filter { $0.isNumber || $0 == "+" })")!) {
                        Label(phone, systemImage: "phone.fill")
                    }
                }
                if let email, let url = URL(string: "mailto:\(email)") {
                    Link(destination: url) { Image(systemName: "envelope.fill") }
                }
            }
            .font(.subheadline)
            .foregroundStyle(MauTheme.blue)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding(17)
        .mauGlass(radius: 22)
    }
}

