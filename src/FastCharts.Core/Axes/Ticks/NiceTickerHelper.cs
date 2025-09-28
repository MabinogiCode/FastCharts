using System;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Axes.Ticks;

internal static class NiceTickerHelper
{
    public static double CalculateNiceStep(double rough)
    {
        var sign = rough < 0 ? -1 : 1;
        rough = Math.Abs(rough);
        if (DoubleUtils.IsZero(rough))
        {
            return 1;
        }
        var exp = Math.Floor(Math.Log10(rough));
        var baseStep = Math.Pow(10, exp);
        var m = rough / baseStep;
        double nice;
        if (m < 1.5)
        {
            nice = 1;
        }
        else if (m < 3)
        {
            nice = 2;
        }
        else if (m < 7)
        {
            nice = 5;
        }
        else
        {
            nice = 10;
        }
        return sign * nice * baseStep;
    }
}