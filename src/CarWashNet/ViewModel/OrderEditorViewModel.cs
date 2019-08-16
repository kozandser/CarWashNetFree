using CarWashNet.Application.Navigation;
using CarWashNet.Applications;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using KLib.Native;
using LinqToDB;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.ViewModel
{
    public class OrderEditorViewModel : ReactiveObject
    {
        [Reactive] public bool IsReadOnly { get; set; }
        [Reactive] public Order EditingItem { get; set; }
        public ReactiveCommand<Unit, int> Save { get; set; }

        [Reactive]public string CarCaption { get; set; }
        [Reactive] public Car SelectedCar { get; set; }
        public ReactiveCommand<Unit, CarSelectorViewModel> SelectCar { get; set; }
        public ReactiveCommand<Unit, Unit> ClearCar { get; set; }
        public CarSelectorViewModel CarSelectorViewModel { get; set; }

        [Reactive] public string ClientCaption { get; set; }
        [Reactive] public Client SelectedClient { get; set; }
        public ReactiveCommand<Unit, ClientSelectorViewModel> SelectClient{ get; set; }
        public ReactiveCommand<Unit, Unit> ClearClient { get; set; }
        public ClientSelectorViewModel ClientSelectorViewModel { get; set; }

        [Reactive] public List<Worker> Workers { get; set; }

        [Reactive] public double Cost { get; set; }
        [Reactive] public double LastCost { get; set; }

        [Reactive] public List<Pricelist> Pricelists { get; set; }
        [Reactive] public Pricelist SelectedPricelist { get; set; }
        [Reactive] public List<PricelistItem> PricelistItems { get; set; }
        [Reactive] public PricelistItem SelectedPricelistItem { get; set; }

        [Reactive] public List<Discountlist> Discountlists { get; set; }
        [Reactive] public Discountlist SelectedDiscountlist { get; set; }
        [Reactive] public List<DiscountlistItem> DiscountlistItems { get; set; }
        [Reactive] public DiscountlistItem SelectedDiscountlistItem { get; set; }

        [Reactive] public ReactiveList<OrderItem> OrderItems { get; set; }
        [Reactive] public OrderItem SelectedOrderItem { get; set; }

        public ReactiveCommand<Unit, Unit> AddOrderItem { get; set; }
        public ReactiveCommand<Unit, Unit> RemoveOrderItem { get; set; }
        public ReactiveCommand<Unit, Unit> DeleteOrderItem { get; set; }
        public ReactiveCommand<Unit, Unit> ApplyDiscount { get; set; }


        public OrderEditorViewModel()
        {
            OrderItems = new ReactiveList<OrderItem>();

            Save = ReactiveCommand.Create<int>(() =>
            {
                var result = SaveImpl();                
                return result;
            }, this.WhenAnyValue(p => p.EditingItem.IsClosed).Select(p => p == false));
            Save.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            CarSelectorViewModel = new CarSelectorViewModel();
            CarSelectorViewModel.Select.Subscribe(car =>
            {
                SelectedCar = DbService.GetDataContext().GetTable<Car>()
                    .LoadWith(p => p.CarModel)
                    .FirstOrDefault(p => p.ID == car.ID);
            });
            SelectCar = ReactiveCommand.Create(() =>
            {
                CarSelectorViewModel.Init(SelectedCar?.ID ?? -1);
                return CarSelectorViewModel;
                
            });
            ClearCar = ReactiveCommand.Create(() =>
            {
                SelectedCar = null;
            });
            this.WhenAnyValue(p => p.SelectedCar)
                .Subscribe(car =>
                {                    
                    if (car != null)
                    {
                        SelectedClient = DbService.GetDataContext().GetTable<Client>().FirstOrDefault(p => p.ID == car.ClientID);                        
                        CarCaption = $"{car.FedCode}, {car.CarModel?.Caption}";
                    }
                    else
                    {
                        SelectedClient = null;
                        CarCaption = String.Empty;
                    }                        
                });

            ClientSelectorViewModel = new ClientSelectorViewModel();
            ClientSelectorViewModel.Select.Subscribe(client =>
            {
                SelectedClient = DbService.GetDataContext().GetTable<Client>().FirstOrDefault(p => p.ID == client.ID);                                      
            });
            SelectClient = ReactiveCommand.Create(() =>
            {
                ClientSelectorViewModel.Init(SelectedClient?.ID ?? -1);
                return ClientSelectorViewModel;
            });
            ClearClient = ReactiveCommand.Create(() =>
            {
                SelectedClient = null;
            });            
            this.WhenAnyValue(p => p.SelectedClient)
                .Subscribe(client =>
                {
                    if (client != null)
                    {
                        SelectedDiscountlist = Discountlists.FirstOrDefault(p => p.ID == client.DiscountlistID);
                        ClientCaption = $"{client.Caption}, {client.Card}";
                    }
                    else
                    {
                        SelectedDiscountlist = null;
                        ClientCaption = String.Empty;
                    }
                });

            this.WhenAnyValue(p => p.SelectedPricelist)
                .Subscribe(pricelist =>
                {
                    if (pricelist == null) PricelistItems = new List<PricelistItem>();
                    else PricelistItems = DbService.GetDataContext().GetTable<PricelistItem>()
                        .LoadWith(p => p.Service)
                        .Where(p => p.PricelistID == pricelist.ID)
                        .Where(p => p.Service.EntityState == EntityStateEnum.Active)
                        .OrderBy(p => p.Service.Caption)
                        .ToList();
                });           

            AddOrderItem = ReactiveCommand.Create(() =>
            {
                if (SelectedPricelistItem == null) return;
                var orderItem = OrderItems.FirstOrDefault(
                    p => p.OrderID == EditingItem.ID &&
                    p.ServiceID == SelectedPricelistItem.ServiceID &&
                    p.Price == SelectedPricelistItem.Price);
                if (orderItem == null)
                {
                    orderItem = new OrderItem()
                    {
                        OrderID = EditingItem.ID,
                        ServiceID = SelectedPricelistItem.ServiceID,
                        Service = DbService.GetDataContext().GetTable<Service>().FirstOrDefault(p => p.ID == SelectedPricelistItem.ServiceID),
                        Price = SelectedPricelistItem.Price.Value,
                        Quantity = 1,
                        LastPrice = 0
                    };                    

                    if (SelectedDiscountlist != null)
                    {
                        var discountlistItem = DbService.GetDataContext().GetTable<DiscountlistItem>()
                            .Where(p => p.DiscountlistID == SelectedDiscountlist.ID)
                            .FirstOrDefault(p => p.ServiceID == orderItem.ServiceID);
                        if (discountlistItem != null) orderItem.Discount = discountlistItem.Discount.Value;
                    }
                    orderItem.CalcLastPrice();
                    OrderItems.Add(orderItem);
                }
                else
                {
                    orderItem.Quantity++;
                }

                SelectedOrderItem = orderItem;
                calcCost();
            });
            AddOrderItem.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            RemoveOrderItem = ReactiveCommand.Create(() =>
            {
                if (SelectedOrderItem == null) return;
                if (SelectedOrderItem.Quantity > 1) SelectedOrderItem.Quantity--;
                else
                {
                    OrderItems.Remove(SelectedOrderItem);
                }
                calcCost();
            });
            RemoveOrderItem.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            DeleteOrderItem = ReactiveCommand.Create(() =>
            {
                if (SelectedOrderItem == null) return;
                OrderItems.Remove(SelectedOrderItem);
                calcCost();
            });
            DeleteOrderItem.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            ApplyDiscount = ReactiveCommand.Create(() =>
            {
                if (SelectedDiscountlist == null)
                    OrderItems.ForEach(orderItem =>
                    {
                        orderItem.Discount = 0;
                        orderItem.CalcLastPrice();
                    });
                else
                {
                    var discountlistItems = DbService.GetDataContext().GetTable<DiscountlistItem>()
                        .Where(p => p.DiscountlistID == SelectedDiscountlist.ID)
                        .ToList();
                    OrderItems.ForEach(orderItem =>
                    {
                        var discountlistItem = discountlistItems
                            .FirstOrDefault(q => q.ServiceID == orderItem.ServiceID);
                        if (discountlistItem != null) orderItem.Discount = discountlistItem.Discount.Value;
                        else orderItem.Discount = 0;
                        orderItem.CalcLastPrice();
                    });
                }
                calcCost();
            });
            ApplyDiscount.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

        }
        public virtual void Init(Order entity)
        {           
            using (var db = DbService.GetDb())
            {
                Workers = db.Workers
                    .OnlyActive(entity.WorkerID ?? -1)
                    .OrderBy(p => p.Caption)
                    .ToList();

                Pricelists = db.Pricelists
                    .OnlyActive()
                    .OrderByDescending(p => p.Date)
                    .ToList();

                Discountlists = db.Discountlists
                    .OnlyActive()
                    .OrderByDescending(p => p.Date)
                    .ToList();

                OrderItems.ReplaceRange(db.OrderItems
                    .LoadWith(p => p.Service)
                    .Where(p => p.OrderID == entity.ID)
                    .OrderBy(p => p.Service.Caption)
                    .ToList());
            }

            EditingItem = entity;

            SelectedPricelist = Pricelists.FirstOrDefault();
            SelectedCar = EditingItem.Car;
            SelectedClient = EditingItem.Client;
            SelectedDiscountlist = EditingItem.Discountlist;

            calcCost();
            LastCost = EditingItem.LastCost;

            IsReadOnly = EditingItem.CloseTime != null;
        }

        protected virtual int SaveImpl()
        {
            EditingItem.CarID = SelectedCar?.ID;
            EditingItem.ClientID = SelectedClient?.ID;
            EditingItem.DiscountID = SelectedDiscountlist?.ID;
            EditingItem.LastCost = LastCost;

            EntityManagerService.DefaultOrderManager.ValidateAndSave(EditingItem, OrderItems.ToList());
            return EditingItem.ID;
        }
        void calcCost()
        {
            Cost = OrderItems.Sum(p => p.LastCost);
            LastCost = Cost;
        }
    }
}
