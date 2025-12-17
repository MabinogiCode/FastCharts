namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Test data class for complex data points with nested properties
    /// </summary>
    public class ComplexDataPoint
    {
        public Point2D Location { get; set; } = new();
        public int Value { get; set; }
    }
}