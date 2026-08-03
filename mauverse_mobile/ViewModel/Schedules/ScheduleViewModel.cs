using CommunityToolkit.Maui.Views;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Caching.Memory;

namespace mau.ViewModel.Schedules
{
    public partial class ScheduleViewModel : BaseScheduleViewModel
    {

        [ObservableProperty]
        List<WeekDaysDTO> _days = [];

        [ObservableProperty]
        List<Room> _roomsList = [];

        [ObservableProperty]
        List<SubGroup> _subgroups = [];

        [ObservableProperty]
        List<string> _teacherList = [];

        [ObservableProperty]
        WeekDaysDTO _selectedDay = null!;

        [ObservableProperty]
        SubGroup? _selectedSubgroup;

        [ObservableProperty]
        bool _filtersIsVisible;

        [ObservableProperty]
        bool _teacherFilterListIsVisible;

        [ObservableProperty]
        bool _roomFilterListIsVisible;

        [ObservableProperty]
        string _currentScheduleLabel = string.Empty;

        [ObservableProperty]
        string _showLabel = "Показать";

        [ObservableProperty]
        string _roomName = string.Empty;

        [ObservableProperty]
        string _teacherName = string.Empty;

        [ObservableProperty]
        Room? _selectedRoom;

        [ObservableProperty]
        bool _isHaveSubgroups;

        bool _isFiltered;
        bool _isInitialized;
        string? _subgroupsLoadedForGroup;
        List<ScheduleNote> _defaultSchedules = [];
        private readonly IUserRequests _userRequests;
        private readonly IParserRequests _parserRequests;
        private readonly IMemoryCache _memoryCache;
        private readonly IAppNavigationService _navigation;
        private CancellationTokenSource? _throttleCts;
        private CancellationTokenSource? _filterCatalogCts;
        private Task? _filterCatalogLoadTask;
        private bool _filterCatalogsLoaded;
        private long _renderedScheduleVersion = -1;
        private DateTime _renderedWeekStart;
        private DateTime _currentWeek;

        public ScheduleViewModel(
            DbConnect context,
            IScheduleRequests scheduleRequests,
            IUserRequests userRequests,
            IParserRequests parserRequests,
            IMemoryCache memoryCache,
            IAppNavigationService navigation) : base(context, scheduleRequests, navigation)
        {
            _filtersIsVisible = false;
            _userRequests = userRequests;
            _parserRequests = parserRequests;
            _memoryCache = memoryCache;
            _navigation = navigation;
            _currentWeek = DateTime.Now;
        }

        [RelayCommand]
        void SelectDay()
        {
            if (SelectedDay is null)
                return;

            CurrentScheduleLabel = $"{SelectedDay.Day} {SelectedDay.Month}";
            DaySelected();
        }

