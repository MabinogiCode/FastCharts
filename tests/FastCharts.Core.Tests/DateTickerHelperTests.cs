using System;
using FastCharts.Core.Axes.Ticks;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class DateTickerHelperTests
{
    [Theory]
    [InlineData(0.01)] // Less than 1 hour / 24 -> Minute, 5
    [InlineData(1.0)] // 1 day -> Hour, 1
    [InlineData(5.0)] // 5 days -> Hour, 6
    [InlineData(30.0)] // 30 days -> Day, 1
    [InlineData(150.0)] // 150 days (weekly) -> Day, 7
    [InlineData(500.0)] // 500 days (monthly) -> Month, 1
    [InlineData(2000.0)] // 2000 days (quarterly) -> Month, 3
    [InlineData(5000.0)] // 5000 days (yearly) -> Year, 1
    public void ChooseUnit_ShouldSelectAppropriateUnitAndStep(double days)
    {
        // Act
        DateTickerHelper.ChooseUnit(days, out var unit, out var step);

        // Assert based on expected behavior
        switch (days)
        {
            case <= (1.0 / 24.0):
                unit.Should().Be(TimeUnit.Minute);
                step.Should().Be(5);
                break;
            case <= 2.0:
                unit.Should().Be(TimeUnit.Hour);
                step.Should().Be(1);
                break;
            case <= 10.0:
                unit.Should().Be(TimeUnit.Hour);
                step.Should().Be(6);
                break;
            case <= 40.0:
                unit.Should().Be(TimeUnit.Day);
                step.Should().Be(1);
                break;
            case <= 200.0:
                unit.Should().Be(TimeUnit.Day);
                step.Should().Be(7);
                break;
            case <= 800.0:
                unit.Should().Be(TimeUnit.Month);
                step.Should().Be(1);
                break;
            case <= 3650.0:
                unit.Should().Be(TimeUnit.Month);
                step.Should().Be(3);
                break;
            default:
                unit.Should().Be(TimeUnit.Year);
                step.Should().Be(1);
                break;
        }
    }

    [Fact]
    public void Align_ShouldAlignSecondCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 3, 15, 14, 27, 30);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Second, 30);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Align_ShouldAlignMinuteCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 3, 15, 14, 15, 0);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Minute, 15);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Align_ShouldAlignHourCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 3, 15, 12, 0, 0);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Hour, 6);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Align_ShouldAlignDayCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 3, 15, 0, 0, 0);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Day, 1);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Align_ShouldAlignMonthCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 3, 1, 0, 0, 0);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Month, 1);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Align_ShouldAlignYearCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 14, 27, 33);
        var expected = new DateTime(2024, 1, 1, 0, 0, 0);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Year, 1);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddSecondsCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2024, 3, 15, 12, 0, 30);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Second, 30);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddMinutesCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2024, 3, 15, 12, 15, 0);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Minute, 15);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddHoursCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2024, 3, 15, 18, 0, 0);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Hour, 6);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddDaysCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2024, 3, 22, 12, 0, 0);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Day, 7);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddMonthsCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2024, 6, 15, 12, 0, 0);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Month, 3);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Add_ShouldAddYearsCorrectly()
    {
        // Arrange
        var input = new DateTime(2024, 3, 15, 12, 0, 0);
        var expected = new DateTime(2026, 3, 15, 12, 0, 0);

        // Act
        var result = DateTickerHelper.Add(input, TimeUnit.Year, 2);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ClampOADate_ShouldReturnValueWhenInRange()
    {
        // Arrange
        var validOADate = new DateTime(2024, 1, 1).ToOADate();

        // Act
        var result = DateTickerHelper.ClampOADate(validOADate);

        // Assert
        result.Should().Be(validOADate);
    }

    [Fact]
    public void ClampOADate_ShouldClampToMinValueWhenTooLow()
    {
        // Arrange
        var tooLowOADate = DateTime.MinValue.ToOADate() - 1000;

        // Act
        var result = DateTickerHelper.ClampOADate(tooLowOADate);

        // Assert
        result.Should().Be(DateTime.MinValue.ToOADate());
    }

    [Fact]
    public void ClampOADate_ShouldClampToMaxValueWhenTooHigh()
    {
        // Arrange
        var tooHighOADate = DateTime.MaxValue.ToOADate() + 1000;

        // Act
        var result = DateTickerHelper.ClampOADate(tooHighOADate);

        // Assert
        result.Should().Be(DateTime.MaxValue.ToOADate());
    }

    [Theory]
    [InlineData("2024-02-15 14:27:33", 5)] // February with 29 days (leap year)
    [InlineData("2023-02-15 14:27:33", 5)] // February with 28 days (non-leap year)
    [InlineData("2024-01-31 14:27:33", 7)] // January with 31 days
    public void Align_WithDayUnit_ShouldRespectMonthBoundaries(string inputDate, int step)
    {
        // Arrange
        var input = DateTime.Parse(inputDate);

        // Act
        var result = DateTickerHelper.Align(input, TimeUnit.Day, step);

        // Assert
        result.Month.Should().Be(input.Month);
        result.Year.Should().Be(input.Year);
        result.Hour.Should().Be(0);
        result.Minute.Should().Be(0);
        result.Second.Should().Be(0);
        result.Day.Should().BeLessOrEqualTo(DateTime.DaysInMonth(input.Year, input.Month));
    }

    [Fact]
    public void Align_WithQuarterlyMonths_ShouldAlignCorrectly()
    {
        // Arrange
        var julyInput = new DateTime(2024, 7, 15, 14, 27, 33);
        var novemberInput = new DateTime(2024, 11, 15, 14, 27, 33);
        
        // Act
        var julyResult = DateTickerHelper.Align(julyInput, TimeUnit.Month, 3);
        var novemberResult = DateTickerHelper.Align(novemberInput, TimeUnit.Month, 3);

        // Assert
        julyResult.Should().Be(new DateTime(2024, 7, 1, 0, 0, 0)); // Q3
        novemberResult.Should().Be(new DateTime(2024, 10, 1, 0, 0, 0)); // Q4
    }

    [Fact]
    public void Align_WithYearStep_ShouldAlignCorrectly()
    {
        // Arrange
        var input2024 = new DateTime(2024, 3, 15, 14, 27, 33);
        var input2027 = new DateTime(2027, 3, 15, 14, 27, 33);

        // Act
        var result2024 = DateTickerHelper.Align(input2024, TimeUnit.Year, 5);
        var result2027 = DateTickerHelper.Align(input2027, TimeUnit.Year, 10);

        // Assert
        result2024.Should().Be(new DateTime(2020, 1, 1, 0, 0, 0)); // 2024 -> 2020 (5-year step)
        result2027.Should().Be(new DateTime(2020, 1, 1, 0, 0, 0)); // 2027 -> 2020 (10-year step)
    }
}