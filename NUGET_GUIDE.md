# FastCharts NuGet Packages

## ?? Package Overview

FastCharts is distributed as three complementary NuGet packages:

| Package | Description | Use Case |
|---------|-------------|----------|
| **FastCharts.Core** | Core algorithms and abstractions | Console apps, services, cross-platform |
| **FastCharts.Rendering.Skia** | Cross-platform rendering engine | Console apps, web apps, cross-platform UI |
| **FastCharts.Wpf** | WPF controls and MVVM integration | Windows desktop applications |

## ?? Quick Start

### For WPF Applications (Recommended)
```bash
dotnet add package FastCharts.Wpf
```

This automatically includes Core and Rendering.Skia as dependencies.

**XAML:**
```xml
<Window xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf">
    <fc:FastChart Model="{Binding ChartModel}" />
</Window>
```

**C#:**
```csharp
using FastCharts.Core;
using FastCharts.Core.Series;

var model = new ChartModel();
model.AddSeries(new LineSeries(new[] {
    new PointD(0, 10),
    new PointD(1, 20),
    new PointD(2, 15)
}) { Title = "My Data" });

// Bind to your ViewModel
this.DataContext = new { ChartModel = model };
```

### For Cross-Platform Applications
```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

**C#:**
```csharp
using FastCharts.Core;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;

// Create chart model
var model = new ChartModel();
model.AddSeries(new LineSeries(data) { Title = "Series 1" });

// Render to PNG
var exporter = new SkiaChartExporter();
var pngData = exporter.ExportPngBytes(model, 800, 600);
File.WriteAllBytes("chart.png", pngData);
```

### For Console Applications
```bash
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia
```

Perfect for generating charts in background services, APIs, or command-line tools.

## ?? Use Cases by Package

### FastCharts.Core
- ? **Business Logic**: Chart models, series, axes
- ? **Data Processing**: LTTB decimation, streaming data
- ? **Algorithms**: Auto-scaling, range calculations
- ? **Interactions**: Behaviors, events, state management
- ? **Rendering**: No visual output (abstract only)

### FastCharts.Rendering.Skia  
- ? **Cross-Platform Rendering**: Windows, macOS, Linux
- ? **Export Formats**: PNG, JPEG with high quality
- ? **Performance**: Hardware-accelerated rendering
- ? **Headless**: Server-side chart generation
- ? **UI Controls**: No interactive controls (rendering only)

### FastCharts.Wpf
- ? **Windows Desktop**: Native WPF integration
- ? **MVVM Support**: Data binding, commands, reactive UI
- ? **Interactive Controls**: Pan, zoom, tooltips, crosshair
- ? **Design-Time Support**: Visual Studio/Blend integration
- ? **Cross-Platform**: Windows-only

## ?? Feature Matrix

| Feature | Core | Skia | WPF |
|---------|:----:|:----:|:---:|
| **Series Types** |
| Line Series | ? | ? | ? |
| Scatter Series | ? | ? | ? |  
| Bar Series | ? | ? | ? |
| Area/Band Series | ? | ? | ? |
| OHLC Series | ? | ? | ? |
| **Axes & Scaling** |
| Linear Axes | ? | ? | ? |
| Logarithmic Axes | ? | ? | ? |
| Category Axes | ? | ? | ? |
| Multi-Axis Support | ? | ? | ? |
| **Performance** |
| LTTB Decimation | ? | ? | ? |
| Streaming Data | ? | ? | ? |
| Performance Metrics | ? | ? | ? |
| **Interactions** |
| Pan & Zoom | ? | ? | ? |
| Tooltips | ? | ? | ? |
| Pinned Tooltips | ? | ? | ? |
| Crosshair | ? | ? | ? |
| **Export** |
| PNG Export | ? | ? | ? |
| Clipboard Support | ? | ? | ? |
| **Annotations** |
| Line Annotations | ? | ? | ? |
| Range Annotations | ? | ? | ? |

## ?? Advanced Scenarios

### 1. Real-Time Dashboard (WPF)
```csharp
// Install: FastCharts.Wpf
var streamingSeries = new StreamingLineSeries { 
    MaxPointCount = 1000,
    Title = "Live Data"
};

// Add real-time data
streamingSeries.AppendPoint(new PointD(DateTime.Now.Ticks, sensorValue));
```

### 2. Web API Chart Generation (ASP.NET Core)
```csharp
// Install: FastCharts.Core + FastCharts.Rendering.Skia
[HttpGet("chart")]
public IActionResult GetChart()
{
    var model = CreateChartModel();
    var exporter = new SkiaChartExporter();
    var pngData = exporter.ExportPngBytes(model, 800, 600);
    return File(pngData, "image/png");
}
```

### 3. Massive Dataset Visualization
```csharp
// LTTB decimation for 10M+ points
var largeSeries = new LineSeries(millionsOfPoints) {
    EnableAutoResampling = true
};
// Automatically reduces to screen resolution
```

### 4. Multi-Platform Console Tool
```csharp
// Works on Windows, macOS, Linux
dotnet add package FastCharts.Core
dotnet add package FastCharts.Rendering.Skia

// Generate charts without UI framework
```

## ?? Performance Benchmarks

- **Rendering Speed**: Up to 60 FPS for interactive charts
- **Memory Usage**: Optimized for large datasets (10M+ points)
- **Decimation**: LTTB algorithm reduces data while preserving visual fidelity
- **Streaming**: Real-time updates with rolling windows

## ??? Framework Compatibility

| Framework | Core | Rendering.Skia | WPF |
|-----------|:----:|:-------------:|:---:|
| .NET Standard 2.0 | ? | ? | ? |
| .NET Framework 4.8 | ? | ? | ? |
| .NET 6 | ? | ? | ? |
| .NET 8 | ? | ? | ? |
| **Platforms** |
| Windows | ? | ? | ? |
| macOS | ? | ? | ? |
| Linux | ? | ? | ? |

## ?? Links

- ?? **Documentation**: [GitHub Repository](https://github.com/MabinogiCode/FastCharts)
- ?? **Examples**: [Demo Projects](https://github.com/MabinogiCode/FastCharts/tree/main/demos)
- ?? **NuGet**: [FastCharts.Wpf](https://www.nuget.org/packages/FastCharts.Wpf/)
- ?? **Issues**: [GitHub Issues](https://github.com/MabinogiCode/FastCharts/issues)
- ?? **Discussions**: [GitHub Discussions](https://github.com/MabinogiCode/FastCharts/discussions)

---

**Ready to build amazing charts? Choose the package that fits your needs! ??**