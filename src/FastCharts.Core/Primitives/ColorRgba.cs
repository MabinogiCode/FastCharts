namespace FastCharts.Core.Primitives;

public readonly struct ColorRgba
{
    public byte R { get; }
    public byte G { get; }
    public byte B { get; }
    public byte A { get; }
    public ColorRgba(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}
