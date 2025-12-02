using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;
using ReactiveUI;

namespace FastCharts.Core.Annotations
{
    /// <summary>
    /// Represents a horizontal or vertical line annotation with optional label (P1-ANN-LINE)
    /// </summary>
    public sealed class AnnotationLine : ReactiveObject, IAnnotation
    {
        private string? _title;
        private bool _isVisible = true;
        private int _zIndex = 0;
        private bool _includeInAutoFit = false;
        private double _value;
        private AnnotationOrientation _orientation;
        private ColorRgba _color;
        private double _thickness = 1.0;
        private LineStyle _lineStyle = LineStyle.Solid;
        private LabelPosition _labelPosition = LabelPosition.End;
        private bool _showLabel = true;

        /// <summary>
        /// Initializes a new horizontal annotation line
        /// </summary>
        /// <param name="value">Y-value for horizontal line or X-value for vertical line</param>
        /// <param name="orientation">Orientation of the line</param>
        public AnnotationLine(double value, AnnotationOrientation orientation = AnnotationOrientation.Horizontal)
        {
            _value = value;
            _orientation = orientation;
            _color = new ColorRgba(128, 128, 128, 180); // Semi-transparent gray
            _title = orientation == AnnotationOrientation.Horizontal ? $"Y = {value:F2}" : $"X = {value:F2}";
        }

        /// <summary>
        /// Gets or sets the title/label for this annotation
        /// </summary>
        public string? Title 
        { 
            get => _title; 
            set => this.RaiseAndSetIfChanged(ref _title, value); 
        }

        /// <summary>
        /// Gets or sets whether this annotation is visible
        /// </summary>
        public bool IsVisible 
        { 
            get => _isVisible; 
            set => this.RaiseAndSetIfChanged(ref _isVisible, value); 
        }

        /// <summary>
        /// Gets or sets the Z-index for layering annotations
        /// </summary>
        public int ZIndex 
        { 
            get => _zIndex; 
            set => this.RaiseAndSetIfChanged(ref _zIndex, value); 
        }

        /// <summary>
        /// Gets or sets whether this annotation should be included in auto-fit calculations
        /// </summary>
        public bool IncludeInAutoFit 
        { 
            get => _includeInAutoFit; 
            set => this.RaiseAndSetIfChanged(ref _includeInAutoFit, value); 
        }

        /// <summary>
        /// Gets or sets the value where the line is drawn
        /// For horizontal lines, this is the Y-value. For vertical lines, this is the X-value.
        /// </summary>
        public double Value 
        { 
            get => _value; 
            set => this.RaiseAndSetIfChanged(ref _value, value); 
        }

        /// <summary>
        /// Gets or sets the orientation of the line (horizontal or vertical)
        /// </summary>
        public AnnotationOrientation Orientation 
        { 
            get => _orientation; 
            set => this.RaiseAndSetIfChanged(ref _orientation, value); 
        }

        /// <summary>
        /// Gets or sets the color of the annotation line
        /// </summary>
        public ColorRgba Color 
        { 
            get => _color; 
            set => this.RaiseAndSetIfChanged(ref _color, value); 
        }

        /// <summary>
        /// Gets or sets the thickness of the annotation line in pixels
        /// </summary>
        public double Thickness 
        { 
            get => _thickness; 
            set => this.RaiseAndSetIfChanged(ref _thickness, Math.Max(0.1, value)); 
        }

        /// <summary>
        /// Gets or sets the line style (solid, dashed, dotted)
        /// </summary>
        public LineStyle LineStyle 
        { 
            get => _lineStyle; 
            set => this.RaiseAndSetIfChanged(ref _lineStyle, value); 
        }

        /// <summary>
        /// Gets or sets the position of the label on the line
        /// </summary>
        public LabelPosition LabelPosition 
        { 
            get => _labelPosition; 
            set => this.RaiseAndSetIfChanged(ref _labelPosition, value); 
        }

        /// <summary>
        /// Gets or sets whether to show the label for this annotation
        /// </summary>
        public bool ShowLabel 
        { 
            get => _showLabel; 
            set => this.RaiseAndSetIfChanged(ref _showLabel, value); 
        }

        /// <summary>
        /// Creates a horizontal annotation line at the specified Y-value
        /// </summary>
        /// <param name="yValue">Y-value where the horizontal line will be drawn</param>
        /// <param name="title">Optional title for the line</param>
        /// <returns>New horizontal annotation line</returns>
        public static AnnotationLine Horizontal(double yValue, string? title = null)
        {
            return new AnnotationLine(yValue, AnnotationOrientation.Horizontal)
            {
                Title = title ?? $"Y = {yValue:F2}"
            };
        }

        /// <summary>
        /// Creates a vertical annotation line at the specified X-value
        /// </summary>
        /// <param name="xValue">X-value where the vertical line will be drawn</param>
        /// <param name="title">Optional title for the line</param>
        /// <returns>New vertical annotation line</returns>
        public static AnnotationLine Vertical(double xValue, string? title = null)
        {
            return new AnnotationLine(xValue, AnnotationOrientation.Vertical)
            {
                Title = title ?? $"X = {xValue:F2}"
            };
        }
    }

    /// <summary>
    /// Defines the orientation of an annotation line
    /// </summary>
    public enum AnnotationOrientation
    {
        /// <summary>
        /// Horizontal line (constant Y-value)
        /// </summary>
        Horizontal,

        /// <summary>
        /// Vertical line (constant X-value)
        /// </summary>
        Vertical
    }

    /// <summary>
    /// Defines the style of an annotation line
    /// </summary>
    public enum LineStyle
    {
        /// <summary>
        /// Solid continuous line
        /// </summary>
        Solid,

        /// <summary>
        /// Dashed line
        /// </summary>
        Dashed,

        /// <summary>
        /// Dotted line
        /// </summary>
        Dotted,

        /// <summary>
        /// Dash-dot pattern
        /// </summary>
        DashDot
    }

    /// <summary>
    /// Defines the position of a label on an annotation line
    /// </summary>
    public enum LabelPosition
    {
        /// <summary>
        /// Label at the start of the line
        /// </summary>
        Start,

        /// <summary>
        /// Label at the middle of the line
        /// </summary>
        Middle,

        /// <summary>
        /// Label at the end of the line
        /// </summary>
        End
    }
}