using System;
using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

/// <summary>
/// Cached geometry per series (T-PERF-CACHE): projected pixels and the built SKPath
/// are reused across frames while the data version, visible ranges, plot rect and
/// smoothing mode are unchanged — the common case during tooltip/crosshair redraws.
/// Entries for series no longer rendered are swept and disposed each frame.
/// </summary>
internal sealed class LineGeometryCacheEntry : IDisposable
{
    public SKPath Path { get; } = new SKPath();

    public SKPoint[] Pixels { get; set; } = Array.Empty<SKPoint>();

    public int DataVersion { get; set; } = -1;

    public double XMin { get; set; }

    public double XMax { get; set; }

    public double YMin { get; set; }

    public double YMax { get; set; }

    public SKRect PlotRect { get; set; }

    public LineSmoothing Smoothing { get; set; }

    public int YAxisIndex { get; set; }

    // Axis instances the pixels were projected with: replacing an axis (e.g. linear -> log)
    // with the same visible range must still rebuild the geometry.
    public object? XAxis { get; set; }

    public object? YAxis { get; set; }

    public bool Seen { get; set; }

    public void Dispose()
    {
        Path.Dispose();
    }
}
