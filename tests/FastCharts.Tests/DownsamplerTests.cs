using System.Linq;

using FastCharts.Wpf.Downsampling;

using Xunit;

public class DownsamplerTests
{
    [Fact]
    public void MinMaxPerPixel_Returns_AtMost2PerPixel()
    {
        int n = 10000;
        var x = Enumerable.Range(0, n).Select(i => (double)i).ToArray();
        var y = x.Select(i => (i % 50) - 25).ToArray();
        var ds = new MinMaxPerPixelDownsampler();
        var idx = ds.Downsample(x, y, pixelWidth: 1000, maxPointsPerPixel: 2);
        Assert.True(idx.Count <= 2002);
        Assert.True(idx[0] == 0 || idx[0] < n);
        Assert.True(idx[idx.Count-1] == n-1);
    }
}
