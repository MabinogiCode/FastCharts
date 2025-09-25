using System;
using System.Collections.Generic;
using Xunit;
using SkiaSharp;
using FastCharts.Core;              // ChartModel
using FastCharts.Core.Series;       // LineSeries
using FastCharts.Rendering.Skia;    // SkiaChartRenderer
using FastCharts.Core.Primitives;   // PointD (si l'espace de noms diffère, adapte l'using)
using FastCharts.Core.Axes;         // AxisExtensions

namespace FastCharts.Tests
{
    public class SkiaChartRendererClipAndMappingTests
    {
        // Tolérance de comparaison (AA, épaisseur trait, etc.)
        private static bool SameColor(SKColor a, SKColor b, byte tol = 6)
        {
            return Math.Abs(a.Red - b.Red) <= tol
                && Math.Abs(a.Green - b.Green) <= tol
                && Math.Abs(a.Blue - b.Blue) <= tol
                && Math.Abs(a.Alpha - b.Alpha) <= tol;
        }

        [Fact]
        public void Plot_Is_Hard_Clipped_Margins_Unaffected()
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
                int left = (int)margins.Left;
                int top = (int)margins.Top;
                int right = (int)margins.Right;
                int bottom = (int)margins.Bottom;

                int plotW = Math.Max(0, W - (left + right));
                int plotH = Math.Max(0, H - (top + bottom));

                // Couleur "surface" = coin haut-gauche (en dehors du plot)
                var surfaceColor = bmp.GetPixel(1, 1);

                // Assert (marges doivent rester à la couleur surface)
                // marge gauche (milieu vertical)
                Assert.True(SameColor(bmp.GetPixel(Math.Max(1, left / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // marge droite
                Assert.True(SameColor(bmp.GetPixel(W - Math.Max(2, right / 2), top + Math.Max(1, plotH / 2)), surfaceColor));
                // marge haute
                Assert.True(SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), Math.Max(1, top / 2)), surfaceColor));
                // marge basse
                Assert.True(SameColor(bmp.GetPixel(left + Math.Max(1, plotW / 2), H - Math.Max(2, bottom / 2)), surfaceColor));
            }
        }

        [Fact]
        public void Diagonal_Maps_From_BottomLeft_To_TopRight_Inside_Plot()
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
                int left = (int)m.Left;
                int top = (int)m.Top;
                int right = (int)m.Right;
                int bottom = (int)m.Bottom;

                int plotW = Math.Max(0, W - (left + right));
                int plotH = Math.Max(0, H - (top + bottom));

                // On récupère la couleur "plot" en plein centre du plot
                var plotCenter = bmp.GetPixel(left + plotW / 2, top + plotH / 2);

                // On s'attend à ce que la diagonale traverse :
                //  - proche du coin bas-gauche du plot
                //  - proche du coin haut-droit du plot
                // Pour être robustes à l'AA, on sonde un petit disque de rayon 2 px et
                // on vérifie que la couleur diffère nettement du fond du plot.
                Assert.True(ProbeNotColor(bmp, new System.Drawing.Point(left + 3, top + plotH - 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near bottom-left inside plot.");
                Assert.True(ProbeNotColor(bmp, new System.Drawing.Point(left + plotW - 3, top + 3), plotCenter, 2),
                    "Expected a drawn pixel (series/grid) near top-right inside plot.");
            }
        }

        private static bool ProbeNotColor(SKBitmap bmp, System.Drawing.Point center, SKColor forbidden, int radius)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int y = center.Y + dy;
                if (y < 0 || y >= bmp.Height) continue;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    int x = center.X + dx;
                    if (x < 0 || x >= bmp.Width) continue;

                    var c = bmp.GetPixel(x, y);
                    if (!SameColor(c, forbidden)) return true;
                }
            }
            return false;
        }
    }
}
