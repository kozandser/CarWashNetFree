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
    public partial class ReportsPage : Page, IViewFor<ReportsViewModel>
    {
        protected bool firstRun = true;
        public ReportsViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get => ViewModel; set { ViewModel = (ReportsViewModel)value; } }
        
        public ReportsPage()
        {
            InitializeComponent();

            ViewModel = new ReportsViewModel();
            this.WhenActivated(disposables =>
             {
                 this.WhenAnyValue(p => p.ViewModel).BindTo(this, x =>
                     x.DataContext)
                     .DisposeWith(disposables);

                 if (firstRun)
                 {                    
                     firstRun = false;
                     ViewModel.RefreshReports.Subscribe(async p =>
                     {
                         var stat_srvc = await ViewModel.Calc_stat_srvc();
                         plot_stat_srvc.Model = stat_srvc.PlotModel;
                         dg_stat_srvc.ItemsSource = stat_srvc.List;

                         var stat_workers = await ViewModel.Calc_stat_workers();
                         plot_stat_workers.Model = stat_workers.PlotModel;
                         dg_stat_workers.ItemsSource = stat_workers.List;

                         var stat_pays = await ViewModel.Calc_stat_pays();
                         plot_stat_pays.Model = stat_pays.PlotModel;
                         dg_stat_pays.ItemsSource = stat_pays.List;

                         var dyn_days = await ViewModel.Calc_dyn_days();
                         plot_dyn_days_cost.Model = dyn_days.PlotModelCost;
                         plot_dyn_days_qty.Model = dyn_days.PlotModelQty;
                         dg_dyn_days.ItemsSource = dyn_days.List;

                         var dyn_weeks = await ViewModel.Calc_dyn_weeks();
                         plot_dyn_weeks_cost.Model = dyn_weeks.PlotModelCost;
                         plot_dyn_weeks_qty.Model = dyn_weeks.PlotModelQty;
                         dg_dyn_weeks.ItemsSource = dyn_weeks.List;

                         var dyn_months = await ViewModel.Calc_dyn_month();
                         plot_dyn_months_cost.Model = dyn_months.PlotModelCost;
                         plot_dyn_months_qty.Model = dyn_months.PlotModelQty;
                         dg_dyn_months.ItemsSource = dyn_months.List;                         

                     });

                     ViewModel.RefreshReports.Execute().Subscribe();

                     ViewModel.TimelineDayViewModel.RefreshReport.Subscribe(async p =>
                     {
                         var timeline_day = await ViewModel.TimelineDayViewModel.Calc_timeline_day();
                         plot_timeline_day.Model = timeline_day.PlotModel;

                     });
                     ViewModel.TimelineDayViewModel.RefreshReport.Execute().Subscribe();
                 }
             });
        }
    }
}
