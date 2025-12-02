using System;
using System.IO;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Series;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Core.Annotations;
using FastCharts.Rendering.Skia;
using Xunit;

namespace FastCharts.Tests.Export
{
    /// <summary>
    /// Integration tests for PNG export with various chart types and configurations (P1-EXPORT-PNG)
    /// </summary>
    public class PngExportIntegrationTests
    {
        private readonly SkiaChartRenderer _renderer = new();

        [Fact]
        public void ExportPng_LineSeriesChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateLineChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_BarSeriesChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateBarChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_CategoryAxisChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateCategoryAxisChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_AnnotatedChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateAnnotatedChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_MultiSeriesChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateMultiSeriesChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 1200, 800);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_DarkThemeChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateLineChart();
            chart.Theme = new DarkTheme();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_OhlcChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateOhlcChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_StackedBarChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateStackedBarChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_ScatterChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateScatterChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        [Fact]
        public void ExportPng_DateTimeAxisChart_ProducesValidOutput()
        {
            // Arrange
            var chart = CreateDateTimeAxisChart();
            using var stream = new MemoryStream();

            // Act
            _renderer.ExportPng(chart, stream, 800, 600);

            // Assert
            Assert.True(stream.Length > 0);
            VerifyPngHeader(stream);
        }

        private static ChartModel CreateLineChart()
        {
            var model = new ChartModel { Title = "Line Chart Export Test", Theme = new LightTheme() };
            
            var data = Enumerable.Range(0, 20)
                .Select(i => new PointD(i, Math.Sin(i * 0.3) * 50 + 50))
                .ToArray();
            
            model.AddSeries(new LineSeries(data) { Title = "Sine Wave", StrokeThickness = 2 });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateBarChart()
        {
            var model = new ChartModel { Title = "Bar Chart Export Test", Theme = new LightTheme() };
            
            var data = new[]
            {
                new BarPoint(0, 100),
                new BarPoint(1, 150),
                new BarPoint(2, 120),
                new BarPoint(3, 180),
                new BarPoint(4, 90)
            };
            
            model.AddSeries(new BarSeries(data) { Title = "Sample Bars" });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateCategoryAxisChart()
        {
            var model = new ChartModel { Title = "Category Axis Export Test", Theme = new LightTheme() };
            
            var categoryAxis = new CategoryAxis(new[] { "Q1", "Q2", "Q3", "Q4" });
            model.ReplaceXAxis(categoryAxis);
            
            var data = new[]
            {
                new BarPoint(0, 200),
                new BarPoint(1, 250),
                new BarPoint(2, 180),
                new BarPoint(3, 300)
            };
            
            model.AddSeries(new BarSeries(data) { Title = "Quarterly Data" });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateAnnotatedChart()
        {
            var model = new ChartModel { Title = "Annotated Chart Export Test", Theme = new LightTheme() };
            
            var lineData = Enumerable.Range(0, 10)
                .Select(i => new PointD(i, 100 + Math.Sin(i * 0.5) * 20))
                .ToArray();
            
            model.AddSeries(new LineSeries(lineData) { Title = "Stock Price", StrokeThickness = 2 });
            
            // Add annotations to test overlay rendering
            var supportLine = new AnnotationLine(90, AnnotationOrientation.Horizontal, "Support Level")
            {
                Color = new ColorRgba(0, 255, 0, 180),
                LineStyle = LineStyle.Dashed
            };
            
            var resistanceLine = new AnnotationLine(120, AnnotationOrientation.Horizontal, "Resistance Level")
            {
                Color = new ColorRgba(255, 0, 0, 180),
                LineStyle = LineStyle.Dashed
            };
            
            model.AddAnnotation(supportLine);
            model.AddAnnotation(resistanceLine);
            
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateMultiSeriesChart()
        {
            var model = new ChartModel { Title = "Multi-Series Export Test", Theme = new LightTheme() };
            
            // Line series
            var lineData = Enumerable.Range(0, 15)
                .Select(i => new PointD(i, Math.Sin(i * 0.4) * 30 + 70))
                .ToArray();
            model.AddSeries(new LineSeries(lineData) { Title = "Line", StrokeThickness = 2 });
            
            // Bar series
            var barData = Enumerable.Range(0, 8)
                .Select(i => new BarPoint(i * 2, 50 + i * 5))
                .ToArray();
            model.AddSeries(new BarSeries(barData) { Title = "Bars" });
            
            // Scatter series
            var scatterData = Enumerable.Range(0, 10)
                .Select(i => new PointD(i * 1.5, 60 + (i % 3) * 15))
                .ToArray();
            model.AddSeries(new ScatterSeries(scatterData) { Title = "Scatter", MarkerSize = 6 });
            
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateOhlcChart()
        {
            var model = new ChartModel { Title = "OHLC Export Test", Theme = new LightTheme() };
            
            var random = new Random(123);
            var data = Enumerable.Range(0, 10)
                .Select(i =>
                {
                    var open = 100 + i * 2;
                    var close = open + (random.NextDouble() - 0.5) * 10;
                    var high = Math.Max(open, close) + random.NextDouble() * 5;
                    var low = Math.Min(open, close) - random.NextDouble() * 5;
                    return new OhlcPoint(i, open, high, low, close);
                })
                .ToArray();
            
            model.AddSeries(new OhlcSeries(data) { Title = "OHLC Data" });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateStackedBarChart()
        {
            var model = new ChartModel { Title = "Stacked Bar Export Test", Theme = new LightTheme() };
            
            var data = Enumerable.Range(0, 5)
                .Select(i => new StackedBarPoint(i, new double[] { 20 + i * 5, 15 + i * 3, 10 + i * 2 }))
                .ToArray();
            
            model.AddSeries(new StackedBarSeries(data) { Title = "Stacked Data" });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateScatterChart()
        {
            var model = new ChartModel { Title = "Scatter Export Test", Theme = new LightTheme() };
            
            var random = new Random(456);
            var data = Enumerable.Range(0, 30)
                .Select(_ => new PointD(random.NextDouble() * 100, random.NextDouble() * 100))
                .ToArray();
            
            model.AddSeries(new ScatterSeries(data) { Title = "Random Points", MarkerSize = 5 });
            model.UpdateScales(800, 600);
            return model;
        }

        private static ChartModel CreateDateTimeAxisChart()
        {
            var model = new ChartModel { Title = "DateTime Axis Export Test", Theme = new LightTheme() };
            
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today;
            
            var dateAxis = new DateTimeAxis();
            dateAxis.SetVisibleRange(start, end);
            model.ReplaceXAxis(dateAxis);
            
            var data = Enumerable.Range(0, 8)
                .Select(i => new PointD(start.AddDays(i).ToOADate(), 50 + Math.Sin(i * 0.8) * 20))
                .ToArray();
            
            model.AddSeries(new LineSeries(data) { Title = "Daily Values", StrokeThickness = 2 });
            model.UpdateScales(800, 600);
            return model;
        }

        private static void VerifyPngHeader(MemoryStream stream)
        {
            stream.Position = 0;
            var header = new byte[8];
            stream.Read(header, 0, 8);
            var expectedHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.Equal(expectedHeader, header);
        }
    }
}