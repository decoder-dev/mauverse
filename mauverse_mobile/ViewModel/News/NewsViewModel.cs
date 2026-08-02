using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.News
{
    public class Buttons
    {
        public string FilterName { get; set; } = string.Empty;
        public RssData FilterType { get; set; }
    }
    public partial class NewsViewModel : BaseViewModel
    {
        private readonly IParserRequests _parserRequests;
        private readonly IAppNavigationService _navigation;
        Task? _loadTask;
        CancellationTokenSource? _loadCts;
        RssData? _loadingFilter;
        RssData? _loadedFilter;

        [ObservableProperty]
        List<RssDTO> _news = [];

        [ObservableProperty]
        RssDTO? _selectedNews;

        [ObservableProperty]
        Buttons _selectedButton = new();

        [ObservableProperty]
        List<Buttons> _filterButtons = [];

        public bool IsLoading => CurrentState == States.Loading;
        public bool HasContent => CurrentState == States.Success;
        public bool IsEmpty => CurrentState == States.Empty;

        public NewsViewModel(
            DbConnect context,
            IParserRequests parserRequests,
            IAppNavigationService navigation) : base(context)
        {
            _parserRequests = parserRequests;
            _navigation = navigation;
            FilterButtons = LoadFilterData();
            SelectedButton = FilterButtons.First();
        }

        [RelayCommand]
        async Task GetFullInfo(RssDTO news)
        {
            await GetNewsDescription(news);
        }

        [RelayCommand]
        async Task FilterData()
        {
            await LoadNews(SelectedButton?.FilterType ?? RssData.Default);
        }

        [RelayCommand]
        async Task LoadData()
        {
            await LoadNews(RssData.Default);
        }

        async Task LoadNews(RssData filterType)
        {
            if (_loadedFilter == filterType)
                return;

            if (_loadTask is not null && _loadingFilter == filterType)
            {
                await _loadTask;
                return;
            }

            var cancellationSource = new CancellationTokenSource();
            var previousSource = Interlocked.Exchange(ref _loadCts, cancellationSource);
            previousSource?.Cancel();
            _loadingFilter = filterType;
            var loadTask = LoadNewsCore(filterType, cancellationSource.Token);
            _loadTask = loadTask;
            try
            {
                await loadTask;
            }
            finally
            {
                if (ReferenceEquals(_loadTask, loadTask))
                {
                    _loadingFilter = null;
                    _loadTask = null;
                    Interlocked.CompareExchange(ref _loadCts, null, cancellationSource);
                }

                cancellationSource.Dispose();
            }
        }

        async Task LoadNewsCore(RssData filterType, CancellationToken cancellationToken)
        {
            SetState(States.Loading);
            try
            {
                News = [.. await _parserRequests.GetNewsAsync(filterType, cancellationToken)];
                cancellationToken.ThrowIfCancellationRequested();
                _loadedFilter = filterType;
                SetState(News.Count == 0 ? States.Empty : States.Success);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                SetState(States.Empty);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить новости");
            }
        }

        void SetState(string state)
        {
            CurrentState = state;
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(HasContent));
            OnPropertyChanged(nameof(IsEmpty));
        }

        async Task GetNewsDescription(RssDTO news)
        {
            if (news is null)
                return;

            var details = new DetailNewsPopup(news, _navigation);
            await Shell.Current.CurrentPage.ShowPopupAsync(details);
        }

        static List<Buttons> LoadFilterData()
        {
            List<Buttons> buttons = [
                new Buttons {
                    FilterName = "Новости",
                    FilterType = RssData.Default,
                },
                new Buttons {
                    FilterName = "Информация",
                    FilterType = RssData.Events,
                },
                new Buttons {
                    FilterName = "Подразделения",
                    FilterType = RssData.Depts,
                },
                new Buttons {
                    FilterName = "Объединения",
                    FilterType = RssData.Students,
                },
                new Buttons {
                    FilterName = "Спорт",
                    FilterType = RssData.Sports,
                },
                new Buttons {
                    FilterName = "Наука",
                    FilterType = RssData.Science,
                },
                new Buttons {
                    FilterName = "Международное",
                    FilterType = RssData.International,
                },
                new Buttons {
                    FilterName = "Абитуриент",
                    FilterType = RssData.Applicant,
                },
                new Buttons {
                    FilterName = "Календарь",
                    FilterType = RssData.Calendar,
                },
                new Buttons {
                    FilterName = "СМИ о нас",
                    FilterType = RssData.Other,
                },
            ];
            return buttons;
        }

        protected override void CancelPendingOperations() => _loadCts?.Cancel();
    }
}
