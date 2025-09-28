using System.Collections.Generic;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes.BuiltIn;

public sealed class DarkTheme : ITheme
{
    public ColorRgba AxisColor => new(180, 180, 180);
    public ColorRgba GridColor => new(120, 120, 120, 40);
    public ColorRgba LabelColor => new(190, 190, 190);
    public double AxisThickness => 1;
    public double GridThickness => 1;
    public double TickLength => 5;
    public double LabelTextSize => 12;
    public ColorRgba PrimarySeriesColor => new(51, 153, 255);
    public IReadOnlyList<ColorRgba> SeriesPalette => _palette;

    private static readonly ColorRgba[] _palette = new ColorRgba[]
    {
        new( 51, 153, 255),
        new(255, 128,  64),
        new( 60, 220, 130),
        new(220,  70, 140),
        new(155, 120, 255),
        new(255, 200,  60),
    };

    public ColorRgba PlotBackgroundColor => new(18, 18, 18);
    public ColorRgba SurfaceBackgroundColor => new(18, 18, 18);
}
