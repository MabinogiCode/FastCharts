using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using DemoApp.Net8.Services.Abstractions;

namespace DemoApp.Net8.Services
{
    /// <summary>
    /// Service implementation for creating demo charts with .NET 8 support
    /// </summary>
    public sealed class ChartCreationService : IChartCreationService
    {
        /// <summary>
        /// Creates all demo charts
        /// </summary>
        /// <returns>Collection of demo charts</returns>
        public IEnumerable<ChartModel> CreateDemoCharts()
        {
            yield return BuildMixedChart();
            yield return BuildBarsChart();
            yield return BuildStackedBarsChart();
            yield return BuildOhlcChart();
            yield return BuildErrorBarChart();
            yield return BuildMinimalLineChart();
            yield return BuildAreaOnly();
            yield return BuildScatterOnly();
            yield return BuildStepLine();
            yield return BuildSingleBars();
            yield return BuildStacked100();
            yield return BuildOhlcWithErrorOverlay();
            yield return BuildMultiSeriesTooltipShowcase();
            yield return BuildRealtimeChart();
            yield return BuildLogYAxisGrowth();
            yield return BuildLogLogScatter();
            yield return BuildDualYAxisLogSecondary();
        }

        /// <summary>
        /// Creates a chart with random data
        /// </summary>
        /// <param name="pointCount">Number of data points</param>
        /// <returns>Chart with random data</returns>
        public ChartModel CreateRandomChart(int pointCount = 50)
        {
            var random = new Random();
            var points = Enumerable.Range(0, pointCount)
                .Select(i => new PointD(i, random.NextDouble() * 100))
                .ToArray();

            var model = CreateBaseNumeric("Random Chart");
            model.AddSeries(new LineSeries(points)
            {
                Title = "Random Data",
                StrokeThickness = 2.0
            });

            return model;
        }

        private static ChartModel CreateBase(DateTime start, DateTime end, string title)
        {
            var model = new ChartModel 
            { 
                Theme = new LightTheme(), 
                Title = title 
            };
            
            var dateTimeAxis = new DateTimeAxis();
            dateTimeAxis.SetVisibleRange(start, end);
            model.ReplaceXAxis(dateTimeAxis);
            
            return model;
        }

        private static ChartModel CreateBaseNumeric(string title)
        {
            return new ChartModel 
            { 
                Theme = new LightTheme(), 
                Title = title 
            };
        }

        private static ChartModel BuildMixedChart()
        {
            var start = DateTime.Today.AddDays(-14);
            var end = DateTime.Today.AddDays(1);
            const int pointCount = 201;
            
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 1.5))
                .ToArray();
            
            var model = CreateBase(start, end, "Mixed Chart");

            var areaPoints = timePoints
                .Select(x => new PointD(x.ToOADate(), Math.Max(0, Math.Sin(x.Ticks / 1e10))))
                .ToArray();
            model.AddSeries(new AreaSeries(areaPoints) 
            { 
                Title = "Area", 
                Baseline = 0.0, 
                FillOpacity = 0.35 
            });

            var linePoints = timePoints
                .Select(x => new PointD(x.ToOADate(), Math.Cos(x.Ticks / 1e10)))
                .ToArray();
            model.AddSeries(new LineSeries(linePoints) 
            { 
                Title = "Line", 
                StrokeThickness = 1.8 
            });

