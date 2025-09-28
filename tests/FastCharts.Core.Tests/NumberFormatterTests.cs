using FastCharts.Core.Formatting;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

public class NumberFormatterTests
{
    [Theory]
    [InlineData(0, "0")]
    [InlineData(12, "12")]
    [InlineData(1234, "1.2k")]
    [InlineData(1_234_567, "1.2M")]
    [InlineData(1_234_567_890, "1.2B")]
    public void CompactShouldProduceExpectedSuffixes(double value, string expectedPrefix)
    {
        var f = new CompactNumberFormatter(digits: 1);
        f.Format(value).Should().StartWith(expectedPrefix);
    }

    [Theory]
    [InlineData(1.23e6, "1.23E+0")]
    [InlineData(5.5e-5, "5.50E-")]
    public void ScientificShouldUseScientificForExtremeValues(double value, string expectedStart)
    {
        var f = new ScientificNumberFormatter(significantDigits: 3);
        var s = f.Format(value);
        s.Should().StartWith(expectedStart);
    }

    [Fact]
    public void SuffixFormatterShouldScaleDownNumbers()
    {
        var f = new SuffixNumberFormatter(maxDecimals: 2);
        f.Format(1532).Should().StartWith("1.53k");
        f.Format(0.0000021).Should().StartWith("2.1μ");
    }
}
