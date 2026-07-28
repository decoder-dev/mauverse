using mau.Database;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace mau.ViewModel.Debts
{
    public class StudyViewModel : BaseViewModel
    {
        private readonly DbConnect _context;
        private readonly IDebtRequests _debtRequests;
        private readonly IAppNavigationService _navigation;
        private StudySemester? _selectedSemester;
        private StudentDebt? _selectedStudent;
        private ObservableCollection<StudySemester> _semesters = [];
        private ObservableCollection<StudentDebt> _students = [];
        private bool _requiredFillFrameIsVisible;
        private bool _studentDebtIsVisible;
        private bool _curatorDebtIsVisible;
        private bool _profileActionIsVisible;
        private string _frameLabel = string.Empty;

        public StudyViewModel(
            DbConnect context,
            IDebtRequests debtRequests,
            IAppNavigationService navigation) : base(context)
        {
            _context = context;
            _debtRequests = debtRequests;
            _navigation = navigation;
            LoadAllData = new AsyncRelayCommand(LoadData);
            StudentSelected = new AsyncRelayCommand(GoToDebtsByStudentAsync);
            GoToProfile = new AsyncRelayCommand(NavigateToProfileAsync);
        }

        public bool RequiredFillFrameIsVisible
        {
            get => _requiredFillFrameIsVisible;
            set
            {
                _requiredFillFrameIsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool StudentDebtIsVisible
        {
            get => _studentDebtIsVisible;
            set
            {
                _studentDebtIsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool CuratorDebtIsVisible
        {
            get => _curatorDebtIsVisible;
            set
            {
                _curatorDebtIsVisible = value;
                OnPropertyChanged();
            }
        }

        public bool ProfileActionIsVisible
        {
            get => _profileActionIsVisible;
            set
            {
                _profileActionIsVisible = value;
                OnPropertyChanged();
            }
        }

        public string FrameLabel
        {
            get => _frameLabel;
            set
            {
                _frameLabel = value;
                OnPropertyChanged();
            }
        }

        public IAsyncRelayCommand LoadAllData { get; }

        public IAsyncRelayCommand StudentSelected { get; }

        public IAsyncRelayCommand GoToProfile { get; }

        public StudySemester? SelectedSemester
        {
            get => _selectedSemester;
            set
            {
                _selectedSemester = value;
                OnPropertyChanged();
            }
        }

        public StudentDebt? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StudySemester> Semesters
        {
            get => _semesters;
            set
            {
                _semesters = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StudentDebt> Students
        {
            get => _students;
            set
            {
                _students = value;
                OnPropertyChanged();
            }
        }
        async Task GoToDebtsByStudentAsync()
        {
            var student = SelectedStudent;
            if (student is null)
                return;

            var navigationParameters = new Dictionary<string, object>
            {
                { "first_name", student.FirstName},
                { "name", student.Name},
                { "last_name", student.LastName },
            };
            try
            {
                await _navigation.NavigateAsync("services/study_info/details", parameters: navigationParameters);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть задолженности студента");
            }
        }

        private async Task NavigateToProfileAsync()
        {
            try
            {
                await _navigation.NavigateAsync($"///main/{nameof(ProfilePage)}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть профиль");
            }
        }
        async Task LoadData(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            Students.Clear();
            Semesters.Clear();
            IsBusy = true;
            try
            {
                StudentDebtIsVisible = false;
                CuratorDebtIsVisible = false;
                RequiredFillFrameIsVisible = false;
                ProfileActionIsVisible = false;
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                if (user is null)
                    return;

                if (user.Role == UserRole.Teacher)
                {
                    if (string.IsNullOrWhiteSpace(user.GroupName))
                    {
                        FrameLabel = "Для просмотра задолженностей студентов необходимо указать курируемую вами группу";
                        RequiredFillFrameIsVisible = true;
                        ProfileActionIsVisible = true;
                        return;
                    }
                    CuratorDebtIsVisible = true;
                    var students = await _debtRequests.GetGroupDebts(user.GroupName.Trim(), cancellationToken);
                    Students = new(students);
                    Semesters.Clear();
                    if (Students.Count == 0)
                    {
                        FrameLabel = "У студентов группы задолженностей нет";
                        RequiredFillFrameIsVisible = true;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(user.CreditBook))
                    {
                        FrameLabel = "Для просмотра задолженностей необходимо заполнить поле «Зачетная книжка»";
                        RequiredFillFrameIsVisible = true;
                        ProfileActionIsVisible = true;
                        return;
                    }
                    StudentDebtIsVisible = true;
                    var creditBook = user.CreditBook.Trim();
                    var semesters = await _debtRequests.GetSemester(creditBook, cancellationToken);
                    await Task.WhenAll(semesters.Select(async semester =>
                        semester.Debts = await _debtRequests.GetDebtsBySemester(
                            creditBook,
                            semester.SemesterNumber,
                            cancellationToken)));
                    Semesters = new(semesters);
                    Students.Clear();
                    if (Semesters.Count == 0)
                    {
                        FrameLabel = "Учебных задолженностей нет";
                        RequiredFillFrameIsVisible = true;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить задолженности");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected override void CancelPendingOperations()
        {
            LoadAllData.Cancel();
            StudentSelected.Cancel();
        }
    }
}
