#pragma warning disable S3267
using FastCharts.Core.Series;
using System;
using System.Globalization;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace FastCharts.Core.Interaction.Behaviors;

[SuppressMessage("Performance", "S3267", Justification = "Algorithmic loops replaced by LINQ per analyzer rule.")]
public sealed class MultiSeriesTooltipBehavior : IBehavior
{
    public double XSnapToleranceFraction { get; set; } = 0.01;
    public int MaxSeries { get; set; } = 32;
    public string LineFormat { get; set; } = "{0}: {1}";
    public string HeaderFormat { get; set; } = "X = {0}";
    public string NumberFormatY { get; set; } = "G";
    public string NumberFormatX { get; set; } = "G";

    /// <summary>
    /// Maximum pointer travel (in pixels) between press and release for a left click to toggle
    /// the tooltip lock. Longer travels are drags (pan) and leave the lock untouched.
    /// </summary>
    public double ClickTolerance { get; set; } = 4;

    private bool _clickPending;
    private double _pressX;
    private double _pressY;

    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;
        if (ev.Type == PointerEventType.Down && ev.Button == PointerButton.Left)
        {
            // The lock toggles on release, once we know the press was a click and not a pan
            // drag (left drag) or a zoom rectangle (Shift + left drag).
            _clickPending = !ev.Modifiers.Shift;
            _pressX = ev.PixelX;
            _pressY = ev.PixelY;
            return false;
        }
        else if (ev.Type == PointerEventType.Up && ev.Button == PointerButton.Left)
        {
            var isClick = _clickPending && !MovedBeyondTolerance(ev);
            _clickPending = false;
            if (!isClick)
            {
                return false;
            }

            st.TooltipLocked = !st.TooltipLocked;
            if (!st.TooltipLocked)
            {
                st.TooltipAnchorX = null;
            }
            return true;
        }
        else if (ev.Type == PointerEventType.Leave)
        {
            _clickPending = false;
            if (!st.TooltipLocked)
            {
                st.TooltipText = null;
                st.TooltipSeries.Clear();
            }
            return false;
        }
        else if (ev.Type == PointerEventType.Move)
        {
            if (_clickPending && MovedBeyondTolerance(ev))
            {
                _clickPending = false; // became a drag
            }
            if (st.TooltipLocked || !st.DataX.HasValue)
            {
                return false;
            }
            Build(model, st, st.DataX.Value);
            return true;
        }
        return false;
    }

    private bool MovedBeyondTolerance(InteractionEvent ev)
    {
        var dx = ev.PixelX - _pressX;
        var dy = ev.PixelY - _pressY;
        return (dx * dx) + (dy * dy) > ClickTolerance * ClickTolerance;
    }

    private void Build(ChartModel model, InteractionState st, double x)
    {
        var xr = model.XAxis.VisibleRange;
        var tol = xr.Size * XSnapToleranceFraction;
        var ci = CultureInfo.InvariantCulture;
        st.TooltipSeries.Clear();
        if (st.TooltipSeries.Capacity < MaxSeries)
        {
            st.TooltipSeries.Capacity = MaxSeries;
        }

        void AddPoint(string? title, double px, double py, int? paletteIndex)
        {
            if (st.TooltipSeries.Count < MaxSeries)
            {
                st.TooltipSeries.Add(new TooltipSeriesValue { Title = title ?? "?", X = px, Y = py, PaletteIndex = paletteIndex });
            }
        }

        foreach (var s in model.Series)
        {
            if (st.TooltipSeries.Count >= MaxSeries)
            {
                break;
            }
            if (!s.IsVisible || s.IsEmpty)
            {
                continue;
            }

            // Series may be fed from another thread: read them under their lock
            lock (s.SyncRoot)
            {
                AddSeriesValues(st, s, x, tol, AddPoint);
            }
        }

        if (st.TooltipSeries.Count == 0)
        {
            st.TooltipText = null;
            return;
        }

        var anchorX = st.TooltipSeries[0].X;
        st.TooltipAnchorX = anchorX;
        var header = string.Format(ci, HeaderFormat, anchorX.ToString(NumberFormatX, ci));
        var sb = new System.Text.StringBuilder(header.Length + (st.TooltipSeries.Count * 24));
        sb.Append(header);
        for (var i = 0; i < st.TooltipSeries.Count; i++)
        {
            var v = st.TooltipSeries[i];
            var val = v.Y.ToString(NumberFormatY, ci);
            sb.Append('\n');
            sb.AppendFormat(ci, LineFormat, v.Title ?? "?", val);
        }
        st.TooltipText = sb.ToString();
    }

    private void AddSeriesValues(InteractionState st, SeriesBase s, double x, double tol, Action<string?, double, double, int?> addPoint)
    {
        switch (s)
        {
            case LineSeries ls:
                {
                    // Line, area, step and streaming series: binary search on X-sorted data
                    var start = 0;
                    var end = ls.Data.Count;
                    if (ls.TryGetIndexRange(x - tol, x + tol, out var first, out var last))
                    {
                        start = first;
                        end = last;
                    }
                    AddClosest(st, ls.Data, start, end, x, tol, ls.Title, ls.PaletteIndex);
                    break;
                }
            case ScatterSeries sc:
                { AddClosest(st, sc.Data, 0, sc.Data.Count, x, tol, sc.Title, sc.PaletteIndex); break; }
            case BarSeries bar:
                {
                    var halfW = bar.GetWidthFor(0) * 0.5;
                    foreach (var p in bar.Data)
                    {
                        if (Math.Abs(p.X - x) <= halfW)
                        {
                            addPoint(bar.Title ?? "Bar", p.X, p.Y, bar.PaletteIndex);
                        }
                    }
                    break;
                }
            case StackedBarSeries sbar:
                {
                    var halfW = sbar.GetWidthFor(0) * 0.5;
                    foreach (var p in sbar.Data)
                    {
                        if (Math.Abs(p.X - x) <= halfW && p.Values != null)
                        {
                            addPoint(sbar.Title ?? "Stack", p.X, p.Values.Sum(), sbar.PaletteIndex);
                        }
                    }
                    break;
                }
            case OhlcSeries ohlc:
                {
                    foreach (var p in ohlc.Data)
                    {
                        if (Math.Abs(p.X - x) <= tol)
                        {
                            addPoint(ohlc.Title ?? "OHLC", p.X, p.Close, ohlc.PaletteIndex);
                        }
                    }
                    break;
                }
            case ErrorBarSeries err:
                {
                    foreach (var p in err.Data)
                    {
                        if (Math.Abs(p.X - x) <= tol)
                        {
                            addPoint(err.Title ?? "Err", p.X, p.Y, err.PaletteIndex);
                        }
                    }
                    break;
                }
            default:
                { break; }
        }
    }

    /// <summary>
    /// Adds up to the 3 points of data[start..end) closest to <paramref name="x"/> (within
    /// <paramref name="tol"/>), nearest first — single pass, no sorting or allocation.
    /// </summary>
    private void AddClosest(InteractionState st, System.Collections.Generic.IList<FastCharts.Core.Primitives.PointD> data, int start, int end, double x, double tol, string? title, int? paletteIndex)
    {
        const int MaxPerSeries = 3;
        var bestIndex = new int[MaxPerSeries];
        var bestDistance = new double[MaxPerSeries];
        var found = 0;

        for (var i = start; i < end; i++)
        {
            var d = Math.Abs(data[i].X - x);
            if (d > tol || double.IsNaN(d))
            {
                continue;
            }

            // Insertion into the small sorted top-k (stable: earlier points win ties)
            var slot = found;
            while (slot > 0 && d < bestDistance[slot - 1])
            {
                slot--;
            }
            if (slot >= MaxPerSeries)
            {
                continue;
            }
            for (var k = Math.Min(found, MaxPerSeries - 1); k > slot; k--)
            {
                bestIndex[k] = bestIndex[k - 1];
                bestDistance[k] = bestDistance[k - 1];
            }
            bestIndex[slot] = i;
            bestDistance[slot] = d;
            if (found < MaxPerSeries)
            {
                found++;
            }
        }

        for (var k = 0; k < found && st.TooltipSeries.Count < MaxSeries; k++)
        {
            var p = data[bestIndex[k]];
            st.TooltipSeries.Add(new TooltipSeriesValue { Title = title ?? "Line", X = p.X, Y = p.Y, PaletteIndex = paletteIndex });
        }
    }
}
#pragma warning restore S3267
