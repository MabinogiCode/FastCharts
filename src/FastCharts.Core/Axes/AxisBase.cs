using FastCharts.Core.Abstractions;
using FastCharts.Core.Primitives;

namespace FastCharts.Core.Axes;

/// <summary>
/// Common ancestor for all axes (numeric, date, category...). Exposes a double-based visible/data range for layout.
/// Concrete axes provide typed IScale/ITicker via properties.
/// </summary>
public abstract class AxisBase
{
    /// <summary>Whole data range in data units (double). For non-double axes, use an underlying mapping (e.g., OADate).</summary>
    public FRange DataRange { get; set; }

    /// <summary>Current visible range in data units (double). For DateTime axis, use OADate conversion.</summary>
    public FRange VisibleRange { get; set; }

    /// <summary>Label format hint used by renderers when no custom formatter provided.</summary>
    public string? LabelFormat { get; set; }

    protected AxisBase()
    {
        DataRange = new FRange(0, 1);
        VisibleRange = new FRange(0, 1);
        LabelFormat = "G";
    }

    /// <summary>Update internal scale mapping using pixel min/max.</summary>
    public abstract void UpdateScale(double pixelMin, double pixelMax);
}
