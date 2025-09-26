using System.Collections.Generic;
using System.Linq;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// OHLC (Open-High-Low-Close) financial bar series. Each point stores O,H,L,C at X.
    /// </summary>
    public sealed class OhlcSeries : SeriesBase
    {
        public IList<OhlcPoint> Data { get; }

        /// <summary>Width (data units) of the candle body / bar (auto-inferred if null).</summary>
        public double? Width { get; set; }

        /// <summary>Stroke thickness for wicks/borders.</summary>
        public double WickThickness { get; set; } = 1.0;

        /// <summary>Fill opacity for up candles (if using filled style).</summary>
        public double UpFillOpacity { get; set; } = 0.9;

        /// <summary>Fill opacity for down candles.</summary>
        public double DownFillOpacity { get; set; } = 0.4;

        /// <summary>If true, draw filled candle bodies; otherwise draw only outlines.</summary>
        public bool Filled { get; set; } = true;

        public override bool IsEmpty => Data == null || Data.Count == 0;

        public OhlcSeries()
        {
            Data = new List<OhlcPoint>();
        }

        public OhlcSeries(IEnumerable<OhlcPoint> points)
        {
            Data = new List<OhlcPoint>(points);
        }

        public double GetWidthFor(int index)
        {
            if (Width.HasValue) return Width.Value;
            if (Data.Count >= 2)
            {
                double minDx = double.PositiveInfinity;
                for (int i = 1; i < Data.Count; i++)
                {
                    double dx = System.Math.Abs(Data[i].X - Data[i - 1].X);
                    if (dx > 0 && dx < minDx) minDx = dx;
                }
                if (double.IsInfinity(minDx) || minDx <= 0) return 1.0;
                return minDx * 0.6; // candles narrower than bars
            }
            return 1.0;
        }

        public FastCharts.Core.Primitives.FRange GetXRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minX = Data.Min(p => p.X);
            double maxX = Data.Max(p => p.X);
            double half = GetWidthFor(0) * 0.5;
            return new FastCharts.Core.Primitives.FRange(minX - half, maxX + half);
        }

        public FastCharts.Core.Primitives.FRange GetYRange()
        {
            if (IsEmpty) return new FastCharts.Core.Primitives.FRange(0, 0);
            double minY = Data.Min(p => p.Low);
            double maxY = Data.Max(p => p.High);
            return new FastCharts.Core.Primitives.FRange(minY, maxY);
        }
    }

    public struct OhlcPoint
    {
        public double X { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }

        public OhlcPoint(double x, double open, double high, double low, double close)
        {
            X = x; Open = open; High = high; Low = low; Close = close;
        }
    }
}
