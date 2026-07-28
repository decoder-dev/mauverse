using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;

using mau.Database;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;

namespace mau;

public partial class HomeChatPage : ContentPage
{
    public HomeChatPage(
        DbConnect context,
        IParserRequests parserRequests,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new MainChatViewModel(context, parserRequests, navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
