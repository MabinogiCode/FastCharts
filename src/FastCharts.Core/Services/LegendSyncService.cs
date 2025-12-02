using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Legend;
using FastCharts.Core.Series;
using FastCharts.Core.Services;

namespace FastCharts.Core.Services
{
    /// <summary>
    /// Default implementation of legend synchronization service
    /// </summary>
    public sealed class LegendSyncService : ILegendSyncService
    {
        /// <summary>
        /// Synchronizes the legend model with the current series collection
        /// </summary>
        /// <param name="legend">Legend model to synchronize</param>
        /// <param name="series">Series collection to synchronize from</param>
        public void SyncLegendWithSeries(LegendModel? legend, IEnumerable<SeriesBase>? series)
        {
            if (legend == null || series == null)
            {
                return;
            }

            // Convert to IReadOnlyList for LegendModel.SyncFromSeries compatibility
            var seriesList = series.ToList();
            legend.SyncFromSeries(seriesList);
        }

        /// <summary>
        /// Updates legend visibility based on series visibility changes
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series that changed visibility</param>
        /// <param name="isVisible">New visibility state</param>
        public void UpdateSeriesVisibility(LegendModel? legend, SeriesBase? series, bool isVisible)
        {
            if (legend == null || series == null)
            {
                return;
            }

            // Find the legend item for this series and update visibility
            var legendItem = legend.Items.FirstOrDefault(item => item.Title == series.Title);
            if (legendItem != null)
            {
                // Update legend item visibility logic here if LegendModel supports it
                // For now, this is a placeholder for future legend visibility features
            }
        }

        /// <summary>
        /// Adds a series to the legend
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series to add</param>
        public void AddSeriesToLegend(LegendModel? legend, SeriesBase? series)
        {
            if (legend == null || series == null)
            {
                return;
            }

            // Check if series is already in legend
            var existingItem = legend.Items.FirstOrDefault(item => item.Title == series.Title);
            if (existingItem == null)
            {
                // Create new legend item for the series
                // This would involve creating a new LegendItem based on series properties
                // For now, we rely on SyncFromSeries for consistency
                SyncLegendWithSeries(legend, new[] { series });
            }
        }

        /// <summary>
        /// Removes a series from the legend
        /// </summary>
        /// <param name="legend">Legend model to update</param>
        /// <param name="series">Series to remove</param>
        public void RemoveSeriesFromLegend(LegendModel? legend, SeriesBase? series)
        {
            if (legend == null || series == null)
            {
                return;
            }

            // Find and remove the legend item for this series
            var itemToRemove = legend.Items.FirstOrDefault(item => item.Title == series.Title);
            if (itemToRemove != null)
            {
                // Remove logic would go here if LegendModel supported direct item removal
                // For now, this is a placeholder for future legend management features
            }
        }
    }
}