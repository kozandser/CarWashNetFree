using CarWashNet.ViewModel;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.Pages
{
    public partial class ClientsPage : Page, IViewFor<ClientsViewModel>
    {
        protected bool firstRun = true;
        public ClientsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (ClientsViewModel)value; } }
        public ClientsPage()
        {
            InitializeComponent();
            ViewModel = new ClientsViewModel();
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     ViewModel.Init(0);
                     firstRun = false;
                 }
             });           
            
        }        
    }
}
