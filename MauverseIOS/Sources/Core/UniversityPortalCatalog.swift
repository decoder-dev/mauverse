import Foundation

enum UniversityPortalURLs {
    static let studentOffice = "https://mauniver.ru/structure/divs/studof/"
    static let studentFaq = "https://mauniver.ru/student/faq/"
    static let studentGuide = "https://mauniver.ru/student/guide/"
    static let studentTimetable = "https://mauniver.ru/student/timetable/"
    static let studentCommunity = "https://mauniver.ru/student/community/"
    static let eventsCalendar = "https://mauniver.ru/press/calendar/"
    static let careerCenter = "https://mauniver.ru/abit/career/"
    static let hostelRules = "https://mauniver.ru/abit/rules/hostel/"
    static let virtualHelpdesk = "https://mauniver.ru/services/virtual/"
    static let rectorReception = "https://mauniver.ru/rector/reception/"
    static let socialSupport = "https://mauniver.ru/info/docs/ump/"
    static let libraryCatalog = "https://lib.mauniver.ru/MegaPro/Web"

    static let admissionPortal = "https://priem.mauniver.ru/"
    static let admissionRules = "https://mauniver.ru/abit/rules/"
    static let admissionPrograms = "https://mauniver.ru/abit/admission/"
    static let admissionExams = "https://mauniver.ru/abit/exam/"
    static let admissionNews = "https://mauniver.ru/abit/news/"
    static let openDays = "https://mauniver.ru/abit/news/open/"
    static let admissionFaq = "https://mauniver.ru/abit/faq/"
    static let foreignApplicants = "https://mauniver.ru/abit/foreign/"
    static let admissionContacts = "https://mauniver.ru/abit/contact/"
    static let admissionReception = "https://mauniver.ru/abit/reception/"
    static let branches = "https://mauniver.ru/structure/branches/"

    static let scienceHome = "https://mauniver.ru/science/"
    static let scienceNews = "https://mauniver.ru/science/news/"
    static let scienceFields = "https://mauniver.ru/science/fields/"
    static let scienceDigest = "https://mauniver.ru/science/events/digest/"
    static let scienceGrants = "https://mauniver.ru/science/help/grants/"
    static let sciencePublishing = "https://mauniver.ru/science/izdat/"
    static let scienceHelp = "https://mauniver.ru/science/help/"
    static let scienceInnovations = "https://mauniver.ru/science/innovations/"

    static let internationalActivity = "https://mauniver.ru/info/inter/"
    static let englishSite = "https://mauniver.ru/en/"
    static let diplomaSupplement = "https://mauniver.ru/info/inter/diplom"

    static let digitalStudentServices = "https://www.mauniver.ru/services/student/"
    static let webmail = "https://webmail.mauniver.ru/mail/"
    static let dormItRequest = "https://serviceahd.mauniver.ru/index.php?a=add&category=21"
    static let intra = "https://intra.mauniver.ru/"
    static let promt = "https://promt.mauniver.ru/ptf"
    static let eios = "https://eios.mauniver.ru/moodle/"
    static let requisites = "https://mauniver.ru/info/address/"
    static let officialSite = "https://mauniver.ru/"
    static let privacyPolicy = "https://mauniver.ru/info/docs/pdn/"
    static let sveden = "https://mauniver.ru/sveden/"
    static let campusNavigatorSite = "https://mauniver.ru/student/campus/"
}

struct PortalLink: Identifiable, Hashable {
    let title: String
    let subtitle: String
    let url: String
    let systemImage: String
    var id: String { url + title }
}

struct PortalSection: Identifiable, Hashable {
    let title: String
    let links: [PortalLink]
    var id: String { title }
}

struct UniversityContactBlock: Identifiable, Hashable {
    let title: String
    let details: String
    let phone: String?
    let email: String?
    let address: String?
    var id: String { title }
}

struct CampusBuilding: Identifiable, Hashable {
    let title: String
    let address: String
    let searchQuery: String
    let mapCity: String
    var id: String { title + address }
}

struct CampusBuildingGroup: Identifiable, Hashable {
    let title: String
    let transportTip: String
    let buildings: [CampusBuilding]
    var id: String { title }
}

