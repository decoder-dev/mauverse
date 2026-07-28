using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Utils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.ViewModel.Dialogs
{
    public partial class BaseDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        bool _canStateChange;

        [ObservableProperty]
        string _currentState = States.Loading;

        public BaseDialogViewModel() { }

        protected static async Task CloseAsync(Popup popup)
        {
            await popup.CloseAsync();
        }
    }
}
