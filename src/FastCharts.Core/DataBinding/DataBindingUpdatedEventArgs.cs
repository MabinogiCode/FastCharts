using System;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Event arguments for data binding updates
    /// </summary>
    public class DataBindingUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the number of items added
        /// </summary>
        public int ItemsAdded { get; set; }

        /// <summary>
        /// Gets the number of items removed
        /// </summary>
        public int ItemsRemoved { get; set; }

        /// <summary>
        /// Gets the number of items updated
        /// </summary>
        public int ItemsUpdated { get; set; }

        /// <summary>
        /// Gets the total number of items after the update
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Gets the update type
        /// </summary>
        public DataBindingUpdateType UpdateType { get; set; }
    }
}