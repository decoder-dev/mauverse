using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;

namespace mau.ViewModel.Dialogs
{
    public partial class DetailTelephoneViewModel : BaseViewModel
    {
        private readonly IParserRequests _parserRequests;
        [ObservableProperty]
        List<TelephoneInfoDTO> _telephoneInfos = [];

        [ObservableProperty]
        string _deptName;

        public DetailTelephoneViewModel(DbConnect context, IParserRequests parserRequests, DeptInfoDTO dept) : base(context)
        {
            _parserRequests = parserRequests;
            DeptName = dept.Name;
            _ = GetPhone(dept);
        }

        async Task GetPhone(DeptInfoDTO dept)
        {
            IsBusy = true;
            try
            {
                CurrentState = States.Loading;
                TelephoneInfos?.Clear();
                var telephones = await _parserRequests.GetTelephonesAsync(dept);
                TelephoneInfos = [.. telephones];
                CurrentState = TelephoneInfos.Count != 0 ? States.Success : States.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                CurrentState = States.Empty;
                await AppShell.DisplaySnackbarAsync("Не удалось загрузить телефоны подразделения");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1822:Mark members as static",
            Justification = "RelayCommand generation requires the instance command contract used by XAML.")]
        async Task CallAsync(string? rawPhone)
        {
            if (string.IsNullOrWhiteSpace(rawPhone))
                return;

            try
            {
                var dial = PhoneNumberFormatting.ToDialString(rawPhone);
                if (string.IsNullOrWhiteSpace(dial))
                {
                    await AppShell.DisplaySnackbarAsync("Некорректный номер телефона");
                    return;
                }

                PhoneDialer.Default.Open(dial);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть телефон");
            }
        }
    }
}
