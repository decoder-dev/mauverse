using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace mau.ViewModel.Dialogs;

public partial class ConfirmationViewModel(Popup<bool> popup) : ObservableObject
{
    [RelayCommand]
    private Task CancelAsync() => popup.CloseAsync(false);

    [RelayCommand]
    private Task ConfirmAsync() => popup.CloseAsync(true);
}
