using System.Collections.ObjectModel;
using FastCharts.Core.DataBinding.Series;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Tests.DataBinding;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Hosts (the WPF FastChart) redraw on <see cref="ChartModel.Invalidated"/>; series data
/// mutations must raise it so streaming and bound series refresh without user input.
/// </summary>
public class ChartModelInvalidationTests
{
    [Fact]
    public void AddPointRaisesInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var series = new LineSeries();
        model.AddSeries(series);
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        series.AddPoint(new PointD(1, 1));

        // Assert
        raised.Should().Be(1);
    }

    [Fact]
    public void StreamingAppendRaisesInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var series = new StreamingLineSeries(maxPointCount: 10);
        model.AddSeries(series);
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        series.AppendPoint(new PointD(1, 1));

        // Assert
        raised.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BoundCollectionChangeRaisesInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var items = new ObservableCollection<object> { new Point2D { X = 0, Y = 0 } };
        using var series = new ObservableScatterSeries(items, nameof(Point2D.X), nameof(Point2D.Y));
        model.AddSeries(series);
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        items.Add(new Point2D { X = 1, Y = 1 });

        // Assert
        raised.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RemovedSeriesNoLongerRaisesInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var series = new LineSeries();
        model.AddSeries(series);
        model.Series.Remove(series);
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        series.AddPoint(new PointD(1, 1));

        // Assert
        raised.Should().Be(0);
    }

    [Fact]
    public void ClearedSeriesNoLongerRaiseInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var series = new LineSeries();
        model.AddSeries(series);
        model.ClearSeries();
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        series.AddPoint(new PointD(1, 1));

        // Assert
        raised.Should().Be(0);
    }

    [Fact]
    public void InvalidateRaisesInvalidated()
    {
        // Arrange
        using var model = new ChartModel();
        var raised = 0;
        model.Invalidated += (_, _) => raised++;

        // Act
        model.Invalidate();

        // Assert
        raised.Should().Be(1);
    }
}
