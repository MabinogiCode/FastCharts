using System;
using System.Diagnostics;

namespace FastCharts.Core.Performance
{
    /// <summary>
    /// Performance metrics collector for charts (P1-METRICS)
    /// Tracks rendering performance, memory usage, and data statistics
    /// </summary>
    public sealed class RenderMetrics
    {
        private readonly Stopwatch _frameTimer = new();
        private readonly CircularBuffer<double> _frameTimes = new(60); // Last 60 frames for FPS calculation
        private long _totalFrames;
        private long _lastGcMemoryBytes;
        private DateTime _startTime = DateTime.UtcNow;

        /// <summary>
        /// Starts measuring a new frame
        /// </summary>
        public void StartFrame()
        {
            _frameTimer.Restart();
        }

        /// <summary>
        /// Completes the current frame measurement
        /// </summary>
        public void EndFrame()
        {
            _frameTimer.Stop();
            var frameTimeMs = _frameTimer.Elapsed.TotalMilliseconds;
            _frameTimes.Add(frameTimeMs);
            _totalFrames++;
            
            // Update memory stats periodically (every 30th frame to avoid overhead)
            if (_totalFrames % 30 == 0)
            {
                UpdateMemoryStats();
            }
        }

        /// <summary>
        /// Gets the current FPS based on recent frame times
        /// </summary>
        public double CurrentFPS
        {
            get
            {
                if (_frameTimes.Count == 0) return 0.0;
                
                var avgFrameTimeMs = _frameTimes.Average();
                return avgFrameTimeMs > 0 ? 1000.0 / avgFrameTimeMs : 0.0;
            }
        }

        /// <summary>
        /// Gets the last frame render time in milliseconds
        /// </summary>
        public double LastFrameTimeMs => _frameTimes.Count > 0 ? _frameTimes.Latest : 0.0;

        /// <summary>
        /// Gets the average frame time over recent frames
        /// </summary>
        public double AverageFrameTimeMs => _frameTimes.Count > 0 ? _frameTimes.Average() : 0.0;

        /// <summary>
        /// Gets the maximum frame time in recent history
        /// </summary>
        public double MaxFrameTimeMs => _frameTimes.Count > 0 ? _frameTimes.Maximum() : 0.0;

        /// <summary>
        /// Gets the total number of frames rendered
        /// </summary>
        public long TotalFrames => _totalFrames;

        /// <summary>
        /// Gets the current memory usage in bytes
        /// </summary>
        public long MemoryUsageBytes { get; private set; }

        /// <summary>
        /// Gets the memory usage in a human-readable format
        /// </summary>
        public string MemoryUsageFormatted => FormatBytes(MemoryUsageBytes);

        /// <summary>
        /// Gets the total uptime of the metrics collector
        /// </summary>
        public TimeSpan Uptime => DateTime.UtcNow - _startTime;

        /// <summary>
        /// Number of data points being rendered (set by chart)
        /// </summary>
        public int DataPointCount { get; set; }

        /// <summary>
        /// Number of series being rendered (set by chart)
        /// </summary>
        public int SeriesCount { get; set; }

        /// <summary>
        /// Whether resampling was applied in the last render (set by chart)
        /// </summary>
        public bool IsResampled { get; set; }

        /// <summary>
        /// Resampling reduction ratio (set by chart)
        /// </summary>
        public double ResamplingRatio { get; set; } = 1.0;

        /// <summary>
        /// Resets all metrics
        /// </summary>
        public void Reset()
        {
            _frameTimes.Clear();
            _totalFrames = 0;
            _startTime = DateTime.UtcNow;
            DataPointCount = 0;
            SeriesCount = 0;
            IsResampled = false;
            ResamplingRatio = 1.0;
            UpdateMemoryStats();
        }

        /// <summary>
        /// Gets a comprehensive metrics summary
        /// </summary>
        public string GetSummary()
        {
            return $"FPS: {CurrentFPS:F1} | Frame: {LastFrameTimeMs:F1}ms | Points: {DataPointCount:N0}" +
                   $"{(IsResampled ? $" (?{ResamplingRatio:P0})" : "")} | Series: {SeriesCount} | " +
                   $"Memory: {MemoryUsageFormatted} | Uptime: {Uptime:hh\\:mm\\:ss}";
        }

        /// <summary>
        /// Gets performance status indicator
        /// </summary>
        public PerformanceStatus GetPerformanceStatus()
        {
            var fps = CurrentFPS;
            var frameTime = LastFrameTimeMs;

            if (fps >= 55 && frameTime <= 18) return PerformanceStatus.Excellent;
            if (fps >= 25 && frameTime <= 40) return PerformanceStatus.Good;
            if (fps >= 15 && frameTime <= 67) return PerformanceStatus.Fair;
            return PerformanceStatus.Poor;
        }

        private void UpdateMemoryStats()
        {
            try
            {
                // Get memory usage without forcing GC to avoid performance impact
                MemoryUsageBytes = GC.GetTotalMemory(false);
            }
            catch
            {
                // Fallback if GC operations fail
                MemoryUsageBytes = _lastGcMemoryBytes;
            }
            
            _lastGcMemoryBytes = MemoryUsageBytes;
        }

        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            var doubleBytes = (double)bytes;
            var suffixIndex = 0;

            while (doubleBytes >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                doubleBytes /= 1024;
                suffixIndex++;
            }

            return $"{doubleBytes:F1} {suffixes[suffixIndex]}";
        }
    }

    /// <summary>
    /// Performance status levels
    /// </summary>
    public enum PerformanceStatus
    {
        /// <summary>
        /// Excellent performance (55+ FPS)
        /// </summary>
        Excellent,

        /// <summary>
        /// Good performance (25-54 FPS)
        /// </summary>
        Good,

        /// <summary>
        /// Fair performance (15-24 FPS)
        /// </summary>
        Fair,

        /// <summary>
        /// Poor performance (<15 FPS)
        /// </summary>
        Poor
    }

    /// <summary>
    /// Circular buffer for efficient storage of recent values
    /// </summary>
    internal class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _start;
        private int _size;
        private readonly int _capacity;

        public CircularBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            var end = (_start + _size) % _capacity;
            _buffer[end] = item;

            if (_size == _capacity)
            {
                _start = (_start + 1) % _capacity;
            }
            else
            {
                _size++;
            }
        }

        public void Clear()
        {
            _start = 0;
            _size = 0;
        }

        public int Count => _size;

        public T Latest => _size > 0 ? _buffer[(_start + _size - 1) % _capacity] : default(T)!;

        public double Average()
        {
            if (_size == 0 || typeof(T) != typeof(double)) return 0.0;

            double sum = 0;
            for (var i = 0; i < _size; i++)
            {
                var index = (_start + i) % _capacity;
                sum += Convert.ToDouble(_buffer[index]);
            }

            return sum / _size;
        }

        public double Maximum()
        {
            if (_size == 0 || typeof(T) != typeof(double)) return 0.0;

            var max = double.MinValue;
            for (var i = 0; i < _size; i++)
            {
                var index = (_start + i) % _capacity;
                var value = Convert.ToDouble(_buffer[index]);
                if (value > max) max = value;
            }

            return max;
        }
    }
}