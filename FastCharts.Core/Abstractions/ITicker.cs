using System.Collections.Generic;

using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions;

public interface ITicker<T>
{
    IReadOnlyList<T> GetTicks(FRange range, double approxStep);
}
