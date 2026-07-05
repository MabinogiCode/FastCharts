using System;
using System.Collections.ObjectModel;
using System.Threading;
using FastCharts.Core;
using FastCharts.Core.DataBinding;
using FastCharts.Core.DataBinding.Series;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.DataBinding
{
    /// <summary>
    /// End-to-end MVVM tests: observable series must be real chart series
    /// (addable to ChartModel, visible to renderers and range calculation).
    /// </summary>
    public class ObservableSeriesChartIntegrationTests
    {
        [Fact]
        public void ObservableLineSeries_IsARenderableSeries()
        {
            var series = new ObservableLineSeries();

            series.Should().BeAssignableTo<SeriesBase>();
            series.Should().BeAssignableTo<LineSeries>();
        }

        [Fact]
        public void ObservableScatterSeries_IsARenderableSeries()
        {
            new ObservableScatterSeries().Should().BeAssignableTo<ScatterSeries>();
        }

        [Fact]
        public void ObservableBarSeries_IsARenderableSeries()
        {
            new ObservableBarSeries().Should().BeAssignableTo<BarSeries>();
        }

        [Fact]
        public void ObservableLineSeries_CanBeAddedToChartModel_AndDrivesDataRange()
        {
            using var model = new ChartModel();
            var source = new ObservableCollection<SensorReading>
            {
                new() { Time = DateTime.FromOADate(10), Temperature = 5.0 },
                new() { Time = DateTime.FromOADate(20), Temperature = 15.0 }
            };

            using var series = new ObservableLineSeries(source, nameof(SensorReading.Time), nameof(SensorReading.Temperature));
            model.AddSeries(series);

            model.Series.Should().Contain(series);
            model.YAxis.DataRange.Min.Should().BeLessThanOrEqualTo(5.0);
            model.YAxis.DataRange.Max.Should().BeGreaterThanOrEqualTo(15.0);
        }

        [Fact]
        public void ObservableLineSeries_CollectionChange_UpdatesRenderData_WithoutManualRefresh()
        {
            var source = new ObservableCollection<SensorReading>
            {
                new() { Time = DateTime.FromOADate(1), Temperature = 1.0 }
            };

            using var series = new ObservableLineSeries(source, nameof(SensorReading.Time), nameof(SensorReading.Temperature));

            // Default RefreshThrottle is zero => synchronous refresh on collection change
            source.Add(new SensorReading { Time = DateTime.FromOADate(2), Temperature = 2.0 });

            series.Data.Should().HaveCount(2);
            series.GetRenderData(800).Should().HaveCount(2);
        }

        [Fact]
        public void ObservableLineSeries_ItemAddedAfterSubscription_IsTrackedForPropertyChanges()
        {
            var source = new ObservableCollection<ObservableSensorReading>
            {
                new() { Time = DateTime.FromOADate(1), Temperature = 1.0 }
            };

            using var series = new ObservableLineSeries(source, nameof(ObservableSensorReading.Time), nameof(ObservableSensorReading.Temperature));

            var lateItem = new ObservableSensorReading { Time = DateTime.FromOADate(2), Temperature = 2.0 };
            source.Add(lateItem);

            // Mutating the late-added item must trigger a rebind
            lateItem.Temperature = 42.0;

            series.Data.Should().HaveCount(2);
            series.Data[1].Y.Should().Be(42.0);
        }

        [Fact]
        public void ObservableLineSeries_RemovedItem_StopsTriggeringUpdates()
        {
            var item = new ObservableSensorReading { Time = DateTime.FromOADate(1), Temperature = 1.0 };
            var source = new ObservableCollection<ObservableSensorReading> { item };

            using var series = new ObservableLineSeries(source, nameof(ObservableSensorReading.Time), nameof(ObservableSensorReading.Temperature));

            source.Remove(item);
            var updates = 0;
            series.DataBindingUpdated += (_, _) => updates++;

            item.Temperature = 99.0; // Must not trigger anything anymore

            updates.Should().Be(0);
            series.Data.Should().BeEmpty();
        }

        [Fact]
        public void ObservableLineSeries_WithThrottle_CoalescesBurstsIntoSingleRefresh()
        {
            var source = new ObservableCollection<SensorReading>();
            using var series = new ObservableLineSeries(source, nameof(SensorReading.Time), nameof(SensorReading.Temperature))
            {
                RefreshThrottle = TimeSpan.FromMilliseconds(50)
            };

            var updates = 0;
            series.DataBindingUpdated += (_, _) => Interlocked.Increment(ref updates);

            for (var i = 0; i < 10; i++)
            {
                source.Add(new SensorReading { Time = DateTime.FromOADate(i), Temperature = i });
            }

            // Wait past the throttle window for the coalesced refresh to land
            SpinWait.SpinUntil(() => Volatile.Read(ref updates) > 0, TimeSpan.FromSeconds(2)).Should().BeTrue();

            Volatile.Read(ref updates).Should().Be(1, "10 rapid adds within the throttle window must coalesce into one refresh");
            series.Data.Should().HaveCount(10);
        }

        [Fact]
        public void ObservableLineSeries_AutoRefreshDisabled_DoesNotUpdateUntilManualRefresh()
        {
            var source = new ObservableCollection<SensorReading>
            {
                new() { Time = DateTime.FromOADate(1), Temperature = 1.0 }
            };

            using var series = new ObservableLineSeries(source, nameof(SensorReading.Time), nameof(SensorReading.Temperature))
            {
                AutoRefresh = false
            };

            source.Add(new SensorReading { Time = DateTime.FromOADate(2), Temperature = 2.0 });
            series.Data.Should().HaveCount(1);

            series.RefreshData();
            series.Data.Should().HaveCount(2);
        }
    }
}
