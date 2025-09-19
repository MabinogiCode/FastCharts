using System.Windows;
using System.Windows.Controls;
using FastCharts.Core;

namespace FastCharts.Wpf.Controls
{

    public class FastChart : Control
    {
        static FastChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(FastChart),
                new FrameworkPropertyMetadata(typeof(FastChart)));
        }

        public ChartModel? Model
        {
            get => (ChartModel?)GetValue(ModelProperty);
            set => SetValue(ModelProperty, value);
        }

        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(
                nameof(Model),
                typeof(ChartModel),
                typeof(FastChart),
                new FrameworkPropertyMetadata(default(ChartModel), FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
