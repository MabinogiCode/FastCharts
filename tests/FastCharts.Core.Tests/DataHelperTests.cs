using System.Collections.Generic;
using FastCharts.Core.Primitives;
using FastCharts.Core.Utilities;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

public class DataHelperTests
{
    [Fact]
    public void GetMinMax_WithSingleSelector_EmptyList_ShouldReturnZeros()
    {
        var data = new List<PointD>();

        var (min, max) = DataHelper.GetMinMax(data, p => p.X);

        min.Should().Be(0);
        max.Should().Be(0);
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_NullList_ShouldReturnZeros()
    {
        List<PointD>? data = null;

        var (min, max) = DataHelper.GetMinMax(data!, p => p.X);

        min.Should().Be(0);
        max.Should().Be(0);
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_SingleElement_ShouldReturnSameValue()
    {
        var data = new List<PointD> { new PointD(5.0, 10.0) };

        var (min, max) = DataHelper.GetMinMax(data, p => p.X);

        min.Should().Be(5.0);
        max.Should().Be(5.0);
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_MultipleElements_ShouldReturnCorrectMinMax()
    {
        var data = new List<PointD>
        {
            new PointD(3.0, 10.0),
            new PointD(1.0, 20.0),
            new PointD(5.0, 15.0),
            new PointD(2.0, 30.0)
        };

        var (minX, maxX) = DataHelper.GetMinMax(data, p => p.X);
        var (minY, maxY) = DataHelper.GetMinMax(data, p => p.Y);

        minX.Should().Be(1.0);
        maxX.Should().Be(5.0);
        minY.Should().Be(10.0);
        maxY.Should().Be(30.0);
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_NegativeValues_ShouldHandleCorrectly()
    {
        var data = new List<PointD>
        {
            new PointD(-5.0, -10.0),
            new PointD(-1.0, -20.0),
            new PointD(-3.0, -15.0)
        };

        var (minX, maxX) = DataHelper.GetMinMax(data, p => p.X);
        var (minY, maxY) = DataHelper.GetMinMax(data, p => p.Y);

        minX.Should().Be(-5.0);
        maxX.Should().Be(-1.0);
        minY.Should().Be(-20.0);
        maxY.Should().Be(-10.0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_EmptyList_ShouldReturnZeros()
    {
        var data = new List<TestPoint>();

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(0);
        max.Should().Be(0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_NullList_ShouldReturnZeros()
    {
        List<TestPoint>? data = null;

        var (min, max) = DataHelper.GetMinMax(data!, p => p.Low, p => p.High);

        min.Should().Be(0);
        max.Should().Be(0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_SingleElement_ShouldReturnLowAndHigh()
    {
        var data = new List<TestPoint> { new TestPoint(5.0, 15.0) };

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(5.0);
        max.Should().Be(15.0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_MultipleElements_ShouldReturnCorrectMinMax()
    {
        var data = new List<TestPoint>
        {
            new TestPoint(10.0, 20.0),
            new TestPoint(5.0, 25.0),
            new TestPoint(8.0, 18.0),
            new TestPoint(12.0, 22.0)
        };

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(5.0);
        max.Should().Be(25.0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_NegativeValues_ShouldHandleCorrectly()
    {
        var data = new List<TestPoint>
        {
            new TestPoint(-20.0, -5.0),
            new TestPoint(-25.0, -10.0),
            new TestPoint(-15.0, -8.0)
        };

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(-25.0);
        max.Should().Be(-5.0);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_MixedValues_ShouldFindGlobalMinMax()
    {
        var data = new List<TestPoint>
        {
            new TestPoint(-10.0, 15.0),
            new TestPoint(5.0, 20.0),
            new TestPoint(-5.0, 10.0)
        };

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(-10.0);
        max.Should().Be(20.0);
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_LargeDataset_ShouldBeEfficient()
    {
        // Create a large dataset to verify performance
        var data = new List<PointD>();
        for (var i = 0; i < 10000; i++)
        {
            data.Add(new PointD(i, i * 2.0));
        }

        var (minX, maxX) = DataHelper.GetMinMax(data, p => p.X);

        minX.Should().Be(0);
        maxX.Should().Be(9999);
    }

    [Fact]
    public void GetMinMax_WithDualSelectors_OverlappingRanges_ShouldFindTrueMinMax()
    {
        // Test where some low values are higher than some high values
        var data = new List<TestPoint>
        {
            new TestPoint(20.0, 30.0), // Range: 20-30
            new TestPoint(10.0, 15.0), // Range: 10-15
            new TestPoint(25.0, 35.0)  // Range: 25-35
        };

        var (min, max) = DataHelper.GetMinMax(data, p => p.Low, p => p.High);

        min.Should().Be(10.0); // Minimum of all low values
        max.Should().Be(35.0); // Maximum of all high values
    }

    [Fact]
    public void GetMinMax_WithSingleSelector_InfinityValues_ShouldHandleCorrectly()
    {
        var data = new List<PointD>
        {
            new PointD(double.NegativeInfinity, 10.0),
            new PointD(5.0, double.PositiveInfinity),
            new PointD(3.0, 15.0)
        };

        var (minX, maxX) = DataHelper.GetMinMax(data, p => p.X);
        var (minY, maxY) = DataHelper.GetMinMax(data, p => p.Y);

        minX.Should().Be(double.NegativeInfinity);
        maxX.Should().Be(5.0);
        minY.Should().Be(10.0);
        maxY.Should().Be(double.PositiveInfinity);
    }

    // Helper class for testing dual-selector overload
    private class TestPoint
    {
        public double Low { get; }
        public double High { get; }

        public TestPoint(double low, double high)
        {
            Low = low;
            High = high;
        }
    }
}
