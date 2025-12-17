using System;

namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// Test data class for sensor readings
    /// </summary>
    public class SensorReading
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
    }
}