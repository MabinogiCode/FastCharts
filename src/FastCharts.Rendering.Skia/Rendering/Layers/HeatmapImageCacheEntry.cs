using System;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

/// <summary>
/// Colored image of one heatmap grid (one pixel per cell), reused across frames while the
/// values and the color scale are unchanged.
/// </summary>
internal sealed class HeatmapImageCacheEntry : IDisposable
{
    public SKImage? Image { get; set; }

    public int DataVersion { get; set; } = -1;

    public ColorMap? ColorMap { get; set; }

    public double RangeMin { get; set; }

    public double RangeMax { get; set; }

    public ColorRgba MissingValueColor { get; set; }

    public bool Seen { get; set; }

    public void Dispose()
    {
        Image?.Dispose();
        Image = null;
    }
}
