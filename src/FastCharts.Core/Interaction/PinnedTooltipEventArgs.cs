using System;

namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// Event arguments for pinned tooltip operations
    /// </summary>
    public class PinnedTooltipEventArgs : EventArgs
    {
        public PinnedTooltipEventArgs(PinnedTooltip tooltip, PinnedTooltipAction action)
        {
            Tooltip = tooltip;
            Action = action;
        }

        /// <summary>
        /// The tooltip involved in the operation
        /// </summary>
        public PinnedTooltip Tooltip { get; }

        /// <summary>
        /// The action performed
        /// </summary>
        public PinnedTooltipAction Action { get; }
    }
}