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

namespace CarWashNet.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserAppBindingsPage.xaml
    /// </summary>
    public partial class DiscountlistSelectorPage : Page, IViewFor<DiscountlistSelectorViewModel>
    {
        public DiscountlistSelectorViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (DiscountlistSelectorViewModel)value; } }

        public DiscountlistSelectorPage(DiscountlistSelectorViewModel viewModel)
        {
            //InitializeComponent();
            ViewModel = viewModel;

            this.WhenActivated(disposables =>
            {
                ViewModel.Select.Subscribe(_ =>
                    {
                        if (NavigationService != null) NavigationService.GoBack();
                    })
                    .DisposeWith(disposables);

                //this.Bind(ViewModel, p => p, p => p.DataContext);
                //.DisposeWith(disposables);

                this.WhenAnyValue(p => p.ViewModel).BindTo(this, x => 
                    x.DataContext)
                    .DisposeWith(disposables);
            });

            //ViewModel.Select.Subscribe(_ =>
            //{
            //    if (NavigationService != null) NavigationService.GoBack();
            //    //x.Dispose();
            //});
            //this.DataContext = ViewModel;

            
            InitializeComponent();




        }
        
        

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
