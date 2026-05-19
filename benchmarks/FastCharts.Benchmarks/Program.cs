using BenchmarkDotNet.Running;

namespace FastCharts.Benchmarks;

/// <summary>
/// Entry point for the FastCharts micro-benchmarks. These validate the
/// performance targets in RoadMap section 7. Run optimized, for example:
/// <c>dotnet run -c Release --project benchmarks/FastCharts.Benchmarks</c>.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
