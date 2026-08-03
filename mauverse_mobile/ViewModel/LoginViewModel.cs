using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;

using System.Net;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.Utils.Services;

using Microsoft.EntityFrameworkCore;

namespace mau.ViewModel
{
    public class LoginViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly DbConnect _context;
        private readonly IAPIService _apiService;
        private readonly IUserRequests _userRequests;
        private readonly IAppNavigationService _navigation;
        private string _password = string.Empty;
        private string _username = string.Empty;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("login"))
            {
                CurrentState = States.Empty;
            }
        }

        public LoginViewModel(
            DbConnect context,
            IAPIService service,
            IUserRequests userRequests,
            IAppNavigationService navigation) : base(context)
        {
            _apiService = service;
            _userRequests = userRequests;
            _context = context;
            _navigation = navigation;
            CurrentState = States.Empty;
            Auth = new AsyncRelayCommand(Login);
        }

        public IAsyncRelayCommand Auth { get; }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        private async Task Login(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            _apiService.RemoveHttpHeaders();
            if (string.IsNullOrWhiteSpace(Username))
            {
                Password = string.Empty;
                await AppShell.DisplayToastAsync("Введите логин");
                return;
            }
            if (string.IsNullOrEmpty(Password))
            {
                Password = string.Empty;
                await AppShell.DisplayToastAsync("Введите пароль");
                return;
            }

            var loginSucceeded = false;
            try
            {
                IsBusy = true;
                CurrentState = States.Loading;
                var user = await _userRequests.Auth(Username.Trim(), Password, cancellationToken);
                if (user is null)
                {
                    CurrentState = States.Empty;
                    var exceptionPopup = new ExceptionPopup("Не удалось найти информацию о пользователе", "Проверьте введённые данные или повторите попытку позднее.");
                    await Shell.Current.CurrentPage.ShowPopupAsync(
                        exceptionPopup,
                        PopupOptions.Empty,
                        cancellationToken);
                    return;
                }
                if (user.Error is not null)
                {
                    CurrentState = States.Empty;
                    var exceptionPopup = new ExceptionPopup("Не удалось войти", "Проверьте логин и пароль или повторите попытку позднее.");
                    await Shell.Current.CurrentPage.ShowPopupAsync(
                        exceptionPopup,
                        PopupOptions.Empty,
                        cancellationToken);
                    return;
                }
                if (string.IsNullOrWhiteSpace(user.Token))
                {
                    CurrentState = States.Empty;
                    await AppShell.DisplaySnackbarAsync("Не удалось завершить вход. Повторите попытку позднее");
                    return;
                }
                _apiService.SetHttpHeaders(Username.Trim(), user.Token);
                await CreateUserAsync(user, cancellationToken);
                await SetCurrentUserAsync(_context, cancellationToken);
                Username = string.Empty;
                Password = string.Empty;
                await _navigation.NavigateAsync("///LoadingPage?refresh=true");
                loginSucceeded = true;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync(GetLoginErrorMessage(ex));
            }
            finally
            {
                if (!loginSucceeded)
                {
                    CurrentState = States.Empty;
                    _context.ChangeTracker.Clear();
                    _apiService.RemoveHttpHeaders();
                    Password = string.Empty;
                }
                IsBusy = false;
            }
        }

        private static string GetLoginErrorMessage(Exception exception) =>
            exception switch
            {
                TimeoutException => "Сервер не ответил вовремя. Повторите попытку",
                HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden } =>
                    "Неверный логин или пароль",
                HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } =>
                    "Слишком много попыток входа. Подождите немного и попытайтесь снова",
                HttpRequestException
                {
                    StatusCode: HttpStatusCode.BadGateway or
                        HttpStatusCode.RequestTimeout or
                        HttpStatusCode.ServiceUnavailable or
                        HttpStatusCode.GatewayTimeout
                } =>
                    "Сервис авторизации временно недоступен. Повторите попытку позднее",
                HttpRequestException { StatusCode: null } =>
                    "Нет подключения к интернету. Проверьте сеть и повторите попытку",
                InvalidOperationException =>
                    "Не удалось войти. Проверьте логин и пароль",
                _ => "Не удалось войти. Повторите попытку позднее"
            };

        private async Task CreateUserAsync(UserDTO existingUser, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(existingUser.Speciality))
            {
                existingUser.Speciality = "Не указано";
            }

            existingUser.CreditBook = string.Empty;
            existingUser.SubGroupId = string.Empty;
            var user = new User
            {
                UserId = existingUser.UserId,
                Username = existingUser.Username,
                FirstName = existingUser.FirstName,
                FullName = existingUser.FullName,
                Role = existingUser.Role,
                CreditBook = existingUser.CreditBook,
                GroupId = existingUser.GroupId,
                SubGroupId = existingUser.SubGroupId,
                GroupName = existingUser.GroupName,
                GroupDescription = existingUser.Speciality,
                Token = existingUser.Token,
                PrivateToken = existingUser.PrivateToken,
            };

            if (existingUser.Role == UserRole.Student && !string.IsNullOrWhiteSpace(existingUser.GroupName))
            {
                var userSubgroups = await _userRequests.GetSubGroupsAsync(existingUser.GroupName, cancellationToken);
                while (user.SubGroupId == string.Empty && userSubgroups.SubGroups.Any())
                {
                    var subgroup = await Shell.Current.DisplayActionSheetAsync(
                        "Выберите подгруппу",
                        "Выбрать позже в профиле",
                        null,
                        buttons: userSubgroups.SubGroups.Select(p => p.Name).ToArray());
                    if (subgroup is null || subgroup == "Выбрать позже в профиле")
                    {
                        await AppShell.DisplaySnackbarAsync(
                            "Подгруппа не выбрана — укажите её в профиле, чтобы расписание было точным");
                        break;
                    }

                    var selectedSubgroup = userSubgroups.SubGroups.FirstOrDefault(p => p.Name == subgroup);
                    if (selectedSubgroup != null)
                    {
                        user.SubGroupId = selectedSubgroup.GroupId;
                    }
                }
            }
            await UserCredentialStore.MigrateLegacyAsync(_context, cancellationToken);
            await UserCredentialStore.SaveAsync(user, cancellationToken);
            user.Token = string.Empty;
            user.PrivateToken = string.Empty;
            if (!await _context.Users.AsNoTracking().AnyAsync(p => p.UserId == user.UserId, cancellationToken))
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        protected override void CancelPendingOperations() => Auth.Cancel();
    }
}
