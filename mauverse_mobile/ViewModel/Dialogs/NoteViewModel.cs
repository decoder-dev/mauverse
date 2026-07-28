using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Models;
using mau.Utils.Services.Interface;
using mau.ViewModel.Schedules;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace mau.ViewModel.Dialogs
{
    public partial class NoteViewModel : BaseDialogViewModel
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private Note? _note;

        [ObservableProperty]
        private bool _isCreate;

        [ObservableProperty]
        private bool _isEdit;

        [ObservableProperty]
        private bool _isDelete;

        [ObservableProperty]
        private bool _isEditOrCreate;

        [ObservableProperty]
        private bool _isEditable;

        [ObservableProperty]
        private string _popupLabel = string.Empty;

        private int _scheduleId;
        private readonly DbConnect _context;
        private readonly Popup _popup;
        private readonly IAppNavigationService _navigation;
        private bool _isBusy;

        public NoteViewModel(
            Popup popup,
            DbConnect context,
            IAppNavigationService navigation,
            int scheduleId,
            bool isPreview = false, bool isCreate = false, bool isEdit = false, bool isDelete = false)
        {
            _scheduleId = scheduleId;
            _context = context;
            _popup = popup;
            _navigation = navigation;
            IsCreate = isCreate;
            IsEdit = isEdit;
            IsDelete = isDelete;
            IsEditOrCreate = isCreate || isEdit;
            IsEditable = !isPreview;
            try
            {
                LoadNote();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                PopupLabel = "Не удалось загрузить заметку";
                IsEditOrCreate = false;
            }
        }

        [RelayCommand]
        private Task CreateNote(CancellationToken cancellationToken) => RunSafely(async () =>
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                await AppShell.DisplaySnackbarAsync("Укажите название заметки");
                return;
            }

            var newNote = new Note()
            {
                Title = Title.Trim(),
                Schedule_id = _scheduleId,
                Description = Description,
            };
            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync(cancellationToken);
            await _popup.CloseAsync(cancellationToken);
            await _navigation.NavigateAsync($"///LoadingPage?redirect={nameof(SchedulePage)}");
        }, "Не удалось создать заметку");

        [RelayCommand]
        private Task EditNote(CancellationToken cancellationToken) => RunSafely(async () =>
        {
            if (Note is null)
                return;
            if (string.IsNullOrWhiteSpace(Title))
            {
                await AppShell.DisplaySnackbarAsync("Укажите название заметки");
                return;
            }

            Note.Title = Title.Trim();
            Note.Description = Description;
            _context.Notes.Update(Note);
            await _context.SaveChangesAsync(cancellationToken);
            await _popup.CloseAsync(cancellationToken);
        }, "Не удалось сохранить заметку");

        [RelayCommand]
        private Task DeleteNote(CancellationToken cancellationToken) => RunSafely(async () =>
        {
            if (Note is null)
                return;

            _context.Notes.Remove(Note);
            await _context.SaveChangesAsync(cancellationToken);
            await _popup.CloseAsync(cancellationToken);
            await _navigation.NavigateAsync($"///LoadingPage?redirect={nameof(SchedulePage)}");
        }, "Не удалось удалить заметку");

        private async Task RunSafely(Func<Task> action, string errorMessage)
        {
            if (_isBusy)
                return;

            _isBusy = true;
            try
            {
                await action();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                await AppShell.DisplaySnackbarAsync(errorMessage);
            }
            finally
            {
                _isBusy = false;
            }
        }

        private void LoadNote()
        {
            Note = _context.Notes.FirstOrDefault(p => p.Schedule_id == _scheduleId);

            if (Note is not null)
            {
                Title = Note.Title;
                Description = Note.Description;
                _scheduleId = Note.Schedule_id;
                if (IsDelete)
                {
                    this.PopupLabel = $"Вы уверены, что хотите удалить {Note.Title}?";
                    IsDelete = true;
                }
                else if (IsEdit)
                {
                    this.PopupLabel = $"Редактирование {Note.Title}";
                    IsEdit = true;
                }
                else if (!IsEditable)
                {
                    this.PopupLabel = $"Просмотр заметки {Note.Title}";
                    IsEditOrCreate = true;
                }
            }
            else
            {
                IsCreate = IsEditable;
                IsEditOrCreate = IsEditable;
                PopupLabel = IsEditable ? "Новая заметка" : "Заметка не найдена";
            }
        }
    }
}
