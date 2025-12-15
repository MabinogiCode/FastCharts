using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes.Ticks
{
    /// <summary>
    /// Ticker for logarithmic axes that generates major ticks at powers of 10
    /// and minor ticks at intermediate values (2, 3, 4, 5, 6, 7, 8, 9)
    /// </summary>
    public sealed class LogTicker : ITicker<double>
    {
        private readonly double _logBase;

        /// <summary>
        /// Creates a logarithmic ticker
        /// </summary>
        /// <param name="logBase">Base for logarithmic ticks (default: 10)</param>
        public LogTicker(double logBase = 10.0)
        {
            if (logBase <= 0 || Math.Abs(logBase - 1.0) < 1e-10)
            {
                throw new ArgumentException("Logarithm base must be positive and not equal to 1");
            }

            _logBase = logBase;
        }

        /// <summary>
        /// Generates major ticks at powers of the logarithm base
        /// </summary>
        /// <param name="range">Data range</param>
        /// <param name="approxStep">Approximate step size (not used for log scale)</param>
        /// <returns>List of major tick values</returns>
        public IReadOnlyList<double> GetTicks(FRange range, double approxStep)
        {
            var ticks = new List<double>();

            if (range.Min <= 0 || range.Max <= 0 || range.Min >= range.Max)
            {
                return ticks;
            }

            // Calculate the range in log space
            var logMin = Math.Log(range.Min, _logBase);
            var logMax = Math.Log(range.Max, _logBase);

            // Find the first and last powers to include
            var startPower = (int)Math.Floor(logMin);
            var endPower = (int)Math.Ceiling(logMax);

            // Limit the number of decades to prevent too many ticks
            const int maxDecades = 10;
            if (endPower - startPower > maxDecades)
            {
                // Reduce density by stepping by 2 or more powers
                var step = Math.Max(1, (endPower - startPower) / maxDecades);
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
                // Standard case: one tick per power
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
        /// Generates minor ticks between major ticks for better granularity
        /// For base-10: generates ticks at 2, 3, 4, 5, 6, 7, 8, 9 × 10^n
        /// </summary>
        /// <param name="range">Data range</param>
        /// <param name="majorTicks">Major tick values</param>
        /// <returns>List of minor tick values</returns>
        public IReadOnlyList<double> GetMinorTicks(FRange range, IReadOnlyList<double> majorTicks)
        {
            var minorTicks = new List<double>();

            if (range.Min <= 0 || range.Max <= 0 || majorTicks == null || majorTicks.Count == 0)
            {
                return minorTicks;
            }

            // Generate minor tick multipliers based on base
            var multipliers = GenerateMinorMultipliers();

            // For each decade (between consecutive powers), add minor ticks
            var logMin = Math.Log(range.Min, _logBase);
            var logMax = Math.Log(range.Max, _logBase);

            var startPower = (int)Math.Floor(logMin);
            var endPower = (int)Math.Ceiling(logMax);

            for (var power = startPower; power <= endPower; power++)
            {
                var baseValue = Math.Pow(_logBase, power);

                foreach (var multiplier in multipliers)
                {
                    var minorValue = baseValue * multiplier;

                    // Only include if within range and not already a major tick
                    if (minorValue >= range.Min && minorValue <= range.Max)
                    {
                        // Check if this is not already a major tick
                        var isMajorTick = majorTicks.Any(major => Math.Abs(major - minorValue) < major * 1e-10);
                        if (!isMajorTick)
                        {
                            minorTicks.Add(minorValue);
                        }
                    }
                }
            }

            return minorTicks.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Generates appropriate multipliers for minor ticks based on the logarithm base
        /// </summary>
        private double[] GenerateMinorMultipliers()
        {
            if (Math.Abs(_logBase - 10.0) < 1e-10)
            {
                // For base-10: 2, 3, 4, 5, 6, 7, 8, 9
                return new[] { 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };
            }
            else if (Math.Abs(_logBase - 2.0) < 1e-10)
            {
                // For base-2: 1.5 (between 1 and 2)
                return new[] { 1.5 };
            }
            else
            {
                // For other bases: use geometric subdivision
                var count = Math.Min(8, (int)Math.Ceiling(_logBase));
                var multipliers = new List<double>();

                for (var i = 2; i < _logBase; i++)
                {
                    if (multipliers.Count < count)
                    {
                        multipliers.Add(i);
                    }
                }

                return multipliers.ToArray();
            }
        }

        /// <summary>
        /// Gets the logarithm base used by this ticker
        /// </summary>
        public double LogBase => _logBase;
    }
}