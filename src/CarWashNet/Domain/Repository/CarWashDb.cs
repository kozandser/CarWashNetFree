using CarWashNet.Domain.Model;
using KLib.Native;
using LinqToDB;
using LinqToDB.DataProvider.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Domain.Repository
{
    public class CarWashDb : LinqToDB.Data.DataConnection
    {
        public CarWashDb(LinqToDB.DataProvider.IDataProvider provider, string connectionString) : base(provider, connectionString)
        {

        }

        public CarWashDb(string connectionString) : base(ProviderName.SQLiteClassic, connectionString)
        {

        }

        public async Task CheckDbConnectionAsync()
        {
            try
            {
                await EnsureConnectionAsync();
            }
            catch (Exception ex)
            {                
                throw new DbConnectionException("Нет прав для записи в БД, требуется запуск от имени администратора", ex);
            }
        }
        public async Task CheckDbAccessAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    BeginTransaction();
                    CommitTransaction();
                });
            }
            catch (Exception ex)
            {
                throw new DbCriticalException("Нет прав для записи в БД, требуется запуск от имени администратора", ex);
            }
        }
        public async Task CheckDbConsistencyAsync()
        {
            try
            {
                var x = await this.GetTable<DbPatch>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DbConsistencyException("БД повреждена", ex);
            }
        }
        public void CheckVersion(Version appVersion, Version minDbVersion)
        {
            var dbVer = DbPatches.ToList().OrderBy(p => p.XVersion).LastOrDefault();
            if (appVersion < dbVer.XMinimalAppVersion)
            {
                throw new DbCriticalException($"Версия программы ({appVersion}) меньше минимальной требуемой версии ({dbVer.MinimalAppVersion}). Обновите программу.");
            }
            if (minDbVersion > dbVer.XVersion)
            {
                throw new DbCriticalException($"Версия БД ({dbVer.Version}) меньше минимальной требуемой версии ({minDbVersion}). Обновите БД.");
            }
        }
        public T GetByID<T>(int id) where T : class, IEntity
        {
            return GetTable<T>().FirstOrDefault(p => p.ID == id);
        }

        #region Tables
        public ITable<DbPatch>          DbPatches => GetTable<DbPatch>();
        public ITable<DbSetting>        DbSettings => GetTable<DbSetting>();
        public ITable<User>             Users => GetTable<User>();
        public ITable<AppItem>          Apps => GetTable<AppItem>();
        public ITable<UserAppBinding>   UserAppBindings => GetTable<UserAppBinding>();
        public ITable<Worker>           Workers => GetTable<Worker>();
        public ITable<Service>          Services => GetTable<Service>();
        public ITable<Pricelist>        Pricelists => GetTable<Pricelist>();
        public ITable<PricelistItem>    PricelistItems => GetTable<PricelistItem>();
        public ITable<Discountlist>     Discountlists => GetTable<Discountlist>();
        public ITable<DiscountlistItem> DiscountlistItems => GetTable<DiscountlistItem>();
        public ITable<CarModel>         CarModels => GetTable<CarModel>();
        public ITable<Client>           Clients => GetTable<Client>();
        public ITable<Car>              Cars => GetTable<Car>();
        public ITable<Order>            Orders => GetTable<Order>();
        public ITable<OrderItem>        OrderItems => GetTable<OrderItem>();
        #endregion
    }
}
