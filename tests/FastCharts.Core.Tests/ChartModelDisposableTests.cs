using System;
using System.Collections.Specialized;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Interaction;
using FastCharts.Core.Series;
using FastCharts.Core.Primitives;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests;

/// <summary>
/// Tests validating proper resource management and IDisposable implementation of ChartModel
/// </summary>
public class ChartModelDisposableTests
{
    [Fact]
    public void ChartModelShouldImplementIDisposable()
    {
        // Arrange & Act
        var chartModel = new ChartModel();

        // Assert
        chartModel.Should().BeAssignableTo<IDisposable>();
    }

    [Fact]
    public void DisposeShouldUnsubscribeFromCollectionChanged()
    {
        // Arrange
        var chartModel = new ChartModel();

        // Hack to test: Replace Legend with a mock to count sync calls
        var originalLegend = chartModel.Legend;

        // Add a series to trigger initial sync
        var series = new LineSeries(new[] { new PointD(0, 0) });
        chartModel.AddSeries(series);

        // Get initial legend item count
        var initialItemCount = originalLegend.Items?.Count ?? 0;

        // Act - Dispose the chart model
        chartModel.Dispose();

        // Add another series after disposal
        var secondSeries = new LineSeries(new[] { new PointD(1, 1) }) { Title = "After Dispose" };
        chartModel.Series.Add(secondSeries); // Direct add to bypass AddSeries method

        // Assert - Legend should not sync after disposal
        var finalItemCount = originalLegend.Items?.Count ?? 0;
        finalItemCount.Should().Be(initialItemCount, "Legend should not sync after disposal");
    }

    [Fact]
    public void DisposeShouldBeIdempotent()
    {
        // Arrange
        var chartModel = new ChartModel();

        // Act - Call Dispose multiple times
        chartModel.Dispose();
        chartModel.Dispose();
        chartModel.Dispose();

        // Assert - Should not throw or cause issues
        chartModel.Series.Should().BeEmpty();
        chartModel.Behaviors.Should().BeEmpty();
    }

    [Fact]
    public void DisposeShouldClearCollections()
    {
        // Arrange
        var chartModel = new ChartModel();
        chartModel.AddSeries(new LineSeries(new[] { new PointD(0, 0) }));
        chartModel.Behaviors.Add(new TestBehavior());

        // Verify collections have items
        chartModel.Series.Should().HaveCount(1);
        chartModel.Behaviors.Should().HaveCount(1);

        // Act
        chartModel.Dispose();

        // Assert
        chartModel.Series.Should().BeEmpty();
        chartModel.Behaviors.Should().BeEmpty();
    }

    [Fact]
    public void DisposeShouldDisposeDisposableBehaviors()
    {
        // Arrange
        var chartModel = new ChartModel();
        var disposableBehavior = new DisposableTestBehavior();
        var nonDisposableBehavior = new TestBehavior();

        chartModel.Behaviors.Add(disposableBehavior);
        chartModel.Behaviors.Add(nonDisposableBehavior);

        disposableBehavior.IsDisposed.Should().BeFalse();

        // Act
        chartModel.Dispose();

        // Assert
        disposableBehavior.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void OnSeriesCollectionChangedShouldIgnoreEventsAfterDisposal()
    {
        // Arrange
        var chartModel = new ChartModel();
        var initialLegendItemCount = chartModel.Legend.Items?.Count ?? 0;

        // Act
        chartModel.Dispose();

        // Try to trigger the event handler after disposal
        chartModel.Series.Add(new LineSeries(new[] { new PointD(0, 0) }));

        // Assert - Legend should not have been updated
        var finalLegendItemCount = chartModel.Legend.Items?.Count ?? 0;
        finalLegendItemCount.Should().Be(initialLegendItemCount);
    }

    [Fact]
    public void ChartModelShouldStillFunctionAfterDisposal()
    {
        // Arrange
        var chartModel = new ChartModel();

        // Act
        chartModel.Dispose();

        // Assert - Basic properties should still work
        chartModel.Invoking(x => x.UpdateScales(800, 600)).Should().NotThrow();
        chartModel.Invoking(x => x.AutoFitDataRange()).Should().NotThrow();
        chartModel.Theme.Should().NotBeNull();
        chartModel.XAxis.Should().NotBeNull();
        chartModel.YAxis.Should().NotBeNull();
    }

    [Fact]
    public void UsingStatementShouldProperlydDisposeChartModel()
    {
        // Arrange
        DisposableTestBehavior? behavior = null;

        // Act
        using (var chartModel = new ChartModel())
        {
            behavior = new DisposableTestBehavior();
            chartModel.Behaviors.Add(behavior);
            behavior.IsDisposed.Should().BeFalse();
        } // Dispose called automatically here

        // Assert
        behavior.IsDisposed.Should().BeTrue();
    }
}