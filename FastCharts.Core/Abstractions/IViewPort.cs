using FastCharts.Core.Primitives;

namespace FastCharts.Core.Abstractions;

public interface IViewport
{
    FRange X { get; }
    FRange Y { get; }
    void SetVisible(FRange x, FRange y);
    void Zoom(double scaleX, double scaleY, PointD pivotData);
    void Pan(double dxData, double dyData);
}
