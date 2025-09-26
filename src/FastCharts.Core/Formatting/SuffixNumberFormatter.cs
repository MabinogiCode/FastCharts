using System;
using System.Globalization;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Formats numbers using engineering-like suffixes: k, M, G, T, P, E (and m, μ, n for small).
    /// Example: 1530 -> 1.53k, 0.000002 -> 2μ.
    /// </summary>
    public sealed class SuffixNumberFormatter : INumberFormatter
    {
        private readonly int _maxDecimals;

        public SuffixNumberFormatter(int maxDecimals = 2)
        {
            if (maxDecimals < 0) maxDecimals = 0;
            if (maxDecimals > 6) maxDecimals = 6;
            _maxDecimals = maxDecimals;
        }

        public string Format(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value == 0.0)
            {
                return value.ToString("G", CultureInfo.InvariantCulture);
            }

            double abs = Math.Abs(value);
            string suffix = string.Empty;
            double scaled = value;

            // Large units
            if (abs >= 1e18) { scaled = value / 1e18; suffix = "E"; }
            else if (abs >= 1e15) { scaled = value / 1e15; suffix = "P"; }
            else if (abs >= 1e12) { scaled = value / 1e12; suffix = "T"; }
            else if (abs >= 1e9) { scaled = value / 1e9; suffix = "B"; }
            else if (abs >= 1e6) { scaled = value / 1e6; suffix = "M"; }
            else if (abs >= 1e3) { scaled = value / 1e3; suffix = "k"; }
            // Small units
            else if (abs < 1e-9) { scaled = value * 1e12; suffix = "p"; }      // pico
            else if (abs < 1e-6) { scaled = value * 1e9; suffix = "n"; }       // nano
            else if (abs < 1e-3) { scaled = value * 1e6; suffix = "μ"; }       // micro
            else if (abs < 1) { scaled = value * 1e3; suffix = "m"; }          // milli

            string fmt = "F" + _maxDecimals;
            string text = scaled.ToString(fmt, CultureInfo.InvariantCulture);
            // Trim trailing zeros & dot
            text = TrimZeros(text);
            return text + suffix;
        }

        private static string TrimZeros(string s)
        {
            int dotPos = s.IndexOf('.');
            if (dotPos < 0)
            {
                dotPos = s.IndexOf(',');
            }
            if (dotPos < 0)
            {
                return s;
            }
            int end = s.Length - 1;
            while (end > dotPos && s[end] == '0') end--;
            if (end == dotPos) end--;
            return s.Substring(0, end + 1);
        }
    }
}
