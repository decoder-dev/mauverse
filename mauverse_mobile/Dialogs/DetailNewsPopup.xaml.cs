using CommunityToolkit.Maui.Views;

using mau.DTOModels;
using mau.Utils.Services.Interface;
using mau.ViewModel.Dialogs;

namespace mau.Dialogs;

public partial class DetailNewsPopup : Popup
{
    public DetailNewsPopup(RssDTO news, IAppNavigationService navigation)
    {
        InitializeComponent();
        ResponsivePopupSize.Apply(this, 360, 620);
        BindingContext = new DetailsNewsViewModel(news, this, navigation);
    }
}
