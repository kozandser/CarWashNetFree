using CarWashNet.Applications;
using CarWashNet.Domain.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace CarWashNet.ViewModel
{
    public class AppSettingsViewModel : ReactiveObject
    {
        [Reactive] public string AppVersion { get; set; }
        [Reactive] public string DbVersion { get; set; }

        [Reactive] public Activator Activator { get; set; }
        [Reactive] public Backuper Backuper { get; set; }
        [Reactive] public Updater Updater { get; set; }
        [Reactive] public Feedbacker Feedbacker { get; set; }

        [Reactive] public string NewSerialNumber { get; set; }
        [Reactive] public SerialNumber SelectedSerialNumber { get; set; }


        public AppSettingsViewModel()
        {
            AppVersion = GlobalService.AppVersion.ToString();
            DbVersion = DbService.DbVersion;

            Activator = GlobalService.Activator;
            Backuper = GlobalService.Backuper;
            Updater = GlobalService.Updater;
            Feedbacker = GlobalService.Feedbacker;
        }
    }
}
