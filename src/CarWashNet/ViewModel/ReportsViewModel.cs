using System;
using System.Collections.Generic;
using System.Linq;
using KLib.Native;
using ReactiveUI;
using System.Reactive.Linq;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using System.Threading.Tasks;
using LinqToDB;
using CarWashNet.Domain.Repository;
using CarWashNet.Application;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using CarWashNet.Applications;

namespace CarWashNet.ViewModel
{
    public class ReportsViewModel : ReactiveObject
    {
        List<OxyColor> FlatPalette = new List<OxyColor>
                {
                    OxyColor.Parse("#1abc9c"),
                    OxyColor.Parse("#2ecc71"),
                    OxyColor.Parse("#3498db"),
                    OxyColor.Parse("#9b59b6"),
                    OxyColor.Parse("#34495e"),
                    OxyColor.Parse("#16a085"),
                    OxyColor.Parse("#27ae60"),
                    OxyColor.Parse("#2980b9"),
                    OxyColor.Parse("#8e44ad"),
                    OxyColor.Parse("#2c3e50"),
                    OxyColor.Parse("#f1c40f"),
                    OxyColor.Parse("#e67e22"),
                    OxyColor.Parse("#e74c3c"),
                    OxyColor.Parse("#ecf0f1"),
                    OxyColor.Parse("#e74c3c"),
                    OxyColor.Parse("#95a5a6"),
                    OxyColor.Parse("#f39c12"),
                    OxyColor.Parse("#d35400"),
                    OxyColor.Parse("#c0392b"),
                    OxyColor.Parse("#bdc3c7"),
                    OxyColor.Parse("#7f8c8d")

                };

        public PeriodFilter PeriodFilter { get; set; }

        [Reactive]public int OrdersCount { get; set; }
        [Reactive] public int OrderItemsCount { get; set; }
        [Reactive] public double OrdersCost { get; set; }

        public ReactiveCommand<Unit, Unit> RefreshReports { get; set; }

        private List<Order> orders;
        private List<OrderItem> orderItems;
        private List<Service> services;
        private List<Worker> workers;

        public TimelineDayViewModel TimelineDayViewModel { get; set; }

        public ReportsViewModel()
        {
            PeriodFilter = new PeriodFilter();
            PeriodFilter.Periods.ForEach(p => p.IsActive = true);

            RefreshReports = ReactiveCommand.CreateFromTask(async () =>
            {
                await loadDataAsync();
                calcSummary();                
            });
            RefreshReports.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            PeriodFilter.PeriodChanged.Subscribe(p =>
            {
                RefreshReports.Execute().Subscribe();
            });


            TimelineDayViewModel = new TimelineDayViewModel();
        }
       
        void calcSummary()
        {
            OrdersCount = orders.Count();
            OrderItemsCount = orderItems.Sum(p => p.Quantity);
            OrdersCost = orders.Sum(p => p.LastCost);
        }
        async Task loadDataAsync()
        {
            using (var db = DbService.GetDb())
            {
                orderItems = await db.OrderItems
                    .LoadWith(p => p.Order)
                    .Where(p => p.Order.EntityState == EntityStateEnum.Active)
                    .Where(p => p.Order.CloseTime != null)
                    .Where(p => p.Order.InTime >= PeriodFilter.StartDate && p.Order.InTime <= PeriodFilter.EndDate)
                    .ToListAsync();

                orders = await db.Orders
                    .Where(p => p.CloseTime != null)
                    .OnlyActive()
                    .Where(p => p.InTime >= PeriodFilter.StartDate && p.InTime <= PeriodFilter.EndDate)
                    .ToListAsync();

                services = db.Services.ToList();
                workers = db.Workers.ToList();
            }
        }

        public async Task<(List<StatClass> List, PlotModel PlotModel)> Calc_stat_srvc()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Статистика по услугам";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0.2;
            categoryAxis1.TickStyle = TickStyle.None;
            categoryAxis1.MinimumPadding = 0.1;
            categoryAxis1.MaximumPadding = 0.1;
            categoryAxis1.Position = AxisPosition.Left;
            categoryAxis1.Labels.Add("Сумма");
            categoryAxis1.Labels.Add("Количество");
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.Position = AxisPosition.Top;
            linearAxis1.Key = "Q";
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            plotModel1.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.MinimumPadding = 0;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Key = "C";
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.IsPanEnabled = false;
            plotModel1.Axes.Add(linearAxis2);
            #endregion

