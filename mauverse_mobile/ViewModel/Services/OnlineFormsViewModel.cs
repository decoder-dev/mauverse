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
            FluentUI.certificate_24_regular,
            true),
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
            FluentUI.receipt_money_24_regular)
    ];

    public IReadOnlyList<StudentOnlineForm> SupportForms { get; } =
    [
        new(
            "Виртуальная справочная",
            "Вопрос студенческому офису или библиотеке",
            "https://mauniver.ru/services/virtual/",
            FluentUI.person_feedback_24_regular),
        new(
            "Вопросы ректору",
            "Обращение в виртуальную приёмную",
            "https://mauniver.ru/rector/reception/",
            FluentUI.mail_inbox_24_regular),
        new(
            "Стать волонтёром МАУ",
            "Заявка в волонтёрские объединения",
            "https://mauniver.ru/student/community/volunteer/",
            FluentUI.heart_24_regular),
        new(
            "Поддержка молодых семей",
            "Обращение по мерам семейной поддержки",
            $"{FormsBaseUrl}/material/",
            FluentUI.people_community_24_regular),
        new(
            "Поддержка участников СВО",
            "Единое окно мер поддержки",
            $"{FormsBaseUrl}/support-svo/",
            FluentUI.shield_24_regular),
        new(
            "Портал «Моё образование»",
            "Обратная связь по ГИС СЦОС",
            $"{FormsBaseUrl}/gis-scos/",
            FluentUI.document_checkmark_24_regular),
        new(
            "Вопрос приёмной комиссии",
            "Консультация для абитуриентов",
            "https://mauniver.ru/abit/reception/",
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
