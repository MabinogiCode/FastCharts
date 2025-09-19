using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Interactivity;

public sealed class Viewport : IViewport
{
    public Viewport(FRange x, FRange y)
    {
        X = x;
        Y = y;
    }

    public FRange X { get; private set; }
    public FRange Y { get; private set; }

    public void SetVisible(FRange x, FRange y)
    {
        X = x;
        Y = y;
    }

    public void Zoom(double scaleX, double scaleY, PointD pivotData)
    {
        var newX = new FRange(
            pivotData.X - (pivotData.X - X.Min) / scaleX,
            pivotData.X + (X.Max - pivotData.X) / scaleX
        );
        var newY = new FRange(
            pivotData.Y - (pivotData.Y - Y.Min) / scaleY,
            pivotData.Y + (Y.Max - pivotData.Y) / scaleY
        );

        SetVisible(newX, newY);
    }

    public void Pan(double dxData, double dyData)
    {
        SetVisible(
            new FRange(X.Min + dxData, X.Max + dxData),
            new FRange(Y.Min + dyData, Y.Max + dyData)
        );
    }
}
