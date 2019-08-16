using CarWashNet.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CarWashNet.View
{
    /// <summary>
    /// Логика взаимодействия для UserAppBindingsPage.xaml
    /// </summary>
    public partial class OrdersPage : Page, IViewFor<OrdersViewModel>
    {
        protected bool firstRun = true;
        public OrdersViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (OrdersViewModel)value; } }

        private string _appCode;
        private OrderEditorPage _orderEditorPage;
        public OrdersPage(OrdersViewModel viewModel, string appCode)
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
                     _orderEditorPage = new OrderEditorPage(ViewModel.EditorViewModel, _appCode);                     
                     ViewModel.Init(0);
                     ViewModel.Add.Subscribe(_ =>
                     {
                         NavigationService.Navigate(_orderEditorPage);
                     });
                     ViewModel.Edit.Subscribe(_ =>
                     {
                         NavigationService.Navigate(_orderEditorPage);
                     });                     
                     firstRun = false;
                 }
             });
        }
    }
}
