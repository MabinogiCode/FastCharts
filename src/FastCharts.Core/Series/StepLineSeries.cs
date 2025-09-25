using System.Collections.Generic;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    public enum StepMode
    {
        Before, // horizontal then vertical (left step)
        After   // vertical then horizontal (right step)
    }

    public sealed class StepLineSeries : LineSeries
    {
        public StepMode Mode { get; set; } = StepMode.Before;

        public StepLineSeries() : base() { }
        public StepLineSeries(IEnumerable<PointD> points) : base(points) { }
    }
}
