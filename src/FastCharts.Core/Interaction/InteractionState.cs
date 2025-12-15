using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public InteractionState()
    {
        PinnedTooltips = new List<PinnedTooltip>();
    }

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

    /// <summary>
    /// Collection of pinned tooltips for P1-TOOLTIP-PIN feature
    /// Allows users to lock multiple tooltips simultaneously
    /// </summary>
    public List<PinnedTooltip> PinnedTooltips { get; }

    /// <summary>
    /// Event raised when a tooltip is pinned, unpinned, or modified
    /// </summary>
    public event EventHandler<PinnedTooltipEventArgs>? PinnedTooltipChanged;

    /// <summary>
    /// Pins the current tooltip at the current position
    /// </summary>
    /// <param name="label">Optional label for the pinned tooltip</param>
    /// <returns>The created pinned tooltip, or null if no tooltip to pin</returns>
    public PinnedTooltip? PinCurrentTooltip(string? label = null)
    {
        if (string.IsNullOrEmpty(TooltipText) || !DataX.HasValue || !DataY.HasValue)
            return null;

        var pinnedTooltip = new PinnedTooltip(
            new Primitives.PointD(DataX.Value, DataY.Value),
            new Primitives.PointD(PixelX, PixelY),
            TooltipText ?? string.Empty,
            TooltipSeries)
        {
            Label = label
        };

        PinnedTooltips.Add(pinnedTooltip);
        PinnedTooltipChanged?.Invoke(this, new PinnedTooltipEventArgs(pinnedTooltip, PinnedTooltipAction.Pinned));

        return pinnedTooltip;
    }

    /// <summary>
    /// Unpins a specific tooltip by ID
    /// </summary>
    /// <param name="tooltipId">ID of the tooltip to unpin</param>
    /// <returns>True if tooltip was found and removed</returns>
    public bool UnpinTooltip(Guid tooltipId)
    {
        var tooltip = PinnedTooltips.Find(t => t.Id == tooltipId);
        if (tooltip == null)
            return false;

        PinnedTooltips.Remove(tooltip);
        PinnedTooltipChanged?.Invoke(this, new PinnedTooltipEventArgs(tooltip, PinnedTooltipAction.Unpinned));
        return true;
    }

    /// <summary>
    /// Toggles visibility of a pinned tooltip
    /// </summary>
    /// <param name="tooltipId">ID of the tooltip to toggle</param>
    /// <returns>New visibility state, or null if tooltip not found</returns>
    public bool? ToggleTooltipVisibility(Guid tooltipId)
    {
        var tooltip = PinnedTooltips.Find(t => t.Id == tooltipId);
        if (tooltip == null)
            return null;

        tooltip.IsVisible = !tooltip.IsVisible;
        PinnedTooltipChanged?.Invoke(this, new PinnedTooltipEventArgs(tooltip, PinnedTooltipAction.VisibilityToggled));
        return tooltip.IsVisible;
    }

    /// <summary>
    /// Clears all pinned tooltips
    /// </summary>
    public void ClearAllPinnedTooltips()
    {
        if (PinnedTooltips.Count == 0)
            return;

        PinnedTooltips.Clear();
        // Use a dummy tooltip for the event since we don't have a specific one
        var dummyTooltip = new PinnedTooltip(
            new Primitives.PointD(0, 0),
            new Primitives.PointD(0, 0),
            string.Empty,
            new List<TooltipSeriesValue>());
        PinnedTooltipChanged?.Invoke(this, new PinnedTooltipEventArgs(dummyTooltip, PinnedTooltipAction.AllCleared));
    }

    /// <summary>
    /// Gets all visible pinned tooltips
    /// </summary>
    public IEnumerable<PinnedTooltip> GetVisiblePinnedTooltips()
    {
        return PinnedTooltips.Where(t => t.IsVisible);
    }

    /// <summary>
    /// Finds the pinned tooltip nearest to a pixel position (for interaction)
    /// </summary>
    /// <param name="pixelX">Pixel X coordinate</param>
    /// <param name="pixelY">Pixel Y coordinate</param>
    /// <param name="maxDistance">Maximum distance to consider (default: 20 pixels)</param>
    /// <returns>Nearest tooltip, or null if none within range</returns>
    public PinnedTooltip? FindNearestPinnedTooltip(double pixelX, double pixelY, double maxDistance = 20.0)
    {
        PinnedTooltip? nearest = null;
        var nearestDistance = double.MaxValue;

        foreach (var tooltip in GetVisiblePinnedTooltips())
        {
            var dx = tooltip.PixelPosition.X - pixelX;
            var dy = tooltip.PixelPosition.Y - pixelY;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= maxDistance && distance < nearestDistance)
            {
                nearest = tooltip;
                nearestDistance = distance;
            }
        }

        return nearest;
    }
}