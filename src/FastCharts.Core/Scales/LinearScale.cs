using FastCharts.Core.Abstractions;

namespace FastCharts.Core.Scales;

public sealed class LinearScale : IScale<double>
{
    private readonly double _dataMin;
    private readonly double _dataMax;
    private readonly double _pixelMin;
    private readonly double _pixelMax;

    public LinearScale(double dataMin, double dataMax, double pixelMin, double pixelMax)
    {
        _dataMin = dataMin;
        _dataMax = dataMax;
        _pixelMin = pixelMin;
        _pixelMax = pixelMax;
    }

    public double ToPixels(double value)
    {
        var t = (value - _dataMin) / (_dataMax - _dataMin);
        return _pixelMin + t * (_pixelMax - _pixelMin);
    }

    public double FromPixels(double px)
    {
        var t = (px - _pixelMin) / (_pixelMax - _pixelMin);
        return _dataMin + t * (_dataMax - _dataMin);
    }
}
