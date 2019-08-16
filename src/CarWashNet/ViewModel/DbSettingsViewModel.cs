using CarWashNet.Applications;
using CarWashNet.Domain.Managers;
using CarWashNet.Domain.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.ViewModel
{
    public class DbSettingsViewModel : ReactiveObject
    {
        [Reactive] public double WorkerDayPercent { get; set; }
        [Reactive] public double WorkerNightPercent { get; set; }
        [Reactive] public bool WorkerPayWithDiscount { get; set; }
        [Reactive] public string OrganizationPrintCaption { get; set; }
        [Reactive] public string OrderPrintCaption { get; set; }

        public ReactiveCommand<Unit, Unit> Save { get; set; }
        public ReactiveCommand<Unit, Unit> Cancel { get; set; }

        [Reactive] public bool NeedSave { get; private set; }

        public ReactiveCommand<Unit, Unit> LoadTestData { get; set; }
        public ReactiveCommand<Unit, Unit> ResetDb { get; set; }

        public DbSettingsViewModel()
        {
            Save = ReactiveCommand.Create(() =>
            {
                SaveSettings();
                
            }, this.WhenAnyValue(p => p.NeedSave).Select(p => p));
            Save.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            Cancel = ReactiveCommand.Create(() =>
            {
                LoadSettings();
            });
            Cancel.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));

            LoadTestData = ReactiveCommand.CreateFromTask(async () =>
            {
                var confirm = await Interactions.ShowConfirmationAsync(
                    "Заполнить базу тестовыми данными?",
                    "ВСЯ ИНФОРМАЦИЯ ИЗ БАЗЫ ДАННЫХ БУДЕТ УДАЛЕНА!",
                    "Заполнить", "Отмена");
                if (confirm == InteractionResult.Yes)
                {
                    await Interactions.StartLongTimeOperation("Загружаем тестовые данные", "Ждите...");
                    await DbService.InitializeDbWithTestDataAsync();
                    await Interactions.FinishLongTimeOperation();
                    await Interactions.ShowMessage("Готово!", "Тестовые данные загружены");
                    await Interactions.LogoutUser();
                    //LoadSettings();

                    //DialogService.LogoutUser();
                }


                //bool dialogResult = false;
                //dialogResult = await DialogService.ShowConfirmationAsync("Заполнить базу тестовыми данными?", "ВСЯ ИНФОРМАЦИЯ ИЗ БАЗЫ ДАННЫХ БУДЕТ УДАЛЕНА!", "Заполнить", "Отмена");
                //if (dialogResult)
                //{
                //    await DialogService.ShowDefaultProgressAsync("Загружаем тестовые данные", "Ждите...");
                //    await DbService.InitializeDbWithTestDataAsync();
                //    DialogService.HideDefaultProgressAsync();
                //    await DialogService.ShowMessageAsync("Готово!", "Тестовые данные загружены");
                //    LoadSettings();
                //    DialogService.LogoutUser();
                //}

            });
            LoadTestData.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));
            ResetDb = ReactiveCommand.CreateFromTask(async () =>
            {
                var confirm = await Interactions.ShowConfirmationAsync(
                    "Сбросить БД в начальное состояние?",
                    "ВСЯ ИНФОРМАЦИЯ ИЗ БАЗЫ ДАННЫХ БУДЕТ УДАЛЕНА!",
                    "Заполнить", "Отмена");
                if (confirm == InteractionResult.Yes)
                {
                    await Interactions.StartLongTimeOperation("Пересоздаем бд", "Ждите...");
                    await DbService.ReInitializeDbAsync();
                    await Interactions.FinishLongTimeOperation();
                    await Interactions.ShowMessage("Готово!", "БД сброшена в начальное состояние");
                    await Interactions.LogoutUser();
                    //LoadSettings();

                    //DialogService.LogoutUser();
                }

                //bool dialogResult = false;
                //dialogResult = await DialogService.ShowConfirmationAsync("Сбросить БД в начальное состояние?", "ВСЯ ИНФОРМАЦИЯ ИЗ БАЗЫ ДАННЫХ БУДЕТ УДАЛЕНА!", "Сбросить", "Отмена");
                //if (dialogResult)
                //{
                //    await DialogService.ShowDefaultProgressAsync("Пересоздаем бд", "Ждите...");
                //    await DbService.ReInitializeDbAsync();
                //    DialogService.HideDefaultProgressAsync();
                //    await DialogService.ShowMessageAsync("Готово!", "БД сброшена в начальное состояние");
                //    LoadSettings();
                //    DialogService.LogoutUser();
                //}

            });
            ResetDb.ThrownExceptions.Subscribe(async ex => await Interactions.ShowError(ex.Message));



            this.WhenAnyValue(
                p => p.WorkerDayPercent,
                p => p.WorkerNightPercent,
                p => p.WorkerPayWithDiscount,
                p => p.OrganizationPrintCaption,
                p => p.OrderPrintCaption)                
                .Subscribe(p => NeedSave = true);


        }
        
        public void LoadSettings()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new DbSettingManager(db);
                WorkerDayPercent = manager.WorkerDayPercent * 100;
                WorkerNightPercent = manager.WorkerNightPercent * 100;
                WorkerPayWithDiscount = manager.WorkerPayWithDiscount;
                OrganizationPrintCaption = manager.OrganizationPrintCaption;
                OrderPrintCaption = manager.OrderPrintCaption;
            }
            NeedSave = false;
        }
        public void SaveSettings()
        {
            using (var db = DbService.GetDb())
            {
                var manager = new DbSettingManager(db);
                manager.WorkerDayPercent = WorkerDayPercent / 100;
                manager.WorkerNightPercent = WorkerNightPercent / 100;
                manager.WorkerPayWithDiscount = WorkerPayWithDiscount;
                manager.OrganizationPrintCaption = OrganizationPrintCaption;
                manager.OrderPrintCaption = OrderPrintCaption;
            }
            NeedSave = false;
        }
    }
}
