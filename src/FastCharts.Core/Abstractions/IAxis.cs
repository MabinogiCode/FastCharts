using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions;

public interface IAxis<T>
{
    IScale<T> Scale { get; }
    ITicker<T> Ticker { get; }
    FRange DataRange { get; set; }
    FRange VisibleRange { get; set; }
    string? LabelFormat { get; set; }
}
