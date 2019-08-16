using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CarWashNet.Apps
{
    public static class AppRepository
    {
        static Dictionary<string, ContentControl> apps = new Dictionary<string, ContentControl>();

        public static void Reset()
        {
            apps.Clear();
        }

        public static ContentControl GetApp(string code, string title)
        {
            if(apps.ContainsKey(code) == false)
            {
                switch (code)
                {
                    case "A001":
                        apps[code] = new A001(title);
                        break;
                    case "A002":
                        apps[code] = new A002(title);                        
                        break;
                    case "A003":
                        apps[code] = new A003(title);
                        break;
                    case "A004":
                        apps[code] = new A004(title);
                        break;
                    case "A005":
                        apps[code] = new A005(title);
                        break;
                    case "A006":
                        apps[code] = new A006(title);
                        break;
                    case "A008":
                        apps[code] = new A008(title);
                        break;
                    case "A009":
                        apps[code] = new A009(title);
                        break;
                    case "A010":
                        apps[code] = new A010(title);
                        break;
                    case "A011":
                        apps[code] = new A011(title);
                        break;
                    case "A012":
                        apps[code] = new A012(title);
                        break;
                    case "AppSettings":
                        apps[code] = new XAppSettings(title);
                        break;
                    default:
                        return null;
                }
            }
            return apps[code];
        }


    }
}
