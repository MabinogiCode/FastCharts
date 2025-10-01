using ReactiveUI;

namespace FastCharts.Core.Interaction;

public sealed class TooltipSeriesValue : ReactiveObject
{
    private string? _title;
    private double _x;
    private double _y;
    private int? _paletteIndex;

    public string? Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

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

    public int? PaletteIndex
    {
        get => _paletteIndex;
        set => this.RaiseAndSetIfChanged(ref _paletteIndex, value);
    }
}