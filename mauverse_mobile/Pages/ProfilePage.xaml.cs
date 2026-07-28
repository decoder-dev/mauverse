using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;

namespace mau;

public partial class ProfilePage : ContentPage
{
    public ProfilePage(
        DbConnect context,
        IUserRequests userRequests,
        IValidationRequests validationRequests,
        IAPIService service,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new ProfileViewModel(context, userRequests, service, validationRequests, navigation);
    }
}
