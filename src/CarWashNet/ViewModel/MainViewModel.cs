using CarWashNet.Applications;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.ViewModel
{
    public class MainViewModel : ReactiveObject
    {
        [Reactive] public string DbVersion { get; set; } = "<нет значения>";
        [Reactive] public string AppVersion { get; set; } = "<нет значения>";
        [Reactive] public string CurrentUserName { get; set; } = "<нет входа>";
        [Reactive] public Updater Updater { get; private set; }

        [Reactive] public List<User> Users { get; set; }
        [Reactive] public User SelectedUser { get; set; }
        [Reactive] public bool HasSavedUserPassword { get; set; }
        [Reactive] public bool WrongPassword { get; set; }

        public ReactiveCommand<Unit, bool> CheckAutoLogin { get; set; }
        public ReactiveCommand<string, bool> Login { get; set; }
        public ReactiveCommand<Unit, bool> Logout { get; set; }
        public ReactiveCommand<Unit, Unit> FastLogout { get; set; }
        public ReactiveCommand<Unit, Unit> Init { get; set; }

        public MainViewModel()
        {
            Init = ReactiveCommand.CreateFromTask(async _ =>
            {
                await Interactions.StartLongTimeOperation("Ждите...", "Подключение к БД...");   
                DbService.Init();
                DbVersion = (await DbService.ConnectToDbAsync()).ToString();


                //throw new DbCriticalException("ASD");

                await Interactions.StartLongTimeOperation("Ждите...", "Загрузка настроек...");
                await GlobalService.InitAsync();

                DbVersion = DbService.DbVersion;
                AppVersion = DbService.AppVersion.ToString();
                Updater = GlobalService.Updater;

                await Interactions.FinishLongTimeOperation();                
                await CheckAutoLogin.Execute();

            });
            Init.ThrownExceptions.Subscribe(async ex => await Interactions.RaiseCriticalError(ex.Message));
            CheckAutoLogin = ReactiveCommand.Create<Unit, bool>(_ =>
            {
                if (GlobalService.AppSettings.HasSavedUserPassword == false)
                {
                    return false;
                }
                else
                {
                    string decryptedSavedPassword;
                    try
                    {
                        decryptedSavedPassword = GlobalService.AppSettings.DecryptLastUserPassword();
                        DbService.SetCurrentUser(GlobalService.AppSettings.LastUserID, decryptedSavedPassword);
                    }
                    catch
                    {
                        return false;
                    }
                    CurrentUserName = DbService.CurrentUser.Caption;
                    return true;
                }
            });
            CheckAutoLogin.Subscribe(b =>
            {
                if (b == false)
                {
                    resetAutoLogin();
                    HasSavedUserPassword = false;
                    loadUsers();
                }
            });
            Login = ReactiveCommand.Create<string, bool>(password =>
            {
                try
                {
                    DbService.SetCurrentUser(SelectedUser.ID, password);
                }
                catch
                {
                    WrongPassword = true;
                    return false;
                }

                GlobalService.AppSettings.LastUserID = DbService.CurrentUser.ID;
                GlobalService.AppSettings.HasSavedUserPassword = HasSavedUserPassword;
                if (HasSavedUserPassword) GlobalService.AppSettings.EncryptLastUserPassword(password);
                GlobalService.SaveSettings();

                CurrentUserName = DbService.CurrentUser.Caption;
                WrongPassword = false;
                return true;
            }, this.WhenAnyValue(p => p.SelectedUser).Select(p => p != null));
            Login.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            FastLogout = ReactiveCommand.Create(() =>
            {
                resetAutoLogin();
                DbService.ResetCurrentUser();
                CurrentUserName = "<нет входа>";
                Apps.AppRepository.Reset();
                loadUsers();
            });
            Logout = ReactiveCommand.CreateFromTask<Unit, bool>(async _ =>
            {
                var confirm = await Interactions.ShowConfirmationAsync("Выход", "Сменить пользователя?", "Сменить пользователя", "Нет");
                if (confirm == InteractionResult.Yes)
                {
                    //resetAutoLogin();
                    //DbService.ResetCurrentUser();
                    //CurrentUserName = "<нет входа>";
                    //Apps.AppRepository.Reset();
                    //loadUsers();
                    return true;
                }
                else return false;
            });
            Logout.Where(p => p == true).Select(p => Unit.Default).InvokeCommand(FastLogout);

            Interactions.UserLogout.RegisterHandler(interaction =>
                {
                    FastLogout.Execute().Subscribe();
                    interaction.SetOutput(InteractionResult.OK);
                });
        }
        private void resetAutoLogin()
        {
            GlobalService.AppSettings.HasSavedUserPassword = false;
            GlobalService.AppSettings.LastUserPassword = String.Empty;
            GlobalService.AppSettings.LastAppCode = "";
            GlobalService.SaveSettings();
        }
        private void loadUsers()
        {
            using (var db = DbService.GetDb())
            {
                Users = db.Users
                    .OnlyNotDeleted()
                    .OnlyActive()
                    .ToList();
            }
            SelectedUser = Users.FirstOrDefault(p => p.ID == GlobalService.AppSettings.LastUserID);
        }


    }
}
