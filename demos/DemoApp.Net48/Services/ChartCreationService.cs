using System;
using System.Collections.Generic;
using System.Linq;
using FastCharts.Core;
using FastCharts.Core.Axes;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Core.Themes.BuiltIn;
using DemoApp.Net48.Services.Abstractions;
using DemoApp.Net48.Constants;

namespace DemoApp.Net48.Services
{
    /// <summary>
    /// Service implementation for creating demo charts
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
                Theme = new DarkTheme(),
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
                Theme = new DarkTheme(),
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

        private static ChartModel BuildBarsChart()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Bar Chart");

            const int bucketCount = 10;
            var bucketTimes = Enumerable.Range(0, bucketCount)
                .Select(i => start.AddDays(i))
                .ToArray();

            var barsA = bucketTimes
                .Select((x, i) => new BarPoint(x.ToOADate(), (Math.Sin(i * 0.6) + 1.2) * 0.6))
                .ToArray();
            var barsB = bucketTimes
                .Select((x, i) => new BarPoint(x.ToOADate(), (Math.Cos(i * 0.6) + 1.2) * 0.5))
                .ToArray();

            model.AddSeries(new BarSeries(barsA)
            {
                Title = "Bars A",
                GroupCount = 2,
                GroupIndex = 0,
                FillOpacity = 0.7
            });
            model.AddSeries(new BarSeries(barsB)
            {
                Title = "Bars B",
                GroupCount = 2,
                GroupIndex = 1,
                FillOpacity = 0.7
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildStackedBarsChart()
        {
            var start = DateTime.Today.AddDays(-12);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Stacked Bars");

            const int bucketCount = 8;
            var bucketTimes = Enumerable.Range(0, bucketCount)
                .Select(i => start.AddDays(i * 1.5))
                .ToArray();

            var stackA = bucketTimes
                .Select((x, i) => new StackedBarPoint(x.ToOADate(), new[]
                {
                    (Math.Sin(i * 0.5) + 1.1) * 0.4,
                    (Math.Cos(i * 0.6) + 1.1) * 0.3,
                    (Math.Sin(i * 0.7) + 1.1) * 0.2
                }))
                .ToArray();

            var stackB = bucketTimes
                .Select((x, i) => new StackedBarPoint(x.ToOADate(), new[]
                {
                    (Math.Cos(i * 0.5) + 1.1) * 0.35,
                    (Math.Sin(i * 0.6) + 1.1) * 0.25,
                    (Math.Cos(i * 0.4) + 1.1) * 0.2
                }))
                .ToArray();

            model.AddSeries(new StackedBarSeries(stackA)
            {
                Title = "Stack A",
                GroupCount = 2,
                GroupIndex = 0,
                FillOpacity = 0.8
            });
            model.AddSeries(new StackedBarSeries(stackB)
            {
                Title = "Stack B",
                GroupCount = 2,
                GroupIndex = 1,
                FillOpacity = 0.8
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildOhlcChart()
        {
            var start = DateTime.Today.AddDays(-20);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "OHLC Chart");

            var random = new Random(123);
            const int pointCount = 60;
            double price = 100;
            var ohlcPoints = new OhlcPoint[pointCount];

            for (var i = 0; i < pointCount; i++)
            {
                var open = price;
                var change = (random.NextDouble() - 0.5) * 4;
                var close = open + change;
                var high = Math.Max(open, close) + random.NextDouble() * 2;
                var low = Math.Min(open, close) - random.NextDouble() * 2;

                price = close;
                ohlcPoints[i] = new OhlcPoint(start.AddDays(i).ToOADate(), open, high, low, close);
            }

            model.AddSeries(new OhlcSeries(ohlcPoints) { Title = "OHLC" });
            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildErrorBarChart()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Error Bars");

            const int pointCount = 20;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddDays(i * 0.5))
                .ToArray();

            var random = new Random(456);
            var errorPoints = timePoints.Select(x =>
            {
                var y = 50 + Math.Sin(x.DayOfYear * 0.2) * 10 + random.NextDouble() * 4;
                var error = 2 + random.NextDouble() * 2;
                return new ErrorBarPoint(x.ToOADate(), y, error, error * (0.5 + random.NextDouble() * 0.5));
            }).ToArray();

            model.AddSeries(new ErrorBarSeries(errorPoints) { Title = "ErrorBars" });
            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildMinimalLineChart()
        {
            var start = DateTime.Today.AddDays(-5);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Line Chart");

            const int pointCount = 60;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 2))
                .ToArray();

            var linePoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.2) * 5 + 20))
                .ToArray();

