using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using CarWashNet.Domain.Validation;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarWashNet.Domain.Managers
{
    public class AppItemManager : EntityManager<AppItem, AppItemValidator>
    {
        public AppItemManager(CarWashDb db) : base(db) { }

        public List<AppItem> GetUserApps(User user)
        {
            List<AppItem> result = _db.Apps
                .OnlyActive()
                .OrderBy(p => p.OrderNumber)
                .ToList();

            if (user.IsAdmin)
            {
                result.ForEach(p =>
                {
                    p.IsBinded = true;
                    p.CanBind = false;
                });
            }
            else
            {
                var bindings = _db.UserAppBindings.Where(p => p.UserID == user.ID).ToList();
                result.ForEach(p =>
                {
                    p.IsBinded = bindings.Any(q => q.AppID == p.ID);
                    p.CanBind = true;
                });
            }
            return result;
        }
        public void BindApp(AppItem app, User user, bool bind)
        {
            var binding = _db.UserAppBindings.FirstOrDefault(p => p.AppID == app.ID && p.UserID == user.ID);
            if (bind == true)
            {
                if (binding == null)
                {
                    binding = new UserAppBinding()
                    {
                        AppID = app.ID,
                        UserID = user.ID
                    };
                    _db.Insert(binding);
                }
            }
            else
            {
                if (binding != null)
                {
                    _db.Delete(binding);
                }
            }

        }

        //public List<AppItem> GetUserApps()
        //{
        //    var user = DbService.CurrentUser;
        //    List<AppItem> result = new List<AppItem>();

        //    if (user.IsAdmin)
        //    {
        //        result = _db.Apps
        //            .OnlyActive()
        //            .OrderBy(p => p.OrderNumber)
        //            .ToList();
        //    }
        //    else
        //    {
        //        var bindings = _db.UserAppBindings.Where(p => p.UserID == user.ID).ToList();
        //        result = _db.Apps
        //            .OnlyActive()
        //            .OrderBy(p => p.OrderNumber)
        //            .ToList();
        //        result.ForEach(p =>
        //        {
        //            p.IsBinded = bindings.Any(q => q.AppID == p.ID);
        //            p.CanBind = true;
        //        });
        //        result.RemoveAll(p => p.IsBinded == false);
        //    }
        //    return result;            
        //}
        //public List<AppItem> GetUserAppsForBinding(User user)
        //{
        //    List<AppItem> result = _db.Apps
        //        .OnlyActive()
        //        .OrderBy(p => p.OrderNumber)
        //        .ToList();

        //    if (user.IsAdmin)
        //    {
        //        result.ForEach(p =>
        //        {
        //            p.IsBinded = true;
        //            p.CanBind = false;
        //        });
        //    }
        //    else
        //    {
        //        var bindings = _db.UserAppBindings.Where(p => p.UserID == user.ID).ToList();                
        //        result.ForEach(p =>
        //        {
        //            p.IsBinded = bindings.Any(q => q.AppID == p.ID);
        //            p.CanBind = true;
        //        });
        //    }
        //    return result;
        //}
        
    }
    public class CarManager : EntityManager<Car, CarValidator>
    {
        public CarManager(CarWashDb db) : base(db) { }
        
    }
    public class CarModelManager : EntityManager<CarModel, CarModelValidator>
    {
        public CarModelManager(CarWashDb db) : base(db) { }
        
    }
    public class ClientManager : EntityManager<Client, ClientValidator>
    {
        public ClientManager(CarWashDb db) : base(db) { }
    }
    public class DbSettingManager
    {
        protected CarWashDb _db;
        public DbSettingManager(CarWashDb db)
        {
            _db = db;
        }

        string GetSettingValue(string name, string defaultValue)
        {
            var setting = _db.DbSettings.FirstOrDefault(p => p.Name == name);
            if (setting == null) throw new Exception($"Настройка {name} не найдена");            
            return setting.Value;
        }
        void SetSettingValue(string name, string value)
        {
            var setting = _db.DbSettings.FirstOrDefault(p => p.Name == name);
            if (setting == null) setting = new DbSetting(name, value);
            else setting.Value = value;
            _db.InsertOrReplace(setting);
        }

        public string DbCaption
        {
            get
            {
                string defaultValue = "БД";
                string dbValue;
                string xValue;

                try
                {
                    dbValue = GetSettingValue("DbCaption", defaultValue.ToString());
                    xValue = dbValue;
                }
                catch
                {
                    SetSettingValue("DbCaption", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("DbCaption", value.ToString());
            }
        }
        public double WorkerDayPercent
        {
            get
            {
                Double defaultValue = 0.30;                
                string dbValue;
                Double xValue;

                try
                {
                    dbValue = GetSettingValue("WorkerDayPercent", defaultValue.ToString());
                    xValue = Convert.ToDouble(dbValue);
                }
                catch
                {
                    SetSettingValue("WorkerDayPercent", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("WorkerDayPercent", value.ToString());
            }
        }
        public double WorkerNightPercent
        {
            get
            {
                Double defaultValue = 0.35;
                string dbValue;
                Double xValue;

                try
                {
                    dbValue = GetSettingValue("WorkerNightPercent", defaultValue.ToString());
                    xValue = Convert.ToDouble(dbValue);
                }
                catch
                {
                    SetSettingValue("WorkerNightPercent", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("WorkerNightPercent", value.ToString());
            }
        }
        public double WorkerDayBonus
        {
            get
            {
                Double defaultValue = 100;
                string dbValue;
                Double xValue;

                try
                {
                    dbValue = GetSettingValue("WorkerDayBonus", defaultValue.ToString());
                    xValue = Convert.ToDouble(dbValue);
                }
                catch
                {
                    SetSettingValue("WorkerDayBonus", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("WorkerDayBonus", value.ToString());
            }
        }
        public double WorkerNightBonus
        {
            get
            {
                Double defaultValue = 200;
                string dbValue;
                Double xValue;

                try
                {
                    dbValue = GetSettingValue("WorkerNightBonus", defaultValue.ToString());
                    xValue = Convert.ToDouble(dbValue);
                }
                catch
                {
                    SetSettingValue("WorkerNightBonus", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("WorkerNightBonus", value.ToString());
            }
        }
        public bool WorkerPayWithDiscount
        {
            get
            {
                bool defaultValue = false;
                string dbValue;
                bool xValue;

                try
                {
                    dbValue = GetSettingValue("WorkerPayWithDiscount", defaultValue.ToString());
                    xValue = Convert.ToBoolean(dbValue);
                }
                catch
                {
                    SetSettingValue("WorkerPayWithDiscount", defaultValue.ToString());
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("WorkerPayWithDiscount", value.ToString());
            }            
        }
        public string OrganizationPrintCaption
        {
            get
            {
                string defaultValue = "ООО Ромашка";
                string dbValue;
                string xValue;

                try
                {
                    dbValue = GetSettingValue("OrganizationPrintCaption", defaultValue);
                    xValue = dbValue;
                }
                catch
                {
                    SetSettingValue("OrganizationPrintCaption", defaultValue);
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("OrganizationPrintCaption", value);
            }
        }
        public string OrderPrintCaption
        {
            get
            {
                string defaultValue = "Выполненные работы";
                string dbValue;
                string xValue;

                try
                {
                    dbValue = GetSettingValue("OrderPrintCaption", defaultValue);
                    xValue = dbValue;
                }
                catch
                {
                    SetSettingValue("OrderPrintCaption", defaultValue);
                    xValue = defaultValue;
                }
                return xValue;
            }
            set
            {
                SetSettingValue("OrderPrintCaption", value);
            }
        }
    }
    public class DiscountlistManager : EntityManager<Discountlist, DiscountlistValidator>
    {
        public DiscountlistManager(CarWashDb db) : base(db) { }        
        public int Copy(Discountlist entity)
        {
            var src = _db.Discountlists.FirstOrDefault(p => p.ID == entity.ID);
            var srcitems = _db.DiscountlistItems.Where(p => p.DiscountlistID == entity.ID).ToList();
            var newDiscountlist = new Discountlist()
            {
                Caption = "Копия " + src.Caption,
                Group = src.Group,
                Date = DateTime.Today
            };

            _db.BeginTransaction();
            InsertToDb(newDiscountlist);
            srcitems.ForEach(p =>
            {
                p.DiscountlistID = newDiscountlist.ID;
            });
            _db.BulkCopy(srcitems);
            _db.CommitTransaction();

            return newDiscountlist.ID;
        }
        public void SaveItem(Discountlist discountlist, Service service, double? discount)
        {
            if (discount == null)
            {
                _db.DiscountlistItems
                    .Where(p => p.DiscountlistID == discountlist.ID)
                    .Where(p => p.ServiceID == service.ID)
                    .Delete();
            }
            else
            {
                var discountlistItem = _db.DiscountlistItems
                    .Where(p => p.DiscountlistID == discountlist.ID)
                    .FirstOrDefault(p => p.ServiceID == service.ID);
                if (discountlistItem == null)
                    discountlistItem = new DiscountlistItem()
                    {
                        DiscountlistID = discountlist.ID,
                        ServiceID = service.ID,
                        Discount = discount
                    };
                else discountlistItem.Discount = discount;
                InsertOrUpdateToDb(discountlistItem);
            }
        }
    }
    public class OrderManager : EntityManager<Order, OrderValidator>
    {
        public OrderManager(CarWashDb db) : base(db) { }

        public void ValidateAndSave(Order entity, List<OrderItem> items)
        {
            entity.ValidateAndThrow<OrderValidator, Order>(_db);
            base.Save(entity);
            SaveOrderItems(entity, items);
        }

        public void ValidateAndSetReadiness(Order order, bool isReady)
        {
            if (order.IsClosed == isReady) return;

            if (isReady)
            {
                order.ValidateAndThrow<OrderValidator, Order>("Close",_db);

                var dbSettingManager = new DbSettingManager(_db);
                double percent;
                if (order.IsNight) percent = dbSettingManager.WorkerNightPercent;
                else percent = dbSettingManager.WorkerDayPercent;

                var orderItems = GetOrderItems(order);
                double workerPay = 0;
                if (dbSettingManager.WorkerPayWithDiscount)
                    workerPay = orderItems.Sum(p => p.Quantity * p.LastPrice);
                else
                    workerPay = orderItems.Sum(p => p.Quantity * p.Price);

                workerPay = Math.Round(workerPay * percent, 2);

                _db.Orders
                    .Where(p => p.ID == order.ID)
                    .Set(p => p.CloseTime, p => DateTime.Now)
                    .Set(p => p.WorkerPay, p => workerPay)
                    .Update();
            }
            else
            {
                _db.Orders
                    .Where(p => p.ID == order.ID)
                    .Set(p => p.CloseTime, p => null)
                    .Set(p => p.WorkerPay, p => null)
                    .Update();
            }            
        }
        public void SetReadiness(IEnumerable<Order> orders, bool isReady)
        {
            if (isReady)
            {
                var dbSettingManager = new DbSettingManager(_db);
                double percent;
                foreach (var order in orders)
                {
                    if (order.IsClosed == isReady) continue;

                    if (order.Validate<OrderValidator, Order>("Close", _db).IsValid)
                    {
                        if (order.IsNight) percent = dbSettingManager.WorkerNightPercent;
                        else percent = dbSettingManager.WorkerDayPercent;

                        var orderItems = GetOrderItems(order);
                        double workerPay = 0;
                        if (dbSettingManager.WorkerPayWithDiscount)
                            workerPay = orderItems.Sum(p => p.Quantity * p.LastPrice);
                        else
                            workerPay = orderItems.Sum(p => p.Quantity * p.Price);

                        workerPay = Math.Round(workerPay * percent, 2);

                        _db.Orders
                            .Where(p => p.ID == order.ID)
                            .Set(p => p.CloseTime, p => DateTime.Now)
                            .Set(p => p.WorkerPay, p => workerPay)
                            .Update();
                    }
                }
            }
            else
            {
                foreach (var order in orders)
                {
                    if (order.IsClosed == isReady) continue;

                    _db.Orders
                        .Where(p => p.ID == order.ID)
                        .Set(p => p.CloseTime, p => null)
                        .Set(p => p.WorkerPay, p => null)
                        .Update();
                }
            }
        }
        public List<OrderItem> GetOrderItems(Order order)
        {
            return _db.OrderItems.Where(p => p.OrderID == order.ID).ToList();
        }

        public void SaveOrderItems(Order order, List<OrderItem> orderItems)
        {
            _db.OrderItems.Where(p => p.OrderID == order.ID).Delete();
            orderItems.ForEach(p => p.OrderID = order.ID);
            _db.BulkCopy(orderItems);
        }
        public List<Order> GetOrders(DateTime startDate, DateTime endDate, bool isClosed = true)
        {
            var query = _db.Orders.AsQueryable();
            if (isClosed) query = query.Where(p => p.CloseTime != null);
            else query = query.Where(p => p.CloseTime == null);

            query = query
                .Where(p => p.InTime >= startDate)
                .Where(p => p.InTime <= endDate);

            return query.ToList();
        }
    }
    public class PricelistManager : EntityManager<Pricelist, PricelistValidator>
    {
        public PricelistManager(CarWashDb db) : base(db) { }        
        public int Copy(Pricelist entity)
        {
            var src = _db.Pricelists.FirstOrDefault(p => p.ID == entity.ID);
            var srcitems = _db.PricelistItems.Where(p => p.PricelistID == entity.ID).ToList();
            var newpricelist = new Pricelist()
            {
                Caption = "Копия " + src.Caption,
                Group = src.Group,
                Date = DateTime.Today
            };

            _db.BeginTransaction();
            InsertToDb(newpricelist);
            srcitems.ForEach(p =>
            {
                p.PricelistID = newpricelist.ID;
            });
            _db.BulkCopy(srcitems);
            _db.CommitTransaction();

            return newpricelist.ID;
        }
        public void SaveItem(Pricelist pricelist, Service service, double? price)
        {
            if (price == null)
            {
                _db.PricelistItems
                    .Where(p => p.PricelistID == pricelist.ID)
                    .Where(p => p.ServiceID == service.ID)
                    .Delete();
            }
            else
            {
                var pricelistItem = _db.PricelistItems
                    .Where(p => p.PricelistID == pricelist.ID)
                    .FirstOrDefault(p => p.ServiceID == service.ID);
                if (pricelistItem == null)
                    pricelistItem = new PricelistItem()
                    {
                        PricelistID = pricelist.ID,
                        ServiceID = service.ID,
                        Price = price
                    };
                else pricelistItem.Price = price;
                InsertOrUpdateToDb(pricelistItem);
            }
        }
    }
    public class ServiceManager : EntityManager<Service, ServiceValidator>
    {
        public ServiceManager(CarWashDb db) : base(db) { }
    }
    public class UserManager : EntityManager<User, UserValidator>
    {
        public UserManager(CarWashDb db) : base(db) { }
    }
    public class WorkerManager : EntityManager<Worker, WorkerValidator>
    {
        public WorkerManager(CarWashDb db) : base(db) { }
    }
}
