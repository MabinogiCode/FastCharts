namespace FastCharts.Core.Series
{
    /// <summary>
    /// Bar datum: X position, Y value, optional per-point width.
    /// </summary>
    public struct BarPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double? Width { get; set; }

        public BarPoint(double x, double y, double? width = null)
        {
            X = x; Y = y; Width = width;
        }
    }
}
