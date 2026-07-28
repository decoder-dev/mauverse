using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.ViewModel.Debts;

namespace mau;

public partial class DetailStudentDebtPage : ContentPage
{
    public DetailStudentDebtPage(DbConnect context, IDebtRequests debtRequests)
    {
        InitializeComponent();
        this.BindingContext = new DetailStudentDebtViewModel(context, debtRequests);
        Shell.SetTabBarIsVisible(this, false);
    }
}
