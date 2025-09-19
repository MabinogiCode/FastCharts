using System.Collections.Generic;

namespace FastCharts.Core.Abstractions;

public interface ISeries<TPoint>
{
    IReadOnlyList<TPoint> Data { get; }
}
