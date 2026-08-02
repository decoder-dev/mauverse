import SwiftUI

enum MauService: String, CaseIterable, Identifiable {
    case eios, forms, messenger, campus, debts, digital
    case studentGuide, applicantGuide, scienceGuide, internationalGuide
    case teachers, departments, contacts, eventsCalendar

    var id: String { rawValue }

    var title: String {
        return switch self {
        case .eios: "ЭИОС"
        case .forms: "Онлайн-формы"
        case .messenger: "Мессенджер ЭИОС"
        case .campus: "Навигатор по корпусам"
        case .debts: "Учебные задолженности"
        case .digital: "Цифровые сервисы"
        case .studentGuide: "Студенту"
        case .applicantGuide: "Абитуриенту"
        case .scienceGuide: "Наука"
        case .internationalGuide: "International"
        case .teachers: "Контакты преподавателей"
        case .departments: "Подразделения и телефоны"
        case .contacts: "Контакты и реквизиты"
        case .eventsCalendar: "Календарь событий"
        }
    }

    var icon: String {
        return switch self {
        case .eios: "graduationcap.fill"
        case .forms: "doc.text.fill"
        case .messenger: "message.fill"
        case .campus: "map.fill"
        case .debts: "exclamationmark.circle.fill"
        case .digital: "network"
        case .studentGuide: "person.3.fill"
        case .applicantGuide: "graduationcap.circle.fill"
        case .scienceGuide: "atom"
        case .internationalGuide: "globe"
        case .teachers: "person.text.rectangle.fill"
        case .departments: "building.2.fill"
        case .contacts: "doc.text.fill"
        case .eventsCalendar: "calendar"
        }
    }

    var color: Color {
        return switch self {
        case .eios, .teachers, .studentGuide: MauTheme.blue
        case .forms, .departments, .applicantGuide: .purple
        case .messenger, .digital, .scienceGuide: .teal
        case .campus, .eventsCalendar: .orange
        case .debts: .red
        case .internationalGuide, .contacts: MauTheme.violet
        }
    }

    var subtitle: String {
        return switch self {
        case .eios: "Курсы и задания"
        case .forms: "Справки и обращения"
        case .messenger: "Сообщения в ЭИОС"
        case .campus: "Корпуса и маршруты"
        case .debts: "Проверка успеваемости"
        case .digital: "Почта, ЭИОС, библиотека"
        case .studentGuide: "Гид по разделу «Студенту»"
        case .applicantGuide: "Поступление и приёмная"
        case .scienceGuide: "Наука и исследования"
        case .internationalGuide: "English and international"
        case .teachers: "Поиск по имени"
        case .departments: "Телефоны и кабинеты"
        case .contacts: "Приёмка и платежи"
        case .eventsCalendar: "Анонсы пресс-центра"
        }
    }
}

private enum ServiceCategory: String, CaseIterable, Identifiable {
    case services = "Услуги"
    case portal = "Портал МАУ"
    case directories = "Справочники"
    var id: String { rawValue }

