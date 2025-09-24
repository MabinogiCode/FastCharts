using System.Collections.Generic;
using System.Linq;
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
            get { return Data == null || Data.Count == 0; }
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

        public FRange GetXRange()
        {
            if (IsEmpty) { return new FRange(0, 0); }
            var min = Data.Min(p => p.X);
            var max = Data.Max(p => p.X);
            return new FRange(min, max);
        }

        public FRange GetYRange()
        {
            if (IsEmpty) { return new FRange(0, 0); }
            var min = Data.Min(p => p.Y);
            var max = Data.Max(p => p.Y);
            return new FRange(min, max);
        }
    }
}
