using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes
{
    /// <summary>
    /// Extensions for IAxis<double> to set visible range with guards.
    /// </summary>
    public static class AxisExtensions
    {
        public static void SetVisibleRange(this IAxis<double> axis, double min, double max)
        {
            if (axis == null)
            {
                return;
            }

            if (double.IsNaN(min) || double.IsNaN(max))
            {
                return;
            }

            if (min > max)
            {
                var t = min; min = max; max = t;
            }

            if (Math.Abs(min - max) < double.Epsilon)
            {
                var eps = (min == 0d) ? 1e-6 : Math.Abs(min) * 1e-6;
                min -= eps;
                max += eps;
            }

            axis.VisibleRange = new FRange(min, max);
        }
    }
}
