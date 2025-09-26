using System.Collections.Generic;

using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions;

public interface ITicker<T>
{
    IReadOnlyList<T> GetTicks(FRange range, double approxStep);
    IReadOnlyList<T> GetMinorTicks(FRange range, IReadOnlyList<T> majorTicks);
}
