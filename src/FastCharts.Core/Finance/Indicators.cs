using System;
using System.Collections.Generic;

using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.Finance
{
    /// <summary>
    /// Financial indicators producing ready-to-add chart series (P3-FIN-INDICATORS).
    /// KISS: <c>model.AddSeries(Indicators.Sma(prices, 20));</c>
    /// All overloads accepting <see cref="OhlcSeries"/> use the Close price.
    /// </summary>
    public static class Indicators
    {
        /// <summary>
        /// Simple Moving Average. The output starts at the first index where a full
        /// window is available (period - 1), so no partial averages are emitted.
        /// </summary>
        /// <param name="source">Input points (sorted by X)</param>
        /// <param name="period">Window size (≥ 1)</param>
        /// <param name="title">Optional title (default "SMA {period}")</param>
        /// <returns>Line series of the moving average</returns>
        public static LineSeries Sma(IReadOnlyList<PointD> source, int period, string? title = null)
        {
            ValidateInput(source, period);

            var result = new List<PointD>(Math.Max(0, source.Count - period + 1));
            var sum = 0.0;

            for (var i = 0; i < source.Count; i++)
            {
                sum += source[i].Y;

                if (i >= period)
                {
                    sum -= source[i - period].Y;
                }

                if (i >= period - 1)
                {
                    result.Add(new PointD(source[i].X, sum / period));
                }
            }

            return new LineSeries(result) { Title = title ?? $"SMA {period}" };
        }

        /// <summary>
        /// Simple Moving Average over an OHLC series' Close prices
        /// </summary>
        public static LineSeries Sma(OhlcSeries source, int period, string? title = null)
        {
            return Sma(ToClosePoints(source), period, title);
        }

        /// <summary>
        /// Exponential Moving Average (smoothing factor 2 / (period + 1)),
        /// seeded with the SMA of the first window.
        /// </summary>
        /// <param name="source">Input points (sorted by X)</param>
        /// <param name="period">Window size (≥ 1)</param>
        /// <param name="title">Optional title (default "EMA {period}")</param>
        /// <returns>Line series of the exponential moving average</returns>
        public static LineSeries Ema(IReadOnlyList<PointD> source, int period, string? title = null)
        {
            ValidateInput(source, period);

            var result = new List<PointD>(Math.Max(0, source.Count - period + 1));

            if (source.Count >= period)
            {
                // Seed: SMA of the first window
                var seed = 0.0;
                for (var i = 0; i < period; i++)
                {
                    seed += source[i].Y;
                }

                seed /= period;
                result.Add(new PointD(source[period - 1].X, seed));

                var alpha = 2.0 / (period + 1);
                var ema = seed;

                for (var i = period; i < source.Count; i++)
                {
                    ema = ((source[i].Y - ema) * alpha) + ema;
                    result.Add(new PointD(source[i].X, ema));
                }
            }

            return new LineSeries(result) { Title = title ?? $"EMA {period}" };
        }

        /// <summary>
        /// Exponential Moving Average over an OHLC series' Close prices
        /// </summary>
        public static LineSeries Ema(OhlcSeries source, int period, string? title = null)
        {
            return Ema(ToClosePoints(source), period, title);
        }

        /// <summary>
        /// Bollinger Bands: middle SMA plus a band at ±k standard deviations.
        /// Add both parts to the chart:
        /// <code>
        /// var bb = Indicators.BollingerBands(prices, 20, 2);
        /// model.AddSeries(bb.Band);
        /// model.AddSeries(bb.Middle);
        /// </code>
        /// </summary>
        /// <param name="source">Input points (sorted by X)</param>
        /// <param name="period">Window size (default 20)</param>
        /// <param name="k">Standard deviation multiplier (default 2)</param>
        /// <returns>Middle line series + envelope band series</returns>
        public static BollingerBandsResult BollingerBands(IReadOnlyList<PointD> source, int period = 20, double k = 2.0)
        {
            ValidateInput(source, period);

            var middle = new List<PointD>(Math.Max(0, source.Count - period + 1));
            var band = new List<BandPoint>(Math.Max(0, source.Count - period + 1));

            for (var i = period - 1; i < source.Count; i++)
            {
                var mean = 0.0;
                for (var j = i - period + 1; j <= i; j++)
                {
                    mean += source[j].Y;
                }

                mean /= period;

                var variance = 0.0;
                for (var j = i - period + 1; j <= i; j++)
                {
                    var d = source[j].Y - mean;
                    variance += d * d;
                }

                var sigma = Math.Sqrt(variance / period);

                middle.Add(new PointD(source[i].X, mean));
                band.Add(new BandPoint(source[i].X, mean - (k * sigma), mean + (k * sigma)));
            }

            return new BollingerBandsResult(
                new LineSeries(middle) { Title = $"BB {period} middle" },
                new BandSeries(band) { Title = $"BB {period} ±{k}σ" });
        }

        /// <summary>
        /// Bollinger Bands over an OHLC series' Close prices
        /// </summary>
        public static BollingerBandsResult BollingerBands(OhlcSeries source, int period = 20, double k = 2.0)
        {
            return BollingerBands(ToClosePoints(source), period, k);
        }

        private static IReadOnlyList<PointD> ToClosePoints(OhlcSeries source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var points = new List<PointD>(source.Data.Count);
            for (var i = 0; i < source.Data.Count; i++)
            {
                points.Add(new PointD(source.Data[i].X, source.Data[i].Close));
            }

            return points;
        }

        private static void ValidateInput(IReadOnlyList<PointD> source, int period)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (period < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(period), period, "Period must be at least 1");
            }
        }
    }
}
