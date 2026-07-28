using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.ViewModel.Dialogs
{
    public partial class ExceptionViewModel : BaseDialogViewModel
    {
        [ObservableProperty]
        string _message;

        [ObservableProperty]
        string _description;

        private readonly Popup _popup;
        public ExceptionViewModel(string message, string description, Popup popup)
        {
            _message = message;
            _description = description;
            _popup = popup;
        }

        [RelayCommand]
        private async Task Ok()
        {
            await CloseAsync(_popup);
        }
    }
}
