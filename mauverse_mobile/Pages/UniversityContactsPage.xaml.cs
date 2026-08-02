using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class UniversityContactsPage : ContentPage
{
    public UniversityContactsPage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new UniversityContactsViewModel(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
