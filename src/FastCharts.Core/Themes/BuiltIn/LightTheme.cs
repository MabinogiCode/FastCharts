using System.Collections.Generic;

using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes.BuiltIn
{
    public sealed class LightTheme : ITheme
    {
        public ColorRgba AxisColor
        {
            get { return new ColorRgba(153, 153, 153, 255); }
        }
        public ColorRgba GridColor
        {
            get { return new ColorRgba(80, 80, 80, 64); }
        }
        public ColorRgba LabelColor
        {
            get { return new ColorRgba(136, 136, 136, 255); }
        }
        public double AxisThickness
        {
            get { return 1; }
        }
        public double GridThickness
        {
            get { return 1; }
        }
        public double TickLength
        {
            get { return 5; }
        }
        public double LabelTextSize
        {
            get { return 12; }
        }
        public ColorRgba PrimarySeriesColor
        {
            get { return new ColorRgba(51, 153, 255, 255); }
        }
        public IReadOnlyList<ColorRgba> SeriesPalette
        {
            get { return _palette; }
        }
        private static readonly ColorRgba[] _palette =
        [
            new ColorRgba( 51, 153, 255),
            new ColorRgba(255, 128,  64),
            new ColorRgba( 60, 180,  90),
            new ColorRgba(220,  70, 140),
            new ColorRgba(155, 120, 255),
            new ColorRgba(255, 200,  60),
        ];
        public ColorRgba PlotBackgroundColor
        {
            get { return new ColorRgba(255, 255, 255); }
        }
        public ColorRgba SurfaceBackgroundColor
        {
            get { return new ColorRgba(255, 255, 255); }
        }
    }
}
