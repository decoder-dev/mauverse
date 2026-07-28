using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.Utils.Services;
using mau.ViewModel.Schedules;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mau.ViewModel
{
    public partial class LoadingViewModel : BaseViewModel, IQueryAttributable
    {
        private static readonly TimeSpan SessionValidationBudget = TimeSpan.FromMilliseconds(2500);

        private readonly DbConnect _context;
        private readonly IAPIService _apiService;
        private readonly IAppNavigationService _navigation;

        [ObservableProperty]
        string _progressLabel = "Инициализация...";
        bool _isRefresh;
        string _redirectWindow = string.Empty;
        public LoadingViewModel(
            DbConnect context,
            IAPIService service,
            IAppNavigationService navigation) : base(context)
        {
            _context = context;
            _apiService = service;
            _navigation = navigation;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _isRefresh = query.TryGetValue("refresh", out var refreshValue) &&
                (refreshValue is true || bool.TryParse(
                    Convert.ToString(refreshValue, System.Globalization.CultureInfo.InvariantCulture),
                    out var refresh) && refresh);
            _redirectWindow = query.TryGetValue("redirect", out var redirect)
                ? Convert.ToString(redirect, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty
                : string.Empty;
        }

        [RelayCommand]
        private async Task PerformNavigation(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                ProgressLabel = "Инициализация...";
                await Task.Yield();
                await _context.EnsureDatabaseCreatedAsync(cancellationToken);
                ProgressLabel = "Проверяем подключение...";
                await GetInternetConnectionInfoAsync(cancellationToken);
                if (await _context.Users.AsNoTracking().AnyAsync(cancellationToken))
                {
                    if (CurrentUser is null)
                        await SetCurrentUserAsync(_context, cancellationToken);
                    if (CurrentUser is null)
                    {
                        await ClearSessionAndNavigateToLoginAsync(
                            "Сохранённая сессия недействительна. Войдите снова");
                        return;
                    }
                    await UserCredentialStore.MigrateLegacyAsync(_context, cancellationToken);
                    await UserCredentialStore.RestoreAsync(CurrentUser, cancellationToken);
                    if (string.IsNullOrWhiteSpace(CurrentUser.Token))
                    {
                        await ClearSessionAndNavigateToLoginAsync(
                            "Сохранённая сессия недействительна. Войдите снова");
                        return;
                    }
                    _apiService.SetHttpHeaders(CurrentUser.Username, CurrentUser.Token);
                    var hasInternet = CheckConnection();
                    if (CurrentUser.Role == UserRole.Student &&
                        (string.IsNullOrWhiteSpace(CurrentUser.GroupName) || string.IsNullOrWhiteSpace(CurrentUser.GroupId)))
                    {
                        if (hasInternet)
                        {
                            ProgressLabel = "Проверяем сессию...";
                            await ValidateSessionAsync(cancellationToken);
                        }
                        await _navigation.NavigateAsync($"///main/{nameof(ProfilePage)}");
                        await AppShell.DisplaySnackbarAsync("Укажите учебную группу в профиле");
                        return;
                    }
                    using var scheduleRequests = new ScheduleRequests(_apiService);
                    var shouldRefreshSchedule = _isRefresh && hasInternet;
                    if (shouldRefreshSchedule)
                    {
                        ProgressLabel = "Обновляем расписание...";
                        await BaseScheduleViewModel.LoadSchedule(
                            CurrentUser,
                            scheduleRequests,
                            isRefresh: true,
                            cancellationToken);
                        Preferences.Default.Set(
                            "schedule_last_sync_utc",
                            DateTimeOffset.UtcNow.ToString("O"));
                    }
                    else
                    {
                        ProgressLabel = hasInternet
                            ? "Проверяем сессию..."
                            : "Загружаем сохранённые данные...";
                        var scheduleTask = BaseScheduleViewModel.LoadSchedule(
                            CurrentUser,
                            scheduleRequests,
                            cancellationToken: cancellationToken);
                        if (hasInternet)
                        {
                            var sessionTask = ValidateSessionAsync(cancellationToken);
                            await Task.WhenAll(scheduleTask, sessionTask);
                        }
                        else
                        {
                            await scheduleTask;
                        }
                    }

                    ProgressLabel = "Готово";
                    if (!string.IsNullOrEmpty(_redirectWindow))
                    {
                        await _navigation.NavigateAsync($"///main/{_redirectWindow}");
                    }
                    else
                    {
                        await _navigation.NavigateAsync($"///main/{nameof(MainPage)}");
                    }
                }
                else
                {
                    await _navigation.NavigateAsync($"///{nameof(LoginPage)}");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (HttpRequestException ex) when (IsAuthenticationFailure(ex))
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await ClearSessionAndNavigateToLoginAsync(
                    "Сессия истекла или была отозвана. Войдите снова");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось обновить данные. Используется сохранённая информация");
                await NavigateAfterFailureAsync();
            }
            finally
            {
                CurrentState = States.Empty;
                IsBusy = false;
            }
        }

        private async Task ValidateSessionAsync(CancellationToken cancellationToken)
        {
            using var validationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            validationSource.CancelAfter(SessionValidationBudget);
            try
            {
                await _apiService.PostAsync<JsonElement>(
                    "/get_user_info",
                    data: null,
                    cancellationToken: validationSource.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // A slow health check must not hold the user on the startup screen.
                System.Diagnostics.Debug.WriteLine("Session validation exceeded the startup budget");
            }
        }

        private static bool IsAuthenticationFailure(HttpRequestException exception) =>
            exception.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;

        private async Task ClearSessionAndNavigateToLoginAsync(string message)
        {
            try
            {
                await DeleteDataAndExitAsync(
                    _context,
                    _apiService,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            await _navigation.NavigateAsync($"///{nameof(LoginPage)}?login=true");
            if (Shell.Current is AppShell shell)
                shell.ResetAccountScopedPages();
            await AppShell.DisplaySnackbarAsync(message);
        }

        private async Task NavigateAfterFailureAsync()
        {
            var hasLocalUser = false;
            try
            {
                hasLocalUser = await _context.Users.AsNoTracking().AnyAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                _context.ChangeTracker.Clear();
            }

            var route = hasLocalUser
                ? $"///main/{nameof(MainPage)}"
                : $"///{nameof(LoginPage)}";
            await _navigation.NavigateAsync(route);
        }

        protected override void CancelPendingOperations() => PerformNavigationCommand.Cancel();
    }
}
