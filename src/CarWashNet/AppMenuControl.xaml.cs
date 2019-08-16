using CarWashNet.Applications;
using CarWashNet.Apps;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using CarWashNet.ViewModel;
using MahApps.Metro.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CarWashNet.WPF
{
    public partial class AppMenuControl : UserControl, IViewFor<AppMenuViewModel>
    {
        public AppMenuViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (AppMenuViewModel)value; } }
        public AppMenuControl()
        {
            InitializeComponent();
            ViewModel = new AppMenuViewModel();
            this.DataContext = ViewModel;

            this.WhenActivated(disposables =>
            {
                ViewModel.WhenAnyValue(p => p.SelectedItem)
                    .Where(p => p != null)
                    .Subscribe(p =>
                    {
                        HamburgerMenuControl.Content = AppRepository.GetApp(p.Code, p.Caption);
                    });

                ViewModel.WhenAnyValue(p => p.SelectedOptionsItem)
                    .Where(p => p != null)
                    .Subscribe(p =>
                    {
                        HamburgerMenuControl.Content = AppRepository.GetApp("AppSettings", "Настройки программы");
                        //SelectedApp = new AppSettingsControl();
                    });
            });


        }
    }


}
