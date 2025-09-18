
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FastCharts.Wpf.Axes;
using FastCharts.Wpf.Contracts;
using FastCharts.Wpf.Downsampling;

namespace FastCharts.Wpf.Controls
{
    public class FastLineChart : Control
    {
        static FastLineChart(){ DefaultStyleKeyProperty.OverrideMetadata(typeof(FastLineChart), new FrameworkPropertyMetadata(typeof(FastLineChart))); }
        public FastLineChart()
        {
            _redrawRequests.Throttle(TimeSpan.FromMilliseconds(16)).ObserveOnDispatcher().Subscribe(_ => InvalidateVisual());
            if (XAxis == null) XAxis = ChartAxis.CreateDefault(AxisPosition.Bottom);
            if (YAxis == null) YAxis = ChartAxis.CreateDefault(AxisPosition.Left);
        }

        public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(nameof(Series), typeof(IEnumerable<IReadOnlyLineSeries>), typeof(FastLineChart), new FrameworkPropertyMetadata(null, OnSeriesChanged));
        public IEnumerable<IReadOnlyLineSeries> Series { get => (IEnumerable<IReadOnlyLineSeries>)GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(FastLineChart), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Stroke { get => (Brush)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(FastLineChart), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double StrokeThickness { get => (double)GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }

        public static readonly DependencyProperty ShowPointsProperty = DependencyProperty.Register(nameof(ShowPoints), typeof(bool), typeof(FastLineChart), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool ShowPoints { get => (bool)GetValue(ShowPointsProperty); set => SetValue(ShowPointsProperty, value); }

        public static readonly DependencyProperty DrawLinesProperty = DependencyProperty.Register(nameof(DrawLines), typeof(bool), typeof(FastLineChart), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool DrawLines { get => (bool)GetValue(DrawLinesProperty); set => SetValue(DrawLinesProperty, value); }

        public static readonly DependencyProperty PlotPaddingProperty = DependencyProperty.Register(nameof(PlotPadding), typeof(Thickness), typeof(FastLineChart), new FrameworkPropertyMetadata(new Thickness(8), FrameworkPropertyMetadataOptions.AffectsRender));
        public Thickness PlotPadding { get => (Thickness)GetValue(PlotPaddingProperty); set => SetValue(PlotPaddingProperty, value); }

        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(nameof(XAxis), typeof(ChartAxis), typeof(FastLineChart), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public ChartAxis XAxis { get => (ChartAxis)GetValue(XAxisProperty); set => SetValue(XAxisProperty, value); }

        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(nameof(YAxis), typeof(ChartAxis), typeof(FastLineChart), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public ChartAxis YAxis { get => (ChartAxis)GetValue(YAxisProperty); set => SetValue(YAxisProperty, value); }

        public static readonly DependencyProperty EnableDownsamplingProperty = DependencyProperty.Register(nameof(EnableDownsampling), typeof(bool), typeof(FastLineChart), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool EnableDownsampling { get => (bool)GetValue(EnableDownsamplingProperty); set => SetValue(EnableDownsamplingProperty, value); }

        private static void OnSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chart = (FastLineChart)d;
            chart.DetachSeriesNotifications(e.OldValue as IEnumerable);
            chart.AttachSeriesNotifications(e.NewValue as IEnumerable);
            chart._redrawRequests.OnNext(Unit.Default);
        }

        private readonly Subject<Unit> _redrawRequests = new Subject<Unit>();
        private readonly List<INotifyPropertyChanged> _seriesSubscriptions = new();
        private INotifyCollectionChanged? _seriesCollectionNotifier;

        private void AttachSeriesNotifications(IEnumerable? seriesEnum)
        {
            if (seriesEnum is null) return;
            if (seriesEnum is INotifyCollectionChanged incc) { _seriesCollectionNotifier = incc; incc.CollectionChanged += OnSeriesCollectionChanged; }
            foreach (var s in seriesEnum.OfType<INotifyPropertyChanged>()) { s.PropertyChanged += OnSeriesPropertyChanged; _seriesSubscriptions.Add(s); }
        }
        private void DetachSeriesNotifications(IEnumerable? seriesEnum)
        {
            if (_seriesCollectionNotifier != null) { _seriesCollectionNotifier.CollectionChanged -= OnSeriesCollectionChanged; _seriesCollectionNotifier = null; }
            foreach (var s in _seriesSubscriptions) s.PropertyChanged -= OnSeriesPropertyChanged; _seriesSubscriptions.Clear();
        }
        private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null) foreach (var s in e.OldItems.OfType<INotifyPropertyChanged>()) { s.PropertyChanged -= OnSeriesPropertyChanged; _seriesSubscriptions.Remove(s); }
            if (e.NewItems != null) foreach (var s in e.NewItems.OfType<INotifyPropertyChanged>()) { s.PropertyChanged += OnSeriesPropertyChanged; _seriesSubscriptions.Add(s); }
            _redrawRequests.OnNext(Unit.Default);
        }
        private void OnSeriesPropertyChanged(object? sender, PropertyChangedEventArgs e) => _redrawRequests.OnNext(Unit.Default);

        private static double NiceStep(double range, double targetCount){ if (range<=0||targetCount<=0) return 1; double rough=range/targetCount; double exp=Math.Floor(Math.Log10(rough)); double frac=rough/Math.Pow(10,exp); double nice = frac<1.5?1: frac<3?2: frac<7?5:10; return nice*Math.Pow(10,exp); }
        private static double FirstTick(double min, double step){ if (step<=0) return min; double k=Math.Ceiling(min/step); return k*step; }
        private static string FormatLabel(double v, string format){ try{ return v.ToString(format, CultureInfo.CurrentCulture);} catch{ return v.ToString("G", CultureInfo.CurrentCulture);} }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            var items = Series?.OfType<FastCharts.Wpf.Contracts.LineSeries>().ToList();
            if (items==null || items.Count==0) return;
            double W=ActualWidth,H=ActualHeight; if (W<=0||H<=0) return;

            double xmin=double.PositiveInfinity,xmax=double.NegativeInfinity,ymin=double.PositiveInfinity,ymax=double.NegativeInfinity;
            foreach (var s in items)
            {
                if (s.X.Length<2) continue;
                if (s.X[0]<xmin) xmin=s.X[0];
                if (s.X[s.X.Length-1]>xmax) xmax=s.X[s.X.Length-1];
                for (int i=0;i<s.Y.Length;i++){ double yv=s.Y[i]; if (yv<ymin) ymin=yv; if (yv>ymax) ymax=yv; }
            }
            if (double.IsInfinity(xmin) || double.IsInfinity(ymin)) return;
            if (xmax<=xmin) xmax=xmin+1e-9; if (ymax<=ymin) ymax=ymin+1e-9;

            double left=(YAxis!=null && YAxis.IsVisible)?48:8;
            double bottom=(XAxis!=null && XAxis.IsVisible)?28:8;
            double right=8, top=8;
            var pad=PlotPadding; left+=pad.Left; right+=pad.Right; top+=pad.Top; bottom+=pad.Bottom;

            var plot=new Rect(left, top, Math.Max(0,W-left-right), Math.Max(0,H-top-bottom)); if (plot.Width<=0||plot.Height<=0) return;

            double xRange=xmax-xmin, yRange=ymax-ymin;
            double xStep=(XAxis!=null && XAxis.MajorStep>0)? XAxis.MajorStep : NiceStep(xRange, plot.Width/(XAxis?.DesiredPixelStep ?? 80.0));
            double yStep=(YAxis!=null && YAxis.MajorStep>0)? YAxis.MajorStep : NiceStep(yRange, plot.Height/(YAxis?.DesiredPixelStep ?? 50.0));
            double xFirst=FirstTick(xmin,xStep);
            double yFirst=FirstTick(ymin,yStep);

            DrawAxesAndGrid(dc, plot, xmin,xmax,ymin,ymax, xFirst,xStep, yFirst,yStep);

            var downsampler=new MinMaxPerPixelDownsampler();
            int pixelWidth=(int)Math.Max(1, plot.Width);

            foreach (var s in items)
            {
                if (s.X.Length<2) continue;
                var stroke=s.Stroke ?? Stroke;
                var thickness=s.StrokeThickness ?? StrokeThickness;
                var showPoints=s.ShowPoints ?? ShowPoints;
                var drawLines=s.DrawLines ?? DrawLines;
                var pointSize=s.PointSize ?? 2.5;
                var pointFill=s.PointFill ?? stroke;
                var pen=new Pen(stroke, thickness); if (pen.CanFreeze) pen.Freeze();

                System.Collections.Generic.IReadOnlyList<int> indices;
                if (EnableDownsampling) indices=downsampler.Downsample(s.X,s.Y,pixelWidth,2);
                else { var tmp=new int[s.X.Length]; for (int i=0;i<tmp.Length;i++) tmp[i]=i; indices=tmp; }

                if (drawLines && indices.Count>1)
                {
                    var geo=new StreamGeometry();
                    using (var ctx = geo.Open())
                    {
                        int i0=indices[0];
                        double x0=plot.X + (s.X[i0]-xmin)/xRange*plot.Width;
                        double y0=plot.Y + plot.Height - (s.Y[i0]-ymin)/yRange*plot.Height;
                        ctx.BeginFigure(new Point(x0,y0), false, false);
                        for (int k=1;k<indices.Count;k++)
                        {
                            int i=indices[k];
                            double x=plot.X+(s.X[i]-xmin)/xRange*plot.Width;
                            double y=plot.Y+plot.Height-(s.Y[i]-ymin)/yRange*plot.Height;
                            ctx.LineTo(new Point(x,y), true, false);
                        }
                    }
                    if (geo.CanFreeze) geo.Freeze();
                    dc.DrawGeometry(null, pen, geo);
                }

                if (showPoints)
                {
                    Brush pf=pointFill; if (pf is Freezable fb && fb.CanFreeze) fb.Freeze();
                    for (int k=0;k<indices.Count;k++)
                    {
                        int i=indices[k];
                        double x=plot.X+(s.X[i]-xmin)/xRange*plot.Width;
                        double y=plot.Y+plot.Height-(s.Y[i]-ymin)/yRange*plot.Height;
                        dc.DrawEllipse(pf, null, new Point(x,y), pointSize, pointSize);
                    }
                }
            }
        }

        private void DrawAxesAndGrid(DrawingContext dc, Rect plot, double xmin,double xmax,double ymin,double ymax, double xFirst,double xStep,double yFirst,double yStep)
        {
            var xa=XAxis; var ya=YAxis;
            if (ya!=null && ya.IsVisible){ var p=new Pen(ya.Stroke, ya.Thickness); if (p.CanFreeze) p.Freeze(); dc.DrawLine(p, new Point(plot.Left,plot.Top), new Point(plot.Left,plot.Bottom)); }
            if (xa!=null && xa.IsVisible){ var p=new Pen(xa.Stroke, xa.Thickness); if (p.CanFreeze) p.Freeze(); dc.DrawLine(p, new Point(plot.Left,plot.Bottom), new Point(plot.Right,plot.Bottom)); }

            if (ya!=null && ya.IsVisible)
            {
                var tickPen=new Pen(ya.TickStroke ?? ya.Stroke, ya.TickThickness); if (tickPen.CanFreeze) tickPen.Freeze();
                var gridPen=new Pen(ya.GridStroke, ya.GridThickness); if (gridPen.CanFreeze) gridPen.Freeze();
                for(double v=yFirst; v<=ymax+1e-12; v+=yStep)
                {
                    double y=plot.Y+plot.Height-(v-ymin)/(ymax-ymin)*plot.Height;
                    if (ya.ShowGrid) dc.DrawLine(gridPen, new Point(plot.Left,y), new Point(plot.Right,y));
                    if (ya.ShowTicks) dc.DrawLine(tickPen, new Point(plot.Left-ya.TickLength,y), new Point(plot.Left,y));
                    if (ya.ShowLabels)
                    {
                        string text=FormatLabel(v, ya.LabelFormat);
                        DrawLabel(dc, text, new Point(plot.Left-ya.TickLength-4, y), ya.LabelBrush, ya.LabelFontSize, HorizontalAlignment.Right, VerticalAlignment.Center, ya.LabelRotation);
                    }
                }
            }

            if (xa!=null && xa.IsVisible)
            {
                var tickPen=new Pen(xa.TickStroke ?? xa.Stroke, xa.TickThickness); if (tickPen.CanFreeze) tickPen.Freeze();
                var gridPen=new Pen(xa.GridStroke, xa.GridThickness); if (gridPen.CanFreeze) gridPen.Freeze();
                for(double v=xFirst; v<=xmax+1e-12; v+=xStep)
                {
                    double x=plot.X+(v-xmin)/(xmax-xmin)*plot.Width;
                    if (xa.ShowGrid) dc.DrawLine(gridPen, new Point(x,plot.Top), new Point(x,plot.Bottom));
                    if (xa.ShowTicks) dc.DrawLine(tickPen, new Point(x,plot.Bottom), new Point(x,plot.Bottom+xa.TickLength));
                    if (xa.ShowLabels)
                    {
                        string text=FormatLabel(v, xa.LabelFormat);
                        DrawLabel(dc, text, new Point(x, plot.Bottom+xa.TickLength+2), xa.LabelBrush, xa.LabelFontSize, HorizontalAlignment.Center, VerticalAlignment.Top, xa.LabelRotation);
                    }
                }
            }
        }

        private void DrawLabel(DrawingContext dc, string text, Point anchor, Brush brush, double fontSize, HorizontalAlignment hAlign, VerticalAlignment vAlign, double rotation)
        {
            var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
#if NET48
            var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, brush);
#else
            double ppd = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, brush, ppd);
#endif
            Size sz=new Size(ft.Width, ft.Height);
            double x=anchor.X, y=anchor.Y;
            if (hAlign==HorizontalAlignment.Right) x-=sz.Width; else if (hAlign==HorizontalAlignment.Center) x-=sz.Width*0.5;
            if (vAlign==VerticalAlignment.Bottom) y-=sz.Height; else if (vAlign==VerticalAlignment.Center) y-=sz.Height*0.5;
            var origin=new Point(x,y);
            if (Math.Abs(rotation)>0.01){ dc.PushTransform(new RotateTransform(rotation, anchor.X, anchor.Y)); dc.DrawText(ft, origin); dc.Pop(); }
            else dc.DrawText(ft, origin);
        }
    }
    internal struct Unit { public static readonly Unit Default = new Unit(); }
}
