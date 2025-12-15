using System.Collections.Generic;

namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Interface for series that support observable data binding
    /// Enables automatic synchronization with source collections and property paths
    /// </summary>
    public interface IObservableSeries<T> : IObservableSeries
    {
        /// <summary>
        /// Gets or sets the source collection for data binding
        /// Supports INotifyCollectionChanged for automatic updates
        /// </summary>
        IEnumerable<T> ItemsSource { get; set; }
    }
}