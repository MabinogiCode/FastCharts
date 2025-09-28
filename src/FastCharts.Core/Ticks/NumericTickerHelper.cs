using System;

namespace FastCharts.Core.Ticks;

internal static class NumericTickerHelper
{
    public static double CalculateNiceStep(double rough)
    {
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
        return nice * baseStep;
    }
}