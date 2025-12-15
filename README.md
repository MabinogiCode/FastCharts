# FastCharts - High-Performance .NET Charting Library

<div align="center">
  <img src="mabinogi-icon.png" alt="FastCharts" width="128" height="128">
  
  [![NuGet](https://img.shields.io/nuget/v/FastCharts.Wpf.svg)](https://www.nuget.org/packages/FastCharts.Wpf/)
  [![Downloads](https://img.shields.io/nuget/dt/FastCharts.Core.svg)](https://www.nuget.org/packages/FastCharts.Core/)
  [![Build Status](https://github.com/MabinogiCode/FastCharts/workflows/.NET%20CI/badge.svg)](https://github.com/MabinogiCode/FastCharts/actions)
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
</div>

## 🌟 **What is FastCharts?**

FastCharts is a **high-performance .NET charting library** designed for real-time applications, financial dashboards, and data visualization scenarios requiring **smooth rendering** of massive datasets (10M+ points).

### ⚡ **Key Features**

- 🚀 **Real-time Performance**: 60 FPS rendering with LTTB decimation
- 📊 **Rich Chart Types**: Line, Scatter, Bar, Area, Band, OHLC, ErrorBar
- 🎯 **Multi-Axis Support**: Independent left/right Y axes
- 📈 **Advanced Axes**: Linear, Logarithmic, Category with custom bases  
- 🔄 **Streaming Data**: Rolling windows with efficient append operations
- 🎨 **Interactive**: Pan, zoom, crosshair, pinned tooltips
- 📍 **Annotations**: Lines, ranges, labels with full styling
- 💾 **Export Ready**: PNG export with clipboard integration
- 🌐 **Cross-Platform**: Windows, macOS, Linux support
- 🏗️ **MVVM Ready**: Full WPF integration with ReactiveUI

### 🎯 **Perfect For**

- **Financial Applications**: Trading platforms, market analysis
- **Real-Time Dashboards**: IoT monitoring, system metrics  
- **Scientific Visualization**: Research data, sensor readings
- **Business Intelligence**: KPI dashboards, analytics
- **Desktop Applications**: WPF apps with rich data visualization

## 📦 **Quick Start**

### For WPF Applications (Recommended)
```bash
dotnet add package FastCharts.Wpf
```

### Basic Usage

**XAML:**
```xml
<Window xmlns:fc="clr-namespace:FastCharts.Wpf.Controls;assembly=FastCharts.Wpf">
    <fc:FastChart Model="{Binding ChartModel}" />
</Window>
```

**C# Code:**
```csharp
using FastCharts.Core;
using FastCharts.Core.Series;

// Create chart model
var model = new ChartModel();

// Add data series
model.AddSeries(new LineSeries(new[] {
    new PointD(0, 10),
    new PointD(1, 20),
    new PointD(2, 15),
    new PointD(3, 25)
}) { 
    Title = "Sales Data",
    Color = ColorRgba.Blue,
    StrokeWidth = 2
});

// Bind to your ViewModel
this.DataContext = new { ChartModel = model };
```

### Real-Time Streaming Example
```csharp
// Create streaming series with rolling window
var streamingSeries = new StreamingLineSeries {
    Title = "Live Data",
    MaxPointCount = 1000, // Keep last 1000 points
    RollingWindowDuration = TimeSpan.FromMinutes(5)
};

// Add real-time data
streamingSeries.AppendPoint(new PointD(DateTime.Now.Ticks, sensorValue));

model.AddSeries(streamingSeries);
```

### Multi-Axis Example
```csharp
// Create chart with multiple Y axes
var model = new ChartModel();

// Add left axis series (temperature)
model.AddSeries(new LineSeries(temperatureData) {
    Title = "Temperature (°C)",
    YAxisIndex = 0, // Left Y axis
    Color = ColorRgba.Red
});

// Add right axis series (pressure) 
model.AddSeries(new LineSeries(pressureData) {
    Title = "Pressure (hPa)", 
    YAxisIndex = 1, // Right Y axis
    Color = ColorRgba.Blue
});
```

## 🏗️ **Architecture**

FastCharts uses a clean, modular architecture:

```
├── FastCharts.Core          # Core algorithms & abstractions
├── FastCharts.Rendering.Skia # Cross-platform rendering engine
└── FastCharts.Wpf           # WPF controls & MVVM integration
```

### Package Selection Guide

| Scenario | Packages Needed |
|----------|----------------|
| **WPF Desktop App** | `FastCharts.Wpf` (includes others) |
| **Cross-Platform Console** | `FastCharts.Core` + `FastCharts.Rendering.Skia` |
| **Web API Chart Generation** | `FastCharts.Core` + `FastCharts.Rendering.Skia` |
| **Business Logic Only** | `FastCharts.Core` |

## 📊 **Advanced Features**

### LTTB Decimation for Massive Datasets
```csharp
var largeSeries = new LineSeries(millionsOfPoints) {
    EnableAutoResampling = true // Automatically reduces to screen resolution
};
// Maintains visual fidelity while ensuring 60 FPS performance
```

### Performance Metrics Overlay
```csharp
model.AddBehavior(new MetricsOverlayBehavior {
    ShowDetailed = true,
    Position = MetricsPosition.TopLeft
});
// Press F3 to toggle, F4 for detail level, F5 to reset
```

### Interactive Behaviors
```csharp
model.AddBehavior(new PanBehavior());
model.AddBehavior(new ZoomBehavior());
model.AddBehavior(new CrosshairBehavior());
model.AddBehavior(new PinnedTooltipBehavior()); // Right-click to pin tooltips
```

### Annotations
```csharp
// Add horizontal line annotation
model.AddAnnotation(new LineAnnotation {
    Type = LineAnnotationType.Horizontal,
    Value = 100,
    Label = "Target Value",
    Color = ColorRgba.Green
});

// Add range highlight
model.AddAnnotation(new RangeAnnotation {
    Type = RangeAnnotationType.Horizontal, 
    StartValue = 90,
    EndValue = 110,
    FillColor = ColorRgba.Green.WithAlpha(0.2f),
    Label = "Acceptable Range"
});
```

## 🚀 **Performance**

- **Rendering Speed**: Up to 60 FPS for interactive charts
- **Memory Efficiency**: Optimized for large datasets (10M+ points)
- **LTTB Algorithm**: Reduces data while preserving visual fidelity
- **Hardware Acceleration**: GPU-accelerated rendering via SkiaSharp
- **Streaming Optimized**: Real-time updates with minimal overhead

## 🌐 **Framework Support**

| Framework | FastCharts.Core | Rendering.Skia | WPF |
|-----------|:---------------:|:--------------:|:---:|
| .NET Standard 2.0 | ✅ | ✅ | ❌ |
| .NET Framework 4.8 | ✅ | ✅ | ✅ |
| .NET 6 | ✅ | ✅ | ✅ |
| .NET 8 | ✅ | ✅ | ✅ |

**Platforms**: Windows, macOS, Linux (Core + Skia), Windows only (WPF)

## 📖 **Documentation**

- 🚀 [**Getting Started**](docs/getting-started.md) | [**Démarrage Rapide**](docs/getting-started-fr.md)
- 📊 [**Chart Types Guide**](docs/chart-types.md) | [**Guide des Types de Graphiques**](docs/chart-types-fr.md)
- 🎨 [**Styling & Themes**](docs/styling.md) | [**Style & Thèmes**](docs/styling-fr.md)
- ⚡ [**Performance Guide**](docs/performance.md) | [**Guide Performance**](docs/performance-fr.md)
- 🔌 [**API Reference**](docs/api-reference.md) | [**Référence API**](docs/api-reference-fr.md)
- 🧪 [**Examples & Demos**](demos/) | [**Exemples & Démos**](demos/)

## 🤝 **Contributing**

We welcome contributions! See our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup
```bash
git clone https://github.com/MabinogiCode/FastCharts.git
cd FastCharts
dotnet restore
dotnet build
dotnet test # 594 tests should pass
```

## 📝 **License**

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 **Acknowledgments**

- Built with [SkiaSharp](https://github.com/mono/SkiaSharp) for cross-platform graphics
- Inspired by [LiveCharts2](https://github.com/beto-rodriguez/LiveCharts2) and [ScottPlot](https://github.com/ScottPlot/ScottPlot)
- LTTB algorithm implementation based on [research by Sveinn Steinarsson](https://github.com/sveinn-steinarsson/flot-downsample)

---

<div align="center">
  <strong>Made with ❤️ for the .NET community</strong><br>
  <sub>FastCharts - Where performance meets visualization</sub>
</div>
