# FastCharts Roadmap

> Living document – will evolve as features are designed and delivered. Keep PRs small and reference the section / item IDs below.

## 0. Vision
High‑performance, extensible .NET charting library (Core + Skia renderer + WPF host) with clear API boundaries (data model / interaction / rendering) and analyzers guiding best usage. Target: real‑time, finance, analytical dashboards.

**Inspiration**: LiveCharts2, ScottPlot, OxyPlot, SciChart, Aspose.Charts for feature completeness and API design patterns.

## 1. Current Baseline (DONE)
- Core primitives (ranges, points, basic series: Line, Area, Band, Bar, StackedBar, StepLine, Scatter, Ohlc, ErrorBar)
- Numeric / Nice / Date tickers, minor ticks
- Interaction basics: pan, wheel zoom, rectangle zoom, crosshair, nearest point, legend toggle, multi‑series tooltip
- Skia rendering layers + WPF control host
- Nullability + analyzers cleaned; warnings treated as errors

## 2. Guiding Principles
1. Zero breaking changes without deprecation window.
2. Render pipeline layered: data → (optional transforms) → geometry → Skia.
3. Performance first: avoid LINQ in hot paths; preallocate; support decimation.
4. Extensibility via small interfaces (IBehavior, ITicker, IAnnotation (planned), IResampler (planned)).
5. Tests for every public feature; visual regression where feasible.

---
## 3. Phase 1 (Short Term: Foundational Enhancements)
| ID | Feature | Description | Notes |
|----|---------|-------------|-------|
| P1-AX-MULTI | Multiple Y axes | Support attaching series to left/right axes | Refactor ChartModel.Axes collection |
| P1-AX-LOG | Log axis | Base‑10 (later custom bases) | Separate LogNumericAxis |
| P1-AX-CAT | Category axis | Discrete string labels with custom spacing | CategoryAxis for bar charts |
| P1-AX-COLOR | Color scale axis | Continuous color mapping for heatmaps | ColorAxis with gradient |
| P1-ANN-LINE | Annotation line | Horizontal/vertical line with label | IAnnotation start |
| P1-ANN-RANGE | Range highlight | Filled band between two values (X or Y) | Depends on annotation layer |
| P1-EXPORT-PNG | PNG export | Render to SKBitmap / stream | Reuse existing render path |
| P1-EXPORT-CLIPBOARD | Clipboard support | Copy chart as image | Windows.Forms.Clipboard integration |
| P1-RESAMPLE-LTTB | Line LOD (LTTB) | Downsample large line series to viewport pixels | Plug via IResampler |
| P1-RESAMPLE-MINMAX | MinMax decimation | Bucket-based min/max for fast preview | Alternative to LTTB |
| P1-STREAM-APPEND | Streaming append API | Efficient append w/ rolling window | Add Append/Trim methods |
| P1-TOOLTIP-PIN | Pinned tooltips | User can lock multiple tooltips | Extend InteractionState |
| P1-TOOLTIP-DOCK | Dockable tooltips | Tooltip positioning options | Follow cursor vs fixed |
| P1-METRICS | Render metrics overlay | FPS, point count, memory usage | Behavior + overlay layer |
| P1-SNAP-GRID | Snap to grid/data | Crosshair snaps to nearest data points | Enhanced CrosshairBehavior |

Deliverables for phase closure: feature flags, tests, basic docs updates.

---
## 4. Phase 2 (Mid Term: Breadth & UX)
| ID | Feature | Description | Priority |
|----|---------|-------------|----------|
| P2-SERIES-PIE | Pie/Donut charts | Non-cartesian with labels, explosion | High |
| P2-SERIES-CANDLESTICK | Enhanced OHLC | Volume bars, custom colors | High |
| P2-SERIES-SPLINE | Spline interpolation | Cubic/Bezier smooth curves | Medium |
| P2-SERIES-BUBBLE | Bubble charts | X/Y/Size mapping with scaling | Medium |
| P2-HEATMAP | HeatMap series | Uniform grid + continuous color scale | High |
| P2-HISTOGRAM | Histogram / density | Auto bin or user supplied | High |
| P2-BOXPLOT | Box & whisker | From statistical summary (Q1,Q2,Q3,outliers) | Medium |
| P2-AX-LINK | Linked axes across charts | Sync zoom/pan between models | High |
| P2-AX-BREAKS | Axis breaks | Skip large empty ranges with zigzag | Low |
| P2-AX-POLAR | Polar/Radar axes | Angle + radius for radar charts | Medium |
| P2-ANNOT-TEXT | Text & callout annotations | Anchored to data/pixel coords with arrows | High |
| P2-ANNOT-SHAPE | Shape annotations | Rectangle, ellipse, arrow overlays | Medium |
| P2-LEGEND-DOCK | Dockable/scroll legend | Layout improvements, pagination | High |
| P2-LEGEND-TREE | Hierarchical legend | Group series, expandable nodes | Medium |
| P2-EXPORT-SVG | SVG / vector export | Path serialization, scalable output | High |
| P2-EXPORT-PDF | PDF export | Via SkiaSharp.Extended or PdfSharp | Medium |
| P2-RESAMPLE-AGG | Aggregation (min/max/avg) | Precomputation pipeline for large datasets | High |
| P2-DATA-BIND | Observable binding | INotifyCollectionChanged bridge for MVVM | High |
| P2-DATA-PROVIDER | Data provider interface | Abstract data source (DB, file, stream) | Medium |
| P2-THEME-DYNAMIC | Dynamic themes | Runtime theme switching, custom palettes | High |
| P2-THEME-BUILTIN | Additional themes | Material, Office, Metro, High-contrast | Medium |

