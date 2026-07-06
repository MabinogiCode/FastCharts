```

BenchmarkDotNet v0.14.0, Ubuntu 24.04.4 LTS (Noble Numbat)
Intel Xeon Processor 2.80GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK 9.0.315
  [Host] : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX-512F+CD+BW+DQ+VL
  Dry    : .NET 8.0.28 (8.0.2826.26413), X64 RyuJIT AVX-512F+CD+BW+DQ+VL

Job=Dry  IterationCount=1  LaunchCount=1  
RunStrategy=ColdStart  UnrollFactor=1  WarmupCount=1  

```
| Method                               | Mean     | Error | Allocated |
|------------------------------------- |---------:|------:|----------:|
| &#39;Cached resolver, simple path x1000&#39; | 13.21 ms |    NA |  24.16 KB |
