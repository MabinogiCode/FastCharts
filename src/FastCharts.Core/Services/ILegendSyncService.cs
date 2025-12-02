using System.Collections.Generic;
using FastCharts.Core.Legend;
using FastCharts.Core.Series;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Service interface for synchronizing legend with series collection
    /// </summary>
    public interface ILegendSyncService
    {
        /// <summary>
        /// Synchronizes the legend model with the current series collection
        /// </summary>
        /// <param name="legend">Legend model to synchronize</param>
        /// <param name="series">Series collection to synchronize from</param>
        void SyncLegendWithSeries(LegendModel? legend, IEnumerable<SeriesBase>? series);

        /// <summary>
        /// Updates legend visibility based on series visibility changes
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series that changed visibility</param>
        /// <param name="isVisible">New visibility state</param>
        void UpdateSeriesVisibility(LegendModel? legend, SeriesBase? series, bool isVisible);

        /// <summary>
        /// Adds a series to the legend
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series to add</param>
        void AddSeriesToLegend(LegendModel? legend, SeriesBase? series);

        /// <summary>
        /// Removes a series from the legend
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series to remove</param>
        void RemoveSeriesFromLegend(LegendModel? legend, SeriesBase? series);
    }
}