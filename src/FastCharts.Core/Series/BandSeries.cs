using System.Collections.Generic;

namespace FastCharts.Core.Series
{
    public sealed class BandSeries : SeriesBase
    {
        public IList<BandPoint> Data { get; }

        /// <summary>
        /// Fill opacity [0..1] for the band region.
        /// </summary>
        public double FillOpacity { get; set; }

        public override bool IsEmpty
        {
            get
            {
                return Data == null || Data.Count == 0;
            }
        }

        public BandSeries()
        {
            Data = new List<BandPoint>();
            // StrokeThickness inherited (default 1.5)
            FillOpacity = 0.25;
        }

        public BandSeries(IEnumerable<BandPoint> points)
        {
            Data = new List<BandPoint>(points);
            FillOpacity = 0.25;
        }
    }
}