enum UniversityPortalCatalog {
    static let studentSections: [PortalSection] = [
        PortalSection(title: "Учёба и сервис", links: [
            link("Студенческий офис", "Справки, перевод, восстановление и продление билета",
                 UniversityPortalURLs.studentOffice, "building.columns.fill"),
            link("Частые вопросы", "Стипендии, сессия, общежитие и учебный процесс",
                 UniversityPortalURLs.studentFaq, "questionmark.circle.fill"),
            link("Путеводитель первокурсника", "Что важно знать в начале обучения",
                 UniversityPortalURLs.studentGuide, "book.fill"),
            link("Расписание на сайте", "Официальное расписание учебных занятий",
                 UniversityPortalURLs.studentTimetable, "calendar")
        ]),
        PortalSection(title: "Жизнь в университете", links: [
            link("Студенческие объединения", "Наука, спорт, волонтёрство и творчество",
                 UniversityPortalURLs.studentCommunity, "person.3.fill"),
            link("Календарь событий", "Мероприятия и анонсы пресс-центра МАУ",
                 UniversityPortalURLs.eventsCalendar, "calendar.badge.clock"),
            link("Карьера и трудоустройство", "Центр карьеры и партнёры университета",
                 UniversityPortalURLs.careerCenter, "briefcase.fill"),
            link("Общежитие", "Правила заселения и проживания",
                 UniversityPortalURLs.hostelRules, "bed.double.fill")
        ]),
        PortalSection(title: "Поддержка и обращения", links: [
            link("Виртуальная справочная", "Вопросы студенческому офису онлайн",
                 UniversityPortalURLs.virtualHelpdesk, "bubble.left.and.bubble.right.fill"),
            link("Вопросы ректору", "Виртуальная приёмная ректора",
                 UniversityPortalURLs.rectorReception, "envelope.fill"),
            link("Поддержка семей и СВО", "Меры социальной поддержки обучающихся",
                 UniversityPortalURLs.socialSupport, "heart.fill"),
            link("Электронный каталог библиотеки", "Поиск книг и электронных ресурсов МАУ",
                 UniversityPortalURLs.libraryCatalog, "books.vertical.fill")
        ])
    ]

    static let applicantSections: [PortalSection] = [
        PortalSection(title: "Поступление", links: [
            link("Приёмная кампания", "Программы, квоты и подача документов",
                 UniversityPortalURLs.admissionPortal, "graduationcap.fill"),
            link("Правила приёма", "Условия, льготы и особые права",
                 UniversityPortalURLs.admissionRules, "doc.richtext.fill"),
            link("Образовательные программы", "Направления, стоимость и планы приёма",
                 UniversityPortalURLs.admissionPrograms, "book.closed.fill"),
            link("Расписание экзаменов", "Консультации и вступительные испытания",
                 UniversityPortalURLs.admissionExams, "calendar")
        ]),
        PortalSection(title: "Помощь абитуриенту", links: [
            link("Новости приёмной комиссии", "Списки, приказы и важные даты",
                 UniversityPortalURLs.admissionNews, "megaphone.fill"),
            link("Дни открытых дверей", "Экскурсии и встречи с институтами",
                 UniversityPortalURLs.openDays, "building.2.fill"),
            link("FAQ абитуриента", "Ответы на частые вопросы о поступлении",
                 UniversityPortalURLs.admissionFaq, "questionmark.circle.fill"),
            link("Иностранным абитуриентам", "Поступление для граждан других стран",
                 UniversityPortalURLs.foreignApplicants, "globe")
        ]),
        PortalSection(title: "Контакты", links: [
            link("Приёмная комиссия", "Адреса, телефоны и график работы",
                 UniversityPortalURLs.admissionContacts, "phone.fill"),
            link("Задать вопрос", "Консультация по поступлению",
                 UniversityPortalURLs.admissionReception, "text.bubble.fill"),
            link("Общежитие для поступивших", "Как забронировать место",
                 UniversityPortalURLs.hostelRules, "bed.double.fill"),
            link("Филиалы и колледжи", "Апатиты, Кировск, Полярный и колледжи МАУ",
                 UniversityPortalURLs.branches, "building.2.crop.circle")
        ])
    ]

