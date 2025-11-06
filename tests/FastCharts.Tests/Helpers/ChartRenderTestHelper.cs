using System;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Rendering.Skia;
using SkiaSharp;

namespace FastCharts.Tests.Helpers;

/// <summary>
/// Helper class for chart rendering tests with Skia backend.
/// Provides common utilities for rendering charts to bitmaps and comparing results.
/// </summary>
internal static class ChartRenderTestHelper
{
    /// <summary>
    /// Renders a ChartModel to SKBitmap using SkiaChartRenderer.
    /// </summary>
    /// <param name="model">The chart model to render.</param>
    /// <param name="width">The bitmap width in pixels.</param>
    /// <param name="height">The bitmap height in pixels.</param>
    /// <returns>A tuple containing the rendered bitmap and the original model.</returns>
    public static (SKBitmap bmp, ChartModel model) Render(ChartModel model, int width = 420, int height = 300)
    {
        var renderer = new SkiaChartRenderer();
        var bitmap = new SKBitmap(width, height, true);
        using var canvas = new SKCanvas(bitmap);
        renderer.Render(model, canvas, width, height);
        canvas.Flush();
        return (bitmap, model);
    }

    /// <summary>
    /// Counts the number of differing pixels between two bitmaps.
    /// Useful for smoke tests to verify that chart rendering produces visual changes.
    /// </summary>
    /// <param name="bitmapA">First bitmap to compare.</param>
    /// <param name="bitmapB">Second bitmap to compare.</param>
    /// <returns>The number of pixels that differ between the two bitmaps.</returns>
    public static int CountDifferingPixels(SKBitmap bitmapA, SKBitmap bitmapB)
    {
        var width = Math.Min(bitmapA.Width, bitmapB.Width);
        var height = Math.Min(bitmapA.Height, bitmapB.Height);
        var differenceCount = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (bitmapA.GetPixel(x, y) != bitmapB.GetPixel(x, y))
                {
                    differenceCount++;
                }
            }
        }
        return differenceCount;
    }

    /// <summary>
    /// Sets visible range for both X and Y axes on a ChartModel.
    /// Convenience method for test setup.
    /// </summary>
    /// <param name="model">The chart model to configure.</param>
    /// <param name="xMin">Minimum X axis value.</param>
    /// <param name="xMax">Maximum X axis value.</param>
    /// <param name="yMin">Minimum Y axis value.</param>
    /// <param name="yMax">Maximum Y axis value.</param>
    public static void SetVisibleRange(ChartModel model, double xMin, double xMax, double yMin, double yMax)
    {
        model.XAxis.VisibleRange = new FRange(xMin, xMax);
        model.YAxis.VisibleRange = new FRange(yMin, yMax);
    }
}