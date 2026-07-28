using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.ViewModel.Services;

namespace mau;

public partial class TelephonePage : ContentPage
{
    public TelephonePage(DbConnect context, IParserRequests parserRequests)
    {
        InitializeComponent();
        this.BindingContext = new TelephoneViewModel(context, parserRequests);
        Shell.SetTabBarIsVisible(this, false);
    }
}
