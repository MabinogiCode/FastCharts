using FastCharts.Core.Primitives;
using System;
using System.Collections.Generic;

namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// Represents a pinned (locked) tooltip at a specific location
    /// Allows users to keep multiple tooltips visible simultaneously
    /// </summary>
    public sealed class PinnedTooltip
    {
        public PinnedTooltip(PointD dataPosition, PointD pixelPosition, string text, List<TooltipSeriesValue> seriesValues)
        {
            Id = Guid.NewGuid();
            DataPosition = dataPosition;
            PixelPosition = pixelPosition;
            Text = text ?? string.Empty;
            SeriesValues = new List<TooltipSeriesValue>(seriesValues);
            CreatedAt = DateTime.UtcNow;
            IsVisible = true;
        }

        /// <summary>
        /// Unique identifier for this pinned tooltip
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Position in data coordinates where tooltip is pinned
        /// </summary>
        public PointD DataPosition { get; }

        /// <summary>
        /// Position in pixel coordinates (may change on zoom/pan)
        /// </summary>
        public PointD PixelPosition { get; set; }

        /// <summary>
        /// Formatted text content of the tooltip
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Series values included in this tooltip
        /// </summary>
        public List<TooltipSeriesValue> SeriesValues { get; }

        /// <summary>
        /// When this tooltip was created
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Whether this pinned tooltip is currently visible
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Optional user-defined label for this tooltip
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Optional color for visual distinction
        /// </summary>
        public ColorRgba? Color { get; set; }

        /// <summary>
        /// Gets a short summary of this pinned tooltip
        /// </summary>
        public string Summary => $"{DataPosition.X:F2}, {SeriesValues.Count} series";

        public override string ToString()
        {
            return $"PinnedTooltip: {Summary} at {CreatedAt:HH:mm:ss}";
        }
    }
}