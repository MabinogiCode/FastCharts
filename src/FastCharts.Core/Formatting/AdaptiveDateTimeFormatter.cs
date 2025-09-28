using System;
using System.Globalization;

namespace FastCharts.Core.Formatting;

public sealed class AdaptiveDateTimeFormatter : IDateTimeFormatter
{
    public double VisibleSpanDaysHint { get; set; } = 1.0;

    public string Format(DateTime value)
    {
        var d = VisibleSpanDaysHint;
        if (d <= (1.0 / 24.0))
        {
            return value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }
        if (d <= 2.0)
        {
            return value.ToString("HH:mm\nMMM d", CultureInfo.InvariantCulture);
        }
        if (d <= 40.0)
        {
            return value.ToString("MMM d", CultureInfo.InvariantCulture);
        }
        if (d <= 800.0)
        {
            return value.ToString("MMM yyyy", CultureInfo.InvariantCulture);
        }
        return value.ToString("yyyy", CultureInfo.InvariantCulture);
    }
}
