using CarWashNet.ViewModel;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class DbSettingsPage : Page, IViewFor<DbSettingsViewModel>
    {
        protected bool firstRun = true;
        public DbSettingsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (DbSettingsViewModel)value; } }
        public DbSettingsPage()
        {
            InitializeComponent();
            ViewModel = new DbSettingsViewModel();
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     ViewModel.LoadSettings();
                     firstRun = false;
                 }
             });           
            
        }        
    }
}
