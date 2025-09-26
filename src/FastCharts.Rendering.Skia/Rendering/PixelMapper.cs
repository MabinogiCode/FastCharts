using System;
using System.Globalization;

using FastCharts.Core.Abstractions;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    /// <summary>
    /// Centralized mapping between data space and pixel space based on axis VisibleRange and plotRect.
    /// </summary>
    internal static class PixelMapper
    {
        // Data -> Pixel (X) no clamp (caller clips to plot)
        public static float X<T>(T value, IAxis<T> axis, SKRect plotRect)
            where T : struct, IComparable<T>
        {
            var vr = axis.VisibleRange; // FRange (double)
            double span = vr.Max - vr.Min;
            if (span == 0) return plotRect.Left;
            double v = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double t = (v - vr.Min) / span;
            return (float)(plotRect.Left + t * plotRect.Width);
        }

        // Data -> Pixel (Y, inverted in pixels) no clamp
        public static float Y<T>(T value, IAxis<T> axis, SKRect plotRect)
            where T : struct, IComparable<T>
        {
            var vr = axis.VisibleRange;
            double span = vr.Max - vr.Min;
            if (span == 0) return plotRect.Bottom;
            double v = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            double t = (v - vr.Min) / span;
            return (float)(plotRect.Bottom - t * plotRect.Height);
        }

        // Pixel -> Data (X) clamp to visible
        public static double ToDataX(float px, IAxis<double> axis, SKRect plotRect)
        {
            var vr = axis.VisibleRange;
            if (plotRect.Width <= 0) return vr.Min;
            double t = (px - plotRect.Left) / plotRect.Width;
            if (t < 0) t = 0; else if (t > 1) t = 1;
            return vr.Min + t * (vr.Max - vr.Min);
        }

        // Pixel -> Data (Y) clamp to visible
        public static double ToDataY(float py, IAxis<double> axis, SKRect plotRect)
        {
            var vr = axis.VisibleRange;
            if (plotRect.Height <= 0) return vr.Max;
            double t = (py - plotRect.Top) / plotRect.Height;
            if (t < 0) t = 0; else if (t > 1) t = 1;
            t = 1.0 - t; // invert pixels
            return vr.Min + t * (vr.Max - vr.Min);
        }
    }
}

