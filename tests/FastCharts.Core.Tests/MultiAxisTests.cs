using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

using Xunit;

namespace FastCharts.Core.Tests
{
    public class MultiAxisTests
    {
        [Fact]
        public void AddingSecondarySeriesCreatesSecondaryAxisWithIndependentRange()
        {
            var m = new ChartModel();
            // Primary Y series (range roughly 0..10)
            var primary = new LineSeries(new[]
            {
                new PointD(0, 0),
                new PointD(5, 10)
            })
            { Title = "Primary" };
            m.AddSeries(primary);
            // Secondary Y series (range 100..200)
            var secondary = new LineSeries(new[]
            {
                new PointD(0, 100),
                new PointD(5, 200)
            })
            { Title = "Secondary", YAxisIndex = 1 };
            m.AddSeries(secondary);

            // AutoFit is invoked by AddSeries, but call again explicitly for clarity
            m.AutoFitDataRange();
            Assert.NotNull(m.YAxisSecondary);

            var y1 = m.YAxis.DataRange;
            var y2 = m.YAxisSecondary!.DataRange;

            Assert.InRange(y1.Min, -0.01, 0.01);
            Assert.InRange(y1.Max, 9.99, 10.01);
            Assert.InRange(y2.Min, 99.5, 100.5);
            Assert.InRange(y2.Max, 199.5, 200.5);
        }

        [Fact]
        public void SecondaryAxisKeepsItsOwnVisibleRangeAfterAutoFitAndUpdateScales()
        {
            var m = new ChartModel();
            m.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));
            m.AddSeries(new LineSeries(new[] { new PointD(0, 100), new PointD(10, 200) }) { YAxisIndex = 1 });

            m.UpdateScales(400, 300);

            Assert.Equal(0, m.YAxis.VisibleRange.Min, 6);
            Assert.Equal(10, m.YAxis.VisibleRange.Max, 6);
            Assert.Equal(100, m.YAxisSecondary!.VisibleRange.Min, 6);
            Assert.Equal(200, m.YAxisSecondary.VisibleRange.Max, 6);
        }

        [Fact]
        public void SecondaryAxisFollowsPrimaryZoomProportionally()
        {
            var m = new ChartModel();
            m.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));
            m.AddSeries(new LineSeries(new[] { new PointD(0, 100), new PointD(10, 200) }) { YAxisIndex = 1 });

            // Show the upper half of the primary range: the secondary shows its upper half too
            m.Viewport.SetVisible(m.XAxis.VisibleRange, new FRange(5, 10));

            Assert.Equal(150, m.YAxisSecondary!.VisibleRange.Min, 6);
            Assert.Equal(200, m.YAxisSecondary.VisibleRange.Max, 6);
        }

        [Fact]
        public void SecondaryAxisWithoutSeriesMirrorsPrimary()
        {
            var m = new ChartModel();
            m.AddSeries(new LineSeries(new[] { new PointD(0, 0), new PointD(10, 10) }));

            m.EnsureSecondaryYAxis();

            Assert.Equal(m.YAxis.VisibleRange.Min, m.YAxisSecondary!.VisibleRange.Min, 6);
            Assert.Equal(m.YAxis.VisibleRange.Max, m.YAxisSecondary.VisibleRange.Max, 6);
        }
    }
}
