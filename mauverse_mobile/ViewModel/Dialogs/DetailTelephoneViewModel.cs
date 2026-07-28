using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.DTOModels;
using mau.Utils;
using mau.Utils.API.Interaface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
    }
}
