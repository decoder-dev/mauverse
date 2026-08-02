using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Models;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

using System.Collections.ObjectModel;

namespace mau.ViewModel.Services;

public partial class CampusNavigatorViewModel : ObservableObject
{
    private readonly IAppNavigationService _navigation;
    private static readonly IReadOnlyList<CampusBuilding> SouthCampus =
    [
        Building("Корпус А", "ул. Спортивная, 13/6"),
        Building("Корпус Б", "ул. Колхозная, 2"),
        Building("Корпус В", "ул. Спортивная, 13"),
        Building("Корпус Г", "ул. Советская, 8А"),
        Building("Корпус Д", "ул. Советская, 8"),
        Building("Корпус Е", "ул. Советская, 12А"),
        Building("Корпус К", "ул. Спортивная, 9"),
        Building("Корпус Л1", "ул. Кирова, 1"),
        Building("Корпус Л2", "ул. Кирова, 1"),
        Building("Корпус М", "ул. Советская, 17"),
        Building("Корпус Н", "ул. Спортивная, 11"),
        Building("Корпус П", "ул. Советская, 10"),
        Building("Корпус С", "ул. Советская, 14"),
        Building("Корпус Э", "ул. Горького, 14"),
        Building("Столовая", "ул. Колхозная, 15А"),
        Building("КСК", "ул. Колхозная, 15")
    ];

    private static readonly IReadOnlyList<CampusBuilding> NorthCampus =
    [
        Building("Е15", "ул. Капитана Егорова, 15"),
        Building("Е16", "ул. Капитана Егорова, 16"),
        Building("К9", "ул. Коммуны, 9"),
        Building("Л57", "пр. Ленина, 57")
    ];

    private static readonly IReadOnlyList<CampusBuilding> Branches =
    [
        Building("Филиал в г. Апатиты", "ул. Лесная, 29", "apatity", "Апатиты"),
        Building("Филиал в г. Кировске", "ул. 50 лет Октября, 2", "kirovsk", "Кировск"),
        Building("Филиал в г. Полярный", "ул. Лунина, 5", "murmansk", "Полярный")
    ];

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ObservableCollection<CampusBuildingGroup> CampusGroups { get; } = [];

    public bool HasResults => CampusGroups.Count > 0;
    public bool ShowEmptyState => !HasResults;

    public CampusNavigatorViewModel(IAppNavigationService navigation)
    {
        _navigation = navigation;
        ApplyFilter();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    [RelayCommand]
    private async Task OpenRouteAsync(CampusBuilding? building)
    {
        if (building is null)
            return;

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для построения маршрута требуется интернет");
            return;
        }

        try
        {
            var query = Uri.EscapeDataString(building.SearchQuery);
            var city = string.IsNullOrWhiteSpace(building.MapCity) ? "murmansk" : building.MapCity;
            var routeUri = new Uri($"https://2gis.ru/{city}/search/{query}");
            var opened = await _navigation.OpenExternalAsync(routeUri);

            if (!opened)
                await AppShell.DisplaySnackbarAsync("Не удалось открыть карту");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть карту");
        }
    }

    [RelayCommand]
    private async Task OpenOfficialNavigatorAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для открытия навигатора требуется интернет");
            return;
        }

        try
        {
            await _navigation.OpenKnownBrowserAsync(BrowserDestinationRegistry.CampusNavigatorSiteKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть навигатор на сайте");
        }
    }

    private void ApplyFilter()
    {
        var query = SearchText.Trim();
        CampusGroups.Clear();

        AddGroup(
            "Южный кампус",
            "Остановки: МАУ, переулок Хибинский",
            Filter(SouthCampus, query));
        AddGroup(
            "Северный кампус",
            "Остановки: Капитана Егорова, Академика Книповича",
            Filter(NorthCampus, query));
        AddGroup(
            "Филиалы",
            "Апатиты, Кировск и Полярный",
            Filter(Branches, query));

        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(ShowEmptyState));
    }

    private static IEnumerable<CampusBuilding> Filter(
        IEnumerable<CampusBuilding> source,
        string query) => string.IsNullOrWhiteSpace(query)
        ? source
        : source.Where(building =>
            building.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            building.Address.Contains(query, StringComparison.OrdinalIgnoreCase));

    private void AddGroup(string name, string stops, IEnumerable<CampusBuilding> source)
    {
        var buildings = source.ToList();
        if (buildings.Count > 0)
            CampusGroups.Add(new CampusBuildingGroup(name, stops, buildings));
    }

    private static CampusBuilding Building(string title, string address) =>
        new(title, address, $"МАУ {title}, {address}, Мурманск");

    private static CampusBuilding Building(
        string title,
        string address,
        string mapCity,
        string cityName) =>
        new(title, address, $"МАУ {title}, {address}, {cityName}", mapCity);
}
