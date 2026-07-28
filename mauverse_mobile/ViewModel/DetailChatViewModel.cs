using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.Services.Interface;

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace mau.ViewModel
{
    public class DetailChatViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly MoodleRequests _moodleRequests = new();
        private readonly IAppNavigationService _navigation;
        private ObservableCollection<MessageDTO> _messages = [];
        private int _conversationId;
        private string _contactFullname = string.Empty;
        private string _message = string.Empty;

        public DetailChatViewModel(DbConnect context, IAppNavigationService navigation) : base(context)
        {
            _navigation = navigation;
            LoadAllData = new AsyncRelayCommand(LoadData);
            SendMessageCommand = new AsyncRelayCommand(SendMessage);
            BackCommand = new AsyncRelayCommand(NavigateBackAsync);
        }

        public IAsyncRelayCommand LoadAllData { get; }
        public IAsyncRelayCommand SendMessageCommand { get; }
        public IAsyncRelayCommand BackCommand { get; }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        private async Task SendMessage(CancellationToken cancellationToken)
        {
            var text = Message?.Trim();
            if (IsBusy || string.IsNullOrEmpty(text) || _conversationId <= 0 || CurrentUser is null)
                return;

            IsBusy = true;
            try
            {
                var sentMessage = await _moodleRequests.SendMessage(
                    CurrentUser.Token,
                    _conversationId,
                    text,
                    cancellationToken);
                if (sentMessage is null)
                {
                    await AppShell.DisplaySnackbarAsync("Не удалось отправить сообщение");
                    return;
                }

                FormatMessage(sentMessage);
                Messages.Add(sentMessage);
                Message = string.Empty;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось отправить сообщение");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadData(CancellationToken cancellationToken)
        {
            if (IsBusy || CurrentUser is null || string.IsNullOrWhiteSpace(CurrentUser.Token) || _conversationId <= 0)
                return;

            IsBusy = true;
            try
            {
                var messages = await _moodleRequests.GetChatMessages(
                    CurrentUser.Token,
                    CurrentUser.UserId,
                    _conversationId,
                    cancellationToken);
                Messages.Clear();
                foreach (var message in messages.TakeLast(15))
                {
                    if (message is null)
                        continue;
                    FormatMessage(message);
                    Messages.Add(message);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить сообщения. Повторите позже");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void FormatMessage(MessageDTO message)
        {
            var timestamp = message.TimeCreated is >= -62135596800 and <= 253402300799
                ? DateTimeOffset.FromUnixTimeSeconds(message.TimeCreated).LocalDateTime
                : DateTime.Now;
            message.TimeCreateString = $"от {timestamp:t}";
            message.Text = System.Net.WebUtility.HtmlDecode(
                Regex.Replace(message.Text ?? string.Empty, @"<[^>]*>", string.Empty));
            message.FullnameFrom = message.UserIdFrom == CurrentUser.UserId ? "Вы" : ContactFullname;
        }

        private async Task NavigateBackAsync()
        {
            try
            {
                await _navigation.GoBackAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось вернуться назад");
            }
        }

        public ObservableCollection<MessageDTO> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        public string ContactFullname
        {
            get => _contactFullname;
            set
            {
                _contactFullname = value;
                OnPropertyChanged();
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            LoadAllData.Cancel();
            SendMessageCommand.Cancel();
            _conversationId = query.TryGetValue("conversation_id", out var conversationId) && conversationId is int id
                ? id
                : 0;
            ContactFullname = query.TryGetValue("contact_fullname", out var contactName)
                ? contactName?.ToString() ?? string.Empty
                : string.Empty;
            Messages = [];
        }

        protected override void CancelPendingOperations()
        {
            LoadAllData.Cancel();
            SendMessageCommand.Cancel();
        }
    }
}
