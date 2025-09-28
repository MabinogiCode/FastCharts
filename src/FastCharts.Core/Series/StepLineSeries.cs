using System.Collections.Generic;

using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    public sealed class StepLineSeries : LineSeries
    {
        public StepMode Mode { get; set; } = StepMode.Before;

        public StepLineSeries() : base()
        {
        }

        public StepLineSeries(IEnumerable<PointD> points) : base(points)
        {
        }
    }
}
