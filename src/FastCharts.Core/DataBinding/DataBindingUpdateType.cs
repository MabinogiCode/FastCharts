namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Types of data binding updates
    /// </summary>
    public enum DataBindingUpdateType
    {
        /// <summary>
        /// Full refresh of all data
        /// </summary>
        FullRefresh,

        /// <summary>
        /// Incremental update (add/remove/modify specific items)
        /// </summary>
        Incremental,

        /// <summary>
        /// Property values changed but collection structure unchanged
        /// </summary>
        PropertyUpdate
    }
}