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
        public string TooltipText { get; set; }
    }
}
