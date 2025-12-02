using System;
using FastCharts.Core.Annotations;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests.Annotations
{
    /// <summary>
    /// Tests for AnnotationLine implementation (P1-ANN-LINE)
    /// </summary>
    public class AnnotationLineTests
    {
        [Fact]
        public void AnnotationLine_Constructor_Horizontal_InitializesCorrectly()
        {
            // Arrange & Act
            var line = new AnnotationLine(50.0, AnnotationOrientation.Horizontal);

            // Assert
            Assert.Equal(50.0, line.Value);
            Assert.Equal(AnnotationOrientation.Horizontal, line.Orientation);
            Assert.True(line.IsVisible);
            Assert.Equal(0, line.ZIndex);
            Assert.False(line.IncludeInAutoFit);
            Assert.Equal("Y = 50.00", line.Title);
        }

        [Fact]
        public void AnnotationLine_Constructor_Vertical_InitializesCorrectly()
        {
            // Arrange & Act
            var line = new AnnotationLine(25.5, AnnotationOrientation.Vertical);

            // Assert
            Assert.Equal(25.5, line.Value);
            Assert.Equal(AnnotationOrientation.Vertical, line.Orientation);
            Assert.Equal("X = 25.50", line.Title);
        }

        [Fact]
        public void AnnotationLine_Constructor_Horizontal_WithCustomTitle_UsesProvidedTitle()
        {
            // Arrange & Act
            var line = new AnnotationLine(100.0, AnnotationOrientation.Horizontal, "Support Level");

            // Assert
            Assert.Equal("Support Level", line.Title);
        }

        [Fact]
        public void AnnotationLine_Constructor_Vertical_WithCustomTitle_UsesProvidedTitle()
        {
            // Arrange & Act
            var line = new AnnotationLine(75.0, AnnotationOrientation.Vertical, "Important Date");

            // Assert
            Assert.Equal("Important Date", line.Title);
        }

        [Fact]
        public void AnnotationLine_Properties_CanBeModified()
        {
            // Arrange
            var line = new AnnotationLine(0, AnnotationOrientation.Horizontal);

            // Act
            line.Value = 5;
            line.Title = "New Title";
            line.IsVisible = false;
            line.Color = new ColorRgba(255, 0, 0, 255);
            line.Thickness = 2.5;

            // Assert
            Assert.Equal(5, line.Value);
            Assert.Equal("New Title", line.Title);
            Assert.False(line.IsVisible);
            Assert.Equal(new ColorRgba(255, 0, 0, 255), line.Color);
            Assert.Equal(2.5, line.Thickness);
        }

        [Fact]
        public void AnnotationLine_DefaultTitle_GeneratesCorrectFormat()
        {
            // Arrange & Act
            var horizontalLine = new AnnotationLine(42.5, AnnotationOrientation.Horizontal);
            var verticalLine = new AnnotationLine(15.3, AnnotationOrientation.Vertical);

            // Assert
            Assert.Equal("Y = 42.50", horizontalLine.Title);
            Assert.Equal("X = 15.30", verticalLine.Title);
        }

        [Fact]
        public void AnnotationLine_LineStyle_CanBeSet()
        {
            // Arrange
            var line = new AnnotationLine(10, AnnotationOrientation.Horizontal);

            // Act
            line.LineStyle = LineStyle.Dashed;

            // Assert
            Assert.Equal(LineStyle.Dashed, line.LineStyle);
        }

        [Fact]
        public void AnnotationLine_LabelPosition_CanBeSet()
        {
            // Arrange
            var line = new AnnotationLine(10, AnnotationOrientation.Horizontal);

            // Act
            line.LabelPosition = LabelPosition.Middle;

            // Assert
            Assert.Equal(LabelPosition.Middle, line.LabelPosition);
        }

        [Fact]
        public void AnnotationLine_ShowLabel_CanBeToggled()
        {
            // Arrange
            var line = new AnnotationLine(10, AnnotationOrientation.Horizontal);

            // Act
            line.ShowLabel = false;

            // Assert
            Assert.False(line.ShowLabel);
        }

        [Fact]
        public void AnnotationLine_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var line = new AnnotationLine(10, AnnotationOrientation.Horizontal);

            // Assert
            Assert.True(line.IsVisible);
            Assert.Equal(0, line.ZIndex);
            Assert.False(line.IncludeInAutoFit);
            Assert.Equal(1.0, line.Thickness);
            Assert.Equal(LineStyle.Solid, line.LineStyle);
            Assert.Equal(LabelPosition.End, line.LabelPosition);
            Assert.True(line.ShowLabel);
        }

        [Fact]
        public void AnnotationLine_Color_HasValidDefault()
        {
            // Arrange & Act
            var line = new AnnotationLine(10, AnnotationOrientation.Horizontal);

            // Assert - Default color should be semi-transparent gray
            Assert.Equal(128, line.Color.R);
            Assert.Equal(128, line.Color.G);
            Assert.Equal(128, line.Color.B);
            Assert.Equal(180, line.Color.A);
        }

        [Fact]
        public void AnnotationLine_Constructor_DefaultOrientation_IsHorizontal()
        {
            // Arrange & Act - Use constructor with default orientation
            var line = new AnnotationLine(42.0);

            // Assert
            Assert.Equal(AnnotationOrientation.Horizontal, line.Orientation);
            Assert.Equal("Y = 42.00", line.Title);
        }
    }
}