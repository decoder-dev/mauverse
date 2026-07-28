using mau.ViewModel;

namespace mau;

public partial class LoadingPage : ContentPage
{
    private readonly LoadingViewModel _viewModel;

    public LoadingPage(LoadingViewModel model)
    {
        InitializeComponent();
        _viewModel = model;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = RunStartupAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.PerformNavigationCommand.Cancel();
        base.OnDisappearing();
    }

    private async Task RunStartupAsync()
    {
        try
        {
            await _viewModel.PerformNavigationCommand.ExecuteAsync(parameter: null);
        }
        catch (Exception exception)
        {
            // Keep lifecycle event failures observed even if a future command handler changes.
            System.Diagnostics.Debug.WriteLine(exception);
        }
    }
}
