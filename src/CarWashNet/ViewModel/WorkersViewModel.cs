using System;
using System.Collections.Generic;
using System.Linq;
using KLib.Native;
using ReactiveUI;
using System.Reactive.Linq;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using System.Threading.Tasks;
using LinqToDB;
using CarWashNet.Domain.Repository;

namespace CarWashNet.ViewModel
{
    public class WorkersViewModel : BaseItemsWithStateViewModel<Worker>
    {
        public WorkerEditorViewModel EditorViewModel { get; set; }
        public WorkersViewModel()
        {            
            Items.ShapeView().OrderBy(p => p.Caption).Apply();

            EditorViewModel = new WorkerEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);            
        }
        protected override async Task<IEnumerable<Worker>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Workers
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new Worker();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            var item = DbService.DefaultDb.Workers.FirstOrDefault(p => p.ID == SelectedItem.ID);
            EditorViewModel.Init(item);
        }
        protected override void DeleteImpl()
        {
            EntityManagerService.DefaultWorkerManager.ValidateAndDelete(SelectedItem);
        }        
    }
    public class WorkerEditorViewModel : BaseEditorViewModel<Worker>
    {
        protected override int SaveImpl()
        {
            EntityManagerService.DefaultWorkerManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }
    }
}
