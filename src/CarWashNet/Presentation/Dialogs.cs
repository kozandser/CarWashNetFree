using CarWashNet.Applications;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Presentation
{
    public interface IDialogHost
    {
        object DataContext { get; set; }
        Task RaiseCriticalErrorAsync(string message);
        void Logout();
    }
    public class DialogManager
    {
        private static IDialogHost _dialogHost;
        private static IDialogCoordinator _dialogCoordinator;
        private static ProgressDialogController _defaultProgressDialogController;

        public DialogManager(IDialogHost dialogHost, IDialogCoordinator dialogCoordinator)
        {
            _dialogHost = dialogHost;
            _dialogCoordinator = dialogCoordinator;
        }
        public async Task<MessageDialogResult> ShowMessageAsync(string title, string message)
        {
            return await _dialogCoordinator.ShowMessageAsync(_dialogHost.DataContext, title, message);
        }
        public async Task<MessageDialogResult> ShowErrorAsync(string message)
        {
            return await _dialogCoordinator.ShowMessageAsync(_dialogHost.DataContext, "Ошибка", message);
        }
        public async Task RaiseCriticalErrorAsync(string message)
        {
            await _dialogHost.RaiseCriticalErrorAsync(message);
        }



        public async Task<bool> ShowConfirmationAsync(string title, string message, string yes = "Да", string no = "Нет")
        {
            var result = await _dialogCoordinator.ShowMessageAsync(
                _dialogHost.DataContext,
                title,
                message,
                MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings
                {
                    AffirmativeButtonText = yes,
                    NegativeButtonText = no
                });
            if (result == MessageDialogResult.Affirmative) return true;
            else return false;
        }
        public async Task ShowDefaultProgressAsync(string title, string message)
        {
            if (_defaultProgressDialogController == null)
            {
                _defaultProgressDialogController = await _dialogCoordinator.ShowProgressAsync(_dialogHost.DataContext, title, message);
                _defaultProgressDialogController.SetIndeterminate();
            }
            else
            {
                _defaultProgressDialogController.SetTitle(title);
                _defaultProgressDialogController.SetMessage(message);
            }
        }
        public async Task HideDefaultProgressAsync()
        {
            if (_defaultProgressDialogController != null)
            {
                await _defaultProgressDialogController.CloseAsync();
                //try
                //{
                //    await _defaultProgressDialogController.CloseAsync();
                //}
                //catch (Exception)
                //{

                    
                //}
                
                _defaultProgressDialogController = null;
            }
        }
        public void SetDefaultProgressMessage(string message)
        {
            if (_defaultProgressDialogController != null) _defaultProgressDialogController.SetMessage(message);
        }
        public void LogoutUser()
        {
            _dialogHost.Logout();
        }
        
    }
    public static class DialogService
    {
        public static DialogManager MainDialogManager { get; private set; }
        public static DialogManager Init(IDialogHost dialogHost, IDialogCoordinator dialogCoordinator, bool initDefaultInteractionsHandlers)
        {
            MainDialogManager = new DialogManager(dialogHost, dialogCoordinator);
            if(initDefaultInteractionsHandlers)
            {
                Interactions.SimpleMessage.RegisterHandler(async interaction =>
                {
                    var result = await MainDialogManager.ShowMessageAsync(interaction.Input.Title, interaction.Input.Message);
                    if (result == MessageDialogResult.Affirmative) interaction.SetOutput(InteractionResult.OK);
                    else interaction.SetOutput(InteractionResult.Cancel);
                });
                Interactions.CriticalError.RegisterHandler(async interaction =>
                {
                    await MainDialogManager.RaiseCriticalErrorAsync(interaction.Input.Message);
                    interaction.SetOutput(InteractionResult.OK);                    
                });


                //Interactions.SimpleMessage.Subscribe(async input => await MainDialogManager.ShowMessageAsync(input.Title, input.Message));
                //Interactions.LongTimeOperationStarter.Subscribe(async input => await MainDialogManager.ShowDefaultProgressAsync(input.Title, input.Message));
                //Interactions.LongTimeOperationFinisher.Subscribe(async _ => await MainDialogManager.HideDefaultProgressAsync());
                //Interactions.CriticalError.Subscribe(async input => await MainDialogManager.RaiseCriticalErrorAsync(input.Message));
                Interactions.Confirmation.RegisterHandler(async interaction =>
                {
                    var result = await MainDialogManager.ShowConfirmationAsync(
                        interaction.Input.Title,
                        interaction.Input.Message,
                        interaction.Input.YesString,
                        interaction.Input.NoString);
                    if (result) interaction.SetOutput(InteractionResult.Yes);
                    else interaction.SetOutput(InteractionResult.No);
                });

                Interactions.LongTimeOperationStarter.RegisterHandler(async interaction =>
                {
                    await MainDialogManager.ShowDefaultProgressAsync(interaction.Input.Title, interaction.Input.Message);
                    interaction.SetOutput(InteractionResult.OK);                    
                });
                Interactions.LongTimeOperationFinisher.RegisterHandler(async interaction =>
                {
                    await MainDialogManager.HideDefaultProgressAsync();
                    interaction.SetOutput(InteractionResult.OK);
                });
                
            }


            return MainDialogManager;
        }
    }
}
