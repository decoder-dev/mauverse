using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class StudentGuidePage : ContentPage
{
    public StudentGuidePage(IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = UniversityGuideViewModel.CreateStudent(navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
