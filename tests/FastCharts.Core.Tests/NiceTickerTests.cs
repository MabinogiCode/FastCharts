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
            for (var i = 1; i < ticks.Count; i++)
            {
                Assert.True(ticks[i] >= ticks[i - 1]);
            }
        }

        [Theory]
        [InlineData(-0.3, 0.3, 0.1)]
        [InlineData(-0.7, 0.9, 0.1)]
        [InlineData(-1.0, 1.0, 0.2)]
        [InlineData(-2.5, 2.5, 0.5)]
        public void CrossingZeroYieldsAnExactZeroTick(double min, double max, double step)
        {
            // Floating-point error used to leave ticks such as -2.7e-17, labeled "-0"
            var ticks = new NiceTicker().GetTicks(new FRange(min, max), step);

            Assert.Contains(0.0, ticks);
            Assert.All(ticks, t => Assert.True(t.Equals(0.0) || System.Math.Abs(t) >= step / 2, $"tick {t:R} should be 0"));
            Assert.DoesNotContain(ticks, t => t.ToString(System.Globalization.CultureInfo.InvariantCulture) == "-0");
        }
    }
}
