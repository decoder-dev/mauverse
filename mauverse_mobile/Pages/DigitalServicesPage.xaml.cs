using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class DigitalServicesPage : ContentPage
{
    public DigitalServicesPage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = UniversityGuideViewModel.CreateDigital(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
