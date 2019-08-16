using CarWashNet.ViewModel;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class CarsPage : Page, IViewFor<CarsViewModel>
    {
        protected bool firstRun = true;
        public CarsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (CarsViewModel)value; } }

        private string _appCode;
        private CarModelSelectorPage _carModelSelectorPage;
        private ClientSelectorPage _clientSelectorPage;

        public CarsPage(CarsViewModel viewModel, string appCode)
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
                     ViewModel.Init(0);
                     firstRun = false;

                     ViewModel.EditorViewModel.SelectCarModel.Subscribe(vm =>
                     {
                         if (_carModelSelectorPage == null) _carModelSelectorPage = new CarModelSelectorPage(vm, _appCode);
                         else _carModelSelectorPage.ViewModel = vm;

                         NavigationService.Navigate(_carModelSelectorPage);
                     });
                     ViewModel.EditorViewModel.SelectClient.Subscribe(vm =>
                     {
                         if (_clientSelectorPage == null) _clientSelectorPage = new ClientSelectorPage(vm, _appCode);
                         else _clientSelectorPage.ViewModel = vm;

                         NavigationService.Navigate(_clientSelectorPage);
                     });
                 }
             });            
        }        
    }
}
