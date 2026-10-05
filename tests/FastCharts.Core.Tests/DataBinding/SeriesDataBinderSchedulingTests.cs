using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using FastCharts.Core.DataBinding;
using FastCharts.Core.Primitives;

using FluentAssertions;

using Xunit;

namespace FastCharts.Core.Tests.DataBinding;

/// <summary>
/// Throttled refreshes enumerate the bound collection, so they must run on the context the
/// binder was created on (the UI thread in apps), not on the thread pool.
/// </summary>
public class SeriesDataBinderSchedulingTests
{
    [Fact]
    public void ThrottledRefreshIsDeliveredOnTheCapturedContext()
    {
        // Arrange
        var previous = SynchronizationContext.Current;
        var context = new RecordingSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);
        using var applied = new ManualResetEventSlim();
        SeriesDataBinder binder;
        try
        {
            binder = new SeriesDataBinder(_ => applied.Set(), () => 0)
            {
                RefreshThrottle = TimeSpan.FromMilliseconds(10),
                XPath = nameof(Point2D.X),
                YPath = nameof(Point2D.Y)
            };
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }

        using (binder)
        {
            // Act
            binder.ItemsSource = new ObservableCollection<Point2D> { new Point2D { X = 1, Y = 2 } };

            // Assert
            applied.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
            context.PostCount.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void WithoutContextThrottledRefreshStillRuns()
    {
        // Arrange
        var previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        using var applied = new ManualResetEventSlim();
        SeriesDataBinder binder;
        try
        {
            binder = new SeriesDataBinder(_ => applied.Set(), () => 0)
            {
                RefreshThrottle = TimeSpan.FromMilliseconds(10),
                XPath = nameof(Point2D.X),
                YPath = nameof(Point2D.Y)
            };
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }

        using (binder)
        {
            // Act
            binder.ItemsSource = new List<Point2D> { new Point2D { X = 1, Y = 2 } };

            // Assert
            applied.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        }
    }
}
