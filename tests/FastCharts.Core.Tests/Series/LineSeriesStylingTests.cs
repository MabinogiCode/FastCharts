using System.Collections.Generic;
using FastCharts.Core;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Tests for v1.2 styling additions: markers, smoothing, quick-plot kinds.
    /// </summary>
    public class LineSeriesStylingTests
    {
        [Fact]
        public void LineSeries_MarkerDefaults_AreSensible()
        {
            var series = new LineSeries();

            series.ShowMarkers.Should().BeFalse();
            series.MarkerSize.Should().Be(5.0);
            series.MarkerShape.Should().Be(MarkerShape.Circle);
            series.Smoothing.Should().Be(LineSmoothing.None);
        }

        [Fact]
        public void ObservableLineSeries_InheritsMarkerProperties()
        {
            using var series = new FastCharts.Core.DataBinding.Series.ObservableLineSeries
            {
                ShowMarkers = true,
                MarkerShape = MarkerShape.Diamond
            };

            LineSeries asBase = series;
            asBase.ShowMarkers.Should().BeTrue();
            asBase.MarkerShape.Should().Be(MarkerShape.Diamond);
        }

        [Fact]
        public void MarkerShape_HasExtendedShapes()
        {
            // Extended in 1.2 — values must stay stable for the renderer contract
            ((int)MarkerShape.Circle).Should().Be(0);
            ((int)MarkerShape.Square).Should().Be(1);
            ((int)MarkerShape.Triangle).Should().Be(2);
            ((int)MarkerShape.Diamond).Should().Be(3);
            ((int)MarkerShape.Cross).Should().Be(4);
            ((int)MarkerShape.Plus).Should().Be(5);
        }

        [Theory]
        [InlineData(ChartKind.Line, typeof(LineSeries))]
        [InlineData(ChartKind.Area, typeof(AreaSeries))]
        [InlineData(ChartKind.Scatter, typeof(ScatterSeries))]
        [InlineData(ChartKind.Bar, typeof(BarSeries))]
        [InlineData(ChartKind.StepLine, typeof(StepLineSeries))]
        public void AddSeries_WithChartKind_CreatesRequestedSeriesType(ChartKind kind, System.Type expectedType)
        {
            using var model = new ChartModel();
            var data = new Dictionary<double, double> { [2] = 20, [1] = 10 };

            var series = model.AddSeries(data, kind, "S");

            series.Should().BeOfType(expectedType);
            series.Title.Should().Be("S");
            model.Series.Should().Contain(series);
        }

        [Fact]
        public void AddSeries_WithChartKind_SortsByX()
        {
            using var model = new ChartModel();
            var data = new Dictionary<double, double> { [3] = 3, [1] = 1, [2] = 2 };

            var series = (ScatterSeries)model.AddSeries(data, ChartKind.Scatter);

            series.Data[0].X.Should().Be(1);
            series.Data[2].X.Should().Be(3);
        }
    }
}
