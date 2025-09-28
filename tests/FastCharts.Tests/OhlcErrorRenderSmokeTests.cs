using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Tests.Helpers;

using Xunit;

namespace FastCharts.Tests
{
    public class OhlcErrorRenderSmokeTests
    {
        [Fact]
        public void OhlcSeriesShouldAlterBitmapComparedToEmpty()
        {
            var empty = new ChartModel();
            ChartRenderTestHelper.SetVisibleRange(empty, 0, 10, 90, 110);

            var withOhlc = new ChartModel();
            ChartRenderTestHelper.SetVisibleRange(withOhlc, 0, 10, 90, 110);

            // Simple OHLC data
            var data = Enumerable.Range(0, 8).Select(i =>
            {
                var x = i + 0.5;
                var open = 100 + Math.Sin(i * 0.3) * 2;
                var close = open + (Math.Sin(i * 0.7) * 1.5);
                var high = Math.Max(open, close) + 1.5;
                var low = Math.Min(open, close) - 1.5;
                return new OhlcPoint(x, open, high, low, close);
            }).ToArray();
            withOhlc.AddSeries(new OhlcSeries(data));

            var (bmpEmpty, _) = ChartRenderTestHelper.Render(empty);
            var (bmpOhlc, _) = ChartRenderTestHelper.Render(withOhlc);
            var diff = ChartRenderTestHelper.CountDifferingPixels(bmpEmpty, bmpOhlc);
            // Expect some thousands of differing pixels (grid + candles). Just assert > 200 to be safe.
            Assert.True(diff > 200, $"Expected rendered OHLC to differ from empty baseline, diff={diff}");
        }

        [Fact]
        public void ErrorBarSeriesShouldAlterBitmapComparedToEmpty()
        {
            var empty = new ChartModel();
            ChartRenderTestHelper.SetVisibleRange(empty, 0, 20, 0, 100);

            var withErr = new ChartModel();
            ChartRenderTestHelper.SetVisibleRange(withErr, 0, 20, 0, 100);

            var rnd = new Random(42);
            var pts = Enumerable.Range(0, 15).Select(i =>
            {
                var x = i + 0.7;
                var y = 40 + Math.Sin(i * 0.4) * 10;
                var pe = 5 + rnd.NextDouble() * 3;
                var ne = pe * (0.4 + rnd.NextDouble() * 0.6);
                return new ErrorBarPoint(x, y, pe, ne);
            }).ToArray();
            withErr.AddSeries(new ErrorBarSeries(pts));

            var (bmpEmpty, _) = ChartRenderTestHelper.Render(empty);
            var (bmpErr, _) = ChartRenderTestHelper.Render(withErr);
            var diff = ChartRenderTestHelper.CountDifferingPixels(bmpEmpty, bmpErr);
            Assert.True(diff > 150, $"Expected rendered ErrorBars to differ from empty baseline, diff={diff}");
        }

        [Fact]
        public void AutoFitShouldCoverOhlcAndErrorRanges()
        {
            var model = new ChartModel();
            var ohlc = new OhlcSeries(new[]
            {
                new OhlcPoint(10, 100, 110, 95, 105),
                new OhlcPoint(20, 105, 120, 101, 118)
            });
            var errs = new ErrorBarSeries(new[]
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
