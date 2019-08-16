using LinqToDB.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLib.Native;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reactive.Linq;

namespace CarWashNet.Domain.Model
{
    [Table]
    public class DbPatch : Entity
    {
        [Column] public string Version { get; set; }
        [Column] public DateTime Date { get; set; }
        [Column] public string MinimalAppVersion { get; set; }
        [Column] public DateTime PatchDate { get; set; }
        [Column] public string Description { get; set; }

        public Version XVersion => new Version(Version);
        public Version XMinimalAppVersion => new Version(MinimalAppVersion);
    }
    [Table]
    public class DbSetting
    {
        [Column, NotNull, PrimaryKey] public string Name { get; set; }
        [Column] public string Value { get; set; }

        public DbSetting() { }

        public DbSetting(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
    [Table]
    public class User : EntityWithState
    {        
        [Column] public string Caption { get; set; }
        [Column] public string Password { get; set; }
        [Column] public bool IsAdmin { get; set; }

        [Reactive] public string DecryptedPassword { get; set; }

        public void DecryptPassword()
        {
            DecryptedPassword = Password.Decrypt();
        }
        public void EncryptPassword()
        {
            Password = DecryptedPassword.Encrypt();
        }
    }
    [Table]
    public class AppItem : EntityWithState
    {
        [Column] public int OrderNumber { get; set; }
        [Column] public string Caption { get; set; }
        [Column] public string Code { get; set; }
        [Column] public string AppGroup { get; set; }
        [Column] public string Note { get; set; }

        [Reactive] public UserAppBinding AppBinding { get; set; }
        [Reactive] public bool IsBinded { get; set; }
        [Reactive] public bool CanBind { get; set; } = true;
    }
    [Table]
    public class UserAppBinding
    {
        [Column] public int UserID { get; set; }
        [Column] public int AppID { get; set; }

        [Association(ThisKey = "AppID", OtherKey = "ID")] public AppItem App { get; set; }
    }
    [Table]
    public class Worker : EntityWithState
    {
        [Column]public string Caption { get; set; }
        [Column]public string Note { get; set; }
    }
    [Table]
    public class Service : EntityWithState, IEntityWithGroup
    {
        [Column] public string Caption { get; set; }
        [Column] public string Group { get; set; }
        [Column] public string Note { get; set; }
        [Column] public int OrderNumber { get; set; }
    }
    [Table]
    public class Pricelist : EntityWithState, IEntityWithGroup
    {
        [Column] public string Caption { get; set; }
        [Column] public string Group { get; set; }
        [Column] public DateTime Date { get; set; } = DateTime.Today;
        [Column] public string Note { get; set; }
    }
    [Table]
    public class PricelistItem : EntityWithState
    {
        [Column] public int PricelistID { get; set; }
        [Column] public int ServiceID { get; set; }
        [Column] public double? Price { get; set; }

        [Association(ThisKey = "ServiceID", OtherKey = "ID")] public Service Service { get; set; }
        [Association(ThisKey = "PricelistID", OtherKey = "ID")] public Pricelist Pricelist { get; set; }
    }
    [Table]
    public class Discountlist : EntityWithState, IEntityWithGroup
    {
        [Column] public string Caption { get; set; }
        [Column] public string Group { get; set; }
        [Column] public DateTime Date { get; set; } = DateTime.Today;
        [Column] public string Note { get; set; }
    }
 [Table]
    public class DiscountlistItem : EntityWithState
    {
        [Column] public int DiscountlistID { get; set; }
        [Column] public int ServiceID { get; set; }
        [Column] public double? Discount { get; set; }

        [Association(ThisKey = "ServiceID", OtherKey = "ID")] public Service Service { get; set; }
        [Association(ThisKey = "DiscountlistID", OtherKey = "ID")] public Discountlist Discountlist { get; set; }
    }
    [Table]
    public class CarModel : EntityWithState, IEntityWithGroup
    {
        [Column] public string Caption { get; set; }
        [Column] public string Group { get; set; }
        [Column] public string Note { get; set; }
    }
    [Table]
    public class Client : EntityWithState, IEntityWithGroup
    {
        [Column] public string Caption { get; set; }
        [Column] public string Phone { get; set; }
        [Column] public string Card { get; set; }
        [Column] public string Group { get; set; }
        [Column, Nullable][Reactive] public int? DiscountlistID { get; set; }
        [Column] public string Note { get; set; }

        [Association(ThisKey = "DiscountlistID", OtherKey = "ID")] [Reactive] public Discountlist Discountlist { get; set; }
    }
    [Table]
    public class Car : EntityWithState
    {
        [Column] public string FedCode { get; set; }
        [Column, Nullable] public int? CarModelID { get; set; }        
        [Column, Nullable] public int? ClientID { get; set; }
        [Column] public string Note { get; set; }

        [Association(ThisKey = "CarModelID", OtherKey = "ID")] [Reactive] public CarModel CarModel { get; set; }        
        [Association(ThisKey = "ClientID", OtherKey = "ID")] [Reactive] public Client Client { get; set; }
    }
    [Table]
    public class Order : EntityWithState
    {
        [Column] public Guid OrderGuid { get; set; }
        [Column] public DateTime InTime { get; set; }
        [Column] public DateTime OutTime { get; set; }
        [Column] public int? CarID { get; set; }
        [Column] public int? ClientID { get; set; }
        [Column] public int? WorkerID { get; set; }
        [Column] [Reactive] public int? DiscountID { get; set; }
        [Column] public double OverrodeDiscount { get; set; }
        [Column][Reactive] public double LastCost { get; set; }
        [Column] public bool IsNight { get; set; }
        [Column] public string Note { get; set; }
        [Column] public int BoxID { get; set; }
        [Column] public DateTime? CloseTime { get; set; }
        [Column] public Double? WorkerPay { get; set; }
        [Column] public int? UserID { get; set; }

        public DateTime InDate => InTime.Date;
        public bool IsClosed => CloseTime != null;

        [Association(ThisKey = "CarID", OtherKey = "ID")][Reactive] public Car Car { get; set; }
        [Association(ThisKey = "ClientID", OtherKey = "ID")] [Reactive] public Client Client { get; set; }
        [Association(ThisKey = "WorkerID", OtherKey = "ID")] public Worker Worker { get; set; }
        [Association(ThisKey = "DiscountID", OtherKey = "ID")] public Discountlist Discountlist { get; set; }        
        [Association(ThisKey = "UserID", OtherKey = "ID")] public User User { get; set; }
    }
    [Table]
    public class OrderItem : ReactiveObject
    {
        [Column] public int OrderID { get; set; }
        [Column] public int ServiceID { get; set; }
        [Column][Reactive] public int Quantity { get; set; }
        [Column][Reactive] public double Price { get; set; }
        [Column] [Reactive] public double Discount { get; set; }
        [Column][Reactive] public double LastPrice { get; set; }        
        [Reactive] public double LastCost { get; private set; }
        [Association(ThisKey = "ServiceID", OtherKey = "ID")] public Service Service { get; set; }
        [Association(ThisKey = "OrderID", OtherKey = "ID")] public Order Order { get; set; }

        public OrderItem()
        {
            this.WhenAnyValue(
                p => p.LastPrice,
                p => p.Quantity,
                (p, q) => p * q)
                .Subscribe(lc => LastCost = lc);
        }
        public double CalcLastPrice()
        {
            LastPrice = Math.Round(Price - Price * Discount / 100, 2, MidpointRounding.AwayFromZero);
            return LastPrice;
        }
        public double CalcLastCost()
        {
            LastCost = LastPrice * Quantity;
            return LastCost;
        }
    }
}
