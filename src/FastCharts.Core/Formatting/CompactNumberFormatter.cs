using System;
using System.Globalization;

namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Formats numbers like: 950 -> "950", 1_200 -> "1.2k", 3_400_000 -> "3.4M".
    /// Suffixes: k (10^3), M (10^6), B (10^9), T (10^12).
    /// </summary>
    public sealed class CompactNumberFormatter : INumberFormatter
    {
        private readonly int _digits; // number of decimals for scaled values
        private readonly CultureInfo _culture;

        public CompactNumberFormatter(int digits = 1, CultureInfo? culture = null)
        {
            if (digits < 0) digits = 0;
            _digits = digits;
            _culture = culture ?? CultureInfo.InvariantCulture;
        }

        public string Format(double value)
        {
            double av = Math.Abs(value);
            string fmt = "0";
            if (_digits > 0) fmt += "." + new string('#', _digits);

            if (av >= 1_000_000_000_000d) return (value / 1_000_000_000_000d).ToString(fmt, _culture) + "T";
            if (av >= 1_000_000_000d)     return (value / 1_000_000_000d).ToString(fmt, _culture) + "B";
            if (av >= 1_000_000d)         return (value / 1_000_000d).ToString(fmt, _culture) + "M";
            if (av >= 1_000d)             return (value / 1_000d).ToString(fmt, _culture) + "k";

            // For small values, keep up to `_digits+1` decimals for readability
            string smallFmt = _digits > 0 ? "0." + new string('#', _digits + 1) : "0";
            return value.ToString(smallFmt, _culture);
        }
    }
}
