using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Utilities;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Tests for DataRangeAggregator to ensure the Result struct extraction didn't break functionality.
/// </summary>
public class DataRangeAggregatorTests
{
    [Fact]
    public void AggregateWithEmptyCollectionReturnsEmptyResult()
    {
        // Arrange
        var emptySeries = new SeriesBase[0];

        // Act
        var result = DataRangeAggregator.Aggregate(emptySeries);

        // Assert
        result.HasX.Should().BeFalse();
        result.HasPrimary.Should().BeFalse();
        result.HasSecondary.Should().BeFalse();
    }

    [Fact]
    public void AggregateWithSingleLineSeriesReturnsCorrectRanges()
    {
        // Arrange
        var points = new[] { new PointD(1, 10), new PointD(2, 20), new PointD(3, 5) };
        var series = new LineSeries(points)
        {
            IsVisible = true,
            YAxisIndex = 0 // Primary axis
        };

        // Act
        var result = DataRangeAggregator.Aggregate(new[] { series });

        // Assert
        result.HasX.Should().BeTrue();
        result.HasPrimary.Should().BeTrue();
        result.HasSecondary.Should().BeFalse();
        result.XMin.Should().Be(1);
        result.XMax.Should().Be(3);
        result.PrimaryYMin.Should().Be(5);
        result.PrimaryYMax.Should().Be(20);
    }

    [Fact]
    public void AggregateWithSecondaryAxisSeriesReturnsSecondaryRanges()
    {
        // Arrange
        var points = new[] { new PointD(1, 100), new PointD(2, 200) };
        var series = new LineSeries(points)
        {
            IsVisible = true,
            YAxisIndex = 1 // Secondary axis
        };

        // Act
        var result = DataRangeAggregator.Aggregate(new[] { series });

        // Assert
        result.HasX.Should().BeTrue();
        result.HasPrimary.Should().BeFalse();
        result.HasSecondary.Should().BeTrue();
        result.XMin.Should().Be(1);
        result.XMax.Should().Be(2);
        result.SecondaryYMin.Should().Be(100);
        result.SecondaryYMax.Should().Be(200);
    }

    [Fact]
    public void AggregateWithMixedSeriesAggregatesCorrectly()
    {
        // Arrange
        var primarySeries = new LineSeries(new[] { new PointD(1, 10), new PointD(4, 40) })
        {
            IsVisible = true,
            YAxisIndex = 0
        };
        var secondarySeries = new LineSeries(new[] { new PointD(2, 100), new PointD(3, 300) })
        {
            IsVisible = true,
            YAxisIndex = 1
        };

        // Act
        var result = DataRangeAggregator.Aggregate(new[] { primarySeries, secondarySeries });

        // Assert
        result.HasX.Should().BeTrue();
        result.HasPrimary.Should().BeTrue();
        result.HasSecondary.Should().BeTrue();
        result.XMin.Should().Be(1);
        result.XMax.Should().Be(4);
        result.PrimaryYMin.Should().Be(10);
        result.PrimaryYMax.Should().Be(40);
        result.SecondaryYMin.Should().Be(100);
        result.SecondaryYMax.Should().Be(300);
    }

    [Fact]
    public void AggregateWithInvisibleSeriesIgnoresInvisibleSeries()
    {
        // Arrange
        var visibleSeries = new LineSeries(new[] { new PointD(1, 10) })
        {
            IsVisible = true
        };
        var invisibleSeries = new LineSeries(new[] { new PointD(100, 1000) })
        {
            IsVisible = false
        };

        // Act
        var result = DataRangeAggregator.Aggregate(new[] { visibleSeries, invisibleSeries });

        // Assert
        result.HasX.Should().BeTrue();
        result.XMin.Should().Be(1);
        result.XMax.Should().Be(1);
        result.PrimaryYMin.Should().Be(10);
        result.PrimaryYMax.Should().Be(10);
    }

    [Fact]
    public void DataRangeAggregatorResultConstructorSetsAllProperties()
    {
        // Act
        var result = new DataRangeAggregatorResult(
            hasX: true,
            hasPrimary: true,
            hasSecondary: false,
            xMin: 1.0,
            xMax: 10.0,
            yMin: 2.0,
            yMax: 20.0,
            y2Min: 0.0,
            y2Max: 0.0);

        // Assert
        result.HasX.Should().BeTrue();
        result.HasPrimary.Should().BeTrue();
        result.HasSecondary.Should().BeFalse();
        result.XMin.Should().Be(1.0);
        result.XMax.Should().Be(10.0);
        result.PrimaryYMin.Should().Be(2.0);
        result.PrimaryYMax.Should().Be(20.0);
        result.SecondaryYMin.Should().Be(0.0);
        result.SecondaryYMax.Should().Be(0.0);
    }
}