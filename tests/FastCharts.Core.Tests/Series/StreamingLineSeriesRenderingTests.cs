using System;
using System.Linq;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Regression tests: streaming series data must be visible through the base
    /// LineSeries contract used by renderers and range calculation services.
    /// </summary>
    public class StreamingLineSeriesRenderingTests
    {
        [Fact]
        public void GetRenderData_ThroughBaseReference_ReturnsStreamedPoints()
        {
            // Renderers hold LineSeries references; streamed points must flow through
            LineSeries series = new StreamingLineSeries(maxPointCount: 100);
            ((StreamingLineSeries)series).AppendPoint(new PointD(1, 10));
            ((StreamingLineSeries)series).AppendPoint(new PointD(2, 20));

            var renderData = series.GetRenderData(800);

            renderData.Should().HaveCount(2);
            renderData[0].X.Should().Be(1);
            renderData[1].Y.Should().Be(20);
        }

        [Fact]
        public void Data_ThroughBaseReference_SeesStreamedPoints()
        {
            LineSeries series = new StreamingLineSeries();
            ((StreamingLineSeries)series).AppendPoints(new[] { new PointD(1, 1), new PointD(2, 2), new PointD(3, 3) });

            series.Data.Should().HaveCount(3);
            series.IsEmpty.Should().BeFalse();
        }

        [Fact]
        public void RangeProvider_ThroughInterface_UsesStreamedPoints()
        {
            var streaming = new StreamingLineSeries();
            streaming.AppendPoints(new[] { new PointD(5, -3), new PointD(15, 7) });

            var provider = (ISeriesRangeProvider)streaming;
            provider.TryGetRanges(out var xRange, out var yRange).Should().BeTrue();

            xRange.Min.Should().Be(5);
            xRange.Max.Should().Be(15);
            yRange.Min.Should().Be(-3);
            yRange.Max.Should().Be(7);
        }

        [Fact]
        public void MaxPointCount_TrimsOldestPoints()
        {
            var streaming = new StreamingLineSeries(maxPointCount: 5);
            streaming.AppendPoints(Enumerable.Range(0, 10).Select(i => new PointD(i, i)));

            streaming.PointCount.Should().Be(5);
            streaming.Data[0].X.Should().Be(5); // Oldest points trimmed FIFO
        }

        [Fact]
        public void PointsRemovedEvent_FiresOnTrim()
        {
            var streaming = new StreamingLineSeries(maxPointCount: 3);
            var removed = 0;
            streaming.PointsRemoved += (_, args) => removed += args.PointsRemoved;

            streaming.AppendPoints(Enumerable.Range(0, 8).Select(i => new PointD(i, i)));

            removed.Should().Be(5);
        }

        [Fact]
        public void NonTimeXValues_DoNotBreakTrimming()
        {
            // X values far outside the OADate range must not throw
            var streaming = new StreamingLineSeries(maxPointCount: 10, rollingWindow: TimeSpan.FromMinutes(5));

            var act = () => streaming.AppendPoint(new PointD(1e12, 42));

            act.Should().NotThrow();
            streaming.PointCount.Should().Be(1);
        }

        [Fact]
        public void GetRenderData_LargeStream_IsResampled()
        {
            var streaming = new StreamingLineSeries();
            streaming.AutoResampleThreshold = 500;
            streaming.AppendPoints(Enumerable.Range(0, 5000).Select(i => new PointD(i, Math.Sin(i * 0.01))));

            LineSeries asBase = streaming;
            var renderData = asBase.GetRenderData(400);

            renderData.Count.Should().BeLessThan(5000);
            renderData.Count.Should().BeGreaterThan(2);
            renderData[0].X.Should().Be(0); // LTTB keeps endpoints
            renderData[renderData.Count - 1].X.Should().Be(4999);
        }
    }
}
