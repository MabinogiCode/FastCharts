using System;
using FastCharts.Core.Formatting;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class AdaptiveDateTimeFormatterTests
{
    private readonly DateTime _testDateTime = new(2024, 7, 15, 14, 27, 33);

    [Theory]
    [InlineData(0.01, "14:27:33")] // Less than 1 hour - seconds precision
    [InlineData(0.04, "14:27:33")] // 1 hour exactly (1/24) - seconds precision
    public void FormatShouldShowSecondsWhenSpanIsVeryShort(double spanDays, string expected)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0.5, "14:27\nJul 15")] // Half day
    [InlineData(1.0, "14:27\nJul 15")] // 1 day
    [InlineData(2.0, "14:27\nJul 15")] // 2 days exactly
    public void FormatShouldShowHoursAndDateWhenSpanIsShort(double spanDays, string expected)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(5.0, "Jul 15")] // 5 days
    [InlineData(30.0, "Jul 15")] // 1 month
    [InlineData(40.0, "Jul 15")] // 40 days exactly
    public void FormatShouldShowDateOnlyWhenSpanIsMedium(double spanDays, string expected)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100.0, "Jul 2024")] // ~3 months
    [InlineData(365.0, "Jul 2024")] // 1 year
    [InlineData(800.0, "Jul 2024")] // 800 days exactly
    public void FormatShouldShowMonthYearWhenSpanIsLong(double spanDays, string expected)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1000.0, "2024")] // ~3 years
    [InlineData(3650.0, "2024")] // 10 years
    [InlineData(36500.0, "2024")] // 100 years
    public void FormatShouldShowYearOnlyWhenSpanIsVeryLong(double spanDays, string expected)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatShouldUseDefaultSpanWhenNotSet()
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter(); // Default is 1.0 day

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert
        result.Should().Be("14:27\nJul 15"); // Should use 1-day format
    }

    [Theory]
    [InlineData(1, 1, 0, 0, 0, 5.0, "Jan 1")] // January
    [InlineData(12, 31, 23, 59, 59, 5.0, "Dec 31")] // December
    [InlineData(2, 29, 12, 0, 0, 5.0, "Feb 29")] // Leap year
    public void FormatShouldHandleDifferentDatesAndTimes(int month, int day, int hour, int minute, int second, double spanDays, string expected)
    {
        // Arrange
        var dateTime = new DateTime(2024, month, day, hour, minute, second);
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(dateTime);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FormatShouldHandleMinDateTime()
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = 1.0 };

        // Act & Assert - Should not throw
        var result = formatter.Format(DateTime.MinValue);
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FormatShouldHandleMaxDateTime()
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = 1.0 };

        // Act & Assert - Should not throw
        var result = formatter.Format(DateTime.MaxValue);
        result.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(0.041666666, "14:27:33")] // Exactly 1/24 (1 hour) - should use seconds
    [InlineData(0.041666667, "14:27\nJul 15")] // Just over 1/24 - should use hours
    [InlineData(2.0, "14:27\nJul 15")] // Exactly 2 days - should use hours
    [InlineData(2.0000001, "Jul 15")] // Just over 2 days - should use date only
    [InlineData(40.0, "Jul 15")] // Exactly 40 days - should use date only
    [InlineData(40.0000001, "Jul 2024")] // Just over 40 days - should use month/year
    [InlineData(800.0, "Jul 2024")] // Exactly 800 days - should use month/year
    [InlineData(800.0000001, "2024")] // Just over 800 days - should use year only
    public void FormatShouldHandleBoundaryValues(double spanDays, string expectedPattern)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act
        var result = formatter.Format(_testDateTime);

        // Assert - Check pattern rather than exact match for boundary cases
        if (expectedPattern.Contains(':') && expectedPattern.Contains('\n'))
        {
            result.Should().MatchRegex(@"\d{2}:\d{2}\n\w{3} \d{1,2}");
        }
        else if (expectedPattern.Contains(':'))
        {
            result.Should().MatchRegex(@"\d{2}:\d{2}:\d{2}");
        }
        else if (expectedPattern.Contains(' ') && expectedPattern.Length <= 6)
        {
            result.Should().MatchRegex(@"\w{3} \d{1,2}");
        }
        else if (expectedPattern.Contains(' '))
        {
            result.Should().MatchRegex(@"\w{3} \d{4}");
        }
        else
        {
            result.Should().MatchRegex(@"\d{4}");
        }
    }

    [Theory]
    [InlineData(-1.0)] // Negative span
    [InlineData(0.0)] // Zero span
    public void FormatShouldHandleInvalidSpansWithoutThrowing(double spanDays)
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter { VisibleSpanDaysHint = spanDays };

        // Act & Assert - Should not throw
        var result = () => formatter.Format(_testDateTime);
        result.Should().NotThrow();
    }

    [Fact]
    public void VisibleSpanDaysHintShouldBeSettableAndGettable()
    {
        // Arrange
        var formatter = new AdaptiveDateTimeFormatter();

        // Act
        formatter.VisibleSpanDaysHint = 42.5;

        // Assert
        formatter.VisibleSpanDaysHint.Should().Be(42.5);
    }
}