using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using mau.Database;
using mau.Dialogs;
using mau.DTOModels;
using mau.Models;
using mau.Utils;
using mau.Utils.Services.Interface;
using mau.Utils.Services;

using Microsoft.EntityFrameworkCore;

namespace mau.ViewModel
{
    public partial class BaseViewModel : ObservableObject
    {
        private readonly DbConnect _context;
        public static User CurrentUser { get; private set; } = null!;

        public BaseViewModel(DbConnect context)
        {
            _context = context;
            CancelPendingOperationsCommand = new RelayCommand(CancelPendingOperations);
        }

        public IRelayCommand CancelPendingOperationsCommand { get; }

        protected virtual void CancelPendingOperations()
        {
        }

        [ObservableProperty]
        bool _isBusy;

        [ObservableProperty]
        bool _canStateChange;

        [ObservableProperty]
        string _currentState = States.Loading;

        public static async Task<bool> SetCurrentUserAsync(
            DbConnect context,
            CancellationToken cancellationToken = default)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
            if (user is null)
            {
                CurrentUser = null!;
                return false;
            }

            await UserCredentialStore.RestoreAsync(user, cancellationToken);
            CurrentUser = user;
            return true;
        }

        public static bool CheckConnection()
        {
            var accessType = Connectivity.Current.NetworkAccess;
            return accessType == NetworkAccess.Internet;
        }

        public static async Task DeleteDataAndExitAsync(
            DbConnect context,
            IAPIService apiService,
            CancellationToken cancellationToken = default)
        {
            context.ChangeTracker.Clear();
            try
            {
                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                await context.Notes.ExecuteDeleteAsync(cancellationToken);
                await context.Schedules.ExecuteDeleteAsync(cancellationToken);
                await context.Users.ExecuteDeleteAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                apiService.RemoveHttpHeaders();
                UserCredentialStore.Clear();
                CurrentUser = null!;
            }
        }

        public async Task GetInternetConnectionInfoAsync(CancellationToken cancellationToken = default)
        {
            if (!CheckConnection())
            {
                if (!await _context.Users.AsNoTracking().AnyAsync(cancellationToken))
                {
                    var dialog = new ExceptionPopup("Нет интернет-соединения, попробуйте еще раз", "Проверьте соединение.");
                    await Shell.Current.CurrentPage.ShowPopupAsync(
                        dialog,
                        PopupOptions.Empty,
                        cancellationToken);
                }
                else
                {
                    await AppShell.DisplaySnackbarAsync("Офлайн-режим: используются сохраненные данные");
                }
            }
        }
    }
}
