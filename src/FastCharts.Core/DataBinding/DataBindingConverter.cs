using System;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Utility class for converting values to chart data types
    /// Handles type conversions for X/Y coordinates from arbitrary object properties
    /// </summary>
    public static class DataBindingConverter
    {
        // Unix epoch for .NET Standard 2.0 compatibility
#pragma warning disable S6588 // Use DateTime.UnixEpoch is not available in .NET Standard 2.0
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
#pragma warning restore S6588

        /// <summary>
        /// Converts a value to double for chart coordinates
        /// Supports various numeric types, DateTime, and custom conversions
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Double value or NaN if conversion fails</returns>
        public static double ToDouble(object? value)
        {
            if (value == null)
            {
                return double.NaN;
            }

            try
            {
                return value switch
                {
                    double d => d,
                    float f => f,
                    int i => i,
                    long l => l,
                    decimal dec => (double)dec,
                    byte b => b,
                    short s => s,
                    uint ui => ui,
                    ulong ul => ul,
                    ushort us => us,
                    sbyte sb => sb,
                    DateTime dt => dt.Ticks,
                    DateTimeOffset dto => dto.Ticks,
                    TimeSpan ts => ts.TotalMilliseconds,
                    bool boolean => boolean ? 1.0 : 0.0,
                    string str when double.TryParse(str, out var parsed) => parsed,
                    _ => Convert.ToDouble(value)
                };
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Converts a value to string for chart labels
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>String representation or empty string if null</returns>
        public static string ToString(object? value)
        {
            return value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Checks if a value represents a valid numeric coordinate
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if value can be converted to a valid double</returns>
        public static bool IsValidCoordinate(object? value)
        {
            var doubleValue = ToDouble(value);
            return !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue);
        }

        /// <summary>
        /// Converts DateTime to chart coordinate based on specified unit
        /// </summary>
        /// <param name="dateTime">DateTime to convert</param>
        /// <param name="unit">Time unit for conversion</param>
        /// <returns>Double representation of the DateTime</returns>
        public static double DateTimeToDouble(DateTime dateTime, DateTimeUnit unit = DateTimeUnit.Ticks)
        {
            return unit switch
            {
                DateTimeUnit.Ticks => dateTime.Ticks,
                DateTimeUnit.Milliseconds => dateTime.Subtract(UnixEpoch).TotalMilliseconds,
                DateTimeUnit.Seconds => dateTime.Subtract(UnixEpoch).TotalSeconds,
                DateTimeUnit.Minutes => dateTime.Subtract(UnixEpoch).TotalMinutes,
                DateTimeUnit.Hours => dateTime.Subtract(UnixEpoch).TotalHours,
                DateTimeUnit.Days => dateTime.Subtract(UnixEpoch).TotalDays,
                _ => dateTime.Ticks
            };
        }

        /// <summary>
        /// Converts chart coordinate back to DateTime
        /// </summary>
        /// <param name="value">Chart coordinate value</param>
        /// <param name="unit">Time unit for conversion</param>
        /// <returns>DateTime representation</returns>
        public static DateTime DoubleToDateTime(double value, DateTimeUnit unit = DateTimeUnit.Ticks)
        {
            try
            {
                return unit switch
                {
                    DateTimeUnit.Ticks => new DateTime((long)value, DateTimeKind.Unspecified),
                    DateTimeUnit.Milliseconds => UnixEpoch.AddMilliseconds(value),
                    DateTimeUnit.Seconds => UnixEpoch.AddSeconds(value),
                    DateTimeUnit.Minutes => UnixEpoch.AddMinutes(value),
                    DateTimeUnit.Hours => UnixEpoch.AddHours(value),
                    DateTimeUnit.Days => UnixEpoch.AddDays(value),
                    _ => new DateTime((long)value, DateTimeKind.Unspecified)
                };
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }

    /// <summary>
    /// Units for DateTime to double conversion in data binding
    /// </summary>
    public enum DateTimeUnit
    {
        /// <summary>
        /// DateTime ticks (most precise)
        /// </summary>
        Ticks,

        /// <summary>
        /// Milliseconds since Unix epoch
        /// </summary>
        Milliseconds,

        /// <summary>
        /// Seconds since Unix epoch
        /// </summary>
        Seconds,

        /// <summary>
        /// Minutes since Unix epoch
        /// </summary>
        Minutes,

        /// <summary>
        /// Hours since Unix epoch
        /// </summary>
        Hours,

        /// <summary>
        /// Days since Unix epoch
        /// </summary>
        Days
    }
}