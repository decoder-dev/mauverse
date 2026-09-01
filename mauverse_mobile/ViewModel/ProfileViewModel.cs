using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.Utils.Services;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mau.ViewModel
{
    public partial class ProfileViewModel : BaseViewModel
    {
        [ObservableProperty]
        bool _isEdit;
        [ObservableProperty]
        bool _subGroupButtonIsVisible;
        [ObservableProperty]
        bool _isCreditBookPanelIsVisible;
        [ObservableProperty]
        string _group = string.Empty;
        [ObservableProperty]
        string _name = string.Empty;
        [ObservableProperty]
        string _creditBook = string.Empty;
        string _editLabel = string.Empty;
        [ObservableProperty]
        string _groupDescription = string.Empty;
        [ObservableProperty]
        string _groupHint = DefaultGroupHint;
        [ObservableProperty]
        bool _isGroupListVisible;
        [ObservableProperty]
        List<string> _groupList = [];
        private readonly DbConnect _context;
        private readonly IUserRequests _userRequests;
        private readonly IValidationRequests _validationRequests;
        private readonly IAPIService _apiService;
        private readonly IAppNavigationService _navigation;
        private readonly AsyncRelayCommand _loadData;
        private User _user;
        private string? _subgroupsLoadedForGroup;
        private CancellationTokenSource? _groupThrottleCts;
        private bool _groupSelected;

        const string DefaultGroupHint = "Введите код группы, например ИС-21. Появятся подсказки из расписания.";
        const string ShortGroupHint = "Введите минимум 2 символа — покажем подсказки из расписания.";
        const string SelectedGroupHint = "Группа выбрана из списка расписания.";

        public ProfileViewModel(DbConnect context,
            IUserRequests userRequests,
            IAPIService service,
            IValidationRequests validationRequests,
            IAppNavigationService navigation) : base(context)
        {
            _context = context;
            _apiService = service;
            _userRequests = userRequests;
            _validationRequests = validationRequests;
            _navigation = navigation;
            _user = CurrentUser ?? new User();
            if (CurrentUser is null)
            {
                ButtonExitLabel = "Выйти";
                ButtonChangeLabel = "Выбрать подгруппу";
                PageTitle = "Личный кабинет";
                MainSectionLabel = "Основная информация";
                DescriptionLabel = "Специальность";
                NameLabel = "Полное имя";
                GroupLabel = "Группа";
                CreditBookLabel = "Номер зачетной книжки";
                _loadData = new AsyncRelayCommand(LoadDataAsync);
                Edit = new AsyncRelayCommand(cancellationToken =>
                    RunSafelyAsync(EditInfoAsync, "Не удалось сохранить изменения", cancellationToken));
                Change = new AsyncRelayCommand(cancellationToken =>
                    RunSafelyAsync(ChangeSubgroupAsync, "Не удалось изменить подгруппу", cancellationToken));
                OpenSettings = new AsyncRelayCommand(OpenSettingsAsync);
                Exit = new AsyncRelayCommand(ConfirmExitAsync);
                return;
            }

            if (CurrentUser.Role == UserRole.Student)
            {
                IsCreditBookPanelIsVisible = true;
            }
            else if (CurrentUser.Role == UserRole.Teacher)
            {
                IsCreditBookPanelIsVisible = false;
            }
            ButtonExitLabel = "Выйти";
            ButtonChangeLabel = "Выбрать подгруппу";
            PageTitle = "Личный кабинет";
            MainSectionLabel = "Основная информация";
            DescriptionLabel = "Специальность";
            NameLabel = "Полное имя";
            GroupLabel = "Группа";
            CreditBookLabel = "Номер зачетной книжки";
            _loadData = new AsyncRelayCommand(LoadDataAsync);
            Edit = new AsyncRelayCommand(cancellationToken =>
                RunSafelyAsync(EditInfoAsync, "Не удалось сохранить изменения", cancellationToken));
            Change = new AsyncRelayCommand(cancellationToken =>
                RunSafelyAsync(ChangeSubgroupAsync, "Не удалось изменить подгруппу", cancellationToken));
            OpenSettings = new AsyncRelayCommand(OpenSettingsAsync);
            Exit = new AsyncRelayCommand(ConfirmExitAsync);
        }

        async Task PrepareData(CancellationToken cancellationToken)
        {
            ButtonEditLabel = "Редактировать";
            IsEdit = false;
            GetProfile();
            var normalizedGroup = Group.Trim();
            if (string.Equals(_subgroupsLoadedForGroup, normalizedGroup, StringComparison.OrdinalIgnoreCase))
                return;

            await IsHaveSubgroups(cancellationToken);
            _subgroupsLoadedForGroup = normalizedGroup;
        }

        public IAsyncRelayCommand Edit { get; }
        public IAsyncRelayCommand LoadData => _loadData;
        public IAsyncRelayCommand Change { get; }
        public IAsyncRelayCommand OpenSettings { get; }
        public IAsyncRelayCommand Exit { get; }

        async Task OpenSettingsAsync()
        {
            try
            {
                await _navigation.NavigateAsync("profile/settings");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть настройки");
            }
        }

        async Task ConfirmExitAsync(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            object? result;
            try
            {
                var confirmation = new ConfirmationPopup();
                var popupResult = await Shell.Current.CurrentPage.ShowPopupAsync<bool>(
                    confirmation,
                    PopupOptions.Empty,
                    cancellationToken);
                result = popupResult.Result;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть подтверждение выхода");
                return;
            }

            if (result is not true)
                return;

            await RunSafelyAsync(ExitAsync, "Не удалось завершить сеанс", cancellationToken);
        }

        async Task ExitAsync(CancellationToken cancellationToken)
        {
            try
            {
                await DeleteDataAndExitAsync(_context, _apiService, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            await _navigation.NavigateAsync($"///{nameof(LoginPage)}?login=true");
            if (Shell.Current is AppShell shell)
                shell.ResetAccountScopedPages();
        }

        async Task LoadDataAsync(CancellationToken cancellationToken)
        {
            ButtonEditLabel = "Редактировать";
            IsEdit = false;
            GetProfile();

            var normalizedGroup = Group.Trim();
            if (string.Equals(_subgroupsLoadedForGroup, normalizedGroup, StringComparison.OrdinalIgnoreCase))
                return;

            await RunSafelyAsync(
                PrepareData,
                "Не удалось обновить профиль",
                cancellationToken);
        }
        public string ButtonExitLabel
        {
            get;
            private set;
        }
        public string ButtonEditLabel
        {
            get => _editLabel;
            set
            {
                _editLabel = value;
                OnPropertyChanged();
            }
        }
        public string NameLabel
        {
            get;
            private set;
        }
        public string DescriptionLabel
        {
            get;
            private set;
        }
        public string GroupLabel
        {
            get;
            private set;
        }
        public string CreditBookLabel
        {
            get;
            private set;
        }
        public string PageTitle
        {
            get;
            private set;
        }
        public string MainSectionLabel
        {
            get;
            private set;
        }
        public string ButtonChangeLabel
        {
            get;
            private set;
        }

        void GetProfile()
        {
            if (CurrentUser is null)
                return;

            Name = CurrentUser.FullName;
            CreditBook = CurrentUser.CreditBook;
            Group = CurrentUser.GroupName;
            GroupDescription = CurrentUser.GroupDescription;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task GroupTextChanged()
        {
            if (!IsEdit || _groupSelected)
                return;

            var current = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref _groupThrottleCts, current);
            previous?.Cancel();
            try
            {
                await Task.Delay(150, current.Token);
                var query = Group.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    GroupHint = DefaultGroupHint;
                    IsGroupListVisible = false;
                    GroupList.Clear();
                    return;
                }

                if (query.Length < 2)
                {
                    GroupHint = ShortGroupHint;
                    IsGroupListVisible = false;
                    GroupList.Clear();
                    return;
                }

                var groups = (await _userRequests.GetGroupAsync(query, current.Token)).ToList();
                current.Token.ThrowIfCancellationRequested();
                IsGroupListVisible = true;
                GroupList = groups.Count > 0
                    ? groups
                    : ["Ничего не найдено"];
                GroupHint = groups.Count > 0
                    ? "Выберите группу из списка или продолжите ввод"
                    : "Группа не найдена. Проверьте код, например ИС-21";
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                GroupList = [];
                IsGroupListVisible = false;
                GroupHint = "Не удалось загрузить подсказки. Проверьте соединение";
            }
            finally
            {
                Interlocked.CompareExchange(ref _groupThrottleCts, null, current);
                current.Dispose();
            }
        }

        [RelayCommand]
        void GroupSelected(string group)
        {
            if (string.IsNullOrWhiteSpace(group) || group == "Ничего не найдено")
                return;

            _groupSelected = true;
            Group = group;
            IsGroupListVisible = false;
            GroupList.Clear();
            GroupHint = SelectedGroupHint;
            _groupSelected = false;
        }

        void ResetGroupSuggestions()
        {
            Interlocked.Exchange(ref _groupThrottleCts, null)?.Cancel();
            IsGroupListVisible = false;
            GroupList.Clear();
            GroupHint = DefaultGroupHint;
        }

        async Task ChangeSubgroupAsync(CancellationToken cancellationToken)
        {
            if (CurrentUser is null || string.IsNullOrWhiteSpace(CurrentUser.GroupName))
                return;

            var groupInfo = await _userRequests.GetSubGroupsAsync(CurrentUser.GroupName, cancellationToken);
            var subgroups = groupInfo.SubGroups?.ToList() ?? [];
            if (subgroups.Count == 0)
                return;

            var subgroup = await Shell.Current.DisplayActionSheetAsync("Выберите подгруппу",
                null,
                null,
                buttons: subgroups.Select(p => p.Name).ToArray());
            if (subgroup == null)
            {
                return;
            }
            var selectedSubgroup = subgroups.FirstOrDefault(p => p.Name == subgroup);
            if (selectedSubgroup is null)
                return;

            CurrentUser.SubGroupId = selectedSubgroup.GroupId;
            _context.Users.Update(CurrentUser);
            await _context.SaveChangesAsync(cancellationToken);
            await SetCurrentUserAsync(_context, cancellationToken);
            await _navigation.NavigateAsync("///LoadingPage?refresh=true");
        }
        async Task IsHaveSubgroups(CancellationToken cancellationToken)
        {
            if (CurrentUser is null ||
                CurrentUser.Role != UserRole.Student ||
                string.IsNullOrWhiteSpace(Group))
            {
                SubGroupButtonIsVisible = false;
                return;
            }

            var subgroups = await _userRequests.GetSubGroupsAsync(Group.Trim(), cancellationToken);
            SubGroupButtonIsVisible = subgroups?.SubGroups?.Any() == true;
        }
        async Task EditInfoAsync(CancellationToken cancellationToken)
        {
            if (CurrentUser is null)
                return;

            if (IsEdit)
            {
                if (string.IsNullOrWhiteSpace(Group))
                {
                    await AppShell.DisplaySnackbarAsync("Укажите группу");
                    return;
                }
                var normalizedGroup = Group.Trim();
                var groupChanged = string.IsNullOrWhiteSpace(_user.GroupId) ||
                    !string.Equals(normalizedGroup, _user.GroupName.Trim(), StringComparison.OrdinalIgnoreCase);
                if (groupChanged && !await _validationRequests.CheckGroupAsync(normalizedGroup, cancellationToken))
                {
                    await AppShell.DisplaySnackbarAsync("Данной группы нет в списке");
                    return;
                }
                if (groupChanged)
                {
                    var groupInfo = await _userRequests.GetSubGroupsAsync(normalizedGroup, cancellationToken);
                    CurrentUser.GroupName = normalizedGroup;
                    CurrentUser.GroupId = groupInfo.GroupId;
                    CurrentUser.GroupDescription = groupInfo.Speciality;
                    CurrentUser.SubGroupId = string.Empty;
                    SubGroupButtonIsVisible = groupInfo.SubGroups?.Any() == true;
                    _subgroupsLoadedForGroup = normalizedGroup;
                }
                CurrentUser.CreditBook = CreditBook?.Trim() ?? string.Empty;
                _context.Users.Update(CurrentUser);
                await _context.SaveChangesAsync(cancellationToken);
                await SetCurrentUserAsync(_context, cancellationToken);
                _user = CurrentUser;
                IsEdit = false;
                ResetGroupSuggestions();
                ButtonEditLabel = "Редактировать";
                if (groupChanged)
                {
                    await _navigation.NavigateAsync("///LoadingPage?refresh=true");
                }
                return;
            }
            ButtonEditLabel = "Сохранить";
            IsEdit = true;
            GroupHint = DefaultGroupHint;
        }

        private async Task RunSafelyAsync(
            Func<CancellationToken, Task> action,
            string errorMessage,
            CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                await action(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync(errorMessage);
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void CancelPendingOperations()
        {
            _loadData.Cancel();
            Edit.Cancel();
            Change.Cancel();
            Exit.Cancel();
            Interlocked.Exchange(ref _groupThrottleCts, null)?.Cancel();
        }
    }
}
