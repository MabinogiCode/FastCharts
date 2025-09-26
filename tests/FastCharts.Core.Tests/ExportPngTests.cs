using System.IO;
using FastCharts.Core;
using FastCharts.Core.Primitives;
using FastCharts.Core.Series;
using FastCharts.Rendering.Skia;
using Xunit;

namespace FastCharts.Core.Tests
{
    public class ExportPngTests
    {
        [Fact]
        public void ExportPngWritesNonEmptyPngStream()
        {
            var model = new ChartModel();
            model.AddSeries(new LineSeries(new[]
            {
                new PointD(0,0), new PointD(1,1), new PointD(2,4), new PointD(3,9)
            }) { Title = "Parabola" });
            model.UpdateScales(400, 300);
            var r = new SkiaChartRenderer();
            using var ms = new MemoryStream();
            r.ExportPng(model, ms, 400, 300);
            Assert.True(ms.Length > 100, $"PNG length too small: {ms.Length}");
            ms.Position = 0;
            var sig = new byte[8];
            int read = ms.Read(sig, 0, 8);
            Assert.Equal(8, read);
            Assert.Equal(0x89, sig[0]);
            Assert.Equal((byte)'P', sig[1]);
            Assert.Equal((byte)'N', sig[2]);
            Assert.Equal((byte)'G', sig[3]);
        }
    }
}
