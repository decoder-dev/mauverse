using CommunityToolkit.Mvvm.Input;

using mau.Models;
using mau.Resources.Fonts;
using mau.Utils;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.Services;

public partial class OnlineFormsViewModel
{
    private const string FormsBaseUrl = "https://mauniver.ru/services/student";
    private readonly IAppNavigationService _navigation;

    public OnlineFormsViewModel(IAppNavigationService navigation)
    {
        _navigation = navigation;
    }

    public IReadOnlyList<StudentOnlineForm> CertificateForms { get; } =
    [
        new(
            "Справка об обучении",
            "Обычная, гербовая или электронная",
            $"{FormsBaseUrl}/",
            FluentUI.certificate_24_regular),
        new(
            "Справка для перевода",
            "С перечнем дисциплин и оценок",
            $"{FormsBaseUrl}/perevod/",
            FluentUI.document_text_24_regular),
        new(
            "Справка о стипендии",
            "Выплаты за выбранный период",
            $"{FormsBaseUrl}/spravka/",
            FluentUI.money_24_regular),
        new(
            "Архивная справка",
            "Для выпускников и бывших студентов",
            $"{FormsBaseUrl}/archive/",
            FluentUI.archive_24_regular),
        new(
            "Архивная справка для отчисленных",
            "Установленного образца",
            $"{FormsBaseUrl}/archive-expl/",
            FluentUI.document_multiple_24_regular),
        new(
            "Справка-вызов",
            "Для предоставления работодателю",
            $"{FormsBaseUrl}/vyzov/",
            FluentUI.call_24_regular),
        new(
            "Справка для налоговой",
            "Для социального налогового вычета",
            $"{FormsBaseUrl}/nalog/",
            FluentUI.document_checkmark_24_regular)
    ];

    public IReadOnlyList<StudentOnlineForm> OtherApplications { get; } =
    [
        new(
            "Дубликат диплома",
            "Заявление на повторную выдачу",
            $"{FormsBaseUrl}/diplom/",
            FluentUI.document_copy_24_regular),
        new(
            "Счёт за обучение",
            "Для платных образовательных услуг",
            $"{FormsBaseUrl}/application/",
            FluentUI.receipt_money_24_regular),
        new(
            "Онлайн-сервисы ММРК",
            "Справки и заявления колледжа",
            "https://mauniver.ru/structure/branches/mmrc/online/",
            FluentUI.building_24_regular),
        new(
            "Справочная студенческого офиса",
            "Задать вопрос об учебном процессе",
            "https://mauniver.ru/services/virtual/",
            FluentUI.chat_help_24_regular),
        new(
            "Справочная библиотеки",
            "Получить помощь библиотекаря",
            "https://mauniver.ru/structure/divs/library/guide/",
            FluentUI.book_question_mark_24_regular),
        new(
            "Виртуальная приёмная ректора",
            "Направить официальное обращение",
            "https://mauniver.ru/rector/reception/",
            FluentUI.mail_24_regular),
        new(
            "Вопрос приёмной комиссии",
            "Обращение по вопросам поступления",
            "https://mauniver.ru/abit/reception/",
            FluentUI.person_question_mark_24_regular),
        new(
            "Стать волонтёром МАУ",
            "Присоединиться к волонтёрскому движению",
            "https://mauniver.ru/student/community/volunteer/",
            FluentUI.heart_24_regular),
        new(
            "Поддержка молодых семей",
            "Направить обращение и документы",
            $"{FormsBaseUrl}/material/",
            FluentUI.people_community_24_regular),
        new(
            "Поддержка участников СВО",
            "Единое окно поддержки студентов",
            $"{FormsBaseUrl}/support-svo/",
            FluentUI.shield_24_regular),
        new(
            "Обратная связь «Моё образование»",
            "Исправление данных в ГИС СЦОС",
            $"{FormsBaseUrl}/gis-scos/",
            FluentUI.hat_graduation_24_regular)
    ];

    [RelayCommand]
    private async Task OpenFormAsync(StudentOnlineForm? form)
    {
        if (form is null)
            return;

        if (form.IsNative)
        {
            await _navigation.NavigateAsync("services/forms/certificate");
            return;
        }

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для онлайн-форм требуется интернет");
            return;
        }

        try
        {
            if (!ExternalUri.TryCreateHttp(form.Url, out var formUri))
            {
                await AppShell.DisplaySnackbarAsync("Ссылка на форму недоступна");
                return;
            }

            await _navigation.OpenBrowserAsync(
                BrowserDestinationRegistry.CreateUniversityForm(form.Title, formUri));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть форму");
        }
    }
}
