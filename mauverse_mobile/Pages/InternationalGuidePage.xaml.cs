using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class InternationalGuidePage : ContentPage
{
    public InternationalGuidePage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = UniversityGuideViewModel.CreateInternational(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
