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
using LinqToDB.Mapping;
using System.Threading.Tasks;
using LinqToDB;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Repository;

namespace CarWashNet.ViewModel
{
    public class DiscountlistsViewModel : BaseItemsWithStateViewModel<Discountlist>
    {
        public ReactiveCommand<Unit, Unit> Copy { get; set; }
        public ReactiveCommand<Unit, Unit> EditGroup { get; set; }
        public DiscountlistEditorViewModel EditorViewModel { get; set; }
        public DiscountlistGroupEditorViewModel GroupEditorViewModel { get; set; }
        public DiscountlistServicesViewModel ServicesViewModel { get; set; }

        public DiscountlistsViewModel()
        {
            ServicesViewModel = new DiscountlistServicesViewModel();
            Items.ShapeView().OrderBy(p => p.Caption).Apply();

            EditorViewModel = new DiscountlistEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);
            GroupEditorViewModel = new DiscountlistGroupEditorViewModel();
            GroupEditorViewModel.Save.Select(p => 0).InvokeCommand(LoadItems);

            
            Copy = ReactiveCommand.Create(() =>
            {
                var id = EntityManagerService.DefaultDiscountlistManager.Copy(SelectedItem);
                LoadItems.Execute(id).Subscribe();
            }, canEdit);

            this.WhenAnyValue(p => p.SelectedItem)
                .Where(p => p != null)
                .Subscribe(p =>
                {
                    ServicesViewModel.Init(0, SelectedItem);
                });
        }
        protected override async Task<IEnumerable<Discountlist>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Discountlists
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Discountlist();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            using (var db = DbService.GetDb())
            {
                var item = db.Discountlists
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);
                EditorViewModel.Init(item);
            }
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new DiscountlistManager(db);
                if (IsMultiSelect == false) manager.ValidateAndDelete(SelectedItem);
                else
                {
                    db.BeginTransaction();                    
                    manager.ValidateAndDelete(Items.OnlySelected());
                    db.CommitTransaction();
                }
            }
        }
    }
    public class DiscountlistEditorViewModel : BaseEditorViewModel<Discountlist>
    {
        [Reactive] public List<string> Groups { get; set; }
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultDiscountlistManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }

        public override void Init(Discountlist entity)
        {
            base.Init(entity);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Discountlist>();
        }
    }
    public class DiscountlistGroupEditorViewModel : BaseGroupEditorViewModel<Discountlist>
    {
        public override void Init(IEnumerable<Discountlist> items)
        {
            base.Init(items);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Discountlist>();
            if (items.Count() > 1) Group = Groups.FirstOrDefault();
        }
        protected override void SaveImpl()
        {
            EntityManagerService.DefaultEntityManager.SetGroup<Discountlist>(Items, Group);
            base.SaveImpl();
        }
    }
    public class DiscountlistServicesViewModel : BaseItemsViewModel<DiscountlistServicesViewModel.ServiceViewModel>
    {
        [Table(Name = "Service")]
        public class ServiceViewModel : Service
        {
            [Reactive] public DiscountlistItem DiscountlistItem { get; set; }

        }
        private Discountlist _discountlist;
        public ReactiveCommand<Unit, Unit> SaveDiscount { get; set; }
        public DiscountlistServicesViewModel()
        {
            Items.ShapeView().OrderBy(p => p.Caption).Apply();
            SaveDiscount = ReactiveCommand.Create(() =>
            {
                if (SelectedItem == null) return;
                EntityManagerService.DefaultDiscountlistManager.SaveItem(_discountlist, SelectedItem, SelectedItem.DiscountlistItem.Discount);
                if (SelectedItem.DiscountlistItem?.Discount == null)
                {
                    SelectedItem.DiscountlistItem.ID = 0;
                }
            });
        }
        public void Init(int id, Discountlist discountlist)
        {
            _discountlist = discountlist;
            base.Init(id);
        }
        protected override async Task<IEnumerable<ServiceViewModel>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                var query = await db.GetTable<ServiceViewModel>().OnlyActive().ToListAsync();
                var discountlistitems = db.DiscountlistItems.Where(p => p.DiscountlistID == _discountlist.ID).ToList();

                query.ForEach(p =>
                {
                    p.DiscountlistItem = discountlistitems.FirstOrDefault(q => q.ServiceID == p.ID);
                    if (p.DiscountlistItem == null)
                    {
                        p.DiscountlistItem = new DiscountlistItem()
                        {
                            ServiceID = p.ID,
                            DiscountlistID = _discountlist.ID
                        };
                    }
                });

                return query.ToList();
            }
        }



    }
    public class DiscountlistSelectorViewModel : DiscountlistsViewModel
    {       
        protected override async Task<IEnumerable<Discountlist>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Discountlists
                    .OnlyActive()
                    .ToListAsync();
            }
        }
    }
}