            return await Task.Run(() =>
            {
                var query = from oi in orderItems
                            group oi by oi.ServiceID into gr
                            select new StatClass()
                            {
                                Caption = services.FirstOrDefault(p => p.ID == gr.Key).Caption,
                                Count = gr.Sum(p => p.Quantity),
                                Sum = gr.Sum(p => p.LastPrice)
                            };
                list = query.OrderBy(p => p.Caption).ToList();

                foreach (var i in list.OrderByDescending(p => p.Count))
                {
                    var barSeries1 = new BarSeries();
                    barSeries1.IsStacked = true;
                    barSeries1.StrokeThickness = 0;
                    barSeries1.Title = i.Caption;
                    barSeries1.XAxisKey = "Q";
                    barSeries1.Items.Add(new BarItem(i.Count, 1));
                    plotModel1.Series.Add(barSeries1);
                }
                foreach (var i in list.OrderByDescending(p => p.Sum))
                {
                    var barSeries1 = new BarSeries();
                    barSeries1.IsStacked = true;
                    barSeries1.StrokeThickness = 0;
                    barSeries1.Title = i.Caption;
                    barSeries1.XAxisKey = "C";
                    barSeries1.TrackerFormatString = "{0}\n{1}: {2:C2}";
                    barSeries1.Items.Add(new BarItem(i.Sum, 0));
                    plotModel1.Series.Add(barSeries1);
                }

                return (list, plotModel1);
            });
        }
        public async Task<(List<StatClass> List, PlotModel PlotModel)> Calc_stat_workers()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Статистика по работникам";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0.2;
            categoryAxis1.TickStyle = TickStyle.None;
            categoryAxis1.MinimumPadding = 0.1;
            categoryAxis1.MaximumPadding = 0.1;
            categoryAxis1.Position = AxisPosition.Left;
            categoryAxis1.Labels.Add("Сумма");
            categoryAxis1.Labels.Add("Количество заездов");
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.Position = AxisPosition.Top;
            linearAxis1.Key = "Q";
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            plotModel1.Axes.Add(linearAxis1);

            var linearAxis2 = new LinearAxis();
            linearAxis2.MinimumPadding = 0;
            linearAxis2.Position = AxisPosition.Bottom;
            linearAxis2.Key = "C";
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.IsPanEnabled = false;
            plotModel1.Axes.Add(linearAxis2);
            #endregion

