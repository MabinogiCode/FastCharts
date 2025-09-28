namespace FastCharts.Core.Primitives
{
    public readonly struct ColorRgba(byte r, byte g, byte b, byte a = 255)
    {
        public byte R { get; } = r;
        public byte G { get; } = g;
        public byte B { get; } = b;
        public byte A { get; } = a;
    }
}
