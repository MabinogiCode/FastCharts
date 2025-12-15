using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Annotations
{
    /// <summary>
    /// Represents a range annotation that highlights an area between two values (P1-ANN-RANGE)
    /// Can be horizontal (between two Y values) or vertical (between two X values)
    /// Pure POCO for Core layer - no UI coupling
    /// </summary>
    public class AnnotationRange : IAnnotation
    {
        private double _startValue;
        private double _endValue;

        /// <summary>
        /// Initializes a new instance of the AnnotationRange class
        /// </summary>
        /// <param name="startValue">Start value of the range</param>
        /// <param name="endValue">End value of the range</param>
        /// <param name="orientation">Orientation of the range (horizontal or vertical)</param>
        /// <param name="title">Optional title for the range</param>
        public AnnotationRange(double startValue, double endValue, AnnotationOrientation orientation, string? title = null)
        {
            _startValue = Math.Min(startValue, endValue);
            _endValue = Math.Max(startValue, endValue);
            Orientation = orientation;
            Title = title ?? GenerateDefaultTitle();

            // Set default values
            FillColor = new ColorRgba(128, 128, 128, 60); // Semi-transparent gray
            BorderColor = new ColorRgba(128, 128, 128, 120);
            BorderThickness = 1.0;
            LabelPosition = LabelPosition.Middle;
            IsVisible = true;
            ZIndex = 0;
            IncludeInAutoFit = false;
        }

        /// <summary>
        /// Start value of the range
        /// </summary>
        public double StartValue
        {
            get => _startValue;
            set
            {
                _startValue = value;

                // Ensure start <= end
                if (_startValue > _endValue)
                {
                    (_startValue, _endValue) = (_endValue, _startValue);
                }

                UpdateDefaultTitle();
            }
        }

        /// <summary>
        /// End value of the range
        /// </summary>
        public double EndValue
        {
            get => _endValue;
            set
            {
                _endValue = value;

                // Ensure start <= end
                if (_startValue > _endValue)
                {
                    (_startValue, _endValue) = (_endValue, _startValue);
                }

                UpdateDefaultTitle();
            }
        }

        /// <summary>
        /// Orientation of the range (horizontal or vertical)
        /// </summary>
        public AnnotationOrientation Orientation { get; set; }

        /// <summary>
        /// Fill color for the range area
        /// </summary>
        public ColorRgba FillColor { get; set; }

        /// <summary>
        /// Border color for the range outline
        /// </summary>
        public ColorRgba BorderColor { get; set; }

        /// <summary>
        /// Thickness of the border line
        /// </summary>
        public double BorderThickness { get; set; }

        /// <summary>
        /// Position of the label on the range
        /// </summary>
        public LabelPosition LabelPosition { get; set; }

        // IAnnotation implementation
        public string? Title { get; set; }
        public bool IsVisible { get; set; }
        public int ZIndex { get; set; }
        public bool IncludeInAutoFit { get; set; }

        /// <summary>
        /// Gets the span of the range
        /// </summary>
        public double Span => _endValue - _startValue;

        /// <summary>
        /// Gets the center value of the range
        /// </summary>
        public double Center => (_startValue + _endValue) / 2.0;

        /// <summary>
        /// Checks if a value is within this range
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value is within the range</returns>
        public bool Contains(double value)
        {
            return value >= _startValue && value <= _endValue;
        }

        public override string ToString()
        {
            var orientationStr = Orientation == AnnotationOrientation.Horizontal ? "Horizontal" : "Vertical";
            return $"{orientationStr} Range: [{_startValue:F2} - {_endValue:F2}]";
        }

        private string GenerateDefaultTitle()
        {
            return Orientation == AnnotationOrientation.Horizontal
                ? $"Y Range [{_startValue:F2} - {_endValue:F2}]"
                : $"X Range [{_startValue:F2} - {_endValue:F2}]";
        }

        private void UpdateDefaultTitle()
        {
            // Only update if using default title format
            if (string.IsNullOrEmpty(Title) || (Title?.Contains("Range [") == true))
            {
                Title = GenerateDefaultTitle();
            }
        }
    }
}