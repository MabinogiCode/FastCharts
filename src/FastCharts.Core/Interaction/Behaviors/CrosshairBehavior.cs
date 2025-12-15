using System.Globalization;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class CrosshairBehavior : IBehavior
{
    public string TooltipFormatX { get; set; } = "X: {0}";
    public string TooltipFormatY { get; set; } = "Y: {0}";

    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        model.InteractionState ??= new InteractionState();
        var st = model.InteractionState;

        return ev.Type switch
        {
            PointerEventType.Move => HandleMove(st, ev),
            PointerEventType.Leave => HandleLeave(st),
            PointerEventType.Down or
            PointerEventType.Up or
            PointerEventType.Wheel or
            PointerEventType.KeyDown or
            PointerEventType.None => false,
            _ => false
        };
    }

    private bool HandleMove(InteractionState st, InteractionEvent ev)
    {
        st.ShowCrosshair = true;
        st.PixelX = ev.PixelX;
        st.PixelY = ev.PixelY;

        if (st.DataX.HasValue && st.DataY.HasValue)
        {
            var ci = CultureInfo.InvariantCulture;
            st.TooltipText = string.Format(ci, "{0}\n{1}",
                string.Format(ci, TooltipFormatX, st.DataX.Value),
                string.Format(ci, TooltipFormatY, st.DataY.Value));
        }

        return true;
    }

    private static bool HandleLeave(InteractionState st)
    {
        st.ShowCrosshair = false;
        st.TooltipText = null;
        return true;
    }
}
