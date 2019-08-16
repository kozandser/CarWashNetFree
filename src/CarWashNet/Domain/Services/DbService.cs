using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Infrastructure;
using KLib.Native;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CarWashNet.Domain.Services
{
    public static class DbService
    {
        public static DbSettings DbSettings { get; private set; }

        private static Lazy<CarWashDb> _defaultDb = new Lazy<CarWashDb>(() => GetDb());
        public static CarWashDb DefaultDb => _defaultDb.Value;

        private static bool _isInitialized = false;
        private static BaseDbConnectionSetting _currentDbConnection;
        private static LinqToDB.DataProvider.IDataProvider _currentDataProvider;
        private static string _currentConnectionString;

        public static Version MinimalDbVersion { get; private set; } = new Version("3.0.0");
        public static Version AppVersion { get; private set; } = new Version("3.0.0");

        public static User CurrentUser { get; private set; }
        public static string DbCaption { get; private set; }
        public static string DbVersion { get; private set; }
        public static string DbType { get; private set; }

        public static string CurrentDbPath => ((SQLiteConnectionSetting)_currentDbConnection).DataSource;

        public static void Init()
        {
            try
            {
                var json = ResourceHelper.GetEmbeddedResource("Infrastructure.DbSettings.json");
                DbSettings = json.DeserializeJson<DbSettings>();
                _currentDbConnection = DbSettings.GetCurrentConnection();
                _currentDataProvider = getDataProviderByDbConnectionType(_currentDbConnection);
                _currentConnectionString = _currentDbConnection.ConnectionString;
                DbType = _currentDbConnection.Type;
            }
            catch (Exception ex)
            {
                throw new DbCriticalException(ex.Message);
            }
            _isInitialized = true;
        }

        public static CarWashDb GetDb()
        {
            if (_isInitialized == false) throw new DbCriticalException("Не инициализирован DbService");
            return new CarWashDb(_currentDataProvider, _currentConnectionString);
        }
        public static DataContext GetDataContext()
        {
            if (_isInitialized == false) throw new DbCriticalException("Не инициализирован DbService");
            return new DataContext(_currentDataProvider, _currentConnectionString);

        }
        public static async Task<Version> ConnectToDbAsync()
        {
            return await ConnectToDbAsync(true, AppVersion, MinimalDbVersion);
        }
        public static async Task<Version> ConnectToDbAsync(bool checkVersions, Version appVersion, Version minDbVersion)
        {
            using (var db = GetDb())
            {
                try
                {
                    await db.CheckDbConnectionAsync();
                    await db.CheckDbAccessAsync();
                    await db.CheckDbConsistencyAsync();
                    if (checkVersions) db.CheckVersion(appVersion, minDbVersion);
                }
                catch (DbConnectionException)
                {
                    throw;
                }
                catch (DbConsistencyException)
                {
                    await InitializeDbAsync();
                }
                catch (DbCriticalException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }

                var ver = db.DbPatches.ToList().OrderBy(p => p.XVersion).LastOrDefault().XVersion;
                DbVersion = ver.ToString();
                var manager = new DbSettingManager(db);
                DbCaption = manager.DbCaption;

                return ver;
            }
        }
        
        public static void SetCurrentUser(int userId, string password)
        {
            using (var db = GetDb())
            {
                var user = db.Users.FirstOrDefault(p => p.ID == userId);
                if (user == null) throw new BusinessLogicException($"Не найден пользователь с Уч№ {userId}");
                else if(user.EntityState != EntityStateEnum.Active) throw new BusinessLogicException($"Пользователь с Уч№ {userId} неактивен");
                else
                {
                    string decryptedPassword;
                    try
                    {
                        decryptedPassword = user.Password.Decrypt();
                    }
                    catch
                    {
                        throw new Exception($"Ошибка дешифрации пароля");
                    }
                    if (decryptedPassword != password)
                    {
                        throw new BusinessLogicException($"Неверный пароль");
                    }
                    CurrentUser = user;
                }
            }
        }
        public static void ResetCurrentUser()
        {
            CurrentUser = null;
        }

        public static async Task InitializeDbAsync()
        {
            await createTablesAsync();
            await loadInitDataAsync();
        }
        public static async Task InitializeDbWithTestDataAsync()
        {
            await purgeDbAsync();
            await createTablesAsync();
            await loadTestDataAsync();
        }
        public static async Task ReInitializeDbAsync()
        {
            await purgeDbAsync();
            await InitializeDbAsync();
        }

        private static LinqToDB.DataProvider.IDataProvider getDataProviderByDbConnectionType(BaseDbConnectionSetting dbConnection)
        {
            var type = dbConnection.GetType();
            if (type == typeof(SQLiteConnectionSetting)) return new LinqToDB.DataProvider.SQLite.SQLiteDataProvider(LinqToDB.ProviderName.SQLiteClassic);
            else if (type == typeof(PostgreSQLConnectionSetting)) return new LinqToDB.DataProvider.PostgreSQL.PostgreSQLDataProvider();
            else throw new Exception($"Неизвестный тип подключения {type}");
        }

        private static void tryCreateTable<T>(CarWashDb db) where T : class
        {
            try
            {
                db.CreateTable<T>();
            }
            catch
            {

            }
        }
        private static void tryDropTable<T>(CarWashDb db) where T : class
        {
            try
            {
                db.DropTable<T>();
            }
            catch
            {

            }
        }
        private static async Task purgeDbAsync()
        {
            using (var db = GetDb())
            {
                await Task.Run(() =>
                {
                    tryDropTable<DbPatch>(db);
                    tryDropTable<DbSetting>(db);
                    tryDropTable<User>(db);
                    tryDropTable<AppItem>(db);
                    tryDropTable<UserAppBinding>(db);
                    tryDropTable<Worker>(db);
                    tryDropTable<Service>(db);
                    tryDropTable<Pricelist>(db);
                    tryDropTable<PricelistItem>(db);
                    tryDropTable<Discountlist>(db);
                    tryDropTable<DiscountlistItem>(db);
                    tryDropTable<CarModel>(db);
                    tryDropTable<Client>(db);
                    tryDropTable<Car>(db);
                    tryDropTable<Order>(db);
                    tryDropTable<OrderItem>(db);
                });
            }
        }
        private static async Task createTablesAsync()
        {
            using (var db = GetDb())
            {
                await Task.Run(() =>
                {
                    tryCreateTable<DbPatch>(db);
                    tryCreateTable<DbSetting>(db);
                    tryCreateTable<User>(db);
                    tryCreateTable<AppItem>(db);
                    tryCreateTable<UserAppBinding>(db);
                    tryCreateTable<Worker>(db);
                    tryCreateTable<Service>(db);
                    tryCreateTable<Pricelist>(db);
                    tryCreateTable<PricelistItem>(db);
                    tryCreateTable<Discountlist>(db);
                    tryCreateTable<DiscountlistItem>(db);
                    tryCreateTable<CarModel>(db);
                    tryCreateTable<Client>(db);
                    tryCreateTable<Car>(db);
                    tryCreateTable<Order>(db);
                    tryCreateTable<OrderItem>(db);
                });
            }
        }
        private static async Task loadInitDataAsync()
        {
            var initDb = DbSettings.GetConnection("init");

            await Task.Run(() =>
            {
                using (var initdb = new CarWashDb(initDb.ConnectionString))
                {
                    using (var db = GetDb())
                    {
                        var x = initdb.DbPatches.ToList();

                        db.BeginTransaction();
                        db.BulkCopy(initdb.DbPatches.ToList());
                        db.BulkCopy(initdb.DbSettings.ToList());
                        db.BulkCopy(initdb.Users.ToList());
                        db.BulkCopy(initdb.Apps.ToList());
                        db.CommitTransaction();
                    }
                }
            });
        }
        private static async Task loadTestDataAsync()
        {
            await loadInitDataAsync();
            var initDb = DbSettings.GetConnection("init");
            await Task.Run(() =>
            {
                using (var initdb = new CarWashDb(initDb.ConnectionString))
                {
                    using (var db = GetDb())
                    {
                        db.BeginTransaction();
                        db.BulkCopy(initdb.Workers.ToList());
                        db.BulkCopy(initdb.Services.ToList());
                        db.BulkCopy(initdb.Pricelists.ToList());
                        db.BulkCopy(initdb.PricelistItems.ToList());
                        db.BulkCopy(initdb.Discountlists.ToList());
                        db.BulkCopy(initdb.DiscountlistItems.ToList());
                        db.BulkCopy(initdb.Clients.ToList());
                        db.BulkCopy(initdb.CarModels.ToList());
                        db.BulkCopy(initdb.Cars.ToList());
                        db.CommitTransaction();
                    }
                }
            });
        }
























        //private static XDocument getXmlData(string resourceName)
        //{
        //    XDocument xdoc;
        //    var assembly = Assembly.GetExecutingAssembly();
        //    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        //    {
        //        xdoc = XDocument.Load(stream);
        //    }
        //    return xdoc;
        //}

        //private static void tryCreateTable<T>(CarWashDb db) where T : class
        //{
        //    try
        //    {
        //        db.CreateTable<T>();
        //    }
        //    catch
        //    {

        //    }
        //}
        //private static void tryDropTable<T>(CarWashDb db) where T : class
        //{
        //    try
        //    {
        //        db.DropTable<T>();
        //    }
        //    catch
        //    {

        //    }
        //}
        //private static async Task purgeDbAsync()
        //{
        //    using (var db = GetDb())
        //    {
        //        await Task.Run(() =>
        //        {
        //            tryDropTable<DbPatch>(db);
        //            tryDropTable<DbPatch>(db);
        //            tryDropTable<DbSetting>(db);
        //            tryDropTable<User>(db);
        //            tryDropTable<AppItem>(db);
        //            tryDropTable<UserAppBinding>(db);
        //            tryDropTable<Worker>(db);
        //            tryDropTable<Service>(db);
        //            tryDropTable<Pricelist>(db);
        //            tryDropTable<PricelistItem>(db);
        //            tryDropTable<Discountlist>(db);
        //            tryDropTable<DiscountlistItem>(db);
        //            tryDropTable<CarModel>(db);
        //            tryDropTable<Client>(db);
        //            tryDropTable<Car>(db);
        //            tryDropTable<Order>(db);
        //            tryDropTable<OrderItem>(db);
        //        });               
        //    }
        //}
        //private static async Task createTablesAsync()
        //{
        //    using (var db = GetDb())
        //    {
        //        await Task.Run(() =>
        //        {
        //            tryCreateTable<DbPatch>(db);
        //            tryCreateTable<DbPatch>(db);
        //            tryCreateTable<DbSetting>(db);
        //            tryCreateTable<User>(db);
        //            tryCreateTable<AppItem>(db);
        //            tryCreateTable<UserAppBinding>(db);
        //            tryCreateTable<Worker>(db);
        //            tryCreateTable<Service>(db);
        //            tryCreateTable<Pricelist>(db);
        //            tryCreateTable<PricelistItem>(db);
        //            tryCreateTable<Discountlist>(db);
        //            tryCreateTable<DiscountlistItem>(db);
        //            tryCreateTable<CarModel>(db);
        //            tryCreateTable<Client>(db);
        //            tryCreateTable<Car>(db);
        //            tryCreateTable<Order>(db);
        //            tryCreateTable<OrderItem>(db);
        //        });
        //    }
        //}
        //private static async Task loadInitDataAsync()
        //{
        //    await Task.Run(() =>
        //    {
        //        using (var testdb = new CarWashDb("Data Source=db.test"))
        //        {
        //            using (var db = GetDb())
        //            {
        //                db.BeginTransaction();
        //                db.BulkCopy(testdb.DbPatches.ToList());
        //                db.BulkCopy(testdb.DbSettings.ToList());
        //                db.BulkCopy(testdb.Users.ToList());
        //                db.BulkCopy(testdb.Apps.ToList());
        //                db.CommitTransaction();
        //            }
        //        }
        //    });
        //}
        //private static async Task loadTestDataAsync()
        //{
        //    await loadInitDataAsync();
        //    await Task.Run(() =>
        //    {
        //        using (var testdb = new CarWashDb("Data Source=db.test"))
        //        {
        //            using (var db = GetDb())
        //            {
        //                db.BeginTransaction();
        //                db.BulkCopy(testdb.Workers.ToList());
        //                db.BulkCopy(testdb.Services.ToList());
        //                db.BulkCopy(testdb.Pricelists.ToList());
        //                db.BulkCopy(testdb.PricelistItems.ToList());
        //                db.BulkCopy(testdb.Discountlists.ToList());
        //                db.BulkCopy(testdb.DiscountlistItems.ToList());
        //                db.BulkCopy(testdb.Clients.ToList());
        //                db.BulkCopy(testdb.CarModels.ToList());
        //                db.BulkCopy(testdb.Cars.ToList());
        //                db.CommitTransaction();
        //            }
        //        }
        //    });
        //}

        //private static async Task tryCreateTableAsync<T>(CarWashDb db) where T : class
        //{
        //    try
        //    {
        //        await db.CreateTableAsync<T>();                
        //    }
        //    catch
        //    {

        //    }            
        //}
        //private static async Task tryDropTableAsync<T>(CarWashDb db) where T : class
        //{
        //    try
        //    {
        //        await db.DropTableAsync<T>();
        //    }
        //    catch
        //    {

        //    }
        //}
        //private static async Task purgeDbAsync2()
        //{
        //    using (var db = GetDb())
        //    {
        //        await tryDropTableAsync<DbPatch>(db);
        //        await tryDropTableAsync<DbPatch>(db);
        //        await tryDropTableAsync<DbSetting>(db);
        //        await tryDropTableAsync<User>(db);
        //        await tryDropTableAsync<AppItem>(db);
        //        await tryDropTableAsync<UserAppBinding>(db);
        //        await tryDropTableAsync<Worker>(db);
        //        await tryDropTableAsync<Service>(db);
        //        await tryDropTableAsync<Pricelist>(db);
        //        await tryDropTableAsync<PricelistItem>(db);
        //        await tryDropTableAsync<Discountlist>(db);
        //        await tryDropTableAsync<DiscountlistItem>(db);
        //        await tryDropTableAsync<CarModel>(db);
        //        await tryDropTableAsync<Client>(db);
        //        await tryDropTableAsync<Car>(db);
        //        await tryDropTableAsync<Order>(db);
        //        await tryDropTableAsync<OrderItem>(db);
        //    }
        //}
        //private static async Task createTablesAsync2()
        //{
        //    using (var db = GetDb())
        //    {
        //        await tryCreateTableAsync<DbPatch>(db);
        //        await tryCreateTableAsync<DbSetting>(db);
        //        await tryCreateTableAsync<User>(db);
        //        await tryCreateTableAsync<AppItem>(db);
        //        await tryCreateTableAsync<UserAppBinding>(db);
        //        await tryCreateTableAsync<Worker>(db);
        //        await tryCreateTableAsync<Service>(db);
        //        await tryCreateTableAsync<Pricelist>(db);
        //        await tryCreateTableAsync<PricelistItem>(db);
        //        await tryCreateTableAsync<Discountlist>(db);
        //        await tryCreateTableAsync<DiscountlistItem>(db);
        //        await tryCreateTableAsync<CarModel>(db);
        //        await tryCreateTableAsync<Client>(db);
        //        await tryCreateTableAsync<Car>(db);
        //        await tryCreateTableAsync<Order>(db);
        //        await tryCreateTableAsync<OrderItem>(db);
        //    }
        //}
        


        //private static async Task loadInitData(XDocument xdoc)
        //{
        //    await Task.Run(() =>
        //    {
        //        using (var db = GetDb())
        //        {
        //            var entityManager = new EntityManager(db);
        //            db.BeginTransaction();

        //            if (xdoc.Element("InitData").Element("DbPatches") != null)
        //            {
        //                foreach (XElement el in xdoc.Element("InitData").Element("DbPatches")?.Elements("DbPatch"))
        //                {
        //                    entityManager.Save(
        //                        new DbPatch()
        //                        {
        //                            Version = el.Element("Version").Value,
        //                            Date = DateTime.ParseExact(el.Element("Date").Value, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture),
        //                            MinimalAppVersion = el.Element("MinimalAppVersion").Value,
        //                            PatchDate = DateTime.ParseExact(el.Element("PatchDate").Value, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture),
        //                            Description = el.Element("Description").Value
        //                        });
        //                }
        //            }

                    
        //            foreach (XElement el in xdoc.Element("InitData").Element("DbSettings")?.Elements("DbSetting"))
        //            {
        //                db.Insert(
        //                    new DbSetting()
        //                    {
        //                        Name = el.Element("Name").Value,
        //                        Value = el.Element("Value").Value
        //                    });
        //            }
        //            foreach (XElement el in xdoc.Element("InitData").Element("Users").Elements("User"))
        //            {
        //                entityManager.Save(
        //                    new User()
        //                    {
        //                        EntityState = ((EntityStateEnum)(int)el.Element("EntityState")),
        //                        Caption = el.Element("Caption").Value,
        //                        Password = el.Element("Password").Value.Encrypt(),
        //                        IsAdmin = ((bool)el.Element("IsAdmin")),
        //                    });
        //            }
        //            foreach (XElement el in xdoc.Element("InitData").Element("Apps").Elements("App"))
        //            {
        //                entityManager.Save(
        //                    new AppItem()
        //                    {
        //                        EntityState = ((EntityStateEnum)(int)el.Element("EntityState")),
        //                        OrderNumber = ((int)el.Element("OrderNumber")),
        //                        Caption = el.Element("Caption").Value,
        //                        Code = el.Element("Code").Value,
        //                        AppGroup = el.Element("AppGroup").Value,
        //                        Note = el.Element("Note").Value
        //                    });
        //            }

        //            if (xdoc.Element("InitData").Element("Services") != null)
        //            {
        //                foreach (XElement el in xdoc.Element("InitData").Element("Services")?.Elements("Service"))
        //                {
        //                    entityManager.Save(
        //                        new Service()
        //                        {
        //                            EntityState = ((EntityStateEnum)(int)el.Element("EntityState")),
        //                            Caption = el.Element("Caption").Value,
        //                            Note = el.Element("Note").Value
        //                        });
        //                }
        //            }
        //            if (xdoc.Element("InitData").Element("Pricelists") != null)
        //            {
        //                foreach (XElement el in xdoc.Element("InitData").Element("Pricelists")?.Elements("Pricelist"))
        //                {
        //                    entityManager.Save(
        //                        new Pricelist()
        //                        {
        //                            EntityState = ((EntityStateEnum)(int)el.Element("EntityState")),
        //                            Caption = el.Element("Caption").Value,
        //                            Date = DateTime.ParseExact(el.Element("Date").Value, "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture),
        //                            Note = el.Element("Note").Value
        //                        });
        //                }
        //            }

                        
        //            db.CommitTransaction();
        //        }
        //    });
        //}
    }


}
