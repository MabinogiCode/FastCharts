# FastCharts - High-Performance .NET Charting

FastCharts is a charting library designed for real-time applications requiring smooth rendering of massive datasets (10M+ points).

## Why Choose FastCharts?

- **60 FPS Performance** - LTTB decimation of the visible window, per-series geometry cache
- **Rich Chart Types** - Line, Scatter, Bar, Stacked Bar, Area, Band, Step, OHLC, Error Bar, Histogram
- **Multi-Axis Support** - Independent left/right Y axes, linear/logarithmic/date/category axes
- **Real-Time Streaming** - Rolling windows, thread-safe appends, automatic redraw
- **Fully Interactive** - Pan, zoom, crosshair, tooltips, pinned tooltips
- **Export Ready** - PNG and SVG export, clipboard support (WPF)
- **Cross-Platform Core** - Windows, macOS, Linux (WPF control on Windows)

## Quick Install

### WPF Applications
```bash
dotnet add package FastCharts.Wpf
```

### Cross-Platform / Console
```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## 30-Second Example

```xml
<!-- XAML -->
<fc:FastChart Model="{Binding ChartModel}"
              xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf" />
```

```csharp
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

// C# - Create chart with data (colors come from the theme palette)
var model = new ChartModel();
model.AddSeries(new LineSeries(new[] {
    new PointD(0, 10), new PointD(1, 20), new PointD(2, 15)
}) { Title = "Sales Data", StrokeThickness = 2 });
```

## Advanced Features

### Real-Time Streaming
```csharp
var streamingSeries = new StreamingLineSeries(maxPointCount: 1000, rollingWindow: TimeSpan.FromMinutes(5));
model.AddSeries(streamingSeries);
streamingSeries.AppendRealTimePoint(value); // the chart redraws automatically
```

### Massive Dataset Support
```csharp
var largeSeries = new LineSeries(millionsOfPoints) {
    EnableAutoResampling = true // LTTB on the visible window (default)
};
```

### Multi-Axis Charts
```csharp
model.AddSeries(new LineSeries(tempData) { YAxisIndex = 0 }); // Left axis
model.AddSeries(new LineSeries(pressureData) { YAxisIndex = 1 }); // Right axis
```

### Interactive Behaviors
```csharp
using FastCharts.Core.Interaction.Behaviors;

// FastChart installs defaults when the model has none; to customize:
model.Behaviors.Add(new PanBehavior());
model.Behaviors.Add(new ZoomWheelBehavior());
model.Behaviors.Add(new PinnedTooltipBehavior()); // Right-click to pin
```

## Perfect For

- Financial trading platforms
- Real-time IoT dashboards
- Scientific data visualization
- Business intelligence apps
- System monitoring tools

## Framework Support

| Package | Target frameworks | Platforms |
|---------|-------------------|-----------|
| FastCharts.Core | .NET Standard 2.0, .NET 8 | All |
| FastCharts.Rendering.Skia | .NET Standard 2.0, .NET 8 | All |
| FastCharts.Wpf | .NET Framework 4.8, .NET 6, .NET 8 (Windows) | Windows |

## Architecture

```
FastCharts.Core                # Algorithms, data structures, interactions
├── FastCharts.Rendering.Skia  # Cross-platform rendering and export
└── FastCharts.Wpf             # WPF control and MVVM integration
```

## Documentation & Examples

- [Getting Started Guide](https://github.com/MabinogiCode/FastCharts/blob/main/docs/getting-started.md)
- [Guide de démarrage (français)](https://github.com/MabinogiCode/FastCharts/blob/main/docs/getting-started-fr.md)
- [Demo applications](https://github.com/MabinogiCode/FastCharts/tree/main/demos)
- [Changelog](https://github.com/MabinogiCode/FastCharts/blob/main/CHANGELOG.md)

## Pro Tips

- Use `StreamingLineSeries` for real-time data (appends may come from a background thread)
- Keep `EnableAutoResampling` on for large datasets: zooming in reveals every point
- Add `MetricsOverlayBehavior` and press **F3** for the performance overlay
- Right-click to pin tooltips (with `PinnedTooltipBehavior`)
- Use the secondary Y axis (`YAxisIndex = 1`) for different value ranges
