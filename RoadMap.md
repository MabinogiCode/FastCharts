# FastCharts Roadmap

> Living document – will evolve as features are designed and delivered. Keep PRs small and reference the section / item IDs below.

## 0. Vision
High‑performance, extensible .NET charting library (Core + Skia renderer + WPF host) with clear API boundaries (data model / interaction / rendering) and analyzers guiding best usage. Target: real‑time, finance, analytical dashboards.

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
| P1-ANN-LINE | Annotation line | Horizontal/vertical line with label | IAnnotation start |
| P1-ANN-RANGE | Range highlight | Filled band between two values (X or Y) | Depends on annotation layer |
| P1-EXPORT-PNG | PNG export | Render to SKBitmap / stream | Reuse existing render path |
| P1-RESAMPLE-LTTB | Line LOD (LTTB) | Downsample large line series to viewport pixels | Plug via IResampler |
| P1-STREAM-APPEND | Streaming append API | Efficient append w/ rolling window | Add Append/Trim methods |
| P1-TOOLTIP-PIN | Pinned tooltips | User can lock multiple tooltips | Extend InteractionState |
| P1-METRICS | Render metrics overlay | FPS, point count | Behavior + overlay layer |

Deliverables for phase closure: feature flags, tests, basic docs updates.

---
## 4. Phase 2 (Mid Term: Breadth & UX)
| ID | Feature | Description |
|----|---------|-------------|
| P2-AX-LINK | Linked axes across charts | Sync zoom/pan between models |
| P2-AX-BREAKS | Axis breaks | Skip large empty ranges |
| P2-HEATMAP | HeatMap series | Uniform grid first |
| P2-HISTOGRAM | Histogram / density | Auto bin or user supplied |
| P2-BOXPLOT | Box & whisker | From statistical summary |
| P2-ANNOT-TEXT | Text & callout annotations | Anchored to data/pixel coords |
| P2-LEGEND-DOCK | Dockable/scroll legend | Layout improvements |
| P2-EXPORT-SVG | SVG / vector export | Path serialization |
| P2-RESAMPLE-AGG | Aggregation (min/max/avg) | Precomputation pipeline |
| P2-DATA-BIND | Observable binding | INotifyCollectionChanged bridge |

---
## 5. Phase 3 (Advanced / Nice-to-have)
| ID | Feature | Description |
|----|---------|-------------|
| P3-FIN-BANDS | Bollinger / moving averages | Derived series helpers |
| P3-POLAR | Polar / radar chart | Angle + radius axes |
| P3-PIE | Pie / donut | Non-cartesian renderer |
| P3-WATERFALL | Waterfall series | Stacked delta representation |
| P3-ANIM | Animations | Appear / transition (optional) |
| P3-GPU | GPU accelerated path | Skia GPU / device contexts |
| P3-PLUGIN | Plugin discovery | Attribute-based loading |
| P3-LOCALE | Full culture support | Axis & tooltip formatting |

---
## 6. Technical Tasks (Cross-Cutting)
| ID | Task | Description |
|----|------|-------------|
| T-ARCH-AXREF | Refactor series→axis mapping | Each series holds AxisId(s) |
| T-ARCH-ANNLAYER | Annotation layer insertion | After series, before overlay |
| T-ARCH-RESAMPLE | Resampler pipeline | ChartModel holds optional IResampler per series type |
| T-PERF-PROF | Benchmark suite | BenchmarkDotNet project |
| T-QA-VISUAL | Visual tests harness | Hash images per test |
| T-DEV-DOCS | Doc site scaffold | DocFX or markdown export |
| T-PKG-NUGET | NuGet polish | README, icon, SourceLink |
| T-CI-MATRIX | CI matrix | Windows + Linux (core libs) |

---
## 7. Status Tracker
(Will evolve – check off as PRs merge)
- [ ] P1-AX-MULTI
- [ ] P1-AX-LOG
- [ ] P1-ANN-LINE
- [ ] P1-ANN-RANGE
- [ ] P1-EXPORT-PNG
- [ ] P1-RESAMPLE-LTTB
- [ ] P1-STREAM-APPEND
- [ ] P1-TOOLTIP-PIN
- [ ] P1-METRICS

---
## 8. Contribution Guidelines (Draft)
- Each feature: design note (Issue) → small PRs.
- Keep public API additions in separate commit for review clarity.
- Add/Update tests + docs snippet.
- No analyzer warnings introduced (TreatWarningsAsErrors).

---
## 9. Next Immediate Step
(Default suggestion) Start with P1-EXPORT-PNG (low coupling) OR P1-AX-MULTI (foundation). Confirm choice and create corresponding issue before implementation.

---
*Last updated: 2025-09-26*
