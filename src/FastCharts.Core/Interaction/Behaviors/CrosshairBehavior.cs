using System.Globalization;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class CrosshairBehavior : IBehavior
{
    public string TooltipFormatX { get; set; } = "X: {0}";
    public string TooltipFormatY { get; set; } = "Y: {0}";
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        if (model == null)
        {
            return false;
        }
        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;
        switch (ev.Type)
        {
            case PointerEventType.Move:
                {
                    st.ShowCrosshair = true;
                    st.PixelX = ev.PixelX;
                    st.PixelY = ev.PixelY;
                    if (st.DataX.HasValue && st.DataY.HasValue)
                    {
                        var ci = CultureInfo.InvariantCulture;
                        st.TooltipText = string.Format(ci, "{0}\n{1}", string.Format(ci, TooltipFormatX, st.DataX.Value), string.Format(ci, TooltipFormatY, st.DataY.Value));
                    }
                    return true;
                }
            case PointerEventType.Leave:
                {
                    st.ShowCrosshair = false;
                    st.TooltipText = null;
                    return true;
                }
            case PointerEventType.Down:
            case PointerEventType.Up:
            case PointerEventType.Wheel:
                {
                    return false;
                }
            default:
                {
                    return false;
                }
        }
    }
}
