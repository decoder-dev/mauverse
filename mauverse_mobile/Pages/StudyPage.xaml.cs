using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel.Debts;

namespace mau;

public partial class StudyPage : ContentPage
{
    public StudyPage(
        DbConnect context,
        IDebtRequests debtRequests,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new StudyViewModel(context, debtRequests, navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
