using CommunityToolkit.Maui.Views;

using mau.ViewModel.Dialogs;

namespace mau.Dialogs;

public partial class ExceptionPopup : Popup
{
    public ExceptionPopup(string message, string description = "")
    {
        InitializeComponent();
        BindingContext = new ExceptionViewModel(message, description, this);
    }
}
