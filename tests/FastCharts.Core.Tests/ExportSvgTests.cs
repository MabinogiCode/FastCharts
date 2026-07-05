using System;
using System.Collections.Generic;
using System.IO;
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Core.Services;
using FastCharts.Rendering.Skia;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// Tests for SVG vector export (v1.2)
    /// </summary>
    public class ExportSvgTests
    {
        private static ChartModel CreateModel()
        {
            var model = new ChartModel();
            model.AddSeries(new Dictionary<double, double> { [0] = 1, [1] = 3, [2] = 2 }, "svg-series");
            return model;
        }

        [Fact]
        public void ExportSvg_WritesSvgDocument()
        {
            using var model = CreateModel();
            var renderer = new SkiaChartRenderer();
            using var stream = new MemoryStream();

            renderer.ExportSvg(model, stream, 640, 480);

            stream.Length.Should().BeGreaterThan(0);
            var svg = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            svg.Should().Contain("<svg");
            svg.Should().Contain("640");
        }

        [Fact]
        public void ExportSvg_ContainsVectorPaths_NotJustAnImage()
        {
            using var model = CreateModel();
            var renderer = new SkiaChartRenderer();
            using var stream = new MemoryStream();

            renderer.ExportSvg(model, stream, 400, 300);

            var svg = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            svg.Should().Contain("path", "series must be exported as vector paths");
        }

        [Fact]
        public void ExportSvg_InvalidArguments_Throw()
        {
            using var model = CreateModel();
            var renderer = new SkiaChartRenderer();
            using var stream = new MemoryStream();

            FluentActions.Invoking(() => renderer.ExportSvg(null!, stream, 100, 100)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => renderer.ExportSvg(model, null!, 100, 100)).Should().Throw<ArgumentNullException>();
            FluentActions.Invoking(() => renderer.ExportSvg(model, stream, 0, 100)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ChartExportService_ExportsSvgStringAndFile()
        {
            using var model = CreateModel();
            var service = new ChartExportService(new SkiaChartRenderer());

            var svg = service.ExportToSvgString(model, 320, 240);
            svg.Should().Contain("<svg");

            var path = Path.Combine(Path.GetTempPath(), $"fastcharts-test-{Guid.NewGuid():N}.svg");
            try
            {
                service.ExportToSvgFile(model, path, 320, 240);
                File.Exists(path).Should().BeTrue();
                new FileInfo(path).Length.Should().BeGreaterThan(0);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void ExportSvg_WithSplineAndMarkers_Succeeds()
        {
            using var model = new ChartModel();
            var series = model.AddSeries(new Dictionary<double, double> { [0] = 1, [1] = 4, [2] = 2, [3] = 5 });
            series.Smoothing = LineSmoothing.Spline;
            series.ShowMarkers = true;
            series.MarkerShape = MarkerShape.Diamond;

            var renderer = new SkiaChartRenderer();
            using var stream = new MemoryStream();

            renderer.ExportSvg(model, stream, 500, 300);

            stream.Length.Should().BeGreaterThan(0);
        }
    }
}
