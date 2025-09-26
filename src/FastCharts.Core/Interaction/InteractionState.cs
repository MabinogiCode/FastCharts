namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// Shared, renderer-agnostic interaction state the renderer can read to draw overlays (crosshair, tooltip, selection, etc.).
    /// </summary>
    public sealed class InteractionState
    {
        public bool ShowCrosshair { get; set; }

        // Last mouse position in SURFACE pixels
        public double PixelX { get; set; }
        public double PixelY { get; set; }

        // Optional data-space coordinates (host may set them)
        public double? DataX { get; set; }
        public double? DataY { get; set; }

        // Optional text to show near the cursor
        public string? TooltipText { get; set; }

        // Multi-series aggregated tooltip (raw numeric X anchor and list of lines)
        public System.Collections.Generic.List<TooltipSeriesValue> TooltipSeries { get; } = new();
        public bool TooltipLocked { get; set; }
        public double? TooltipAnchorX { get; set; }

        // Selection rectangle (SURFACE pixels)
        public bool ShowSelectionRect { get; set; }
        public double SelX1 { get; set; }
        public double SelY1 { get; set; }
        public double SelX2 { get; set; }
        public double SelY2 { get; set; }

        // Nearest-point highlight (in DATA space)
        public bool ShowNearest { get; set; }
        public double NearestDataX { get; set; }
        public double NearestDataY { get; set; }

        // Legend hit test rectangles (SURFACE pixels)
        public System.Collections.Generic.List<LegendHit> LegendHits { get; set; } = new System.Collections.Generic.List<LegendHit>();

        // Pan state (for UI cursor feedback)
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
}
