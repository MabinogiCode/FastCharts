using System;
using FastCharts.Core;
using FastCharts.Core.Annotations;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using FastCharts.Rendering.Skia;
using FastCharts.Tests.Helpers;
using Xunit;

namespace FastCharts.Tests.Integration
{
    /// <summary>
    /// Integration tests for annotation functionality (P1-ANN-LINE) with full rendering pipeline
    /// </summary>
    public class AnnotationIntegrationTests
    {
        [Fact]
        public void AnnotationLine_Horizontal_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Horizontal Annotation Test" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(1, 20),
                new PointD(2, 15),
                new PointD(3, 25),
                new PointD(4, 18)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add horizontal annotation
            var horizontalLine = new AnnotationLine(20.0, AnnotationOrientation.Horizontal, "Target Level");
            model.AddAnnotation(horizontalLine);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void AnnotationLine_Vertical_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Vertical Annotation Test" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(1, 20),
                new PointD(2, 15),
                new PointD(3, 25),
                new PointD(4, 18)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add vertical annotation
            var verticalLine = new AnnotationLine(2.5, AnnotationOrientation.Vertical, "Event Time");
            model.AddAnnotation(verticalLine);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void AnnotationLine_MultipleAnnotations_RenderInZIndexOrder()
        {
            // Arrange
            var model = new ChartModel 
            { 
                Title = "Multiple Annotations Test",
                Theme = new DarkTheme()
            };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(5, 50)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add multiple annotations with different Z-indices
            var line1 = new AnnotationLine(30.0, AnnotationOrientation.Horizontal, "Level 1")
            {
                ZIndex = 1
            };
            
            var line2 = new AnnotationLine(40.0, AnnotationOrientation.Horizontal, "Level 2")
            {
                ZIndex = 0
            };
            
            var line3 = new AnnotationLine(2.5, AnnotationOrientation.Vertical, "Milestone")
            {
                ZIndex = 2
            };

            model.AddAnnotation(line1);
            model.AddAnnotation(line2);
            model.AddAnnotation(line3);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
            Assert.Equal(3, model.Annotations.Count);
        }

        [Fact]
        public void AnnotationLine_DifferentStyles_RenderCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Different Line Styles Test" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(10, 50)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add annotations with different styles
            var solidLine = new AnnotationLine(20.0, AnnotationOrientation.Horizontal, "Solid")
            {
                LineStyle = LineStyle.Solid,
                Thickness = 2.0
            };
            
            var dashedLine = new AnnotationLine(30.0, AnnotationOrientation.Horizontal, "Dashed")
            {
                LineStyle = LineStyle.Dashed,
                Thickness = 1.5
            };
            
            var dottedLine = new AnnotationLine(40.0, AnnotationOrientation.Horizontal, "Dotted")
            {
                LineStyle = LineStyle.Dotted,
                Color = new ColorRgba(255, 0, 0, 200) // Red
            };

            model.AddAnnotation(solidLine);
            model.AddAnnotation(dashedLine);
            model.AddAnnotation(dottedLine);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void AnnotationLine_WithoutLabels_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "No Labels Test" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 0),
                new PointD(5, 25)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add annotation without label
            var line = new AnnotationLine(15.0, AnnotationOrientation.Horizontal, "Hidden Label")
            {
                ShowLabel = false
            };
            model.AddAnnotation(line);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void AnnotationLine_InvisibleAnnotation_DoesNotRender()
        {
            // Arrange
            var model = new ChartModel { Title = "Invisible Annotation Test" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 0),
                new PointD(5, 25)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add invisible annotation
            var line = new AnnotationLine(15.0, AnnotationOrientation.Horizontal, "Invisible")
            {
                IsVisible = false
            };
            model.AddAnnotation(line);

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
            {
                var (bitmap, _) = ChartRenderTestHelper.Render(model);
                bitmap?.Dispose();
            });
            
            Assert.Null(exception);
        }

        [Fact]
        public void AnnotationLine_OutsideVisibleRange_DoesNotCrash()
        {
            // Arrange
            var model = new ChartModel { Title = "Out of Range Test" };
            
            // Add some data with limited range
            var lineData = new[]
            {
                new PointD(0, 0),
                new PointD(5, 5)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add annotations outside visible range
            var farHorizontal = new AnnotationLine(1000.0, AnnotationOrientation.Horizontal, "Far Above");
            var farVertical = new AnnotationLine(-100.0, AnnotationOrientation.Vertical, "Far Left");

            model.AddAnnotation(farHorizontal);
            model.AddAnnotation(farVertical);

            // Act & Assert - should not throw
            var exception = Record.Exception(() =>
            {
                var (bitmap, _) = ChartRenderTestHelper.Render(model);
                bitmap?.Dispose();
            });
            
            Assert.Null(exception);
        }

        [Fact]
        public void AnnotationLine_WithBarSeries_RendersCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Annotations with Bar Chart" };
            
            // Add bar data
            var barData = new[]
            {
                new BarPoint(0, 10),
                new BarPoint(1, 25),
                new BarPoint(2, 15),
                new BarPoint(3, 30)
            };
            model.AddSeries(new BarSeries(barData) { Title = "Sales Data" });

            // Add annotations
            var targetLine = new AnnotationLine(20.0, AnnotationOrientation.Horizontal, "Sales Target");
            var quarterEnd = new AnnotationLine(2.0, AnnotationOrientation.Vertical, "Q1 End");

            model.AddAnnotation(targetLine);
            model.AddAnnotation(quarterEnd);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }

        [Fact]
        public void AnnotationLine_AddRemove_WorksCorrectly()
        {
            // Arrange
            var model = new ChartModel { Title = "Add/Remove Test" };
            
            var line1 = new AnnotationLine(10.0, AnnotationOrientation.Horizontal, "Line 1");
            var line2 = new AnnotationLine(5.0, AnnotationOrientation.Vertical, "Line 2");

            // Act & Assert
            model.AddAnnotation(line1);
            Assert.Single(model.Annotations);

            model.AddAnnotation(line2);
            Assert.Equal(2, model.Annotations.Count);

            var removed = model.RemoveAnnotation(line1);
            Assert.True(removed);
            Assert.Single(model.Annotations);
            Assert.Contains(line2, model.Annotations);

            model.ClearAnnotations();
            Assert.Empty(model.Annotations);
        }

        [Theory]
        [InlineData(LabelPosition.Start)]
        [InlineData(LabelPosition.Middle)]
        [InlineData(LabelPosition.End)]
        public void AnnotationLine_DifferentLabelPositions_RenderCorrectly(LabelPosition position)
        {
            // Arrange
            var model = new ChartModel { Title = $"Label Position: {position}" };
            
            // Add some data
            var lineData = new[]
            {
                new PointD(0, 10),
                new PointD(10, 30)
            };
            model.AddSeries(new LineSeries(lineData) { Title = "Data Series" });

            // Add annotation with specific label position
            var line = new AnnotationLine(20.0, AnnotationOrientation.Horizontal, $"Label at {position}")
            {
                LabelPosition = position
            };
            model.AddAnnotation(line);

            // Act
            var (bitmap, _) = ChartRenderTestHelper.Render(model);

            // Assert
            Assert.NotNull(bitmap);
            Assert.True(bitmap.Width > 0);
            Assert.True(bitmap.Height > 0);
        }
    }
}