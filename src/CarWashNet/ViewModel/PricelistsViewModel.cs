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
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Mapping;
using CarWashNet.Domain.Repository;
using CarWashNet.Reports;

namespace CarWashNet.ViewModel
{
    public class PricelistsViewModel : BaseItemsWithStateViewModel<Pricelist>
    {        
        public ReactiveCommand<Unit, Unit> Copy { get; set; }
        public ReactiveCommand<Unit, Unit> EditGroup { get; set; }
        public ReactiveCommand<Unit, Unit> Print { get; set; }
        public PricelistEditorViewModel EditorViewModel { get; set; }
        public PricelistGroupEditorViewModel GroupEditorViewModel { get; set; }
        public PricelistServicesViewModel ServicesViewModel { get; set; }

        public PricelistsViewModel()
        {
            ServicesViewModel = new PricelistServicesViewModel();
            Items.ShapeView().OrderBy(p => p.Caption).Apply();

            EditorViewModel = new PricelistEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);
            GroupEditorViewModel = new PricelistGroupEditorViewModel();
            GroupEditorViewModel.Save.Select(p => 0).InvokeCommand(LoadItems);            
            
            Copy = ReactiveCommand.Create(() =>
            {
                var id = EntityManagerService.DefaultPricelistManager.Copy(SelectedItem);
                LoadItems.Execute(id).Subscribe();
            }, canEdit);
            EditGroup = ReactiveCommand.Create(() =>
            {
                List<Pricelist> items;
                if (IsMultiSelect == false) items = Items.Where(p => p.ID == SelectedItem.ID).ToList();
                else items = Items.OnlySelected().ToList();
                GroupEditorViewModel.Init(items);
            }, canMultiEdit);
            Print = ReactiveCommand.Create(() =>
            {               
                var report = new PricelistReport("Reports/ReportResources/Pricelist.frx");
                report.ShowReport(SelectedItem.ID);
            }, canEdit);

            this.WhenAnyValue(p => p.SelectedItem)
                .Where(p => p != null)
                .Subscribe(p =>
                {
                    ServicesViewModel.Init(0, SelectedItem);
                });
        }
        protected override async Task<IEnumerable<Pricelist>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Pricelists
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Pricelist();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            var item = DbService.DefaultDb.Pricelists.FirstOrDefault(p => p.ID == SelectedItem.ID);
            EditorViewModel.Init(item);
        }
        protected override void DeleteImpl()
        {
            EntityManagerService.DefaultPricelistManager.ValidateAndDelete(SelectedItem);
        }
    }
    public class PricelistEditorViewModel : BaseEditorViewModel<Pricelist>
    {
        [Reactive] public List<string> Groups { get; set; }
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultPricelistManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }

        public override void Init(Pricelist entity)
        {
            base.Init(entity);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Pricelist>();
        }
    }
    public class PricelistGroupEditorViewModel : BaseGroupEditorViewModel<Pricelist>
    {
        public override void Init(IEnumerable<Pricelist> items)
        {
            base.Init(items);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Pricelist>();
            if (items.Count() > 1) Group = Groups.FirstOrDefault();
        }
        protected override void SaveImpl()
        {
            EntityManagerService.DefaultEntityManager.SetGroup<Pricelist>(Items, Group);
            base.SaveImpl();
        }
    }
    public class PricelistServicesViewModel : BaseItemsViewModel<PricelistServicesViewModel.ServiceViewModel>
    {
        [Table(Name ="Service")]
        public class ServiceViewModel : Service
        {
            [Reactive] public PricelistItem PricelistItem { get; set; }            

        }
        private Pricelist _pricelist;
        public ReactiveCommand<Unit, Unit> SavePrice { get; set; }
        public PricelistServicesViewModel()
        {
            Items.ShapeView().OrderBy(p => p.Caption).Apply();
            SavePrice = ReactiveCommand.Create(() =>
            {
                if (SelectedItem == null) return;
                EntityManagerService.DefaultPricelistManager.SaveItem(_pricelist, SelectedItem, SelectedItem.PricelistItem.Price);
                if (SelectedItem.PricelistItem?.Price == null)
                {                    
                      SelectedItem.PricelistItem.ID = 0;
                }
            });
        }
        public void Init(int id, Pricelist pricelist)
        {
            _pricelist = pricelist;
            base.Init(id);
        }
        protected override async Task<IEnumerable<ServiceViewModel>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                var query = await db.GetTable<ServiceViewModel>().OnlyActive().ToListAsync();
                var pricelistitems = db.PricelistItems.Where(p => p.PricelistID == _pricelist.ID).ToList();

                query.ForEach(p =>
                {
                    p.PricelistItem = pricelistitems.FirstOrDefault(q => q.ServiceID == p.ID);
                    if (p.PricelistItem == null) 
                    {
                        p.PricelistItem = new PricelistItem()
                        {
                            ServiceID = p.ID,
                            PricelistID = _pricelist.ID
                        };
                    }
                });

                return query.ToList();
            }
        }



    }
}
