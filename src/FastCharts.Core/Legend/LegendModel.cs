using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FastCharts.Core.Series;

namespace FastCharts.Core.Legend;

public sealed class LegendModel
{
    public ObservableCollection<LegendItem> Items { get; } = new();
    public bool IsVisible { get; set; } = true;

    public void SyncFromSeries(IReadOnlyList<SeriesBase> seriesList)
    {
        Items.Clear();
        for (var i = 0; i < seriesList.Count; i++)
        {
            var s = seriesList[i];
            var title = s.Title ?? $"Series {i + 1}";
            Items.Add(new LegendItem(title, s, i));
        }
    }

    public LegendItem? FindBySeries(SeriesBase s)
    {
        return Items.FirstOrDefault(it => ReferenceEquals(it.SeriesReference, s));
    }
}