    var services: [MauService] {
        return switch self {
        case .services: [.eios, .forms, .messenger, .campus, .debts, .digital]
        case .portal: [.studentGuide, .applicantGuide, .scienceGuide, .internationalGuide]
        case .directories: [.teachers, .departments, .contacts, .eventsCalendar]
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
                    VStack(alignment: .leading, spacing: 6) {
                        Text("Сервисы").font(.system(size: 36, weight: .bold, design: .rounded))
                        Text("Учебные сервисы и разделы сайта МАУ")
                            .font(.subheadline)
                            .foregroundStyle(MauTheme.muted)
                    }

                    ForEach(ServiceCategory.allCases) { category in
                        MauSectionHeader(title: category.rawValue)
                        LazyVGrid(columns: columns, spacing: 13) {
                            ForEach(category.services) { service in
                                NavigationLink(destination: ServiceDestination(service: service)) {
                                    VStack(alignment: .leading, spacing: 16) {
                                        HStack {
                                            IconTile(systemName: service.icon, color: service.color)
                                            Spacer()
                                            Image(systemName: "arrow.up.right")
                                                .font(.caption.bold())
                                                .foregroundStyle(MauTheme.muted)
                                        }
                                        VStack(alignment: .leading, spacing: 4) {
                                            Text(service.title)
                                                .font(.subheadline.weight(.semibold))
                                                .foregroundStyle(MauTheme.ink)
                                                .multilineTextAlignment(.leading)
                                                .lineLimit(2)
                                            Text(service.subtitle)
                                                .font(.caption2)
                                                .foregroundStyle(MauTheme.muted)
                                                .lineLimit(1)
                                        }
                                        .frame(maxWidth: .infinity, minHeight: 42, alignment: .topLeading)
                                    }
                                    .padding(16)
                                    .frame(maxWidth: .infinity, minHeight: 135, alignment: .topLeading)
                                }
                                .buttonStyle(.plain)
                                .mauSurface(radius: 23)
                            }
                        }
                    }
                }
                .padding(20)
                .padding(.bottom, 96)
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
            WebServiceView(title: service.title, url: UniversityPortalURLs.eios)
        case .digital:
            DigitalServicesView()
        case .messenger:
            WebServiceView(
                title: service.title,
                url: "https://eios.mauniver.ru/moodle/message/index.php"
            )
        case .forms:
            FormsView()
        case .campus:
            CampusNavigatorView()
        case .debts:
            DebtsView()
        case .teachers:
            TeacherContactsView()
        case .departments:
            DepartmentsView()
        case .studentGuide:
            PortalGuideView(
                title: "Студенту",
                subtitle: "Офис, FAQ, общежитие и поддержка",
                sections: UniversityPortalCatalog.studentSections
            )
        case .applicantGuide:
            PortalGuideView(
                title: "Абитуриенту",
                subtitle: "Поступление, экзамены и контакты приёмной",
                sections: UniversityPortalCatalog.applicantSections
            )
        case .scienceGuide:
            PortalGuideView(
                title: "Наука",
                subtitle: "Направления, гранты и публикации МАУ",
                sections: UniversityPortalCatalog.scienceSections
            )
        case .internationalGuide:
            PortalGuideView(
                title: "International",
                subtitle: "English site and international activity",
                sections: UniversityPortalCatalog.internationalSections
            )
        case .contacts:
            UniversityContactsView()
        case .eventsCalendar:
            EventsCalendarView()
        }
    }
}

private struct WebServiceView: View {
    let title: String
    let url: String

    var body: some View {
        if let destination = URL(string: url) {
            InAppBrowserView(url: destination, title: title)
        } else {
            EmptyState(icon: "link.badge.plus", title: "Ссылка недоступна", message: url)
        }
    }
}

