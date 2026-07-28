using CommunityToolkit.Maui.Views;

using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.ViewModel;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mau.ViewModel.Services
{
    public class TelephoneViewModel : BaseViewModel
    {
        private const int PageSize = 8;

        private readonly IParserRequests _parserRequests;
        private readonly DbConnect _context;
        private ObservableCollection<DeptInfoDTO> _deptInfos = [];
        private List<DeptInfoDTO> _depts = [];
        private DeptInfoDTO? _selectedDept;
        private string _titleLabel = string.Empty;
        private string _subtitleLabel = string.Empty;
        private int _page;
        private int _totalPages;

        public TelephoneViewModel(DbConnect context, IParserRequests parserRequests) : base(context)
        {
            _parserRequests = parserRequests;
            _context = context;
            TitleLabel = "Телефоны";
            SubtitleLabel = "Контактная информация АУП ВУЗа";
            LoadAllData = new AsyncRelayCommand(LoadAllDataAsync);
            GetPhone = new AsyncRelayCommand<object?>(OpenDepartmentAsync);
            NextPage = new RelayCommand(MoveToNextPage, () => Page < TotalPages);
            PreviousPage = new RelayCommand(MoveToPreviousPage, () => Page > 1);
            Page = 1;
            TotalPages = 1;
        }

        public IAsyncRelayCommand LoadAllData { get; }
        public IAsyncRelayCommand<object?> GetPhone { get; }

        async Task LoadAllDataAsync(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await LoadTelephones(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                CurrentState = States.Empty;
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить контакты");
            }
            finally
            {
                IsBusy = false;
            }
        }

        async Task OpenDepartmentAsync(object? obj)
        {
            var dept = obj as DeptInfoDTO;
            if (IsBusy || dept is null)
                return;

            IsBusy = true;
            try
            {
                var detailPopup = new DetailTelephonePopup(_context, _parserRequests, dept);
                await Shell.Current.CurrentPage.ShowPopupAsync(detailPopup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть контакты подразделения");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public IRelayCommand NextPage { get; }
        public IRelayCommand PreviousPage { get; }

        private void MoveToNextPage()
        {
            if (Page >= TotalPages)
                return;

            Page++;
            UpdatePage();
        }

        private void MoveToPreviousPage()
        {
            if (Page <= 1)
                return;

            Page--;
            UpdatePage();
        }
        public DeptInfoDTO? SelectedDept
        {
            get => _selectedDept;
            set
            {
                _selectedDept = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<DeptInfoDTO> DeptInfos
        {
            get => _deptInfos;
            set
            {
                _deptInfos = value;
                OnPropertyChanged();
            }
        }

        public int Page
        {
            get => _page;
            set
            {
                _page = value;
                OnPropertyChanged();
                NextPage.NotifyCanExecuteChanged();
                PreviousPage.NotifyCanExecuteChanged();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
                NextPage.NotifyCanExecuteChanged();
                PreviousPage.NotifyCanExecuteChanged();
            }
        }
        public string TitleLabel
        {
            get => _titleLabel;
            set
            {
                _titleLabel = value;
                OnPropertyChanged();
            }
        }

        public string SubtitleLabel
        {
            get => _subtitleLabel;
            set
            {
                _subtitleLabel = value;
                OnPropertyChanged();
            }
        }

        async Task LoadTelephones(CancellationToken cancellationToken)
        {
            CurrentState = States.Loading;
            _depts = [.. await _parserRequests.GetDeptsAsync(cancellationToken)];
            Page = 1;
            DeptInfos = new(_depts.Take(PageSize));
            TotalPages = Math.Max(1, (int)Math.Ceiling(_depts.Count / (double)PageSize));
            CurrentState = _depts.Count == 0 ? States.Empty : States.Success;
        }

        void UpdatePage()
        {
            DeptInfos = new(_depts.Skip((Page - 1) * PageSize).Take(PageSize));
        }

        protected override void CancelPendingOperations() => LoadAllData.Cancel();
    }
}
