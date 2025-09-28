using FastCharts.Core.Utilities;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class DoubleUtilsTests
{
    [Fact]
    public void AreEqual_WithEqualValues_ShouldReturnTrue()
    {
        var a = 1.0;
        var b = 1.0;
        
        DoubleUtils.AreEqual(a, b).Should().BeTrue();
    }

    [Fact] 
    public void AreEqual_WithVeryCloseValues_ShouldReturnTrue()
    {
        var a = 0.1 + 0.2;
        var b = 0.3;
        
        DoubleUtils.AreEqual(a, b).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WithDifferentValues_ShouldReturnFalse()
    {
        var a = 1.0;
        var b = 2.0;
        
        DoubleUtils.AreEqual(a, b).Should().BeFalse();
    }

    [Fact]
    public void IsZero_WithZero_ShouldReturnTrue()
    {
        var value = 0.0;
        
        DoubleUtils.IsZero(value).Should().BeTrue();
    }

    [Fact]
    public void IsZero_WithVerySmallValue_ShouldReturnTrue()
    {
        var value = 1e-16;
        
        DoubleUtils.IsZero(value).Should().BeTrue();
    }

    [Fact]
    public void IsZero_WithNonZeroValue_ShouldReturnFalse()
    {
        var value = 1.0;
        
        DoubleUtils.IsZero(value).Should().BeFalse();
    }

    [Fact]
    public void AreEqual_WithNaN_ShouldHandleCorrectly()
    {
        var a = double.NaN;
        var b = double.NaN;
        
        DoubleUtils.AreEqual(a, b).Should().BeTrue();
        DoubleUtils.AreEqual(a, 1.0).Should().BeFalse();
    }

    [Fact]
    public void AreEqual_WithInfinity_ShouldHandleCorrectly()
    {
        var a = double.PositiveInfinity;
        var b = double.PositiveInfinity;
        var c = double.NegativeInfinity;
        
        DoubleUtils.AreEqual(a, b).Should().BeTrue();
        DoubleUtils.AreEqual(a, c).Should().BeFalse();
    }

    [Fact]
    public void AreNotEqual_ShouldBeOppositeOfAreEqual()
    {
        var a = 1.0;
        var b = 1.0;
        var c = 2.0;
        
        DoubleUtils.AreNotEqual(a, b).Should().BeFalse();
        DoubleUtils.AreNotEqual(a, c).Should().BeTrue();
    }

    [Fact]
    public void IsNotZero_ShouldBeOppositeOfIsZero()
    {
        var zero = 0.0;
        var nonZero = 1.0;
        
        DoubleUtils.IsNotZero(zero).Should().BeFalse();
        DoubleUtils.IsNotZero(nonZero).Should().BeTrue();
    }

    [Fact]
    public void AreEqual_WithCustomEpsilon_ShouldRespectEpsilon()
    {
        var a = 1.0;
        var b = 1.1;
        var largeEpsilon = 0.2;
        var smallEpsilon = 0.05;
        
        DoubleUtils.AreEqual(a, b, largeEpsilon).Should().BeTrue();
        DoubleUtils.AreEqual(a, b, smallEpsilon).Should().BeFalse();
    }
}