namespace FastCharts.Core.Abstractions;

public interface IScale<T>
{
    double ToPixels(T value);
    T FromPixels(double px);
}
