using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.Axes;

public class AxisCoordinatesTests
{
    [Fact]
    public void ToNormalizedOnLinearAxisIsProportional()
    {
        // Arrange
        var axis = new NumericAxis { VisibleRange = new FRange(0, 200) };

        // Act
        var t = AxisCoordinates.ToNormalized(axis, 50);

        // Assert
        t.Should().BeApproximately(0.25, 1e-12);
    }

    [Fact]
    public void ToNormalizedOnLogAxisIsLogarithmic()
    {
        // Arrange
        var axis = new LogNumericAxis();
        axis.SetVisibleRange(1, 100);

        // Act
        var t = AxisCoordinates.ToNormalized(axis, 10);

        // Assert
        t.Should().BeApproximately(0.5, 1e-12);
    }

    [Fact]
    public void FromNormalizedOnLogAxisIsInverseOfToNormalized()
    {
        // Arrange
        var axis = new LogarithmicAxis();
        axis.SetVisibleRange(1, 1000);

        // Act
        var value = AxisCoordinates.FromNormalized(axis, AxisCoordinates.ToNormalized(axis, 42));

        // Assert
        value.Should().BeApproximately(42, 1e-9);
    }

    [Fact]
    public void ToNormalizedOnLogAxisClampsNonPositiveValues()
    {
        // Arrange
        var axis = new LogNumericAxis();
        axis.SetVisibleRange(1, 100);

        // Act
        var t = AxisCoordinates.ToNormalized(axis, -5);

        // Assert
        double.IsNaN(t).Should().BeFalse();
        double.IsInfinity(t).Should().BeFalse();
        t.Should().BeLessThan(0);
    }

    [Fact]
    public void ToNormalizedWithZeroSpanReturnsZero()
    {
        // Arrange
        var axis = new NumericAxis { VisibleRange = new FRange(5, 5) };

        // Act
        var t = AxisCoordinates.ToNormalized(axis, 7);

        // Assert
        t.Should().Be(0);
    }

    [Fact]
    public void FollowRelativeAppliesSameRelativeZoomToFollower()
    {
        // Arrange
        var leader = new NumericAxis();
        var follower = new NumericAxis { VisibleRange = new FRange(100, 200) };

        // Act: leader zooms on the upper half of [0, 10]
        AxisCoordinates.FollowRelative(leader, new FRange(0, 10), new FRange(5, 10), follower);

        // Assert
        follower.VisibleRange.Min.Should().BeApproximately(150, 1e-9);
        follower.VisibleRange.Max.Should().BeApproximately(200, 1e-9);
    }

    [Fact]
    public void FollowRelativeAppliesSameRelativePanToFollower()
    {
        // Arrange
        var leader = new NumericAxis();
        var follower = new NumericAxis { VisibleRange = new FRange(100, 200) };

        // Act: leader pans by 10% of its span
        AxisCoordinates.FollowRelative(leader, new FRange(0, 10), new FRange(1, 11), follower);

        // Assert
        follower.VisibleRange.Min.Should().BeApproximately(110, 1e-9);
        follower.VisibleRange.Max.Should().BeApproximately(210, 1e-9);
    }

    [Fact]
    public void FollowRelativeIgnoresInvalidLeaderRange()
    {
        // Arrange
        var leader = new NumericAxis();
        var follower = new NumericAxis { VisibleRange = new FRange(100, 200) };

        // Act
        AxisCoordinates.FollowRelative(leader, new FRange(3, 3), new FRange(0, 10), follower);

        // Assert
        follower.VisibleRange.Should().Be(new FRange(100, 200));
    }
}
