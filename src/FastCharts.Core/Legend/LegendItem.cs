namespace FastCharts.Core.Legend
{
    /// <summary>
    /// A single legend entry referencing a series.
    /// </summary>
    public sealed class LegendItem
    {
        public LegendItem(string title, object seriesRef, int index)
        {
            Title = title;
            SeriesReference = seriesRef;
            SeriesIndex = index;
            IsVisible = true;
        }

        public string Title { get; set; }
        public object SeriesReference { get; }
        public int SeriesIndex { get; }
        public bool IsVisible { get; set; }
    }
}