---
## 5. Phase 3 (Advanced Features)
| ID | Feature | Description | Complexity |
|----|---------|-------------|------------|
| P3-FIN-INDICATORS | Financial indicators | SMA, EMA, RSI, MACD, Bollinger Bands | High |
| P3-FIN-VOLUME | Volume profile | Price-volume distribution | High |
| P3-MATH-REGRESSION | Trend lines | Linear, polynomial, exponential regression | Medium |
| P3-MATH-STATS | Statistical overlays | Standard deviation bands, percentiles | Medium |
| P3-POLAR-FULL | Complete polar charts | Rose, wind rose, polar scatter | High |
| P3-3D-SURFACE | 3D Surface plots | Height maps with perspective | Very High |
| P3-WATERFALL | Waterfall series | Cumulative delta visualization | Medium |
| P3-SANKEY | Sankey diagrams | Flow visualization between nodes | High |
| P3-TREEMAP | TreeMap charts | Hierarchical area-based visualization | High |
| P3-ANIM | Animations | Appear / transition effects (optional) | Medium |
| P3-GPU | GPU accelerated path | Skia GPU / device contexts for massive datasets | Very High |
| P3-PLUGIN | Plugin discovery | Attribute-based loading for custom series | Medium |
| P3-LOCALE | Full culture support | Axis & tooltip formatting, RTL support | Medium |
| P3-REALTIME-OPT | Real-time optimizations | Circular buffers, incremental rendering | High |
| P3-DASHBOARD | Dashboard layout | Multiple charts with shared interactions | High |

---
## 6. Technical Tasks (Cross-Cutting)
| ID | Task | Description | Impact |
|----|------|-------------|--------|
| T-ARCH-AXREF | Refactor series→axis mapping | Each series holds AxisId(s) | Breaking |
| T-ARCH-ANNLAYER | Annotation layer insertion | After series, before overlay | Additive |
| T-ARCH-RESAMPLE | Resampler pipeline | ChartModel holds optional IResampler per series type | Additive |
| T-ARCH-SERVICES | Extract sub-services | IRenderInvalidationService, IInteractionService, ILayoutService | Refactor |
| T-ARCH-PIPELINE | Declarative render pipeline | Layer composition with z-order and invalidation | Major |
| T-ARCH-VIRTUALIZATION | Data virtualization | IObservable/Channel-based streaming | Major |
| T-PERF-PROF | Benchmark suite | BenchmarkDotNet project with scenarios | QA |
| T-PERF-CACHE | Geometry caching | Reuse SKPath objects, dirty rectangles | Performance |
| T-PERF-GPU | GPU rendering option | SkiaSharp GPU context integration | Performance |
| T-QA-VISUAL | Visual tests harness | Hash images per test, pixel difference tolerance | QA |
| T-QA-INTERACTION | Interaction testing | Simulate gestures and validate state | QA |
| T-QA-FUZZING | Fuzzing for robustness | Extreme values, NaN, infinities handling | QA |
| T-DEV-DOCS | Doc site scaffold | DocFX or markdown export with interactive gallery | Documentation |
| T-DEV-COOKBOOK | Usage cookbook | Real-time, finance, analytics scenarios | Documentation |
| T-PKG-NUGET | NuGet polish | README, icon, SourceLink, symbols | Distribution |
| T-CI-MATRIX | CI matrix | Windows + Linux (core libs), performance regression detection | CI/CD |
| T-ACCESSIBILITY | Accessibility support | ARIA-like metadata, keyboard navigation | Compliance |

---
## 7. Performance & Scalability Targets
- **Small datasets** (< 10K points): Render < 16ms (60 FPS)
- **Medium datasets** (10K - 100K points): Render < 33ms (30 FPS) with decimation
- **Large datasets** (100K - 1M points): Render < 100ms with aggressive decimation
- **Streaming data**: Handle 1K+ points/second with rolling window
- **Memory**: < 100MB for 1M point dataset after decimation

---
## 8. Status Tracker
### Phase 1 Progress
- [ ] P1-AX-MULTI
- [ ] P1-AX-LOG  
- [ ] P1-AX-CAT
- [ ] P1-ANN-LINE
- [ ] P1-ANN-RANGE
- [x] P1-EXPORT-PNG (In Progress)
- [ ] P1-RESAMPLE-LTTB
- [ ] P1-STREAM-APPEND
- [ ] P1-TOOLTIP-PIN
- [ ] P1-METRICS

### Technical Tasks Progress
- [ ] T-ARCH-AXREF
- [ ] T-ARCH-SERVICES
- [ ] T-PERF-PROF
- [ ] T-QA-VISUAL

---
## 9. Contribution Guidelines (Draft)
- Each feature: design note (Issue) → small PRs.
- Keep public API additions in separate commit for review clarity.
- Add/Update tests + docs snippet.
- No analyzer warnings introduced (TreatWarningsAsErrors).
- Performance features must include benchmarks.
- Breaking changes require deprecation period and migration guide.

---
## 10. Competitive Analysis Notes
**Strengths to maintain vs competitors:**
- Clean architecture with separated concerns (vs ScottPlot monolithic approach)
- MVVM-first design (vs OxyPlot's dated patterns)
- Modern async/reactive patterns (vs WinForms-era APIs)
- Multi-targeting without compromise

**Features to match/exceed:**
- Series variety (SciChart leads here)
- Real-time performance (ScottPlot's strength)
- Export options (OxyPlot comprehensive)
- Financial indicators (dedicated libs like StockCharts)

---
## 11. Next Immediate Steps
**Current focus**: P1-EXPORT-PNG (low coupling, high value)
**After PNG export**: Choose between T-ARCH-AXREF foundation or P1-AX-MULTI implementation

**Decision criteria**: 
- Architecture changes first if multiple features depend on them
- User-facing features first if architecture can evolve incrementally

---
*Last updated: 2025-01-27*
