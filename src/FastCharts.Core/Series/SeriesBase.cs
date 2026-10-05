using System;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Base class for all series types. Holds common metadata used by renderers.
    /// </summary>
    public abstract class SeriesBase
    {
        /// <summary>
        /// Display name (for legends, tooltips).
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Whether the series is visible. Renderers should skip drawing when false.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Z-order hint. Higher values should be drawn after lower ones.
        /// </summary>
        public int ZIndex { get; set; }

        /// <summary>
        /// Optional fixed palette index to pick a color deterministically.
        /// If null, renderer chooses sequentially.
        /// </summary>
        public int? PaletteIndex { get; set; }

        /// <summary>
        /// Line/outline thickness in pixels (when applicable).
        /// </summary>
        public double StrokeThickness { get; set; }

        /// <summary>
        /// Tag object for app-specific metadata.
        /// </summary>
        public object? Tag { get; set; }

        /// <summary>
        /// Indicates which Y axis this series should use (0 = primary, 1 = secondary). Defaults to 0.
        /// </summary>
        public int YAxisIndex { get; set; }

        /// <summary>
        /// Series has no data to render.
        /// </summary>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Raised when the series content changed and hosts should redraw (points appended,
        /// data replaced by a binding, ...). May be raised from a background thread.
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// Signals that the series content changed so hosting charts redraw. Mutations made
        /// through the series API call it automatically; call it after editing <c>Data</c> directly.
        /// </summary>
        public void NotifyChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        protected SeriesBase()
        {
            Title = null;
            IsVisible = true;
            ZIndex = 0;
            PaletteIndex = null;
            StrokeThickness = 1.5;
            Tag = null;
        }
    }
}
