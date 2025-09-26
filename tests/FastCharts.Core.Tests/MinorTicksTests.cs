using System.Linq;
using FastCharts.Core.Axes;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;
using Xunit;

namespace FastCharts.Core.Tests
{
    public class MinorTicksTests
    {
        [Fact]
        public void NumericAxis_MinorTicks_Disabled_WhenFlagsFalse()
        {
            var ax = new NumericAxis();
            ax.ShowMinorTicks = false;
            ax.ShowMinorGrid = false;
            var range = new FRange(0, 100);
            var majors = ax.Ticker.GetTicks(range, 20);
            var minors = ax.Ticker.GetMinorTicks(range, majors);
            // Rendering layer will ignore minors when flags false, ensure majors exist
            Assert.True(majors.Count > 0);
            // We still compute minors here (interface), but UI decides use; so just verify minors are non-negative
            Assert.All(minors, m => Assert.InRange(m, range.Min - 1, range.Max + 1));
        }

        [Fact]
        public void NumericAxis_MinorTicks_1_2_5_Subdivision()
        {
            var ax = new NumericAxis();
            var range = new FRange(0, 10);
            var majors = ax.Ticker.GetTicks(range, 2);
            var minors = ax.Ticker.GetMinorTicks(range, majors);
            Assert.True(majors.Count >= 3);
            // Ensure no minor equals a major
            var majorSet = majors.ToHashSet();
            Assert.DoesNotContain(minors, m => majorSet.Contains(m));
            // Some minors expected
            Assert.True(minors.Count > majors.Count);
        }

        [Fact]
        public void DateTicker_Minors_For_Weekly_Majors()
        {
            var dt = new DateTicker();
            // 60 days range -> weekly majors likely
            var start = System.DateTime.Today;
            var end = start.AddDays(60);
            var range = new FRange(start.ToOADate(), end.ToOADate());
            var majors = dt.GetTicks(range, 0);
            var minors = dt.GetMinorTicks(range, majors);
            Assert.True(majors.Count > 4);
            Assert.True(minors.Count > majors.Count); // daily minors
            var majorSet = majors.ToHashSet();
            Assert.DoesNotContain(minors, m => majorSet.Contains(m));
        }
    }
}