            return await Task.Run(() =>
            {
                var query = from o in orders
                            group o by o.WorkerID into gr
                            select new StatClass()
                            {
                                Caption = workers.FirstOrDefault(p => p.ID == gr.Key).Caption,
                                Count = gr.Count(),
                                Sum = gr.Sum(p => p.LastCost)
                            };
                list = query.OrderBy(p => p.Caption).ToList();

                foreach (var i in list.OrderByDescending(p => p.Count))
                {
                    var barSeries1 = new BarSeries();
                    barSeries1.IsStacked = true;
                    barSeries1.StrokeThickness = 0;
                    barSeries1.Title = i.Caption;
                    barSeries1.XAxisKey = "Q";
                    barSeries1.Items.Add(new BarItem(i.Count, 1));
                    plotModel1.Series.Add(barSeries1);
                }
                foreach (var i in list.OrderByDescending(p => p.Sum))
                {
                    var barSeries1 = new BarSeries();
                    barSeries1.IsStacked = true;
                    barSeries1.StrokeThickness = 0;
                    barSeries1.Title = i.Caption;
                    barSeries1.XAxisKey = "C";
                    barSeries1.TrackerFormatString = "{0}\n{1}: {2:C2}";
                    barSeries1.Items.Add(new BarItem(i.Sum, 0));
                    plotModel1.Series.Add(barSeries1);
                }

                return (list, plotModel1);
            });
        }
        public async Task<(List<StatClass> List, PlotModel PlotModel)> Calc_stat_pays()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Статистика по выплатам";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0.2;
            categoryAxis1.TickStyle = TickStyle.None;
            categoryAxis1.MinimumPadding = 0.1;
            categoryAxis1.MaximumPadding = 0.1;
            categoryAxis1.Position = AxisPosition.Left;
            categoryAxis1.Key = "W";
            categoryAxis1.Labels.Add("Выплата");
            //categoryAxis1.Labels.Add("sd");
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.Position = AxisPosition.Top;
            linearAxis1.Key = "Q";
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            plotModel1.Axes.Add(linearAxis1);
            #endregion

            return await Task.Run(() =>
            {
                var query = from o in orders
                            group o by o.WorkerID into gr
                            select new StatClass()
                            {
                                Caption = workers.FirstOrDefault(p => p.ID == gr.Key).Caption,
                                Count = gr.Count(),
                                Sum = gr.Sum(p => p.WorkerPay.Value)
                            };
                list = query.OrderBy(p => p.Caption).ToList();

                foreach (var i in list.OrderByDescending(p => p.Sum))
                {
                    var barSeries1 = new BarSeries();
                    barSeries1.IsStacked = true;
                    barSeries1.StrokeThickness = 0;
                    barSeries1.Title = i.Caption;
                    barSeries1.XAxisKey = "Q";
                    barSeries1.YAxisKey = "W";
                    barSeries1.TrackerFormatString = "{0}\n{1}: {2:C2}";
                    barSeries1.Items.Add(new BarItem(i.Sum, 0));
                    plotModel1.Series.Add(barSeries1);
                }

                return (list, plotModel1);
            });
        }
        public async Task<(List<StatClass> List, PlotModel PlotModelQty, PlotModel PlotModelCost)> Calc_dyn_days()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Динамика по заездам (дни)";
            plotModel1.Subtitle = "Количество";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;
            plotModel1.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MinimumPadding = 0;
            categoryAxis1.MaximumPadding = 0;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.IsAxisVisible = false;
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.MajorStep = 10;
            linearAxis1.MinorStep = 5;
            plotModel1.Axes.Add(linearAxis1);

            var plotModel2 = new PlotModel();
            plotModel2.Subtitle = "Сумма";
            plotModel2.IsLegendVisible = false;
            plotModel2.DefaultColors = FlatPalette;
            plotModel2.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.IsZoomEnabled = false;
            categoryAxis2.IsPanEnabled = false;
            categoryAxis2.GapWidth = 0;
            categoryAxis2.MinimumPadding = 0;
            categoryAxis2.MaximumPadding = 0;
            categoryAxis2.Position = AxisPosition.Bottom;
            categoryAxis2.IsAxisVisible = false;
            plotModel2.Axes.Add(categoryAxis2);

            var linearAxis2 = new LinearAxis();
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.Position = AxisPosition.Left;
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.IsPanEnabled = false;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            //linearAxis2.MajorStep = 10;
            //linearAxis2.MinorStep = 5;
            plotModel2.Axes.Add(linearAxis2);
            #endregion

            return await Task.Run(() =>
            {
                var period = new List<string>();
                for (var dt = PeriodFilter.StartDate; dt <= PeriodFilter.EndDate; dt = dt.AddDays(1))
                {
                    list.Add(new StatClass()
                    {
                        Caption = dt.ToString("dd.MM.yyyy")
                    });
                }

                var query = from o in orders
                            group o by o.InTime.ToString("dd.MM.yyyy") into gr
                            select new StatClass()
                            {
                                Caption = gr.Key,
                                Count = gr.Count(),
                                Sum = gr.Sum(p => p.LastCost)
                            };

                foreach (var q in query)
                {
                    var l = list.FirstOrDefault(p => p.Caption == q.Caption);
                    l.Count = q.Count;
                    l.Sum = q.Sum;
                }

                var columnSeries1 = new ColumnSeries();
                columnSeries1.StrokeThickness = 0;
                columnSeries1.ValueField = "Count";
                columnSeries1.FillColor = OxyColor.Parse("#2ecc71");
                foreach (var i in list)
                {
                    columnSeries1.Items.Add(new ColumnItem(i.Count));
                    categoryAxis1.Labels.Add(i.Caption);
                }
                plotModel1.Series.Add(columnSeries1);

                var columnSeries2 = new ColumnSeries();
                columnSeries2.StrokeThickness = 0;
                columnSeries2.ValueField = "Sum";
                columnSeries2.TrackerFormatString = "{0}\n{1}: {2:C2}";
                columnSeries2.FillColor = OxyColor.Parse("#3498db");
                foreach (var i in list)
                {
                    columnSeries2.Items.Add(new ColumnItem(i.Sum));
                    categoryAxis2.Labels.Add(i.Caption);
                }
                plotModel2.Series.Add(columnSeries2);

                return (list, plotModel1, plotModel2);
            });
        }
        public async Task<(List<StatClass> List, PlotModel PlotModelQty, PlotModel PlotModelCost)> Calc_dyn_weeks()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Динамика по заездам (недели)";
            plotModel1.Subtitle = "Количество";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;
            plotModel1.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MinimumPadding = 0;
            categoryAxis1.MaximumPadding = 0;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.IsAxisVisible = false;
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.MajorStep = 10;
            linearAxis1.MinorStep = 5;
            plotModel1.Axes.Add(linearAxis1);

            var plotModel2 = new PlotModel();
            plotModel2.Subtitle = "Сумма";
            plotModel2.IsLegendVisible = false;
            plotModel2.DefaultColors = FlatPalette;
            plotModel2.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.IsZoomEnabled = false;
            categoryAxis2.IsPanEnabled = false;
            categoryAxis2.GapWidth = 0;
            categoryAxis2.MinimumPadding = 0;
            categoryAxis2.MaximumPadding = 0;
            categoryAxis2.Position = AxisPosition.Bottom;
            categoryAxis2.IsAxisVisible = false;
            plotModel2.Axes.Add(categoryAxis2);

            var linearAxis2 = new LinearAxis();
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.Position = AxisPosition.Left;
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.IsPanEnabled = false;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            //linearAxis2.MajorStep = 10;
            //linearAxis2.MinorStep = 5;
            plotModel2.Axes.Add(linearAxis2);
            #endregion

            return await Task.Run(() =>
            {
                var period = new List<DateTime>();
                for (var dt = PeriodFilter.StartDate; dt <= PeriodFilter.EndDate; dt = dt.AddDays(1))
                {
                    period.Add(dt);
                }
                list = period
                    .GroupBy(p => p.Year.ToString() + "." + Sql.DatePart(Sql.DateParts.Week, p.Date).ToString())
                    .Select(p => new StatClass()
                    {
                        Caption = p.Key
                    })
                    .ToList();

                var query = from o in orders
                            group o by o.InTime.Year.ToString() + "." + Sql.DatePart(Sql.DateParts.Week, o.InTime.Date).ToString() into gr
                            select new StatClass()
                            {
                                Caption = gr.Key,
                                Count = gr.Count(),
                                Sum = gr.Sum(p => p.LastCost)
                            };

                foreach (var q in query)
                {
                    var l = list.FirstOrDefault(p => p.Caption == q.Caption);
                    l.Count = q.Count;
                    l.Sum = q.Sum;
                }

                var columnSeries1 = new ColumnSeries();
                columnSeries1.StrokeThickness = 0;
                columnSeries1.ValueField = "Count";
                columnSeries1.FillColor = OxyColor.Parse("#2ecc71");
                foreach (var i in list)
                {
                    columnSeries1.Items.Add(new ColumnItem(i.Count));
                    categoryAxis1.Labels.Add(i.Caption);
                }
                plotModel1.Series.Add(columnSeries1);

                var columnSeries2 = new ColumnSeries();
                columnSeries2.StrokeThickness = 0;
                columnSeries2.ValueField = "Sum";
                columnSeries2.TrackerFormatString = "{0}\n{1}: {2:C2}";
                columnSeries2.FillColor = OxyColor.Parse("#3498db");
                foreach (var i in list)
                {
                    columnSeries2.Items.Add(new ColumnItem(i.Sum));
                    categoryAxis2.Labels.Add(i.Caption);
                }
                plotModel2.Series.Add(columnSeries2);

                return (list, plotModel1, plotModel2);
            });
        }
        public async Task<(List<StatClass> List, PlotModel PlotModelQty, PlotModel PlotModelCost)> Calc_dyn_month()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Динамика по заездам (месяцы)";
            plotModel1.Subtitle = "Количество";
            plotModel1.IsLegendVisible = false;
            plotModel1.DefaultColors = FlatPalette;
            plotModel1.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0;
            categoryAxis1.MinimumPadding = 0;
            categoryAxis1.MaximumPadding = 0;
            categoryAxis1.Position = AxisPosition.Bottom;
            categoryAxis1.IsAxisVisible = false;
            plotModel1.Axes.Add(categoryAxis1);

            var linearAxis1 = new LinearAxis();
            linearAxis1.MinimumPadding = 0;
            linearAxis1.MaximumPadding = 0;
            linearAxis1.Position = AxisPosition.Left;
            linearAxis1.IsZoomEnabled = false;
            linearAxis1.IsPanEnabled = false;
            linearAxis1.MajorGridlineStyle = LineStyle.Solid;
            linearAxis1.MinorGridlineStyle = LineStyle.Dot;
            linearAxis1.MajorStep = 10;
            linearAxis1.MinorStep = 5;
            plotModel1.Axes.Add(linearAxis1);

            var plotModel2 = new PlotModel();
            plotModel2.Subtitle = "Сумма";
            plotModel2.IsLegendVisible = false;
            plotModel2.DefaultColors = FlatPalette;
            plotModel2.PlotMargins = new OxyThickness(50, 0, 0, 0);

            var categoryAxis2 = new CategoryAxis();
            categoryAxis2.IsZoomEnabled = false;
            categoryAxis2.IsPanEnabled = false;
            categoryAxis2.GapWidth = 0;
            categoryAxis2.MinimumPadding = 0;
            categoryAxis2.MaximumPadding = 0;
            categoryAxis2.Position = AxisPosition.Bottom;
            categoryAxis2.IsAxisVisible = false;
            plotModel2.Axes.Add(categoryAxis2);

            var linearAxis2 = new LinearAxis();
            linearAxis2.MinimumPadding = 0;
            linearAxis2.MaximumPadding = 0;
            linearAxis2.Position = AxisPosition.Left;
            linearAxis2.IsZoomEnabled = false;
            linearAxis2.IsPanEnabled = false;
            linearAxis2.MajorGridlineStyle = LineStyle.Solid;
            linearAxis2.MinorGridlineStyle = LineStyle.Dot;
            //linearAxis2.MajorStep = 10;
            //linearAxis2.MinorStep = 5;
            plotModel2.Axes.Add(linearAxis2);
            #endregion

            return await Task.Run(() =>
            {
                var period = new List<DateTime>();
                for (var dt = PeriodFilter.StartDate; dt <= PeriodFilter.EndDate; dt = dt.AddDays(1))
                {
                    period.Add(dt);
                }
                list = period
                    .GroupBy(p => p.ToString("MMMM yyyy"))
                    .Select(p => new StatClass()
                    {
                        Caption = p.Key
                    })
                    .ToList();

                var query = from o in orders
                            group o by o.InTime.ToString("MMMM yyyy") into gr
                            select new StatClass()
                            {
                                Caption = gr.Key,
                                Count = gr.Count(),
                                Sum = gr.Sum(p => p.LastCost)
                            };

                foreach (var q in query)
                {
                    var l = list.FirstOrDefault(p => p.Caption == q.Caption);
                    l.Count = q.Count;
                    l.Sum = q.Sum;
                }

                var columnSeries1 = new ColumnSeries();
                columnSeries1.StrokeThickness = 0;
                columnSeries1.ValueField = "Count";
                columnSeries1.TrackerFormatString = "{0}\n{1}: {2}";
                columnSeries1.FillColor = OxyColor.Parse("#2ecc71");
                foreach (var i in list)
                {
                    columnSeries1.Items.Add(new ColumnItem(i.Count));
                    categoryAxis1.Labels.Add(i.Caption);
                }
                plotModel1.Series.Add(columnSeries1);

                var columnSeries2 = new ColumnSeries();
                columnSeries2.StrokeThickness = 0;
                columnSeries2.ValueField = "Sum";
                columnSeries2.TrackerFormatString = "{0}\n{1}: {2:C2}";
                columnSeries2.FillColor = OxyColor.Parse("#3498db");
                foreach (var i in list)
                {
                    columnSeries2.Items.Add(new ColumnItem(i.Sum));
                    categoryAxis2.Labels.Add(i.Caption);
                }
                plotModel2.Series.Add(columnSeries2);

                return (list, plotModel1, plotModel2);
            });
        }        
    }

    public class TimelineDayViewModel : ReactiveObject
    {
        [Reactive] public DateTime TimelineDayDate { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshReport { get; set; }

        public TimelineDayViewModel()
        {
            TimelineDayDate = DateTime.Today;

            RefreshReport = ReactiveCommand.Create(() =>
            {

            });
            RefreshReport.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            this.WhenAnyValue(p => p.TimelineDayDate)
                .Subscribe(p => RefreshReport.Execute().Subscribe());
        }

        public async Task<(List<StatClass> List, PlotModel PlotModel)> Calc_timeline_day()
        {
            var list = new List<StatClass>();

            #region Настройка графики
            var plotModel1 = new PlotModel();
            plotModel1.Title = "Загрузка за день";
            plotModel1.IsLegendVisible = false;

            var dateAxis1 = new DateTimeAxis();
            dateAxis1.Position = AxisPosition.Bottom;
            dateAxis1.IsZoomEnabled = false;
            dateAxis1.IsPanEnabled = false;
            plotModel1.Axes.Add(dateAxis1);

            var categoryAxis1 = new CategoryAxis();
            categoryAxis1.IsZoomEnabled = false;
            categoryAxis1.IsPanEnabled = false;
            categoryAxis1.GapWidth = 0.2;
            categoryAxis1.TickStyle = TickStyle.None;
            categoryAxis1.MinimumPadding = 0.1;
            categoryAxis1.MaximumPadding = 0.1;
            categoryAxis1.Position = AxisPosition.Left;
            plotModel1.Axes.Add(categoryAxis1);
            #endregion

            return await Task.Run(() =>
            {
                using (var db = DbService.GetDb())
                {
                    var orders = db.Orders
                        .LoadWith(p => p.Car)
                        .LoadWith(p => p.Car.CarModel)
                        .Where(p => p.CloseTime != null)
                        .OnlyActive()
                        .Where(p => p.InTime >= TimelineDayDate && p.InTime <= TimelineDayDate.AddDays(1).AddSeconds(-1))
                        .ToList()
                        .OrderByDescending(p => p.InTime)
                        .ToList();

                    IntervalBarSeries barSeries = new OxyPlot.Series.IntervalBarSeries();
                    barSeries.TrackerFormatString = "Автомобиль: {6}\n{4:HH:mm} - {5:HH:mm}";
                    //barSeries.TextColor = OxyColors.White;
                    barSeries.FillColor = OxyColor.Parse("#CCFF90");

                    var i = 0;
                    foreach (var order in orders)
                    {
                        categoryAxis1.Labels.Add(order.Car.FedCode);
                        IntervalBarItem item = new IntervalBarItem
                        {
                            Start = OxyPlot.Axes.DateTimeAxis.ToDouble(order.InTime),
                            End = OxyPlot.Axes.DateTimeAxis.ToDouble(order.OutTime),
                            CategoryIndex = i,
                            Title = $"{order.Car.FedCode}, {order.Car.CarModel.Caption}"
                            //Color = FlatPalette[0]
                        };
                        barSeries.Items.Add(item);
                        i++;
                    }

                    plotModel1.Series.Add(barSeries);
                }


                return (list, plotModel1);
            });
        }



    }

    public class StatClass
    {
        public string Caption { get; set; }
        public int Count { get; set; }
        public double Sum { get; set; }
    }
}
