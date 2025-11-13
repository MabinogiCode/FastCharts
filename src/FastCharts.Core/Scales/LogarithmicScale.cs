using System;
using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Scales
{
    /// <summary>
    /// Logarithmic scale implementation for mapping between logarithmic data space and pixel space.
    /// </summary>
    public sealed class LogarithmicScale : IScale<double>
    {
        private readonly double _logBase;
        private readonly double _logDataMin;
        private readonly double _logDataSpan;
        private readonly double _pixelSpan;

        public LogarithmicScale(double dataMin, double dataMax, double pixelMin, double pixelMax, double logBase = 10.0)
        {
            if (dataMin <= 0 || dataMax <= 0)
            {
                throw new ArgumentException("Logarithmic scale requires positive data values");
            }

            if (logBase <= 0 || double.IsNaN(logBase) || double.IsInfinity(logBase))
            {
                throw new ArgumentException("Log base must be positive and finite");
            }

            DataMin = dataMin;
            DataMax = dataMax;
            PixelMin = pixelMin;
            PixelMax = pixelMax;
            _logBase = logBase;

            _logDataMin = Math.Log(dataMin, _logBase);
            var logDataMax = Math.Log(dataMax, _logBase);
            _logDataSpan = logDataMax - _logDataMin;
            _pixelSpan = pixelMax - pixelMin;

            // Handle degenerate case
            if (Math.Abs(_logDataSpan) < double.Epsilon)
            {
                _logDataSpan = 1.0; // Fallback to prevent division by zero
            }
        }

        public double DataMin { get; }
        public double DataMax { get; }
        public double PixelMin { get; }
        public double PixelMax { get; }

        /// <summary>
        /// Maps a data value to pixel coordinate using logarithmic transformation.
        /// </summary>
        /// <param name="value">Data value to map (must be positive)</param>
        /// <returns>Corresponding pixel coordinate</returns>
        public double ToPixels(double value)
        {
            if (value <= 0)
            {
                // Handle edge case: return pixel position for minimum visible value
                return PixelMin;
            }

            var logValue = Math.Log(value, _logBase);
            var normalizedPosition = (logValue - _logDataMin) / _logDataSpan;
            return PixelMin + normalizedPosition * _pixelSpan;
        }

        /// <summary>
        /// Maps a pixel coordinate to data value using inverse logarithmic transformation.
        /// </summary>
        /// <param name="px">Pixel coordinate</param>
        /// <returns>Corresponding data value (always positive)</returns>
        public double FromPixels(double px)
        {
            var normalizedPosition = (px - PixelMin) / _pixelSpan;
            var logValue = _logDataMin + normalizedPosition * _logDataSpan;
            return Math.Pow(_logBase, logValue);
        }

        /// <summary>
        /// Clamps a data value to the valid range of this scale.
        /// </summary>
        /// <param name="dataValue">Data value to clamp</param>
        /// <returns>Clamped data value within [DataMin, DataMax]</returns>
        public double ClampData(double dataValue)
        {
            return Math.Max(DataMin, Math.Min(DataMax, dataValue));
        }

        /// <summary>
        /// Clamps a pixel value to the valid range of this scale.
        /// </summary>
        /// <param name="pixelValue">Pixel value to clamp</param>
        /// <returns>Clamped pixel value within [PixelMin, PixelMax]</returns>
        public double ClampPixel(double pixelValue)
        {
            return Math.Max(PixelMin, Math.Min(PixelMax, pixelValue));
        }
    }
}