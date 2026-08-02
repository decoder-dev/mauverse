using mau.Models;
using mau.Resources.Fonts;

namespace mau.Utils.Services;

public static class UniversityGuideCatalog
{
    public static IReadOnlyList<UniversityGuideSection> StudentSections { get; } =
    [
        new(
            "Учёба и сервис",
            [
                Link(
                    "Студенческий офис",
                    "Справки, перевод, восстановление и продление билета",
                    UniversityPortalUrls.StudentOffice,
                    FluentUI.building_home_24_regular),
                Link(
                    "Частые вопросы",
                    "Стипендии, сессия, общежитие и учебный процесс",
                    UniversityPortalUrls.StudentFaq,
                    FluentUI.question_24_regular),
                Link(
                    "Путеводитель первокурсника",
                    "Что важно знать в начале обучения",
                    UniversityPortalUrls.StudentGuide,
                    FluentUI.book_compass_24_regular),
                Link(
                    "Расписание на сайте",
                    "Официальное расписание учебных занятий",
                    UniversityPortalUrls.StudentTimetable,
                    FluentUI.calendar_ltr_24_regular)
            ]),
        new(
            "Жизнь в университете",
            [
                Link(
                    "Студенческие объединения",
                    "Наука, спорт, волонтёрство и творчество",
                    UniversityPortalUrls.StudentCommunity,
                    FluentUI.people_community_24_regular),
                Link(
                    "Календарь событий",
                    "Мероприятия и анонсы пресс-центра МАУ",
                    UniversityPortalUrls.EventsCalendar,
                    FluentUI.calendar_ltr_24_regular),
                Link(
                    "Карьера и трудоустройство",
                    "Центр карьеры и партнёры университета",
                    UniversityPortalUrls.CareerCenter,
                    FluentUI.briefcase_24_regular),
                Link(
                    "Общежитие",
                    "Правила заселения и проживания",
                    UniversityPortalUrls.HostelRules,
                    FluentUI.bed_24_regular)
            ]),
        new(
            "Поддержка и обращения",
            [
                Link(
                    "Виртуальная справочная",
                    "Вопросы студенческому офису онлайн",
                    UniversityPortalUrls.VirtualHelpdesk,
                    FluentUI.person_feedback_24_regular),
                Link(
                    "Вопросы ректору",
                    "Виртуальная приёмная ректора",
                    UniversityPortalUrls.RectorReception,
                    FluentUI.mail_inbox_24_regular),
                Link(
                    "Поддержка семей и СВО",
                    "Меры социальной поддержки обучающихся",
                    UniversityPortalUrls.SocialSupport,
                    FluentUI.heart_24_regular),
                Link(
                    "Электронный каталог библиотеки",
                    "Поиск книг и электронных ресурсов МАУ",
                    UniversityPortalUrls.LibraryCatalog,
                    FluentUI.library_24_regular)
            ])
    ];

    public static IReadOnlyList<UniversityGuideSection> ApplicantSections { get; } =
    [
        new(
            "Поступление",
            [
                Link(
                    "Приёмная кампания",
                    "Программы, квоты и подача документов",
                    UniversityPortalUrls.AdmissionPortal,
                    FluentUI.hat_graduation_24_regular),
                Link(
                    "Правила приёма",
                    "Условия, льготы и особые права",
                    UniversityPortalUrls.AdmissionRules,
                    FluentUI.document_ribbon_24_regular),
                Link(
                    "Образовательные программы",
                    "Направления, стоимость и планы приёма",
                    UniversityPortalUrls.AdmissionPrograms,
                    FluentUI.book_open_24_regular),
                Link(
                    "Расписание экзаменов",
                    "Консультации и вступительные испытания",
                    UniversityPortalUrls.AdmissionExams,
                    FluentUI.calendar_ltr_24_regular)
            ]),
        new(
            "Помощь абитуриенту",
            [
                Link(
                    "Новости приёмной комиссии",
                    "Списки, приказы и важные даты",
                    UniversityPortalUrls.AdmissionNews,
                    FluentUI.megaphone_loud_24_regular),
                Link(
                    "Дни открытых дверей",
                    "Экскурсии и встречи с институтами",
                    UniversityPortalUrls.OpenDays,
                    FluentUI.building_home_24_regular),
                Link(
                    "FAQ абитуриента",
                    "Ответы на частые вопросы о поступлении",
                    UniversityPortalUrls.AdmissionFaq,
                    FluentUI.question_24_regular),
                Link(
                    "Иностранным абитуриентам",
                    "Поступление для граждан других стран",
                    UniversityPortalUrls.ForeignApplicants,
                    FluentUI.globe_24_regular)
            ]),
        new(
            "Контакты",
            [
                Link(
                    "Приёмная комиссия",
                    "Адреса, телефоны и график работы",
                    UniversityPortalUrls.AdmissionContacts,
                    FluentUI.call_24_regular),
                Link(
                    "Задать вопрос",
                    "Консультация по поступлению",
                    UniversityPortalUrls.AdmissionReception,
                    FluentUI.person_feedback_24_regular),
                Link(
                    "Общежитие для поступивших",
                    "Как забронировать место",
                    UniversityPortalUrls.HostelRules,
                    FluentUI.bed_24_regular),
                Link(
                    "Филиалы и колледжи",
                    "Апатиты, Кировск, Полярный и колледжи МАУ",
                    UniversityPortalUrls.Branches,
                    FluentUI.building_multiple_24_regular)
            ])
    ];

    static UniversityGuideLink Link(string title, string description, string url, string glyph) =>
        new(title, description, url, glyph);
}
