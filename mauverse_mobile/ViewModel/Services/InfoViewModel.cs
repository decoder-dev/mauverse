using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.ViewModel.Schedules;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.ViewModel.Services
{
    public partial class InfoViewModel : BaseViewModel
    {
        private readonly IValidationRequests _validationRequests;
        private readonly IParserRequests _parserRequests;
        private CancellationTokenSource? _throttleCts;

        [ObservableProperty]
        string _contactLabel = string.Empty;

        [ObservableProperty]
        string _teacherName = string.Empty;

        [ObservableProperty]
        string _selectedTeacher = string.Empty;

        [ObservableProperty]
        bool _isListVisible;

        [ObservableProperty]
        List<UniversityInfo> _teacherInfo = [];

        [ObservableProperty]
        List<string> _teacherList = [];

        [ObservableProperty]
        bool _panelVisible;

        bool _teacherSelected;
        public InfoViewModel(DbConnect context, IValidationRequests validationRequests, IParserRequests parserRequests) : base(context)
        {
            _validationRequests = validationRequests;
            _parserRequests = parserRequests;
            ContactLabel = "Воспользуйтесь поиском, чтобы посмотреть контакты связи с преподавателем";
            PanelVisible = false;
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        async Task TextChanged()
        {
            if (_teacherSelected)
                return;
            var current = new CancellationTokenSource();
            var previous = Interlocked.Exchange(ref _throttleCts, current);
            previous?.Cancel();
            try
            {
                await Task.Delay(150, current.Token);
                if (string.IsNullOrWhiteSpace(TeacherName))
                {
                    IsListVisible = false;
                    TeacherList.Clear();
                    return;
                }
                var teachers = BaseScheduleViewModel.Teachers.Count > 0
                    ? BaseScheduleViewModel.Teachers
                    : [.. await _parserRequests.GetTeachersAsync(true, current.Token)];
                current.Token.ThrowIfCancellationRequested();
                IsListVisible = true;
                TeacherList = [.. teachers.Where(p => p.Contains(TeacherName, StringComparison.OrdinalIgnoreCase)).Take(5)];
                if (TeacherList.Count == 0)
                    TeacherList.Add("Ничего не найдено");
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                TeacherList = [];
                IsListVisible = false;
            }
            finally
            {
                Interlocked.CompareExchange(ref _throttleCts, null, current);
                current.Dispose();
            }
        }

        [RelayCommand]
        async Task GetTeacherInfo(CancellationToken cancellationToken)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(TeacherName))
                return;

            IsBusy = true;
            try
            {
                TeacherInfo.Clear();
                var normalizedName = TeacherName.Trim();
                var checkTeacher = await _validationRequests.CheckTeacherAsync(normalizedName, cancellationToken);
                if (!checkTeacher)
                {
                    await AppShell.DisplaySnackbarAsync("Данного преподавателя нет в списке");
                    return;
                }

                var teacherInfo = await _parserRequests.GetTeacherInfoAsync(normalizedName, cancellationToken);
                TeacherInfo = [teacherInfo];
                ContactLabel = "Контактные данные преподавателя";
                PanelVisible = false;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Проверьте правильность ввода ФИО преподавателя");
                PanelVisible = true;
                return;
            }
            finally
            {
                _teacherSelected = false;
                IsBusy = false;
            }
        }

        [RelayCommand]
        void TeacherSelected(string teacher)
        {
            if (string.IsNullOrWhiteSpace(teacher) || teacher == "Ничего не найдено")
            {
                return;
            }

            _teacherSelected = true;
            TeacherName = teacher;
            IsListVisible = false;
            TeacherList.Clear();
        }

        protected override void CancelPendingOperations()
        {
            GetTeacherInfoCommand.Cancel();
            Interlocked.Exchange(ref _throttleCts, null)?.Cancel();
        }
    }
}
