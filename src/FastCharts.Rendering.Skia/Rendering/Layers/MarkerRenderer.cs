using FastCharts.Core.Series;
using SkiaSharp;

namespace FastCharts.Rendering.Skia.Rendering.Layers;

/// <summary>
/// Shared marker drawing for scatter points and line series markers.
/// </summary>
internal static class MarkerRenderer
{
    /// <summary>
    /// Draws a single marker centered on (px, py). For stroke-based shapes (Cross, Plus)
    /// the paint style is switched temporarily to stroke.
    /// </summary>
    public static void Draw(SKCanvas canvas, MarkerShape shape, float px, float py, float half, SKPaint paint)
    {
        switch (shape)
        {
            case MarkerShape.Square:
                canvas.DrawRect(new SKRect(px - half, py - half, px + half, py + half), paint);
                break;

            case MarkerShape.Triangle:
                {
                    using var path = new SKPath();
                    path.MoveTo(px, py - half);
                    path.LineTo(px - half, py + half);
                    path.LineTo(px + half, py + half);
                    path.Close();
                    canvas.DrawPath(path, paint);
                    break;
                }

            case MarkerShape.Diamond:
                {
                    using var path = new SKPath();
                    path.MoveTo(px, py - half);
                    path.LineTo(px + half, py);
                    path.LineTo(px, py + half);
                    path.LineTo(px - half, py);
                    path.Close();
                    canvas.DrawPath(path, paint);
                    break;
                }

            case MarkerShape.Cross:
                {
                    var previousStyle = paint.Style;
                    paint.Style = SKPaintStyle.Stroke;
                    canvas.DrawLine(px - half, py - half, px + half, py + half, paint);
                    canvas.DrawLine(px - half, py + half, px + half, py - half, paint);
                    paint.Style = previousStyle;
                    break;
                }

            case MarkerShape.Plus:
                {
                    var previousStyle = paint.Style;
                    paint.Style = SKPaintStyle.Stroke;
                    canvas.DrawLine(px - half, py, px + half, py, paint);
                    canvas.DrawLine(px, py - half, px, py + half, paint);
                    paint.Style = previousStyle;
                    break;
                }

            case MarkerShape.Circle:
            default:
                canvas.DrawCircle(px, py, half, paint);
                break;
        }
    }
}
