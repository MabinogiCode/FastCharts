using System;
using FastCharts.Core.Annotations;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests.Annotations
{
    /// <summary>
    /// Tests for AnnotationRange implementation (P1-ANN-RANGE)
    /// </summary>
    public class AnnotationRangeTests
    {
        [Fact]
        public void AnnotationRange_Constructor_Horizontal_InitializesCorrectly()
        {
            // Arrange & Act
            var range = new AnnotationRange(10.0, 50.0, AnnotationOrientation.Horizontal);

            // Assert
            Assert.Equal(10.0, range.StartValue);
            Assert.Equal(50.0, range.EndValue);
            Assert.Equal(AnnotationOrientation.Horizontal, range.Orientation);
            Assert.True(range.IsVisible);
            Assert.Equal(0, range.ZIndex);
            Assert.False(range.IncludeInAutoFit);
            Assert.Contains("Y Range [10.00 - 50.00]", range.Title);
        }

        [Fact]
        public void AnnotationRange_Constructor_Vertical_InitializesCorrectly()
        {
            // Arrange & Act
            var range = new AnnotationRange(25.5, 75.8, AnnotationOrientation.Vertical);

            // Assert
            Assert.Equal(25.5, range.StartValue);
            Assert.Equal(75.8, range.EndValue);
            Assert.Equal(AnnotationOrientation.Vertical, range.Orientation);
            Assert.Contains("X Range [25.50 - 75.80]", range.Title);
        }

        [Fact]
        public void AnnotationRange_Constructor_SwappedValues_AutoSorts()
        {
            // Arrange & Act - Pass values in reverse order
            var range = new AnnotationRange(100.0, 20.0, AnnotationOrientation.Horizontal);

            // Assert - Should be automatically sorted
            Assert.Equal(20.0, range.StartValue);
            Assert.Equal(100.0, range.EndValue);
        }

        [Fact]
        public void AnnotationRange_Constructor_WithCustomTitle_UsesProvidedTitle()
        {
            // Arrange & Act
            var range = new AnnotationRange(50.0, 100.0, AnnotationOrientation.Horizontal, "Target Zone");

            // Assert
            Assert.Equal("Target Zone", range.Title);
        }

        [Fact]
        public void AnnotationRange_SetStartValue_UpdatesAndMaintainsOrder()
        {
            // Arrange
            var range = new AnnotationRange(10.0, 50.0, AnnotationOrientation.Horizontal);
            
            // Act - Set start value higher than end
            range.StartValue = 60.0;

            // Assert - Should swap values to maintain order
            Assert.Equal(50.0, range.StartValue);
            Assert.Equal(60.0, range.EndValue);
        }

        [Fact]
        public void AnnotationRange_SetEndValue_UpdatesAndMaintainsOrder()
        {
            // Arrange
            var range = new AnnotationRange(10.0, 50.0, AnnotationOrientation.Horizontal);
            
            // Act - Set end value lower than start
            range.EndValue = 5.0;

            // Assert - Should swap values to maintain order
            Assert.Equal(5.0, range.StartValue);
            Assert.Equal(10.0, range.EndValue);
        }

        [Fact]
        public void AnnotationRange_Span_CalculatesCorrectly()
        {
            // Arrange
            var range = new AnnotationRange(10.0, 30.0, AnnotationOrientation.Horizontal);

            // Act & Assert
            Assert.Equal(20.0, range.Span);
        }

        [Fact]
        public void AnnotationRange_Center_CalculatesCorrectly()
        {
            // Arrange
            var range = new AnnotationRange(10.0, 30.0, AnnotationOrientation.Horizontal);

            // Act & Assert
            Assert.Equal(20.0, range.Center);
        }

        [Theory]
        [InlineData(15.0, true)]  // Within range
        [InlineData(10.0, true)]  // At start
        [InlineData(30.0, true)]  // At end
        [InlineData(5.0, false)]  // Before range
        [InlineData(35.0, false)] // After range
        public void AnnotationRange_Contains_ReturnsCorrectResult(double value, bool expected)
        {
            // Arrange
            var range = new AnnotationRange(10.0, 30.0, AnnotationOrientation.Horizontal);

            // Act & Assert
            Assert.Equal(expected, range.Contains(value));
        }

        [Fact]
        public void AnnotationRange_Colors_CanBeSetAndRetrieved()
        {
            // Arrange
            var range = new AnnotationRange(0, 10, AnnotationOrientation.Horizontal);
            var fillColor = new ColorRgba(255, 128, 64, 100);
            var borderColor = new ColorRgba(255, 0, 0, 200);

            // Act
            range.FillColor = fillColor;
            range.BorderColor = borderColor;

            // Assert
            Assert.Equal(fillColor, range.FillColor);
            Assert.Equal(borderColor, range.BorderColor);
        }

        [Fact]
        public void AnnotationRange_BorderThickness_CanBeSet()
        {
            // Arrange
            var range = new AnnotationRange(0, 10, AnnotationOrientation.Horizontal);

            // Act
            range.BorderThickness = 2.5;

            // Assert
            Assert.Equal(2.5, range.BorderThickness);
        }

        [Fact]
        public void AnnotationRange_LabelPosition_CanBeSet()
        {
            // Arrange
            var range = new AnnotationRange(0, 10, AnnotationOrientation.Horizontal);

            // Act
            range.LabelPosition = LabelPosition.End;

            // Assert
            Assert.Equal(LabelPosition.End, range.LabelPosition);
        }

        [Fact]
        public void AnnotationRange_ToString_ReturnsDescriptiveString()
        {
            // Arrange
            var horizontalRange = new AnnotationRange(10.0, 30.0, AnnotationOrientation.Horizontal);
            var verticalRange = new AnnotationRange(5.0, 15.0, AnnotationOrientation.Vertical);

            // Act & Assert
            Assert.Contains("Horizontal Range: [10.00 - 30.00]", horizontalRange.ToString());
            Assert.Contains("Vertical Range: [5.00 - 15.00]", verticalRange.ToString());
        }

        [Fact]
        public void AnnotationRange_Properties_CanBeModified()
        {
            // Arrange
            var range = new AnnotationRange(0, 10, AnnotationOrientation.Horizontal);

            // Act - Test that properties are mutable
            range.Title = "Modified Title";
            range.IsVisible = false;
            range.ZIndex = 5;
            range.Orientation = AnnotationOrientation.Vertical;

            // Assert
            Assert.Equal("Modified Title", range.Title);
            Assert.False(range.IsVisible);
            Assert.Equal(5, range.ZIndex);
            Assert.Equal(AnnotationOrientation.Vertical, range.Orientation);
        }

        [Fact]
        public void AnnotationRange_DefaultTitle_UpdatesWhenValuesChange()
        {
            // Arrange - Use default title
            var range = new AnnotationRange(10.0, 20.0, AnnotationOrientation.Horizontal);
            var originalTitle = range.Title;

            // Act
            range.StartValue = 15.0;

            // Assert - Title should update to reflect new values
            Assert.NotEqual(originalTitle, range.Title);
            Assert.Contains("15.00", range.Title);
        }

        [Fact]
        public void AnnotationRange_CustomTitle_DoesNotUpdateWhenValuesChange()
        {
            // Arrange - Use custom title
            var range = new AnnotationRange(10.0, 20.0, AnnotationOrientation.Horizontal, "Custom Title");

            // Act
            range.StartValue = 15.0;

            // Assert - Custom title should not change
            Assert.Equal("Custom Title", range.Title);
        }
    }
}