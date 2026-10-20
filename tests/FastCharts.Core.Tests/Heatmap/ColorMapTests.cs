using System;
using System.Collections.Generic;

using FastCharts.Core.Primitives;
using FastCharts.Core.Themes;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Heatmap;

/// <summary>
/// P1-AX-COLOR: continuous gradients used by color axes.
/// </summary>
public class ColorMapTests
{
    private static readonly ColorRgba Black = new ColorRgba(0, 0, 0);
    private static readonly ColorRgba White = new ColorRgba(255, 255, 255);

    [Fact]
    public void MapInterpolatesBetweenStops()
    {
        // Arrange
        var map = new ColorMap("bw", new[] { Black, White });

        // Act
        var middle = map.Map(0.5);

        // Assert
        middle.Should().Be(new ColorRgba(128, 128, 128));
    }

    [Fact]
    public void MapHitsIntermediateStopsExactly()
    {
        // Arrange
        var red = new ColorRgba(255, 0, 0);
        var map = new ColorMap("brw", new[] { Black, red, White });

        // Act / Assert
        map.Map(0.5).Should().Be(red);
        map.Map(0.25).Should().Be(new ColorRgba(128, 0, 0));
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void MapClampsLowAndNaNToFirstStop(double t)
    {
        // Arrange
        var map = new ColorMap("bw", new[] { Black, White });

        // Act / Assert
        map.Map(t).Should().Be(Black);
    }

    [Theory]
    [InlineData(1.0)]
    [InlineData(2.5)]
    [InlineData(double.PositiveInfinity)]
    public void MapClampsHighToLastStop(double t)
    {
        // Arrange
        var map = new ColorMap("bw", new[] { Black, White });

        // Act / Assert
        map.Map(t).Should().Be(White);
    }

    [Fact]
    public void ReversedSwapsEnds()
    {
        // Arrange
        var map = new ColorMap("bw", new[] { Black, White });

        // Act
        var reversed = map.Reversed();

        // Assert
        reversed.Map(0).Should().Be(White);
        reversed.Map(1).Should().Be(Black);
        reversed.Name.Should().Contain("bw");
        map.Map(0).Should().Be(Black, "reversing must not change the original map");
    }

    [Fact]
    public void ConstructorRequiresTwoStops()
    {
        FluentActions.Invoking(() => new ColorMap("one", new[] { Black })).Should().Throw<ArgumentException>();
        FluentActions.Invoking(() => new ColorMap("none", null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StopsAreACopyThatCannotBeMutated()
    {
        // Arrange
        var stops = new List<ColorRgba> { Black, White };
        var map = new ColorMap("bw", stops);

        // Act
        stops[0] = White;

        // Assert
        map.Map(0).Should().Be(Black);
        (map.Stops is ColorRgba[]).Should().BeFalse("shared built-in maps must not expose their array");
    }

    [Fact]
    public void BuiltInMapsAreUsable()
    {
        foreach (var map in new[] { ColorMaps.Viridis, ColorMaps.Plasma, ColorMaps.Grayscale, ColorMaps.BlueWhiteRed })
        {
            map.Stops.Count.Should().BeGreaterThanOrEqualTo(2);
            map.Name.Should().NotBeNullOrEmpty();
        }

        ColorMaps.Grayscale.Map(0).Should().Be(Black);
        ColorMaps.Grayscale.Map(1).Should().Be(White);
        ColorMaps.Viridis.Map(0).Should().Be(new ColorRgba(0x44, 0x01, 0x54));
        ColorMaps.Viridis.Map(1).Should().Be(new ColorRgba(0xFD, 0xE7, 0x25));
    }
}
