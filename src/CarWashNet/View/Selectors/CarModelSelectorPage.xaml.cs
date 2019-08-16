using CarWashNet.ViewModel;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CarWashNet.View
{
    public partial class CarModelSelectorPage : Page, IViewFor<CarModelSelectorViewModel>
    {
        protected bool firstRun = true;
        public CarModelSelectorViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (CarModelSelectorViewModel)value; } }
        private string _appCode;
        public CarModelSelectorPage(CarModelSelectorViewModel viewModel, string appCode)
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
