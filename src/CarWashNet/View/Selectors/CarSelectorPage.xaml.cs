using CarWashNet.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class CarSelectorPage : Page, IViewFor<CarSelectorViewModel>
    {
        protected bool firstRun = true;
        public CarSelectorViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (CarSelectorViewModel)value; } }

        private string _appCode;
        private CarModelSelectorPage _carModelSelectorPage;
        private ClientSelectorPage _clientSelectorPage;

        public CarSelectorPage(CarSelectorViewModel viewModel, string appCode)
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
                     ViewModel.Select.Subscribe(_ => NavigationService.GoBack());

                     firstRun = false;
                 }
             });            
        }        
    }
}
