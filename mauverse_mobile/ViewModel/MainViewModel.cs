using mau.Database;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel.Schedules;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace mau.ViewModel
{
    public class MainViewModel : BaseScheduleViewModel
    {
        const string LastScheduleSyncKey = "schedule_last_sync_utc";
        static readonly TimeSpan NotificationRefreshInterval = TimeSpan.FromMinutes(2);

        readonly IUserRequests _userRequests;
        readonly IAppNavigationService _navigation;
        ObservableCollection<Message> _notifications = [];
        bool _notificationFrameIsVisible;
        bool _isProfileSetupRequired;
        bool _hasNextLesson;
        string _greetingsLabel = string.Empty;
        string _todayLabel = string.Empty;
        string _contextLabel = string.Empty;
        string _notificationStateText = string.Empty;
        string _syncStatusLabel = string.Empty;
        string _scheduleSummaryLabel = string.Empty;
        string _nextLessonStatusLabel = string.Empty;
        Schedule? _nextLesson;
        DateTimeOffset _lastNotificationAttempt;
        bool _hasLoaded;
        long _renderedScheduleVersion = -1;
        DateTime _renderedScheduleDate;

        public MainViewModel(
            DbConnect context,
            IScheduleRequests scheduleRequests,
            IUserRequests userRequests,
            IAppNavigationService navigation) : base(context, scheduleRequests, navigation)
        {
            _userRequests = userRequests;
            _navigation = navigation;
            LoadData = new AsyncRelayCommand(LoadHomeAsync);
            OpenProfile = CreateNavigationCommand($"///main/{nameof(ProfilePage)}", false);
            OpenSchedule = CreateNavigationCommand($"///main/{nameof(SchedulePage)}", false);
            OpenServices = CreateNavigationCommand($"///main/{nameof(ServiceListPage)}", false);
            OpenNews = CreateNavigationCommand($"///main/{nameof(NewsPage)}", false);
            Refresh = CreateNavigationCommand($"///{nameof(LoadingPage)}?refresh=true", true);
            OpenNotification = new AsyncRelayCommand<Message?>(OpenNotificationAsync);
        }

        public IAsyncRelayCommand LoadData { get; }
        public IAsyncRelayCommand OpenProfile { get; }
        public IAsyncRelayCommand OpenSchedule { get; }
        public IAsyncRelayCommand OpenServices { get; }
        public IAsyncRelayCommand OpenNews { get; }
        public IAsyncRelayCommand Refresh { get; }
        public IAsyncRelayCommand<Message?> OpenNotification { get; }

        async Task OpenNotificationAsync(Message? message)
        {
            if (message is null || !ExternalUri.TryCreateHttp(message.ContextUrl, out var uri))
            {
                await AppShell.DisplaySnackbarAsync("Ссылка уведомления недоступна");
                return;
            }

            try
            {
                await _navigation.OpenBrowserAsync(
                    BrowserDestinationRegistry.CreateUniversityNotification("Уведомление", uri));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть уведомление");
            }
        }

        AsyncRelayCommand CreateNavigationCommand(string route, bool animated) =>
            new AsyncRelayCommand(async () =>
            {
                try
                {
                    await _navigation.NavigateAsync(route, animated);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    await AppShell.DisplaySnackbarAsync("Не удалось открыть раздел");
                }
            });

        async Task LoadHomeAsync(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            PrepareHeader();
            ProcessSchedule();

            if (_hasLoaded && DateTimeOffset.UtcNow - _lastNotificationAttempt < NotificationRefreshInterval)
                return;

            IsBusy = true;
            var shouldThrottleNextAttempt = false;
            if (!_hasLoaded && Notifications.Count == 0)
                NotificationFrameIsVisible = false;

            try
            {
                await LoadNotificationsAsync(cancellationToken);
                shouldThrottleNextAttempt = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                Notifications.Clear();
                NotificationStateText = CheckConnection()
                    ? "Обновления временно недоступны"
                    : "Обновления появятся после подключения";
                NotificationFrameIsVisible = true;
                shouldThrottleNextAttempt = true;
            }
            finally
            {
                if (shouldThrottleNextAttempt)
                {
                    _hasLoaded = true;
                    _lastNotificationAttempt = DateTimeOffset.UtcNow;
                }

                IsBusy = false;
            }
        }

        void PrepareHeader()
        {
            var now = DateTime.Now;
            TodayLabel = Capitalize(now.ToString("dddd, d MMMM", CultureInfo.GetCultureInfo("ru-RU")));
            GreetingsLabel = $"{GetGreeting(now.Hour)}, {GetFirstName()}";
            IsProfileSetupRequired = CurrentUser?.Role == UserRole.Student &&
                (string.IsNullOrWhiteSpace(CurrentUser.GroupName) || string.IsNullOrWhiteSpace(CurrentUser.GroupId));
            ContextLabel = CurrentUser?.Role == UserRole.Student
                ? (IsProfileSetupRequired ? "Завершите настройку профиля" : CurrentUser.GroupName)
                : CurrentUser?.FullName ?? string.Empty;
            SyncStatusLabel = BuildSyncStatus();
        }

        void ProcessSchedule()
        {
            var now = DateTime.Now;
            if (_renderedScheduleVersion == ScheduleVersion && _renderedScheduleDate == now.Date)
            {
                UpdateNextLesson(Schedules, now);
                return;
            }

            var today = ScheduleList
                .Where(item => item.Date.Date == now.Date)
                .OrderBy(item => ParseTime(item.StartTime))
                .ToList();

            Schedules = new(today);
            _renderedScheduleVersion = ScheduleVersion;
            _renderedScheduleDate = now.Date;
            ScheduleFrameIsVisible = today.Count == 0;
            ScheduleSummaryLabel = today.Count == 0
                ? "Занятий нет"
                : $"{today.Count} {GetLessonWord(today.Count)}";

            UpdateNextLesson(today, now);
        }

        void UpdateNextLesson(IEnumerable<Schedule> schedules, DateTime now)
        {
            NextLesson = schedules.FirstOrDefault(item => GetLessonEnd(item) >= now);
            HasNextLesson = NextLesson is not null;
            NextLessonStatusLabel = NextLesson is null ? string.Empty : BuildLessonStatus(NextLesson, now);
        }

        async Task LoadNotificationsAsync(CancellationToken cancellationToken)
        {
            if (!CheckConnection())
            {
                Notifications.Clear();
                NotificationStateText = "Обновления появятся после подключения";
                NotificationFrameIsVisible = true;
                return;
            }

            if (CurrentUser is null || string.IsNullOrWhiteSpace(CurrentUser.Token))
            {
                Notifications.Clear();
                NotificationStateText = "Обновления недоступны: войдите повторно";
                NotificationFrameIsVisible = true;
                return;
            }

            var notifications = await _userRequests.GetNotifications(
                CurrentUser.Token,
                CurrentUser.UserId,
                cancellationToken);
            Notifications = new(notifications);
            NotificationStateText = notifications.Count == 0
                ? "Новых обновлений нет"
                : string.Empty;
            NotificationFrameIsVisible = notifications.Count == 0;
        }

        static string BuildSyncStatus()
        {
            var rawValue = Preferences.Default.Get(LastScheduleSyncKey, string.Empty);
            if (!DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var syncedAt))
                return CheckConnection() ? "Сохраненные данные" : "Офлайн";

            var localTime = syncedAt.ToLocalTime();
            return CheckConnection()
                ? $"Обновлено в {localTime:HH:mm}"
                : $"Офлайн, данные на {localTime:HH:mm}";
        }

        static string BuildLessonStatus(Schedule lesson, DateTime now)
        {
            var start = lesson.Date.Date.Add(ParseTime(lesson.StartTime));
            var end = lesson.Date.Date.Add(ParseTime(lesson.EndTime));
            if (now >= start && now <= end)
                return $"Идет сейчас, до {lesson.EndTime}";

            var remaining = start - now;
            if (remaining.TotalMinutes < 60)
                return $"Через {Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes))} мин";

            return $"Начало в {lesson.StartTime}";
        }

        static DateTime GetLessonEnd(Schedule lesson) =>
            lesson.Date.Date.Add(ParseTime(lesson.EndTime));

        static TimeSpan ParseTime(string value) =>
            TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var time) ? time : TimeSpan.Zero;

        static string GetGreeting(int hour) => hour switch
        {
            < 6 => "Доброй ночи",
            < 12 => "Доброе утро",
            < 18 => "Добрый день",
            < 23 => "Добрый вечер",
            _ => "Доброй ночи"
        };

        static string GetFirstName()
            => UserGreeting.ResolveFirstName(
                CurrentUser?.FirstName,
                CurrentUser?.FullName,
                CurrentUser?.Username);

        static string Capitalize(string value) => string.IsNullOrEmpty(value)
            ? value
            : char.ToUpper(value[0], CultureInfo.GetCultureInfo("ru-RU")) + value[1..];

        static string GetLessonWord(int count)
        {
            var lastTwoDigits = count % 100;
            if (lastTwoDigits is >= 11 and <= 14)
                return "занятий";

            return (count % 10) switch
            {
                1 => "занятие",
                2 or 3 or 4 => "занятия",
                _ => "занятий"
            };
        }

        protected override void CancelPendingOperations()
        {
            LoadData.Cancel();
            Refresh.Cancel();
        }

        public ObservableCollection<Message> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }

        public string GreetingsLabel
        {
            get => _greetingsLabel;
            set => SetProperty(ref _greetingsLabel, value);
        }

        public string TodayLabel
        {
            get => _todayLabel;
            set => SetProperty(ref _todayLabel, value);
        }

        public string ContextLabel
        {
            get => _contextLabel;
            set => SetProperty(ref _contextLabel, value);
        }

        public string NotificationStateText
        {
            get => _notificationStateText;
            set => SetProperty(ref _notificationStateText, value);
        }

        public string SyncStatusLabel
        {
            get => _syncStatusLabel;
            set => SetProperty(ref _syncStatusLabel, value);
        }

        public string ScheduleSummaryLabel
        {
            get => _scheduleSummaryLabel;
            set => SetProperty(ref _scheduleSummaryLabel, value);
        }

        public string NextLessonStatusLabel
        {
            get => _nextLessonStatusLabel;
            set => SetProperty(ref _nextLessonStatusLabel, value);
        }

        public Schedule? NextLesson
        {
            get => _nextLesson;
            set => SetProperty(ref _nextLesson, value);
        }

        public bool NotificationFrameIsVisible
        {
            get => _notificationFrameIsVisible;
            set => SetProperty(ref _notificationFrameIsVisible, value);
        }

        public bool IsProfileSetupRequired
        {
            get => _isProfileSetupRequired;
            set
            {
                if (SetProperty(ref _isProfileSetupRequired, value))
                {
                    OnPropertyChanged(nameof(IsProfileReady));
                    OnPropertyChanged(nameof(ShowNoNextLesson));
                }
            }
        }

        public bool IsProfileReady => !IsProfileSetupRequired;

        public bool ShowNoNextLesson => IsProfileReady && !HasNextLesson;

        public bool HasNextLesson
        {
            get => _hasNextLesson;
            set
            {
                if (SetProperty(ref _hasNextLesson, value))
                    OnPropertyChanged(nameof(ShowNoNextLesson));
            }
        }
    }
}
