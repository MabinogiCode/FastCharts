using System;
using System.Linq;
using Xunit;
using FastCharts.Core.Axes.Ticks;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Tests
{
    public class DateTickerTests
    {
        [Fact]
        public void TwoHours_Produces_Hourly_Ticks()
        {
            var t0 = DateTime.Today;
            var t1 = t0.AddHours(2);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, approxStep: 0);

            Assert.True(ticks.Count >= 2);
            // Differences around 1 hour
            var diffs = ticks.Zip(ticks.Skip(1), (a, b) => b - a).ToArray();
            double oneHour = TimeSpan.FromHours(1).TotalDays; // in OADate
            Assert.All(diffs, d => Assert.InRange(d, oneHour * 0.9, oneHour * 1.1));
        }

        [Fact]
        public void SevenDays_Produces_6h_Ticks()
        {
            var t0 = DateTime.Today;
            var t1 = t0.AddDays(7);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            var diffs = ticks.Zip(ticks.Skip(1), (a, b) => b - a).ToArray();
            double sixHours = TimeSpan.FromHours(6).TotalDays;
            Assert.All(diffs, d => Assert.InRange(d, sixHours * 0.9, sixHours * 1.1));
        }

        [Fact]
        public void ThirtyDays_Produces_Daily_Ticks()
        {
            var t0 = new DateTime(2024, 1, 1);
            var t1 = t0.AddDays(30);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            var diffs = ticks.Zip(ticks.Skip(1), (a, b) => b - a).ToArray();
            double oneDay = 1.0;
            Assert.All(diffs, d => Assert.InRange(d, oneDay * 0.95, oneDay * 1.05));
        }

        [Fact]
        public void OneHundredEightyDays_Produces_Weekly_Ticks()
        {
            var t0 = DateTime.Today;
            var t1 = t0.AddDays(180);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            var diffs = ticks.Zip(ticks.Skip(1), (a, b) => b - a).ToArray();
            double oneWeek = 7.0;
            Assert.All(diffs, d => Assert.InRange(d, oneWeek * 0.95, oneWeek * 1.05));
        }

        [Fact]
        public void ThreeYears_Produces_Quarterly_Ticks()
        {
            var t0 = new DateTime(2020, 1, 15);
            var t1 = t0.AddYears(3);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            // month steps of 3: ensure month increments by ~3 and day normalized near start
            var dt = ticks.Select(DateTime.FromOADate).ToArray();
            Assert.All(dt.Zip(dt.Skip(1), (a, b) => (a, b)), pair =>
            {
                int dm = (pair.b.Year - pair.a.Year) * 12 + (pair.b.Month - pair.a.Month);
                Assert.InRange(dm, 2, 4); // allow FP/edge alignment
            });
        }

        [Fact]
        public void FifteenYears_Produces_Yearly_Ticks()
        {
            var t0 = new DateTime(2000, 5, 1);
            var t1 = t0.AddYears(15);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            var dt = ticks.Select(DateTime.FromOADate).ToArray();
            Assert.All(dt.Zip(dt.Skip(1), (a, b) => (a, b)), pair =>
            {
                Assert.InRange(pair.b.Year - pair.a.Year, 1, 1);
            });
        }

        [Fact]
        public void Ticks_Are_Monotonic_And_In_Range()
        {
            var t0 = new DateTime(2022, 11, 3, 10, 23, 0);
            var t1 = t0.AddDays(12);
            var tr = new FRange(t0.ToOADate(), t1.ToOADate());
            var ticks = new DateTicker().GetTicks(tr, 0);
            Assert.True(ticks.Count > 0);
            for (int i = 1; i < ticks.Count; i++)
            {
                Assert.True(ticks[i] >= ticks[i - 1]);
            }
            Assert.True(ticks.First() >= tr.Min - 1e-6);
            Assert.True(ticks.Last() <= tr.Max + 1e-6);
        }
    }
}
