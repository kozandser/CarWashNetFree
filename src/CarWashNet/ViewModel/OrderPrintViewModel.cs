using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using LinqToDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.ViewModel
{
    public class OrderPrintViewModel : ReactiveObject
    {
        [Reactive] public Order Order { get; set; }
        [Reactive] public List<OrderItem> OrderItems { get; set; }
        public ReactiveCommand<Unit, Unit> Print { get; set; }
        //public ReportDocument Report { get; set; }

        public OrderPrintViewModel()
        {
            Report = new ReportDocument();
            Print = ReactiveCommand.Create(() =>
            {



            });
        }
        public void Init(int id)
        {
            using (var db = DbService.GetDb())
            {
                Order = db.Orders
                    .LoadWith(p => p.Car)
                    .LoadWith(p => p.Car.CarModel)
                    .LoadWith(p => p.Client)
                    .LoadWith(p => p.User)
                    .LoadWith(p => p.Worker)
                    .FirstOrDefault(p => p.ID == id);

                OrderItems = db.OrderItems
                    .LoadWith(p => p.Service)
                    .Where(p => p.OrderID == id)
                    .OrderBy(p => p.Service.Caption)
                    .ToList();

                //Report.Load("Reports/OrderForClient.rpt");
                //Report.SetDataSource(OrderItems);
                //Report.SetParameterValue("OrderDate", Order.InTime);
                //Report.SetParameterValue("CarModel", Order.Car?.CarModel?.Caption ?? "-");
                //Report.SetParameterValue("FedCode", Order.Car?.FedCode ?? "-");
                //Report.SetParameterValue("ClientCaption", Order.Client?.Caption ?? "-");
                //Report.SetParameterValue("ClientCard", Order.Client?.Card ?? "-");
            }
        }
    }
}
