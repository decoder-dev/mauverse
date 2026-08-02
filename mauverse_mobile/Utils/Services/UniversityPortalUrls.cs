namespace mau.Utils.Services;

/// <summary>
/// Stable HTTPS destinations mirrored from mauniver.ru for guides and unit tests.
/// </summary>
public static class UniversityPortalUrls
{
    public const string StudentOffice = "https://mauniver.ru/structure/divs/studof/";
    public const string StudentFaq = "https://mauniver.ru/student/faq/";
    public const string StudentGuide = "https://mauniver.ru/student/guide/";
    public const string StudentTimetable = "https://mauniver.ru/student/timetable/";
    public const string StudentCommunity = "https://mauniver.ru/student/community/";
    public const string EventsCalendar = "https://mauniver.ru/press/calendar/";
    public const string CareerCenter = "https://mauniver.ru/abit/career/";
    public const string HostelRules = "https://mauniver.ru/abit/rules/hostel/";
    public const string VirtualHelpdesk = "https://mauniver.ru/services/virtual/";
    public const string RectorReception = "https://mauniver.ru/rector/reception/";
    public const string SocialSupport = "https://mauniver.ru/info/docs/ump/";
    public const string LibraryCatalog = "https://lib.mauniver.ru/MegaPro/Web";

    public const string AdmissionPortal = "https://priem.mauniver.ru/";
    public const string AdmissionRules = "https://mauniver.ru/abit/rules/";
    public const string AdmissionPrograms = "https://mauniver.ru/abit/admission/";
    public const string AdmissionExams = "https://mauniver.ru/abit/exam/";
    public const string AdmissionNews = "https://mauniver.ru/abit/news/";
    public const string OpenDays = "https://mauniver.ru/abit/news/open/";
    public const string AdmissionFaq = "https://mauniver.ru/abit/faq/";
    public const string ForeignApplicants = "https://mauniver.ru/abit/foreign/";
    public const string AdmissionContacts = "https://mauniver.ru/abit/contact/";
    public const string AdmissionReception = "https://mauniver.ru/abit/reception/";
    public const string Branches = "https://mauniver.ru/structure/branches/";

    public const string ScienceHome = "https://mauniver.ru/science/";
    public const string ScienceNews = "https://mauniver.ru/science/news/";
    public const string ScienceFields = "https://mauniver.ru/science/fields/";
    public const string ScienceEvents = "https://mauniver.ru/science/events/";
    public const string ScienceDigest = "https://mauniver.ru/science/events/digest/";
    public const string ScienceGrants = "https://mauniver.ru/science/help/grants/";
    public const string SciencePublishing = "https://mauniver.ru/science/izdat/";
    public const string ScienceHelp = "https://mauniver.ru/science/help/";
    public const string ScienceInnovations = "https://mauniver.ru/science/innovations/";
    public const string ScienceStructure = "https://mauniver.ru/science/structure/";

    public const string InternationalActivity = "https://mauniver.ru/info/inter/";
    public const string EnglishSite = "https://mauniver.ru/en/";
    public const string DiplomaSupplement = "https://mauniver.ru/info/inter/diplom";

    public const string DigitalStudentServices = "https://www.mauniver.ru/services/student/";
    public const string Webmail = "https://webmail.mauniver.ru/mail/";
    public const string DormItRequest = "https://serviceahd.mauniver.ru/index.php?a=add&category=21";
    public const string Intra = "https://intra.mauniver.ru/";
    public const string Promt = "https://promt.mauniver.ru/ptf";
    public const string Eios = "https://eios.mauniver.ru/moodle/";
    public const string Requisites = "https://mauniver.ru/info/address/";

    public static IReadOnlyList<string> StudentUrls { get; } =
    [
        StudentOffice,
        StudentFaq,
        StudentGuide,
        StudentTimetable,
        StudentCommunity,
        EventsCalendar,
        CareerCenter,
        HostelRules,
        VirtualHelpdesk,
        RectorReception,
        SocialSupport,
        LibraryCatalog
    ];

    public static IReadOnlyList<string> ApplicantUrls { get; } =
    [
        AdmissionPortal,
        AdmissionRules,
        AdmissionPrograms,
        AdmissionExams,
        AdmissionNews,
        OpenDays,
        AdmissionFaq,
        ForeignApplicants,
        AdmissionContacts,
        AdmissionReception,
        HostelRules,
        Branches
    ];

    public static IReadOnlyList<string> ScienceUrls { get; } =
    [
        ScienceHome,
        ScienceNews,
        ScienceFields,
        ScienceEvents,
        ScienceDigest,
        ScienceGrants,
        SciencePublishing,
        ScienceHelp,
        ScienceInnovations,
        ScienceStructure
    ];

    public static IReadOnlyList<string> InternationalUrls { get; } =
    [
        InternationalActivity,
        EnglishSite,
        ForeignApplicants,
        DiplomaSupplement
    ];

    public static IReadOnlyList<string> DigitalServiceUrls { get; } =
    [
        DigitalStudentServices,
        LibraryCatalog,
        Webmail,
        DormItRequest,
        Intra,
        Promt,
        Eios
    ];
}
