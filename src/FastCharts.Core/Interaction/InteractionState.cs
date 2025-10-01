using ReactiveUI;

namespace FastCharts.Core.Interaction;

public sealed class InteractionState : ReactiveObject
{
    private bool _showCrosshair;
    private double _pixelX;
    private double _pixelY;
    private double? _dataX;
    private double? _dataY;
    private string? _tooltipText;
    private bool _tooltipLocked;
    private double? _tooltipAnchorX;
    private bool _showSelectionRect;
    private double _selX1;
    private double _selY1;
    private double _selX2;
    private double _selY2;
    private bool _showNearest;
    private double _nearestDataX;
    private double _nearestDataY;
    private bool _isPanning;

    public bool ShowCrosshair
    {
        get => _showCrosshair;
        set => this.RaiseAndSetIfChanged(ref _showCrosshair, value);
    }

    public double PixelX
    {
        get => _pixelX;
        set => this.RaiseAndSetIfChanged(ref _pixelX, value);
    }

    public double PixelY
    {
        get => _pixelY;
        set => this.RaiseAndSetIfChanged(ref _pixelY, value);
    }

    public double? DataX
    {
        get => _dataX;
        set => this.RaiseAndSetIfChanged(ref _dataX, value);
    }

    public double? DataY
    {
        get => _dataY;
        set => this.RaiseAndSetIfChanged(ref _dataY, value);
    }

    public string? TooltipText
    {
        get => _tooltipText;
        set => this.RaiseAndSetIfChanged(ref _tooltipText, value);
    }

    public System.Collections.Generic.List<TooltipSeriesValue> TooltipSeries { get; } = new();

    public bool TooltipLocked
    {
        get => _tooltipLocked;
        set => this.RaiseAndSetIfChanged(ref _tooltipLocked, value);
    }

    public double? TooltipAnchorX
    {
        get => _tooltipAnchorX;
        set => this.RaiseAndSetIfChanged(ref _tooltipAnchorX, value);
    }

    public bool ShowSelectionRect
    {
        get => _showSelectionRect;
        set => this.RaiseAndSetIfChanged(ref _showSelectionRect, value);
    }

    public double SelX1
    {
        get => _selX1;
        set => this.RaiseAndSetIfChanged(ref _selX1, value);
    }

    public double SelY1
    {
        get => _selY1;
        set => this.RaiseAndSetIfChanged(ref _selY1, value);
    }

    public double SelX2
    {
        get => _selX2;
        set => this.RaiseAndSetIfChanged(ref _selX2, value);
    }

    public double SelY2
    {
        get => _selY2;
        set => this.RaiseAndSetIfChanged(ref _selY2, value);
    }

    public bool ShowNearest
    {
        get => _showNearest;
        set => this.RaiseAndSetIfChanged(ref _showNearest, value);
    }

    public double NearestDataX
    {
        get => _nearestDataX;
        set => this.RaiseAndSetIfChanged(ref _nearestDataX, value);
    }

    public double NearestDataY
    {
        get => _nearestDataY;
        set => this.RaiseAndSetIfChanged(ref _nearestDataY, value);
    }

    public System.Collections.Generic.List<LegendHit> LegendHits { get; set; } = new();

    public bool IsPanning
    {
        get => _isPanning;
        set => this.RaiseAndSetIfChanged(ref _isPanning, value);
    }
}