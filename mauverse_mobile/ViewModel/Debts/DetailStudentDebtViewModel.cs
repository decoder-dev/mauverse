using mau.Database;
using mau.Models;
using mau.Utils;
using mau.Utils.API.Interaface;
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
    public class DetailStudentDebtViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IDebtRequests _debtRequests;
        private ObservableCollection<StudySemester> _semesters = [];
        private string _titleStudentName = string.Empty;
        private string _studentFirstName = string.Empty;
        private string _studentName = string.Empty;
        private string _studentLastName = string.Empty;

        public DetailStudentDebtViewModel(DbConnect context, IDebtRequests debtRequests) : base(context)
        {
            _debtRequests = debtRequests;
            LoadAllData = new AsyncRelayCommand(LoadData);
        }

        public IAsyncRelayCommand LoadAllData { get; }

        public string TitleStudentName
        {
            get => _titleStudentName;
            set
            {
                _titleStudentName = value;
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

        async Task LoadData(CancellationToken cancellationToken)
        {
            var groupName = CurrentUser?.GroupName?.Trim();
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(_studentFirstName))
                return;
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                var semesters = await _debtRequests.GetSemesterByStudentGroup(
                    groupName,
                    _studentFirstName, _studentName, _studentLastName,
                    cancellationToken);
                await Task.WhenAll(semesters.Select(async semester =>
                    semester.Debts = await _debtRequests.GetDebtByStudentGroup(
                        semester.SemesterNumber,
                        groupName,
                        _studentFirstName, _studentName, _studentLastName,
                        cancellationToken)));
                Semesters = new ObservableCollection<StudySemester>(semesters);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить задолженности студента");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            _studentFirstName = GetString(query, "first_name");
            _studentName = GetString(query, "name");
            _studentLastName = GetString(query, "last_name");
            TitleStudentName = $"{_studentFirstName} {_studentName} {_studentLastName}";
        }

        private static string GetString(IDictionary<string, object> query, string key) =>
            query.TryGetValue(key, out var value) ? value?.ToString() ?? string.Empty : string.Empty;

        protected override void CancelPendingOperations() => LoadAllData.Cancel();
    }
}
