# Getting Started with FastCharts

Welcome to FastCharts! This guide will help you get up and running with high-performance charting in your .NET applications.

## Installation

### For WPF Applications (Most Common)

Install the WPF package which includes everything you need:

```bash
dotnet add package FastCharts.Wpf
```

### For Cross-Platform Applications

Install the core packages for console apps, web services, or non-WPF scenarios (rendering to PNG/SVG):

```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## Your First Chart

### 1. Basic WPF Chart

Create a simple line chart in your WPF application:

**MainWindow.xaml:**
```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf"
        Title="My First FastChart" Height="450" Width="800">
    <Grid>
        <fc:FastChart Model="{Binding ChartModel}" />
    </Grid>
</Window>
```

**MainWindow.xaml.cs:**
```csharp
using System.Windows;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace MyApp
{
    public partial class MainWindow : Window
    {
        public ChartModel ChartModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            // Create chart model
            ChartModel = new ChartModel();

            // Add sample data
            var data = new[]
            {
                new PointD(0, 10),
                new PointD(1, 25),
                new PointD(2, 15),
                new PointD(3, 30),
                new PointD(4, 20)
            };

            // Create and add series (colors come from the theme palette)
            var series = new LineSeries(data)
            {
                Title = "Sample Data",
                StrokeThickness = 2
            };

            ChartModel.AddSeries(series);

            // Set data context for binding
            DataContext = this;
        }
    }
}
```

Quick plots need even less code:

```csharp
// Any Dictionary<double, double> (or Y values only) becomes a sorted line
ChartModel.AddSeries(new Dictionary<double, double> { [0] = 10, [1] = 25, [2] = 15 }, "Measures");
ChartModel.AddSeries(new[] { 10.0, 25.0, 15.0 }, "Values"); // X = index
```

### 2. Multiple Series Chart

Add multiple data series to compare different datasets:

```csharp
public MainWindow()
{
    InitializeComponent();

    ChartModel = new ChartModel();

    // Sales data
    var salesData = new[]
    {
        new PointD(1, 100), new PointD(2, 150), new PointD(3, 120),
        new PointD(4, 180), new PointD(5, 200), new PointD(6, 175)
    };

    // Profit data
    var profitData = new[]
    {
        new PointD(1, 20), new PointD(2, 35), new PointD(3, 25),
        new PointD(4, 45), new PointD(5, 55), new PointD(6, 40)
    };

    // Each series takes the next palette color; PaletteIndex pins a specific one
    ChartModel.AddSeries(new LineSeries(salesData) { Title = "Sales", StrokeThickness = 2 });
    ChartModel.AddSeries(new LineSeries(profitData) { Title = "Profit", StrokeThickness = 2, PaletteIndex = 2 });

    // Different value ranges? Put a series on the secondary (right) Y axis
    // ChartModel.AddSeries(new LineSeries(otherData) { Title = "Margin %", YAxisIndex = 1 });

    DataContext = this;
}
```

### 3. Real-Time Streaming Chart

Create a chart that updates in real-time. The `FastChart` control redraws automatically when
points are appended — no manual refresh needed:

```csharp
using System;
using System.Windows;
using System.Windows.Threading;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

public partial class MainWindow : Window
{
    private readonly StreamingLineSeries _streamingSeries;
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private double _currentTime;

    public ChartModel ChartModel { get; }

    public MainWindow()
    {
        InitializeComponent();

        ChartModel = new ChartModel();

        // Keep the last 100 points
        _streamingSeries = new StreamingLineSeries(maxPointCount: 100)
        {
            Title = "Live Data",
            StrokeThickness = 2
        };

        ChartModel.AddSeries(_streamingSeries);

        // Setup timer for real-time updates
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += UpdateData;
        _timer.Start();

        DataContext = this;
    }

    private void UpdateData(object? sender, EventArgs e)
    {
        // Generate random data point
        var value = Math.Sin(_currentTime) * 50 + _random.NextDouble() * 10;
        _streamingSeries.AppendPoint(new PointD(_currentTime, value));

        _currentTime += 0.1;

        // Keep the newest points in view
        ChartModel.AutoFitDataRange();
    }
}
```

Time-based windows: `new StreamingLineSeries(rollingWindow: TimeSpan.FromMinutes(2))` keeps the
last two minutes when X values are timestamps — use `AppendRealTimePoint(value)`, which stamps
points with the current time (OLE Automation date), and a `DateTimeAxis`
(`ChartModel.ReplaceXAxis(new DateTimeAxis())`) to display dates.

Points can also be appended from a background thread: series data is protected by
`series.SyncRoot`, and the control marshals its redraw to the UI thread.

## Chart Types

FastCharts supports various chart types:

### Line Charts
```csharp
var lineSeries = new LineSeries(data)
{
    Title = "Line Chart",
    StrokeThickness = 2,
    ShowMarkers = true,
    MarkerShape = MarkerShape.Diamond,
    Smoothing = LineSmoothing.Spline // smooth curve through the points
};
```

### Scatter Plots
```csharp
var scatterSeries = new ScatterSeries(data)
{
    Title = "Scatter Plot",
    MarkerSize = 5,
    MarkerShape = MarkerShape.Circle
};
```

### Bar Charts
```csharp
var barData = new[]
{
    new BarPoint(0, 10), new BarPoint(1, 15), new BarPoint(2, 8),
    new BarPoint(3, 20), new BarPoint(4, 12)
};