    static let scienceSections: [PortalSection] = [
        PortalSection(title: "Наука МАУ", links: [
            link("Раздел «Наука»", "Главная страница научного блока университета",
                 UniversityPortalURLs.scienceHome, "atom"),
            link("Новости науки", "Анонсы исследований и научных событий",
                 UniversityPortalURLs.scienceNews, "megaphone.fill"),
            link("Научные направления", "Приоритетные области исследований МАУ",
                 UniversityPortalURLs.scienceFields, "compass.drawing"),
            link("Мероприятия и дайджест", "Конференции, семинары и научный дайджест",
                 UniversityPortalURLs.scienceDigest, "calendar")
        ]),
        PortalSection(title: "Исследователю", links: [
            link("Гранты", "Конкурсы и поддержка научных проектов",
                 UniversityPortalURLs.scienceGrants, "trophy.fill"),
            link("Помощь исследователю", "Инструкции, базы и доступ к ресурсам",
                 UniversityPortalURLs.scienceHelp, "questionmark.folder.fill"),
            link("Издательство", "Журналы и публикации МАУ",
                 UniversityPortalURLs.sciencePublishing, "book.fill"),
            link("Инновации и инфраструктура", "Площадки, лаборатории и инновации",
                 UniversityPortalURLs.scienceInnovations, "gearshape.2.fill")
        ])
    ]

    static let internationalSections: [PortalSection] = [
        PortalSection(title: "International", links: [
            link("English website", "English-language pages of Murmansk Arctic University",
                 UniversityPortalURLs.englishSite, "globe"),
            link("Международная деятельность", "Партнёрства, обмен и проекты МАУ",
                 UniversityPortalURLs.internationalActivity, "airplane"),
            link("Иностранным поступающим", "Поступление для граждан других стран",
                 UniversityPortalURLs.foreignApplicants, "graduationcap.fill"),
            link("Diploma Supplement", "Европейское приложение к диплому",
                 UniversityPortalURLs.diplomaSupplement, "doc.badge.ellipsis")
        ])
    ]

    static let digitalSections: [PortalSection] = [
        PortalSection(title: "Обучение и документы", links: [
            link("Цифровые сервисы студентов", "Справки и онлайн-заявки Студенческого офиса",
                 UniversityPortalURLs.digitalStudentServices, "building.columns.fill"),
            link("ЭИОС", "Электронная информационно-образовательная среда",
                 UniversityPortalURLs.eios, "graduationcap.fill"),
            link("Электронный каталог библиотеки", "Поиск книг и электронных ресурсов",
                 UniversityPortalURLs.libraryCatalog, "books.vertical.fill")
        ]),
        PortalSection(title: "Почта и поддержка", links: [
            link("Webmail МАУ", "Корпоративная почта @mauniver.ru",
                 UniversityPortalURLs.webmail, "envelope.fill"),
            link("Заявка по общежитию (УИТ)", "Интернет и ИТ-поддержка в общежитии",
                 UniversityPortalURLs.dormItRequest, "wifi"),
            link("Intra МАУ", "Внутренний портал для работников",
                 UniversityPortalURLs.intra, "person.2.fill"),
            link("PROMT Translation Factory", "Сервис перевода для направления «Лингвистика»",
                 UniversityPortalURLs.promt, "character.book.closed.fill")
        ])
    ]

    static let admissionContacts: [UniversityContactBlock] = [
        UniversityContactBlock(
            title: "Приёмная комиссия (ВО)",
            details: "Бакалавриат, специалитет, магистратура, аспирантура\nпн–пт 10:00–16:00, перерыв 13:00–14:00",
            phone: "8 800 350-12-21",
            email: "priem@mauniver.ru",
            address: "183010, г. Мурманск, пр. Кирова, д. 1, корпус Л, каб. 112"
        ),
        UniversityContactBlock(
            title: "Приёмная комиссия колледжа МАУ",
            details: "Среднее профессиональное образование\nпн–пт 10:00–16:00, перерыв 13:00–14:00",
            phone: "8 8152 21-38-72",
            email: "priem.spo@mauniver.ru",
            address: "183038, г. Мурманск, ул. Ленина, 57, каб. 107"
        ),
        UniversityContactBlock(
            title: "Филиал в г. Апатиты",
            details: "пн–пт 9:00–17:00, сб–вс выходной",
            phone: nil,
            email: "priem@arcticsu.ru",
            address: "184209, г. Апатиты, ул. Лесная, д. 29"
        ),
        UniversityContactBlock(
            title: "Филиал в г. Кировске",
            details: "пн–пт 9:00–17:00, сб–вс выходной",
            phone: "8 8153 15-54-08",
            email: "priem.kirovsk@mauniver.ru",
            address: "184250, г. Кировск, ул. 50 лет Октября, д. 2, каб. 1116"
        ),
        UniversityContactBlock(
            title: "Филиал в г. Полярный",
            details: "пн–пт 9:00–17:00, обед 13:00–14:00",
            phone: "8 8155 17-36-60",
            email: "priem.pf@mauniver.ru",
            address: "184651, г. Полярный, ул. Лунина, д. 5"
        )
    ]

