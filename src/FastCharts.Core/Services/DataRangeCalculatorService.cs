using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Default implementation of data range calculator service.
    /// </summary>
    public class DataRangeCalculatorService : IDataRangeCalculatorService
    {
        /// <summary>
        /// Calculates data ranges from the provided series.
        /// </summary>
        public DataRangeCalculationResult CalculateDataRanges(IEnumerable<SeriesBase> series)
        {
            if (series == null)
            {
                return DataRangeCalculationResult.Empty;
            }

            var visibleSeries = series.Where(s => s.IsVisible).ToList();
            if (visibleSeries.Count == 0)
            {
                return DataRangeCalculationResult.Empty;
            }

            var result = DataRangeAggregator.Aggregate(visibleSeries);
            if (!result.HasX && !result.HasPrimary && !result.HasSecondary)
            {
                return DataRangeCalculationResult.Empty;
            }

            return new DataRangeCalculationResult(
                true,
                result.HasX,
                result.HasPrimary,
                result.HasSecondary,
                result.HasX ? DataRangeValidator.EnsureValidRange(result.XMin, result.XMax) : new FRange(0, 1),
                result.HasPrimary ? DataRangeValidator.EnsureValidRange(result.PrimaryYMin, result.PrimaryYMax) : new FRange(0, 1),
                result.HasSecondary ? DataRangeValidator.EnsureValidRange(result.SecondaryYMin, result.SecondaryYMax) : new FRange(0, 1)
            );
        }
    }
}