namespace FastCharts.Core.Abstractions
{
    /// <summary>
    /// Base interface for chart annotations (P1-ANN-LINE start)
    /// Annotations are decorative elements that overlay on top of chart data
    /// </summary>
    public interface IAnnotation
    {
        /// <summary>
        /// Gets or sets the title/label for this annotation
        /// </summary>
        string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether this annotation is visible
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the Z-index for layering annotations
        /// Higher values render on top
        /// </summary>
        int ZIndex { get; set; }

        /// <summary>
        /// Gets or sets whether this annotation should be included in auto-fit calculations
        /// </summary>
        bool IncludeInAutoFit { get; set; }
    }
}
