using System;
using System.Collections.Generic;

using FastCharts.Core;              // ChartModel
using FastCharts.Core.Axes;         // AxisExtensions
using FastCharts.Core.Primitives;   // PointD
using FastCharts.Core.Series;       // LineSeries
using FastCharts.Rendering.Skia;    // SkiaChartRenderer
using FastCharts.Tests.Helpers;

using SkiaSharp;

using Xunit;

namespace FastCharts.Tests
{
    public class SkiaChartRendererClipAndMappingTests
    {
        [Fact]
        public void PlotIsHardClippedMarginsUnaffected()
        {
            // Arrange
            var model = new ChartModel();
            // Simple visible ranges
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            // Basic series (diagonal) via points constructor
            var points = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(100, 100)
            };
            model.Series.Add(new LineSeries(points));

            const int W = 420, H = 300;
            using (var bmp = new SKBitmap(W, H, true))
            using (var canvas = new SKCanvas(bmp))
            {
                // Act
                var renderer = new SkiaChartRenderer();
                renderer.Render(model, canvas, W, H);
                canvas.Flush();

                // Effective margins come from model (default values if not modified)
                var margins = model.PlotMargins;
                var left = (int)margins.Left;
                var top = (int)margins.Top;
                var right = (int)margins.Right;
                var bottom = (int)margins.Bottom;

                var plotW = Math.Max(0, W - (left + right));
                var plotH = Math.Max(0, H - (top + bottom));

                // Surface color = top-left corner (outside plot area)
                var surfaceColor = bmp.GetPixel(1, 1);

                // Assert (margins should remain at surface color)
                // left margin (vertical center)
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(Math.Max(1, left / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // right margin
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(W - Math.Max(2, right / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // top margin
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), Math.Max(1, top / 2)), surfaceColor));
                // bottom margin
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), H - Math.Max(2, bottom / 2)), surfaceColor));
            }
        }

        [Fact]
        public void DiagonalMapsFromBottomLeftToTopRightInsidePlot()
        {
            // Arrange
            var model = new ChartModel();
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            var points = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(100, 100)
            };
            model.Series.Add(new LineSeries(points));

            const int W = 420, H = 300;
            using (var bmp = new SKBitmap(W, H, true))
            using (var canvas = new SKCanvas(bmp))
            {
                // Act
                var renderer = new SkiaChartRenderer();
                renderer.Render(model, canvas, W, H);
                canvas.Flush();

                var m = model.PlotMargins;
                var left = (int)m.Left;
                var top = (int)m.Top;
                var right = (int)m.Right;
                var bottom = (int)m.Bottom;

                var plotW = Math.Max(0, W - (left + right));
                var plotH = Math.Max(0, H - (top + bottom));

                // Get plot color from the center of plot area
                var plotCenter = bmp.GetPixel(left + plotW / 2, top + plotH / 2);

                // We expect the diagonal to cross:
                //  - near bottom-left corner of plot
                //  - near top-right corner of plot
                // To be robust against anti-aliasing, we probe a small disk with radius 2 px and
                // verify that the color differs noticeably from the plot background.
                Assert.True(SkiaTestHelper.ProbeNotColor(bmp, new System.Drawing.Point(left + 3, top + plotH - 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near bottom-left inside plot.");
                Assert.True(SkiaTestHelper.ProbeNotColor(bmp, new System.Drawing.Point(left + plotW - 3, top + 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near top-right inside plot.");
            }
        }
    }
}
