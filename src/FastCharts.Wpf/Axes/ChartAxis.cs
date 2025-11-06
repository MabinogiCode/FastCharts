using System.Windows;
using System.Windows.Media;
namespace FastCharts.Wpf.Axes
{
    public sealed class ChartAxis : DependencyObject
    {
        public static ChartAxis CreateDefault(AxisPosition pos)
        {
            return new ChartAxis
            {
                Position = pos,
                IsVisible = true,
                ShowTicks = true,
                ShowLabels = true,
                ShowGrid = false,
                Thickness = 1.0,
                Stroke = Brushes.Gray,
                TickLength = 4.0,
                TickThickness = 1.0,
                TickStroke = Brushes.Gray,
                LabelBrush = Brushes.Gray,
                LabelFontSize = 12.0,
                LabelFormat = "G",
                DesiredPixelStep = pos is AxisPosition.Bottom or AxisPosition.Top ? 80.0 : 50.0,
                MajorStep = 0.0,
                GridStroke = Brushes.Gainsboro,
                GridThickness = 0.5,
                LabelRotation = 0.0
            };
        }
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(nameof(Position), typeof(AxisPosition), typeof(ChartAxis), new FrameworkPropertyMetadata(AxisPosition.Bottom, FrameworkPropertyMetadataOptions.AffectsRender));
        public AxisPosition Position { get => (AxisPosition)GetValue(PositionProperty); set => SetValue(PositionProperty, value); }
        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(nameof(IsVisible), typeof(bool), typeof(ChartAxis), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool IsVisible { get => (bool)GetValue(IsVisibleProperty); set => SetValue(IsVisibleProperty, value); }
        public static readonly DependencyProperty ShowTicksProperty = DependencyProperty.Register(nameof(ShowTicks), typeof(bool), typeof(ChartAxis), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool ShowTicks { get => (bool)GetValue(ShowTicksProperty); set => SetValue(ShowTicksProperty, value); }
        public static readonly DependencyProperty ShowLabelsProperty = DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(ChartAxis), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool ShowLabels { get => (bool)GetValue(ShowLabelsProperty); set => SetValue(ShowLabelsProperty, value); }
        public static readonly DependencyProperty LabelFormatProperty = DependencyProperty.Register(nameof(LabelFormat), typeof(string), typeof(ChartAxis), new FrameworkPropertyMetadata("G", FrameworkPropertyMetadataOptions.AffectsRender));
        public string LabelFormat { get => (string)GetValue(LabelFormatProperty); set => SetValue(LabelFormatProperty, value); }
        public static readonly DependencyProperty LabelBrushProperty = DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(ChartAxis), new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush LabelBrush { get => (Brush)GetValue(LabelBrushProperty); set => SetValue(LabelBrushProperty, value); }
        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double LabelFontSize { get => (double)GetValue(LabelFontSizeProperty); set => SetValue(LabelFontSizeProperty, value); }
        public static readonly DependencyProperty LabelRotationProperty = DependencyProperty.Register(nameof(LabelRotation), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double LabelRotation { get => (double)GetValue(LabelRotationProperty); set => SetValue(LabelRotationProperty, value); }
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(ChartAxis), new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Stroke { get => (Brush)GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Thickness { get => (double)GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
        public static readonly DependencyProperty TickLengthProperty = DependencyProperty.Register(nameof(TickLength), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double TickLength { get => (double)GetValue(TickLengthProperty); set => SetValue(TickLengthProperty, value); }
        public static readonly DependencyProperty TickStrokeProperty = DependencyProperty.Register(nameof(TickStroke), typeof(Brush), typeof(ChartAxis), new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush TickStroke { get => (Brush)GetValue(TickStrokeProperty); set => SetValue(TickStrokeProperty, value); }
        public static readonly DependencyProperty TickThicknessProperty = DependencyProperty.Register(nameof(TickThickness), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double TickThickness { get => (double)GetValue(TickThicknessProperty); set => SetValue(TickThicknessProperty, value); }
        public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(ChartAxis), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        public bool ShowGrid { get => (bool)GetValue(ShowGridProperty); set => SetValue(ShowGridProperty, value); }
        public static readonly DependencyProperty GridStrokeProperty = DependencyProperty.Register(nameof(GridStroke), typeof(Brush), typeof(ChartAxis), new FrameworkPropertyMetadata(Brushes.Gainsboro, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush GridStroke { get => (Brush)GetValue(GridStrokeProperty); set => SetValue(GridStrokeProperty, value); }
        public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.Register(nameof(GridThickness), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.AffectsRender));
        public double GridThickness { get => (double)GetValue(GridThicknessProperty); set => SetValue(GridThicknessProperty, value); }
        public static readonly DependencyProperty DesiredPixelStepProperty = DependencyProperty.Register(nameof(DesiredPixelStep), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(80.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double DesiredPixelStep { get => (double)GetValue(DesiredPixelStepProperty); set => SetValue(DesiredPixelStepProperty, value); }
        public static readonly DependencyProperty MajorStepProperty = DependencyProperty.Register(nameof(MajorStep), typeof(double), typeof(ChartAxis), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        public double MajorStep { get => (double)GetValue(MajorStepProperty); set => SetValue(MajorStepProperty, value); }
    }
}