private struct FormsView: View {
    private let forms = [
        OnlineFormLink(
            title: "Справка об обучении",
            subtitle: "Обычная, гербовая или электронная",
            icon: "doc.badge.plus",
            url: "https://mauniver.ru/services/student/"
        ),
        OnlineFormLink(
            title: "Справка для перевода",
            subtitle: "Перечень дисциплин и оценок",
            icon: "doc.text.fill",
            url: "https://mauniver.ru/services/student/perevod/"
        ),
        OnlineFormLink(
            title: "Справка о стипендии",
            subtitle: "Выплаты за выбранный период",
            icon: "banknote.fill",
            url: "https://mauniver.ru/services/student/spravka/"
        ),
        OnlineFormLink(
            title: "Архивная справка",
            subtitle: "Для выпускников и бывших студентов",
            icon: "archivebox.fill",
            url: "https://mauniver.ru/services/student/archive/"
        ),
        OnlineFormLink(
            title: "Архивная справка для отчисленных",
            subtitle: "С перечнем дисциплин и оценок",
            icon: "doc.on.doc.fill",
            url: "https://mauniver.ru/services/student/archive-expl/"
        ),
        OnlineFormLink(
            title: "Справка-вызов",
            subtitle: "Для предоставления работодателю",
            icon: "briefcase.fill",
            url: "https://mauniver.ru/services/student/vyzov/"
        ),
        OnlineFormLink(
            title: "Справка для налоговой",
            subtitle: "Для социального налогового вычета",
            icon: "checkmark.seal.fill",
            url: "https://mauniver.ru/services/student/nalog/"
        ),
        OnlineFormLink(
            title: "Дубликат диплома",
            subtitle: "Заявление на повторную выдачу",
            icon: "doc.on.doc.fill",
            url: "https://mauniver.ru/services/student/diplom/"
        ),
        OnlineFormLink(
            title: "Счёт за обучение",
            subtitle: "Платные образовательные услуги",
            icon: "creditcard.fill",
            url: "https://mauniver.ru/services/student/application/"
        ),
        OnlineFormLink(
            title: "Онлайн-сервисы ММРК",
            subtitle: "Справки и заявления колледжа",
            icon: "building.columns.fill",
            url: "https://mauniver.ru/structure/branches/mmrc/online/"
        ),
        OnlineFormLink(
            title: "Справочная студенческого офиса",
            subtitle: "Задать вопрос об учебном процессе",
            icon: "questionmark.bubble.fill",
            url: "https://mauniver.ru/services/virtual/"
        ),
        OnlineFormLink(
            title: "Справочная библиотеки",
            subtitle: "Получить помощь библиотекаря",
            icon: "books.vertical.fill",
            url: "https://mauniver.ru/structure/divs/library/guide/"
        ),
        OnlineFormLink(
            title: "Виртуальная приёмная ректора",
            subtitle: "Направить официальное обращение",
            icon: "envelope.badge.fill",
            url: "https://mauniver.ru/rector/reception/"
        ),
        OnlineFormLink(
            title: "Вопрос приёмной комиссии",
            subtitle: "Обращение по вопросам поступления",
            icon: "person.crop.circle.badge.questionmark",
            url: "https://mauniver.ru/abit/reception/"
        ),
        OnlineFormLink(
            title: "Стать волонтёром МАУ",
            subtitle: "Присоединиться к волонтёрскому движению",
            icon: "heart.fill",
            url: "https://mauniver.ru/student/community/volunteer/"
        ),
        OnlineFormLink(
            title: "Поддержка молодых семей",
            subtitle: "Направить обращение и документы",
            icon: "figure.2.and.child.holdinghands",
            url: "https://mauniver.ru/services/student/material/"
        ),
        OnlineFormLink(
            title: "Поддержка участников СВО",
            subtitle: "Единое окно поддержки студентов",
            icon: "shield.fill",
            url: "https://mauniver.ru/services/student/support-svo/"
        ),
        OnlineFormLink(
            title: "Обратная связь «Моё образование»",
            subtitle: "Исправление данных в ГИС СЦОС",
            icon: "graduationcap.fill",
            url: "https://mauniver.ru/services/student/gis-scos/"
        ),
        OnlineFormLink(
            title: "Все официальные формы",
            subtitle: "Актуальный каталог онлайн-сервисов МАУ",
            icon: "safari.fill",
            url: "https://mauniver.ru/services/student/"
        )
    ]

    var body: some View {
        ZStack {
            MauBackground()
            ScrollView {
                VStack(spacing: 13) {
                    ForEach(forms) { form in
                        NavigationLink(destination: destination(for: form)) {
                            HStack(spacing: 14) {
                                IconTile(systemName: form.icon)
                                VStack(alignment: .leading, spacing: 3) {
                                    Text(form.title).font(.headline)
                                    Text(form.subtitle)
                                        .font(.caption)
                                        .foregroundStyle(MauTheme.muted)
                                }
                                Spacer()
                                Image(systemName: "chevron.right").foregroundStyle(MauTheme.muted)
                            }
                            .foregroundStyle(MauTheme.ink)
                            .padding(17)
                        }
                        .buttonStyle(.plain)
                        .mauGlass(radius: 22)
                    }
                    Text("Все страницы открываются во внутреннем браузере MAUverse.")
                        .font(.footnote)
                        .foregroundStyle(MauTheme.muted)
                        .padding()
                }
                .padding(20)
            }
        }
        .navigationTitle("Онлайн-формы")
        .navigationBarTitleDisplayMode(.large)
        .toolbar(.hidden, for: .tabBar)
    }

    @ViewBuilder
    private func destination(for form: OnlineFormLink) -> some View {
        if let value = form.url, let url = URL(string: value) {
            InAppBrowserView(url: url, title: form.title)
        } else {
            EmptyState(icon: "link.badge.plus", title: "Ссылка недоступна", message: form.title)
        }
    }
}

