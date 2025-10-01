namespace FastCharts.Rendering.Skia.Helpers
{
    /// <summary>
    /// ? SOLID PRINCIPLES: Helper class for rendering constants and utilities
    /// Extracted to follow SRP and enable easier maintenance
    /// </summary>
    public static class RenderingHelper
    {
        /// <summary>
        /// Default margin extension for secondary Y axis labels
        /// </summary>
        public const double DefaultSecondaryAxisMargin = 48.0;

        /// <summary>
        /// Minimum plot dimension to prevent rendering issues
        /// </summary>
        public const int MinimumPlotDimension = 1;

        /// <summary>
        /// Default anti-aliasing tolerance for color comparisons
        /// </summary>
        public const byte DefaultColorTolerance = 6;

        /// <summary>
        /// Default PNG quality for exports
        /// </summary>
        public const int DefaultPngQuality = 100;

        /// <summary>
        /// Calculates the effective right margin considering secondary Y axis
        /// </summary>
        /// <param name="baseMargin">Base right margin</param>
        /// <param name="hasSecondaryYAxis">Whether secondary Y axis is present</param>
        /// <returns>Effective right margin</returns>
        public static double CalculateEffectiveRightMargin(double baseMargin, bool hasSecondaryYAxis)
        {
            return hasSecondaryYAxis ? System.Math.Max(baseMargin, DefaultSecondaryAxisMargin) : baseMargin;
        }

        /// <summary>
        /// Ensures plot dimensions are valid for rendering
        /// </summary>
        /// <param name="width">Proposed width</param>
        /// <param name="height">Proposed height</param>
        /// <returns>Validated dimensions (at least minimum size)</returns>
        public static (int width, int height) EnsureValidPlotDimensions(int width, int height)
        {
            return (
                System.Math.Max(width, MinimumPlotDimension),
                System.Math.Max(height, MinimumPlotDimension)
            );
        }
    }
}