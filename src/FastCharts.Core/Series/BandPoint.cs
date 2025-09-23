namespace FastCharts.Core.Series
{
    /// <summary>
    /// Single band datum: an interval [YLow, YHigh] at X.
    /// </summary>
    public struct BandPoint
    {
        public double X { get; set; }

        public double YLow { get; set; }

        public double YHigh { get; set; }

        public BandPoint(double x, double yLow, double yHigh)
        {
            X = x;
            YLow = yLow;
            YHigh = yHigh;
        }
    }
}
