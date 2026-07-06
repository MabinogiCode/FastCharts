using System;
using System.IO;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Tests for the render geometry cache (v1.4, T-PERF-CACHE):
    /// cached frames must be pixel-identical to freshly built ones,
    /// and data mutations must invalidate via DataVersion.
    /// </summary>
    public class GeometryCacheTests
    {
        [Fact]
        public void DataVersion_IncrementsOnEveryMutation()
        {
            var series = new LineSeries();
            var v0 = series.DataVersion;

            series.AddPoint(new PointD(0, 0));
            var v1 = series.DataVersion;

            series.AddPoints(new[] { new PointD(1, 1) });
            var v2 = series.DataVersion;

            series.ReplacePoints(new[] { new PointD(2, 2) });
            var v3 = series.DataVersion;

            series.Clear();
            var v4 = series.DataVersion;

            v1.Should().BeGreaterThan(v0);
            v2.Should().BeGreaterThan(v1);
            v3.Should().BeGreaterThan(v2);
            v4.Should().BeGreaterThan(v3);
        }

        [Fact]
        public void DataVersion_IncrementsOnStreamingAppend()
        {
            var series = new StreamingLineSeries(maxPointCount: 10);
            var v0 = series.DataVersion;

            series.AppendPoint(new PointD(1, 1));

            series.DataVersion.Should().BeGreaterThan(v0);
        }

        [Fact]
        public void CachedFrame_IsPixelIdenticalToFirstFrame()
        {
            using var model = new ChartModel();
            model.AddSeries(new LineSeries(Enumerable.Range(0, 500).Select(i => new PointD(i, Math.Sin(i * 0.05)))) { Title = "wave" });

            var renderer = new SkiaChartRenderer();
            var first = ExportBytes(renderer, model);   // cold: builds geometry
            var second = ExportBytes(renderer, model);  // warm: cache hit

            second.Should().Equal(first, "a cache hit must render exactly the same pixels as the initial build");
        }

        [Fact]
        public void CachedFrame_WithSplineAndMarkers_IsPixelIdentical()
        {
            using var model = new ChartModel();
            var series = model.AddSeries(Enumerable.Range(0, 50).ToDictionary(i => (double)i, i => Math.Cos(i * 0.3)));
            series.Smoothing = LineSmoothing.Spline;
            series.ShowMarkers = true;

            var renderer = new SkiaChartRenderer();
            var first = ExportBytes(renderer, model);
            var second = ExportBytes(renderer, model);

            second.Should().Equal(first);
        }

        [Fact]
        public void DataMutation_InvalidatesCachedGeometry()
        {
            using var model = new ChartModel();
            var series = new LineSeries(Enumerable.Range(0, 100).Select(i => new PointD(i, 0)));
            model.AddSeries(series);

            var renderer = new SkiaChartRenderer();
            var flat = ExportBytes(renderer, model);

            // Big visible change, same point count would not fool DataVersion either
            series.ReplacePoints(Enumerable.Range(0, 100).Select(i => new PointD(i, Math.Sin(i * 0.2) * 50)));
            model.AutoFitDataRange();
            var wavy = ExportBytes(renderer, model);

            wavy.Should().NotEqual(flat, "mutated data must not render from stale cached geometry");
        }

        [Fact]
        public void ZoomChange_InvalidatesCachedGeometry()
        {
            using var model = new ChartModel();
            model.AddSeries(new LineSeries(Enumerable.Range(0, 200).Select(i => new PointD(i, Math.Sin(i * 0.1)))));

            var renderer = new SkiaChartRenderer();
            var before = ExportBytes(renderer, model);

            model.Viewport.SetVisible(new FRange(50, 100), model.YAxis.VisibleRange); // zoom in
            var after = ExportBytes(renderer, model);

            after.Should().NotEqual(before);
        }

        [Fact]
        public void RemovedSeries_IsSweptWithoutError()
        {
            using var model = new ChartModel();
            var a = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) });
            var b = new LineSeries(new[] { new PointD(0, 1), new PointD(1, 0) });
            model.AddSeries(a);
            model.AddSeries(b);

            var renderer = new SkiaChartRenderer();
            ExportBytes(renderer, model);

            model.Series.Remove(b);
            var act = () => ExportBytes(renderer, model);

            act.Should().NotThrow();
        }

        private static byte[] ExportBytes(SkiaChartRenderer renderer, ChartModel model)
        {
            using var stream = new MemoryStream();
            renderer.ExportPng(model, stream, 640, 400);
            return stream.ToArray();
        }
    }
}
