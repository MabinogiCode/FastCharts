using System.Collections.Generic;

using FastCharts.Core.Axes;
using FastCharts.Core.Interaction;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;

namespace FastCharts.Core.Abstractions;

/// <summary>
/// Read-mostly view of a chart model: enough for interaction behaviors to
/// inspect axes and series and to update interaction state, without depending
/// on the concrete <see cref="ChartModel"/> implementation.
/// </summary>
public interface IChartModel
{
    /// <summary>Gets the chart axes.</summary>
    IReadOnlyList<AxisBase> Axes { get; }

    /// <summary>Gets the chart series.</summary>
    IReadOnlyList<SeriesBase> Series { get; }

    /// <summary>Gets the primary X axis.</summary>
    IAxis<double> XAxis { get; }

    /// <summary>Gets the primary Y axis.</summary>
    IAxis<double> YAxis { get; }

    /// <summary>Gets the viewport describing the visible data ranges.</summary>
    IViewport Viewport { get; }

    /// <summary>Gets the plot margins.</summary>
    Margins PlotMargins { get; }

    /// <summary>Gets or sets the current interaction state.</summary>
    InteractionState? InteractionState { get; set; }
}
