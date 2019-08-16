using KLib.Native.Serialization;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Applications
{
    public class Updater : ReactiveObject
    {
        public string UpdateURL { get; private set; }
        public UpdateCheckPeriod UpdateCheckPeriod { get; private set; } = UpdateCheckPeriod.Monthly;

        [Reactive] public UpdaterData UpdaterData { get; private set; }        
        [Reactive] public DateTime? LastUpdateCheckDate { get; set; }
        [Reactive] public bool HasNewVersion { get; set; } = false;

        public ReactiveCommand<Unit, UpdateCheckResult> CheckUpdate { get; set; }
        public ReactiveCommand<Unit, Unit> OpenLinkInBrowser { get; set; }

        private Updater()
        {
            CheckUpdate = ReactiveCommand.CreateFromTask(async () =>
            {
                return await CheckUpdateAsync();
            });
            OpenLinkInBrowser = ReactiveCommand.Create(() =>
            {
                Process.Start(new ProcessStartInfo(UpdaterData.Link));
            });
        }

        public static Updater Create(string updateURL)
        {
            var result = new Updater();
            result.UpdateURL = updateURL; 
            return result;
        }

        async Task<string> GetUpdaterDataXMLAsync()
        {
            string xml = string.Empty; ;
            using (var client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                xml = await client.DownloadStringTaskAsync(UpdateURL);
            }
            return xml;
        }
        async Task<UpdateCheckResult> CheckUpdateAsync()
        {
            string xml;
            try
            {
                xml = await GetUpdaterDataXMLAsync();
            }
            catch (Exception ex)
            {
                var x = ex;
                return UpdateCheckResult.Error;
            }

            UpdaterData = xml.DeserializeXml<UpdaterData>();
            if (UpdaterData == null) return UpdateCheckResult.Error;
            else
            {
                LastUpdateCheckDate = DateTime.Now;
                Version v1 = GlobalService.AppVersion;
                Version v2 = new Version(UpdaterData.LastVersion);
                if (v2 > v1)
                {
                    HasNewVersion = true;
                }
                else
                {
                    HasNewVersion = false;
                }                
            }

            if (HasNewVersion) return UpdateCheckResult.HasUpdate;
            else return UpdateCheckResult.NoUpdate;
        }
    }


    [Serializable]
    public class UpdaterData
    {
        public string LastVersion { get; set; } = "1.0.0";
        public DateTime ReleaseDate { get; set; } = DateTime.Now;
        public string Link { get; set; } = @"https://carwashnet.wordpress.com/download/";
        public string Description { get; set; } = "ываор цукажруфк фукп ";        
    }
    public enum UpdateCheckPeriod
    {
        Never,
        Daily,
        Weekly,
        Monthly
    }
    public enum UpdateCheckResult
    {
        Error,
        HasUpdate,
        NoUpdate
    }
}
