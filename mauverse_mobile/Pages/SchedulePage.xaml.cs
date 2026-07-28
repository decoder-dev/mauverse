using mau.Database;
using mau.Utils.API.Interaface;
using Microsoft.Extensions.Caching.Memory;
using mau.ViewModel.Schedules;
using mau.Utils.Services.Interface;

namespace mau;

public partial class SchedulePage : ContentPage
{
    public SchedulePage(
        DbConnect context,
        IScheduleRequests scheduleRequests,
        IUserRequests userRequests,
        IParserRequests parserRequests,
        IMemoryCache memoryCache,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new ScheduleViewModel(
            context,
            scheduleRequests,
            userRequests,
            parserRequests,
            memoryCache,
            navigation);
    }
}
