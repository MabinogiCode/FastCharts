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
        public void AnnotationLine_Horizontal_FactoryMethod_CreatesHorizontalLine()
        {
            // Arrange & Act
            var line = AnnotationLine.Horizontal(100.0);

            // Assert
            Assert.Equal(100.0, line.Value);
            Assert.Equal(AnnotationOrientation.Horizontal, line.Orientation);
            Assert.Equal("Y = 100.00", line.Title);
        }

        [Fact]
        public void AnnotationLine_Vertical_FactoryMethod_CreatesVerticalLine()
        {
            // Arrange & Act
            var line = AnnotationLine.Vertical(75.0);

            // Assert
            Assert.Equal(75.0, line.Value);
            Assert.Equal(AnnotationOrientation.Vertical, line.Orientation);
            Assert.Equal("X = 75.00", line.Title);
        }

        [Fact]
        public void AnnotationLine_Horizontal_WithCustomTitle_UsesProvidedTitle()
        {
            // Arrange & Act
            var line = AnnotationLine.Horizontal(50.0, "Support Level");

            // Assert
            Assert.Equal("Support Level", line.Title);
        }

        [Fact]
        public void AnnotationLine_Vertical_WithCustomTitle_UsesProvidedTitle()
        {
            // Arrange & Act
            var line = AnnotationLine.Vertical(25.0, "Important Date");

            // Assert
            Assert.Equal("Important Date", line.Title);
        }

        [Fact]
        public void AnnotationLine_Properties_AreReactive()
        {
            // Arrange
            var line = new AnnotationLine(0);
            var titleChanged = false;
            var visibilityChanged = false;
            var valueChanged = false;

            line.PropertyChanged += (s, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(AnnotationLine.Title):
                        titleChanged = true;
                        break;
                    case nameof(AnnotationLine.IsVisible):
                        visibilityChanged = true;
                        break;
                    case nameof(AnnotationLine.Value):
                        valueChanged = true;
                        break;
                }
            };

            // Act
            line.Title = "New Title";
            line.IsVisible = false;
            line.Value = 123.45;

            // Assert
            Assert.True(titleChanged);
            Assert.True(visibilityChanged);
            Assert.True(valueChanged);
        }

        [Fact]
        public void AnnotationLine_Color_DefaultValue_IsGray()
        {
            // Arrange & Act
            var line = new AnnotationLine(0);

            // Assert
            Assert.Equal(128, line.Color.R);
            Assert.Equal(128, line.Color.G);
            Assert.Equal(128, line.Color.B);
            Assert.Equal(180, line.Color.A); // Semi-transparent
        }

        [Fact]
        public void AnnotationLine_Thickness_DefaultValue_IsOne()
        {
            // Arrange & Act
            var line = new AnnotationLine(0);

            // Assert
            Assert.Equal(1.0, line.Thickness);
        }

        [Fact]
        public void AnnotationLine_Thickness_SetNegative_ClampsToMinimum()
        {
            // Arrange
            var line = new AnnotationLine(0);

            // Act
            line.Thickness = -5.0;

            // Assert
            Assert.Equal(0.1, line.Thickness); // Minimum value
        }

        [Fact]
        public void AnnotationLine_LineStyle_DefaultValue_IsSolid()
        {
            // Arrange & Act
            var line = new AnnotationLine(0);

            // Assert
            Assert.Equal(LineStyle.Solid, line.LineStyle);
        }

        [Theory]
        [InlineData(LineStyle.Solid)]
        [InlineData(LineStyle.Dashed)]
        [InlineData(LineStyle.Dotted)]
        [InlineData(LineStyle.DashDot)]
        public void AnnotationLine_LineStyle_AllValues_SetCorrectly(LineStyle style)
        {
            // Arrange
            var line = new AnnotationLine(0);

            // Act
            line.LineStyle = style;

            // Assert
            Assert.Equal(style, line.LineStyle);
        }

        [Fact]
        public void AnnotationLine_LabelPosition_DefaultValue_IsEnd()
        {
            // Arrange & Act
            var line = new AnnotationLine(0);

            // Assert
            Assert.Equal(LabelPosition.End, line.LabelPosition);
        }

        [Theory]
        [InlineData(LabelPosition.Start)]
        [InlineData(LabelPosition.Middle)]
        [InlineData(LabelPosition.End)]
        public void AnnotationLine_LabelPosition_AllValues_SetCorrectly(LabelPosition position)
        {
            // Arrange
            var line = new AnnotationLine(0);

            // Act
            line.LabelPosition = position;

            // Assert
            Assert.Equal(position, line.LabelPosition);
        }

        [Fact]
        public void AnnotationLine_ShowLabel_DefaultValue_IsTrue()
        {
            // Arrange & Act
            var line = new AnnotationLine(0);

            // Assert
            Assert.True(line.ShowLabel);
        }

        [Fact]
        public void AnnotationLine_ZIndex_CanBeSetToNegative()
        {
            // Arrange
            var line = new AnnotationLine(0);

            // Act
            line.ZIndex = -10;

            // Assert
            Assert.Equal(-10, line.ZIndex);
        }

        [Fact]
        public void AnnotationLine_Color_CanBeModified()
        {
            // Arrange
            var line = new AnnotationLine(0);
            var red = new Color(255, 0, 0, 255);

            // Act
            line.Color = red;

            // Assert
            Assert.Equal(255, line.Color.R);
            Assert.Equal(0, line.Color.G);
            Assert.Equal(0, line.Color.B);
            Assert.Equal(255, line.Color.A);
        }

        [Fact]
        public void AnnotationLine_IncludeInAutoFit_CanBeEnabled()
        {
            // Arrange
            var line = new AnnotationLine(0);

            // Act
            line.IncludeInAutoFit = true;

            // Assert
            Assert.True(line.IncludeInAutoFit);
        }
    }
}