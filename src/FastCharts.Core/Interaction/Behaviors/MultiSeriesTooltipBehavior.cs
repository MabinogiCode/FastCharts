using System;
using System.Globalization;
using FastCharts.Core.Series;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class MultiSeriesTooltipBehavior : IBehavior
{
    public double XSnapToleranceFraction { get; set; } = 0.01;
    public int MaxSeries { get; set; } = 32;
    public string LineFormat { get; set; } = "{0}: {1}";
    public string HeaderFormat { get; set; } = "X = {0}";
    public string NumberFormatY { get; set; } = "G";
    public string NumberFormatX { get; set; } = "G";

    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        if (model == null)
        {
            return false;
        }
        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;

        if (ev.Type == PointerEventType.Down && ev.Button == PointerButton.Left)
        {
            st.TooltipLocked = !st.TooltipLocked;
            if (!st.TooltipLocked)
            {
                st.TooltipAnchorX = null;
            }
            return true;
        }
        else if (ev.Type == PointerEventType.Leave)
        {
            if (!st.TooltipLocked)
            {
                st.TooltipText = null;
                st.TooltipSeries.Clear();
            }
            return false;
        }
        else if (ev.Type == PointerEventType.Move)
        {
            if (st.TooltipLocked || !st.DataX.HasValue)
            {
                return false;
            }
            Build(model, st, st.DataX.Value);
            return true;
        }
        // Ignore Up / Wheel / others
        return false;
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
            switch (s)
            {
                case AreaSeries area:
                {
                    AddXY(st, area.Data, x, tol, area.Title, area.PaletteIndex);
                    break;
                }
                case StepLineSeries step:
                {
                    AddXY(st, step.Data, x, tol, step.Title, step.PaletteIndex);
                    break;
                }
                case LineSeries ls:
                {
                    AddXY(st, ls.Data, x, tol, ls.Title, ls.PaletteIndex);
                    break;
                }
                case ScatterSeries sc:
                {
                    AddXY(st, sc.Data, x, tol, sc.Title, sc.PaletteIndex);
                    break;
                }
                case BarSeries bar:
                {
                    foreach (var p in bar.Data)
                    {
                        var halfW = bar.GetWidthFor(0) * 0.5;
                        if (Math.Abs(p.X - x) <= halfW)
                        {
                            AddPoint(bar.Title ?? "Bar", p.X, p.Y, bar.PaletteIndex);
                        }
                    }
                    break;
                }
                case StackedBarSeries sbar:
                {
                    foreach (var p in sbar.Data)
                    {
                        var halfW = sbar.GetWidthFor(0) * 0.5;
                        if (Math.Abs(p.X - x) <= halfW && p.Values != null)
                        {
                            var sum = 0d;
                            for (var i = 0; i < p.Values.Length; i++)
                            {
                                sum += p.Values[i];
                            }
                            AddPoint(sbar.Title ?? "Stack", p.X, sum, sbar.PaletteIndex);
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
                            AddPoint(ohlc.Title ?? "OHLC", p.X, p.Close, ohlc.PaletteIndex);
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
                            AddPoint(err.Title ?? "Err", p.X, p.Y, err.PaletteIndex);
                        }
                    }
                    break;
                }
                default:
                {
                    break;
                }
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

    private static void AddXY(InteractionState st, System.Collections.Generic.IList<FastCharts.Core.Primitives.PointD> data, double x, double tol, string? title, int? paletteIndex)
    {
        if (data == null || data.Count == 0)
        {
            return;
        }
        var lo = 0;
        var hi = data.Count - 1;
        while (lo < hi)
        {
            var mid = (lo + hi) >> 1;
            if (data[mid].X < x)
            {
                lo = mid + 1;
            }
            else
            {
                hi = mid;
            }
        }
        var idx = lo;
        for (var d = -1; d <= 1; d++)
        {
            var i = idx + d;
            if (i < 0 || i >= data.Count)
            {
                continue;
            }
            var p = data[i];
            if (Math.Abs(p.X - x) <= tol)
            {
                st.TooltipSeries.Add(new TooltipSeriesValue { Title = title ?? "Line", X = p.X, Y = p.Y, PaletteIndex = paletteIndex });
                if (st.TooltipSeries.Count >= st.TooltipSeries.Capacity)
                {
                    return;
                }
            }
        }
    }
}
