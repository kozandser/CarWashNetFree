using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Application
{
    public static class PeriodFilterService
    {
        private static DateTime baseDate = DateTime.Today;
        public static DateTime ThisWeekStart { get; private set; } = baseDate.AddDays(-(int)baseDate.DayOfWeek + 1);
        public static DateTime ThisWeekEnd { get; private set; } = ThisWeekStart.AddDays(7).AddSeconds(-1);
        public static DateTime LastWeekStart { get; private set; } = ThisWeekStart.AddDays(-7);
        public static DateTime LastWeekEnd { get; private set; } = ThisWeekStart.AddSeconds(-1);
        public static DateTime ThisMonthStart { get; private set; } = baseDate.AddDays(1 - baseDate.Day);
        public static DateTime ThisMonthEnd { get; private set; } = ThisMonthStart.AddMonths(1).AddSeconds(-1);
        public static DateTime LastMonthStart { get; private set; } = ThisMonthStart.AddMonths(-1);
        public static DateTime LastMonthEnd { get; private set; } = ThisMonthStart.AddSeconds(-1);
        public static DateTime ThisYearStart { get; private set; } = new DateTime(baseDate.Year, 1, 1);
        public static DateTime ThisYearEnd { get; private set; } = ThisYearStart.AddYears(1).AddSeconds(-1);
        public static DateTime LastYearStart { get; private set; } = new DateTime(baseDate.Year - 1, 1, 1);
        public static DateTime LastYearEnd { get; private set; } = LastYearStart.AddYears(1).AddSeconds(-1);

        public static PeriodFilterItem Today => new PeriodFilterItem()
        {
            Caption = "Сегодня",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddDays(1).AddSeconds(-1)
        };
        public static PeriodFilterItem Yesterday =>
            new PeriodFilterItem()
            {
                Caption = "Вчера",
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today.AddSeconds(-1)
            };
        public static PeriodFilterItem Last7Days =>
            new PeriodFilterItem()
            {
                Caption = "Последние 7 дней",
                StartDate = DateTime.Today.AddDays(-6),
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1)
            };
        public static PeriodFilterItem Last30Days =>
            new PeriodFilterItem()
            {
                Caption = "Последние 30 дней",
                StartDate = DateTime.Today.AddDays(-29),
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1)
            };
        public static PeriodFilterItem Last100Days =>
            new PeriodFilterItem()
            {
                Caption = "Последние 100 дней",
                StartDate = DateTime.Today.AddDays(-99),
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1)
            };
        public static PeriodFilterItem ThisWeek =>
            new PeriodFilterItem()
            {
                Caption = "Эта неделя",                
                StartDate = ThisWeekStart,
                EndDate = ThisWeekEnd
            };
        public static PeriodFilterItem LastWeek =>
            new PeriodFilterItem()
            {
                Caption = "Прошлая неделя",
                StartDate = LastWeekStart,
                EndDate = LastWeekEnd
            };
        public static PeriodFilterItem ThisMonth =>
            new PeriodFilterItem()
            {
                Caption = "Этот месяц",
                StartDate = ThisMonthStart,
                EndDate = ThisMonthEnd
            };
        public static PeriodFilterItem LastMonth =>
            new PeriodFilterItem()
            {
                Caption = "Прошлый месяц",
                StartDate = LastMonthStart,
                EndDate = LastMonthEnd
            };
        public static PeriodFilterItem ThisYear =>
            new PeriodFilterItem()
            {
                Caption = "Этот год",
                StartDate = ThisYearStart,
                EndDate = ThisYearEnd
            };
        public static PeriodFilterItem LastYear =>
            new PeriodFilterItem()
            {
                Caption = "Прошлый год",
                StartDate = LastYearStart,
                EndDate = LastYearEnd
            };
        public static PeriodFilterItem ManualPeriod =>
            new PeriodFilterItem()
            {
                IsManual = true,
                Caption = "Свой период",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1).AddSeconds(-1)
            };

        public static List<PeriodFilterItem> GetDefaultPeriods()
        {
            return new List<PeriodFilterItem>()
            {
                Today,
                Yesterday,
                Last7Days,
                Last30Days,
                Last100Days,
                ThisWeek,
                LastWeek,
                ThisMonth,
                LastMonth,
                ThisYear,
                LastYear,
                ManualPeriod
            };
        }
    }

    public class PeriodFilterItem : ReactiveObject
    {
        public string Caption { get; set; }
        public bool IsManual { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PeriodFilter : ReactiveObject
    {
        public List<PeriodFilterItem> Periods { get; set; }
        [Reactive] public PeriodFilterItem SelectedPeriod { get; set; }

        [Reactive] public DateTime StartDate { get; set; }
        [Reactive] public DateTime EndDate { get; set; }

        private bool isPeriodChangedSuppressed = false;
        [Reactive] public bool IsManualPeriod { get; set; }

        public Subject<Unit> PeriodChanged { get; set; }

        public PeriodFilter()
        {
            PeriodChanged = new Subject<Unit>();
            Periods = PeriodFilterService.GetDefaultPeriods();
            SelectedPeriod = Periods[0];            

            this.WhenAnyValue(p => p.SelectedPeriod)
                .Where(p => p != null)
                .Subscribe(p =>
                {
                    isPeriodChangedSuppressed = true;
                    if (p.IsManual)
                    {
                        IsManualPeriod = true;
                    }
                    else
                    {
                        StartDate = p.StartDate;
                        EndDate = p.EndDate;
                        IsManualPeriod = false;
                    }
                    isPeriodChangedSuppressed = false;
                    PeriodChanged.OnNext(Unit.Default);
                });

            this.WhenAnyValue(p => p.StartDate, p => p.EndDate)
                .Subscribe(p => 
                {
                    if (isPeriodChangedSuppressed == false) PeriodChanged.OnNext(Unit.Default);
                });
        }

        public void SetManualDate(DateTime date)
        {
            isPeriodChangedSuppressed = true;
            StartDate = date;
            EndDate = date.AddDays(1).AddSeconds(-1);
            SelectedPeriod = Periods.FirstOrDefault(p => p.IsManual);
            if(SelectedPeriod != null)
            {
                PeriodChanged.OnNext(Unit.Default);                
            }
            isPeriodChangedSuppressed = false;
        }
    }
}
