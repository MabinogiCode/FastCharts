using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FastCharts.Core.Legend
{
    /// <summary>
    /// Holds legend items synchronized from the ChartModel's series collection.
    /// </summary>
    public sealed class LegendModel
    {
        public ObservableCollection<LegendItem> Items { get; } = new ObservableCollection<LegendItem>();

        public void SyncFromSeries(IReadOnlyList<object> seriesList)
        {
            // Simple full rebuild (optimize later if needed)
            Items.Clear();
            for (int i = 0; i < seriesList.Count; i++)
            {
                var s = seriesList[i];
                string title = (s as FastCharts.Core.Series.SeriesBase)?.Title ?? $"Series {i+1}";
                Items.Add(new LegendItem(title, s, i));
            }
        }

        public LegendItem? FindBySeries(object s)
        {
            return Items.FirstOrDefault(it => ReferenceEquals(it.SeriesReference, s));
        }
    }
}
