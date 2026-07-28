using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs.Note;
using mau.Utils.Services.Interface;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mau.ViewModel.Dialogs
{
    public partial class MenuViewModel : BaseDialogViewModel
    {
        private readonly int _scheduleId;
        private readonly Popup _popup;
        private readonly DbConnect _context;
        private readonly IAppNavigationService _navigation;

        [ObservableProperty]
        bool _isNoteEmpty;

        [ObservableProperty]
        bool _isNoteCreated;

        public MenuViewModel(
            DbConnect context,
            int scheduleId,
            Popup popup,
            IAppNavigationService navigation)
        {
            _scheduleId = scheduleId;
            _popup = popup;
            _context = context;
            _navigation = navigation;
            if (_context.Notes.Any(p => p.Schedule_id == _scheduleId))
            {
                IsNoteCreated = true;
                IsNoteEmpty = false;
            }
            else
            {
                IsNoteCreated = false;
                IsNoteEmpty = true;
            }
        }

        [RelayCommand]
        private async Task CreateNote()
        {
            await _popup.CloseAsync();
            var notePopup = new NotePopup(_context, _scheduleId, _navigation, isCreate: true);
            await Shell.Current.CurrentPage.ShowPopupAsync(notePopup);
        }

        [RelayCommand]
        private async Task EditNote()
        {
            await _popup.CloseAsync();
            var notePopup = new NotePopup(_context, _scheduleId, _navigation, isEdit: true);
            await Shell.Current.CurrentPage.ShowPopupAsync(notePopup);
        }

        [RelayCommand]
        private async Task DeleteNote()
        {
            await _popup.CloseAsync();
            var notePopup = new NotePopup(_context, _scheduleId, _navigation, isDelete: true);
            await Shell.Current.CurrentPage.ShowPopupAsync(notePopup);
        }
    }
}
