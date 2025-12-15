namespace FastCharts.Core.Interaction
{
    /// <summary>
    /// Types of actions that can be performed on pinned tooltips
    /// </summary>
    public enum PinnedTooltipAction
    {
        /// <summary>
        /// A tooltip was pinned (added)
        /// </summary>
        Pinned,

        /// <summary>
        /// A tooltip was unpinned (removed)
        /// </summary>
        Unpinned,

        /// <summary>
        /// A tooltip's visibility was toggled
        /// </summary>
        VisibilityToggled,

        /// <summary>
        /// All tooltips were cleared
        /// </summary>
        AllCleared
    }
}