namespace FastCharts.Core.Series
{
    /// <summary>
    /// A single error bar datum with symmetric or asymmetric errors.
    /// </summary>
    public struct ErrorBarPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double PositiveError { get; set; }
        public double? NegativeError { get; set; }

        public ErrorBarPoint(double x, double y, double positiveError, double? negativeError = null)
        {
            X = x; Y = y; PositiveError = positiveError; NegativeError = negativeError;
        }
    }
}