private struct OnlineFormLink: Identifiable {
    let title: String
    let subtitle: String
    let icon: String
    let url: String?
    var id: String { title }
}

private struct CertificateRequestView: View {
    @EnvironmentObject private var session: SessionStore
    @Environment(\.dismiss) private var dismiss

    @State private var fullName = ""
    @State private var email = ""
    @State private var phone = ""
    @State private var birthDate = Date(timeIntervalSince1970: 946_684_800)
    @State private var institute = ""
    @State private var studyForm = ""
    @State private var funding = ""
    @State private var certificateType = ""
    @State private var copies = 1
    @State private var delivery = ""
    @State private var postalAddress = ""
    @State private var universityEmail = ""
    @State private var comment = ""
    @State private var consent = false
    @State private var isSending = false
    @State private var errorMessage: String?
    @State private var sent = false

    private let institutes = [
        "Морская академия",
        "Институт прикладных арктических технологий",
        "Естественно-технологический институт",
        "Институт педагогики и психологии",
        "Институт креативных индустрий и предпринимательства",
        "Институт интеллектуальных систем и цифровых технологий",
        "Институт гуманитарных и социальных наук",
        "Медико-биологический институт",
        "Юридический факультет",
        "Факультет физической культуры и спорта"
    ]
    private let studyForms = ["Очная", "Заочная", "Очно-заочная"]
    private let fundingSources = ["Бюджет", "Договор"]
    private let certificateTypes = [
        "Простая, с печатью Студенческого офиса",
        "Гербовая, с гербовой печатью Университета",
        "Электронная, с электронной подписью"
    ]
    private let deliveryMethods = [
        "Лично: Спортивная, 13, кабинет 203В",
        "Лично: Егорова, 16, кабинет 112",
        "Почта России",
        "Электронная почта в домене @mauniver.ru"
    ]

    private var isPostalDelivery: Bool { delivery == deliveryMethods[2] }
    private var isEmailDelivery: Bool { delivery == deliveryMethods[3] }

    var body: some View {
        Form {
            Section("Контактные данные") {
                TextField("ФИО как в паспорте", text: $fullName)
                    .textContentType(.name)
                TextField("Контактный e-mail", text: $email)
                    .keyboardType(.emailAddress)
                    .textInputAutocapitalization(.never)
                TextField("Телефон", text: $phone)
                    .keyboardType(.phonePad)
                DatePicker(
                    "Дата рождения",
                    selection: $birthDate,
                    in: Date(timeIntervalSince1970: -1_262_304_000)...Date(),
                    displayedComponents: .date
                )
            }

            Section("Учебные данные") {
                selectionPicker("Факультет или институт", selection: $institute, values: institutes)
                selectionPicker("Форма обучения", selection: $studyForm, values: studyForms)
                selectionPicker("Источник финансирования", selection: $funding, values: fundingSources)
            }

            Section("Документ") {
                selectionPicker("Вид справки", selection: $certificateType, values: certificateTypes)
                    .onChange(of: certificateType) { _, value in
                        if value == certificateTypes[2] { delivery = deliveryMethods[3] }
                    }
                Stepper("Количество экземпляров: \(copies)", value: $copies, in: 1...5)
                selectionPicker("Способ получения", selection: $delivery, values: deliveryMethods)
                    .disabled(certificateType == certificateTypes[2])
                if isPostalDelivery {
                    TextField("Полный почтовый адрес", text: $postalAddress, axis: .vertical)
                        .lineLimit(2...4)
                }
                if isEmailDelivery {
                    TextField("Адрес @mauniver.ru", text: $universityEmail)
                        .keyboardType(.emailAddress)
                        .textInputAutocapitalization(.never)
                }
                TextField("Комментарий (необязательно)", text: $comment, axis: .vertical)
                    .lineLimit(2...5)
            }

            Section {
                Toggle("Согласие на обработку персональных данных", isOn: $consent)
                if let errorMessage {
                    Text(errorMessage).font(.footnote).foregroundStyle(.red)
                }
                Button {
                    Task { await submit() }
                } label: {
                    HStack {
                        Spacer()
                        if isSending { ProgressView() }
                        Text(isSending ? "Отправляем…" : "Отправить заявку")
                            .fontWeight(.semibold)
                        Spacer()
                    }
                }
                .disabled(isSending)
            }
        }
        .navigationTitle("Заказ справки")
        .navigationBarTitleDisplayMode(.inline)
        .toolbar(.hidden, for: .tabBar)
        .onAppear {
            if fullName.isEmpty { fullName = session.user?.displayName ?? "" }
        }
        .alert("Заявка отправлена", isPresented: $sent) {
            Button("Готово") { dismiss() }
        } message: {
            Text("Студенческий офис получил вашу заявку.")
        }
    }

