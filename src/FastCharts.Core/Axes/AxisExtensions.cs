using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Axes;

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
            (min, max) = (max, min);
        }
        if (Math.Abs(min - max) < double.Epsilon)
        {
            var eps = DoubleUtils.IsZero(min) ? 1e-6 : Math.Abs(min) * 1e-6;
            min -= eps;
            max += eps;
        }
        switch (axis)
        {
            case DateTimeAxis dta:
            {
                dta.SetVisibleRange(min, max);
                break;
            }
            case NumericAxis na:
            {
                na.SetVisibleRange(min, max);
                break;
            }
            case AxisBase ab:
            {
                ab.VisibleRange = new FRange(min, max);
                break;
            }
            default:
            {
                // Unknown axis type - attempt reflection fallback (avoid failure)
                var prop = axis.GetType().GetProperty("VisibleRange");
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        prop.SetValue(axis, new FRange(min, max));
                    }
                    catch
                    {
                        // swallow
                    }
                }
                break;
            }
        }
    }
}
