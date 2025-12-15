# Changelog

All notable changes to FastCharts will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-12-15

### ?? **PHASE 1 COMPLETE - MAJOR RELEASE**

This is the first stable release of FastCharts with all Phase 1 features implemented and production-ready.

### ? **Added**

#### Core Features
- **Multi-Axis Support** (P1-AX-MULTI): Independent left/right Y axes with series attachment via `YAxisIndex`
- **Logarithmic Axes** (P1-AX-LOG): Base-10 and custom base logarithmic scaling with `LogScale`, `LogTicker`, and `ScientificNumberFormatter`
- **Category Axes** (P1-AX-CAT): Discrete string labels with custom spacing for bar charts and categorical data
- **Annotations System**: 
  - Line annotations (P1-ANN-LINE): Horizontal/vertical lines with labels and styling
  - Range annotations (P1-ANN-RANGE): Filled bands between two values for highlighting ranges

#### Export & Rendering
- **PNG Export** (P1-EXPORT-PNG): High-quality image export with sync/async methods
- **Clipboard Support**: Direct copy-to-clipboard functionality for charts
- **Cross-Platform Rendering**: SkiaSharp-based rendering engine supporting Windows, macOS, and Linux

#### Performance & Real-Time
- **LTTB Decimation** (P1-RESAMPLE-LTTB): Largest Triangle Three Buckets algorithm for massive dataset visualization (10M+ points)
- **Streaming Series** (P1-STREAM-APPEND): `StreamingLineSeries` with rolling windows and efficient real-time append API
- **Performance Metrics** (P1-METRICS): Complete metrics system with FPS tracking, memory monitoring, and overlay display

#### Interactions
- **Pinned Tooltips** (P1-TOOLTIP-PIN): Multi-tooltip support with right-click pinning and data export
- **Interactive Behaviors**: Pan, zoom, crosshair, nearest point tracking
- **Keyboard Shortcuts**: F3 (toggle metrics), F4 (detail level), F5 (reset metrics)

#### Architecture & Quality
- **Clean Architecture**: Modular design with Core, Rendering.Skia, and WPF packages
- **MVVM Support**: Full ReactiveUI integration with data binding and commands
- **Multi-Framework Support**: .NET Standard 2.0, .NET Framework 4.8, .NET 6, .NET 8
- **594 Unit Tests**: Comprehensive test coverage with 100% pass rate
- **Guidelines Compliance**: SA1402, English documentation, proper code formatting

### ?? **Design & Branding**
- **Mabinogi Icon**: Added custom branding icon to all NuGet packages
- **Bilingual Documentation**: Complete Getting Started guides in English and French
- **Professional NuGet Metadata**: Rich package descriptions with proper icons and links

### ?? **Developer Experience**
- **Automated Release Pipeline**: GitHub Actions workflows for tag-based NuGet publishing  
- **Release Scripts**: PowerShell automation for local and remote releases
- **Comprehensive Documentation**: Getting started, API reference, performance guides
- **Example Projects**: Working demos for WPF applications

### ?? **Series Types**
- `LineSeries`: High-performance line charts with markers and styling
- `ScatterSeries`: Scatter plots with customizable markers  
- `BarSeries`: Bar charts with grouping and baseline support
- `StackedBarSeries`: Stacked bar visualization
- `AreaSeries`: Filled area charts
- `BandSeries`: Range/band visualization with high/low values
- `OhlcSeries`: Open-High-Low-Close financial charts
- `ErrorBarSeries`: Error bar visualization
- `StepLineSeries`: Step-line charts
- `StreamingLineSeries`: Real-time streaming with rolling windows

### ?? **Target Applications**
- Financial trading platforms and market analysis
- Real-time IoT dashboards and system monitoring
- Scientific data visualization and research tools
- Business intelligence and KPI dashboards  
- Desktop applications with rich data visualization needs

### ?? **Platform Support**
- **Windows**: Full WPF integration + cross-platform rendering
- **macOS**: Cross-platform rendering via Core + Rendering.Skia
- **Linux**: Cross-platform rendering via Core + Rendering.Skia
- **Framework Targets**: .NET Standard 2.0, .NET Framework 4.8, .NET 6, .NET 8

### ?? **NuGet Packages**
- `FastCharts.Core` 1.0.0: Core algorithms and abstractions
- `FastCharts.Rendering.Skia` 1.0.0: Cross-platform rendering engine  
- `FastCharts.Wpf` 1.0.0: WPF controls and MVVM integration

### ?? **Performance Benchmarks**
- **Rendering**: Up to 60 FPS for interactive charts
- **Memory**: Optimized for datasets up to 10M points
- **Streaming**: Real-time updates with 1K+ points/second capability
- **Decimation**: LTTB algorithm maintains visual fidelity while ensuring performance

---

## Development Phases

### ? **Phase 1 Complete (v1.0.0)**
All 13 planned Phase 1 features have been implemented and tested:
- Multi-axis support
- Logarithmic axes  
- Category axes
- Line and range annotations
- PNG export with clipboard
- LTTB decimation
- Streaming series
- Pinned tooltips
- Performance metrics system

### ?? **Phase 2 Planned**
Upcoming features for future releases:
- Pie/Donut charts (P2-SERIES-PIE)
- HeatMap visualization (P2-HEATMAP)  
- SVG export (P2-EXPORT-SVG)
- Enhanced data binding (P2-DATA-BIND)
- Financial indicators (RSI, MACD, Bollinger Bands)
- 3D visualization capabilities

---

**?? FastCharts v1.0.0 - Production Ready for the .NET Community!**