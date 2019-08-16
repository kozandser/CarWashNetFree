using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Applications
{
    public class Backuper : ReactiveObject
    {
        [Reactive] public string DbPath { get; private set; }
        [Reactive] public string BackupPath { get; private set; }
        [Reactive] public DateTime? LastBackupTime { get; set; }

        public ReactiveCommand<Unit, Unit> BackupDb { get; set; }

        private Backuper()
        {
            BackupDb = ReactiveCommand.Create(() =>
            {
                System.IO.Directory.CreateDirectory("Backups");
                File.Copy(DbPath, BackupPath, true);
                GetLastBackupTime();
            });

        }

        public static Backuper Create(string dbPath, string backupPath)
        {
            var result = new Backuper();
            result.DbPath = dbPath;
            result.BackupPath = backupPath;
            result.GetLastBackupTime();
            return result;
        }

        void GetLastBackupTime()
        {
            try
            {
                System.IO.FileInfo file1 = new System.IO.FileInfo(BackupPath);
                if (file1.Exists) LastBackupTime = file1.LastWriteTime;
                else LastBackupTime = null;
            }
            catch
            {
                LastBackupTime = null;
            }
        }
    }
}
