using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Annotations
{
    /// <summary>
    /// Represents a horizontal or vertical line annotation with optional label (P1-ANN-LINE)
    /// Pure POCO for Core layer - no UI coupling
    /// </summary>
    public sealed class AnnotationLine : IAnnotation
    {
        /// <summary>
        /// Initializes a new annotation line
        /// </summary>
        /// <param name="value">Y-value for horizontal line or X-value for vertical line</param>
        /// <param name="orientation">Orientation of the line</param>
        /// <param name="title">Optional title for the line</param>
        public AnnotationLine(double value, AnnotationOrientation orientation = AnnotationOrientation.Horizontal, string? title = null)
        {
            Value = value;
            Orientation = orientation;
            Color = new ColorRgba(128, 128, 128, 180); // Semi-transparent gray
            Title = title ?? (orientation == AnnotationOrientation.Horizontal ? $"Y = {value:F2}" : $"X = {value:F2}");
            
            // Set default values
            IsVisible = true;
            ZIndex = 0;
            IncludeInAutoFit = false;
            Thickness = 1.0;
            LineStyle = LineStyle.Solid;
            LabelPosition = LabelPosition.End;
            ShowLabel = true;
        }

        /// <summary>
        /// Gets or sets the title/label for this annotation
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets whether this annotation is visible
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the Z-index for layering annotations
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Gets or sets whether this annotation should be included in auto-fit calculations
        /// </summary>
        public bool IncludeInAutoFit { get; set; }

        /// <summary>
        /// Gets or sets the value where the line is drawn
        /// For horizontal lines, this is the Y-value. For vertical lines, this is the X-value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Gets or sets the orientation of the line (horizontal or vertical)
        /// </summary>
        public AnnotationOrientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the color of the annotation line
        /// </summary>
        public ColorRgba Color { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the annotation line in pixels
        /// </summary>
        public double Thickness { get; set; }

        /// <summary>
        /// Gets or sets the line style (solid, dashed, dotted)
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the position of the label on the line
        /// </summary>
        public LabelPosition LabelPosition { get; set; }

        /// <summary>
        /// Gets or sets whether to show the label for this annotation
        /// </summary>
        public bool ShowLabel { get; set; }
    }
}