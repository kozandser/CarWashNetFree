using KLib.Native;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Applications.Dialogs
{
    public static class DialogService
    {
        private static IDialogHost _dialogHost;
        public static IDialogCoordinator DialogCoordinator;
        public static ProgressDialogController ProgressDialogController;

        public static void Init(IDialogHost dialogHost, IDialogCoordinator dialogCoordinator)
        {
            _dialogHost = dialogHost;
            DialogCoordinator = dialogCoordinator;
        }
        public static async Task OnCriticalErrorAsync(string message)
        {
            await _dialogHost.OnCriticalErrorAsync(message);
        }
        public static void LogoutUser()
        {
            _dialogHost.Logout();
        }
        public static async Task OnErrorAsync(string message)
        {
            await DialogCoordinator.ShowMessageAsync(_dialogHost.DataContext, "Ошибка", message);
        }
        public static void OnError(string message)
        {
            AsyncHelper.FireAndForget(() => OnErrorAsync(message));
        }
        public static async Task ShowMessageAsync(string title, string message)
        {
            await DialogCoordinator.ShowMessageAsync(_dialogHost.DataContext, title, message);
        }
        public static async Task<bool> ShowConfirmationAsync(string title, string message, string yes = "Да", string no = "Нет")
        {
            var result = await DialogCoordinator.ShowMessageAsync(
                _dialogHost.DataContext,
                title,
                message,
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = yes,
                    NegativeButtonText = no
                });
            if (result == MessageDialogResult.Affirmative) return true;
            else return false;
        }
        public static async Task<ProgressDialogController> ShowProgressAsync(string title, string message)
        {
            return await DialogCoordinator.ShowProgressAsync(_dialogHost.DataContext, title, message);
        }
        public static async Task ShowDefaultProgressAsync(string title, string message)
        {
            ProgressDialogController = await DialogCoordinator.ShowProgressAsync(_dialogHost.DataContext, title, message);
            ProgressDialogController.SetIndeterminate();
        }
        public static async Task HideDefaultProgressAsync()
        {
            if (ProgressDialogController != null) await ProgressDialogController.CloseAsync().ConfigureAwait(false);
        }
        public static void SetDefaultProgressMessage(string message)
        {
            if (ProgressDialogController != null) ProgressDialogController.SetMessage(message);
        }
    }
}
