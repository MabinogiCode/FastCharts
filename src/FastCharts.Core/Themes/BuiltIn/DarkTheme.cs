using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes.BuiltIn;

public sealed class DarkTheme : ITheme
{
    public ColorRgba AxisColor       => new(180, 180, 180);
    public ColorRgba GridColor       => new(120, 120, 120, 40);
    public ColorRgba LabelColor      => new(190, 190, 190);
    public double    AxisThickness   => 1;
    public double    GridThickness   => 1;
    public double    TickLength      => 5;
    public double    LabelTextSize   => 12;
    public ColorRgba PrimarySeriesColor => new(51, 153, 255);

    // pleasant, high-contrast palette for multiple series
    public IReadOnlyList<ColorRgba> SeriesPalette => _palette;
    private static readonly ColorRgba[] _palette =
    {
        new( 51, 153, 255), // blue
        new(255, 128,  64), // orange
        new( 60, 220, 130), // green
        new(220,  70, 140), // pink
        new(155, 120, 255), // purple
        new(255, 200,  60), // yellow
    };
}
