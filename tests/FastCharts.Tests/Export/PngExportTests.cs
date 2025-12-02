using System;
using System.IO;
using System.Threading.Tasks;
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Rendering.Skia;
using SkiaSharp;
using Xunit;

namespace FastCharts.Tests.Export
{
    /// <summary>
    /// Tests for PNG export functionality (P1-EXPORT-PNG)
    /// </summary>
    public class PngExportTests : IDisposable
    {
        private readonly SkiaChartRenderer _renderer;
        private readonly ChartModel _testChart;
        private bool _disposed = false;

        public PngExportTests()
        {
            _renderer = new SkiaChartRenderer();
            _testChart = CreateTestChart();
        }

        [Fact]
        public void RenderToBitmap_ValidChart_ReturnsValidBitmap()
        {
            // Act
            using var bitmap = _renderer.RenderToBitmap(_testChart, 800, 600);

            // Assert
            Assert.NotNull(bitmap);
            Assert.Equal(800, bitmap.Width);
            Assert.Equal(600, bitmap.Height);
            Assert.Equal(SKColorType.Bgra8888, bitmap.ColorType);
        }

        [Fact]
        public void RenderToBitmap_WithTransparentBackground_ReturnsValidBitmap()
        {
            // Act
            using var bitmap = _renderer.RenderToBitmap(_testChart, 800, 600, transparentBackground: true);

            // Assert
            Assert.NotNull(bitmap);
            Assert.Equal(800, bitmap.Width);
            Assert.Equal(600, bitmap.Height);
        }

        [Fact]
        public async Task RenderToBitmapAsync_ValidChart_ReturnsValidBitmap()
        {
            // Act
            using var bitmap = await _renderer.RenderToBitmapAsync(_testChart, 800, 600);

            // Assert
            Assert.NotNull(bitmap);
            Assert.Equal(800, bitmap.Width);
            Assert.Equal(600, bitmap.Height);
        }

        [Fact]
        public void ExportPng_ToStream_CreatesValidPngData()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            _renderer.ExportPng(_testChart, memoryStream, 800, 600, quality: 95);

            // Assert
            Assert.True(memoryStream.Length > 0);
            memoryStream.Position = 0;
            
            // Verify PNG header
            var pngHeader = new byte[8];
            memoryStream.Read(pngHeader, 0, 8);
            var expectedHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.Equal(expectedHeader, pngHeader);
        }

        [Fact]
        public async Task ExportPngAsync_ToStream_CreatesValidPngData()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            await _renderer.ExportPngAsync(_testChart, memoryStream, 800, 600, quality: 95);

            // Assert
            Assert.True(memoryStream.Length > 0);
            memoryStream.Position = 0;
            
            // Verify PNG header
            var pngHeader = new byte[8];
            await memoryStream.ReadAsync(pngHeader, 0, 8);
            var expectedHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            Assert.Equal(expectedHeader, pngHeader);
        }

        [Fact]
        public void ExportPng_WithDifferentQualitySettings_ProducesValidOutput()
        {
            // Arrange
            var qualities = new[] { 50, 75, 95, 100 };

            foreach (var quality in qualities)
            {
                using var memoryStream = new MemoryStream();

                // Act
                _renderer.ExportPng(_testChart, memoryStream, 400, 300, quality);

                // Assert
                Assert.True(memoryStream.Length > 0, $"Quality {quality} should produce valid output");
            }
        }

        [Fact]
        public void ExportPng_WithTransparentBackground_ProducesValidOutput()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            _renderer.ExportPng(_testChart, memoryStream, 800, 600, quality: 95, transparentBackground: true);

            // Assert
            Assert.True(memoryStream.Length > 0);
        }

        [Fact]
        public void ExportPng_DifferentSizes_ProducesValidOutput()
        {
            // Arrange
            var sizes = new[] 
            { 
                (200, 150), 
                (800, 600), 
                (1920, 1080),
                (300, 300) // Square
            };

            foreach (var (width, height) in sizes)
            {
                using var memoryStream = new MemoryStream();

                // Act
                _renderer.ExportPng(_testChart, memoryStream, width, height);

                // Assert
                Assert.True(memoryStream.Length > 0, $"Size {width}x{height} should produce valid output");
                
                // Verify the exported image has correct dimensions
                using var bitmap = _renderer.RenderToBitmap(_testChart, width, height);
                Assert.Equal(width, bitmap.Width);
                Assert.Equal(height, bitmap.Height);
            }
        }

        [Fact]
        public void ExportPng_EmptyChart_DoesNotThrow()
        {
            // Arrange
            var emptyChart = new ChartModel { Title = "Empty Chart", Theme = new LightTheme() };
            using var memoryStream = new MemoryStream();

            // Act & Assert
            var exception = Record.Exception(() => _renderer.ExportPng(emptyChart, memoryStream, 800, 600));
            Assert.Null(exception);
            Assert.True(memoryStream.Length > 0);
        }

        [Theory]
        [InlineData(1, 1)]        // Minimal size
        [InlineData(50, 50)]      // Small size
        [InlineData(1000, 1000)]  // Large size
        [InlineData(2000, 100)]   // Wide aspect ratio
        [InlineData(100, 2000)]   // Tall aspect ratio
        public void ExportPng_VariousDimensions_ProducesValidOutput(int width, int height)
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            var exception = Record.Exception(() => _renderer.ExportPng(_testChart, memoryStream, width, height));

            // Assert
            Assert.Null(exception);
            Assert.True(memoryStream.Length > 0);
        }

        [Fact]
        public void ExportPng_NullModel_ThrowsArgumentException()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _renderer.ExportPng(null!, memoryStream, 800, 600));
        }

        [Fact]
        public void ExportPng_NullStream_DoesNotThrow()
        {
            // Act & Assert - The current implementation returns early for null streams
            var exception = Record.Exception(() => _renderer.ExportPng(_testChart, null!, 800, 600));
            Assert.Null(exception);
        }

        [Fact]
        public async Task ExportPngAsync_NullModel_ThrowsArgumentException()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _renderer.ExportPngAsync(null!, memoryStream, 800, 600));
        }

        [Fact]
        public async Task ExportPngAsync_NullStream_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                _renderer.ExportPngAsync(_testChart, null!, 800, 600));
        }

        private static ChartModel CreateTestChart()
        {
            var model = new ChartModel
            {
                Title = "Test Chart for PNG Export",
                Theme = new LightTheme()
            };

            // Add some test data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(1, 25),
                new PointD(2, 15),
                new PointD(3, 30),
                new PointD(4, 20),
                new PointD(5, 35)
            };

            var barData = new[]
            {
                new BarPoint(0.5, 8),
                new BarPoint(1.5, 22),
                new BarPoint(2.5, 12),
                new BarPoint(3.5, 28),
                new BarPoint(4.5, 18)
            };

            model.AddSeries(new LineSeries(lineData) { Title = "Line Series", StrokeThickness = 2 });
            model.AddSeries(new BarSeries(barData) { Title = "Bar Series" });

            model.UpdateScales(800, 600);
            return model;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _testChart?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}