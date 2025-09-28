using System;
using System.Globalization;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Formatting;

/// <summary>
/// Formats numbers using engineering-like suffixes: k, M, G, T, P, E (and m, μ, n for small).
/// Example: 1530 -> 1.53k, 0.000002 -> 2μ.
/// </summary>
public sealed class SuffixNumberFormatter : INumberFormatter
{
    private readonly int _maxDecimals;

    public SuffixNumberFormatter(int maxDecimals = 2)
    {
        if (maxDecimals < 0)
        {
            maxDecimals = 0;
        }
        if (maxDecimals > 6)
        {
            maxDecimals = 6;
        }
        _maxDecimals = maxDecimals;
    }

    public string Format(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || DoubleUtils.IsZero(value))
        {
            return value.ToString("G", CultureInfo.InvariantCulture);
        }
        var abs = System.Math.Abs(value);
        var suffix = string.Empty;
        var scaled = value;
        if (abs >= 1e18)
        {
            scaled = value / 1e18; suffix = "E";
        }
        else if (abs >= 1e15)
        {
            scaled = value / 1e15; suffix = "P";
        }
        else if (abs >= 1e12)
        {
            scaled = value / 1e12; suffix = "T";
        }
        else if (abs >= 1e9)
        {
            scaled = value / 1e9; suffix = "B";
        }
        else if (abs >= 1e6)
        {
            scaled = value / 1e6; suffix = "M";
        }
        else if (abs >= 1e3)
        {
            scaled = value / 1e3; suffix = "k";
        }
        else if (abs < 1e-9)
        {
            scaled = value * 1e12; suffix = "p";
        }
        else if (abs < 1e-6)
        {
            scaled = value * 1e9; suffix = "n";
        }
        else if (abs < 1e-3)
        {
            scaled = value * 1e6; suffix = "μ";
        }
        else if (abs < 1)
        {
            scaled = value * 1e3; suffix = "m";
        }
        var fmt = "F" + _maxDecimals;
        var text = scaled.ToString(fmt, CultureInfo.InvariantCulture);
        text = TrimZeros(text);
        return text + suffix;
    }

    private static string TrimZeros(string s)
    {
        var dotPos = s.IndexOf('.');
        if (dotPos < 0)
        {
            dotPos = s.IndexOf(',');
        }
        if (dotPos < 0)
        {
            return s;
        }
        var end = s.Length - 1;
        while (end > dotPos && s[end] == '0')
        {
            end--;
        }
        if (end == dotPos)
        {
            end--;
        }
        return s.Substring(0, end + 1);
    }
}
