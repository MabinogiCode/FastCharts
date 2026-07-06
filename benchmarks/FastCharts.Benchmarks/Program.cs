using BenchmarkDotNet.Running;

namespace FastCharts.Benchmarks
{
    /// <summary>
    /// Entry point: runs all benchmarks or a filtered subset.
    /// Usage: dotnet run -c Release [-- --filter *Lttb*]
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
