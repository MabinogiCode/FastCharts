using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks
{
    /// <summary>
    /// Ticker implementation for logarithmic axes that generates ticks at powers of the base
    /// and intermediate linear subdivisions within each decade.
    /// </summary>
    public sealed class LogarithmicTicker : ITicker<double>
    {
        private readonly double _logBase;
        private readonly double[] _linearSubdivisions;

        public LogarithmicTicker(double logBase = 10.0)
        {
            if (logBase <= 0 || double.IsNaN(logBase) || double.IsInfinity(logBase))
            {
                throw new ArgumentException("Log base must be positive and finite", nameof(logBase));
            }

            _logBase = logBase;

            // For base 10: use subdivisions 1, 2, 3, 4, 5, 6, 7, 8, 9
            // For other bases: use fewer subdivisions to avoid clutter
            if (Math.Abs(_logBase - 10.0) < 1e-10)
            {
                _linearSubdivisions = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
            else
            {
                _linearSubdivisions = new double[] { 1, 2, 5 }; // Common logarithmic subdivisions
            }
        }

        /// <summary>
        /// Generates major ticks (powers of the base) for the logarithmic axis.
        /// </summary>
        /// <param name="range">Visible range of the axis</param>
        /// <param name="approxStep">Approximate step size (used as a hint)</param>
        /// <returns>Collection of major tick positions</returns>
        public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
        {
            var ticks = new List<double>();

            if (range.Min <= 0)
            {
                return ticks; // Can't generate log ticks for non-positive ranges
            }

            var minLog = Math.Log(range.Min, _logBase);
            var maxLog = Math.Log(range.Max, _logBase);

            var startPower = Math.Floor(minLog);
            var endPower = Math.Ceiling(maxLog);

            // Use approxStep to estimate desired number of ticks
            var estimatedTickCount = (int)Math.Max(1, (range.Max - range.Min) / Math.Max(approxStep, 0.1));
            var maxDecades = Math.Max(estimatedTickCount, 20);

            if (endPower - startPower > maxDecades)
            {
                var step = Math.Ceiling((endPower - startPower) / maxDecades);
                for (var power = startPower; power <= endPower; power += step)
                {
                    var tickValue = Math.Pow(_logBase, power);
                    if (tickValue >= range.Min && tickValue <= range.Max)
                    {
                        ticks.Add(tickValue);
                    }
                }
            }
            else
            {
                // Generate ticks for each power of the base
                for (var power = startPower; power <= endPower; power++)
                {
                    var tickValue = Math.Pow(_logBase, power);
                    if (tickValue >= range.Min && tickValue <= range.Max)
                    {
                        ticks.Add(tickValue);
                    }
                }
            }

            return ticks;
        }

        /// <summary>
        /// Generates minor ticks (linear subdivisions within each decade) for the logarithmic axis.
        /// </summary>
        /// <param name="range">Visible range of the axis</param>
        /// <param name="majorTicks">Major tick positions for reference</param>
        /// <returns>Collection of minor tick positions</returns>
        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            var minorTicks = new List<double>();

            if (range.Min <= 0)
            {
                return minorTicks;
            }

            var minLog = Math.Log(range.Min, _logBase);
            var maxLog = Math.Log(range.Max, _logBase);

            var startPower = Math.Floor(minLog);
            var endPower = Math.Ceiling(maxLog);

            // Limit minor ticks to reasonable number of decades
            var spanDecades = endPower - startPower;
            if (spanDecades > 5)
            {
                return minorTicks; // Too many decades, skip minor ticks to avoid clutter
            }

            var minorTicksSet = new HashSet<double>();

            for (var power = startPower; power <= endPower; power++)
            {
                var basePower = Math.Pow(_logBase, power);

                foreach (var subdivision in _linearSubdivisions)
                {
                    var tickValue = basePower * subdivision;

                    // Only add if within range and not overlapping with major ticks
                    if (tickValue >= range.Min && tickValue <= range.Max)
                    {
                        var isMajorTick = majorTicks.Any(major => Math.Abs(major - tickValue) < tickValue * 1e-10);
                        if (!isMajorTick)
                        {
                            minorTicksSet.Add(tickValue);
                        }
                    }
                }
            }

            return minorTicksSet.OrderBy(t => t).ToList();
        }
    }
}