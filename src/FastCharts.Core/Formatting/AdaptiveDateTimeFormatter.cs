using System;
using System.Globalization;

namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Adaptive DateTime formatter that chooses a format string based on the visible span.
    /// Caller provides the total visible span in days to help decide.
    /// </summary>
    public sealed class AdaptiveDateTimeFormatter : IDateTimeFormatter
    {
        public double VisibleSpanDaysHint { get; set; } = 1.0; // can be set by renderer before formatting ticks

        public string Format(DateTime value)
        {
            double d = VisibleSpanDaysHint;
            if (d <= 1.0 / 24.0) // < 1h
            {
                return value.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            }
            if (d <= 2.0) // <= 2 days
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
}