        [RelayCommand]
        async Task LoadData(CancellationToken cancellationToken)
        {
            var currentWeekStart = GetWeekStart(DateTime.Today);
            var shouldRender = _renderedScheduleVersion != ScheduleVersion || _renderedWeekStart != currentWeekStart;
            if (shouldRender)
            {
                RenderSchedule();
                if (!_isFiltered)
                    _defaultSchedules = [.. ScheduleList];
            }
            if (_isInitialized)
                return;

            IsBusy = true;
            try
            {
                await LoadSubgroupsFilterAsync(cancellationToken);
                _isInitialized = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось подготовить расписание");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task FilterData(CancellationToken cancellationToken)
        {
            var teacher = TeacherName?.Trim() ?? string.Empty;
            if (string.Equals(teacher, "Ничего не найдено", StringComparison.Ordinal))
                return;

            if (!string.IsNullOrWhiteSpace(teacher))
            {
                SelectedRoom = null;
                RoomName = string.Empty;
                await FilterScheduleAsync(teacher: teacher, cancellationToken: cancellationToken);
            }
            else if (SelectedRoom is { RoomId: > 0 })
            {
                TeacherName = string.Empty;
                RoomName = SelectedRoom.Name;
                await FilterScheduleAsync(roomId: SelectedRoom.RoomId, cancellationToken: cancellationToken);
            }
            else
            {
                return;
            }

            TeacherList.Clear();
            RoomsList.Clear();
            TeacherFilterListIsVisible = false;
            RoomFilterListIsVisible = false;
            _isFiltered = true;
        }

        [RelayCommand]
        async Task ShowFilters(CancellationToken cancellationToken)
        {
            FiltersIsVisible = !FiltersIsVisible;
            ShowLabel = FiltersIsVisible ? "Скрыть" : "Показать";
            if (FiltersIsVisible)
            {
                await EnsureFilterCatalogsLoadedAsync(cancellationToken);
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task TeacherFilterChanged()
        {
            await TeacherTextChangedAsync();
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task RoomFilterChanged()
        {
            await RoomTextChangedAsync();
        }

        [RelayCommand]
        async Task FilterBySubgroup(CancellationToken cancellationToken)
        {
            if (SelectedSubgroup is null)
                return;

            IsBusy = true;
            IsScheduleBusy = true;
            try
            {
                await LoadSchedule(subgroupId: SelectedSubgroup.GroupId, cancellationToken);
                _defaultSchedules = [.. ScheduleList];
                _isFiltered = false;
                RenderSchedule();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить расписание подгруппы");
            }
            finally
            {
                IsScheduleBusy = false;
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task Refresh()
        {
            TeacherName = string.Empty;
            RoomName = string.Empty;
            await _navigation.NavigateAsync($"///{nameof(LoadingPage)}?refresh=true&redirect={nameof(SchedulePage)}");
        }

        async Task FilterScheduleAsync(
            int roomId = 0,
            string teacher = "",
            CancellationToken cancellationToken = default)
        {
            IsBusy = true;
            IsScheduleBusy = true;
            try
            {
                CurrentScheduleLabel = $"{SelectedDay.Day} {SelectedDay.Month}";
                ScheduleFrameIsVisible = false;
                await LoadSchedule(roomId, teacher, cancellationToken);
                RenderSchedule();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось применить фильтр расписания");
            }
            finally
            {
                IsScheduleBusy = false;
                IsBusy = false;
            }
        }

        async Task LoadSubgroupsFilterAsync(CancellationToken cancellationToken)
        {
            if (CurrentUser is null ||
                CurrentUser.Role != UserRole.Student ||
                string.IsNullOrWhiteSpace(CurrentUser.GroupName))
            {
                IsHaveSubgroups = false;
                Subgroups = [];
                return;
            }

            var normalizedGroup = CurrentUser.GroupName.Trim();
            if (string.Equals(_subgroupsLoadedForGroup, normalizedGroup, StringComparison.OrdinalIgnoreCase))
                return;

            var subgroups = await _userRequests.GetSubGroupsAsync(normalizedGroup, cancellationToken);
            _subgroupsLoadedForGroup = normalizedGroup;
            if (subgroups?.SubGroups?.Any() != true)
            {
                IsHaveSubgroups = false;
                return;
            }
            IsHaveSubgroups = true;
            Subgroups = subgroups.SubGroups.ToList();
        }

        void LoadDaysData()
        {
            var days = new List<WeekDaysDTO>();
            var weekStart = GetWeekStart(_currentWeek);
            string[] dayNames = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];

            for (var week = 0; week < 2; week++)
            {
                for (var dayIndex = 0; dayIndex < dayNames.Length; dayIndex++)
                {
                    var date = weekStart.AddDays(week * 7 + dayIndex);
                    var dayNumber = week * dayNames.Length + dayIndex + 1;
                    var day = new WeekDaysDTO
                    {
                        DayName = dayNames[dayIndex],
                        DayNumber = dayNumber,
                        Day = date.Day.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Month = date.ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")),
                        Date = date.Date
                    };
                    if (!CheckDaySchedule(dayNumber))
                    {
                        day.IsWeekend = true;
                    }
                    days.Add(day);
                }
            }

            Days = new(days);
            SelectedDay = Days.FirstOrDefault(day => day.Date == DateTime.Today) ?? Days.First();
        }

        void RenderSchedule()
        {
            _currentWeek = DateTime.Now;
            LoadDaysData();
            CurrentScheduleLabel = $"{SelectedDay.Day} {SelectedDay.Month}";
            DaySelected();
            _renderedScheduleVersion = ScheduleVersion;
            _renderedWeekStart = GetWeekStart(_currentWeek);
        }

        void DaySelected()
        {
            var schedules = GetScheduleByDate(SelectedDay.DayNumber);
            if (schedules.Count > 0)
            {
                Schedules = new(schedules);
                ScheduleFrameIsVisible = false;
            }
            else
            {
                Schedules.Clear();
                ScheduleFrameIsVisible = true;
            }
        }

        bool CheckDaySchedule(int day)
        {
            var selectedDate = GetDateByDayNumber(day);
            return ScheduleList.Any(p => p.Date.Date == selectedDate.Date);
        }

        List<ScheduleNote> GetScheduleByDate(int day)
        {
            var selectedDate = GetDateByDayNumber(day);
            return ScheduleList.Where(p => p.Date.Date == selectedDate.Date).ToList();
        }

        private DateTime GetDateByDayNumber(int day)
        {
            var zeroBasedDay = Math.Max(day - 1, 0);
            var weekOffset = zeroBasedDay / 6;
            var dayOffset = zeroBasedDay % 6;
            return GetWeekStart(_currentWeek).AddDays(weekOffset * 7 + dayOffset);
        }

        private static DateTime GetWeekStart(DateTime date)
        {
            var diff = date.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - date.DayOfWeek;
            return date.Date.AddDays(diff);
        }

        async Task TeacherTextChangedAsync()
        {
            var throttle = RenewThrottle();
            try
            {
                await Task.Delay(150, throttle.Token);
                await EnsureFilterCatalogsLoadedAsync(throttle.Token);
                throttle.Token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(TeacherName))
                {
                    TeacherFilterListIsVisible = false;
                    TeacherList.Clear();
                    if (_isFiltered)
                    {
                        RestoreDefaultSchedule();
                        _isFiltered = false;
                    }
                    return;
                }
                TeacherList = [.. Teachers.Where(p => p.Contains(TeacherName, StringComparison.OrdinalIgnoreCase)).Take(5)];
                TeacherFilterListIsVisible = true;
                _isFiltered = true;
                if (TeacherList.Count == 0)
                    TeacherList.Add("Ничего не найдено");
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ReleaseThrottle(throttle);
            }
        }

        async Task RoomTextChangedAsync()
        {
            var throttle = RenewThrottle();
            try
            {
                await Task.Delay(200, throttle.Token);
                await EnsureFilterCatalogsLoadedAsync(throttle.Token);
                throttle.Token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(RoomName))
                {
                    RoomFilterListIsVisible = false;
                    RoomsList.Clear();
                    if (_isFiltered)
                    {
                        RestoreDefaultSchedule();
                        _isFiltered = false;
                    }
                    return;
                }
                RoomsList = [.. Rooms.Where(p => p.Name.Contains(RoomName, StringComparison.OrdinalIgnoreCase)).Take(5)];
                RoomFilterListIsVisible = true;
                _isFiltered = true;
                if (RoomsList.Count == 0)
                {
                    RoomsList.Add(new Room { Name = "Ничего не найдено", RoomId = 0 });
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                ReleaseThrottle(throttle);
            }
        }

        private CancellationTokenSource RenewThrottle()
        {
            var current = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref _throttleCts, current);
            previous?.Cancel();
            return current;
        }

        private void ReleaseThrottle(CancellationTokenSource throttle)
        {
            Interlocked.CompareExchange(ref _throttleCts, null, throttle);
            throttle.Dispose();
        }

        private void RestoreDefaultSchedule()
        {
            ScheduleList = [.. _defaultSchedules];
            RenderSchedule();
        }

        private async Task EnsureFilterCatalogsLoadedAsync(CancellationToken cancellationToken)
        {
            if (_filterCatalogsLoaded)
                return;

            if (_filterCatalogLoadTask is null)
            {
                _filterCatalogCts?.Dispose();
                _filterCatalogCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _filterCatalogLoadTask = LoadFilterCatalogsAsync(_filterCatalogCts.Token);
            }
            var loadTask = _filterCatalogLoadTask;
            var loadCancellationSource = _filterCatalogCts;
            try
            {
                await loadTask.WaitAsync(cancellationToken);
            }
            finally
            {
                if (loadTask.IsCompleted && ReferenceEquals(_filterCatalogLoadTask, loadTask))
                {
                    _filterCatalogLoadTask = null;
                    Interlocked.CompareExchange(
                        ref _filterCatalogCts,
                        null,
                        loadCancellationSource);
                    loadCancellationSource?.Dispose();
                }
            }
        }

        private async Task LoadFilterCatalogsAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.WhenAll(
                    LoadTeacherList(_parserRequests, _memoryCache, cancellationToken),
                    LoadRoomList(_parserRequests, _memoryCache, cancellationToken));
                _filterCatalogsLoaded = true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить каталоги фильтров");
            }
        }

        protected override void CancelPendingOperations()
        {
            LoadDataCommand.Cancel();
            FilterDataCommand.Cancel();
            FilterBySubgroupCommand.Cancel();
            Interlocked.Exchange(ref _throttleCts, null)?.Cancel();
            Interlocked.Exchange(ref _filterCatalogCts, null)?.Cancel();
            _filterCatalogLoadTask = null;
        }
    }
}
