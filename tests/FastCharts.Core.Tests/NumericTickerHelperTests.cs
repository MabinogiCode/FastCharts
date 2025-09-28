using FastCharts.Core.Ticks;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class NumericTickerHelperTests
{
    [Theory]
    [InlineData(1.3, 1.0)] // m = 1.3 < 1.5 -> nice = 1
    [InlineData(1.8, 2.0)] // m = 1.8 < 3 -> nice = 2
    [InlineData(4.5, 5.0)] // m = 4.5 < 7 -> nice = 5
    [InlineData(8.5, 10.0)] // m = 8.5 >= 7 -> nice = 10
    public void CalculateNiceStep_ShouldReturnCorrectNiceValue_ForSingleDigits(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(13.0, 10.0)] // 13 -> 10 (m = 1.3 < 1.5)
    [InlineData(18.0, 20.0)] // 18 -> 20 (m = 1.8 < 3)
    [InlineData(45.0, 50.0)] // 45 -> 50 (m = 4.5 < 7)
    [InlineData(85.0, 100.0)] // 85 -> 100 (m = 8.5 >= 7)
    public void CalculateNiceStep_ShouldReturnCorrectNiceValue_ForTensDigits(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(0.13, 0.1)] // 0.13 -> 0.1 (m = 1.3 < 1.5)
    [InlineData(0.18, 0.2)] // 0.18 -> 0.2 (m = 1.8 < 3)
    [InlineData(0.45, 0.5)] // 0.45 -> 0.5 (m = 4.5 < 7)
    [InlineData(0.85, 1.0)] // 0.85 -> 1.0 (m = 8.5 >= 7)
    public void CalculateNiceStep_ShouldReturnCorrectNiceValue_ForDecimalDigits(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(1000.0, 1000.0)] // Large values
    [InlineData(2500.0, 2000.0)]
    [InlineData(7500.0, 5000.0)]
    [InlineData(15000.0, 10000.0)]
    public void CalculateNiceStep_ShouldHandleLargeValues(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(0.001, 0.001)] // Very small values
    [InlineData(0.0025, 0.002)]
    [InlineData(0.0075, 0.005)]
    [InlineData(0.015, 0.01)]
    public void CalculateNiceStep_ShouldHandleVerySmallValues(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-15);
    }

    [Theory]
    [InlineData(1.49, 1.0)] // Edge case: just below 1.5
    [InlineData(1.50, 2.0)] // Edge case: exactly 1.5
    [InlineData(2.99, 2.0)] // Edge case: just below 3
    [InlineData(3.00, 5.0)] // Edge case: exactly 3
    [InlineData(6.99, 5.0)] // Edge case: just below 7
    [InlineData(7.00, 10.0)] // Edge case: exactly 7
    public void CalculateNiceStep_ShouldHandleEdgeCases(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-10);
    }

    [Theory]
    [InlineData(1.0, 1.0)] // Perfect powers of 10
    [InlineData(10.0, 10.0)]
    [InlineData(100.0, 100.0)]
    [InlineData(0.1, 0.1)]
    [InlineData(0.01, 0.01)]
    public void CalculateNiceStep_ShouldReturnSameValue_ForPerfectPowersOfTen(double rough, double expected)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert
        result.Should().BeApproximately(expected, 1e-15);
    }

    [Fact]
    public void CalculateNiceStep_ShouldHandleZero_ByReturningSomething()
    {
        // Note: Unlike NiceTickerHelper, NumericTickerHelper doesn't handle zero explicitly
        // It will cause Math.Log10(0) which returns -Infinity, leading to interesting behavior
        // This test documents the current behavior - might need adjustment based on requirements
        
        // Act & Assert - Just ensure it doesn't throw
        var result = () => NumericTickerHelper.CalculateNiceStep(0.0);
        result.Should().NotThrow();
    }

    [Theory]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void CalculateNiceStep_ShouldHandleSpecialValues(double rough)
    {
        // Act & Assert - Just ensure it doesn't throw for special values
        var result = () => NumericTickerHelper.CalculateNiceStep(rough);
        result.Should().NotThrow();
    }

    [Theory]
    [InlineData(-13.0)] // Test negative values - note: no explicit sign handling in NumericTickerHelper
    [InlineData(-18.0)]
    [InlineData(-45.0)]
    public void CalculateNiceStep_WithNegativeValues_ShouldProduceResult(double rough)
    {
        // Act
        var result = NumericTickerHelper.CalculateNiceStep(rough);

        // Assert - Just ensure we get a finite result
        double.IsFinite(result).Should().BeTrue();
        result.Should().BePositive(); // NumericTickerHelper doesn't preserve sign
    }
}