using System.Collections.Generic;

using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Area series: fills the area between the curve and a baseline (default: 0.0).
    /// Inherits LineSeries data/shape so renderers can draw the outline as usual.
    /// </summary>
    public sealed class AreaSeries : LineSeries
    {
        /// <summary>
        /// Baseline value in data units. The area is filled between the line and this baseline.
        /// </summary>
        public double Baseline { get; set; }

        /// <summary>
        /// Fill opacity [0..1]. Renderer will use the series color with this alpha factor.
        /// </summary>
        public double FillOpacity { get; set; }

        public AreaSeries()
            : base(new List<PointD>())
        {
            this.Baseline = 0.0;
            this.FillOpacity = 0.35; // 35% default
        }

        public AreaSeries(IEnumerable<PointD> points)
            : base(points)
        {
            this.Baseline = 0.0;
            this.FillOpacity = 0.35;
        }
    }
}
