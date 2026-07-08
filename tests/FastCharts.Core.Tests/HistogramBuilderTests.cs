using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Series;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class HistogramBuilderTests
{
    [Fact]
    public void BuildBinsValuesIntoRequestedBinCount()
    {
        // 0..9 (10 values) into 5 bins: every value is counted, exactly 5 bars produced.
        var values = Enumerable.Range(0, 10).Select(i => (double)i);

        var series = HistogramBuilder.Build(values, binCount: 5);

        series.Data.Should().HaveCount(5);
        series.Data.Sum(b => b.Y).Should().Be(10);
        series.Width.Should().BeApproximately(9.0 / 5.0, 1e-12); // range/bins
    }

    [Fact]
    public void BuildBarWidthEqualsBinWidthAndCountsAreExact()
    {
        var values = new double[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var series = HistogramBuilder.Build(values, binCount: 5);

        var binWidth = (10.0 - 0.0) / 5.0; // 2.0
        series.Width.Should().Be(binWidth);
        series.Data.Sum(b => b.Y).Should().Be(values.Length); // every sample counted once
        // First bar centered on first bin.
        series.Data[0].X.Should().Be(0 + (0.5 * binWidth));
    }

    [Fact]
    public void BuildMaxValueLandsInLastBin()
    {
        var values = new double[] { 0, 10 };

        var series = HistogramBuilder.Build(values, binCount: 2);

        series.Data.Should().HaveCount(2);
        series.Data[0].Y.Should().Be(1); // the 0
        series.Data[1].Y.Should().Be(1); // the 10 clamps into last bin, not a 3rd bin
    }

    [Fact]
    public void BuildAllIdenticalValuesProducesSingleBar()
    {
        var values = Enumerable.Repeat(7.0, 25);

        var series = HistogramBuilder.Build(values);

        series.Data.Should().HaveCount(1);
        series.Data[0].X.Should().Be(7.0);
        series.Data[0].Y.Should().Be(25);
        series.Width.Should().Be(1.0);
    }

    [Fact]
    public void BuildEmptyInputProducesEmptySeries()
    {
        var series = HistogramBuilder.Build(Array.Empty<double>());

        series.IsEmpty.Should().BeTrue();
        series.Data.Should().BeEmpty();
    }

    [Fact]
    public void BuildIgnoresNaNAndInfinity()
    {
        var values = new double[] { 1, 2, double.NaN, 3, double.PositiveInfinity, 4, double.NegativeInfinity };

        var series = HistogramBuilder.Build(values, binCount: 3);

        // Only 1,2,3,4 are finite => 4 samples counted.
        series.Data.Sum(b => b.Y).Should().Be(4);
    }

    [Fact]
    public void BuildAutoBinCountFollowsSturgesRule()
    {
        // Sturges: ceil(log2(n) + 1). n=100 => ceil(6.64+1)=8.
        var values = Enumerable.Range(0, 100).Select(i => (double)i);

        var series = HistogramBuilder.Build(values);

        series.Data.Should().HaveCount(8);
    }

    [Fact]
    public void BuildNullThrows()
    {
        Action act = () => HistogramBuilder.Build(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddHistogramAddsSeriesAndReturnsIt()
    {
        var model = new ChartModel();
        var values = Enumerable.Range(0, 50).Select(i => (double)i);

        var series = model.AddHistogram(values, binCount: 10, title: "Dist");

        series.Should().NotBeNull();
        series.Title.Should().Be("Dist");
        model.Series.Should().Contain(series);
        series.Data.Should().HaveCount(10);
    }
}
