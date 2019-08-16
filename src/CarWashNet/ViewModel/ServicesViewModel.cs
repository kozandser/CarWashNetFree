using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KLib.Native;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Services;
using CarWashNet.Domain.Repository;
using LinqToDB;

namespace CarWashNet.ViewModel
{
    public class ServicesViewModel : BaseItemsWithStateViewModel<Service>
    {
        public ReactiveCommand<Unit, Unit> EditGroup { get; set; }
        public ServiceEditorViewModel EditorViewModel { get; set; }
        public ServiceGroupEditorViewModel GroupEditorViewModel { get; set; }

        public ServicesViewModel()
        {
            Items.ShapeView().OrderBy(p => p.Caption).Apply();

            GroupEditorViewModel = new ServiceGroupEditorViewModel();
            GroupEditorViewModel.Save.Select(p => 0).InvokeCommand(LoadItems);
            EditorViewModel = new ServiceEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);            

            EditGroup = ReactiveCommand.Create(() =>
            {
                List<Service> items;
                if (IsMultiSelect == false) items = Items.Where(p => p.ID == SelectedItem.ID).ToList();
                else items = Items.OnlySelected().ToList();
                GroupEditorViewModel.Init(items);

            }, canMultiEdit);
        }

        protected override async Task<IEnumerable<Service>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Services
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Service();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            using (var db = DbService.GetDb())
            {
                var item = db.Services
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);
                EditorViewModel.Init(item);
            }
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new ServiceManager(db);
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
    public class ServiceEditorViewModel : BaseEditorViewModel<Service>
    {
        [Reactive] public List<string> Groups { get; set; }
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultServiceManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }

        public override void Init(Service entity)
        {
            base.Init(entity);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Service>();
        }
    }

    public class ServiceGroupEditorViewModel : BaseGroupEditorViewModel<Service>
    {
        public override void Init(IEnumerable<Service> items)
        {
            base.Init(items);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<Service>();
            if (items.Count() > 1) Group = Groups.FirstOrDefault();
        }
        protected override void SaveImpl()
        {
            EntityManagerService.DefaultEntityManager.SetGroup<Service>(Items, Group);
            base.SaveImpl();
        }
    }
}
