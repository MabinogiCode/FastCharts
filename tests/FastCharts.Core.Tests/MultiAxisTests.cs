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
        public void SecondaryAxisVisibleRangeSyncsOnUpdateScales()
        {
            var m = new ChartModel();
            var secondary = new LineSeries(new[] { new PointD(0, 50), new PointD(10, 150) }) { YAxisIndex = 1 };
            m.AddSeries(secondary);
            m.AutoFitDataRange();
            Assert.NotNull(m.YAxisSecondary);
            // Modify primary viewport then update scales
            m.Viewport.SetVisible(m.XAxis.DataRange, new FRange(10, 20));
            m.UpdateScales(400, 300);
            Assert.Equal(new FRange(10, 20).Min, m.YAxis.VisibleRange.Min, 6);
            Assert.Equal(new FRange(10, 20).Max, m.YAxis.VisibleRange.Max, 6);
            // Secondary should mirror for now (shared viewport behavior)
            Assert.Equal(m.YAxis.VisibleRange.Min, m.YAxisSecondary!.VisibleRange.Min, 6);
            Assert.Equal(m.YAxis.VisibleRange.Max, m.YAxisSecondary!.VisibleRange.Max, 6);
        }
    }
}