    private func selectionPicker(
        _ title: String,
        selection: Binding<String>,
        values: [String]
    ) -> some View {
        Picker(title, selection: selection) {
            Text("Не выбрано").tag("")
            ForEach(values, id: \.self) { Text($0).tag($0) }
        }
    }

    @MainActor
    private func submit() async {
        errorMessage = validate()
        guard errorMessage == nil else { return }
        isSending = true
        defer { isSending = false }

        var fields = [
            StudentFormField(title: "ФИО (как в паспорте)", value: fullName.trimmed),
            StudentFormField(title: "E-mail", value: email.trimmed),
            StudentFormField(title: "Телефон", value: phone.trimmed),
            StudentFormField(
                title: "Дата рождения",
                value: birthDate.formatted(.dateTime.locale(Locale(identifier: "ru_RU")).day().month().year())
            ),
            StudentFormField(title: "Факультет / институт", value: institute),
            StudentFormField(title: "Форма обучения", value: studyForm),
            StudentFormField(title: "Источник финансирования", value: funding),
            StudentFormField(title: "Вид справки", value: certificateType),
            StudentFormField(title: "Количество справок", value: String(copies)),
            StudentFormField(title: "Способ получения документа", value: delivery)
        ]
        if isPostalDelivery {
            fields.append(StudentFormField(title: "Почтовый адрес", value: postalAddress.trimmed))
        }
        if isEmailDelivery {
            fields.append(StudentFormField(
                title: "Электронный адрес в домене @mauniver.ru",
                value: universityEmail.trimmed
            ))
        }
        if !comment.trimmed.isEmpty {
            fields.append(StudentFormField(title: "Комментарий", value: comment.trimmed))
        }

        do {
            let response: SuccessResponse = try await APIClient.shared.post(
                "send_order",
                body: StudentFormRequest(sender: email.trimmed, username: fullName.trimmed, text: fields),
                user: session.user
            )
            guard response.success == true else {
                throw APIError.server(response.detail ?? response.error ?? "Сервер не подтвердил отправку")
            }
            sent = true
        } catch {
            errorMessage = error.localizedDescription
        }
    }

    private func validate() -> String? {
        if fullName.trimmed.split(separator: " ").count < 2 { return "Укажите полное ФИО" }
        if !email.trimmed.isEmail { return "Проверьте контактный e-mail" }
        if !(10...15).contains(phone.filter(\.isNumber).count) { return "Проверьте номер телефона" }
        if institute.isEmpty { return "Выберите факультет или институт" }
        if studyForm.isEmpty { return "Выберите форму обучения" }
        if funding.isEmpty { return "Выберите источник финансирования" }
        if certificateType.isEmpty { return "Выберите вид справки" }
        if delivery.isEmpty { return "Выберите способ получения" }
        if isPostalDelivery && postalAddress.trimmed.isEmpty { return "Укажите почтовый адрес" }
        if isEmailDelivery && !universityEmail.trimmed.lowercased().hasSuffix("@mauniver.ru") {
            return "Укажите корректный адрес @mauniver.ru"
        }
        if comment.count > 2_000 { return "Комментарий не должен превышать 2000 символов" }
        if !consent { return "Подтвердите согласие на обработку данных" }
        return nil
    }
}

@MainActor
private final class DebtsModel: ObservableObject {
    @Published var semesters: [Semester] = []
    @Published var isLoading = false
    @Published var error: String?

