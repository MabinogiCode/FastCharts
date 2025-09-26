using System;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Core.Primitives;
using FastCharts.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace FastCharts.Tests
{
    public class OhlcErrorRenderSmokeTests
    {
        private (SKBitmap bmp, ChartModel model) Render(ChartModel model, int w = 420, int h = 300)
        {
            var renderer = new SkiaChartRenderer();
            var bmp = new SKBitmap(w, h, true);
            using var canvas = new SKCanvas(bmp);
            renderer.Render(model, canvas, w, h);
            canvas.Flush();
            return (bmp, model);
        }

        private int CountDiff(SKBitmap a, SKBitmap b)
        {
            int w = Math.Min(a.Width, b.Width);
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
        public void OhlcSeries_Should_AlterBitmapComparedToEmpty()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 10, 90, 110);

            var withOhlc = new ChartModel();
            SetVisibleRange(withOhlc, 0, 10, 90, 110);

            // Simple OHLC data
            var data = Enumerable.Range(0, 8).Select(i =>
            {
                double x = i + 0.5;
                double open = 100 + Math.Sin(i * 0.3) * 2;
                double close = open + (Math.Sin(i * 0.7) * 1.5);
                double high = Math.Max(open, close) + 1.5;
                double low = Math.Min(open, close) - 1.5;
                return new OhlcPoint(x, open, high, low, close);
            }).ToArray();
            withOhlc.AddSeries(new OhlcSeries(data));

            var (bmpEmpty, _) = Render(empty);
            var (bmpOhlc, _) = Render(withOhlc);
            int diff = CountDiff(bmpEmpty, bmpOhlc);
            // Expect some thousands of differing pixels (grid + candles). Just assert > 200 to be safe.
            Assert.True(diff > 200, $"Expected rendered OHLC to differ from empty baseline, diff={diff}");
        }

        [Fact]
        public void ErrorBarSeries_Should_AlterBitmapComparedToEmpty()
        {
            var empty = new ChartModel();
            SetVisibleRange(empty, 0, 20, 0, 100);

            var withErr = new ChartModel();
            SetVisibleRange(withErr, 0, 20, 0, 100);

            var rnd = new Random(42);
            var pts = Enumerable.Range(0, 15).Select(i =>
            {
                double x = i + 0.7;
                double y = 40 + Math.Sin(i * 0.4) * 10;
                double pe = 5 + rnd.NextDouble() * 3;
                double ne = pe * (0.4 + rnd.NextDouble() * 0.6);
                return new ErrorBarPoint(x, y, pe, ne);
            }).ToArray();
            withErr.AddSeries(new ErrorBarSeries(pts));

            var (bmpEmpty, _) = Render(empty);
            var (bmpErr, _) = Render(withErr);
            int diff = CountDiff(bmpEmpty, bmpErr);
            Assert.True(diff > 150, $"Expected rendered ErrorBars to differ from empty baseline, diff={diff}");
        }

        [Fact]
        public void AutoFit_ShouldCoverOhlcAndErrorRanges()
        {
            var model = new ChartModel();
            var ohlc = new OhlcSeries(new []
            {
                new OhlcPoint(10, 100, 110, 95, 105),
                new OhlcPoint(20, 105, 120, 101, 118)
            });
            var errs = new ErrorBarSeries(new []
            {
                new ErrorBarPoint(15, 80, 5, 3),
                new ErrorBarPoint(25, 60, 7, 4)
            });
            model.AddSeries(ohlc);
            model.AddSeries(errs);
            // AutoFit called by AddSeries; still can call explicitly
            model.AutoFitDataRange();
            var xr = model.XAxis.DataRange; var yr = model.YAxis.DataRange;
            Assert.True(xr.Min <= 10 && xr.Max >= 25, $"Unexpected X range: {xr.Min}..{xr.Max}");
            // Highest OHLC high is 120; ensure max >= 120 (no enforced padding in AutoFit)
            Assert.True(yr.Min <= 56 && yr.Max >= 120, $"Unexpected Y range: {yr.Min}..{yr.Max}");
        }
    }
}