            var scatterPoints = timePoints
                .Where((x, i) => i % 16 == 0)
                .Select(x => new PointD(
                    x.ToOADate(), 
                    Math.Sin(x.Ticks / 1e10) + (Math.Sin(3 * (x.Ticks / 1e10)) * 0.05)))
                .ToArray();
            model.AddSeries(new ScatterSeries(scatterPoints) 
            { 
                Title = "Scatter", 
                MarkerSize = 4.0 
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildRealtimeChart()
        {
            var model = CreateBaseNumeric("Realtime Data");
            var start = DateTime.Now.AddMinutes(-5);
            var end = DateTime.Now.AddMinutes(1);

            var dateTimeAxis = new DateTimeAxis();
            dateTimeAxis.SetVisibleRange(start, end);
            model.ReplaceXAxis(dateTimeAxis);

            var random = new Random();
            var points = Enumerable.Range(0, 100)
                .Select(i => new PointD(
                    start.AddSeconds(i * 3).ToOADate(),
                    50 + Math.Sin(i * 0.1) * 20 + random.NextDouble() * 10
                ))
                .ToArray();

            model.AddSeries(new LineSeries(points)
            {
                Title = "Realtime Data",
                StrokeThickness = 2.0
            });

            return model;
        }

        private static ChartModel BuildBarsChart()
        {
            var model = CreateBaseNumeric("Bar Chart");
            var random = new Random();
            var points = Enumerable.Range(0, 10)
                .Select(i => new BarPoint(i, random.Next(10, 100)))
                .ToArray();
            model.AddSeries(new BarSeries(points) { Title = "Bars" });
            return model;
        }

        private static ChartModel BuildStackedBarsChart()
        {
            var model = CreateBaseNumeric("Stacked Bars");
            var points = Enumerable.Range(0, 5)
                .Select(i => new StackedBarPoint(i, new double[] { 10 + i * 5, 15 + i * 3, 8 + i * 2 }))
                .ToArray();
            model.AddSeries(new StackedBarSeries(points) { Title = "Stacked" });
            return model;
        }

        private static ChartModel BuildOhlcChart()
        {
            var model = CreateBaseNumeric("OHLC Chart");
            var random = new Random();
            var points = Enumerable.Range(0, 20)
                .Select(i =>
                {
                    var open = 50 + random.NextDouble() * 20;
                    var close = open + (random.NextDouble() - 0.5) * 10;
                    var high = Math.Max(open, close) + random.NextDouble() * 5;
                    var low = Math.Min(open, close) - random.NextDouble() * 5;
                    return new OhlcPoint(i, open, high, low, close);
                })
                .ToArray();
            model.AddSeries(new OhlcSeries(points) { Title = "OHLC" });
            return model;
        }

        private static ChartModel BuildErrorBarChart()
        {
            var model = CreateBaseNumeric("Error Bars");
            var points = Enumerable.Range(0, 10)
                .Select(i => new ErrorBarPoint(i, 50 + i * 5, 5))
                .ToArray();
            model.AddSeries(new ErrorBarSeries(points) { Title = "Error Bars" });
            return model;
        }

        private static ChartModel BuildMinimalLineChart()
        {
            var model = CreateBaseNumeric("Line Chart");
            var points = Enumerable.Range(0, 20)
                .Select(i => new PointD(i, Math.Sin(i * 0.3) * 50 + 50))
                .ToArray();
            model.AddSeries(new LineSeries(points) { Title = "Sine Wave" });
            return model;
        }

        private static ChartModel BuildAreaOnly()
        {
            var model = CreateBaseNumeric("Area Chart");
            var points = Enumerable.Range(0, 30)
                .Select(i => new PointD(i, Math.Abs(Math.Cos(i * 0.2) * 40) + 10))
                .ToArray();
            model.AddSeries(new AreaSeries(points) { Title = "Area", FillOpacity = 0.7 });
            return model;
        }

        private static ChartModel BuildScatterOnly()
        {
            var model = CreateBaseNumeric("Scatter Plot");
            var random = new Random();
            var points = Enumerable.Range(0, 50)
                .Select(i => new PointD(random.NextDouble() * 100, random.NextDouble() * 100))
                .ToArray();
            model.AddSeries(new ScatterSeries(points) { Title = "Random Points", MarkerSize = 6 });
            return model;
        }

        private static ChartModel BuildStepLine()
        {
            var model = CreateBaseNumeric("Step Line");
            var points = Enumerable.Range(0, 15)
                .Select(i => new PointD(i, Math.Floor(i / 3.0) * 20 + 10))
                .ToArray();
            model.AddSeries(new StepLineSeries(points) { Title = "Steps" });
            return model;
        }

        private static ChartModel BuildSingleBars()
        {
            var model = CreateBaseNumeric("Single Bars");
            var points = new[] { new BarPoint(0, 45), new BarPoint(1, 67), new BarPoint(2, 23) };
            model.AddSeries(new BarSeries(points) { Title = "Simple Bars" });
            return model;
        }

        private static ChartModel BuildStacked100()
        {
            var model = CreateBaseNumeric("Stacked 100%");
            var points = Enumerable.Range(0, 4)
                .Select(i => new StackedBarPoint(i, new double[] { 25, 35, 40 }))
                .ToArray();
            model.AddSeries(new StackedBarSeries(points) { Title = "100% Stack" });
            return model;
        }

        private static ChartModel BuildOhlcWithErrorOverlay()
        {
            var model = CreateBaseNumeric("OHLC + Error");
            var random = new Random();

            var ohlcPoints = Enumerable.Range(0, 10)
                .Select(i =>
                {
                    var open = 50 + random.NextDouble() * 20;
                    var close = open + (random.NextDouble() - 0.5) * 10;
                    var high = Math.Max(open, close) + random.NextDouble() * 5;
                    var low = Math.Min(open, close) - random.NextDouble() * 5;
                    return new OhlcPoint(i, open, high, low, close);
                })
                .ToArray();
            model.AddSeries(new OhlcSeries(ohlcPoints) { Title = "OHLC" });

            var errorPoints = Enumerable.Range(0, 10)
                .Select(i => new ErrorBarPoint(i, 70 + i * 2, 3))
                .ToArray();
            model.AddSeries(new ErrorBarSeries(errorPoints) { Title = "Errors" });

            return model;
        }

        private static ChartModel BuildMultiSeriesTooltipShowcase()
        {
            var model = CreateBaseNumeric("Multi-Series");

            for (var s = 0; s < 3; s++)
            {
                var points = Enumerable.Range(0, 20)
                    .Select(i => new PointD(i, Math.Sin(i * 0.3 + s) * 30 + 50 + s * 20))
                    .ToArray();
                model.AddSeries(new LineSeries(points) { Title = $"Series {s + 1}" });
            }

            return model;
        }

        private static ChartModel BuildLogYAxisGrowth()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(0.5);
            var model = CreateBase(start, end, "Growth (Y Log)");

            var logY = new LogarithmicAxis { LogBase = 10.0 };
            logY.SetVisibleRange(1, 10000);
            model.ReplaceYAxis(logY);

            const int pointCount = 120;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 1.2))
                .ToArray();
            
