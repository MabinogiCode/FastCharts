using System.Linq;

namespace FastCharts.Core.Interaction.Behaviors;

public sealed class LegendToggleBehavior : IBehavior
{
    public bool OnEvent(ChartModel model, InteractionEvent ev)
    {
        if (ev.Type != PointerEventType.Down || ev.Button != PointerButton.Left)
        {
            return false;
        }
        if (model.InteractionState?.LegendHits == null)
        {
            return false;
        }
        var hits = model.InteractionState.LegendHits;
        for (var i = 0; i < hits.Count; i++)
        {
            var h = hits[i];
            if (ev.PixelX >= h.X && ev.PixelX <= h.X + h.Width && ev.PixelY >= h.Y && ev.PixelY <= h.Y + h.Height)
            {
                for (var s = 0; s < model.Series.Count; s++)
                {
                    if (ReferenceEquals(model.Series[s], h.SeriesReference))
                    {
                        model.Series[s].IsVisible = !model.Series[s].IsVisible;
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
