using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mau.ViewModel
{
    public class MainChatViewModel : BaseViewModel
    {
        private readonly IParserRequests _parserRequests;
        private readonly IAppNavigationService _navigation;
        private readonly MoodleRequests _moodleRequests = new();
        private ObservableCollection<RecentChatsDTO> _recentChat = [];
        private ObservableCollection<string> _teacherList = [];
        private RecentChatsDTO? _selectedChat;
        private CancellationTokenSource? _searchCts;
        private bool _nothingPanelIsVisible;
        private bool _teacherListIsVisible;
        private string _teacherName = string.Empty;

        public MainChatViewModel(
            DbConnect context,
            IParserRequests parserRequests,
            IAppNavigationService navigation) : base(context)
        {
            _parserRequests = parserRequests;
            _navigation = navigation;
            NothingPanelIsVisible = false;
            TeacherListIsVisible = false;
            TeacherName = string.Empty;
            LoadAllData = new AsyncRelayCommand(LoadData);
            ChatSelected = new AsyncRelayCommand<object?>(OpenChatAsync);
            TextChanged = new AsyncRelayCommand(
                OnTextChangedAsync,
                AsyncRelayCommandOptions.AllowConcurrentExecutions);
            SelectTeacher = new AsyncRelayCommand<string?>(TeacherSelectedAsync);
        }

        public ObservableCollection<RecentChatsDTO> RecentChat
        {
            get { return _recentChat; }
            set
            {
                _recentChat = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> TeacherList
        {
            get { return _teacherList; }
            set
            {
                _teacherList = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TeacherListHeight));
            }
        }

        public double TeacherListHeight => Math.Min(Math.Max(TeacherList.Count, 1), 4) * 52;

        public RecentChatsDTO? SelectedChat
        {
            get { return _selectedChat; }
            set
            {
                _selectedChat = value;
                OnPropertyChanged();
            }
        }

        public bool NothingPanelIsVisible
        {
            get => _nothingPanelIsVisible;
            set
            {
                _nothingPanelIsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool TeacherListIsVisible
        {
            get => _teacherListIsVisible;
            set
            {
                _teacherListIsVisible = value;
                OnPropertyChanged();
            }
        }

        public string TeacherName
        {
            get => _teacherName;
            set
            {
                _teacherName = value;
                OnPropertyChanged();
            }
        }

        public IAsyncRelayCommand<object?> ChatSelected { get; }

        async Task OpenChatAsync(object? obj)
        {
            var chat = obj as UserChatDTO;
            if (chat is null)
                return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "conversation_id", chat.ConvId },
                { "contact_fullname", chat.FullName }
            };
            try
            {
                await _navigation.NavigateAsync("chats/details", parameters: navigationParameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть чат");
            }
        }

        public IAsyncRelayCommand LoadAllData { get; }

        public IAsyncRelayCommand TextChanged { get; }

        public IAsyncRelayCommand<string?> SelectTeacher { get; }

        async Task OnTextChangedAsync()
        {
            var searchCts = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref _searchCts, searchCts);
            previous?.Cancel();

            try
            {
                if (string.IsNullOrWhiteSpace(TeacherName))
                {
                    TeacherList = [];
                    TeacherListIsVisible = false;
                    return;
                }

                await Task.Delay(300, searchCts.Token);
                var teachers = await _parserRequests.GetTeachersAsync(TeacherName.Trim(), searchCts.Token);
                searchCts.Token.ThrowIfCancellationRequested();
                TeacherList = new ObservableCollection<string>(teachers.Take(8));
                TeacherListIsVisible = TeacherList.Count > 0;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                TeacherList = [];
                TeacherListIsVisible = false;
            }
            finally
            {
                Interlocked.CompareExchange(ref _searchCts, null, searchCts);
                searchCts.Dispose();
            }
        }

        async Task TeacherSelectedAsync(string? teacher, CancellationToken cancellationToken)
        {
            TeacherListIsVisible = false;
            try
            {
                if (string.IsNullOrWhiteSpace(teacher))
                    return;

                var teacherNameList = teacher.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (teacherNameList.Length < 3)
                    throw new InvalidOperationException("Укажите ФИО преподавателя полностью");

                var formattedName = $"{teacherNameList[1]} {teacherNameList[2]} {teacherNameList[0]}";
                var userChat = await _moodleRequests.GetContactToChat(
                    CurrentUser.Token,
                    CurrentUser.UserId,
                    formattedName,
                    cancellationToken);
                if (userChat is null)
                    throw new InvalidOperationException("Преподаватель не найден");
                var navigationParameters = new Dictionary<string, object>
                {
                    { "conversation_id", userChat.ConvId},
                    { "contact_fullname", userChat.FullName }
                };
                await _navigation.NavigateAsync("chats/details", parameters: navigationParameters);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Проверьте ФИО преподавателя и интернет-соединение");
                return;
            }
            finally
            {
                TeacherList = [];
                TeacherName = string.Empty;
            }
        }

        async Task LoadData(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            if (CurrentUser is null || string.IsNullOrWhiteSpace(CurrentUser.Token))
            {
                NothingPanelIsVisible = true;
                await AppShell.DisplaySnackbarAsync("Для сообщений ЭИОС необходимо войти повторно");
                return;
            }

            IsBusy = true;
            try
            {
                var recentMessages = await _moodleRequests.GetMessages(
                    CurrentUser.Token,
                    CurrentUser.UserId,
                    cancellationToken);

                var recentChats = new ObservableCollection<RecentChatsDTO>();
                foreach (var message in recentMessages)
                {
                    var user = message.Members?.FirstOrDefault();
                    var userMessage = message.Messages?.FirstOrDefault();
                    if (user != null && userMessage != null)
                    {
                        var createdAt = FromUnixTimeSecondsOrNow(userMessage.TimeCreated).LocalDateTime;
                        userMessage.TimeCreateString = createdAt.Date == DateTime.Today
                            ? createdAt.ToString("t", System.Globalization.CultureInfo.CurrentCulture)
                            : createdAt.ToString("dd.MM", System.Globalization.CultureInfo.InvariantCulture);
                        userMessage.Text = System.Net.WebUtility.HtmlDecode(
                            Regex.Replace(userMessage.Text ?? string.Empty, @"<[^>]*>", string.Empty));
                        user.ConvId = message.Id;
                        recentChats.Add(new RecentChatsDTO
                        {
                            User = user,
                            Message = userMessage
                        });
                    }
                }

                RecentChat = recentChats;
                NothingPanelIsVisible = recentChats.Count == 0;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                NothingPanelIsVisible = RecentChat.Count == 0;
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить список чатов");
            }
            finally
            {
                IsBusy = false;
            }
        }

        static DateTimeOffset FromUnixTimeSecondsOrNow(long value) =>
            value is >= -62135596800 and <= 253402300799
                ? DateTimeOffset.FromUnixTimeSeconds(value)
                : DateTimeOffset.Now;

        protected override void CancelPendingOperations()
        {
            LoadAllData.Cancel();
            SelectTeacher.Cancel();
            Interlocked.Exchange(ref _searchCts, null)?.Cancel();
        }
    }
}
