using mau.Database;
using mau.Utils.Services.Interface;
using mau.ViewModel.Services;

namespace mau;

public partial class CertificateRequestPage : ContentPage
{
    public CertificateRequestPage(
        DbConnect context,
        IStudentFormsService formsService,
        IAppNavigationService navigation)
    {
        InitializeComponent();
        BindingContext = new CertificateRequestViewModel(context, formsService, navigation);
        Shell.SetTabBarIsVisible(this, false);
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is CertificateRequestViewModel viewModel)
            viewModel.CancelPendingOperationsCommand.Execute(null);

        base.OnDisappearing();
    }
}