    static let universityRequisites: [UniversityContactBlock] = [
        UniversityContactBlock(
            title: "Адреса МАУ",
            details: """
            Юридический адрес: 183010, г. Мурманск, ул. Спортивная, д. 13
            Почтовый адрес: 183038, г. Мурманск, ул. Капитана Егорова, д. 15
            Телефон: +7 (8152) 21-38-01
            Факс: +7 (8152) 45-27-52
            """,
            phone: "+7 (8152) 21-38-01",
            email: "office@mauniver.ru",
            address: "183010, г. Мурманск, ул. Спортивная, д. 13"
        ),
        UniversityContactBlock(
            title: "Реквизиты для платежей",
            details: """
            Получатель: УФК по Нижегородской области (ФГАОУ ВО «МАУ», л/сч 30496Ж46000)
            ИНН 5190100176
            КПП 519001001
            Казначейский счёт 03214643000000013212
            Банк: ОТЦ №1 ВВГУ Банка России // УФК по Нижегородской области, г. Нижний Новгород
            БИК 012202102
            Счёт ЕКС 40102810745370000024
            ОГРН 1025100848651
            В назначении платежа за обучение указывайте код 00000000000000000130, ФИО, факультет, специальность, курс.
            """,
            phone: nil,
            email: nil,
            address: nil
        )
    ]

    static let campusGroups: [CampusBuildingGroup] = [
        CampusBuildingGroup(
            title: "Южный кампус",
            transportTip: "Остановки: «МАУ», «переулок Хибинский». Автобусы и маршрутки по ул. Спортивной / Колхозной / Советской.",
            buildings: [
                campus("Корпус А", "ул. Спортивная, 13/6"),
                campus("Корпус Б", "ул. Колхозная, 2"),
                campus("Корпус В", "ул. Спортивная, 13"),
                campus("Корпус Г", "ул. Советская, 8А"),
                campus("Корпус Д", "ул. Советская, 8"),
                campus("Корпус Е", "ул. Советская, 12А"),
                campus("Корпус К", "ул. Спортивная, 9"),
                campus("Корпус Л1", "ул. Кирова, 1"),
                campus("Корпус Л2", "ул. Кирова, 1"),
                campus("Корпус М", "ул. Советская, 17"),
                campus("Корпус Н", "ул. Спортивная, 11"),
                campus("Корпус П", "ул. Советская, 10"),
                campus("Корпус С", "ул. Советская, 14"),
                campus("Корпус Э", "ул. Горького, 14"),
                campus("Столовая", "ул. Колхозная, 15А"),
                campus("КСК", "ул. Колхозная, 15")
            ]
        ),
        CampusBuildingGroup(
            title: "Северный кампус",
            transportTip: "Остановки: «Капитана Егорова», «Академика Книповича». Удобно добираться от ж/д вокзала и центра.",
            buildings: [
                campus("Е15", "ул. Капитана Егорова, 15"),
                campus("Е16", "ул. Капитана Егорова, 16"),
                campus("К9", "ул. Коммуны, 9"),
                campus("Л57", "пр. Ленина, 57")
            ]
        ),
        CampusBuildingGroup(
            title: "Филиалы",
            transportTip: "Апатиты, Кировск и Полярный — смотрите расписание пригородного транспорта и пропуска для ЗАТО Полярный.",
            buildings: [
                campus("Филиал в г. Апатиты", "ул. Лесная, 29", city: "Апатиты", mapCity: "apatity"),
                campus("Филиал в г. Кировске", "ул. 50 лет Октября, 2", city: "Кировск", mapCity: "kirovsk"),
                campus("Филиал в г. Полярный", "ул. Лунина, 5", city: "Полярный", mapCity: "murmansk")
            ]
        )
    ]

    private static func link(
        _ title: String,
        _ subtitle: String,
        _ url: String,
        _ systemImage: String
    ) -> PortalLink {
        PortalLink(title: title, subtitle: subtitle, url: url, systemImage: systemImage)
    }

    private static func campus(_ title: String, _ address: String) -> CampusBuilding {
        CampusBuilding(
            title: title,
            address: address,
            searchQuery: "МАУ \(title), \(address), Мурманск",
            mapCity: "murmansk"
        )
    }

    private static func campus(
        _ title: String,
        _ address: String,
        city: String,
        mapCity: String
    ) -> CampusBuilding {
        CampusBuilding(
            title: title,
            address: address,
            searchQuery: "МАУ \(title), \(address), \(city)",
            mapCity: mapCity
        )
    }
}
