using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Views;

using mau.DTOModels;
using mau.Utils;
using mau.Models;
using mau.Utils.Services.Interface;

namespace mau.ViewModel.Dialogs
{
    public class DetailsNewsViewModel : BaseDialogViewModel
    {
        private string _description = string.Empty;
        private string _url = string.Empty;
        private string _title = string.Empty;
        private string _imageUrl = string.Empty;
        private string _publish = string.Empty;
        private readonly Popup _popup;
        private readonly IAppNavigationService _navigation;

        public DetailsNewsViewModel(
            RssDTO news,
            Popup popup,
            IAppNavigationService navigation)
        {
            _popup = popup;
            _navigation = navigation;
            Title = news.Title;
            ImageUrl = news.Image;
            Description = news.Description;
            Url = news.Link;
            Publish = news.Publish;
            GoToFull = new AsyncRelayCommand<string?>(OpenArticleAsync);
        }

        public IAsyncRelayCommand<string?> GoToFull { get; }

        private async Task OpenArticleAsync(string? url)
        {
            if (!ExternalUri.TryCreateHttp(url, out var articleUri))
            {
                await AppShell.DisplaySnackbarAsync("Ссылка на новость недоступна");
                return;
            }

            try
            {
                await _popup.CloseAsync();
                await _navigation.OpenBrowserAsync(
                    BrowserDestinationRegistry.CreateUniversityNews(Title, articleUri));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync("Не удалось открыть новость");
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged();
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                _imageUrl = value;
                OnPropertyChanged();
            }
        }
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }
        public string Publish
        {
            get => _publish;
            set
            {
                _publish = value;
                OnPropertyChanged();
            }
        }
    }
}
