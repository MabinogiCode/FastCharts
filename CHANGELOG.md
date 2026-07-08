# Changelog

All notable changes to FastCharts will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### ✨ **Added — "Analytics" (v1.5, in progress)**

- **Histogram with auto-binning** (P2-HISTOGRAM): `model.AddHistogram(values)` bins raw values into contiguous bars in one line. Bin count is chosen automatically (Sturges' rule) or can be fixed via `binCount`. NaN/infinities are ignored; all-identical values collapse to a single bar. The pure, unit-tested `HistogramBuilder.Build(values, binCount?)` returns a ready-to-add `BarSeries` (bars centered on each bin, width = bin width, Y = count).

## [1.4.0] - 2026-07-08

### ✨ **Added — "Large volumes & GPU"**

- **Opt-in GPU backend** (T-PERF-GPU): `FastChart.UseGpu` renders the chart through an OpenGL-backed `SKGLElement` instead of the default CPU raster `SKElement`. Default is `false` — the CPU path is untouched (the templated `SKElement` is reused as-is), so existing apps are unaffected. Set `UseGpu="True"` (or bind it) to offload rasterization to the GPU; the surface rebuilds live when the value changes. Visually validated pixel-identical to the CPU path on both .NET 8 and .NET Framework 4.8 demos. `GLWpfControl`/`OpenTK` ship transitively with `SkiaSharp.Views.WPF`, so no extra package references are needed.
- **Geometry caching** (T-PERF-CACHE): per-series `SKPath`/pixel cache in `LineLayer`, keyed on `LineSeries.DataVersion` + ranges + plot rect — identical frames are byte-for-byte reused (shipped in 1.4 dev, now released).
- **BenchmarkDotNet suite** (T-PERF-PROF): `benchmarks/FastCharts.Benchmarks` for measuring resampling/render costs (LTTB 1M points ≈ 4.8 ms).

### 🛠️ **Fixed**

- **Demo apps failed to start**: `DemoApp.Net48` crashed on first render with `FileLoadException` in `SkiaSharp.HandleDictionary..cctor` — SkiaSharp 3.x is built against older facade versions (`System.Runtime.CompilerServices.Unsafe` 4.0.4.1, `System.Buffers` 4.0.3.0) than the ones restored transitively, and `AutoGenerateBindingRedirects` did not bridge them. Added an explicit `app.config` with the missing binding redirects. (`DemoApp.Net8`'s `ReactiveUI.XamForms` `FileNotFoundException` was only a first-chance exception ReactiveUI catches internally — the app always ran fine outside the debugger.)

## [1.3.0] - 2026-07-05

### ✨ **Added — "Finance"**

- **Enhanced candlesticks** (P2-SERIES-CANDLESTICK): `OhlcSeries.BullColor`/`BearColor` (null = theme-derived, unchanged default), optional `OhlcPoint.Volume` and `ShowVolume` rendering volume bars in a bottom band of the plot (`VolumePaneRatio`, `VolumeOpacity`) colored bull/bear — the classic trading layout.
- **Financial indicators** (P3-FIN-INDICATORS, first batch): `Indicators.Sma`, `Indicators.Ema` (SMA-seeded), `Indicators.BollingerBands` (middle `LineSeries` + ±kσ `BandSeries`). All return ready-to-add series and accept either `IReadOnlyList<PointD>` or an `OhlcSeries` (Close prices). O(n) sliding-window SMA/EMA.
- **Linked charts** (P2-AX-LINK): `ChartLinkGroup` synchronizes the visible X range across models (price chart + indicator chart stay aligned under zoom/pan); survives axis replacement (e.g. switching to log X); `Remove`/`Dispose` unlink cleanly.
- **`AxisBase.VisibleRangeChanged` event**: raised on any visible-range change (zoom, pan, auto-fit) — the hook behind axis linking, also usable by apps.
- **Release from the browser**: the *Publish NuGet Packages* workflow now supports manual dispatch (Actions → Run workflow → enter version) — it creates the tag, builds, tests, publishes to NuGet and creates the GitHub release. No local git needed.

## [1.2.0] - 2026-07-05

### ✨ **Added — "Everyday chart"**

- **Markers on line series**: `ShowMarkers`, `MarkerSize`, `MarkerShape` moved to `LineSeries` (inherited by streaming & observable series) and now actually rendered. `MarkerShape` extended with `Diamond`, `Cross`, `Plus` (shared with scatter).
- **Spline smoothing** (P2-SERIES-SPLINE): `series.Smoothing = LineSmoothing.Spline` renders a smooth Catmull-Rom curve through the points (works with LTTB resampling).
- **SVG vector export** (P2-EXPORT-SVG): `SkiaChartRenderer.ExportSvg(model, stream, w, h)`, plus `ChartExportService.ExportToSvgFile/ExportToSvgString`. New optional capability interface `ISvgChartExporter` (non-breaking).
- **Dynamic themes** (P2-THEME-DYNAMIC): `ChartThemes.Light/Dark/HighContrast` singletons for one-liner runtime switching (`model.Theme = ChartThemes.Dark`), new `HighContrastTheme`, and mutable `CustomTheme` seeded from any base theme with overridable palette.
- **KISS chart kinds**: `model.AddSeries(data, ChartKind.Area|Scatter|Bar|StepLine, title)` — the dictionary one-liner now covers all basic series types.

### ⚠️ **Changed**

- Duplicate `FastCharts.Core.DataBinding.Series.MarkerShape` enum removed; the canonical `FastCharts.Core.Series.MarkerShape` is used everywhere (values preserved: Circle=0, Square=1, Triangle=2).

## [1.1.0] - 2026-07-05

### 🛠️ **Fixed — the library is now truly functional for streaming & MVVM**

- **StreamingLineSeries rendered nothing**: the series hid (`new`) `GetRenderData`/`GetXRange`/`GetYRange`/`Data` instead of overriding them. Renderers hold `LineSeries` references, so they always read the (empty) base data list. Streaming data is now stored in the base `LineSeries` list and `GetRenderData`/`GetXRange`/`GetYRange` are `virtual` — streamed points render, resample and auto-fit correctly.
- **Observable (MVVM) series could not be charted**: `ObservableLineSeries`, `ObservableScatterSeries` and `ObservableBarSeries` did not derive from `SeriesBase`, so they could not be added to `ChartModel.Series`. They now derive from `LineSeries`/`ScatterSeries`/`BarSeries` respectively and render like any other series while staying bound to their `ItemsSource`.
- **`RefreshThrottle` was ignored** in observable series: a positive throttle now coalesces refresh bursts into a single update (default remains `TimeSpan.Zero` = synchronous refresh).
- **Items added to a bound collection after the initial subscription were not observed** for `INotifyPropertyChanged`: per-item listeners now track adds/removes/replaces/resets.
- **SkiaSharp native assets on Linux**: test suite now runs cross-platform (Linux natives referenced in tests).

### ⚡ **Performance**

- **Zero-copy render path**: `LineSeries.GetRenderData` no longer calls `Data.ToArray()` on every frame (previously a full copy of the series — even 1M points — up to 60×/s). Small/non-resampled datasets return the backing list directly; resampled output is cached and invalidated by point count + viewport width.
- **Compiled property-path resolver**: new `CachedPropertyPathResolver` (default for data binding) compiles property access chains into cached delegates instead of reflecting per item per refresh.
- **Allocation-free series color resolution**: the Skia renderer no longer allocates `List<T>` per series per frame to find palette indexes.
- **StreamingLineSeries appends**: no more LINQ `ToArray`/`OrderByDescending` per append; single-pass min/max tracking.

### ✨ **Added**

- **KISS quick-plot API**: `model.AddSeries(myDictionary)` plots a `Dictionary<double, double>` as a sorted line in one call (also `model.AddSeries(doubleValues)` with index as X, `LineSeries(IEnumerable<KeyValuePair<double, double>>)`, `LineSeries(IEnumerable<double>)` and `ToLineSeries()` extensions).
- `LineSeries.ReplacePoints(IEnumerable<PointD>)`: single-operation content swap for data-binding scenarios.
- `SeriesDataBinder`: reusable data-binding engine (collection + item observation, throttling, property paths) used by all observable series and available for custom series.
- `CachedPropertyPathResolver.Instance`: high-performance path resolver usable standalone.

### ⚠️ **Changed**

- `ObservableSeriesBase<T>` was removed; observable series now inherit their renderable counterparts. The `IObservableSeries` / `IObservableSeries<T>` contracts are unchanged. (The 1.0.0 observable series could not be attached to a chart, so no functional consumer is affected.)
- `ScatterSeries` and `BarSeries` are no longer `sealed` (enables observable variants).
- `FastCharts.Core.Examples.ServiceConfigurationExample` was removed from the shipped assembly (example code now lives in documentation).

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