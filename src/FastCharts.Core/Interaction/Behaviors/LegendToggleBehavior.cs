using System.Linq;
using FastCharts.Core.Interaction;

namespace FastCharts.Core.Interaction.Behaviors
{
    /// <summary>
    /// Toggle series visibility when clicking legend rows.
    /// </summary>
    public sealed class LegendToggleBehavior : IBehavior
    {
        public bool OnEvent(ChartModel model, InteractionEvent ev)
        {
            if (model == null) return false;
            if (ev.Type != PointerEventType.Down || ev.Button != PointerButton.Left) return false;
            if (model.InteractionState == null || model.InteractionState.LegendHits == null) return false;

            var hits = model.InteractionState.LegendHits;
            for (int i = 0; i < hits.Count; i++)
            {
                var h = hits[i];
                if (ev.PixelX >= h.X && ev.PixelX <= h.X + h.Width && ev.PixelY >= h.Y && ev.PixelY <= h.Y + h.Height)
                {
                    // Find matching series and toggle visibility
                    for (int s = 0; s < model.Series.Count; s++)
                    {
                        if (object.ReferenceEquals(model.Series[s], h.SeriesReference))
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
}
