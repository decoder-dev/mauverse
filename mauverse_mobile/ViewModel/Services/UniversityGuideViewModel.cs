using CommunityToolkit.Mvvm.Input;

using mau.Models;
using mau.Utils;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.Services;

public partial class UniversityGuideViewModel
{
    readonly IAppNavigationService _navigation;

    public UniversityGuideViewModel(
        IAppNavigationService navigation,
        string pageDescription,
        IReadOnlyList<UniversityGuideSection> sections)
    {
        _navigation = navigation;
        PageDescription = pageDescription;
        Sections = sections;
    }

    public string PageDescription { get; }

    public IReadOnlyList<UniversityGuideSection> Sections { get; }

    public static UniversityGuideViewModel CreateStudent(IAppNavigationService navigation) =>
        new(
            navigation,
            "Разделы официального сайта для обучающихся МАУ",
            UniversityGuideCatalog.StudentSections);

    public static UniversityGuideViewModel CreateApplicant(IAppNavigationService navigation) =>
        new(
            navigation,
            "Поступление, программы и контакты приёмной комиссии",
            UniversityGuideCatalog.ApplicantSections);

    public static UniversityGuideViewModel CreateScience(IAppNavigationService navigation) =>
        new(
            navigation,
            "Новости, гранты, издательство и инфраструктура науки МАУ",
            UniversityGuideCatalog.ScienceSections);

    public static UniversityGuideViewModel CreateInternational(IAppNavigationService navigation) =>
        new(
            navigation,
            "English site, exchange and international admissions",
            UniversityGuideCatalog.InternationalSections);

    public static UniversityGuideViewModel CreateDigital(IAppNavigationService navigation) =>
        new(
            navigation,
            "ЭИОС, библиотека, почта, Intra и заявки УИТ",
            UniversityGuideCatalog.DigitalSections);

    [RelayCommand]
    async Task OpenLinkAsync(UniversityGuideLink? link)
    {
        if (link is null)
            return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для открытия раздела требуется интернет");
            return;
        }

        try
        {
            if (!ExternalUri.TryCreateHttp(link.Url, out var uri))
            {
                await AppShell.DisplaySnackbarAsync("Ссылка недоступна");
                return;
            }

            await _navigation.OpenBrowserAsync(
                BrowserDestinationRegistry.CreateUniversityPage(link.Title, uri));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть раздел");
        }
    }
}
