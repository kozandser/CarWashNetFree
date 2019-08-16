using CarWashNet.Domain.Services;
using CarWashNet.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarWashNet.Applications
{
    public static class GlobalService
    {
        private static string _settingsPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static string _backupPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, @"Backups\db.backup");
        private static string _snPath = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "app.sn");
        private static string _updateURL = @"https://raw.githubusercontent.com/kozandser/CarWashNet/master/Versions.xml";

        public static Version MinimalDbVersion { get; set; } = new Version("3.0.0");
        public static Version AppVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        public static Activator Activator { get; private set; }
        public static AppSettings AppSettings { get; private set; }
        public static Backuper Backuper { get; private set; }
        public static Updater Updater { get; private set; }
        public static Feedbacker Feedbacker { get; private set; }

        public static async Task InitAsync()
        {
            Activator = await Activator.Create(500, _snPath);
            SettingsService.Init(new JsonKLibSerializer(), _settingsPath);
            AppSettings = SettingsService.Load<AppSettings>();
            Backuper = Backuper.Create(DbService.CurrentDbPath, _backupPath);

            Updater = Updater.Create(_updateURL);
            Updater.CheckUpdate.Execute().Subscribe();

            Feedbacker = new Feedbacker(Activator.MachineID.ToString());
        }

        public static void SaveSettings()
        {
            SettingsService.Save(AppSettings);
        }

    }
}
