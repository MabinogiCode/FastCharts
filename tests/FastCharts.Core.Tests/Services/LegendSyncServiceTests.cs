using System.Collections.Generic;
using System.Linq;
using FastCharts.Core.Axes;
using FastCharts.Core.Legend;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Services;
using Xunit;

namespace FastCharts.Core.Tests.Services
{
    public class LegendSyncServiceTests
    {
        [Fact]
        public void LegendSyncService_SyncLegendWithSeries_WithNullParameters_ExecutesWithoutException()
        {
            // Arrange
            var service = new LegendSyncService();

            // Act & Assert - should not throw
            var exception1 = Record.Exception(() => service.SyncLegendWithSeries(null, null));
            var exception2 = Record.Exception(() => service.SyncLegendWithSeries(new LegendModel(), null));
            var exception3 = Record.Exception(() => service.SyncLegendWithSeries(null, new List<SeriesBase>()));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
        }

        [Fact]
        public void LegendSyncService_SyncLegendWithSeries_WithValidParameters_ExecutesWithoutException()
        {
            // Arrange
            var service = new LegendSyncService();
            var legend = new LegendModel();
            var series = new List<SeriesBase>
            {
                new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) }) { Title = "Line 1" },
                new LineSeries(new[] { new PointD(0, 1), new PointD(1, 0) }) { Title = "Line 2" }
            };

            // Act & Assert - should not throw
            var exception = Record.Exception(() => service.SyncLegendWithSeries(legend, series));
            Assert.Null(exception);
        }

        [Fact]
        public void LegendSyncService_UpdateSeriesVisibility_WithNullParameters_ExecutesWithoutException()
        {
            // Arrange
            var service = new LegendSyncService();
            var legend = new LegendModel();
            var series = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) });

            // Act & Assert - should not throw
            var exception1 = Record.Exception(() => service.UpdateSeriesVisibility(null, null, true));
            var exception2 = Record.Exception(() => service.UpdateSeriesVisibility(legend, null, true));
            var exception3 = Record.Exception(() => service.UpdateSeriesVisibility(null, series, true));
            var exception4 = Record.Exception(() => service.UpdateSeriesVisibility(legend, series, true));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
            Assert.Null(exception4);
        }

        [Fact]
        public void LegendSyncService_AddSeriesToLegend_WithNullParameters_ExecutesWithoutException()
        {
            // Arrange
            var service = new LegendSyncService();
            var legend = new LegendModel();
            var series = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) });

            // Act & Assert - should not throw
            var exception1 = Record.Exception(() => service.AddSeriesToLegend(null, null));
            var exception2 = Record.Exception(() => service.AddSeriesToLegend(legend, null));
            var exception3 = Record.Exception(() => service.AddSeriesToLegend(null, series));
            var exception4 = Record.Exception(() => service.AddSeriesToLegend(legend, series));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
            Assert.Null(exception4);
        }

        [Fact]
        public void LegendSyncService_RemoveSeriesFromLegend_WithNullParameters_ExecutesWithoutException()
        {
            // Arrange
            var service = new LegendSyncService();
            var legend = new LegendModel();
            var series = new LineSeries(new[] { new PointD(0, 0), new PointD(1, 1) });

            // Act & Assert - should not throw
            var exception1 = Record.Exception(() => service.RemoveSeriesFromLegend(null, null));
            var exception2 = Record.Exception(() => service.RemoveSeriesFromLegend(legend, null));
            var exception3 = Record.Exception(() => service.RemoveSeriesFromLegend(null, series));
            var exception4 = Record.Exception(() => service.RemoveSeriesFromLegend(legend, series));

            Assert.Null(exception1);
            Assert.Null(exception2);
            Assert.Null(exception3);
            Assert.Null(exception4);
        }
    }
}