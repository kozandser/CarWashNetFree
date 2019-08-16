using System;
using System.Collections.Generic;
using System.Linq;
using KLib.Native;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using LinqToDB;
using System.Threading.Tasks;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Repository;
using CarWashNet.Applications;

namespace CarWashNet.ViewModel
{
    public class UserAppBindingsViewModel : BaseItemsWithStateViewModel<User>
    {
        public AppItemsViewModel AppItemsViewModel { get; set; }
        public UserEditorViewModel EditorViewModel { get; set; }

        public UserAppBindingsViewModel()
        {
            AppItemsViewModel = new AppItemsViewModel();
            Items.ShapeView().OrderBy(p => p.Caption).Apply();           

            EditorViewModel = new UserEditorViewModel();
            EditorViewModel.Save.InvokeCommand(LoadItems);

            this.WhenAnyValue(p => p.SelectedItem)
                .Where(p => p != null)
                .Subscribe(p =>
                {
                    AppItemsViewModel.Init(0, SelectedItem);
                });            
        }

        protected override async Task<IEnumerable<User>> LoadItemsImpl()
        {
            using (var db = DbService.GetDb())
            {
                return await db.Users
                    .OnlyNotDeleted()
                    .ToListAsync();
            }
        }
        protected override void AddImpl()
        {
            var item = new User();
            EditorViewModel.Init(item);
        }
        protected override void EditImpl()
        {
            var item = DbService.DefaultDb.Users.FirstOrDefault(p => p.ID == SelectedItem.ID);
            EditorViewModel.Init(item);
        }
        protected override void LockImpl()
        {
            EntityManagerService.DefaultUserManager.ValidateAndSetState(SelectedItem, EntityStateEnum.Unused);
        }
        protected override void DeleteImpl()
        {
            EntityManagerService.DefaultUserManager.ValidateAndDelete(SelectedItem);
        }
    }

    public class UserEditorViewModel : BaseEditorViewModel<User>
    {
        public override void Init(User entity)
        {
            base.Init(entity);
            EditingItem.DecryptPassword();
        }
        protected override int SaveImpl()
        {
            EditingItem.EncryptPassword();
            EntityManagerService.DefaultUserManager.ValidateAndSave(EditingItem);
            return EditingItem.ID;
        }
    }
    public class AppItemsViewModel : BaseItemsViewModel<AppItem>
    {
        private User _user;
        public ReactiveCommand<Unit, Unit> BindApp { get; set; }

        public AppItemsViewModel()
        {
            Items.ShapeView().OrderBy(p => p.OrderNumber).Apply();

            BindApp = ReactiveCommand.Create(() =>
            {
                if (SelectedItem == null) return;
                EntityManagerService.DefaultAppItemManager.BindApp(SelectedItem, _user, !SelectedItem.IsBinded);
                //SelectedApp.BindApp(SelectedItem.ID, !SelectedApp.IsBinded);
            });
            BindApp.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            BindApp.Select(p => 0).InvokeCommand(LoadItems);
        }
        public void Init(int id, User user)
        {
            _user = user;
            base.Init(id);
        }
        protected override async Task<IEnumerable<AppItem>> LoadItemsImpl()
        {
            var result = await Task<List<AppItem>>.Run(() =>
            {
                using (var db = DbService.GetDb())
                {
                    var manager = new AppItemManager(db);
                    var lst = manager.GetUserApps(_user);
                    return lst;
                }
            });
            return result;
        }
    }
}
