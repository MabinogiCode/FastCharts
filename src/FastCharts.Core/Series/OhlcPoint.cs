namespace FastCharts.Core.Series;

public struct OhlcPoint
{
    public double X { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }

    /// <summary>
    /// Optional traded volume for this period (rendered when OhlcSeries.ShowVolume is true).
    /// </summary>
    public double? Volume { get; set; }

    public OhlcPoint(double x, double open, double high, double low, double close)
    {
        X = x;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = null;
    }

    public OhlcPoint(double x, double open, double high, double low, double close, double volume)
    {
        X = x;
        Open = open;
        High = high;
        Low = low;
        Close = close;
        Volume = volume;
    }
}
