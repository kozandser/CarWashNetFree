using FastReport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KLib.Native;

namespace CarWashNet.Reports
{
    public abstract class ReportBase
    {
        public Report Report;
        //private Stream _stream;
        //private Assembly _assembly;
        //private Stream _loc;
        //private string _reportfile;
        private string basePath = "Reports/ReportResources/";

        public static void LoadLocale(string resourceName, Assembly assembly)
        {
            resourceName = assembly.FormatResourceName(resourceName);
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null) throw new Exception($"Не найден файл локализации отчета {resourceName}");
                FastReport.Utils.Res.LoadLocale(resourceStream);
            }
        }
        public static void LoadLocale(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            LoadLocale(resourceName, assembly);
        }

        public ReportBase(string reportFile)
        {
            LoadLocale(basePath + "Russian.frl");
            Report = new Report();
            loadReport(Report, reportFile);
        }
        public void ShowReport()
        {
            if (Report == null) return;
            Report.Show();
        }

        private void loadReport(Report report, string resourceName, Assembly assembly)
        {
            resourceName = assembly.FormatResourceName(resourceName);
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null) throw new Exception($"Не найден файл отчета {resourceName}");
                report.Load(resourceStream);
            }
        }
        private void loadReport(Report report, string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            loadReport(report, resourceName, assembly);
        }
        

    }
}
