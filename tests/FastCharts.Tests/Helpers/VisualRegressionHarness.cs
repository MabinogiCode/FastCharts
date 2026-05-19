using System;
using System.IO;

using SkiaSharp;

namespace FastCharts.Tests.Helpers;

/// <summary>
/// Visual-regression harness. Renders are compared against baseline PNGs stored
/// under <c>tests/FastCharts.Tests/VisualBaselines</c>. When a baseline is
/// missing it is created from the current render and the comparison passes; the
/// developer reviews and commits the new image. Later runs report a mismatch
/// when the rendered output drifts beyond tolerance, writing an
/// <c>&lt;name&gt;.actual.png</c> next to the baseline for inspection.
/// </summary>
internal static class VisualRegressionHarness
{
    /// <summary>Per-channel difference (0-255) below which a pixel counts as unchanged.</summary>
    private const int ChannelTolerance = 8;

    /// <summary>Maximum fraction of differing pixels tolerated before a mismatch.</summary>
    private const double MaxDifferingFraction = 0.02;

    /// <summary>
    /// Compares a rendered bitmap against the named baseline image.
    /// </summary>
    /// <param name="rendered">The freshly rendered bitmap.</param>
    /// <param name="baselineName">Baseline file name without extension.</param>
    /// <returns>The comparison result.</returns>
    public static VisualComparisonResult Compare(SKBitmap rendered, string baselineName)
    {
        if (rendered is null)
        {
            throw new ArgumentNullException(nameof(rendered));
        }

        var baselineDir = ResolveBaselineDirectory();
        Directory.CreateDirectory(baselineDir);
        var baselinePath = Path.Combine(baselineDir, baselineName + ".png");

        if (!File.Exists(baselinePath))
        {
            SavePng(rendered, baselinePath);
            return new VisualComparisonResult(true, true, 0.0);
        }

        using var baseline = SKBitmap.Decode(baselinePath);
        if (baseline is null || baseline.Width != rendered.Width || baseline.Height != rendered.Height)
        {
            SavePng(rendered, Path.Combine(baselineDir, baselineName + ".actual.png"));
            return new VisualComparisonResult(false, false, 1.0);
        }

        var differing = 0;
        var total = rendered.Width * rendered.Height;
        for (var y = 0; y < rendered.Height; y++)
        {
            for (var x = 0; x < rendered.Width; x++)
            {
                if (!WithinTolerance(rendered.GetPixel(x, y), baseline.GetPixel(x, y)))
                {
                    differing++;
                }
            }
        }

        var fraction = total == 0 ? 0.0 : (double)differing / total;
        var matches = fraction <= MaxDifferingFraction;
        if (!matches)
        {
            SavePng(rendered, Path.Combine(baselineDir, baselineName + ".actual.png"));
        }

        return new VisualComparisonResult(matches, false, fraction);
    }

    private static bool WithinTolerance(SKColor a, SKColor b)
    {
        return Math.Abs(a.Red - b.Red) <= ChannelTolerance
            && Math.Abs(a.Green - b.Green) <= ChannelTolerance
            && Math.Abs(a.Blue - b.Blue) <= ChannelTolerance
            && Math.Abs(a.Alpha - b.Alpha) <= ChannelTolerance;
    }

    private static void SavePng(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }

    private static string ResolveBaselineDirectory()
    {
        // Walk up from the test output directory to the repository root so the
        // baselines live in source (and can be reviewed and committed),
        // independent of the build configuration or working directory.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "FastChartsSolution.sln")))
            {
                return Path.Combine(dir.FullName, "tests", "FastCharts.Tests", "VisualBaselines");
            }

            dir = dir.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "VisualBaselines");
    }
}
