namespace FastCharts.Core.Series
{
    /// <summary>
    /// Line interpolation mode between data points.
    /// </summary>
    public enum LineSmoothing
    {
        /// <summary>
        /// Straight segments between points (default)
        /// </summary>
        None = 0,

        /// <summary>
        /// Smooth curve through the points (Catmull-Rom spline rendered as cubic Beziers)
        /// </summary>
        Spline = 1
    }
}
