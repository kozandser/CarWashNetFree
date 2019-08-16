using KLib.Native.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Domain.Services
{
    [Serializable]
    public class DbSettings : BaseDbSettings
    {
        public string InitDbFileName { get; set; }

    }
}
