using CarWashNet.Applications;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Model;
using CarWashNet.Domain.Services;
using KLib.Native;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.ViewModel
{
    public class AppMenuViewModel : ReactiveObject
    {
        [Reactive] public ReactiveList<AppItem> Items { get; set; }
        [Reactive] public AppItem SelectedItem { get; set; }
        [Reactive] public object SelectedOptionsItem { get; set; }

        public ReactiveCommand<Unit, Unit> LoadItems { get; set; }

        public AppMenuViewModel()
        {
            Items = new ReactiveList<AppItem>();
            LoadItems = ReactiveCommand.Create(
                () =>
                {
                    using (var db = DbService.GetDb())
                    {
                        var manager = new AppItemManager(db);
                        var result = manager.GetUserApps(DbService.CurrentUser);
                        Items.ReplaceRange(result);
                    }
                });

            LoadItems.Execute().Subscribe(_ =>
            {
                SelectedItem = Items.FirstOrDefault(p => p.Code == GlobalService.AppSettings.LastAppCode);
                if (SelectedItem == null) SelectedItem = Items.FirstOrDefault();
            });
        }
    }


    //public class AppsMenuViewModel : ReactiveObject
    //{
    //    [Reactive] public ReactiveList<AppItem> Items { get; set; }
    //    [Reactive] public AppItem SelectedItem { get; set; }


    //    [Reactive] public ContentControl SelectedApp { get; set; }

    //    [Reactive] public HamburgerMenuIconItem SelectedOptionsItem { get; set; }


    //    public ReactiveCommand<Unit, Unit> LoadItems { get; set; }

    //    public AppsMenuViewModel()
    //    {
    //        Items = new ReactiveList<AppItem>();
    //        LoadItems = ReactiveCommand.Create(
    //            () =>
    //            {
    //                using (var db = DbService.GetDb())
    //                {
    //                    var manager = new AppItemManager(db);
    //                    var result = manager.GetUserApps(DbService.CurrentUser);
    //                    KLib.Native.Extensions.ReplaceRange(Items, result);
    //                }
    //            });
    //        this.WhenAnyValue(p => p.SelectedItem)
    //            .Where(p => p != null)
    //            .Subscribe(p =>
    //            {
    //                SelectedApp = AppRepository.GetApp(p.Code, p.Caption);

    //            });

    //        this.WhenAnyValue(p => p.SelectedOptionsItem)
    //            .Where(p => p != null)
    //            .Subscribe(p =>
    //            {
    //                SelectedApp = AppRepository.GetApp("AppSettings", p.Label);
    //                //SelectedApp = new AppSettingsControl();
    //            });

    //        LoadItems.Execute().Subscribe(async _ =>
    //        {
    //            await Task.Delay(100);
    //            SelectedItem = Items.FirstOrDefault(p => p.Code == GlobalService.AppSettings.LastAppCode);
    //            if (SelectedItem == null) SelectedItem = Items.FirstOrDefault();
    //        });
    //    }
    //}
}
