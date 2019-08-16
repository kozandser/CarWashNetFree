using CarWashNet.ViewModel;
using CarWashNet.Application.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarWashNet.View
{
    public partial class OrderEditorPage : Page, IViewFor<OrderEditorViewModel>
    {
        protected bool firstRun = true;
        public OrderEditorViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (OrderEditorViewModel)value; } }

        private string _appCode;
        private CarSelectorPage _carSelectorPage;
        private ClientSelectorPage _clientSelectorPage;

        public OrderEditorPage(OrderEditorViewModel viewModel, string appCode)
        {
            InitializeComponent();
            _appCode = appCode;
            ViewModel = viewModel;
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     firstRun = false;
                     viewModel.SelectCar.Subscribe(vm =>
                     {
                         if (_carSelectorPage == null) _carSelectorPage = new CarSelectorPage(vm, _appCode);
                         else _carSelectorPage.ViewModel = vm;

                         NavigationService.Navigate(_carSelectorPage);
                     });
                     viewModel.SelectClient.Subscribe(vm =>
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
