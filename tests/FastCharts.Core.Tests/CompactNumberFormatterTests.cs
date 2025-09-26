using FastCharts.Core.Formatting;
using Xunit;

namespace FastCharts.Core.Tests
{
    public class CompactNumberFormatterTests
    {
        [Theory]
        [InlineData(0, "0")]
        [InlineData(12, "12")]
        [InlineData(999, "999")]
        [InlineData(1200, "1.2k")]
        [InlineData(15000, "15k")]
        [InlineData(1_200_000, "1.2M")]
        [InlineData(2_000_000_000, "2B")]
        [InlineData(3_400_000_000_000, "3.4T")]
        [InlineData(-1250, "-1.3k")]
        public void FormatsExpected(double v, string expectedPrefix)
        {
            var f = new CompactNumberFormatter(digits: 1);
            var s = f.Format(v);
            Assert.StartsWith(expectedPrefix, s);
        }
    }
}
