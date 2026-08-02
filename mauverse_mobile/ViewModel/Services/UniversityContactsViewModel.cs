using CommunityToolkit.Mvvm.Input;

using mau.Models;
using mau.Utils;
using mau.Utils.Services;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.Services;

public partial class UniversityContactsViewModel
{
    readonly IAppNavigationService _navigation;

    public UniversityContactsViewModel(IAppNavigationService navigation)
    {
        _navigation = navigation;
    }

    public string PageDescription { get; } =
        "Телефоны приёмной комиссии и платёжные реквизиты МАУ";

    public IReadOnlyList<UniversityContactBlock> AdmissionContacts { get; } =
        UniversityContactsCatalog.AdmissionContacts;

    public IReadOnlyList<UniversityContactBlock> UniversityRequisites { get; } =
        UniversityContactsCatalog.UniversityRequisites;

    [RelayCommand]
    async Task CallAsync(UniversityContactBlock? block)
    {
        if (block is null || string.IsNullOrWhiteSpace(block.Phone))
            return;

        try
        {
            var digits = new string(block.Phone.Where(c => char.IsDigit(c) || c == '+').ToArray());
            if (digits.StartsWith('8') && digits.Length >= 11)
                digits = "+7" + digits[1..];

            PhoneDialer.Default.Open(digits);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть телефон");
        }
    }

    [RelayCommand]
    async Task CopyAsync(UniversityContactBlock? block)
    {
        if (block is null)
            return;

        try
        {
            var text = string.Join(
                Environment.NewLine,
                new[]
                {
                    block.Title,
                    block.Details,
                    block.Address,
                    block.Phone,
                    block.Email
                }.Where(static part => !string.IsNullOrWhiteSpace(part)));

            await Clipboard.Default.SetTextAsync(text);
            await AppShell.DisplaySnackbarAsync("Скопировано");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось скопировать");
        }
    }

    [RelayCommand]
    async Task OpenRequisitesPageAsync()
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            await AppShell.DisplaySnackbarAsync("Для открытия страницы требуется интернет");
            return;
        }

        try
        {
            if (!ExternalUri.TryCreateHttp(UniversityPortalUrls.Requisites, out var uri))
            {
                await AppShell.DisplaySnackbarAsync("Ссылка недоступна");
                return;
            }

            await _navigation.OpenBrowserAsync(
                BrowserDestinationRegistry.CreateUniversityPage("Реквизиты МАУ", uri));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await AppShell.DisplaySnackbarAsync("Не удалось открыть реквизиты");
        }
    }
}
