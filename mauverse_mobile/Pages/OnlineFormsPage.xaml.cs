using mau.ViewModel.Services;
using mau.Utils.Services.Interface;

namespace mau;

public partial class OnlineFormsPage : ContentPage
{
    public OnlineFormsPage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new OnlineFormsViewModel(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
