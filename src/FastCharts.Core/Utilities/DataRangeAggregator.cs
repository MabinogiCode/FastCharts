using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.Utilities
{
    /// <summary>
    /// Aggregates data ranges across a collection of series implementing ISeriesRangeProvider.
    /// </summary>
    internal static class DataRangeAggregator
    {
        public static DataRangeAggregatorResult Aggregate(IEnumerable<SeriesBase> series)
        {
            var hasX = false; double xMin = 0, xMax = 0;
            var hasPrimary = false; double pyMin = 0, pyMax = 0;
            var hasSecondary = false; double syMin = 0, syMax = 0;

            foreach (var s in series)
            {
                if (s == null || s.IsEmpty || !s.IsVisible)
                {
                    continue;
                }
                if (s is ISeriesRangeProvider rp && rp.TryGetRanges(out var xr, out var yr))
                {
                    // X aggregate
                    if (!hasX)
                    {
                        hasX = true;
                        xMin = xr.Min;
                        xMax = xr.Max;
                    }
                    else
                    {
                        if (xr.Min < xMin)
                        {
                            xMin = xr.Min;
                        }
                        if (xr.Max > xMax)
                        {
                            xMax = xr.Max;
                        }
                    }

                    // Y aggregate (primary vs secondary)
                    if (s.YAxisIndex == 1)
                    {
                        if (!hasSecondary)
                        {
                            hasSecondary = true;
                            syMin = yr.Min;
                            syMax = yr.Max;
                        }
                        else
                        {
                            if (yr.Min < syMin)
                            {
                                syMin = yr.Min;
                            }
                            if (yr.Max > syMax)
                            {
                                syMax = yr.Max;
                            }
                        }
                    }
                    else
                    {
                        if (!hasPrimary)
                        {
                            hasPrimary = true;
                            pyMin = yr.Min;
                            pyMax = yr.Max;
                        }
                        else
                        {
                            if (yr.Min < pyMin)
                            {
                                pyMin = yr.Min;
                            }
                            if (yr.Max > pyMax)
                            {
                                pyMax = yr.Max;
                            }
                        }
                    }
                }
            }

            return new DataRangeAggregatorResult(hasX, hasPrimary, hasSecondary, xMin, xMax, pyMin, pyMax, syMin, syMax);
        }
    }
}
