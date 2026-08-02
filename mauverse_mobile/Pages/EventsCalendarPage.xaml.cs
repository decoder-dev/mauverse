using mau.Database;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class EventsCalendarPage : ContentPage
{
    public EventsCalendarPage(
        DbConnect context,
        IParserRequests parserRequests,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new EventsCalendarViewModel(context, parserRequests, navigation);
        Shell.SetTabBarIsVisible(this, false);
    }
}
