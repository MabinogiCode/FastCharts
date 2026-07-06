using FastCharts.Core.Series;

namespace FastCharts.Core.Finance
{
    /// <summary>
    /// Output of <see cref="Indicators.BollingerBands(System.Collections.Generic.IReadOnlyList{FastCharts.Core.Primitives.PointD}, int, double)"/>:
    /// the middle moving average and the ±kσ envelope band.
    /// </summary>
    public sealed class BollingerBandsResult
    {
        public BollingerBandsResult(LineSeries middle, BandSeries band)
        {
            Middle = middle;
            Band = band;
        }

        /// <summary>
        /// Middle line (simple moving average)
        /// </summary>
        public LineSeries Middle { get; }

        /// <summary>
        /// Envelope band between -kσ and +kσ
        /// </summary>
        public BandSeries Band { get; }
    }
}
