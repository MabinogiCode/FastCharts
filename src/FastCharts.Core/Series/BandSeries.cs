using System.Collections.Generic;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Band series: fills the area between two Y values (YLow/YHigh) along X.
    /// Typical use: confidence intervals, envelopes, Bollinger bands, etc.
    /// </summary>
    public sealed class BandSeries
    {
        /// <summary>
        /// Points with (X, YLow, YHigh). X should be non-decreasing for a proper path.
        /// </summary>
        public IList<BandPoint> Data { get; }

        /// <summary>
        /// Outline stroke thickness (px). If 0, no outline is drawn.
        /// </summary>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Fill opacity [0..1] for the band region.
        /// </summary>
        public double FillOpacity { get; set; }

        public bool IsEmpty
        {
            get
            {
                return Data == null || Data.Count == 0;
            }
        }

        public BandSeries()
        {
            Data = new List<BandPoint>();
            StrokeThickness = 1.5;
            FillOpacity = 0.25;
        }

        public BandSeries(IEnumerable<BandPoint> points)
        {
            Data = new List<BandPoint>(points);
            StrokeThickness = 1.5;
            FillOpacity = 0.25;
        }
    }
}
