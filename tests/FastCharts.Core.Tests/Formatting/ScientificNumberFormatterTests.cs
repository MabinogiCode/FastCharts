using FastCharts.Core.Formatting;
using FluentAssertions;
using System;
using Xunit;

namespace FastCharts.Core.Tests.Formatting
{
    /// <summary>
    /// Tests for ScientificNumberFormatter
    /// </summary>
    public class ScientificNumberFormatterTests
    {
        [Fact]
        public void ScientificNumberFormatter_PowersOf10_FormatsNicely()
        {
            // Arrange
            var formatter = new ScientificNumberFormatter();

            // Act & Assert
            formatter.Format(1).Should().Be("1");
            formatter.Format(10).Should().Be("10^1");
            formatter.Format(100).Should().Be("10^2");
            formatter.Format(1000).Should().Be("10^3");
            formatter.Format(0.1).Should().Be("10^-1");
            formatter.Format(0.01).Should().Be("10^-2");
        }

        [Fact]
        public void ScientificNumberFormatter_NonPowersOf10_UsesScientificNotation()
        {
            // Arrange
            var formatter = new ScientificNumberFormatter(2);

            // Act & Assert
            // With significantDigits=2, we expect 2 significant digits total in mantissa: X.Y format
            formatter.Format(2.5).Should().Be("2.5E+00");
            formatter.Format(123.456).Should().Be("1.2E+02");  // Rounded to 2 significant digits
            formatter.Format(0.00123).Should().Be("1.2E-03");  // Rounded to 2 significant digits
        }

        [Fact]
        public void ScientificNumberFormatter_SpecialValues_HandlesCorrectly()
        {
            // Arrange
            var formatter = new ScientificNumberFormatter();

            // Act & Assert
            formatter.Format(0).Should().Be("0");
            formatter.Format(double.NaN).Should().Be("NaN");
            formatter.Format(double.PositiveInfinity).Should().Be("?");
        }

        [Fact]
        public void ScientificNumberFormatter_MultiplesOfPowersOf10_FormatsNicely()
        {
            // Arrange
            var formatter = new ScientificNumberFormatter();

            // Act & Assert
            formatter.Format(2000).Should().Be("2×10^3");
            formatter.Format(5000000).Should().Be("5×10^6");
            formatter.Format(0.003).Should().Be("3×10^-3");
        }
    }
}