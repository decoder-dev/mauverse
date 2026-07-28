using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;

namespace mau;

public partial class MainPage : ContentPage
{
    public MainPage(
        DbConnect context,
        IScheduleRequests scheduleRequests,
        IUserRequests userRequests,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new MainViewModel(context, scheduleRequests, userRequests, navigation);
    }
}