            model.AddSeries(new LineSeries(linePoints) { Title = "Line" });
            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildAreaOnly()
        {
            var start = DateTime.Today.AddDays(-7);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Area Chart");

            const int pointCount = 120;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i))
                .ToArray();

            var areaPoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), Math.Sin(i * 0.1) * 10 + 30))
                .ToArray();

            model.AddSeries(new AreaSeries(areaPoints)
            {
                Title = "Area Only",
                Baseline = 20,
                FillOpacity = 0.45
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildScatterOnly()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Scatter Plot");

            var random = new Random(789);
            const int pointCount = 80;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 1.2))
                .ToArray();

            var scatterPoints = timePoints
                .Select(x => new PointD(x.ToOADate(), 40 + Math.Sin(x.Ticks / 6e11) * 5 + random.NextDouble() * 3))
                .ToArray();

            model.AddSeries(new ScatterSeries(scatterPoints)
            {
                Title = "Scatter Only",
                MarkerSize = 5.5
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildStepLine()
        {
            var start = DateTime.Today.AddDays(-6);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Step Line");

            const int pointCount = 40;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddHours(i * 4))
                .ToArray();

            var stepPoints = timePoints
                .Select((x, i) => new PointD(x.ToOADate(), (i % 2 == 0 ? 10 : 20) + (i % 5 == 0 ? 5 : 0)))
                .ToArray();

            model.AddSeries(new StepLineSeries(stepPoints)
            {
                Title = "Step",
                Mode = StepMode.Before,
                StrokeThickness = 2
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildSingleBars()
        {
            var start = DateTime.Today.AddDays(-8);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Single Bars");

            const int pointCount = 12;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddDays(i))
                .ToArray();

            var barPoints = timePoints
                .Select((x, i) => new BarPoint(x.ToOADate(), Math.Sin(i * 0.4) * 8 + 15))
                .ToArray();

            model.AddSeries(new BarSeries(barPoints)
            {
                Title = "Bars",
                FillOpacity = 0.75
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildStacked100()
        {
            var start = DateTime.Today.AddDays(-10);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "Stacked 100%");

            const int pointCount = 10;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddDays(i))
                .ToArray();

            var random = new Random(321);
            var stackedPoints = timePoints.Select((x, i) =>
            {
                var a = random.NextDouble() * 1 + 0.2;
                var b = random.NextDouble() * 1 + 0.2;
                var c = random.NextDouble() * 1 + 0.2;
                var sum = a + b + c;
                return new StackedBarPoint(x.ToOADate(), new[] { a / sum, b / sum, c / sum });
            }).ToArray();

            model.AddSeries(new StackedBarSeries(stackedPoints)
            {
                Title = "Stacked 100%",
                FillOpacity = 0.85
            });

            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildOhlcWithErrorOverlay()
        {
            var start = DateTime.Today.AddDays(-15);
            var end = DateTime.Today.AddDays(1);
            var model = CreateBase(start, end, "OHLC + Error");

            var random = new Random(654);
            const int pointCount = 40;
            double price = 50;
            var ohlcPoints = new OhlcPoint[pointCount];
            var errorPoints = new ErrorBarPoint[pointCount];

            for (var i = 0; i < pointCount; i++)
            {
                var open = price;
                var change = (random.NextDouble() - 0.5) * 3;
                var close = open + change;
                var high = Math.Max(open, close) + random.NextDouble() * 1.5;
                var low = Math.Min(open, close) - random.NextDouble() * 1.5;

                price = close;
                var xValue = start.AddDays(i).ToOADate();
                ohlcPoints[i] = new OhlcPoint(xValue, open, high, low, close);

                var central = (open + close) * 0.5;
                var error = 0.5 + random.NextDouble();
                errorPoints[i] = new ErrorBarPoint(xValue, central, error, error * 0.7);
            }

            model.AddSeries(new OhlcSeries(ohlcPoints) { Title = "OHLC" });
            model.AddSeries(new ErrorBarSeries(errorPoints) { Title = "Err" });
            model.UpdateScales(800, 400);
            return model;
        }

        private static ChartModel BuildMultiSeriesTooltipShowcase()
        {
            var start = DateTime.Today.AddDays(-3);
            var end = DateTime.Today.AddDays(0.5);
            var model = CreateBase(start, end, "Multi-Series");

            const int pointCount = 300;
            var timePoints = Enumerable.Range(0, pointCount)
                .Select(i => start.AddMinutes(i * 15))
                .ToArray();

            var linePoints = timePoints
                .Select((x, i) => new PointD(
                    x.ToOADate(),
                    50 + Math.Sin(i * 0.15) * 10 + Math.Sin(i * 0.03) * 5))
                .ToArray();
            model.AddSeries(new LineSeries(linePoints)
            {
                Title = "Line A",
                StrokeThickness = 1.4
            });

            var areaPoints = timePoints
                .Where((_, i) => i % 3 == 0)
                .Select((x, i) => new PointD(x.ToOADate(), 40 + Math.Cos(i * 0.18) * 6))
                .ToArray();
            model.AddSeries(new AreaSeries(areaPoints)
            {
                Title = "Area B",
                Baseline = 35,
                FillOpacity = 0.35
            });

            var scatterPoints = timePoints
                .Where((_, i) => i % 20 == 0)
                .Select((x, i) => new PointD(x.ToOADate(), 55 + Math.Sin(i * 0.9) * 4))
                .ToArray();
            model.AddSeries(new ScatterSeries(scatterPoints)
            {
                Title = "Scatter C",
                MarkerSize = 4
            });

            var dayTimes = Enumerable.Range(0, 4)
                .Select(i => start.AddDays(i))
                .ToArray();
            var barPoints = dayTimes
                .Select((x, i) => new BarPoint(x.ToOADate(), 30 + i * 2 + Math.Sin(i) * 3))
                .ToArray();
            model.AddSeries(new BarSeries(barPoints)
            {
                Title = "Bars D",
                FillOpacity = 0.6
            });

            model.UpdateScales(800, 400);
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
                Theme = new DarkTheme(),
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