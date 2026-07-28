using mau.Database;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;
using mau.ViewModel;

namespace mau;

public partial class LoginPage : ContentPage
{
    private CancellationTokenSource? _focusScrollCancellation;

    public LoginPage(
        DbConnect context,
        IUserRequests userRequests,
        IAPIService service,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new LoginViewModel(context, service, userRequests, navigation);
    }

    private async void OnFieldFocused(object? sender, FocusEventArgs e)
    {
        if (sender is not VisualElement field)
            return;

        var cancellationSource = new CancellationTokenSource();
        var previous = Interlocked.Exchange(ref _focusScrollCancellation, cancellationSource);
        previous?.Cancel();

        try
        {
            await Task.Delay(300, cancellationSource.Token);
            if (Handler is null || LoginScroll.Handler is null || field.Handler is null)
                return;

            await LoginScroll.ScrollToAsync(field, ScrollToPosition.Center, animated: false);
        }
        catch (OperationCanceledException) when (cancellationSource.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
        }
        finally
        {
            Interlocked.CompareExchange(ref _focusScrollCancellation, null, cancellationSource);
            cancellationSource.Dispose();
        }
    }

    protected override void OnDisappearing()
    {
        Interlocked.Exchange(ref _focusScrollCancellation, null)?.Cancel();
        base.OnDisappearing();
    }
}
