using System;

using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Themes;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Heatmap;

/// <summary>
/// P1-AX-COLOR: value range resolution and value-to-color mapping.
/// </summary>
public class ColorAxisTests
{
    [Fact]
    public void ResolveRangeUsesDataRangeByDefault()
    {
        // Arrange
        var axis = new ColorAxis();

        // Act
        var range = axis.ResolveRange(new FRange(-3, 7));

        // Assert
        range.Should().Be(new FRange(-3, 7));
    }

    [Fact]
    public void ResolveRangePrefersFixedBounds()
    {
        // Arrange
        var axis = new ColorAxis { Minimum = 0 };

        // Act
        var lowFixed = axis.ResolveRange(new FRange(-3, 7));
        axis.Maximum = 100;
        var bothFixed = axis.ResolveRange(new FRange(-3, 7));

        // Assert
        lowFixed.Should().Be(new FRange(0, 7));
        bothFixed.Should().Be(new FRange(0, 100));
    }

    [Fact]
    public void ResolveRangeSwapsInvertedBounds()
    {
        // Arrange
        var axis = new ColorAxis { Minimum = 10, Maximum = -10 };

        // Act
        var range = axis.ResolveRange(new FRange(0, 1));

        // Assert
        range.Should().Be(new FRange(-10, 10));
    }

    [Fact]
    public void ResolveRangeWidensASingleValue()
    {
        // Arrange
        var axis = new ColorAxis();

        // Act
        var range = axis.ResolveRange(new FRange(5, 5));

        // Assert
        range.Should().Be(new FRange(4.5, 5.5));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ResolveRangeFallsBackOnNonFiniteBounds(double bound)
    {
        // Arrange
        var axis = new ColorAxis { Maximum = bound };

        // Act
        var range = axis.ResolveRange(new FRange(0, 10));

        // Assert
        range.Should().Be(new FRange(0, 1));
    }

    [Fact]
    public void GetColorMapsValuesOntoTheGradient()
    {
        // Arrange
        var axis = new ColorAxis { ColorMap = ColorMaps.Grayscale };
        var range = new FRange(10, 20);

        // Act / Assert
        axis.GetColor(10, range).Should().Be(new ColorRgba(0, 0, 0));
        axis.GetColor(20, range).Should().Be(new ColorRgba(255, 255, 255));
        axis.GetColor(15, range).Should().Be(new ColorRgba(128, 128, 128));
        axis.GetColor(-50, range).Should().Be(new ColorRgba(0, 0, 0), "values below the range take the first color");
        axis.GetColor(500, range).Should().Be(new ColorRgba(255, 255, 255), "values above the range take the last color");
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void GetColorUsesMissingValueColorForNonFiniteValues(double value)
    {
        // Arrange
        var missing = new ColorRgba(255, 0, 255);
        var axis = new ColorAxis { MissingValueColor = missing };

        // Act / Assert
        axis.GetColor(value, new FRange(0, 1)).Should().Be(missing);
    }

    [Fact]
    public void DefaultsAreViridisTransparentMissingAndVisibleBar()
    {
        // Arrange / Act
        var axis = new ColorAxis();

        // Assert
        axis.ColorMap.Should().BeSameAs(ColorMaps.Viridis);
        axis.MissingValueColor.A.Should().Be(0);
        axis.ShowColorBar.Should().BeTrue();
        axis.Minimum.Should().BeNull();
        axis.Maximum.Should().BeNull();
    }

    [Fact]
    public void ColorMapCannotBeNull()
    {
        FluentActions.Invoking(() => new ColorAxis { ColorMap = null! }).Should().Throw<ArgumentNullException>();
    }
}
