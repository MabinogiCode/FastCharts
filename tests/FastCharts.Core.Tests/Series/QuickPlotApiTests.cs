using System.Collections.Generic;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Extensions;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// KISS API tests: a nice curve from a plain Dictionary&lt;double, double&gt;
    /// must be a one-liner with zero decisions to make.
    /// </summary>
    public class QuickPlotApiTests
    {
        [Fact]
        public void AddSeries_FromDictionary_PlotsSortedCurveAndAutoFits()
        {
            using var model = new ChartModel();
            var data = new Dictionary<double, double>
            {
                [3] = 30,
                [1] = 10,
                [2] = 20
            };

            var series = model.AddSeries(data, "Mesures");

            model.Series.Should().Contain(series);
            series.Title.Should().Be("Mesures");
            series.Data.Select(p => p.X).Should().BeInAscendingOrder();
            series.Data.Should().HaveCount(3);

            // Auto-fit happened: axes cover the data
            model.XAxis.DataRange.Min.Should().BeLessThanOrEqualTo(1);
            model.XAxis.DataRange.Max.Should().BeGreaterThanOrEqualTo(3);
            model.YAxis.DataRange.Max.Should().BeGreaterThanOrEqualTo(30);
        }

        [Fact]
        public void AddSeries_FromValues_UsesIndexAsX()
        {
            using var model = new ChartModel();

            var series = model.AddSeries(new[] { 5.0, 7.0, 6.0 });

            series.Data.Should().HaveCount(3);
            series.Data[0].X.Should().Be(0);
            series.Data[2].X.Should().Be(2);
            series.Data[1].Y.Should().Be(7.0);
        }

        [Fact]
        public void LineSeries_FromDictionary_SortsByX()
        {
            var dict = new Dictionary<double, double> { [10] = 1, [5] = 2, [7.5] = 3 };

            var series = new LineSeries(dict);

            series.Data.Select(p => p.X).Should().ContainInOrder(5, 7.5, 10);
        }

        [Fact]
        public void ToLineSeries_FromDictionary_IsRenderable()
        {
            var dict = new Dictionary<double, double> { [0] = 1, [1] = 4 };

            var series = dict.ToLineSeries("Quick");

            series.Should().BeAssignableTo<SeriesBase>();
            series.Title.Should().Be("Quick");
            series.GetRenderData(800).Should().HaveCount(2);
        }

        [Fact]
        public void ToLineSeries_FromValues_IsRenderable()
        {
            var series = new[] { 1.0, 2.0, 3.0 }.ToLineSeries();

            series.Data.Should().HaveCount(3);
            series.IsEmpty.Should().BeFalse();
        }

        [Fact]
        public void LineSeries_FromNullEnumerables_AreEmptyNotBroken()
        {
            new LineSeries((IEnumerable<KeyValuePair<double, double>>)null!).IsEmpty.Should().BeTrue();
            new LineSeries((IEnumerable<double>)null!).IsEmpty.Should().BeTrue();
        }
    }
}
