using System;
using System.Collections.Generic;

namespace FastCharts.Core.Series;

/// <summary>
/// Builds a histogram <see cref="BarSeries"/> from raw values, with automatic bin
/// sizing. Bins are contiguous (bar width = bin width) and each bar's Y is the count
/// of values in that bin. Pure and side-effect free, so it is unit-testable on its own.
/// </summary>
public static class HistogramBuilder
{
    /// <summary>
    /// Computes evenly spaced bins over the value range and returns a ready-to-add
    /// <see cref="BarSeries"/> (bars centered on each bin, width = bin width, Y = count).
    /// </summary>
    /// <param name="values">Raw sample values. NaN and infinities are ignored.</param>
    /// <param name="binCount">
    /// Number of bins. When null or &lt;= 0, Sturges' rule picks a sensible count.
    /// </param>
    /// <returns>A bar series representing the histogram (empty if no finite values).</returns>
    public static BarSeries Build(IEnumerable<double> values, int? binCount = null)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var samples = new List<double>();
        foreach (var v in values)
        {
            if (!double.IsNaN(v) && !double.IsInfinity(v))
            {
                samples.Add(v);
            }
        }

        var series = new BarSeries { Baseline = 0.0 };
        if (samples.Count == 0)
        {
            return series;
        }

        var min = samples[0];
        var max = samples[0];
        for (var i = 1; i < samples.Count; i++)
        {
            if (samples[i] < min)
            {
                min = samples[i];
            }

            if (samples[i] > max)
            {
                max = samples[i];
            }
        }

        var range = max - min;
        if (range <= 0)
        {
            // All values identical: a single unit-wide bar holding every sample.
            series.Width = 1.0;
            series.Data.Add(new BarPoint(min, samples.Count));
            return series;
        }

        var bins = (binCount.HasValue && binCount.Value > 0) ? binCount.Value : SturgesBinCount(samples.Count);
        var binWidth = range / bins;
        var counts = new int[bins];
        for (var i = 0; i < samples.Count; i++)
        {
            var idx = (int)((samples[i] - min) / binWidth);
            if (idx < 0)
            {
                idx = 0;
            }
            else if (idx >= bins)
            {
                idx = bins - 1; // the maximum value falls into the last bin
            }

            counts[idx]++;
        }

        series.Width = binWidth;
        for (var b = 0; b < bins; b++)
        {
            var center = min + ((b + 0.5) * binWidth);
            series.Data.Add(new BarPoint(center, counts[b]));
        }

        return series;
    }

    /// <summary>
    /// Sturges' rule: <c>ceil(log2(n) + 1)</c>, clamped to at least one bin.
    /// </summary>
    private static int SturgesBinCount(int n)
    {
        if (n <= 1)
        {
            return 1;
        }

        var k = (int)Math.Ceiling(Math.Log(n, 2) + 1);
        return k < 1 ? 1 : k;
    }
}
