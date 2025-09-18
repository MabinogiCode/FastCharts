
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
namespace FastCharts.Wpf.Contracts
{
    public class LineSeries : IReadOnlyLineSeries, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void Raise([CallerMemberName] string? p = null) => PropertyChanged?.Invoke(this, new(p));
        public double[] X { get; }
        public double[] Y { get; }
        public string Name { get; }
        public LineSeries(double[] x, double[] y, string name = "") { X = x; Y = y; Name = name; }
        Brush? _stroke; public Brush? Stroke { get => _stroke; set { _stroke = value; Raise(); } }
        double? _thickness; public double? StrokeThickness { get => _thickness; set { _thickness = value; Raise(); } }
        bool? _showPoints; public bool? ShowPoints { get => _showPoints; set { _showPoints = value; Raise(); } }
        bool? _drawLines; public bool? DrawLines { get => _drawLines; set { _drawLines = value; Raise(); } }
        double? _pointSize; public double? PointSize { get => _pointSize; set { _pointSize = value; Raise(); } }
        Brush? _pointFill; public Brush? PointFill { get => _pointFill; set { _pointFill = value; Raise(); } }
    }
}
