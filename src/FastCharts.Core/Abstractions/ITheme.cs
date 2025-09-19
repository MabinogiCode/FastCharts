using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions;

public interface ITheme
{
    ColorRgba AxisColor { get; }
    ColorRgba GridColor { get; }
    ColorRgba LabelColor { get; }
    double AxisThickness { get; }
    double GridThickness { get; }
    double TickLength { get; }
    double LabelTextSize { get; }
    ColorRgba PrimarySeriesColor { get; }
}
