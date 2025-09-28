using System.Collections.Generic;

namespace FastCharts.Core.Abstractions;

public interface ISeries<out TPoint>
{
    IReadOnlyList<TPoint> Data { get; }
}
