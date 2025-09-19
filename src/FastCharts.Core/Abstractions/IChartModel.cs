using System.Collections.Generic;

namespace FastCharts.Core.Abstractions;

public interface IChartModel
{
    IReadOnlyList<object> Axes { get; }   // kept loose for now; we’ll tighten later
    IReadOnlyList<object> Series { get; } // idem
}
