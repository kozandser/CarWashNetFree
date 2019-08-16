using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Services;
using FastReport;
using LinqToDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Reports
{
    public class OrderForClientReportDTO
    {
        public List<OrderForClientReportOrderDTO> Items { get; set; }
    }
    public class OrderForClientReportOrderDTO
    {
        public string OrganizationPrintCaption { get; set; }
        public string OrderPrintCaption { get; set; }

        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public double LastCost { get; set; }
        public Double WorkerPay { get; set; }
        public string CarModel { get; set; }
        public string FedCode { get; set; }
        public string ClientCaption { get; set; }
        public string ClientCard { get; set; }
        public string UserCaption { get; set; }
        public string WorkerCaption { get; set; }

        public List<OrderForClientReportOrderItemDTO> Items { get; set; }
    }

    public class OrderForClientReportOrderItemDTO
    {
        public string ServiceCaption { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Cost { get; set; }
        public double Discount { get; set; }
        public double LastPrice { get; set; }
        public double LastCost { get; set; }
    }

    public class OrderForClientReport : ReportBase
    {
        public OrderForClientReport(string reportFile) : base(reportFile)
        {

        }
        public void ShowReport(int orderID)
        {
            using (var db = DbService.GetDb())
            {
                var orderDTO = db.Orders
                    .LoadWith(p => p.Car)
                    .LoadWith(p => p.Car.CarModel)
                    .LoadWith(p => p.Client)
                    .LoadWith(p => p.User)
                    .LoadWith(p => p.Worker)
                    .Where(p => p.ID == orderID)
                    .ToList()
                    .Select(p =>
                    {
                        return new OrderForClientReportOrderDTO()
                        {
                            InTime = p.InTime,
                            OutTime = p.OutTime,
                            LastCost = p.LastCost,
                            WorkerPay = p.WorkerPay.Value,
                            CarModel = p.Car.CarModel.Caption,
                            FedCode = p.Car.FedCode,
                            ClientCaption = p.Client?.Caption ?? "-",
                            ClientCard = p.Client?.Card ?? "-",
                            UserCaption = p.User.Caption,
                            WorkerCaption = p.Worker.Caption
                        };
                    })
                    .FirstOrDefault();

                orderDTO.Items = db.OrderItems
                    .LoadWith(p => p.Service)
                    .Where(p => p.OrderID == orderID)
                    .OrderBy(p => p.Service.Caption)
                    .ToList()
                    .Select(p => new OrderForClientReportOrderItemDTO()
                    {
                        ServiceCaption = p.Service.Caption,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Cost = p.Quantity * p.Price,
                        Discount = p.Discount,
                        LastPrice = p.LastPrice,                        
                        LastCost = p.LastCost
                    })
                    .ToList();

                var dbSettingsManager = new DbSettingManager(db);
                orderDTO.OrganizationPrintCaption = dbSettingsManager.OrganizationPrintCaption;
                orderDTO.OrderPrintCaption = dbSettingsManager.OrderPrintCaption;

                ShowReport(orderDTO);
            }
        }

        public void ShowReport(OrderForClientReportOrderDTO dto)
        {
            Report.SetParameterValue("InTime", dto.InTime);
            Report.SetParameterValue("OutTime", dto.OutTime);
            Report.SetParameterValue("CarModel", dto.CarModel);
            Report.SetParameterValue("FedCode", dto.FedCode);
            Report.SetParameterValue("ClientCaption", dto.ClientCaption);
            Report.SetParameterValue("ClientCard", dto.ClientCard);
            Report.SetParameterValue("Worker", dto.WorkerCaption);
            Report.SetParameterValue("User", dto.UserCaption);
            Report.SetParameterValue("OrganizationPrintCaption", dto.OrganizationPrintCaption);
            Report.SetParameterValue("OrderPrintCaption", dto.OrderPrintCaption);


            Report.RegisterData(dto.Items, "D1");
            var dbDataBand1 = Report.FindObject("Data1") as DataBand;
            dbDataBand1.DataSource = Report.GetDataSource("D1");

            ShowReport();
        }
    }
}
