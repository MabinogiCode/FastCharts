using System.Linq;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FluentAssertions;
using Xunit;

namespace FastCharts.Core.Tests.Series
{
    /// <summary>
    /// Tests for enhanced candlestick features (v1.3): bull/bear colors, volume
    /// </summary>
    public class OhlcCandlestickTests
    {
        [Fact]
        public void OhlcPoint_VolumeIsOptional()
        {
            var withoutVolume = new OhlcPoint(1, 10, 12, 9, 11);
            var withVolume = new OhlcPoint(1, 10, 12, 9, 11, 5000);

            withoutVolume.Volume.Should().BeNull();
            withVolume.Volume.Should().Be(5000);
        }

        [Fact]
        public void OhlcSeries_ColorAndVolumeDefaults()
        {
            var series = new OhlcSeries();

            series.BullColor.Should().BeNull("null means theme-derived color");
            series.BearColor.Should().BeNull();
            series.ShowVolume.Should().BeFalse();
            series.VolumePaneRatio.Should().Be(0.2);
            series.VolumeOpacity.Should().Be(0.35);
        }

        [Fact]
        public void OhlcSeries_CustomColors_AreStored()
        {
            var series = new OhlcSeries
            {
                BullColor = new ColorRgba(0, 200, 80),
                BearColor = new ColorRgba(220, 40, 40)
            };

            series.BullColor!.Value.G.Should().Be(200);
            series.BearColor!.Value.R.Should().Be(220);
        }

        [Fact]
        public void OhlcSeries_WithVolume_RangesUnaffected()
        {
            // Volume renders in an overlay band; it must not distort the price Y range
            var series = new OhlcSeries(new[]
            {
                new OhlcPoint(0, 10, 15, 8, 12, 1_000_000),
                new OhlcPoint(1, 12, 18, 11, 17, 2_000_000)
            });

            var yRange = series.GetYRange();

            yRange.Min.Should().Be(8);
            yRange.Max.Should().Be(18);
        }

        [Fact]
        public void OhlcSeries_RendersWithVolumeAndCustomColors()
        {
            using var model = new FastCharts.Core.ChartModel();
            var series = new OhlcSeries(Enumerable.Range(0, 30).Select(i =>
                new OhlcPoint(i, 100 + i, 105 + i, 95 + i, (i % 2 == 0) ? 103 + i : 97 + i, 1000 + (i * 10))))
            {
                ShowVolume = true,
                BullColor = new ColorRgba(0, 200, 80),
                BearColor = new ColorRgba(220, 40, 40)
            };
            model.AddSeries(series);

            var renderer = new FastCharts.Rendering.Skia.SkiaChartRenderer();
            using var stream = new System.IO.MemoryStream();

            // Full render pipeline must succeed with volume band enabled
            renderer.ExportPng(model, stream, 800, 600);

            stream.Length.Should().BeGreaterThan(0);
        }
    }
}
