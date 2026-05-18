namespace FastCharts.Core.DataBinding
{
    /// <summary>
    /// Units for DateTime to double conversion in data binding
    /// </summary>
    public enum DateTimeUnit
    {
        /// <summary>
        /// DateTime ticks (most precise)
        /// </summary>
        Ticks,

        /// <summary>
        /// Milliseconds since Unix epoch
        /// </summary>
        Milliseconds,

        /// <summary>
        /// Seconds since Unix epoch
        /// </summary>
        Seconds,

        /// <summary>
        /// Minutes since Unix epoch
        /// </summary>
        Minutes,

        /// <summary>
        /// Hours since Unix epoch
        /// </summary>
        Hours,

        /// <summary>
        /// Days since Unix epoch
        /// </summary>
        Days
    }
}
