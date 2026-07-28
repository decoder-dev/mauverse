using mau.ViewModel.Services;
using mau.Utils.Services.Interface;

namespace mau;

public partial class CampusNavigatorPage : ContentPage
{
    public CampusNavigatorPage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new CampusNavigatorViewModel(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
