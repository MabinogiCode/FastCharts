using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes.BuiltIn;

public sealed class LightTheme : ITheme
{
    public ColorRgba AxisColor => new(153, 153, 153, 255);   // #999
    public ColorRgba GridColor => new(80, 80, 80, 64);       // subtle grid
    public ColorRgba LabelColor => new(136, 136, 136, 255);  // #888
    public double AxisThickness => 1;
    public double GridThickness => 1;
    public double TickLength => 5;
    public double LabelTextSize => 12;
    public ColorRgba PrimarySeriesColor => new(51, 153, 255, 255); // blue
}
