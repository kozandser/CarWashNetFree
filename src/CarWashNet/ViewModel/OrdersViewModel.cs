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
using System.Reactive;
using ReactiveUI.Fody.Helpers;
using CarWashNet.Domain.Managers;
using CarWashNet.Reports;
using CarWashNet.Applications;
using CarWashNet.Application;

namespace CarWashNet.ViewModel
{
    public class OrdersViewModel : BaseItemsWithStateViewModel<Order>
    {
        public PeriodFilter PeriodFilter { get; set; }

        public OrderEditorViewModel EditorViewModel { get; set; }
        public ReactiveCommand<Unit, Unit> SwitchReadiness { get; set; }
        public ReactiveCommand<Unit, Unit> SetReady { get; set; }
        public ReactiveCommand<Unit, Unit> SetUnready { get; set; }
        public ReactiveCommand<Unit, Unit> Print { get; set; }

        [Reactive] public DaySummaryViewModel DaySummaryViewModel { get; set; }

        public ReactiveCommand<DateTime, Unit> ShowDaySummary { get; set; }

        public OrdersViewModel()
        {
            PeriodFilter = new PeriodFilter();
            PeriodFilter.Periods.ForEach(p => p.IsActive = true);
            PeriodFilter.PeriodChanged.Select(p => 0).InvokeCommand(LoadItems);

            Items.ShapeView().OrderBy(p => p.InTime).Apply();
            Items.ShapeView().GroupBy(p => p.InTime.Date).Apply();

            EditorViewModel = new OrderEditorViewModel();
            EditorViewModel.Save.Subscribe(id =>
            {
                if(EditorViewModel.EditingItem.InDate < PeriodFilter.StartDate ||
                   EditorViewModel.EditingItem.InDate > PeriodFilter.EndDate)
                {
                    PeriodFilter.SetManualDate(EditorViewModel.EditingItem.InTime.Date);
                }
                
                LoadItems.Execute(id).Subscribe();
            });            
            
            SetReady = ReactiveCommand.Create(() =>
            {
                using (var db = DbService.GetDb())
                {
                    var manager = new OrderManager(db);
                    if (IsMultiSelect == false) manager.ValidateAndSetReadiness(SelectedItem, true);
                    else
                    {
                        db.BeginTransaction();
                        manager.SetReadiness(Items.OnlySelected(), true);
                        db.CommitTransaction();
                    }
                }
            }, canMultiEdit);
            SetReady.Select(p => 0).InvokeCommand(LoadItems);
            SetReady.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            SetUnready = ReactiveCommand.Create(() =>
            {
                using (var db = DbService.GetDb())
                {
                    var manager = new OrderManager(db);
                    if (IsMultiSelect == false) manager.ValidateAndSetReadiness(SelectedItem, false);
                    else
                    {
                        db.BeginTransaction();
                        manager.SetReadiness(Items.OnlySelected(), false);
                        db.CommitTransaction();
                    }
                }
            }, canMultiEdit);
            SetUnready.Select(p => 0).InvokeCommand(LoadItems);
            SwitchReadiness = ReactiveCommand.Create(() =>
            {
                EntityManagerService.DefaultOrderManager.ValidateAndSetReadiness(SelectedItem, !SelectedItem.IsClosed);                
            }, canEdit);
            SwitchReadiness.Select(p => 0).InvokeCommand(LoadItems);
            SwitchReadiness.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            DaySummaryViewModel = new DaySummaryViewModel();
            ShowDaySummary = ReactiveCommand.Create<DateTime>((day) =>
            {
                DaySummaryViewModel.Init(day);
            });

            Print = ReactiveCommand.CreateFromTask(async () =>
            {
                if(SelectedItem.IsClosed == false)
                {
                    await Interactions.ShowError("Заезд не закрыт!");
                    return;
                }
                var report = new OrderForClientReport("Reports/ReportResources/OrderForClient.frx");
                report.ShowReport(SelectedItem.ID);
            }, canEdit);
        }
        protected override async Task<IEnumerable<Order>> LoadItemsImpl()
        {
            await Interactions.StartLongTimeOperation("Ждите", "Загрузка заездов...");
            using (var db = DbService.GetDb())
            {
                var query = db.Orders
                    .LoadWith(p => p.User)
                    .LoadWith(p => p.Worker)
                    .LoadWith(p => p.Car)
                    .LoadWith(p => p.Car.CarModel)
                    .LoadWith(p => p.Client)
                    .LoadWith(p => p.Discountlist)
                    .OnlyNotDeleted()
                    .Where(p => p.InTime >= PeriodFilter.StartDate)
                    .Where(p => p.InTime <= PeriodFilter.EndDate);
                var result = await query.ToListAsync();
                //await Task.Delay(1000);
                await Interactions.FinishLongTimeOperation();
                return result;
            }
        }
        protected override void AddImpl()
        {
            var item = new Order()
            {
                UserID = DbService.CurrentUser.ID,
                User = DbService.CurrentUser,
                InTime = DateTime.Now,
                OutTime = DateTime.Now.AddHours(1),
                OrderGuid = Guid.NewGuid()
            };
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            var item = DbService.DefaultDb.Orders
                    .LoadWith(p => p.User)
                    .LoadWith(p => p.Worker)
                    .LoadWith(p => p.Car)
                    .LoadWith(p => p.Car.CarModel)
                    .LoadWith(p => p.Client)
                    .LoadWith(p => p.Discountlist)
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);

            EditorViewModel.Init(item);
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new OrderManager(db);
                if (IsMultiSelect == false) manager.ValidateAndDelete(SelectedItem, true);
                else
                {
                    db.BeginTransaction();
                    manager.ValidateAndDelete(Items.OnlySelected(), true);
                    db.CommitTransaction();
                }
            }
        }        
    }

    public class DaySummaryViewModel : ReactiveObject
    {
        public class DaySummaryItemViewModel : ReactiveObject
        {
            [Reactive] public string Worker { get; set; }
            [Reactive] public int OrderCount{ get; set; }
            [Reactive] public double Cost { get; set; }
            [Reactive] public double Pay { get; set; }
        }

        [Reactive]public DateTime Date { get; set; }
        [Reactive] public List<DaySummaryItemViewModel> Items { get; set; }
        [Reactive] public int OrderCount { get; set; }
        [Reactive] public double Cost { get; set; }
        [Reactive] public double Pay { get; set; }
        [Reactive] public bool IsOpen { get; set; }

        public void Init(DateTime date)
        {
            Date = date;
            using (var db = DbService.GetDb())
            {
                var manager = new OrderManager(db);
                var orders = manager.GetOrders(Date, Date.AddDays(1).AddSeconds(-1));
                var workers = db.Workers.ToList();

                OrderCount = orders.Count;
                Cost = orders.Sum(p => p.LastCost);
                Pay = orders.Sum(p => p.WorkerPay.Value);

                Items = orders.GroupBy(p => p.WorkerID).ToList()
                    .Select(g =>
                    {
                        return new DaySummaryItemViewModel()
                        {
                            Worker = workers.FirstOrDefault(p => p.ID == g.Key).Caption,
                            OrderCount = g.Count(),
                            Cost = g.Sum(p => p.LastCost),
                            Pay = g.Sum(p => p.WorkerPay.Value)
                        };
                    })
                    .ToList();                
            }
            IsOpen = true;
        }


    }
    
}
