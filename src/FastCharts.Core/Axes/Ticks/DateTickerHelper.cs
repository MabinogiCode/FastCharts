using System;

namespace FastCharts.Core.Axes.Ticks;

internal static class DateTickerHelper
{
    public static void ChooseUnit(double days, out TimeUnit unit, out int step)
    {
        if (days <= (1.0 / 24.0))
        {
            unit = TimeUnit.Minute;
            step = 5;
            return;
        }
        if (days <= 2.0)
        {
            unit = TimeUnit.Hour;
            step = 1;
            return;
        }
        if (days <= 10.0)
        {
            unit = TimeUnit.Hour;
            step = 6;
            return;
        }
        if (days <= 40.0)
        {
            unit = TimeUnit.Day;
            step = 1;
            return;
        }
        if (days <= 200.0)
        {
            unit = TimeUnit.Day;
            step = 7;
            return;
        }
        if (days <= 800.0)
        {
            unit = TimeUnit.Month;
            step = 1;
            return;
        }
        if (days <= 3650.0)
        {
            unit = TimeUnit.Month;
            step = 3;
            return;
        }
        unit = TimeUnit.Year;
        step = 1;
    }

    public static DateTime Align(DateTime t, TimeUnit unit, int step)
    {
        return unit switch
        {
            TimeUnit.Second => new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, ((t.Second / step) * step), DateTimeKind.Local),
            TimeUnit.Minute => new DateTime(t.Year, t.Month, t.Day, t.Hour, ((t.Minute / step) * step), 0, DateTimeKind.Local),
            TimeUnit.Hour => new DateTime(t.Year, t.Month, t.Day, ((t.Hour / step) * step), 0, 0, DateTimeKind.Local),
            TimeUnit.Day => AlignDay(t, step),
            TimeUnit.Month => new DateTime(t.Year, (((t.Month - 1) / step) * step) + 1, 1, 0, 0, 0, DateTimeKind.Local),
            TimeUnit.Year => new DateTime(((t.Year / step) * step), 1, 1, 0, 0, 0, DateTimeKind.Local),
            _ => t
        };
    }

    public static DateTime Add(DateTime t, TimeUnit unit, int step)
    {
        return unit switch
        {
            TimeUnit.Second => t.AddSeconds(step),
            TimeUnit.Minute => t.AddMinutes(step),
            TimeUnit.Hour => t.AddHours(step),
            TimeUnit.Day => t.AddDays(step),
            TimeUnit.Month => t.AddMonths(step),
            TimeUnit.Year => t.AddYears(step),
            _ => t
        };
    }

    public static double ClampOADate(double oa)
    {
        if (oa < DateTime.MinValue.ToOADate())
        {
            return DateTime.MinValue.ToOADate();
        }
        if (oa > DateTime.MaxValue.ToOADate())
        {
            return DateTime.MaxValue.ToOADate();
        }
        return oa;
    }

    private static DateTime AlignDay(DateTime t, int step)
    {
        var d0 = new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Local);
        var dayIndex = Math.Max(0, t.Day - 1);
        var alignedDayIndex = (dayIndex / step) * step;
        alignedDayIndex = Math.Min(alignedDayIndex, DateTime.DaysInMonth(t.Year, t.Month) - 1);
        return d0.AddDays(alignedDayIndex);
    }
}