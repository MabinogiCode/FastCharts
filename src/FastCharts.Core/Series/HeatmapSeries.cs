using System;
using System.Diagnostics.CodeAnalysis;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Helpers;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Series
{
    /// <summary>
    /// Heatmap over a uniform grid (P2-HEATMAP): each cell's value is colored through
    /// <see cref="ColorAxis"/>. <c>Values[row, column]</c> — rows run along Y from
    /// <see cref="YRange"/>.Min (row 0, bottom) upward, columns along X from <see cref="XRange"/>.Min.
    /// <see cref="XRange"/>/<see cref="YRange"/> are the outer edges of the grid, so each cell spans
    /// <c>XRange.Size / ColumnCount</c> by <c>YRange.Size / RowCount</c>. NaN cells are left empty.
    /// </summary>
    public sealed class HeatmapSeries : SeriesBase, ISeriesRangeProvider
    {
        private double[,] _values;
        private ColorAxis _colorAxis = new ColorAxis();
        private int _valueRangeVersion = -1;
        private FRange _valueRange;
        private bool _hasValues;

        /// <summary>
        /// Creates a heatmap. When ranges are omitted, cell (row, column) spans
        /// [column, column + 1] × [row, row + 1].
        /// </summary>
        /// <param name="values">Grid values, <c>[row, column]</c> with row 0 at the bottom</param>
        /// <param name="xRange">Outer X edges of the grid (default 0..ColumnCount)</param>
        /// <param name="yRange">Outer Y edges of the grid (default 0..RowCount)</param>
        [SuppressMessage("Major Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "A heatmap is a 2D grid; double[,] is the idiomatic .NET representation (as in OxyPlot and ScottPlot).")]
        public HeatmapSeries(double[,] values, FRange? xRange = null, FRange? yRange = null)
        {
            _values = values ?? throw new ArgumentNullException(nameof(values));
            XRange = ValidateRange(xRange ?? new FRange(0, Math.Max(1, values.GetLength(1))), nameof(xRange));
            YRange = ValidateRange(yRange ?? new FRange(0, Math.Max(1, values.GetLength(0))), nameof(yRange));
            StrokeThickness = 0;
        }

        /// <summary>
        /// Grid values, <c>[row, column]</c>. Edits made in place are picked up after
        /// <see cref="Invalidate"/> (lock <see cref="SeriesBase.SyncRoot"/> when editing from another thread).
        /// </summary>
        public double[,] Values => _values;

        /// <summary>
        /// Number of cells along X.
        /// </summary>
        public int ColumnCount => _values.GetLength(1);

        /// <summary>
        /// Number of cells along Y.
        /// </summary>
        public int RowCount => _values.GetLength(0);

        /// <summary>
        /// Outer X edges of the grid.
        /// </summary>
        public FRange XRange { get; private set; }

        /// <summary>
        /// Outer Y edges of the grid.
        /// </summary>
        public FRange YRange { get; private set; }

        /// <summary>
        /// Value-to-color mapping and color bar settings. Assign the same instance to several
        /// heatmaps to share one scale.
        /// </summary>
        public ColorAxis ColorAxis
        {
            get => _colorAxis;
            set => _colorAxis = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Opacity of the cells [0..1] (default 1).
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Incremented whenever the values change; renderers use it to cache the colored image.
        /// </summary>
        public int DataVersion { get; private set; }

        public override bool IsEmpty => _values.Length == 0;

        /// <summary>
        /// Replaces the grid (and optionally its extent) and notifies hosts.
        /// </summary>
        [SuppressMessage("Major Code Smell", "S2368:Public methods should not have multidimensional array parameters", Justification = "A heatmap is a 2D grid; double[,] is the idiomatic .NET representation (as in OxyPlot and ScottPlot).")]
        public void SetValues(double[,] values, FRange? xRange = null, FRange? yRange = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            var newX = ValidateRange(xRange ?? XRange, nameof(xRange));
            var newY = ValidateRange(yRange ?? YRange, nameof(yRange));
            lock (SyncRoot)
            {
                _values = values;
                XRange = newX;
                YRange = newY;
                DataVersion++;
            }

            NotifyChanged();
        }

        /// <summary>
        /// Call after editing <see cref="Values"/> in place: refreshes cached colors and redraws.
        /// </summary>
        public void Invalidate()
        {
            lock (SyncRoot)
            {
                DataVersion++;
            }

            NotifyChanged();
        }

        /// <summary>
        /// Minimum and maximum of the finite values; false when every cell is missing (NaN).
        /// Cached per <see cref="DataVersion"/>.
        /// </summary>
        public bool TryGetValueRange(out FRange range)
        {
            lock (SyncRoot)
            {
                if (_valueRangeVersion != DataVersion)
                {
                    ComputeValueRange();
                    _valueRangeVersion = DataVersion;
                }

                range = _valueRange;
                return _hasValues;
            }
        }

        /// <summary>
        /// Cell containing the data point (<paramref name="x"/>, <paramref name="y"/>), if any.
        /// </summary>
        public bool TryGetCell(double x, double y, out int row, out int column)
        {
            row = -1;
            column = -1;
            if (IsEmpty || !(XRange.Contains(x) && YRange.Contains(y)))
            {
                return false;
            }

            column = Math.Min(ColumnCount - 1, (int)((x - XRange.Min) / XRange.Size * ColumnCount));
            row = Math.Min(RowCount - 1, (int)((y - YRange.Min) / YRange.Size * RowCount));
            return true;
        }

        /// <summary>
        /// Value of the cell containing (<paramref name="x"/>, <paramref name="y"/>); false outside
        /// the grid or on a missing (NaN) cell.
        /// </summary>
        public bool TryGetValueAt(double x, double y, out double value)
        {
            lock (SyncRoot)
            {
                value = double.NaN;
                if (!TryGetCell(x, y, out var row, out var column))
                {
                    return false;
                }

                value = _values[row, column];
                return ValidationHelper.IsFinite(value);
            }
        }

        bool ISeriesRangeProvider.TryGetRanges(out FRange xRange, out FRange yRange)
        {
            xRange = XRange;
            yRange = YRange;
            return !IsEmpty;
        }

        private void ComputeValueRange()
        {
            var min = double.PositiveInfinity;
            var max = double.NegativeInfinity;
            foreach (var v in _values)
            {
                if (!ValidationHelper.IsFinite(v))
                {
                    continue;
                }

                if (v < min)
                {
                    min = v;
                }

                if (v > max)
                {
                    max = v;
                }
            }

            _hasValues = min <= max;
            _valueRange = _hasValues ? new FRange(min, max) : new FRange(0, 1);
        }

        private static FRange ValidateRange(FRange range, string paramName)
        {
            if (!ValidationHelper.IsValidRange(range))
            {
                throw new ArgumentException($"Grid extent must be finite with Min < Max (got {range})", paramName);
            }

            return range;
        }
    }
}
