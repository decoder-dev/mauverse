using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class ScienceGuidePage : ContentPage
{
    public ScienceGuidePage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = UniversityGuideViewModel.CreateScience(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
