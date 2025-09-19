using System;
using System.Linq;

using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;

namespace DemoApp.Net48.ViewModels
{

    public sealed class MainViewModel
    {
        public ChartModel Chart { get; }

        public MainViewModel()
        {
            Chart = new ChartModel();

            var points = Enumerable.Range(0, 101)
                .Select(i => new PointD(i, Math.Sin(i * Math.PI / 20.0)))
                .ToArray();
            
            Chart.AddSeries(new LineSeries(points));
            Chart.UpdateScales(800, 400); // nominal size; real renderer will update on arrange
        }
    }
}
