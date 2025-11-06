using System.Collections.Generic;
using FastCharts.Core.Series;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Service for calculating data ranges from series
    /// </summary>
    public interface IDataRangeCalculatorService
    {
        DataRangeCalculationResult CalculateDataRanges(IEnumerable<SeriesBase> series);
    }
}