    func load(user: UserDTO?) async {
        guard let user,
              let creditBook = user.creditBook?.trimmed,
              !creditBook.isEmpty else {
            error = "Укажите номер зачётной книжки в профиле"
            semesters = []
            return
        }
        isLoading = true
        error = nil
        defer { isLoading = false }
        do {
            let response: SemesterResponse = try await APIClient.shared.post(
                "get_semesters",
                body: DebtRequest(creditBook: creditBook),
                user: user,
                retryOnTransient: true
            )
            if let message = response.error ?? response.detail {
                throw APIError.server(message)
            }
            var loaded = response.semesters ?? []
            for index in loaded.indices {
                guard let number = loaded[index].semesterNumber else { continue }
                let debts: DebtResponse = try await APIClient.shared.post(
                    "get_debts",
                    body: DebtRequest(creditBook: creditBook, semesterNumber: number),
                    user: user,
                    retryOnTransient: true
                )
                if let message = debts.error ?? debts.detail {
                    throw APIError.server(message)
                }
                loaded[index].debts = debts.debts ?? []
            }
            semesters = loaded
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
                        VStack(spacing: 12) {
                            EmptyState(icon: "exclamationmark.circle", title: "Нет данных", message: error)
                            Button("Повторить") { Task { await model.load(user: session.user) } }
                                .buttonStyle(.borderedProminent)
                        }
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
                .padding(.bottom, 30)
            }
            .refreshable { await model.load(user: session.user) }
        }
        .navigationTitle("Задолженности")
        .toolbar(.hidden, for: .tabBar)
        .task { await model.load(user: session.user) }
    }
}

@MainActor
private final class TeachersModel: ObservableObject {
    @Published var teachers: [Teacher] = []
    @Published var isLoading = false
    @Published var error: String?
    private var activeRequest = UUID()

    func search(_ text: String, user: UserDTO?) async {
        let request = UUID()
        activeRequest = request
        let query = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard query.count >= 2 else {
            teachers = []
            error = "Введите минимум две буквы фамилии"
            return
        }
        isLoading = true
        error = nil
        defer {
            if activeRequest == request { isLoading = false }
        }
        do {
            let loaded = try await ScheduleAPIClient.shared.teachers(matching: query).map {
                Teacher(
                    id: $0.teacherId,
                    teacherId: $0.teacherId,
                    name: $0.teacher,
                    fullName: $0.teacher,
                    email: nil,
                    phone: nil,
                    post: "Преподаватель МАУ",
                    extras: $0.uid
                )
            }
            guard activeRequest == request else { return }
            teachers = loaded
        } catch {
            guard activeRequest == request else { return }
            self.error = error.localizedDescription
        }
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
                        .submitLabel(.search)
                        .onSubmit { Task { await model.search(query, user: session.user) } }
                    Button { Task { await model.search(query, user: session.user) } } label: {
                        Image(systemName: "magnifyingglass").font(.headline)
                    }
                    .disabled(model.isLoading)
                }
                .padding(16)
                .mauGlass(radius: 20)

                if model.isLoading { LoadingOverlay(title: "Ищем") }
                else if let error = model.error { EmptyState(icon: "wifi.exclamationmark", title: "Ошибка", message: error) }
                else if query.isEmpty {
                    EmptyState(
                        icon: "person.text.rectangle",
                        title: "Найдите преподавателя",
                        message: "Введите фамилию или часть имени"
                    )
                } else if model.teachers.isEmpty {
                    EmptyState(icon: "person.slash", title: "Ничего не найдено", message: "Уточните запрос")
                }
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
                    .scrollDismissesKeyboard(.interactively)
                }
            }
            .padding(20)
        }
        .navigationTitle("Преподаватели")
        .toolbar(.hidden, for: .tabBar)
    }
}

@MainActor
private final class DepartmentsModel: ObservableObject {
    @Published var departments: [Department] = []
    @Published var contacts: [Telephone] = []
    @Published var isLoading = false
    @Published var isLoadingContacts = false
    @Published var error: String?
    private var activeContactsRequest = UUID()

    func load(user: UserDTO?) async {
        isLoading = true
        error = nil
        defer { isLoading = false }
        do {
            let loaded: [Department] = try await APIClient.shared.get("get_depts_json", user: user)
            departments = loaded.filter { $0.id != nil && $0.name?.trimmed.isEmpty == false }
        }
        catch { self.error = error.localizedDescription }
    }

