# FastCharts - High-Performance .NET Charting

FastCharts is a **production-ready charting library** designed for real-time applications requiring smooth rendering of massive datasets (10M+ points).

## ? **Why Choose FastCharts?**

- ?? **60 FPS Performance** - LTTB decimation algorithm
- ?? **Rich Chart Types** - Line, Scatter, Bar, Area, Band, OHLC
- ?? **Multi-Axis Support** - Independent left/right Y axes
- ?? **Real-Time Streaming** - Rolling windows with efficient updates
- ?? **Fully Interactive** - Pan, zoom, crosshair, pinned tooltips
- ?? **Export Ready** - PNG export with clipboard support
- ?? **Cross-Platform** - Windows, macOS, Linux

## ?? **Quick Install**

### WPF Applications
```bash
dotnet add package FastCharts.Wpf
```

### Cross-Platform / Console
```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

## ?? **30-Second Example**

```xml
<!-- XAML -->
<fc:FastChart Model="{Binding ChartModel}" 
              xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf" />
```

```csharp
// C# - Create chart with data
var model = new ChartModel();
model.AddSeries(new LineSeries(new[] {
    new PointD(0, 10), new PointD(1, 20), new PointD(2, 15)
}) { Title = "Sales Data", Color = ColorRgba.Blue });
```

## ?? **Advanced Features**

### Real-Time Streaming
```csharp
var streamingSeries = new StreamingLineSeries {
    MaxPointCount = 1000,
    RollingWindowDuration = TimeSpan.FromMinutes(5)
};
streamingSeries.AppendPoint(new PointD(DateTime.Now.Ticks, value));
```

### Massive Dataset Support
```csharp
var largeSeries = new LineSeries(millionsOfPoints) {
    EnableAutoResampling = true // LTTB algorithm
};
```

### Multi-Axis Charts
```csharp
model.AddSeries(new LineSeries(tempData) { YAxisIndex = 0 }); // Left axis
model.AddSeries(new LineSeries(pressureData) { YAxisIndex = 1 }); // Right axis
```

### Interactive Behaviors
```csharp
model.AddBehavior(new PanBehavior());
model.AddBehavior(new ZoomBehavior()); 
model.AddBehavior(new PinnedTooltipBehavior()); // Right-click to pin
```

## ?? **Perfect For**

- **Financial Trading Platforms**
- **Real-Time IoT Dashboards** 
- **Scientific Data Visualization**
- **Business Intelligence Apps**
- **System Monitoring Tools**

## ?? **Framework Support**

| Target | FastCharts.Core | Rendering.Skia | WPF |
|--------|:---------------:|:--------------:|:---:|
| .NET Standard 2.0 | ? | ? | ? |
| .NET Framework 4.8 | ? | ? | ? |
| .NET 6/8 | ? | ? | ? |
| **Platforms** | **All** | **All** | **Windows** |

## ?? **Performance Benchmarks**

- **Rendering**: Up to 60 FPS interactive charts
- **Memory**: Optimized for 10M+ point datasets  
- **Streaming**: 1K+ points/second real-time updates
- **Export**: Hardware-accelerated PNG generation

## ??? **Architecture**

```
FastCharts.Core           # Algorithms & data structures
??? FastCharts.Rendering.Skia  # Cross-platform graphics  
??? FastCharts.Wpf             # WPF controls & MVVM
```

## ?? **Documentation & Examples**

- ?? [**Getting Started Guide**](https://github.com/MabinogiCode/FastCharts/blob/main/docs/getting-started.md)
- ?? [**Live Examples**](https://github.com/MabinogiCode/FastCharts/tree/main/demos)
- ?? [**API Reference**](https://github.com/MabinogiCode/FastCharts/blob/main/docs/api-reference.md)
- ???? [**Documentation Française**](https://github.com/MabinogiCode/FastCharts/blob/main/docs/getting-started-fr.md)

## ?? **Pro Tips**

- Use `StreamingLineSeries` for real-time data
- Enable `EnableAutoResampling` for large datasets
- Press **F3** for performance metrics overlay
- Right-click to pin tooltips (with `PinnedTooltipBehavior`)
- Use multi-axis for different value ranges

---

**?? Ready to build amazing charts? [Get Started Now!](https://github.com/MabinogiCode/FastCharts)**