using FastCharts.Core;

using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering
{
    /// <summary>
    /// Per-frame rendering context passed to the built-in render layers and to
    /// custom <see cref="ISeriesRenderer"/> implementations.
    /// </summary>
    public sealed class RenderContext
    {
        /// <summary>Gets the chart model being rendered.</summary>
        public ChartModel Model { get; }

        /// <summary>Gets the Skia canvas to draw on.</summary>
        public SKCanvas Canvas { get; }

        /// <summary>Gets the plot rectangle (data area) in pixels.</summary>
        public SKRect PlotRect { get; }

        /// <summary>Gets the shared paint pack used by the built-in layers.</summary>
        internal SkiaPaintPack Paints { get; }

        /// <summary>Gets the total surface width in pixels.</summary>
        public int PixelWidth { get; }

        /// <summary>Gets the total surface height in pixels.</summary>
        public int PixelHeight { get; }

        internal RenderContext(ChartModel model, SKCanvas canvas, SKRect plotRect, SkiaPaintPack paints, int w, int h)
        {
            Model = model;
            Canvas = canvas;
            PlotRect = plotRect;
            Paints = paints;
            PixelWidth = w;
            PixelHeight = h;
        }
    }
}
