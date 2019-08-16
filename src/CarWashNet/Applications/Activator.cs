using KLib.Native;
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
    public class Activator : ReactiveObject
    {
        string SecretPhase = "CarWashNet";

        [Reactive] public int MaxFreeOrderCount { get; private set; }
        [Reactive] public string SnPath { get; private set; }
        [Reactive] public int MachineID { get; private set; }
        [Reactive] public bool IsActivated { get; private set; }
        [Reactive] public bool IsUnlimOrders { get; private set; }
        [Reactive] public bool IsExtendedReports { get; private set; }

        public ReactiveList<SerialNumber> SerialNumbers { get; private set; }
        public ReactiveCommand<string, Unit> AddSerialNumber { get; set; }
        public ReactiveCommand<string, Unit> RemoveSerialNumber { get; set; }

        private Activator()
        {            
            SerialNumbers = new ReactiveList<SerialNumber>();

            AddSerialNumber = ReactiveCommand.Create<string>(sn =>
            {
                addSerialNumber(sn);
                Refresh();
            });
            RemoveSerialNumber = ReactiveCommand.Create<string>(sn =>
            {                
                removeSerialNumber(sn);
                Refresh();
            });
        }
        public static async Task<Activator> Create(int maxFreeOrderCount, string snPath)
        {
            var result = new Activator();
            result.MaxFreeOrderCount = maxFreeOrderCount;
            result.SnPath = snPath;
            await result.generateMachineID();
            result.Refresh();
            return result;
        }

        async Task<int> generateMachineID()
        {
            await Task.Run(() =>
            {
                MachineID = new SKGL.Generate().MachineCode;
            });
            return MachineID;
        }
        public void Refresh()
        {
            loadSerialNumbers();
            validateSerialNumbers();

            IsActivated = SerialNumbers.Any(p => p.IsValid);
            IsUnlimOrders = SerialNumbers.Any(p => p.IsUnlimOrders);
            IsExtendedReports = SerialNumbers.Any(p => p.IsExtendedReports);
        }
        void loadSerialNumbers()
        {
            try
            {
                var sn = File
                    .ReadLines(SnPath)
                    .Select(p => new SerialNumber(p))
                    .ToList();
                SerialNumbers.ReplaceRange(sn);
            }
            catch
            {
                SerialNumbers.Clear();
            }
        }
        void validateSerialNumbers()
        {
            if (SerialNumbers == null) return;
            SKGL.Validate ValidateAKey = new SKGL.Validate();
            ValidateAKey.secretPhase = SecretPhase;
            foreach (var sn in SerialNumbers)
            {
                ValidateAKey.Key = sn.Value;
                if (ValidateAKey.IsValid && ValidateAKey.IsOnRightMachine)
                {
                    sn.IsValid = true;
                    sn.IsUnlimOrders = ValidateAKey.Features[0];
                    sn.IsExtendedReports = ValidateAKey.Features[1];
                }
            }
        }
        void addSerialNumber(string value)
        {
            if (value.IsNullOrEmpty()) return;
            if (SerialNumbers.Any(p => p.Value == value)) return;
            SerialNumbers.Add(new SerialNumber(value));
            saveSerialNumbers();
        }
        void removeSerialNumber(string value)
        {
            var sn = SerialNumbers.FirstOrDefault(p => p.Value == value);
            if (sn == null) return;
            SerialNumbers.Remove(sn);
            saveSerialNumbers();
        }
        void saveSerialNumbers()
        {
            try
            {
                File.WriteAllLines(SnPath, SerialNumbers.Select(p => p.Value).ToArray());
            }
            catch
            {
                
            }
        }

    }

    public class SerialNumber : ReactiveObject
    {
        [Reactive] public string Value { get; set; }
        [Reactive] public bool IsValid { get; set; }
        [Reactive] public bool IsUnlimOrders { get; set; }
        [Reactive] public bool IsExtendedReports { get; set; }

        public SerialNumber(string value)
        {
            Value = value;
        }
    }
}
