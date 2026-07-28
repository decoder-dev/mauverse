using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.DTOModels;
using mau.Utils.Services.Interface;

using System.Globalization;
using System.Net.Mail;

namespace mau.ViewModel.Services;

public partial class CertificateRequestViewModel : BaseViewModel
{
    private readonly IStudentFormsService _formsService;
    private readonly IAppNavigationService _navigation;

    public IReadOnlyList<string> Institutes { get; } =
    [
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
    ];

    public IReadOnlyList<string> StudyForms { get; } =
    [
        "Очная",
        "Заочная",
        "Очно-заочная"
    ];

    public IReadOnlyList<string> FundingSources { get; } =
    [
        "Бюджет",
        "Договор"
    ];

    public IReadOnlyList<string> CertificateTypes { get; } =
    [
        "Простая, с печатью Студенческого офиса",
        "Гербовая, с гербовой печатью Университета",
        "Электронная, с электронной подписью"
    ];

    public IReadOnlyList<string> DeliveryMethods { get; } =
    [
        "Лично: Спортивная, 13, кабинет 203В",
        "Лично: Егорова, 16, кабинет 112",
        "Почта России",
        "Электронная почта в домене @mauniver.ru"
    ];

    public DateTime MinimumBirthDate { get; } = new(1930, 1, 1);
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "The instance member is part of the compiled XAML binding contract.")]
    public DateTime MaximumBirthDate => DateTime.Today;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private DateTime _birthDate = new(2000, 1, 1);

    [ObservableProperty]
    private string? _institute;

    [ObservableProperty]
    private string? _studyForm;

    [ObservableProperty]
    private string? _fundingSource;

    [ObservableProperty]
    private string? _certificateType;

    [ObservableProperty]
    private int _copies = 1;

    [ObservableProperty]
    private string? _deliveryMethod;

    [ObservableProperty]
    private string _postalAddress = string.Empty;

    [ObservableProperty]
    private string _universityEmail = string.Empty;

    [ObservableProperty]
    private string _comment = string.Empty;

    [ObservableProperty]
    private bool _isConsentGranted;

    [ObservableProperty]
    private bool _isPostalDelivery;

    [ObservableProperty]
    private bool _isEmailDelivery;

    [ObservableProperty]
    private bool _isDeliveryMethodEnabled = true;

    public CertificateRequestViewModel(
        DbConnect context,
        IStudentFormsService formsService,
        IAppNavigationService navigation) : base(context)
    {
        _formsService = formsService;
        _navigation = navigation;
        FullName = CurrentUser?.FullName ?? string.Empty;
    }

    partial void OnDeliveryMethodChanged(string? value)
    {
        IsPostalDelivery = value == DeliveryMethods[2];
        IsEmailDelivery = value == DeliveryMethods[3];
    }

    partial void OnCertificateTypeChanged(string? value)
    {
        IsDeliveryMethodEnabled = value != CertificateTypes[2];
        if (!IsDeliveryMethodEnabled)
            DeliveryMethod = DeliveryMethods[3];
    }

    [RelayCommand]
    private async Task SubmitAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
            return;

        var validationError = Validate();
        if (validationError is not null)
        {
            await AppShell.DisplaySnackbarAsync(validationError);
            return;
        }

        IsBusy = true;
        try
        {
            List<StudentFormField> fields =
            [
                Field("ФИО (как в паспорте)", FullName),
                Field("E-mail", Email),
                Field("Телефон", Phone),
                Field("Дата рождения", BirthDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture)),
                Field("Факультет / институт", Institute!),
                Field("Форма обучения", StudyForm!),
                Field("Источник финансирования", FundingSource!),
                Field("Вид справки", CertificateType!),
                Field("Количество справок", Copies.ToString(CultureInfo.InvariantCulture)),
                Field("Способ получения документа", DeliveryMethod!)
            ];

            if (IsPostalDelivery)
                fields.Add(Field("Почтовый адрес", PostalAddress));
            if (IsEmailDelivery)
                fields.Add(Field("Электронный адрес в домене @mauniver.ru", UniversityEmail));
            if (!string.IsNullOrWhiteSpace(Comment))
                fields.Add(Field("Комментарий", Comment));

            await _formsService.SubmitCertificateAsync(
                Email.Trim(),
                FullName.Trim(),
                fields,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await AppShell.DisplaySnackbarAsync("Заявка отправлена в Студенческий офис");
            cancellationToken.ThrowIfCancellationRequested();
            await _navigation.GoBackAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось отправить заявку. Попробуйте позже");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string? Validate()
    {
        if (FullName.Length > 160 || FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            return "Укажите полное ФИО как в паспорте";
        if (!IsPlainEmail(Email, out _) || Email.Length > 254)
            return "Проверьте контактный e-mail";
        if (Phone.Length > 32 || Phone.Count(char.IsDigit) is < 10 or > 15)
            return "Проверьте номер телефона";
        if (BirthDate < MinimumBirthDate || BirthDate > MaximumBirthDate)
            return "Проверьте дату рождения";
        if (string.IsNullOrWhiteSpace(Institute))
            return "Выберите факультет или институт";
        if (string.IsNullOrWhiteSpace(StudyForm))
            return "Выберите форму обучения";
        if (string.IsNullOrWhiteSpace(FundingSource))
            return "Выберите источник финансирования";
        if (string.IsNullOrWhiteSpace(CertificateType))
            return "Выберите вид справки";
        if (Copies is < 1 or > 5)
            return "Количество справок должно быть от 1 до 5";
        if (string.IsNullOrWhiteSpace(DeliveryMethod))
            return "Выберите способ получения документа";
        if (CertificateType == CertificateTypes[2] && DeliveryMethod != DeliveryMethods[3])
            return "Электронная справка выдается только на почту @mauniver.ru";
        if (IsPostalDelivery && (string.IsNullOrWhiteSpace(PostalAddress) || PostalAddress.Length > 500))
            return "Укажите полный почтовый адрес";
        if (IsEmailDelivery &&
            (!IsPlainEmail(UniversityEmail, out var universityAddress) ||
             !universityAddress.EndsWith("@mauniver.ru", StringComparison.OrdinalIgnoreCase)))
            return "Укажите адрес в домене @mauniver.ru";
        if (Comment.Length > 2000)
            return "Комментарий не должен превышать 2000 символов";
        if (!IsConsentGranted)
            return "Подтвердите согласие на обработку персональных данных";

        return null;
    }

    private static StudentFormField Field(string title, string value) =>
        new(title, value.Trim());

    private static bool IsPlainEmail(string value, out string address)
    {
        var normalized = value.Trim();
        if (MailAddress.TryCreate(normalized, out var parsed) &&
            string.Equals(parsed.Address, normalized, StringComparison.OrdinalIgnoreCase))
        {
            address = parsed.Address;
            return true;
        }

        address = string.Empty;
        return false;
    }

    protected override void CancelPendingOperations() => SubmitCommand.Cancel();
}
