using System.Collections.Generic;
using FastCharts.Core.Axes;
using FastCharts.Core.Series;

namespace FastCharts.Core.Abstractions;

public interface IChartModel
{
    IReadOnlyList<AxisBase> Axes { get; }
    IReadOnlyList<SeriesBase> Series { get; }
}
