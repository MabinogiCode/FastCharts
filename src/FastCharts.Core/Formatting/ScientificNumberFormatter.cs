using System.Globalization;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Formatting;

public sealed class ScientificNumberFormatter : INumberFormatter
{
    public double UpperThreshold { get; }
    public double LowerThreshold { get; }
    public int SignificantDigits { get; }

    public ScientificNumberFormatter(double upperThreshold = 1e6, double lowerThreshold = 1e-3, int significantDigits = 3)
    {
        if (upperThreshold <= 0)
        {
            upperThreshold = 1e6;
        }
        if (lowerThreshold is <= 0 or >= 1)
        {
            lowerThreshold = 1e-3;
        }
        if (significantDigits < 1)
        {
            significantDigits = 3;
        }
        UpperThreshold = upperThreshold;
        LowerThreshold = lowerThreshold;
        SignificantDigits = significantDigits;
    }

    public string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
        var abs = System.Math.Abs(value);
        if ((abs >= UpperThreshold) || (abs > 0 && abs < LowerThreshold))
        {
            return value.ToString("E" + (SignificantDigits - 1), CultureInfo.InvariantCulture);
        }
        return value.ToString("G", CultureInfo.InvariantCulture);
    }
}
