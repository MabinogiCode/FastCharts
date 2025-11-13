using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Formatting;
using FastCharts.Core.Primitives;
using FastCharts.Core.Scales;
using FastCharts.Core.Utilities;

namespace FastCharts.Core.Axes
{
    /// <summary>
    /// Logarithmic axis implementation supporting base-10 logarithmic scaling.
    /// Values must be positive (> 0) for proper logarithmic transformation.
    /// </summary>
    public sealed class LogarithmicAxis : AxisBase, IAxis<double>
    {
        private double _logBase = 10.0;

        public LogarithmicAxis()
        {
            Scale = new LogarithmicScale(1, 10, 0, 1, _logBase);
            Ticker = new LogarithmicTicker(_logBase);
            DataRange = new FRange(1, 10);
            VisibleRange = new FRange(1, 10);
            NumberFormatter = new CompactNumberFormatter();
            LabelFormat = "G3"; // Default to 3 significant digits for log scales
        }

        /// <summary>
        /// Gets or sets the logarithm base. Default is 10.
        /// </summary>
        public double LogBase
        {
            get => _logBase;
            set
            {
                if (value <= 0 || double.IsNaN(value) || double.IsInfinity(value))
                {
                    throw new ArgumentException("Log base must be positive and finite", nameof(value));
                }

                _logBase = value;
                Ticker = new LogarithmicTicker(_logBase);
                
                // Update scale if it exists
                if (Scale is LogarithmicScale logScale)
                {
                    Scale = new LogarithmicScale(
                        logScale.DataMin, logScale.DataMax, 
                        logScale.PixelMin, logScale.PixelMax, 
                        _logBase);
                }
            }
        }

        public IScale<double> Scale { get; private set; }
        public ITicker<double> Ticker { get; private set; }
        public INumberFormatter? NumberFormatter { get; set; }

        public override void UpdateScale(double pixelMin, double pixelMax)
        {
            // Ensure visible range has positive values
            var minVal = Math.Max(VisibleRange.Min, double.Epsilon);
            var maxVal = Math.Max(VisibleRange.Max, double.Epsilon);

            if (minVal >= maxVal)
            {
                maxVal = minVal * 10; // Default 1 decade span
            }

            Scale = new LogarithmicScale(minVal, maxVal, pixelMin, pixelMax, _logBase);
        }

        /// <summary>
        /// Updates the visible range for logarithmic axis.
        /// Ensures values are positive and handles invalid ranges gracefully.
        /// </summary>
        public void SetVisibleRange(double min, double max)
        {
            if (double.IsNaN(min) || double.IsNaN(max))
            {
                return;
            }

            // Ensure positive values for logarithmic scale
            min = Math.Max(min, double.Epsilon);
            max = Math.Max(max, double.Epsilon);

            if (min > max)
            {
                (min, max) = (max, min);
            }

            // Avoid zero-length range in log space
            if (Math.Abs(Math.Log(max / min, _logBase)) < 1e-10)
            {
                max = min * _logBase; // Default to one base unit span
            }

            VisibleRange = new FRange(min, max);
        }
    }
}