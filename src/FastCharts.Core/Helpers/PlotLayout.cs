using System;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Helpers
{
    /// <summary>
    /// Single place computing where the plot sits inside a chart surface. Renderers and
    /// interaction behaviors must use it so that hit-testing, zoom anchors and the crosshair
    /// line up with what is drawn (e.g. the right margin widened for secondary Y axis labels).
    /// </summary>
    public static class PlotLayout
    {
        /// <summary>
        /// Minimum right margin reserved for secondary Y axis labels.
        /// </summary>
        public const double SecondaryAxisMinRightMargin = 48.0;

        /// <summary>
        /// Right margin actually used, widened when a secondary Y axis needs room for its labels.
        /// </summary>
        public static double EffectiveRightMargin(double baseMargin, bool hasSecondaryYAxis)
        {
            return hasSecondaryYAxis ? Math.Max(baseMargin, SecondaryAxisMinRightMargin) : baseMargin;
        }

        /// <summary>
        /// Plot rectangle for the given margins and surface size.
        /// </summary>
        public static PlotArea Compute(Margins margins, bool hasSecondaryYAxis, double surfaceWidth, double surfaceHeight)
        {
            var right = EffectiveRightMargin(margins.Right, hasSecondaryYAxis);
            return new PlotArea(
                margins.Left,
                margins.Top,
                surfaceWidth - (margins.Left + right),
                surfaceHeight - (margins.Top + margins.Bottom));
        }

        /// <summary>
        /// Plot rectangle of <paramref name="model"/> on a surface of the given size.
        /// </summary>
        public static PlotArea Compute(ChartModel model, double surfaceWidth, double surfaceHeight)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            return Compute(model.PlotMargins, model.YAxisSecondary != null, surfaceWidth, surfaceHeight);
        }
    }
}
