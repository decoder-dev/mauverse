using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.Services;

public partial class EventsCalendarViewModel : BaseViewModel
{
    readonly IParserRequests _parserRequests;
    readonly IAppNavigationService _navigation;
    CancellationTokenSource? _loadCts;
    Task? _loadTask;
    bool _loaded;

    [ObservableProperty]
    List<RssDTO> _events = [];

    public bool IsLoading => CurrentState == States.Loading;
    public bool HasContent => CurrentState == States.Success;
    public bool IsEmpty => CurrentState == States.Empty;

    public EventsCalendarViewModel(
        DbConnect context,
        IParserRequests parserRequests,
        IAppNavigationService navigation) : base(context)
    {
        _parserRequests = parserRequests;
        _navigation = navigation;
    }

    [RelayCommand]
    async Task LoadData()
    {
        if (_loaded && Events.Count > 0)
            return;

        if (_loadTask is not null)
        {
            await _loadTask;
            return;
        }

        var cancellationSource = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _loadCts, cancellationSource);
        previous?.Cancel();
        var loadTask = LoadCoreAsync(cancellationSource.Token);
        _loadTask = loadTask;
        try
        {
            await loadTask;
        }
        finally
        {
            if (ReferenceEquals(_loadTask, loadTask))
            {
                _loadTask = null;
                Interlocked.CompareExchange(ref _loadCts, null, cancellationSource);
            }

            cancellationSource.Dispose();
        }
    }

    [RelayCommand]
    async Task OpenEvent(RssDTO? item)
    {
        if (item is null)
            return;

        var details = new DetailNewsPopup(item, _navigation);
        await Shell.Current.CurrentPage.ShowPopupAsync(details);
    }

    [RelayCommand]
    async Task OpenSiteCalendar()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для открытия календаря требуется интернет");
            return;
        }

        try
        {
            await _navigation.OpenKnownBrowserAsync(BrowserDestinationRegistry.EventsCalendarKey);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть календарь на сайте");
        }
    }

    async Task LoadCoreAsync(CancellationToken cancellationToken)
    {
        SetState(States.Loading);
        try
        {
            Events = [.. await _parserRequests.GetNewsAsync(RssData.Calendar, cancellationToken)];
            cancellationToken.ThrowIfCancellationRequested();
            _loaded = true;
            SetState(Events.Count == 0 ? States.Empty : States.Success);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            SetState(States.Empty);
            await AppShell.DisplaySnackbarAsync("Не удалось загрузить события");
        }
    }

    void SetState(string state)
    {
        CurrentState = state;
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(HasContent));
        OnPropertyChanged(nameof(IsEmpty));
    }

    protected override void CancelPendingOperations() => _loadCts?.Cancel();
}
