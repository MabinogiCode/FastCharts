using System;
using System.Collections.Generic;

using FastCharts.Core;              // ChartModel
using FastCharts.Core.Axes;         // AxisExtensions
using FastCharts.Core.Primitives;   // PointD (si l'espace de noms diffère, adapte l'using)
using FastCharts.Core.Series;       // LineSeries
using FastCharts.Rendering.Skia;    // SkiaChartRenderer
using FastCharts.Tests.Helpers;

using SkiaSharp;

using Xunit;

namespace FastCharts.Tests
{
    public class SkiaChartRendererClipAndMappingTests
    {
        [Fact]
        public void PlotIsHardClippedMarginsUnaffected()
        {
            // Arrange
            var model = new ChartModel();
            // Ranges visibles simples
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            // Une série basique (diagonale), via le ctor par points
            var points = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(100, 100)
            };
            model.Series.Add(new LineSeries(points));

            const int W = 420, H = 300;
            using (var bmp = new SKBitmap(W, H, true))
            using (var canvas = new SKCanvas(bmp))
            {
                // Act
                var renderer = new SkiaChartRenderer();
                renderer.Render(model, canvas, W, H);
                canvas.Flush();

                // Les marges effectives proviennent du modèle (valeurs par défaut si non modifiées)
                var margins = model.PlotMargins;
                var left = (int)margins.Left;
                var top = (int)margins.Top;
                var right = (int)margins.Right;
                var bottom = (int)margins.Bottom;

                var plotW = Math.Max(0, W - (left + right));
                var plotH = Math.Max(0, H - (top + bottom));

                // Couleur "surface" = coin haut-gauche (en dehors du plot)
                var surfaceColor = bmp.GetPixel(1, 1);

                // Assert (marges doivent rester à la couleur surface)
                // marge gauche (milieu vertical)
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(Math.Max(1, left / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // marge droite
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(W - Math.Max(2, right / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // marge haute
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), Math.Max(1, top / 2)), surfaceColor));
                // marge basse
                Assert.True(SkiaTestHelper.SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), H - Math.Max(2, bottom / 2)), surfaceColor));
            }
        }

        [Fact]
        public void DiagonalMapsFromBottomLeftToTopRightInsidePlot()
        {
            // Arrange
            var model = new ChartModel();
            model.XAxis.SetVisibleRange(0, 100);
            model.YAxis.SetVisibleRange(0, 100);

            var points = new List<PointD>
            {
                new PointD(0, 0),
                new PointD(100, 100)
            };
            model.Series.Add(new LineSeries(points));

            const int W = 420, H = 300;
            using (var bmp = new SKBitmap(W, H, true))
            using (var canvas = new SKCanvas(bmp))
            {
                // Act
                var renderer = new SkiaChartRenderer();
                renderer.Render(model, canvas, W, H);
                canvas.Flush();

                var m = model.PlotMargins;
                var left = (int)m.Left;
                var top = (int)m.Top;
                var right = (int)m.Right;
                var bottom = (int)m.Bottom;

                var plotW = Math.Max(0, W - (left + right));
                var plotH = Math.Max(0, H - (top + bottom));

                // On récupère la couleur "plot" en plein centre du plot
                var plotCenter = bmp.GetPixel(left + plotW / 2, top + plotH / 2);

                // On s'attend à ce que la diagonale traverse :
                //  - proche du coin bas-gauche du plot
                //  - proche du coin haut-droit du plot
                // Pour être robustes à l'AA, on sonde un petit disque de rayon 2 px et
                // on vérifie que la couleur diffère nettement du fond du plot.
                Assert.True(SkiaTestHelper.ProbeNotColor(bmp, new System.Drawing.Point(left + 3, top + plotH - 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near bottom-left inside plot.");
                Assert.True(SkiaTestHelper.ProbeNotColor(bmp, new System.Drawing.Point(left + plotW - 3, top + 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near top-right inside plot.");
            }
        }
    }
}