    func contacts(for department: Department, user: UserDTO?) async {
        let request = UUID()
        activeContactsRequest = request
        contacts = []
        error = nil
        isLoadingContacts = true
        defer {
            if activeContactsRequest == request { isLoadingContacts = false }
        }
        do {
            let loaded: [Telephone] = try await APIClient.shared.post(
                "get_contacts_json",
                body: DepartmentRequest(departmentId: department.id ?? 0, name: department.name ?? ""),
                user: user,
                retryOnTransient: true
            )
            guard activeContactsRequest == request else { return }
            contacts = loaded
        } catch {
            guard activeContactsRequest == request else { return }
            self.error = error.localizedDescription
        }
    }
}

private struct DepartmentsView: View {
    @EnvironmentObject private var session: SessionStore
    @StateObject private var model = DepartmentsModel()
    @State private var selected: Department?

    var body: some View {
        ZStack {
            MauBackground()
            if model.isLoading && model.departments.isEmpty {
                LoadingOverlay(title: "Загружаем подразделения")
            } else if let error = model.error, model.departments.isEmpty {
                VStack(spacing: 12) {
                    EmptyState(icon: "wifi.exclamationmark", title: "Ошибка загрузки", message: error)
                    Button("Повторить") { Task { await model.load(user: session.user) } }
                        .buttonStyle(.borderedProminent)
                }
                .padding(20)
            } else if model.departments.isEmpty {
                EmptyState(
                    icon: "building.2",
                    title: "Подразделения не найдены",
                    message: "Справочник МАУ пока пуст"
                )
                .padding(20)
            } else {
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
                .refreshable { await model.load(user: session.user) }
                .scrollContentBackground(.hidden)
            }
        }
        .navigationTitle("Подразделения")
        .toolbar(.hidden, for: .tabBar)
        .task { await model.load(user: session.user) }
        .sheet(item: $selected) { department in
            NavigationStack {
                ScrollView {
                    LazyVStack(spacing: 12) {
                        if model.isLoadingContacts {
                            LoadingOverlay(title: "Загружаем контакты")
                        } else if let error = model.error {
                            EmptyState(icon: "wifi.exclamationmark", title: "Ошибка", message: error)
                            Button("Повторить") {
                                Task { await model.contacts(for: department, user: session.user) }
                            }
                            .buttonStyle(.borderedProminent)
                        } else if model.contacts.isEmpty {
                            EmptyState(
                                icon: "phone.down",
                                title: "Контакты не найдены",
                                message: "У подразделения пока нет опубликованных контактов"
                            )
                        }
                        ForEach(model.contacts) { contact in
                            ContactCard(
                                title: contact.person ?? contact.title ?? "Контакт",
                                subtitle: [contact.building2, contact.room].compactMap { $0 }.joined(separator: ", "),
                                phone: contact.phone,
                                phone2: contact.phone2,
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
    var phone2: String? = nil
    let email: String?

    var body: some View {
        VStack(alignment: .leading, spacing: 8) {
            Text(title).font(.headline)
            if let subtitle, !subtitle.isEmpty {
                Text(subtitle).font(.caption).foregroundStyle(MauTheme.muted)
            }
            HStack {
                if let phone, let destination = phoneURL(phone) {
                    Link(destination: destination) {
                        Label(phone, systemImage: "phone.fill")
                    }
                }
                if let phone2, let destination = phoneURL(phone2) {
                    Link(destination: destination) {
                        Label(phone2, systemImage: "phone.fill")
                    }
                }
                if let email, let url = URL(string: "mailto:\(email.trimmed)") {
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

    private func phoneURL(_ value: String) -> URL? {
        PhoneNumberFormatting.telURL(from: value)
    }
}

private extension String {
    var trimmed: String { trimmingCharacters(in: .whitespacesAndNewlines) }

    var isEmail: Bool {
        let parts = trimmed.split(separator: "@", omittingEmptySubsequences: false)
        return parts.count == 2 && !parts[0].isEmpty && parts[1].contains(".")
    }
}
