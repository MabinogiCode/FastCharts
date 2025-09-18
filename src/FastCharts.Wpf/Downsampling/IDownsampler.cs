
using System.Collections.Generic;
namespace FastCharts.Wpf.Downsampling
{
    public interface IDownsampler
    {
        IReadOnlyList<int> Downsample(double[] X, double[] Y, int pixelWidth, int maxPointsPerPixel = 2);
    }
}
