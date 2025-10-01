using ReactiveUI;

namespace FastCharts.Core.Interaction;

public sealed class LegendHit : ReactiveObject
{
    private double _x;
    private double _y;
    private double _width;
    private double _height;
    private object? _seriesReference;

    public double X
    {
        get => _x;
        set => this.RaiseAndSetIfChanged(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => this.RaiseAndSetIfChanged(ref _y, value);
    }

    public double Width
    {
        get => _width;
        set => this.RaiseAndSetIfChanged(ref _width, value);
    }

    public double Height
    {
        get => _height;
        set => this.RaiseAndSetIfChanged(ref _height, value);
    }

    public object? SeriesReference
    {
        get => _seriesReference;
        set => this.RaiseAndSetIfChanged(ref _seriesReference, value);
    }
}