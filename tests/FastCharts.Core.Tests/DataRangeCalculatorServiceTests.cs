using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Services;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests
{
    /// <summary>
    /// Tests for data range calculator service
    /// </summary>
    public class DataRangeCalculatorServiceTests
    {
        private readonly DataRangeCalculatorService _service;

        public DataRangeCalculatorServiceTests()
        {
            _service = new DataRangeCalculatorService();
        }

        [Fact]
        public void CalculateDataRangesWithNullSeriesReturnsEmptyResult()
        {
            // Act
            var result = _service.CalculateDataRanges(null!);

            // Assert
            result.HasData.Should().BeFalse();
            result.HasX.Should().BeFalse();
            result.HasPrimary.Should().BeFalse();
            result.HasSecondary.Should().BeFalse();
        }

        [Fact]
        public void CalculateDataRangesWithEmptySeriesReturnsEmptyResult()
        {
            // Arrange
            var series = new List<SeriesBase>();

            // Act
            var result = _service.CalculateDataRanges(series);

            // Assert
            result.HasData.Should().BeFalse();
        }

        [Fact]
        public void CalculateDataRangesWithInvisibleSeriesReturnsEmptyResult()
        {
            // Arrange
            var series = new List<SeriesBase>
            {
                new LineSeries(new[] { new PointD(1, 1) }) { IsVisible = false }
            };

            // Act
            var result = _service.CalculateDataRanges(series);

            // Assert
            result.HasData.Should().BeFalse();
        }

        [Fact]
        public void CalculateDataRangesWithValidLineSeriesReturnsCorrectRanges()
        {
            // Arrange
            var series = new List<SeriesBase>
            {
                new LineSeries(new[]
                {
                    new PointD(0, 10),
                    new PointD(5, 20),
                    new PointD(10, 15)
                })
            };

            // Act
            var result = _service.CalculateDataRanges(series);

            // Assert
            result.HasData.Should().BeTrue();
            result.HasX.Should().BeTrue();
            result.HasPrimary.Should().BeTrue();
            result.XRange.Min.Should().Be(0);
            result.XRange.Max.Should().Be(10);
            result.PrimaryYRange.Min.Should().Be(10);
            result.PrimaryYRange.Max.Should().Be(20);
        }

        [Theory]
        [InlineData(5.0, 5.0, 4.5, 5.5)] // Same value must be expanded
        [InlineData(0.0, 10.0, 0.0, 10.0)] // Normal range
        public void CalculateDataRangesHandlesEqualMinMaxValues(double min, double max, double expectedMin, double expectedMax)
        {
            // Arrange
            var series = new List<SeriesBase>
            {
                new LineSeries(new[]
                {
                    new PointD(0, min),
                    new PointD(1, max)
                })
            };

            // Act
            var result = _service.CalculateDataRanges(series);

            // Assert
            result.HasData.Should().BeTrue();
            result.PrimaryYRange.Min.Should().Be(expectedMin);
            result.PrimaryYRange.Max.Should().Be(expectedMax);
        }

        [Fact]
        public void CalculateDataRangesWithMultipleSeriesAggregatesCorrectly()
        {
            // Arrange
            var series = new List<SeriesBase>
            {
                new LineSeries(new[]
                {
                    new PointD(0, 10),
                    new PointD(5, 20)
                }),
                new LineSeries(new[]
                {
                    new PointD(3, 5),
                    new PointD(8, 25)
                })
            };

            // Act
            var result = _service.CalculateDataRanges(series);

            // Assert
            result.HasData.Should().BeTrue();
            result.XRange.Min.Should().Be(0); // Min of all series
            result.XRange.Max.Should().Be(8); // Max of all series
            result.PrimaryYRange.Min.Should().Be(5); // Min of all series
            result.PrimaryYRange.Max.Should().Be(25); // Max of all series
        }
    }
}