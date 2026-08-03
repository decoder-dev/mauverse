using CommunityToolkit.Maui.Views;

using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.Dialogs.Note;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace mau.ViewModel.Schedules
{
    public class BaseScheduleViewModel : BaseViewModel
    {
        private static List<ScheduleNote> _scheduleList = [];
        private static long _scheduleVersion;

        private readonly IScheduleRequests _scheduleRequests;
        private readonly DbConnect _context;
        private readonly IAppNavigationService _navigation;
        private ObservableCollection<Schedule> _schedules = [];
        private bool _scheduleFrameIsVisible;
        private bool _isScheduleBusy;

        public BaseScheduleViewModel(
            DbConnect context,
            IScheduleRequests scheduleRequests,
            IAppNavigationService navigation) : base(context)
        {
            _scheduleRequests = scheduleRequests;
            _context = context;
            _navigation = navigation;
            ShowMenu = new AsyncRelayCommand<ButtonParameters?>(ShowMenuAsync);
            ShowNote = new AsyncRelayCommand<ButtonParameters?>(ShowNotePopupAsync);
        }

        public static List<ScheduleNote> ScheduleList
        {
            get => _scheduleList;
            set
            {
                _scheduleList = value;
                Interlocked.Increment(ref _scheduleVersion);
            }
        }

        protected static long ScheduleVersion => Interlocked.Read(ref _scheduleVersion);
        public static List<string> Teachers { get; set; } = [];
        public static List<Room> Rooms { get; set; } = [];

        public IAsyncRelayCommand<ButtonParameters?> ShowMenu { get; }
        public IAsyncRelayCommand<ButtonParameters?> ShowNote { get; }

        async Task ShowMenuAsync(ButtonParameters? obj)
        {
            if (obj is null)
                return;

            try
            {
                var popup = new MenuPopup(_context, obj.Id, _navigation);
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть действия занятия");
            }
        }

        async Task ShowNotePopupAsync(ButtonParameters? obj)
        {
            if (obj is null)
                return;

            try
            {
                var popup = new NotePopup(_context, obj.Id, _navigation, isPreview: true);
                await Shell.Current.CurrentPage.ShowPopupAsync(popup);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть заметку");
            }
        }

        public ObservableCollection<Schedule> Schedules
        {
            get => _schedules;
            set
            {
                _schedules = value;
                OnPropertyChanged();
            }
        }

        public bool ScheduleFrameIsVisible
        {
            get => _scheduleFrameIsVisible;
            set
            {
                _scheduleFrameIsVisible = value;
                OnPropertyChanged();
            }
        }

        static async Task<List<ScheduleNote>> RefreshLocalScheduleAsync(
            List<Schedule> schedules,
            CancellationToken cancellationToken)
        {
            using var context = new DbConnect();
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.Schedules.ExecuteDeleteAsync(cancellationToken);
            await context.Schedules.AddRangeAsync(schedules, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await MappingScheduleAsync(context, schedules, cancellationToken);
            return ScheduleList;
        }

        public async Task<List<ScheduleNote>> LoadSchedule(
            int roomId = 0,
            string teacher = "",
            CancellationToken cancellationToken = default)
        {
            List<Schedule> schedules = new();
            if (CurrentUser is null && string.IsNullOrEmpty(teacher) && roomId == 0)
            {
                return ScheduleList;
            }

            if (!string.IsNullOrEmpty(teacher))
            {
                var searchTeacher = teacher.Split(' ');
                schedules = new(await _scheduleRequests.GetSchedulesAsync(teacher: searchTeacher, cancellationToken));
            }
            else if (roomId != 0)
            {
                schedules = new(await _scheduleRequests.GetSchedulesAsync(room_id: roomId, cancellationToken));
            }
            else if (CurrentUser!.Role == UserRole.Teacher)
            {
                schedules = [.. await _scheduleRequests.GetSchedulesAsync(teacher: CurrentUser.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries), cancellationToken)];
            }
            else
            {
                schedules = [.. await _scheduleRequests.GetSchedulesAsync(group_id: CurrentUser.GroupId, subgroup_id: CurrentUser.SubGroupId, cancellationToken)];
            }
            await MappingScheduleAsync(_context, schedules, cancellationToken);
            return ScheduleList;
        }

        public static async Task<List<ScheduleNote>> LoadSchedule(User currentUser,
            IScheduleRequests scheduleRequests,
            bool isRefresh = false,
            CancellationToken cancellationToken = default)
        {
            List<Schedule> schedules = [];
            if (!isRefresh)
            {
                using var context = new DbConnect();
                var localSchedules = await context.Schedules.AsNoTracking().ToListAsync(cancellationToken);
                await MappingScheduleAsync(context, localSchedules, cancellationToken);
                return ScheduleList;
            }
            if (currentUser.Role == UserRole.Teacher)
            {
                var searchTeacher = currentUser.FullName.Split(' ');
                schedules = [.. await scheduleRequests.GetSchedulesAsync(teacher: searchTeacher, cancellationToken)];
            }
            else
            {
                schedules = [.. await scheduleRequests.GetSchedulesAsync(group_id: currentUser.GroupId, subgroup_id: currentUser.SubGroupId, cancellationToken)];
            }
            return await RefreshLocalScheduleAsync(schedules, cancellationToken);
        }

        private static async Task MappingScheduleAsync(
            DbConnect context,
            List<Schedule> localSchedules,
            CancellationToken cancellationToken)
        {
            var scheduleIdsWithNotes = await context.Notes
                .AsNoTracking()
                .Select(note => note.Schedule_id)
                .ToHashSetAsync(cancellationToken);
            var mappedSchedules = new List<ScheduleNote>(localSchedules.Count);
            foreach (var schedule in localSchedules)
            {
                var scheduleNote = new ScheduleNote()
                {
                    Id = schedule.Id,
                    Name = schedule.Name,
                    Date = schedule.Date,
                    Teacher = schedule.Teacher,
                    PairType = schedule.PairType,
                    EndTime = schedule.EndTime,
                    ExternalId = schedule.ExternalId,
                    Room = schedule.Room,
                    StartTime = schedule.StartTime,
                };
                scheduleNote.HaveNote = scheduleIdsWithNotes.Contains(schedule.ExternalId);
                mappedSchedules.Add(scheduleNote);
            }
            ScheduleList = mappedSchedules;
        }

        public async Task<List<ScheduleNote>> LoadSchedule(
            string subgroupId,
            CancellationToken cancellationToken = default)
        {
            List<Schedule> schedules = [.. await _scheduleRequests.GetSchedulesAsync(group_id: CurrentUser.GroupId, subgroup_id: subgroupId, cancellationToken)];
            await MappingScheduleAsync(_context, schedules, cancellationToken);
            return ScheduleList;
        }

        public static async Task<List<string>> LoadTeacherList(
            IParserRequests utils,
            IMemoryCache cache,
            CancellationToken cancellationToken = default)
        {
            if (!cache.TryGetValue("teachers", out List<string>? teachers))
            {
                teachers = [.. await utils.GetTeachersAsync(true, cancellationToken)];
                cache.Set("teachers", teachers, TimeSpan.FromMinutes(10));
            }
            Teachers = teachers ?? [];
            return Teachers;
        }

        public bool IsScheduleBusy
        {
            get => _isScheduleBusy;
            set
            {
                _isScheduleBusy = value;
                OnPropertyChanged();
            }
        }

        public static async Task<List<Room>> LoadRoomList(
            IParserRequests utils,
            IMemoryCache cache,
            CancellationToken cancellationToken = default)
        {
            if (!cache.TryGetValue("rooms", out List<Room>? rooms))
            {
                rooms = [.. await utils.GetRoomsAsync(true, cancellationToken)];
                cache.Set("rooms", rooms, TimeSpan.FromMinutes(10));
            }
            Rooms = rooms ?? [];
            return Rooms;
        }

    }
}
