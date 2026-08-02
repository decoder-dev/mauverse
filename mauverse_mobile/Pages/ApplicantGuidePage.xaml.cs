using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class ApplicantGuidePage : ContentPage
{
    public ApplicantGuidePage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = UniversityGuideViewModel.CreateApplicant(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
