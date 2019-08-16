using System;
using System.Collections.Generic;
using System.Linq;
using KLib.Native;
using ReactiveUI;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using System.Reactive;
using CarWashNet.Domain.Repository;
using System.Threading.Tasks;
using LinqToDB;
using CarWashNet.Domain.Managers;

namespace CarWashNet.ViewModel
{
    public class CarModelsViewModel : BaseItemsWithStateViewModel<CarModel>
    {
        public ReactiveCommand<Unit, Unit> EditGroup { get; set; }
        public CarModelEditorViewModel EditorViewModel { get; set; }        
        public CarModelGroupEditorViewModel GroupEditorViewModel { get; set; }

        public CarModelsViewModel()
        {
            Items.ShapeView().OrderBy(p => p.Caption).Apply();            

            GroupEditorViewModel = new CarModelGroupEditorViewModel();
            GroupEditorViewModel.Save.Select(p => 0).InvokeCommand(LoadItems);
            EditorViewModel = new CarModelEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);            

            EditGroup = ReactiveCommand.Create(() =>
            {
                List<CarModel> items;
                if (IsMultiSelect == false) items = Items.Where(p => p.ID == SelectedItem.ID).ToList();
                else items = Items.OnlySelected().ToList();
                GroupEditorViewModel.Init(items);

            }, canMultiEdit);
        }
        protected override async Task<IEnumerable<CarModel>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.CarModels
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new CarModel();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            using (var db = DbService.GetDb())
            {
                var item = db.CarModels
                    .FirstOrDefault(p => p.ID == SelectedItem.ID);
                EditorViewModel.Init(item);
            }
        }
        protected override void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new CarModelManager(db);
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
                .Where(p => p.Caption.SafeContains(FilterText))
                .Apply();
        }
    }
    public class CarModelEditorViewModel : BaseEditorViewModel<CarModel>
    {
        [Reactive] public List<string> Groups { get; set; }
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultCarModelManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }

        public override void Init(CarModel entity)
        {
            base.Init(entity);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<CarModel>();
        }
    }
    public class CarModelGroupEditorViewModel : BaseGroupEditorViewModel<CarModel>
    {
        public override void Init(IEnumerable<CarModel> items)
        {
            base.Init(items);
            Groups = EntityManagerService.DefaultEntityManager.GetGroups<CarModel>();
            if(items.Count() > 1) Group = Groups.FirstOrDefault();
        }
        protected override void SaveImpl()
        {
            EntityManagerService.DefaultEntityManager.SetGroup<CarModel>(Items, Group);
            base.SaveImpl();
        }
    }
    public class CarModelSelectorViewModel : CarModelsViewModel
    {      
        protected override async Task<IEnumerable<CarModel>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.CarModels
                    .OnlyActive()
                    .ToListAsync();
            }
        }
    }
}
