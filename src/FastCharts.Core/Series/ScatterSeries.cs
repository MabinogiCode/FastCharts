using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Scatter series: renders discrete markers at data points.
    /// Inherits LineSeries to integrate seamlessly with the existing series collection.
    /// The line path is intentionally NOT drawn by the renderer for this derived type.
    /// </summary>
    public sealed class ScatterSeries : LineSeries
    {
        /// <summary>
        /// Marker diameter in pixels (renderer may interpret as size).
        /// </summary>
        public double MarkerSize { get; set; }

        /// <summary>
        /// Marker shape (Circle/Square/Triangle).
        /// </summary>
        public MarkerShape MarkerShape { get; set; }

        public ScatterSeries()
            : base(new List<PointD>())
        {
            this.MarkerSize = 5.0;
            this.MarkerShape = MarkerShape.Circle;
        }

        public ScatterSeries(IEnumerable<PointD> points)
            : base(points)
        {
            this.MarkerSize = 5.0;
            this.MarkerShape = MarkerShape.Circle;
        }
    }
}
