namespace FastCharts.Core.Primitives
{
    public readonly struct Margins
    {
        public Margins(double left, double top, double right, double bottom)
        { Left = left; Top = top; Right = right; Bottom = bottom; }

        public double Left  { get; }
        public double Top   { get; }
        public double Right { get; }
        public double Bottom{ get; }

        public void Deconstruct(out double left, out double top, out double right, out double bottom)
        { left = Left; top = Top; right = Right; bottom = Bottom; }
    }
}
