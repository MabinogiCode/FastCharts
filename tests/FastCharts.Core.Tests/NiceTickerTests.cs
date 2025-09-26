using System.Linq;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests
{
    public class NiceTickerTests
    {
        [Theory]
        [InlineData(0, 100)]
        [InlineData(-50, 50)]
        [InlineData(1000, 10000)]
        public void ProducesReasonableTickCount(double min, double max)
        {
            var t = new NiceTicker();
            var r = new FRange(min, max);
            var ticks = t.GetTicks(r, (max - min) / 7.0);
            Assert.InRange(ticks.Count, 3, 12);
        }

        [Fact]
        public void HonorsRangeAndOrder()
        {
            var t = new NiceTicker();
            var r = new FRange(0, 10);
            var ticks = t.GetTicks(r, 1.6);
            Assert.True(ticks[0] <= r.Min + 1e-9);
            Assert.True(ticks[ticks.Count - 1] >= r.Max - 1e-9);
            for (int i = 1; i < ticks.Count; i++)
            {
                Assert.True(ticks[i] >= ticks[i - 1]);
            }
        }
    }
}
