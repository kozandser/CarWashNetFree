using CarWashNet.Applications;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Repository;
using CarWashNet.Domain.Services;
using KLib.Native;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CarWashNet.ViewModel
{
    public abstract class BaseItemsViewModel<T> : ReactiveObject where T : IEntity, ISelectable
    {
        public ReactiveList<T> Items { get; set; }
        [Reactive] public T SelectedItem { get; set; }
        public ICollectionView View { get; set; }

        public ReactiveCommand<int, Unit> LoadItems { get; set; }
        public ReactiveCommand<Unit, Unit> Add { get; set; }
        public ReactiveCommand<Unit, Unit> Edit { get; set; }
        public ReactiveCommand<Unit, Unit> Delete { get; set; }
        public ReactiveCommand<Unit, T> Select { get; set; }

        [Reactive] public bool? IsMultiSelect { get; protected set; }
        public ReactiveCommand<bool?, Unit> SwitchSelection { get; set; }
        public ReactiveCommand<Unit, Unit> SelectAll { get; set; }
        public ReactiveCommand<Unit, Unit> UnselectAll { get; set; }

        [Reactive] public string FilterText { get; set; }

        protected IObservable<bool> canEdit;
        protected IObservable<bool> canMultiEdit;

        public BaseItemsViewModel()
        {
            Items = new ReactiveList<T>();
            View = CollectionViewSource.GetDefaultView(Items);

            this.WhenAnyValue(x => x.FilterText)
                .Throttle(TimeSpan.FromMilliseconds(300), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(filter =>
                {
                    if (String.IsNullOrEmpty(filter)) Items.ShapeView().ClearFilter().Apply();
                    else FilterItems();

                    if (IsMultiSelect != false) setAllSelection(false);
                });

            Items.ChangeTrackingEnabled = true;
            Items.ItemChanged
                .Where(p => p.PropertyName == nameof(p.Sender.IsSelected))
                .Subscribe(p =>
                {
                    var viewItems = View.Cast<ISelectable>().ToList();

                    var count1 = viewItems.Count(q => q.IsSelected);
                    var count = viewItems.Count;
                    if (count == 0) IsMultiSelect = false;
                    else if (count1 == 0) IsMultiSelect = false;
                    else if (count == count1) IsMultiSelect = true;
                    else IsMultiSelect = null;
                });

            canEdit = this.WhenAnyValue(
                p => p.SelectedItem, p => p.IsMultiSelect,
                (s, m) =>
                {
                    if (m == false) return s != null;
                    else return false;
                });
            canMultiEdit = this.WhenAnyValue(
                p => p.SelectedItem, p => p.IsMultiSelect,
                (s, m) =>
                {
                    if (m == false) return s != null;
                    else return true;
                });

            LoadItems = ReactiveCommand.CreateFromTask<int>(async (id) =>
            {
                var result = await LoadItemsImpl();
                RefreshItems(result, id);
                IsMultiSelect = false;
            });
            LoadItems.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            Add = ReactiveCommand.Create(() =>
            {
                AddImpl();
            });
            Edit = ReactiveCommand.Create(() =>
            {
                EditImpl();
            }, canEdit);
            Select = ReactiveCommand.Create<T>(() =>
            {
                return SelectImpl();
            }, canEdit);
            Select.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            SwitchSelection = ReactiveCommand.Create<bool?>(p =>
            {
                bool r;
                if (p == false || p == null) r = true;
                else r = false;

                setViewAllSelection(r);
            });
            SelectAll = ReactiveCommand.Create(() =>
            {
                setViewAllSelection(true);
            });
            UnselectAll = ReactiveCommand.Create(() =>
            {
                setViewAllSelection(false);
            });
        }
        public virtual void Init(int id)
        {
            LoadItems.Execute(id).Subscribe();
        }
        protected virtual bool Filter(T item)
        {
            return true;
        }
        int getCurrentItemId(int id)
        {
            int currentId = -1;
            if (id == 0 && SelectedItem != null) currentId = SelectedItem.ID;
            if (id != 0) currentId = id;

            return currentId;
        }
        void setSelectedItem(int id)
        {
            if (id < 0) SelectedItem = Items.FirstOrDefault();
            else SelectedItem = Items.FirstOrDefault(p => p.ID == id);
        }

        private void setCurrentItem(int id)
        {
            if (View == null) { return; }
            if (View.IsEmpty == false)
            {
                if (id < 0) View.MoveCurrentToFirst();
                else
                {
                    var itemToSelect = View.SourceCollection.OfType<T>().FirstOrDefault(p => p.ID == id);                        
                    if (itemToSelect == null) View.MoveCurrentToFirst();
                    else View.MoveCurrentTo(itemToSelect);                    
                }
                SelectedItem = (T)View.CurrentItem;
            }
        }
        public void RefreshItems(IEnumerable<T> newlist, int id)
        {
            var currentId = getCurrentItemId(id);
            Items.ReplaceRange(newlist);
            setCurrentItem(currentId);
        }
        void setAllSelection(bool flag)
        {
            Items.ForEach(p => p.IsSelected = flag);
        }
        void setViewAllSelection(bool flag)
        {
            foreach (var item in View)
            {
                ((ISelectable)item).IsSelected = flag;
            }
        }
        protected virtual async Task<IEnumerable<T>> LoadItemsImpl()
        {
            var result = await Task<List<T>>.Run(() =>
                {
                    return new List<T>();
                });
            return result;
        }
        protected virtual void FilterItems()
        {

        }
        protected virtual void AddImpl()
        {

        }
        protected virtual void EditImpl()
        {

        }
        protected virtual T SelectImpl()
        {
            return SelectedItem;
        }
    }

    public abstract class BaseItemsWithStateViewModel<T> : BaseItemsViewModel<T> where T : class, IEntityWithState, ISelectable
    {        
        public ReactiveCommand<Unit, Unit> Lock { get; set; }
        public ReactiveCommand<Unit, Unit> Unlock { get; set; }

        public BaseItemsWithStateViewModel()
        {
            Lock = ReactiveCommand.Create(() =>
            {
                LockImpl();                
            }, canMultiEdit);
            Lock.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            Unlock = ReactiveCommand.Create(() =>
            {
                UnlockImpl();              
            }, canMultiEdit);
            Unlock.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                InteractionResult dialogResult;
                if(IsMultiSelect == false)
                {
                    dialogResult = await Interactions.ShowConfirmationAsync("Удалить строку?", "Строка будет удалена безвозвратно.", "Удалить", "Не удалять");
                }
                else
                {
                    dialogResult = await Interactions.ShowConfirmationAsync("Удалить выбранные строки?", "Строки будут удалены безвозвратно.", "Удалить", "Не удалять");
                }
                if(dialogResult == InteractionResult.Yes)  DeleteImpl();
            }, canMultiEdit);
            Delete.Select(p => 0).InvokeCommand(LoadItems);            
            Delete.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

        }

        protected virtual void LockImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new EntityManager(db);
                if (IsMultiSelect == false) manager.Lock(SelectedItem);
                else
                {                    
                    db.BeginTransaction();
                    manager.Lock(Items.OnlySelected());
                    db.CommitTransaction();
                }                   
            }
        }
        protected virtual void UnlockImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new EntityManager(db);
                if (IsMultiSelect == false) manager.Unlock(SelectedItem);
                else
                {
                    db.BeginTransaction();
                    manager.Unlock(Items.OnlySelected());
                    db.CommitTransaction();
                }
            }

        }
        protected virtual void DeleteImpl()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new EntityManager(db);
                if (IsMultiSelect == false) manager.Delete(SelectedItem);
                else
                {
                    db.BeginTransaction();
                    manager.Delete(Items.OnlySelected());
                    db.CommitTransaction();
                }
            }
        }
    }

    public abstract class BaseEditorViewModel<T> : ReactiveObject where T : IEntity
    {
        [Reactive] public bool IsOpen { get; set; }
        [Reactive] public bool IsReadOnly { get; set; }
        [Reactive] public T EditingItem { get; set; }
        public ReactiveCommand<Unit, int> Save { get; set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; set; }

        public BaseEditorViewModel()
        {
            Save = ReactiveCommand.Create<int>(() =>
            {
                var result = SaveImpl();
                IsOpen = false;
                return result;
            },
            this.WhenAnyValue(p => p.IsReadOnly).Select(p => p == false));
            Save.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            Cancel = ReactiveCommand.Create(() =>
            {
                IsOpen = false;
            });

        }
        public virtual void Init(T entity)
        {
            EditingItem = entity;
            IsOpen = true;
        }

        protected virtual int SaveImpl()
        {
            return EditingItem.ID;

        }
    }
    public abstract class BaseGroupEditorViewModel<T> : ReactiveObject where T : IEntity, IEntityWithGroup
    {
        [Reactive] public bool IsOpen { get; set; }
        [Reactive] public bool IsReadOnly { get; set; }
        protected IEnumerable<T> Items;
        [Reactive] public List<string> Groups { get; set; }
        [Reactive] public string Group { get; set; }
        public ReactiveCommand<Unit, Unit> Save { get; set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; set; }

        public BaseGroupEditorViewModel()
        {
            Save = ReactiveCommand.Create(() =>
            {
                SaveImpl();
                IsOpen = false;
            }, this.WhenAnyValue(p => p.IsReadOnly).Select(p => p == false));
            Save.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            Cancel = ReactiveCommand.Create(() => { IsOpen = false; });

        }
        public virtual void Init(IEnumerable<T> items)
        {
            Items = items;
            IsOpen = true;
        }

        protected virtual void SaveImpl()
        {            

        }
    }
}
