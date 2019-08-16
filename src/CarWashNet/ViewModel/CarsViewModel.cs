using System;
using System.Collections.Generic;
using System.Linq;
using KLib.Native;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using LinqToDB;
using CarWashNet.Domain.Repository;
using System.Threading.Tasks;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Validation;

namespace CarWashNet.ViewModel
{
    public class CarsViewModel : BaseItemsWithStateViewModel<Car>
    {
        public CarEditorViewModel EditorViewModel { get; set; }
        public CarInfoViewModel CarInfoViewModel { get; set; }
        public ReactiveCommand ShowInfo { get; set; }

        public CarsViewModel()
        {            
            Items.ShapeView().OrderBy(p => p.FedCode).Apply();
            
            EditorViewModel = new CarEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);

            CarInfoViewModel = new CarInfoViewModel();  
            ShowInfo = ReactiveCommand.Create(() =>
            {
                CarInfoViewModel.Init(SelectedItem.ID);
            }, canEdit);
        }
        protected override async Task<IEnumerable<Car>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Cars
                    .LoadWith(p => p.CarModel)
                    .LoadWith(p => p.Client)
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Car();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            using (var db = DbService.GetDb())
            {
                var item = db.Cars
                    .LoadWith(p => p.CarModel)
                    .LoadWith(p => p.Client)
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);
                EditorViewModel.Init(item);
            }
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new CarManager(db);
                if (IsMultiSelect == false) manager.ValidateAndDelete(SelectedItem);
                else
                {
                    db.BeginTransaction();
                    manager.ValidateAndDelete(Items.OnlySelected());
                    db.CommitTransaction();
                }
            }
        }
        protected override void FilterItems()
        {
            Items.ShapeView()
                .Where(p =>
                    (p.FedCode.SafeContains(FilterText)) ||
                    (p.CarModel == null ? false : p.CarModel.Caption.SafeContains(FilterText)))
                .Apply();
        }
    }
    public class CarEditorViewModel : BaseEditorViewModel<Car>
    {
        public ReactiveCommand<Unit, CarModelSelectorViewModel> SelectCarModel { get; set; }
        public ReactiveCommand<Unit, Unit> ClearCarModel { get; set; }
        public CarModelSelectorViewModel CarModelSelectorViewModel { get; set; }
        public ReactiveCommand<Unit, ClientSelectorViewModel> SelectClient { get; set; }
        public ReactiveCommand<Unit, Unit> ClearClient { get; set; }
        public ClientSelectorViewModel ClientSelectorViewModel { get; set; }

        public CarEditorViewModel()
        {
            CarModelSelectorViewModel = new CarModelSelectorViewModel();
            CarModelSelectorViewModel.Select.Subscribe(_ =>
            {
                EditingItem.CarModelID = CarModelSelectorViewModel.SelectedItem.ID;
                EditingItem.CarModel = CarModelSelectorViewModel.SelectedItem;
            });
            SelectCarModel = ReactiveCommand.Create(() =>
            {
                CarModelSelectorViewModel.Init(EditingItem.CarModelID ?? -1);
                return CarModelSelectorViewModel;
            });
            ClearCarModel = ReactiveCommand.Create(() =>
            {
                EditingItem.CarModelID = null;
                EditingItem.CarModel = null;
            });
            ClientSelectorViewModel = new ClientSelectorViewModel();
            ClientSelectorViewModel.Select.Subscribe(_ =>
            {
                EditingItem.ClientID = ClientSelectorViewModel.SelectedItem.ID;
                EditingItem.Client = ClientSelectorViewModel.SelectedItem;
            });
            SelectClient = ReactiveCommand.Create(() =>
            {
                ClientSelectorViewModel.Init(EditingItem.ClientID ?? -1);
                return ClientSelectorViewModel;
            });
            ClearClient = ReactiveCommand.Create(() =>
            {
                EditingItem.ClientID = null;
                EditingItem.Client = null;
            });            
        }
        protected override int SaveImpl()
        {
            using (var db = DbService.GetDb())
            {
                EditingItem.ValidateAndThrow<CarValidator, Car>("CheckFedCode", db);
            }                
            EntityManagerService.DefaultCarManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }              
    }
    public class CarSelectorViewModel : CarsViewModel
    {
        protected override async Task<IEnumerable<Car>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Cars
                    .LoadWith(p => p.CarModel)
                    .LoadWith(p => p.Client)
                    .OnlyActive()
                    .ToListAsync();
            }
        }
    }
    public class CarInfoViewModel : ReactiveObject
    {
        [Reactive] public bool IsOpen { get; set; }
        [Reactive] public Car Car { get; set; }
        [Reactive] public int OrdersCount { get; set; }
        [Reactive] public double OrdersSum { get; set; }
        [Reactive] public DateTime? FirstOrderDate { get; set; }
        [Reactive] public DateTime? LastOrderDate { get; set; }

        public CarInfoViewModel()
        {

        }

        public void Init(int id)
        {
            using (var db = DbService.GetDb())
            {
                Car = db.Cars
                    .LoadWith(p => p.CarModel)
                    .FirstOrDefault(p => p.ID == id);
                var orders = db.Orders
                    .Where(p => p.CarID == id)
                    .Where(p => p.CloseTime != null)
                    .OrderBy(p => p.InTime)
                    .ToList();
                OrdersCount = orders.Count;
                OrdersSum = orders.Sum(p => p.LastCost);
                FirstOrderDate = orders.FirstOrDefault()?.InTime;
                LastOrderDate = orders.LastOrDefault()?.InTime;
            }
            IsOpen = true;
        }
    }


}
