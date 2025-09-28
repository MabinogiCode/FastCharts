using System;
using System.Globalization;

namespace FastCharts.Core.Formatting;

public sealed class CompactNumberFormatter : INumberFormatter
{
    private readonly int _digits;
    private readonly CultureInfo _culture;

    public CompactNumberFormatter(int digits = 1, CultureInfo? culture = null)
    {
        if (digits < 0)
        {
            digits = 0;
        }
        _digits = digits;
        _culture = culture ?? CultureInfo.InvariantCulture;
    }

    public string Format(double value)
    {
        var av = Math.Abs(value);
        var fmt = "0";
        if (_digits > 0)
        {
            fmt += "." + new string('#', _digits);
        }
        if (av >= 1_000_000_000_000d)
        {
            return (value / 1_000_000_000_000d).ToString(fmt, _culture) + "T";
        }
        if (av >= 1_000_000_000d)
        {
            return (value / 1_000_000_000d).ToString(fmt, _culture) + "B";
        }
        if (av >= 1_000_000d)
        {
            return (value / 1_000_000d).ToString(fmt, _culture) + "M";
        }
        if (av >= 1_000d)
        {
            return (value / 1_000d).ToString(fmt, _culture) + "k";
        }
        var smallFmt = _digits > 0 ? "0." + new string('#', _digits + 1) : "0";
        return value.ToString(smallFmt, _culture);
    }
}