var barSeries = new BarSeries(barData)
{
    Title = "Bar Chart",
    Width = 0.8,        // in X units; omit to size bars from the X spacing
    FillOpacity = 0.85
};
```

### Area Charts
```csharp
var areaSeries = new AreaSeries(data)
{
    Title = "Area Chart",
    FillOpacity = 0.3,
    Baseline = 0
};
```

### Histograms
```csharp
// Bins raw values automatically (Sturges' rule) — or pass binCount
ChartModel.AddHistogram(measurements, title: "Distribution");
```

## Customization

### Themes and Colors
```csharp
using FastCharts.Core.Themes;

ChartModel.Theme = ChartThemes.Dark; // Light, Dark, HighContrast

// Custom palette, seeded from a built-in theme
ChartModel.Theme = new CustomTheme(ChartThemes.Light)
{
    SeriesPalette = new[]
    {
        new ColorRgba(33, 150, 243),
        new ColorRgba(76, 175, 80),
        new ColorRgba(244, 67, 54)
    }
};
```

### Configuring Axes
```csharp
using FastCharts.Core.Axes;
using FastCharts.Core.Formatting;

// Number formatting of tick labels (numeric axes)
if (ChartModel.YAxis is NumericAxis yAxis)
{
    yAxis.NumberFormatter = new SuffixNumberFormatter(); // 1.5k, 2M...
}

// Logarithmic / date axes
ChartModel.SetYAxisLogarithmic();
ChartModel.ReplaceXAxis(new DateTimeAxis());

// Show a specific window (zoom) or go back to the data extent
ChartModel.XAxis.VisibleRange = new FRange(0, 100);
ChartModel.AutoFitDataRange();

// Minor grid
((AxisBase)ChartModel.XAxis).ShowMinorGrid = false;
```

### Adding Interactions

`FastChart` installs sensible defaults when the model has no behaviors: pan (left drag),
wheel zoom, zoom rectangle (Shift + drag), crosshair, multi-series tooltip (click to lock,
Escape to release), nearest-point highlight and legend toggle. To choose your own set:

```csharp
using FastCharts.Core.Interaction.Behaviors;

ChartModel.Behaviors.Add(new PanBehavior());
ChartModel.Behaviors.Add(new ZoomWheelBehavior());
ChartModel.Behaviors.Add(new ZoomRectBehavior());
ChartModel.Behaviors.Add(new CrosshairBehavior());
ChartModel.Behaviors.Add(new MultiSeriesTooltipBehavior());

// Pinned tooltips (right-click to pin)
ChartModel.Behaviors.Add(new PinnedTooltipBehavior());

// Performance overlay: F3 toggles it, F4 switches detail level, F5 resets
ChartModel.Behaviors.Add(new MetricsOverlayBehavior());
```

## Performance Tips

### 1. Use Streaming Series for Real-Time Data
```csharp
var streamingSeries = new StreamingLineSeries(maxPointCount: 1000); // Limit memory usage
```

### 2. Keep Auto-Resampling for Large Datasets
```csharp
var largeSeries = new LineSeries(millionsOfPoints)
{
    EnableAutoResampling = true // LTTB on the visible window (default)
};
```
Only the visible X range is decimated (when X values are sorted), so zooming in reveals every point.

### 3. Batch Updates for Multiple Points
```csharp
// Instead of multiple AppendPoint calls
var newPoints = GenerateMultiplePoints();
streamingSeries.AppendPoints(newPoints); // One lock, one redraw request
```

## Troubleshooting

### Chart Not Displaying
1. Check that `FastChart.Model` is properly bound
2. Ensure series have valid data points
3. Verify that `DataContext` is set correctly

### Chart Not Updating
1. Mutations through the series API (`AddPoint`, `AppendPoint`, `ReplacePoints`...) redraw automatically
2. After editing a `Data` list directly, call `series.NotifyChanged()` (or `ChartModel.Invalidate()` from a view model)
3. When editing `Data` from another thread, lock `series.SyncRoot` while doing it

### Performance Issues
1. Keep auto-resampling enabled for large datasets
2. Use streaming series with a point limit for real-time scenarios
3. Prefer `AppendPoints` to many `AppendPoint` calls

## Next Steps

- [README](../README.md) - Feature overview and more examples (finance, linked charts, export)
- [Demos](../demos/) - Complete WPF demo applications
- [CHANGELOG](../CHANGELOG.md) - What changed in each release

Happy charting with FastCharts!
