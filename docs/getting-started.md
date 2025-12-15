# Getting Started with FastCharts

Welcome to FastCharts! This guide will help you get up and running with high-performance charting in your .NET applications.

## ?? Installation

### For WPF Applications (Most Common)

Install the WPF package which includes everything you need:

```bash
dotnet add package FastCharts.Wpf
```

### For Cross-Platform Applications

Install the core packages for console apps, web services, or non-WPF scenarios:

```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## ?? Your First Chart

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
            
            // Create and add series
            var series = new LineSeries(data)
            {
                Title = "Sample Data",
                Color = ColorRgba.Blue,
                StrokeWidth = 2
            };
            
            ChartModel.AddSeries(series);
            
            // Set data context for binding
            DataContext = this;
        }
    }
}
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
    
    // Add sales series
    ChartModel.AddSeries(new LineSeries(salesData)
    {
        Title = "Sales",
        Color = ColorRgba.Blue,
        StrokeWidth = 2
    });
    
    // Add profit series
    ChartModel.AddSeries(new LineSeries(profitData)
    {
        Title = "Profit", 
        Color = ColorRgba.Green,
        StrokeWidth = 2
    });
    
    DataContext = this;
}
```

### 3. Real-Time Streaming Chart

Create a chart that updates in real-time:

```csharp
using System;
using System.Windows.Threading;
using FastCharts.Core.Series;

public partial class MainWindow : Window
{
    private StreamingLineSeries _streamingSeries;
    private DispatcherTimer _timer;
    private Random _random = new();
    private double _currentTime = 0;

    public MainWindow()
    {
        InitializeComponent();
        
        ChartModel = new ChartModel();
        
        // Create streaming series with rolling window
        _streamingSeries = new StreamingLineSeries
        {
            Title = "Live Data",
            Color = ColorRgba.Red,
            StrokeWidth = 2,
            MaxPointCount = 100, // Keep last 100 points
            RollingWindowDuration = TimeSpan.FromMinutes(2)
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
    
    private void UpdateData(object sender, EventArgs e)
    {
        // Generate random data point
        var value = Math.Sin(_currentTime) * 50 + _random.NextDouble() * 10;
        _streamingSeries.AppendPoint(new PointD(_currentTime, value));
        
        _currentTime += 0.1;
    }
}
```

## ?? Chart Types

FastCharts supports various chart types:

### Line Charts
```csharp
var lineSeries = new LineSeries(data)
{
    Title = "Line Chart",
    Color = ColorRgba.Blue,
    StrokeWidth = 2,
    ShowMarkers = true
};
```

### Scatter Plots
```csharp
var scatterSeries = new ScatterSeries(data)
{
    Title = "Scatter Plot", 
    Color = ColorRgba.Green,
    MarkerSize = 5
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
    FillColor = ColorRgba.Orange,
    Width = 0.8
};
```

### Area Charts
```csharp
var areaSeries = new AreaSeries(data)
{
    Title = "Area Chart",
    FillColor = ColorRgba.Blue.WithAlpha(0.3f),
    StrokeColor = ColorRgba.Blue,
    StrokeWidth = 2
};
```

## ?? Customization

### Styling Series
```csharp
var series = new LineSeries(data)
{
    Title = "Styled Series",
    Color = ColorRgba.Purple,
    StrokeWidth = 3,
    StrokeDashArray = new[] { 5f, 2f }, // Dashed line
    ShowMarkers = true,
    MarkerSize = 6,
    MarkerShape = MarkerShape.Circle
};
```

### Configuring Axes
```csharp
// Customize X axis
ChartModel.XAxis.Title = "Time (seconds)";
ChartModel.XAxis.LabelFormat = "F1";
ChartModel.XAxis.ShowGrid = true;

// Customize Y axis  
ChartModel.YAxis.Title = "Value";
ChartModel.YAxis.LabelFormat = "N0"; 
ChartModel.YAxis.ShowGrid = true;
```

### Adding Interactions
```csharp
// Enable pan and zoom
ChartModel.AddBehavior(new PanBehavior());
ChartModel.AddBehavior(new ZoomBehavior());

// Enable crosshair
ChartModel.AddBehavior(new CrosshairBehavior());

// Enable tooltips
ChartModel.AddBehavior(new TooltipBehavior());

// Enable pinned tooltips (right-click to pin)
ChartModel.AddBehavior(new PinnedTooltipBehavior());
```

## ?? Performance Tips

### 1. Use Streaming Series for Real-Time Data
```csharp
var streamingSeries = new StreamingLineSeries
{
    MaxPointCount = 1000, // Limit memory usage
    RollingWindowDuration = TimeSpan.FromMinutes(5)
};
```

### 2. Enable Auto-Resampling for Large Datasets
```csharp
var largeSeries = new LineSeries(millionsOfPoints)
{
    EnableAutoResampling = true // Uses LTTB algorithm
};
```

### 3. Batch Updates for Multiple Points
```csharp
// Instead of multiple AppendPoint calls
var newPoints = GenerateMultiplePoints();
streamingSeries.AppendPoints(newPoints); // More efficient
```

## ?? Troubleshooting

### Chart Not Displaying
1. Check that `FastChart.Model` is properly bound
2. Ensure series have valid data points
3. Verify that `DataContext` is set correctly

### Performance Issues
1. Enable auto-resampling for large datasets
2. Use streaming series for real-time scenarios  
3. Limit the number of visible points
4. Check if multiple series are causing overhead

### Binding Issues
1. Implement `INotifyPropertyChanged` in your ViewModels
2. Use `ObservableCollection` for dynamic series
3. Ensure proper thread marshalling for UI updates

## ?? Next Steps

- [Chart Types Guide](chart-types.md) - Detailed guide for all chart types
- [Performance Guide](performance.md) - Optimize charts for large datasets
- [Styling Guide](styling.md) - Customize appearance and themes
- [API Reference](api-reference.md) - Complete API documentation
- [Examples](../demos/) - More complex examples and demos

## ?? Tips

- Use `StreamingLineSeries` for real-time data updates
- Enable auto-resampling for datasets with >10K points
- Use behaviors to add interactivity (pan, zoom, tooltips)  
- Pin tooltips by right-clicking (with `PinnedTooltipBehavior`)
- Press F3 to toggle performance metrics overlay
- Use multi-axis support for different value ranges

Happy charting with FastCharts! ??