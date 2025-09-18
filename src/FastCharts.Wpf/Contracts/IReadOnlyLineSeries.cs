
namespace FastCharts.Wpf.Contracts
{
    public interface IReadOnlyLineSeries
    {
        double[] X { get; }
        double[] Y { get; }
        string Name { get; }
    }
}
