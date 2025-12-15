using System;
using System.Globalization;

namespace FastCharts.Core.Formatting;

public sealed class ScientificNumberFormatter : INumberFormatter
{
    public double UpperThreshold { get; }
    public double LowerThreshold { get; }
    public int SignificantDigits { get; }

    public ScientificNumberFormatter(int significantDigits = 3, double upperThreshold = 1e6, double lowerThreshold = 1e-3)
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
        // Handle special values
        if (double.IsNaN(value)) 
        {
            return "NaN";
        }
        if (double.IsInfinity(value)) 
        {
            return "?";
        }
        if (Math.Abs(value) < 1e-15) 
        {
            return "0";
        }

        var abs = Math.Abs(value);
        var sign = value < 0 ? "-" : string.Empty;

        // Determine base 10 exponent
        var exponent = (int)Math.Floor(Math.Log10(abs));
        var pow10 = Math.Pow(10, exponent);
        var multiplier = abs / pow10;
        const double tol = 1e-12;

        // Case 1: value is exactly 1
        if (Math.Abs(abs - 1.0) < tol)
        {
            return sign + "1";
        }

        // Case 2: pure power of 10 (multiplier ? 1)
        if (Math.Abs(multiplier - 1.0) < tol)
        {
            return sign + "10^" + exponent.ToString(CultureInfo.InvariantCulture);
        }

        // Case 3: integer multiples 2..9 of power of 10
        var roundedMult = Math.Round(multiplier);
        if (Math.Abs(multiplier - roundedMult) < tol && roundedMult >= 2 && roundedMult <= 9)
        {
            return sign + roundedMult.ToString(CultureInfo.InvariantCulture) + "×10^" + exponent.ToString(CultureInfo.InvariantCulture);
        }

        // Case 4: generic scientific notation  
        // Mantissa between [1,10)
        var mantissa = abs / pow10;
        
        // For E notation, significantDigits specifies decimal places in mantissa
        // significantDigits=2 ? "X.YZ" (2 decimal places)
        var decimalPlaces = Math.Max(0, SignificantDigits - 1);
        
        var formatString = decimalPlaces > 0 ? 
            $"F{decimalPlaces}" :  // Use F format to force decimal places
            "F0";
            
        var mantissaStr = mantissa.ToString(formatString, CultureInfo.InvariantCulture);
        
        var expSign = exponent >= 0 ? "+" : "-";
        var expAbs = Math.Abs(exponent).ToString("00", CultureInfo.InvariantCulture);
        return sign + mantissaStr + "E" + expSign + expAbs;
    }
}
