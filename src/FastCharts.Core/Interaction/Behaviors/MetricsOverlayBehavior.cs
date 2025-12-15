using FastCharts.Core.Performance;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Behavior for displaying render metrics overlay (P1-METRICS)
    /// Shows FPS, frame time, data points, memory usage, and performance status
    /// </summary>
    public sealed class MetricsOverlayBehavior : IBehavior
    {
        private readonly RenderMetrics _metrics = new();

        /// <summary>
        /// Whether the metrics overlay is visible
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Position of the metrics overlay on screen
        /// </summary>
        public MetricsPosition Position { get; set; } = MetricsPosition.TopLeft;

        /// <summary>
        /// Whether to show detailed metrics or just basic FPS
        /// </summary>
        public bool ShowDetailed { get; set; } = true;

        /// <summary>
        /// Background opacity for the metrics overlay (0.0 - 1.0)
        /// </summary>
        public double BackgroundOpacity { get; set; } = 0.8;

        /// <summary>
        /// Text color for the metrics display
        /// </summary>
        public ColorRgba TextColor { get; set; } = new(255, 255, 255, 255);

        /// <summary>
        /// Background color for the metrics display
        /// </summary>
        public ColorRgba BackgroundColor { get; set; } = new(0, 0, 0, 204); // 80% opacity black

        /// <summary>
        /// Gets the current render metrics
        /// </summary>
        public RenderMetrics Metrics => _metrics;

        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            // Handle keyboard shortcuts for metrics control
            if (ev.Type == PointerEventType.KeyDown)
            {
                switch (ev.Key?.ToUpperInvariant())
                {
                    case "F3":
                        IsVisible = !IsVisible;
                        return true;
                    case "F4":
                        ShowDetailed = !ShowDetailed;
                        return true;
                    case "F5":
                        _metrics.Reset();
                        return true;
                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Should be called at the start of each render frame
        /// </summary>
        public void BeginFrame()
        {
            _metrics.StartFrame();
        }

        /// <summary>
        /// Should be called at the end of each render frame
        /// </summary>
        /// <param name="model">Chart model to extract data statistics from</param>
        public void EndFrame(ChartModel model)
        {
            UpdateDataMetrics(model);
            _metrics.EndFrame();
        }

        /// <summary>
        /// Gets the formatted metrics text for display
        /// </summary>
        public string GetDisplayText()
        {
            if (!IsVisible) return string.Empty;

            if (ShowDetailed)
            {
                return GetDetailedMetricsText();
            }
            else
            {
                return GetBasicMetricsText();
            }
        }

        /// <summary>
        /// Gets the performance-based color for the FPS display
        /// </summary>
        public ColorRgba GetPerformanceColor()
        {
            return _metrics.GetPerformanceStatus() switch
            {
                PerformanceStatus.Excellent => new ColorRgba(0, 255, 0, 255),   // Green
                PerformanceStatus.Good => new ColorRgba(255, 255, 0, 255),      // Yellow
                PerformanceStatus.Fair => new ColorRgba(255, 165, 0, 255),      // Orange
                PerformanceStatus.Poor => new ColorRgba(255, 0, 0, 255),        // Red
                _ => TextColor
            };
        }

        /// <summary>
        /// Gets the pixel position for the metrics overlay
        /// </summary>
        public (double X, double Y) GetOverlayPosition(double canvasWidth, double canvasHeight)
        {
            const double margin = 10;
            
            return Position switch
            {
                MetricsPosition.TopLeft => (margin, margin),
                MetricsPosition.TopRight => (canvasWidth - 200 - margin, margin),
                MetricsPosition.BottomLeft => (margin, canvasHeight - 100 - margin),
                MetricsPosition.BottomRight => (canvasWidth - 200 - margin, canvasHeight - 100 - margin),
                _ => (margin, margin)
            };
        }

        private void UpdateDataMetrics(ChartModel model)
        {
            // Count total data points across all series
            var totalPoints = 0;
            var resampledSeries = 0;
            var totalResampledPoints = 0;

            foreach (var series in model.Series)
            {
                if (series.IsEmpty) continue;

                // Get original point count
                var originalCount = GetSeriesPointCount(series);
                totalPoints += originalCount;

                // Check if series uses resampling
                if (series is Series.LineSeries lineSeries && lineSeries.EnableAutoResampling)
                {
                    var renderData = lineSeries.GetRenderData(800); // Assume 800px viewport
                    if (renderData.Count < originalCount)
                    {
                        resampledSeries++;
                        totalResampledPoints += renderData.Count;
                    }
                }
            }

            _metrics.DataPointCount = totalPoints;
            _metrics.SeriesCount = model.Series.Count;
            _metrics.IsResampled = resampledSeries > 0;
            _metrics.ResamplingRatio = totalPoints > 0 ? (double)totalResampledPoints / totalPoints : 1.0;
        }

        private static int GetSeriesPointCount(Series.SeriesBase series)
        {
            return series switch
            {
                Series.LineSeries line => line.Data.Count,
                Series.ScatterSeries scatter => scatter.Data.Count,
                Series.BarSeries bar => bar.Data.Count,
                Series.StackedBarSeries stackedBar => stackedBar.Data.Count,
                Series.OhlcSeries ohlc => ohlc.Data.Count,
                Series.ErrorBarSeries errorBar => errorBar.Data.Count,
                _ => 0
            };
        }

        private string GetDetailedMetricsText()
        {
            var status = _metrics.GetPerformanceStatus();
            var statusText = status switch
            {
                PerformanceStatus.Excellent => "EXCELLENT",
                PerformanceStatus.Good => "GOOD",
                PerformanceStatus.Fair => "FAIR",
                PerformanceStatus.Poor => "POOR",
                _ => "UNKNOWN"
            };

            return $"?? RENDER METRICS\n" +
                   $"FPS: {_metrics.CurrentFPS:F1} ({statusText})\n" +
                   $"Frame: {_metrics.LastFrameTimeMs:F1}ms (avg: {_metrics.AverageFrameTimeMs:F1}ms)\n" +
                   $"Points: {_metrics.DataPointCount:N0}" +
                   $"{(_metrics.IsResampled ? $" ? {(_metrics.DataPointCount * _metrics.ResamplingRatio):F0} (?{(1 - _metrics.ResamplingRatio):P0})" : "")}\n" +
                   $"Series: {_metrics.SeriesCount}\n" +
                   $"Memory: {_metrics.MemoryUsageFormatted}\n" +
                   $"Uptime: {_metrics.Uptime:hh\\:mm\\:ss}\n" +
                   $"Frames: {_metrics.TotalFrames:N0}\n" +
                   $"Peak Frame: {_metrics.MaxFrameTimeMs:F1}ms";
        }

        private string GetBasicMetricsText()
        {
            return $"FPS: {_metrics.CurrentFPS:F1} | {_metrics.LastFrameTimeMs:F1}ms | {_metrics.DataPointCount:N0} pts";
        }

        /// <summary>
        /// Toggles metrics visibility
        /// </summary>
        public void ToggleVisibility()
        {
            IsVisible = !IsVisible;
        }

        /// <summary>
        /// Toggles between detailed and basic metrics display
        /// </summary>
        public void ToggleDetailLevel()
        {
            ShowDetailed = !ShowDetailed;
        }

        /// <summary>
        /// Resets all performance metrics
        /// </summary>
        public void ResetMetrics()
        {
            _metrics.Reset();
        }
    }

    /// <summary>
    /// Position options for the metrics overlay
    /// </summary>
    public enum MetricsPosition
    {
        /// <summary>
        /// Top-left corner of the chart
        /// </summary>
        TopLeft,

        /// <summary>
        /// Top-right corner of the chart
        /// </summary>
        TopRight,

        /// <summary>
        /// Bottom-left corner of the chart
        /// </summary>
        BottomLeft,

        /// <summary>
        /// Bottom-right corner of the chart
        /// </summary>
        BottomRight
    }
}