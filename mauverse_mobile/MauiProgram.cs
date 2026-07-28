using CommunityToolkit.Maui;
using mau.Controls;
using mau.Database;
using mau.Pages;
using mau.Resources.Fonts;
using mau.Utils.API;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using Microsoft.Extensions.Logging;

namespace mau;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(static options =>
            {
                options.SetPopupDefaults(new DefaultPopupSettings
                {
                    CanBeDismissedByTappingOutsideOfPopup = true,
                    BackgroundColor = Colors.Transparent,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Margin = 0,
                    Padding = 0
                });
                options.SetPopupOptionsDefaults(new DefaultPopupOptionsSettings
                {
                    CanBeDismissedByTappingOutsideOfPopup = true,
                    PageOverlayColor = Color.FromArgb("#80000000"),
                    Shadow = null,
                    Shape = null
                });
            })
            .ConfigureMauiHandlers(handlers =>
            {
#if IOS || MACCATALYST
                handlers.AddHandler<CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
#if IOS
                handlers.AddHandler<Shell, Platforms.iOS.LiquidGlassShellRenderer>();
#endif
#if ANDROID
                handlers.AddHandler<Shell, Platforms.Android.CustomShellRenderer>();
                handlers.AddHandler<ResilientWebView, Platforms.Android.ResilientWebViewHandler>();
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if IOS
        Platforms.iOS.LiquidGlassStyling.Configure();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<ICacheService, JsonFileCacheService>();
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddSingleton<IAppNavigationService, AppNavigationService>();
        builder.Services.AddDbContext<DbConnect>(ServiceLifetime.Transient);

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IUserRequests, UserRequests>();
        builder.Services.AddSingleton<IScheduleRequests, ScheduleRequests>();
        builder.Services.AddSingleton<IDebtRequests, DebtRequests>();
        builder.Services.AddSingleton<IValidationRequests, ValidationRequests>();
        builder.Services.AddSingleton<IParserRequests, ParserRequests>();
        builder.Services.AddSingleton<IStudentFormsService, StudentFormsService>();
        builder.Services.AddTransient<BaseViewModel>();
        builder.Services.AddTransient<LoadingViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<InfoPage>();
        builder.Services.AddTransient<DetailChatPage>();
        builder.Services.AddTransient<DetailStudentDebtPage>();
        builder.Services.AddTransient<TelephonePage>();
        builder.Services.AddTransient<LoadingPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ServiceListPage>();
        builder.Services.AddTransient<StudyPage>();
        builder.Services.AddTransient<SchedulePage>();
        builder.Services.AddTransient<HomeChatPage>();
        builder.Services.AddTransient<CampusNavigatorPage>();
        builder.Services.AddTransient<OnlineFormsPage>();
        builder.Services.AddTransient<CertificateRequestPage>();
        builder.Services.AddTransient<InternalBrowserPage>();
        builder.Services.AddTransient<NewsPage>();

        return builder.Build();
    }
}
