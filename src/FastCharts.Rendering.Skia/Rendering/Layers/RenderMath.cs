namespace FastCharts.Rendering.Skia.Rendering.Layers;

internal static class RenderMath
{
    public static double Clamp01(double v)
    {
        if (v < 0) { return 0; }
        if (v > 1) { return 1; }
        return v;
    }
}
