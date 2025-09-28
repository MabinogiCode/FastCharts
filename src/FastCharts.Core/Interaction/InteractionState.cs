namespace FastCharts.Core.Interaction;

public sealed class InteractionState
{
    public bool ShowCrosshair { get; set; }
    public double PixelX { get; set; }
    public double PixelY { get; set; }
    public double? DataX { get; set; }
    public double? DataY { get; set; }
    public string? TooltipText { get; set; }
    public System.Collections.Generic.List<TooltipSeriesValue> TooltipSeries { get; } = new();
    public bool TooltipLocked { get; set; }
    public double? TooltipAnchorX { get; set; }
    public bool ShowSelectionRect { get; set; }
    public double SelX1 { get; set; }
    public double SelY1 { get; set; }
    public double SelX2 { get; set; }
    public double SelY2 { get; set; }
    public bool ShowNearest { get; set; }
    public double NearestDataX { get; set; }
    public double NearestDataY { get; set; }
    public System.Collections.Generic.List<LegendHit> LegendHits { get; set; } = new();
    public bool IsPanning { get; set; }
}

public sealed class LegendHit
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public object? SeriesReference { get; set; }
}

public sealed class TooltipSeriesValue
{
    public string? Title { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public int? PaletteIndex { get; set; }
}
