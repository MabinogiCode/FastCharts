using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;

using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    /// <summary>
    /// Centralized mapping between data space and pixel space based on axis VisibleRange and plotRect.
    /// Honors logarithmic axes through <see cref="AxisCoordinates"/>, and works on doubles directly
    /// (no per-point boxing on the hot rendering path).
    /// </summary>
    internal static class PixelMapper
    {
        // Data -> Pixel (X) no clamp (caller clips to plot)
        public static float X(double value, IAxis<double> axis, SKRect plotRect)
        {
            var t = AxisCoordinates.ToNormalized(axis, value);
            return (float)(plotRect.Left + (t * plotRect.Width));
        }

        // Data -> Pixel (Y, inverted in pixels) no clamp
        public static float Y(double value, IAxis<double> axis, SKRect plotRect)
        {
            var t = AxisCoordinates.ToNormalized(axis, value);
            return (float)(plotRect.Bottom - (t * plotRect.Height));
        }

        // Pixel -> Data (X) clamp to visible
        public static double ToDataX(float px, IAxis<double> axis, SKRect plotRect)
        {
            if (plotRect.Width <= 0)
            {
                return axis.VisibleRange.Min;
            }

            var t = Clamp01((px - plotRect.Left) / plotRect.Width);
            return AxisCoordinates.FromNormalized(axis, t);
        }

        // Pixel -> Data (Y) clamp to visible
        public static double ToDataY(float py, IAxis<double> axis, SKRect plotRect)
        {
            if (plotRect.Height <= 0)
            {
                return axis.VisibleRange.Max;
            }

            var t = 1.0 - Clamp01((py - plotRect.Top) / plotRect.Height); // invert pixels
            return AxisCoordinates.FromNormalized(axis, t);
        }

        private static double Clamp01(double t)
        {
            if (t < 0)
            {
                return 0;
            }

            return t > 1 ? 1 : t;
        }
    }
}
