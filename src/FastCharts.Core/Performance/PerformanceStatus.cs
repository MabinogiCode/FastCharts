namespace FastCharts.Core.Performance
{
    /// <summary>
    /// Performance status levels
    /// </summary>
    public enum PerformanceStatus
    {
        /// <summary>
        /// Excellent performance (55+ FPS)
        /// </summary>
        Excellent,

        /// <summary>
        /// Good performance (25-54 FPS)
        /// </summary>
        Good,

        /// <summary>
        /// Fair performance (15-24 FPS)
        /// </summary>
        Fair,

        /// <summary>
        /// Poor performance (<15 FPS)
        /// </summary>
        Poor
    }
}