using System;

namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Formats DateTime axis tick labels from OADate values.
    /// </summary>
    public interface IDateTimeFormatter
    {
        string Format(DateTime value);
    }
}
