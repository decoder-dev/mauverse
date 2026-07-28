using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel;

namespace mau;

public partial class DetailChatPage : ContentPage
{
    public DetailChatPage(DbConnect context, IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new DetailChatViewModel(context, navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
