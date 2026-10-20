using FastCharts.Core.Primitives;

namespace FastCharts.Core.Themes
{
    /// <summary>
    /// Ready-made color maps for heatmaps. <see cref="Viridis"/> (the default) is perceptually
    /// uniform and readable by color-blind users and in grayscale print.
    /// </summary>
    public static class ColorMaps
    {
        /// <summary>
        /// Dark purple to yellow, perceptually uniform (default).
        /// </summary>
        public static ColorMap Viridis { get; } = new ColorMap("Viridis", new[]
        {
            Hex(0x440154), Hex(0x482878), Hex(0x3E4989), Hex(0x31688E), Hex(0x26828E),
            Hex(0x1F9E89), Hex(0x35B779), Hex(0x6DCD59), Hex(0xB4DE2C), Hex(0xFDE725)
        });

        /// <summary>
        /// Dark blue to bright yellow through magenta, perceptually uniform.
        /// </summary>
        public static ColorMap Plasma { get; } = new ColorMap("Plasma", new[]
        {
            Hex(0x0D0887), Hex(0x47039F), Hex(0x7301A8), Hex(0x9C179E), Hex(0xBD3786),
            Hex(0xD8576B), Hex(0xED7953), Hex(0xFA9E3B), Hex(0xFDC926), Hex(0xF0F921)
        });

        /// <summary>
        /// Black to white.
        /// </summary>
        public static ColorMap Grayscale { get; } = new ColorMap("Grayscale", new[]
        {
            Hex(0x000000), Hex(0xFFFFFF)
        });

        /// <summary>
        /// Diverging blue - white - red, for values centered on a meaningful midpoint
        /// (set a symmetric <c>Minimum</c>/<c>Maximum</c> on the color axis).
        /// </summary>
        public static ColorMap BlueWhiteRed { get; } = new ColorMap("BlueWhiteRed", new[]
        {
            Hex(0x2166AC), Hex(0x67A9CF), Hex(0xD1E5F0), Hex(0xF7F7F7), Hex(0xFDDBC7), Hex(0xEF8A62), Hex(0xB2182B)
        });

        private static ColorRgba Hex(int rgb)
        {
            return new ColorRgba((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
        }
    }
}
