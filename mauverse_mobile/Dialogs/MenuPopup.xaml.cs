using CommunityToolkit.Maui.Views;

using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel.Dialogs;

namespace mau.Dialogs;

public partial class MenuPopup : Popup
{
    public MenuPopup(DbConnect context, int scheduleId, IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new MenuViewModel(context, scheduleId, this, navigation);
    }
}
