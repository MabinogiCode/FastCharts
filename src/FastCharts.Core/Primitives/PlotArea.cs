namespace FastCharts.Core.Primitives
{
    /// <summary>
    /// Plot rectangle inside a chart surface (same units as the surface: pixels or DIPs).
    /// </summary>
    public readonly struct PlotArea
    {
        public PlotArea(double left, double top, double width, double height)
        {
            Left = left;
            Top = top;
            Width = width < 0 ? 0 : width;
            Height = height < 0 ? 0 : height;
        }

        public double Left { get; }

        public double Top { get; }

        public double Width { get; }

        public double Height { get; }

        public double Right => Left + Width;

        public double Bottom => Top + Height;

        /// <summary>
        /// True when there is no room left to draw the plot.
        /// </summary>
        public bool IsEmpty => Width <= 0 || Height <= 0;
    }
}
