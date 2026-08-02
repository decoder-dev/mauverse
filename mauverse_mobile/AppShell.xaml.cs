using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using mau.Pages;
using mau.Utils.Services;
using Font = Microsoft.Maui.Font;

namespace mau;

public partial class AppShell : Shell
{
    private static readonly TimeSpan PrewarmStartDelay = TimeSpan.FromMilliseconds(600);
    private static readonly TimeSpan PrewarmFrameDelay = TimeSpan.FromMilliseconds(80);
    private bool _tabPrewarmingStarted;

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("chats/details", typeof(DetailChatPage));
        Routing.RegisterRoute("services/chats", typeof(HomeChatPage));
        Routing.RegisterRoute("services/campus", typeof(CampusNavigatorPage));
        Routing.RegisterRoute("services/teacher_info", typeof(InfoPage));
        Routing.RegisterRoute("services/telephones", typeof(TelephonePage));
        Routing.RegisterRoute("services/study_info", typeof(StudyPage));
        Routing.RegisterRoute("services/study_info/details", typeof(DetailStudentDebtPage));
        Routing.RegisterRoute("services/forms", typeof(OnlineFormsPage));
        Routing.RegisterRoute("services/forms/certificate", typeof(CertificateRequestPage));
        Routing.RegisterRoute("services/student_guide", typeof(StudentGuidePage));
        Routing.RegisterRoute("services/applicant", typeof(ApplicantGuidePage));
        Routing.RegisterRoute("services/science_guide", typeof(ScienceGuidePage));
        Routing.RegisterRoute("services/international", typeof(InternationalGuidePage));
        Routing.RegisterRoute("services/digital", typeof(DigitalServicesPage));
        Routing.RegisterRoute("services/contacts", typeof(UniversityContactsPage));
        Routing.RegisterRoute("services/events", typeof(EventsCalendarPage));
        Routing.RegisterRoute("profile/settings", typeof(SettingsPage));
        Routing.RegisterRoute(BrowserDestinationRegistry.InternalBrowserRoute, typeof(InternalBrowserPage));
        Navigated += OnNavigated;
    }

    private async void OnNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (_tabPrewarmingStarted || !string.Equals(CurrentItem?.Route, "main", StringComparison.Ordinal))
            return;

        _tabPrewarmingStarted = true;
        Navigated -= OnNavigated;

        try
        {
            // Spread page construction over idle frames so the first tab tap stays responsive.
            await Task.Delay(PrewarmStartDelay);
            if (CurrentPage is InternalBrowserPage)
                return;

            var mainTabs = Items.FirstOrDefault(item => string.Equals(item.Route, "main", StringComparison.Ordinal));
            if (mainTabs is null)
                return;

            foreach (var section in mainTabs.Items)
            {
                foreach (var content in section.Items)
                {
                    if (CurrentPage is InternalBrowserPage)
                        return;

                    if (content.Content is null && content.ContentTemplate?.CreateContent() is Page page)
                        content.Content = page;

                    await Task.Delay(PrewarmFrameDelay);
                }
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }

    public void ResetAccountScopedPages()
    {
        var mainTabs = Items.FirstOrDefault(item => string.Equals(item.Route, "main", StringComparison.Ordinal));
        if (mainTabs is null)
            return;

        foreach (var section in mainTabs.Items)
        {
            foreach (var content in section.Items)
                content.Content = null;
        }
    }

    public static async Task DisplaySnackbarAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = Color.FromArgb(ThemePalette.BrandBlack),
            TextColor = Color.FromArgb(ThemePalette.BrandWhite),
            ActionButtonTextColor = Color.FromArgb(ThemePalette.BrandBlue),
            CornerRadius = new CornerRadius(8),
            Font = Font.SystemFontOfSize(14),
            ActionButtonFont = Font.SystemFontOfSize(14)
        };

        var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);
        try
        {
            await snackbar.Show(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
        }
    }

    public static async Task DisplayToastAsync(string message)
    {
        if (OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(message))
            return;

        var toast = Toast.Make(message, textSize: 14);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await toast.Show(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
        }
    }
}
