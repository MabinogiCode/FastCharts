namespace FastCharts.Core.Primitives;

public readonly struct ColorRgba
{
    public readonly byte R, G, B, A;
    public ColorRgba(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; }
}
