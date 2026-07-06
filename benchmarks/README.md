# FastCharts Benchmarks

BenchmarkDotNet suite backing the performance targets in `RoadMap.md` §7.

## Run

```bash
cd benchmarks/FastCharts.Benchmarks
dotnet run -c Release                     # all suites (takes a while)
dotnet run -c Release -- --filter *Lttb*  # one suite
dotnet run -c Release -- --list flat      # list available benchmarks
```

## Suites

| Suite | What it measures | Roadmap target |
|-------|------------------|----------------|
| `ResamplingBenchmarks` | LTTB decimation 10K/100K/1M points; per-frame `GetRenderData` cost (cold vs cache hit) | < 100 ms for 1M points |
| `StreamingBenchmarks` | Append throughput at steady state with FIFO trimming | 1K+ points/second |
| `IndicatorBenchmarks` | SMA / EMA / Bollinger over 10K/100K prices | interactive recompute |
| `PropertyPathResolverBenchmarks` | Data-binding property resolution: compiled cache vs reflection | n/a (regression guard) |

Benchmarks are compiled by CI (part of the solution) but never executed there —
run them locally on quiet hardware for meaningful numbers.
