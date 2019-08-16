using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using CarWashNet.ViewModel;
using LinqToDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
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
    public partial class OrderPrintPage : Page, IViewFor<OrderPrintViewModel>
    {
        protected bool firstRun = true;
        public OrderPrintViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (OrderPrintViewModel)value; } }
        public OrderPrintPage(OrderPrintViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {
                     ViewModel.Print.Subscribe(p =>
                     {
                         PrintDialog printDialog = new PrintDialog();
                         if (printDialog.ShowDialog() == true)
                         {
                             //docViewer.PageHeight = printDialog.PrintableAreaHeight;
                             //docViewer.PageWidth = printDialog.PrintableAreaWidth;
                             //docViewer.PagePadding = new Thickness(50);
                             //docViewer.ColumnGap = 0;
                             //docViewer.ColumnWidth = printDialog.PrintableAreaWidth;

                             //printDialog.PrintDocument(
                             //       ((IDocumentPaginatorSource)docViewer).DocumentPaginator,
                             //       "A Flow Document");
                         }
                     });                     

                     firstRun = false;
                 }
             });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //var report = new ReportDocument();
            //report.Load("Reports/OrderForClient.rpt");
            //report.SetDataSource(ViewModel.OrderItems);
            //report.SetParameterValue("OrderDate", ViewModel.Order.InTime);


            CrystalReportsViewer1.ViewerCore.ReportSource = ViewModel.Report;
        }
    }

    
}
