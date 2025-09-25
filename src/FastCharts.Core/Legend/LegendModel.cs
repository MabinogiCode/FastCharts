using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FastCharts.Core.Series;

namespace FastCharts.Core.Legend
{
    /// <summary>
    /// Holds legend items synchronized from the ChartModel's series collection.
    /// </summary>
    public sealed class LegendModel
    {
        public ObservableCollection<LegendItem> Items { get; } = new ObservableCollection<LegendItem>();

        /// <summary>
        /// Controls whether the legend should be rendered. When false, renderers skip drawing it, but series remain unaffected.
        /// </summary>
        public bool IsVisible { get; set; } = true;

        public void SyncFromSeries(IReadOnlyList<SeriesBase> seriesList)
        {
            Items.Clear();
            for (int i = 0; i < seriesList.Count; i++)
            {
                var s = seriesList[i];
                string title = s.Title ?? $"Series {i + 1}";
                Items.Add(new LegendItem(title, s, i));
            }
        }

        public LegendItem? FindBySeries(SeriesBase s)
        {
            return Items.FirstOrDefault(it => ReferenceEquals(it.SeriesReference, s));
        }
    }
}
