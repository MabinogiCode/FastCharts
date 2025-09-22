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
        public void Produces_Reasonable_Tick_Count(double min, double max)
        {
            var t = new NiceTicker();
            var r = new FRange(min, max);
            var ticks = t.GetTicks(r, (max - min) / 7.0);
            Assert.InRange(ticks.Count, 3, 12);
        }

        [Fact]
        public void Honors_Range_And_Order()
        {
            var t = new NiceTicker();
            var r = new FRange(0, 10);
            var ticks = t.GetTicks(r, 1.6);
            Assert.True(ticks.First() <= r.Min + 1e-9);
            Assert.True(ticks.Last()  >= r.Max - 1e-9);
            Assert.True(ticks.SequenceEqual(ticks.OrderBy(x => x)));
        }
    }
}
