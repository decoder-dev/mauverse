using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class ServiceListPage : ContentPage
{
    public ServiceListPage(DbConnect context, IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new ServicesViewModel(context, navigation);
    }
}
