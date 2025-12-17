using System;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Non-generic interface for observable series
    /// </summary>
    public interface IObservableSeries
    {
        /// <summary>
        /// Gets or sets the property path for X values (e.g., "Time", "Date.Ticks")
        /// </summary>
        string? XPath { get; set; }

        /// <summary>
        /// Gets or sets the property path for Y values (e.g., "Value", "Price.Close")
        /// </summary>
        string? YPath { get; set; }

        /// <summary>
        /// Gets or sets the property path for item titles/labels (optional)
        /// </summary>
        string? TitlePath { get; set; }

        /// <summary>
        /// Gets or sets whether to automatically refresh when source data changes
        /// </summary>
        bool AutoRefresh { get; set; }

        /// <summary>
        /// Gets or sets the refresh throttle interval to batch updates
        /// </summary>
        TimeSpan RefreshThrottle { get; set; }

        /// <summary>
        /// Manually refresh data from the source
        /// </summary>
        void RefreshData();

        /// <summary>
        /// Event raised when data binding updates the series
        /// </summary>
        event EventHandler<DataBindingUpdatedEventArgs>? DataBindingUpdated;
    }
}