using mau.Database;
using mau.Utils;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel.News;

namespace mau;

public partial class NewsPage : ContentPage
{
    public NewsPage(
        DbConnect context,
        IParserRequests parserRequests,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new NewsViewModel(context, parserRequests, navigation);
    }
}