            var exponentialPoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 30.0)))
                .ToArray();
            
            model.AddSeries(new LineSeries(exponentialPoints) 
            { 
                Title = "Exp", 
                StrokeThickness = 1.6 
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildLogLogScatter()
        {
            var model = new ChartModel 
            { 
                Theme = new LightTheme(), 
                Title = "Scatter Log-Log" 
            };

            var logX = new LogarithmicAxis { LogBase = 10.0 };
            logX.SetVisibleRange(1, 1000);
            var logY = new LogarithmicAxis { LogBase = 10.0 };
            logY.SetVisibleRange(1, 100000);
            
            model.ReplaceXAxis(logX);
            model.ReplaceYAxis(logY);

            const int pointCount = 60;
            var xValues = Enumerable.Range(0, pointCount)
                .Select(i => 1.0 + i * (999.0 / 59.0))
                .ToArray();
            
            var scatterPoints = xValues
                .Select(x => new PointD(x, Math.Pow(x, 2.5)))
                .ToArray();
            
            model.AddSeries(new ScatterSeries(scatterPoints) 
            { 
                Title = "x^2.5", 
                MarkerSize = 4 
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildDualYAxisLogSecondary()
        {
            var start = DateTime.Today.AddDays(-5);
            var end = DateTime.Today.AddDays(0.2);
            var model = CreateBase(start, end, "Dual Y (secondary log)");

            const int pointCount = 80;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 1.5))
                .ToArray();

            var linearPoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), 50 + Math.Sin(i * 0.2) * 10))
                .ToArray();
            model.AddSeries(new LineSeries(linearPoints) 
            { 
                Title = "Lin", 
                StrokeThickness = 1.4, 
                YAxisIndex = 0 
            });

            model.EnsureSecondaryYAxis();
            var logAxis = new LogarithmicAxis { LogBase = 10.0 };
            logAxis.SetVisibleRange(1, 10000);
            model.ReplaceSecondaryYAxis(logAxis);

            var exponentialPoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), Math.Pow(10, i / 20.0)))
                .ToArray();
            model.AddSeries(new LineSeries(exponentialPoints) 
            { 
                Title = "Exp", 
                StrokeThickness = 1.6, 
                YAxisIndex = 1 
            });

            model.UpdateScales(800, 400);
            return model;
        }
    }
}