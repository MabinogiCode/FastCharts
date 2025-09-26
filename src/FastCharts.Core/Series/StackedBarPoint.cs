namespace FastCharts.Core.Series
{
    /// <summary>
    /// Stacked bar datum containing segment array.
    /// </summary>
    public struct StackedBarPoint
    {
        public double X { get; set; }
        public double[] Values { get; set; }
        public double? Width { get; set; }

        public StackedBarPoint(double x, double[] values, double? width = null)
        {
            X = x; Values = values; Width = width;
        }
    }
}
