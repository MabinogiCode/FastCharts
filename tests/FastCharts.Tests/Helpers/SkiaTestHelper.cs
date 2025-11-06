using System;
using SkiaSharp;

namespace FastCharts.Tests.Helpers;

internal static class SkiaTestHelper
{
    /// <summary>
    /// Compares two SKColor values with tolerance for AA and rendering variations.
    /// </summary>
    public static bool SameColor(SKColor a, SKColor b, byte tolerance = 6)
    {
        return Math.Abs(a.Red - b.Red) <= tolerance
            && Math.Abs(a.Green - b.Green) <= tolerance
            && Math.Abs(a.Blue - b.Blue) <= tolerance
            && Math.Abs(a.Alpha - b.Alpha) <= tolerance;
    }

    /// <summary>
    /// Probes a small area around a center point to find pixels that differ from a forbidden color.
    /// Used for robust testing with anti-aliasing.
    /// </summary>
    public static bool ProbeNotColor(SKBitmap bitmap, System.Drawing.Point center, SKColor forbiddenColor, int radius)
    {
        for (var dy = -radius; dy <= radius; dy++)
        {
            var y = center.Y + dy;
            if (y < 0 || y >= bitmap.Height)
            {
                continue;
            }

            for (var dx = -radius; dx <= radius; dx++)
            {
                var x = center.X + dx;
                if (x < 0 || x >= bitmap.Width)
                {
                    continue;
                }

                var color = bitmap.GetPixel(x, y);
                if (!SameColor(color, forbiddenColor))
                {
                    return true;
                }
            }
        }
        return false;
    }
}