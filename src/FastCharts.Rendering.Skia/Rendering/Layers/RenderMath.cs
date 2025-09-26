namespace FastCharts.Rendering.Skia.Rendering.Layers
{
    internal static class RenderMath
    {
        public static double Clamp01(double v) => v < 0 ? 0 : (v > 1 ? 1 : v);
    }
}
