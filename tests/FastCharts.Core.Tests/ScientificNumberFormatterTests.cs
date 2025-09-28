using FastCharts.Core.Formatting;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class ScientificNumberFormatterTests
{
    [Theory]
    [InlineData(1500000, "1.50E+06")] // Above upper threshold (1e6)
    [InlineData(2.5e6, "2.50E+06")] // Above upper threshold
    [InlineData(0.0005, "5.00E-04")] // Below lower threshold (1e-3)
    [InlineData(1e-5, "1.00E-05")] // Below lower threshold
    public void FormatShouldUseScientificNotationWhenOutsideThresholds(double value, string expected)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100000, "100000")] // Within thresholds
    [InlineData(0.5, "0.5")] // Within thresholds
    [InlineData(0.001, "0.001")] // At lower threshold
    [InlineData(999999, "999999")] // Just below upper threshold
    public void FormatShouldUseRegularNotationWhenWithinThresholds(double value, string expected)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(double.NaN, "NaN")]
    [InlineData(double.PositiveInfinity, "Infinity")]
    [InlineData(double.NegativeInfinity, "-Infinity")]
    public void FormatShouldHandleSpecialValues(double value, string expected)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "1.0E+06")] // 1 significant digit
    [InlineData(2, "1.0E+06")] // 2 significant digits
    [InlineData(4, "1.000E+06")] // 4 significant digits
    [InlineData(6, "1.00000E+06")] // 6 significant digits
    public void FormatShouldRespectSignificantDigits(int significantDigits, string expected)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter(significantDigits: significantDigits);

        // Act
        var result = formatter.Format(1000000);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1e3, 1e-6, 2)] // Custom thresholds
    [InlineData(1e9, 1e-9, 5)] // Very wide thresholds
    public void FormatShouldRespectCustomThresholds(double upperThreshold, double lowerThreshold, int significantDigits)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter(upperThreshold, lowerThreshold, significantDigits);

        // Act - Test value just above upper threshold
        var resultHigh = formatter.Format(upperThreshold * 1.1);
        var resultLow = formatter.Format(lowerThreshold * 0.9);

        // Assert
        resultHigh.Should().Contain("E+"); // Should use scientific notation
        resultLow.Should().Contain("E-"); // Should use scientific notation
    }

    [Fact]
    public void ConstructorShouldUseDefaultsWhenInvalidValuesProvided()
    {
        // Arrange & Act
        var formatter = new ScientificNumberFormatter(-1, -1, -1);

        // Assert
        formatter.UpperThreshold.Should().Be(1e6);
        formatter.LowerThreshold.Should().Be(1e-3);
        formatter.SignificantDigits.Should().Be(3);
    }

    [Fact]
    public void ConstructorShouldUseDefaultsWhenLowerThresholdIsInvalid()
    {
        // Arrange & Act
        var formatter1 = new ScientificNumberFormatter(lowerThreshold: 0);
        var formatter2 = new ScientificNumberFormatter(lowerThreshold: 2.0); // >= 1

        // Assert
        formatter1.LowerThreshold.Should().Be(1e-3);
        formatter2.LowerThreshold.Should().Be(1e-3);
    }

    [Theory]
    [InlineData(0, "0")]
    [InlineData(-1500000, "-1.50E+06")] // Negative large number
    [InlineData(-0.0005, "-5.00E-04")] // Negative small number
    public void FormatShouldHandleZeroAndNegativeValues(double value, string expected)
    {
        // Arrange
        var formatter = new ScientificNumberFormatter();

        // Act
        var result = formatter.Format(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatShouldBeConsistentWithMultipleCalls()
    {
        // Arrange
        var formatter = new ScientificNumberFormatter();
        var value = 1.23456e7;

        // Act
        var result1 = formatter.Format(value);
        var result2 = formatter.Format(value);

        // Assert
        result1.Should().Be(result2);
        result1.Should().Be("1.23E+07");
    }
}