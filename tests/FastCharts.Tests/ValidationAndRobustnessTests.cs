using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace FastCharts.Tests
{
    /// <summary>
    /// ? NEW: Tests for improved validation and robustness
    /// </summary>
    public class ValidationAndRobustnessTests
    {
        [Fact]
        public void ChartModel_ZoomAt_ValidatesParameters()
        {
            // Arrange
            var model = new ChartModel();
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            // Act & Assert - Invalid zoom factors
            model.Invoking(m => m.ZoomAt(double.NaN, 1.0, 50, 50))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("factorX");

            model.Invoking(m => m.ZoomAt(0, 1.0, 50, 50))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("factorX");

            model.Invoking(m => m.ZoomAt(1.0, double.PositiveInfinity, 50, 50))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("factorY");

            // Invalid center coordinates
            model.Invoking(m => m.ZoomAt(1.0, 1.0, double.NaN, 50))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("centerDataX");
        }

        [Fact]
        public void ChartModel_Pan_ValidatesParameters()
        {
            // Arrange
            var model = new ChartModel();
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            // Act & Assert
            model.Invoking(m => m.Pan(double.NaN, 10))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("deltaDataX");

            model.Invoking(m => m.Pan(10, double.PositiveInfinity))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("deltaDataY");
        }

        [Fact]
        public void SkiaChartRenderer_Render_ValidatesParameters()
        {
            // Arrange
            var renderer = new SkiaChartRenderer();
            var model = new ChartModel();
            using var bitmap = new SKBitmap(100, 100);
            using var canvas = new SKCanvas(bitmap);

            // Act & Assert
            renderer.Invoking(r => r.Render(null!, canvas, 100, 100))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("model");

            renderer.Invoking(r => r.Render(model, null!, 100, 100))
                .Should().Throw<ArgumentNullException>()
                .WithParameterName("canvas");

            renderer.Invoking(r => r.Render(model, canvas, 0, 100))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("pixelWidth");

            renderer.Invoking(r => r.Render(model, canvas, 100, -5))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("pixelHeight");
        }

        [Fact]
        public void SkiaChartRenderer_Render_ValidatesRanges()
        {
            // Arrange
            var renderer = new SkiaChartRenderer();
            var model = new ChartModel();
            
            // Set up a valid model first
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);
            
            // Then directly set an invalid range (bypassing SetVisibleRange validation)
            ((AxisBase)model.XAxis).VisibleRange = new FRange(double.NaN, 100);
            
            using var bitmap = new SKBitmap(100, 100);
            using var canvas = new SKCanvas(bitmap);

            // Act & Assert
            renderer.Invoking(r => r.Render(model, canvas, 100, 100))
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*X-axis visible range is invalid*");
        }

        [Fact]
        public async Task SkiaChartRenderer_RenderAsync_SupportsCancellation()
        {
            // Arrange
            var renderer = new SkiaChartRenderer();
            var model = new ChartModel();
            var points = Enumerable.Range(0, 1000)
                .Select(i => new PointD(i, Math.Sin(i * 0.1)))
                .ToArray();
            model.AddSeries(new LineSeries(points));
            
            using var bitmap = new SKBitmap(1000, 1000);
            using var canvas = new SKCanvas(bitmap);
            using var cts = new CancellationTokenSource();

            // Act
            cts.Cancel(); // Cancel immediately

            // Assert
            await renderer.Invoking(r => r.RenderAsync(model, canvas, 1000, 1000, cts.Token))
                .Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task SkiaChartRenderer_ExportPngAsync_WorksCorrectly()
        {
            // Arrange
            var renderer = new SkiaChartRenderer();
            var model = new ChartModel();
            var points = new[] { new PointD(0, 0), new PointD(100, 100) };
            model.AddSeries(new LineSeries(points));
            
            using var stream = new MemoryStream();

            // Act
            await renderer.ExportPngAsync(model, stream, 200, 200);

            // Assert
            stream.Length.Should().BeGreaterThan(0);
            stream.Position = 0;
            
            // Verify PNG header
            var header = new byte[8];
            await stream.ReadAsync(header, 0, 8);
            header.Should().StartWith(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG signature
        }

        [Fact]
        public void ChartModel_Dispose_HandlesResourcesCorrectly()
        {
            // Arrange
            var model = new ChartModel();
            var points = new[] { new PointD(0, 0), new PointD(100, 100) };
            model.AddSeries(new LineSeries(points));

            // Act & Assert - Should not throw
            model.Invoking(m => m.Dispose()).Should().NotThrow();
            
            // Multiple dispose calls should be safe
            model.Invoking(m => m.Dispose()).Should().NotThrow();
        }
    }
}