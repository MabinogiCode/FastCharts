using System;
using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Performance-contract tests for LineSeries.GetRenderData:
    /// hot paths must not copy data, and the resample cache must stay coherent.
    /// </summary>
    public class LineSeriesRenderDataTests
    {
        [Fact]
        public void GetRenderData_SmallDataset_ReturnsBackingListWithoutCopy()
        {
            var series = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) });

            var first = series.GetRenderData(800);
            var second = series.GetRenderData(800);

            ReferenceEquals(first, second).Should().BeTrue("small datasets must not be copied on every frame");
            ReferenceEquals(first, series.Data).Should().BeTrue();
        }

        [Fact]
        public void GetRenderData_ResamplingDisabled_ReturnsBackingList()
        {
            var series = new LineSeries(Enumerable.Range(0, 5000).Select(i => new PointD(i, i)))
            {
                EnableAutoResampling = false
            };

            var renderData = series.GetRenderData(800);

            ReferenceEquals(renderData, series.Data).Should().BeTrue();
        }

        [Fact]
        public void GetRenderData_AboveThreshold_UsesCachedResample()
        {
            var series = new LineSeries(Enumerable.Range(0, 10000).Select(i => new PointD(i, Math.Sin(i * 0.01))));

            var first = series.GetRenderData(500);
            var second = series.GetRenderData(500);

            first.Count.Should().BeLessThan(10000);
            ReferenceEquals(first, second).Should().BeTrue("same viewport and unchanged data must hit the cache");
        }

        [Fact]
        public void GetRenderData_CacheInvalidatedWhenPointAdded()
        {
            var series = new LineSeries(Enumerable.Range(0, 10000).Select(i => new PointD(i, i)));

            var first = series.GetRenderData(500);
            series.AddPoint(new PointD(10000, 10000));
            var second = series.GetRenderData(500);

            ReferenceEquals(first, second).Should().BeFalse("adding a point must invalidate the resample cache");
            second[second.Count - 1].X.Should().Be(10000);
        }

        [Fact]
        public void GetRenderData_CacheInvalidatedWhenViewportChanges()
        {
            var series = new LineSeries(Enumerable.Range(0, 10000).Select(i => new PointD(i, i)));

            var narrow = series.GetRenderData(200);
            var wide = series.GetRenderData(2000);

            wide.Count.Should().BeGreaterThan(narrow.Count);
        }

        [Fact]
        public void ReplacePoints_SwapsContentAndInvalidatesCache()
        {
            var series = new LineSeries(new[] { new PointD(0, 0) });

            series.ReplacePoints(new[] { new PointD(5, 5), new PointD(6, 6) });

            series.Data.Should().HaveCount(2);
            series.Data[0].X.Should().Be(5);
        }
    }
}
