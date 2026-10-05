using System;
using FastCharts.Core.Abstractions;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interactivity;

/// <summary>
/// Viewport backed by the chart axes: the axes' <c>VisibleRange</c> is the single source of truth.
/// Reading returns the current axis ranges; zoom/pan/set write them directly, so a change made
/// through the viewport, the axes, <see cref="ChartModel.ZoomAt"/> or a linked chart is never
/// overwritten at the next render. The secondary Y axis (when present) follows primary Y changes
/// proportionally, keeping its own scale.
/// </summary>
internal sealed class AxisViewport : IViewport
{
    private readonly Func<IAxis<double>> _xAxis;
    private readonly Func<IAxis<double>> _yAxis;
    private readonly Func<IAxis<double>?> _yAxisSecondary;

    public AxisViewport(Func<IAxis<double>> xAxis, Func<IAxis<double>> yAxis, Func<IAxis<double>?> yAxisSecondary)
    {
        _xAxis = xAxis ?? throw new ArgumentNullException(nameof(xAxis));
        _yAxis = yAxis ?? throw new ArgumentNullException(nameof(yAxis));
        _yAxisSecondary = yAxisSecondary ?? throw new ArgumentNullException(nameof(yAxisSecondary));
    }

    public FRange X => _xAxis().VisibleRange;

    public FRange Y => _yAxis().VisibleRange;

    public void SetVisible(FRange x, FRange y)
    {
        var yAxis = _yAxis();
        var oldY = yAxis.VisibleRange;

        _xAxis().VisibleRange = x;
        yAxis.VisibleRange = y;

        var secondary = _yAxisSecondary();
        if (secondary != null)
        {
            AxisCoordinates.FollowRelative(yAxis, oldY, y, secondary);
        }
    }

    /// <summary>
    /// Zooms around a data-space pivot. <paramref name="scaleX"/>/<paramref name="scaleY"/> greater than 1
    /// zoom in. The zoom is computed in normalized axis space, so logarithmic axes zoom uniformly.
    /// </summary>
    public void Zoom(double scaleX, double scaleY, PointD pivotData)
    {
        var xAxis = _xAxis();
        var yAxis = _yAxis();
        SetVisible(ZoomRange(xAxis, scaleX, pivotData.X), ZoomRange(yAxis, scaleY, pivotData.Y));
    }

    /// <summary>
    /// Shifts both visible ranges by the given data-space deltas.
    /// </summary>
    public void Pan(double dxData, double dyData)
    {
        var x = X;
        var y = Y;
        SetVisible(
            new FRange(x.Min + dxData, x.Max + dxData),
            new FRange(y.Min + dyData, y.Max + dyData));
    }

    private static FRange ZoomRange(IAxis<double> axis, double scale, double pivot)
    {
        var range = axis.VisibleRange;
        var p = AxisCoordinates.ToNormalized(axis, range, pivot);
        return AxisCoordinates.SubRange(axis, range, p - (p / scale), p + ((1 - p) / scale));
    }
}
