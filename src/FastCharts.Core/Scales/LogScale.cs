using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Scales
{
    /// <summary>
    /// Logarithmic scale for transforming between data values and pixel coordinates
    /// Supports base-10 logarithmic scaling with proper handling of zero and negative values
    /// </summary>
    public sealed class LogScale : IScale<double>
    {
        private readonly double _dataMin;
        private readonly double _dataMax;
        private readonly double _pixelMin;
        private readonly double _pixelMax;
        private readonly double _logBase;
        private readonly double _logDataMin;
        private readonly double _logDataMax;

        /// <summary>
        /// Creates a logarithmic scale
        /// </summary>
        /// <param name="dataMin">Minimum data value (must be > 0)</param>
        /// <param name="dataMax">Maximum data value (must be > 0)</param>
        /// <param name="pixelMin">Minimum pixel coordinate</param>
        /// <param name="pixelMax">Maximum pixel coordinate</param>
        /// <param name="logBase">Logarithm base (default: 10)</param>
        public LogScale(double dataMin, double dataMax, double pixelMin, double pixelMax, double logBase = 10.0)
        {
            if (dataMin <= 0 || dataMax <= 0)
            {
                throw new ArgumentException("Logarithmic scale requires positive data values");
            }

            if (dataMin >= dataMax)
            {
                throw new ArgumentException("Data minimum must be less than maximum");
            }

            if (logBase <= 0 || Math.Abs(logBase - 1.0) < 1e-10)
            {
                throw new ArgumentException("Logarithm base must be positive and not equal to 1");
            }

            _dataMin = dataMin;
            _dataMax = dataMax;
            _pixelMin = pixelMin;
            _pixelMax = pixelMax;
            _logBase = logBase;

            // Pre-calculate log values for performance
            _logDataMin = Math.Log(dataMin, logBase);
            _logDataMax = Math.Log(dataMax, logBase);
        }

        /// <summary>
        /// Converts a data value to pixel coordinates using logarithmic transformation
        /// </summary>
        /// <param name="value">Data value to convert</param>
        /// <returns>Pixel coordinate</returns>
        public double ToPixels(double value)
        {
            // Handle edge cases
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                return double.NaN;
            }

            // Clamp negative or zero values to minimum
            if (value <= 0)
            {
                value = _dataMin;
            }

            // Calculate logarithmic position
            var logValue = Math.Log(value, _logBase);
            var t = (logValue - _logDataMin) / (_logDataMax - _logDataMin);
            
            // Clamp to prevent overflow
            t = Math.Max(0, Math.Min(1, t));
            
            return _pixelMin + t * (_pixelMax - _pixelMin);
        }

        /// <summary>
        /// Converts pixel coordinates back to data values using inverse logarithmic transformation
        /// </summary>
        /// <param name="px">Pixel coordinate</param>
        /// <returns>Data value</returns>
        public double FromPixels(double px)
        {
            if (double.IsNaN(px) || double.IsInfinity(px))
            {
                return double.NaN;
            }

            // Calculate normalized position
            var t = (px - _pixelMin) / (_pixelMax - _pixelMin);
            
            // Calculate log value at this position
            var logValue = _logDataMin + t * (_logDataMax - _logDataMin);
            
            // Convert back to linear scale
            var value = Math.Pow(_logBase, logValue);
            
            // Ensure result is within valid range
            return Math.Max(_dataMin, Math.Min(_dataMax, value));
        }

        /// <summary>
        /// Gets the logarithm base used by this scale
        /// </summary>
        public double LogBase => _logBase;

        /// <summary>
        /// Gets the data range in log space
        /// </summary>
        public (double Min, double Max) LogDataRange => (_logDataMin, _logDataMax);
    }
}