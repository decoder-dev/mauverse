using mau.Database;
using mau.DTOModels;
using mau.Resources.Fonts;
using mau.Utils;
using mau.Utils.Services.Interface;

using CommunityToolkit.Mvvm.Input;

namespace mau.ViewModel.Services
{
    public sealed class ServiceRow
    {
        public ServiceRow(string sectionTitle, ServiceDTO left, ServiceDTO right)
        {
            SectionTitle = sectionTitle;
            Left = left;
            Right = right;
        }

        public string SectionTitle { get; }
        public bool HasSectionTitle => !string.IsNullOrWhiteSpace(SectionTitle);
        public ServiceDTO Left { get; }
        public ServiceDTO Right { get; }
    }

    public class ServicesViewModel : BaseViewModel
    {
        bool _isNavigating;
        readonly IAppNavigationService _navigation;

        public ServicesViewModel(DbConnect context, IAppNavigationService navigation) : base(context)
        {
            _navigation = navigation;
            ServiceDTO[] universityServices =
            [
                CreateService("ЭИОС", FluentUI.book_open_microphone_24_regular, "eios"),
                CreateService("Онлайн-формы", FluentUI.certificate_24_regular, "forms"),
                CreateService("Мессенджер ЭИОС", FluentUI.chat_24_regular, "chats"),
                CreateService("Навигатор по корпусам", FluentUI.map_24_regular, "campus"),
                CreateService("Учебные задолженности", FluentUI.book_24_regular, "study_info"),
                CreateService("Цифровые сервисы МАУ", FluentUI.building_24_regular, "students")
            ];

            ServiceDTO[] directories =
            [
                CreateService("Контакты преподавателей", FluentUI.person_support_24_regular, "teacher_info"),
                CreateService("Подразделения и телефоны", FluentUI.phone_24_regular, "telephones")
            ];

            ServiceRows =
            [
                new("Услуги", universityServices[0], universityServices[1]),
                new(string.Empty, universityServices[2], universityServices[3]),
                new(string.Empty, universityServices[4], universityServices[5]),
                new("Справочники", directories[0], directories[1])
            ];
            SelectService = new AsyncRelayCommand<string?>(OpenServiceAsync);
        }

        public string PageDescription { get; } = "Учебные и цифровые инструменты";
        public IReadOnlyList<ServiceRow> ServiceRows { get; }
        public IAsyncRelayCommand<string?> SelectService { get; }

        async Task OpenServiceAsync(string? page)
        {
            if (_isNavigating || string.IsNullOrWhiteSpace(page))
                return;

            _isNavigating = true;
            try
            {
                if (string.Equals(page, "eios", StringComparison.Ordinal))
                {
                    await _navigation.OpenKnownBrowserAsync(BrowserDestinationRegistry.EiosKey);
                    return;
                }

                if (string.Equals(page, "students", StringComparison.Ordinal))
                {
                    await _navigation.OpenKnownBrowserAsync(BrowserDestinationRegistry.MauDigitalServicesKey);
                    return;
                }

                await _navigation.NavigateAsync($"services/{page}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть сервис");
            }
            finally
            {
                _isNavigating = false;
            }
        }

        static ServiceDTO CreateService(string name, string glyph, string page) => new()
        {
            ServiceName = name,
            Glyph = glyph,
            ServicePage = page,
            Role = UserRole.All
        };
    }
}
