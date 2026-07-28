using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.ViewModel.Services;

namespace mau;

public partial class InfoPage : ContentPage
{
    public InfoPage(DbConnect context, IValidationRequests validationRequests, IParserRequests parserRequests)
    {
        InitializeComponent();
        BindingContext = new InfoViewModel(context, validationRequests, parserRequests);
        Shell.SetTabBarIsVisible(this, false);
    }
}
