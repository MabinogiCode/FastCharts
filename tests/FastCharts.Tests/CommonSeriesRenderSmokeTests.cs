using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

using SkiaSharp;

using Xunit;

namespace FastCharts.Tests
{
    public class CommonSeriesRenderSmokeTests
    {
        // Static readonly arrays to satisfy CA1861 (reused constant data definitions)
        private static readonly BarPoint[] BarsAutoFitPoints =
        [
            new BarPoint(1,5),
            new BarPoint(2,8),
            new BarPoint(3,3)
        ];

        private static readonly StackedBarPoint[] StackAutoFitPoints =
        [
            new StackedBarPoint(4, [1.0, 2.0]),
            new StackedBarPoint(5, [0.5, 1.5, 1.0])
        ];

        private static (SKBitmap bmp, ChartModel model) Render(ChartModel model, int w = 420, int h = 300)
        {
            var renderer = new SkiaChartRenderer();
            var bmp = new SKBitmap(w, h, true);
            using var canvas = new SKCanvas(bmp);
            renderer.Render(model, canvas, w, h);
            canvas.Flush();
            return (bmp, model);
        }

        private static int CountDiff(SKBitmap a, SKBitmap b)
        {
            var w = Math.Min(a.Width, b.Width);
            int h = Math.Min(a.Height, b.Height);
            int diff = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (a.GetPixel(x, y) != b.GetPixel(x, y)) diff++;
                }
            }
            return diff;
        }

        private static void SetVisibleRange(ChartModel m, double xmin, double xmax, double ymin, double ymax)
        {
            m.XAxis.VisibleRange = new FRange(xmin, xmax);
            m.YAxis.VisibleRange = new FRange(ymin, ymax);
        }

        [Fact]
        public void LineSeriesSmoke()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 100, -10, 10);

            var withLine = new ChartModel();
            SetVisibleRange(withLine, 0, 100, -10, 10);
            var pts = Enumerable.Range(0, 50).Select(i => new PointD(i * 2, Math.Sin(i * 0.2) * 5)).ToArray();
            withLine.AddSeries(new LineSeries(pts));

            var (bmpEmpty, _) = Render(empty);
            var (bmpLine, _) = Render(withLine);
            int diff = CountDiff(bmpEmpty, bmpLine);
            Assert.True(diff > 150, $"Expected diff > 150, got {diff}");
        }

        [Fact]
        public void AreaSeriesSmoke()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 50, 0, 20);

            var withArea = new ChartModel();
            SetVisibleRange(withArea, 0, 50, 0, 20);
            var pts = Enumerable.Range(0, 60).Select(i => new PointD(i * 0.8, 5 + Math.Sin(i * 0.15) * 4 + (i * 0.01))).ToArray();
            withArea.AddSeries(new AreaSeries(pts) { Baseline = 0, FillOpacity = 0.5 });

            var (bmpEmpty, _) = Render(empty);
            var (bmpArea, _) = Render(withArea);
            int diff = CountDiff(bmpEmpty, bmpArea);
            Assert.True(diff > 150, $"Expected diff > 150, got {diff}");
        }

        [Fact]
        public void ScatterSeriesSmoke()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 100, 0, 100);

            var scatter = new ChartModel();
            SetVisibleRange(scatter, 0, 100, 0, 100);
            var rnd = new Random(123);
            var pts = Enumerable.Range(0, 80).Select(i => new PointD(i + 0.5, 50 + Math.Sin(i * 0.3) * 20 + rnd.NextDouble() * 5)).ToArray();
            scatter.AddSeries(new ScatterSeries(pts) { MarkerSize = 4 });

            var (bmpEmpty, _) = Render(empty);
            var (bmpScatter, _) = Render(scatter);
            int diff = CountDiff(bmpEmpty, bmpScatter);
            Assert.True(diff > 120, $"Expected diff > 120, got {diff}");
        }

        [Fact]
        public void BarSeriesSmoke()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 12, 0, 20);

            var bars = new ChartModel();
            SetVisibleRange(bars, 0, 12, 0, 20);
            var pts = Enumerable.Range(0, 10).Select(i => new BarPoint(i + 1, 5 + Math.Sin(i * 0.6) * 4 + i * 0.3)).ToArray();
            bars.AddSeries(new BarSeries(pts));

            var (bmpEmpty, _) = Render(empty);
            var (bmpBars, _) = Render(bars);
            int diff = CountDiff(bmpEmpty, bmpBars);
            Assert.True(diff > 120, $"Expected diff > 120, got {diff}");
        }

        [Fact]
        public void StackedBarSeriesSmoke()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 12, 0, 10);

            var sbar = new ChartModel();
            SetVisibleRange(sbar, 0, 12, 0, 10);
            var rnd = new Random(42);
            var pts = Enumerable.Range(0, 8).Select(i =>
            {
                double a = 1 + rnd.NextDouble();
                double b = 0.5 + rnd.NextDouble();
                double c = 0.25 + rnd.NextDouble();
                return new StackedBarPoint(i + 1, new[] { a, b, c });
            }).ToArray();
            sbar.AddSeries(new StackedBarSeries(pts) { FillOpacity = 0.8 });

            var (bmpEmpty, _) = Render(empty);
            var (bmpStack, _) = Render(sbar);
            int diff = CountDiff(bmpEmpty, bmpStack);
            Assert.True(diff > 120, $"Expected diff > 120, got {diff}");
        }

        [Fact]
        public void AutoFitShouldCoverBarsAndStacked()
        {
            var m = new ChartModel();
            var bars = new BarSeries(BarsAutoFitPoints);
            var stack = new StackedBarSeries(StackAutoFitPoints);
            m.AddSeries(bars);
            m.AddSeries(stack);
            m.AutoFitDataRange();
            var xr = m.XAxis.DataRange; var yr = m.YAxis.DataRange;
            Assert.True(xr.Min <= 1 && xr.Max >= 5, $"Unexpected X range: {xr.Min}..{xr.Max}");
            Assert.True(yr.Min <= 0 && yr.Max >= 5.0, $"Unexpected Y range: {yr.Min}..{yr.Max}");
        }
    }
}
