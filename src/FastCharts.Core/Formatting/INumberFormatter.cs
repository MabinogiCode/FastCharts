namespace FastCharts.Core.Formatting
{
    /// <summary>
    /// Formats numeric tick values for axis labels.
    /// </summary>
    public interface INumberFormatter
    {
        /// <summary>
        /// Format a double value into a display string (culture-invariant by default).
        /// </summary>
        string Format(double value);
    }
}
