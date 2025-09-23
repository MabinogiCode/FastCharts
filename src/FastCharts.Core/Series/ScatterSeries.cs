using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    public sealed class ScatterSeries : SeriesBase
    {
        public IList<PointD> Data { get; }

        /// <summary>
        /// Marker diameter in pixels.
        /// </summary>
        public double MarkerSize { get; set; }

        /// <summary>
        /// Marker shape.
        /// </summary>
        public MarkerShape MarkerShape { get; set; }

        public override bool IsEmpty
        {
            get
            {
                return Data == null || Data.Count == 0;
            }
        }

        public ScatterSeries()
        {
            Data = new List<PointD>();
            MarkerSize = 5.0;
            MarkerShape = MarkerShape.Circle;
        }

        public ScatterSeries(IEnumerable<PointD> points)
        {
            Data = new List<PointD>(points);
            MarkerSize = 5.0;
            MarkerShape = MarkerShape.Circle;
        }
    }
}
