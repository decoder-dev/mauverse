using CommunityToolkit.Maui.Views;

using mau.Database;
using mau.DTOModels;
using mau.Utils.API.Interaface;
using mau.ViewModel.Dialogs;

namespace mau.Dialogs;

public partial class DetailTelephonePopup : Popup
{
    public DetailTelephonePopup(DbConnect context, IParserRequests parserRequests, DeptInfoDTO dept)
    {
        InitializeComponent();
        ResponsivePopupSize.Apply(this, 360, 620);
        BindingContext = new DetailTelephoneViewModel(context, parserRequests, dept);
    }
}
