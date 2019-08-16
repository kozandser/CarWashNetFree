using KLib.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace CarWashNet.Infrastructure
{
    public class AppSettings
    {
        public bool HasSavedUserPassword { get; set; } = false;
        public string LastUserPassword { get; set; } = String.Empty;
        public int LastUserID { get; set; } = 1;
        public string LastAppCode { get; set; } = "LX55001";
        public bool IsMenuOpened { get; set; } = false;
        public bool IsMenuOverlay { get; set; } = true;
        public bool ShowPreparing { get; set; } = true;
        public bool ShowUnusing { get; set; } = true;
        public DateTime? LastBackupTime { get; set; }
        public DateTime? LastUpdateCheckDate { get; set; }

        public string DecryptLastUserPassword()
        {
            return LastUserPassword.Decrypt("jdyrkgistelaefyrnfordbrt");
        }
        public void EncryptLastUserPassword(string password)
        {
            LastUserPassword = password.Encrypt("jdyrkgistelaefyrnfordbrt");
        }
    }

    public abstract class BaseDbConnectionSetting
    {      
        public abstract string Type { get; }
        public string Name { get; set; }
        public abstract string ConnectionString { get; }    
    }
    public class SQLiteConnectionSetting : BaseDbConnectionSetting
    {
        public override string Type => "SQLite";
        public string DataSource { get; set; } //"db.sqlite";
        public string ConnectionUser { get; set; }
        public string ConnectionUserPassword { get; set; }

        public override string ConnectionString => $"Data Source={DataSource};";
    }
    public class PostgreSQLConnectionSetting : BaseDbConnectionSetting
    {
        public override string Type => "PostgreSQL";
        public string Server { get; set; } //"192.168.1.70";
        public string Port { get; set; }  //"5432";
        public string Database { get; set; } //"ProcessesDB";
        public string ConnectionUser { get; set; } //"postgres";
        public string ConnectionUserPassword { get; set; } //"8EmCHRLSw7s=";

        public override string ConnectionString =>
            $"Server={Server};" +
            $"Port={Port};" +
            $"Database={Database};" +
            $"User Id={ConnectionUser};" +
            $"Password={ConnectionUserPassword.Decrypt()};";
    }
    public class DbSettings
    {
        public string InitDbFileName { get; set; }
        public string CurrentConnection { get; set; }
        public List<BaseDbConnectionSetting> Connections { get; set; }

        public BaseDbConnectionSetting GetConnection(string name)
        {
            if (Connections == null) return null;
            return Connections.FirstOrDefault(p => p.Name == name);
        }
        public BaseDbConnectionSetting GetCurrentConnection()
        {
            return GetConnection(CurrentConnection);
        }
    }


    public static class SettingsService
    {
        static IKLibSerializer _serializer;
        static string _defaultSettingsPath;
        public static void Init(IKLibSerializer serializer, string defaultSettingsPath)
        {
            _serializer = serializer;
            _defaultSettingsPath = defaultSettingsPath;
        }
        public static T Load<T>(IKLibSerializer serializer, string settingsPath)
        {
            T result;
            try
            {
                result = serializer.Deserialize<T>(settingsPath.LoadStringFromFile(false));
            }
            catch (Exception ex)
            {
                var error = $"Проблема с файлом настроек {settingsPath} {Environment.NewLine} {ex.Message}";
                throw new Exception(error);
            }
            return result;
        }
        public static T Load<T>()
        {
            if (_serializer == null) throw new InvalidOperationException("Не назначен сериализатор по-умолчанию");
            return Load<T>(_serializer, _defaultSettingsPath);
        }
        public static void Save<T>(this T obj, IKLibSerializer serializer, string settingsPath)
        {
            try
            {
                serializer.Serialize(obj).SaveStringToFile(settingsPath, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void Save<T>(this T obj)
        {
            if (_serializer == null) throw new InvalidOperationException("Не назначен сериализатор по-умолчанию");
            Save(obj, _serializer, _defaultSettingsPath);
        }
    }
}
