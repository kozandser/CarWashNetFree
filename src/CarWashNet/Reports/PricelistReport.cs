using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Model;
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
    public class PricelistReport : ReportBase
    {
        public PricelistReport(string reportFile) : base(reportFile)
        {

        }

        public void ShowReport(int pricelistID)
        {
            using (var db = DbService.GetDb())
            {
                var pricelist = db.GetByID<Pricelist>(pricelistID);
                var services = db.PricelistItems
                    .LoadWith(p => p.Service)
                    .Where(p => p.PricelistID == pricelistID)
                    .Where(p => p.Service.EntityState == EntityStateEnum.Active)
                    .OrderBy(p => p.Service.Caption)
                    .ToList();

                var dbSettingsManager = new DbSettingManager(db);
                Report.SetParameterValue("OrganizationPrintCaption", dbSettingsManager.OrganizationPrintCaption);
                Report.SetParameterValue("PricelistCaption", pricelist.Caption);
                Report.SetParameterValue("PLDate", pricelist.Date.ToShortDateString());

                Report.RegisterData(services, "D1", FastReport.Data.BOConverterFlags.BrowsableOnly, 3);
                var dbDataBand1 = Report.FindObject("Data1") as DataBand;
                dbDataBand1.DataSource = Report.GetDataSource("D1");

                ShowReport();
            }
        }
    }
}
