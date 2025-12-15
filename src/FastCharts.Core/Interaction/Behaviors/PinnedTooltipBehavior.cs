using FastCharts.Core.Primitives;
using System;
using System.Globalization;
using System.Linq;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Enhanced tooltip behavior with pinning capabilities (P1-TOOLTIP-PIN)
    /// Allows users to pin multiple tooltips simultaneously for comparison
    /// </summary>
    public sealed class PinnedTooltipBehavior : IBehavior
    {
        /// <summary>
        /// Usage instructions for pinned tooltips
        /// </summary>
        public static string UsageInstructions => "Right-click to pin tooltip • Right-click pinned tooltip to remove • Left-click pinned tooltip to toggle visibility";

        /// <summary>
        /// Maximum number of pinned tooltips allowed
        /// </summary>
        public int MaxPinnedTooltips { get; set; } = 10;

        /// <summary>
        /// Whether to automatically remove oldest pinned tooltip when limit is reached
        /// </summary>
        public bool AutoRemoveOldest { get; set; } = true;

        /// <summary>
        /// Distance threshold for detecting clicks on existing pinned tooltips (pixels)
        /// </summary>
        public double ClickDetectionRadius { get; set; } = 15.0;

        /// <summary>
        /// Whether pinned tooltips should be automatically repositioned on zoom/pan
        /// </summary>
        public bool AutoRepositionOnTransform { get; set; } = true;

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            model.InteractionState ??= new InteractionState();
            var state = model.InteractionState;

            return ev.Type switch
            {
                PointerEventType.Down when ev.Button == PointerButton.Right => HandleRightClick(state, ev),
                PointerEventType.Down when ev.Button == PointerButton.Left && ev.Modifiers.Ctrl => HandleCtrlClick(state, ev),
                PointerEventType.Down when ev.Button == PointerButton.Left => HandleLeftClick(state, ev),
                PointerEventType.Move => HandleMouseMove(model, state),
                _ => false
            };
        }

        /// <summary>
        /// Handle right-click: Pin current tooltip
        /// </summary>
        private bool HandleRightClick(InteractionState state, InteractionEvent ev)
        {
            // Check if we're clicking on an existing pinned tooltip to remove it
            var existingTooltip = state.FindNearestPinnedTooltip(ev.PixelX, ev.PixelY, ClickDetectionRadius);
            if (existingTooltip != null)
            {
                state.UnpinTooltip(existingTooltip.Id);
                return true;
            }

            // Pin current tooltip if available
            if (!string.IsNullOrEmpty(state.TooltipText) && state.DataX.HasValue && state.DataY.HasValue)
            {
                // Check if we've reached the limit
                if (state.PinnedTooltips.Count >= MaxPinnedTooltips)
                {
                    if (AutoRemoveOldest && state.PinnedTooltips.Count > 0)
                    {
                        var oldest = state.PinnedTooltips.OrderBy(t => t.CreatedAt).First();
                        state.UnpinTooltip(oldest.Id);
                    }
                    else
                    {
                        return false; // Can't pin more
                    }
                }

                var pinned = state.PinCurrentTooltip($"Pin {state.PinnedTooltips.Count + 1}");
                return pinned != null;
            }

            return false;
        }

        /// <summary>
        /// Handle Ctrl+Click: Quick pin without removing existing tooltip behavior
        /// </summary>
        private bool HandleCtrlClick(InteractionState state, InteractionEvent ev)
        {
            return HandleRightClick(state, ev); // Same behavior for now
        }

        /// <summary>
        /// Handle left-click: Check for clicking on pinned tooltips for interaction
        /// </summary>
        private bool HandleLeftClick(InteractionState state, InteractionEvent ev)
        {
            var nearestTooltip = state.FindNearestPinnedTooltip(ev.PixelX, ev.PixelY, ClickDetectionRadius);
            if (nearestTooltip != null)
            {
                // For now, just toggle visibility
                state.ToggleTooltipVisibility(nearestTooltip.Id);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle mouse move: Update pixel positions of pinned tooltips if needed
        /// </summary>
        private bool HandleMouseMove(ChartModel model, InteractionState state)
        {
            if (!AutoRepositionOnTransform)
                return false;

            // Update pixel positions for all pinned tooltips based on current scale
            var needsUpdate = false;
            foreach (var tooltip in state.PinnedTooltips)
            {
                var newPixelX = model.XAxis.Scale?.ToPixels(tooltip.DataPosition.X) ?? tooltip.PixelPosition.X;
                var newPixelY = model.YAxis.Scale?.ToPixels(tooltip.DataPosition.Y) ?? tooltip.PixelPosition.Y;

                if (Math.Abs(newPixelX - tooltip.PixelPosition.X) > 1.0 || 
                    Math.Abs(newPixelY - tooltip.PixelPosition.Y) > 1.0)
                {
                    tooltip.PixelPosition = new PointD(newPixelX, newPixelY);
                    needsUpdate = true;
                }
            }

            return needsUpdate;
        }

        /// <summary>
        /// Gets statistics about current pinned tooltips
        /// </summary>
        public static string GetPinnedTooltipsInfo(InteractionState state)
        {
            var visible = state.GetVisiblePinnedTooltips().Count();
            var total = state.PinnedTooltips.Count;
            
            if (total == 0)
                return "No pinned tooltips";
                
            return $"{visible}/{total} pinned tooltips visible";
        }

        /// <summary>
        /// Clears all pinned tooltips (utility method for UI)
        /// </summary>
        public static void ClearAllPinnedTooltips(InteractionState state)
        {
            state.ClearAllPinnedTooltips();
        }

        /// <summary>
        /// Exports pinned tooltip data to a formatted string for analysis
        /// </summary>
        public static string ExportPinnedTooltipsData(InteractionState state)
        {
            if (state.PinnedTooltips.Count == 0)
                return "No pinned tooltips to export.";

            var lines = new System.Collections.Generic.List<string>
            {
                "Pinned Tooltips Export",
                $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                $"Total: {state.PinnedTooltips.Count}",
                ""
            };

            foreach (var tooltip in state.PinnedTooltips.OrderBy(t => t.DataPosition.X))
            {
                lines.Add($"Pin: {tooltip.Label ?? "Unlabeled"}");
                lines.Add($"  Position: X={tooltip.DataPosition.X:F3}, Y={tooltip.DataPosition.Y:F3}");
                lines.Add($"  Created: {tooltip.CreatedAt:HH:mm:ss}");
                lines.Add($"  Series: {tooltip.SeriesValues.Count}");
                
                foreach (var series in tooltip.SeriesValues)
                {
                    lines.Add($"    {series.Title}: {series.Y:F3}");
                }
                lines.Add("");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}