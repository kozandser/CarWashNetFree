using CarWashNet.ViewModel;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Windows.Controls;

namespace CarWashNet.Pages
{
    public partial class CarModelsPage : Page, IViewFor<CarModelsViewModel>
    {
        protected bool firstRun = true;
        public CarModelsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (CarModelsViewModel)value; } }
        public CarModelsPage()
        {
            InitializeComponent();
            ViewModel = new CarModelsViewModel();
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
