using CommunityToolkit.Maui.Views;

using mau.ViewModel.Dialogs;

namespace mau.Dialogs;

public partial class ConfirmationPopup : Popup<bool>
{
    public ConfirmationPopup()
    {
        InitializeComponent();
        ResponsivePopupSize.Apply(this, 360, 260);
        BindingContext = new ConfirmationViewModel(this);
    }
}
