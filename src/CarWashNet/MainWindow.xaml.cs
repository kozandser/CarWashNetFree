using CarWashNet.Applications;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using CarWashNet.Presentation;
using CarWashNet.ViewModel;
using ControlzEx;
using LinqToDB;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarWashNet.WPF
{
    public partial class MainWindow : IViewFor<MainViewModel>, IDialogHost
    {
        private bool canClose;
        #region IViewFor
        public MainViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (MainViewModel)value; } }
        #endregion
        #region IDialogHost
        public async Task RaiseCriticalErrorAsync(string message)
        {
            await this.ShowMessageAsync("Неисправимая ошибка!", message, MessageDialogStyle.Affirmative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Закрыть программу"
                });
            CloseApp();
        }
        
        public void Logout()
        {
            //ViewModel.Logout.Execute().Subscribe();
        }
        #endregion

        BaseMetroDialog LoginDialog => (BaseMetroDialog)this.Resources["LoginDialog"];
        AppMenuControl menuControl;

        public void CloseApp()
        {
            canClose = true;
            Close();
        }

        public MainWindow()
        {
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
            InitializeComponent();
            DialogService.Init(this, DialogCoordinator.Instance, true);
            
            this.WhenActivated(async disposables =>
            {
                ViewModel.CheckAutoLogin.Subscribe(p =>
                {
                    if (p == false) ShowLoginDialogAsync();
                    else LoadMainControl();
                });
                ViewModel.Login.Subscribe(p =>
                    {
                        if (p == true)
                        {
                            HideLoginDialog();
                            LoadMainControl();
                        }
                    });
                ViewModel.FastLogout.Subscribe(_ =>
                {
                    ContentGrid.Content = null;
                    ShowLoginDialogAsync();
                });
                //ViewModel.Logout.Subscribe(p =>
                //{
                //    if (p == true)
                //    {
                //        ContentGrid.Content = null;
                //        ShowLoginDialogAsync();
                //    }
                //});


                try
                {
                    await ViewModel.Init.Execute();
                }
                catch
                {
                    
                }
            });
        }
        private async void ShowLoginDialogAsync()
        {
            
            if (LoginDialog.IsVisible == false)
            {
                await this.ShowMetroDialogAsync(LoginDialog);
                var ps = LoginDialog.FindChild<PasswordBox>("tbPassword");
                if (ps != null) ps.Password = String.Empty;
                KeyboardNavigationEx.Focus(ps);
            }
            LoginDialog.DataContext = ViewModel;
        }
        private void HideLoginDialog()
        {
            if (LoginDialog.IsVisible == true) this.HideMetroDialogAsync(LoginDialog);

        }
        private void LoadMainControl()
        {
            menuControl = new AppMenuControl();
            ContentGrid.Content = menuControl;
        }

        #region Events
        private async void MetroWindow_ClosingAsync(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canClose)
            {
                e.Cancel = true;
                var result = await this.ShowMessageAsync("Выход", "Выйти из программы?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new MetroDialogSettings()
                    {
                        AffirmativeButtonText = "Выйти",
                        NegativeButtonText = "Нет"
                    });

                if (result == MessageDialogResult.Affirmative)
                {
                    CloseApp();
                }
            }
            else
            {
                System.Windows.Application.Current.Shutdown();
            }

        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var ps = LoginDialog.FindChild<PasswordBox>("tbPassword");
            ViewModel.Login.Execute(ps.Password).Subscribe();
        }
        private void tbPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                var ps = LoginDialog.FindChild<PasswordBox>("tbPassword");
                ViewModel.Login.Execute(ps.Password).Subscribe();
            }
        }
        private void tbPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.WrongPassword = false;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            GlobalService.AppSettings.LastAppCode = menuControl?.ViewModel.SelectedItem?.Code;
        }
        #endregion
    }

    

    
}
