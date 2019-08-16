using CarWashNet.ViewModel;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class AppSettingsPage : Page, IViewFor<AppSettingsViewModel>
    {
        protected bool firstRun = true;
        public AppSettingsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (AppSettingsViewModel)value; } }
        public AppSettingsPage()
        {
            InitializeComponent();
            ViewModel = new AppSettingsViewModel();
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     firstRun = false;
                 }
             });           
            
        }        
    }
}
