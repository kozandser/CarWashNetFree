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

namespace CarWashNet.ViewModel
{
    public class ClientsViewModel : BaseItemsWithStateViewModel<Client>
    {
        public ReactiveCommand<Unit, Unit> EditGroup { get; set; }
        public ClientEditorViewModel EditorViewModel { get; set; }
        public ClientGroupEditorViewModel GroupEditorViewModel { get; set; }
        public ClientInfoViewModel ClientInfoViewModel { get; set; }
        public ReactiveCommand ShowInfo { get; set; }

        public ClientsViewModel()
        {            
            Items.ShapeView().OrderBy(p => p.Caption).Apply();

            GroupEditorViewModel = new ClientGroupEditorViewModel();
            GroupEditorViewModel.Save.Select(p => 0).InvokeCommand(LoadItems);            
            EditorViewModel = new ClientEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);

            ClientInfoViewModel = new ClientInfoViewModel();
            ShowInfo = ReactiveCommand.Create(() =>
            {
                ClientInfoViewModel.Init(SelectedItem.ID);
            }, canEdit);

            EditGroup = ReactiveCommand.Create(() =>
            {
                List<Client> items;
                if (IsMultiSelect == false) items = Items.Where(p => p.ID == SelectedItem.ID).ToList();
                else items = Items.OnlySelected().ToList();
                GroupEditorViewModel.Init(items);

            }, canMultiEdit);
        }
        protected override async Task<IEnumerable<Client>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Clients
                    .LoadWith(p => p.Discountlist)
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Client();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            using (var db = DbService.GetDb())
            {
                var item = db.Clients
                    .LoadWith(p => p.Discountlist)
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);
                EditorViewModel.Init(item);
            }
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new ClientManager(db);
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
                    (p.Caption.SafeContains(FilterText)) ||
                    (p.Phone.SafeContains(FilterText)) ||
                    (p.Card.SafeContains(FilterText)))                    
                .Apply();
        }
    }
    public class ClientEditorViewModel : BaseEditorViewModel<Client>
    {
        [Reactive] public List<string> Groups { get; set; }
        [Reactive] public List<Discountlist> Discountlists { get; set; }
        
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultClientManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }

        public override void Init(Client entity)
        {
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Client>();
            Discountlists = DbService.GetDataContext().GetTable<Discountlist>()
                .OnlyActive()
                .OrderBy(p => p.Date)
                .ToList();
            base.Init(entity);            
        }        
    }
    public class ClientGroupEditorViewModel : BaseGroupEditorViewModel<Client>
    {
        public override void Init(IEnumerable<Client> items)
        {
            base.Init(items);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Client>();
            if (items.Count() > 1) Group = Groups.FirstOrDefault();
        }
        protected override void SaveImpl()
        {
            EntityManagerService.DefaultEntityManager.SetGroup<Client>(Items, Group);
            base.SaveImpl();
        }
    }
    public class ClientSelectorViewModel : ClientsViewModel
    {
        protected override async Task<IEnumerable<Client>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Clients
                    .LoadWith(p => p.Discountlist)
                    .OnlyActive()
                    .ToListAsync();
            }
        }
    }
    public class ClientInfoViewModel : ReactiveObject
    {
        [Reactive] public bool IsOpen { get; set; }
        [Reactive] public Client Client { get; set; }
        [Reactive] public int OrdersCount { get; set; }
        [Reactive] public double OrdersSum { get; set; }
        [Reactive] public DateTime? FirstOrderDate { get; set; }
        [Reactive] public DateTime? LastOrderDate { get; set; }

        public ClientInfoViewModel()
        {

        }

        public void Init(int id)
        {
            using (var db = DbService.GetDb())
            {
                Client = db.Clients
                    .FirstOrDefault(p => p.ID == id);
                var orders = db.Orders
                    .Where(p => p.ClientID == id)
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
