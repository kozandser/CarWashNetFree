using CarWashNet.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class ClientSelectorPage : Page, IViewFor<ClientSelectorViewModel>
    {
        protected bool firstRun = true;
        public ClientSelectorViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (ClientSelectorViewModel)value; } }
        private string _appCode;
        public ClientSelectorPage(ClientSelectorViewModel viewModel, string appCode)
        {
            InitializeComponent();
            ViewModel = viewModel;
            _appCode = appCode;

            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     //ViewModel.Init(0);
                     ViewModel.Select.Subscribe(_ => NavigationService.GoBack());

                     firstRun = false;


                 }
             });
        }
    }
}
