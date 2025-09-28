using FastCharts.Core.Formatting;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class SuffixNumberFormatterTests
{
    [Theory]
    [InlineData(1500, "1.5k")]
    [InlineData(2500000, "2.5M")]
    [InlineData(1200000000, "1.2B")]
    [InlineData(1.5e12, "1.5T")]
    [InlineData(2.3e15, "2.3P")]
    [InlineData(4.7e18, "4.7E")]
    public void FormatShouldUseLargeSuffixesForLargeNumbers(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.5, "500m")]
    [InlineData(0.0025, "2.5m")]
    [InlineData(0.000005, "5?")]
    [InlineData(0.000000008, "8n")]
    [InlineData(0.000000000012, "12p")]
    public void FormatShouldUseSmallSuffixesForSmallNumbers(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(10, "10")]
    [InlineData(100, "100")]
    [InlineData(999, "999")]
    public void FormatShouldUseNoSuffixForIntermediateNumbers(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 1500.0, "1500k")]     // 0 decimals
    [InlineData(1, 1500.0, "1.5k")]     // 1 decimal
    [InlineData(3, 1530.0, "1.53k")]    // 3 decimals
    [InlineData(6, 1530.123, "1.530123k")] // 6 decimals
    public void FormatShouldRespectMaxDecimals(int maxDecimals, double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter(maxDecimals);

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000.0, "1k")]      // Should trim trailing zero
    [InlineData(1500.0, "1.5k")]    // Should not trim non-zero
    [InlineData(1050.0, "1.05k")]   // Should trim one zero
    [InlineData(1005.0, "1.005k")]  // Should not trim inner zero
    public void FormatShouldTrimTrailingZeros(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter(maxDecimals: 3);

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    [InlineData(0.0, "0")]
    public void FormatShouldHandleSpecialValues(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1500, "-1.5k")]
    [InlineData(-2500000, "-2.5M")]
    [InlineData(-0.005, "-5m")]
    [InlineData(-0.000008, "-8?")]
    public void FormatShouldHandleNegativeValues(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ConstructorShouldClampMaxDecimalsWhenOutOfRange()
    {
        // Arrange & Act
        var formatter1 = new SuffixNumberFormatter(-5);  // Below 0
        var formatter2 = new SuffixNumberFormatter(10);  // Above 6

        // Test with a value that would show decimals
        var testValue = 1530.0;

        // Assert
        var result1 = formatter1.Format(testValue);
        var result2 = formatter2.Format(testValue);
        
        result1.Should().Be("1530"); // 0 decimals (clamped)
        result2.Should().Be("1.53k"); // Should be clamped to reasonable max
    }

    [Theory]
    [InlineData(1.0e19, "10E")]  // Beyond exa (1e18)
    [InlineData(1.0e-12, "1p")]  // At pico threshold
    [InlineData(5.0e-13, "500p")] // Below pico threshold
    public void FormatShouldHandleExtremeValues(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatShouldBeConsistentWithMultipleCalls()
    {
        // Arrange
        var formatter = new SuffixNumberFormatter();
        var value = 1234567.89;

        // Act
        var result1 = formatter.Format(value);
        var result2 = formatter.Format(value);

        // Assert
        result1.Should().Be(result2);
        result1.Should().Be("1.23M");
    }

    [Theory]
    [InlineData(999.9, "999.9")]    // Just below k threshold
    [InlineData(1000.0, "1k")]      // At k threshold
    [InlineData(999999.9, "999.9k")] // Just below M threshold
    [InlineData(1000000.0, "1M")]   // At M threshold
    public void FormatShouldHandleThresholdBoundaries(double value, string expected)
    {
        // Arrange
        var formatter = new SuffixNumberFormatter(maxDecimals: 1);

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }
}